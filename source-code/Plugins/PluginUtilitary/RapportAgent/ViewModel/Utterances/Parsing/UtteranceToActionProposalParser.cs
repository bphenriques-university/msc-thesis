using RapportActionProposer.ActionProposalDefinition;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Globalization;

namespace RapportAgentPlugin {

    public class UtteranceActionProposalGenerator {
        private static readonly Regex METHOD_REGEX = new Regex(@"<\w*\([\s\w\.,-]*\)>");

        private ActionProposalFactory ActionProposalGenerator { get; }
        private ActionProposalsLifecycleManager ActionsProposalsLifeManager { get; }

        private Dictionary<string, MethodInfo> AvailableActions { get; } = new Dictionary<string, MethodInfo>();

        public UtteranceActionProposalGenerator(ActionProposalFactory manager, ActionProposalsLifecycleManager lifeCycleManager) {
            this.ActionProposalGenerator = manager;
            this.ActionsProposalsLifeManager = lifeCycleManager;

            foreach(var method in typeof(IAgentActions).GetMethods()) {
                AvailableActions.Add(method.Name.ToLower(), method);
            }
        }

        public Utterance GenerateActionProposal(ushort defaultPriority, string utterance, int defaultTimeOutMs) {
            utterance = utterance.Trim();

            List<string> speechs = new List<string>();
            foreach (var speechPart in METHOD_REGEX.Split(utterance)) {
                string s = speechPart.Trim();
                if (!string.IsNullOrWhiteSpace(s)) {
                    speechs.Add(speechPart.Trim());
                }
            }

            IActionProposal initialProposal = null;
            List<IActionProposal> proposals = new List<IActionProposal>();
            bool startsWithUtterance = utterance.StartsWith("<");
            int count = 0;
            foreach (var methodPart in METHOD_REGEX.Matches(utterance)) {
                string method = methodPart.ToString().Trim();
                string[] split = method.Split(')', '(');

                string methodName = split[0].Trim('<', '>', ' ', '\t').ToLower();
                if (!AvailableActions.ContainsKey(methodName)) {
                    throw new Exception("Provided method " + method + " does not exist");                    
                }

                MethodInfo m = AvailableActions[methodName];

                string[] args = split[1].Split(',');
                for (int i = 0; i < args.Length; i++) {
                    args[i] = args[i].Trim();
                }

                IActionProposal proposal = ParseMethod(m, args, defaultPriority, defaultTimeOutMs);
                if (count == 0 && startsWithUtterance) {
                    initialProposal = proposal;
                }
                else {
                    proposals.Add(proposal);
                }

                count++;
            }

            /*
            plugin.LogInfo("-----------------");
            foreach (string s in speechs) {
                plugin.LogFatal("\t" + s);
            }
            plugin.LogFatal("Number of bookmarks = number of actions proposals = " + proposals.Count);
            plugin.LogInfo("-----------------");
            */

            return new Utterance(ActionsProposalsLifeManager, speechs, proposals, initialProposal);
        }

        private IActionProposal ParseMethod(MethodInfo method, string[] args, ushort defaultPriority, int defaultTimeOutMs) {
            List<object> arguments = new List<object>();

            ParameterInfo[] parameters = method.GetParameters();

            int requiredParameters = parameters.Length - 3; //minus priority, initialDelay and timeoutOut as they are optionals
            int providedParameters = args.Length;
            int extraArguments = providedParameters - requiredParameters;

            if (extraArguments >= 0) {
                for (int i = 0; i < args.Length; i++) {
                    arguments.Add(Convert.ChangeType(args[i], parameters[i].ParameterType, CultureInfo.InvariantCulture));
                }
                if (extraArguments == 0) { //only the arguments itself, add priority, initial delay and timeout
                    arguments.Add(defaultPriority);
                    arguments.Add(0);
                    arguments.Add(defaultTimeOutMs);
                }
                else if (extraArguments == 1) { //only the arguments+priority, add initial delay and timeout
                    arguments.Add(0);
                    arguments.Add(defaultTimeOutMs);
                }
                else if (extraArguments == 2) { //add timeout
                    arguments.Add(defaultTimeOutMs);
                }
                else if (extraArguments != 3) {
                    throw new Exception("Too many arguments on: " + method);
                }
            }
            else {
                throw new Exception("Too few arguments on: " + method + ". Requires " + requiredParameters + " but provided " + providedParameters);
            }

            return method.Invoke(ActionProposalGenerator, arguments.ToArray()) as IActionProposal;
        }
    }
}
