using RapportActionProposer.ActionProposalDefinition;
using RapportAgentPlugin.ViewModel;
using RapportAgentPlugin.ViewModel.Utterances;
using System;
using System.Collections.Generic;

namespace RapportAgentPlugin {
    public class ActionProposalFactory : IAgentActions, IDisposable {
        public AgentActionsManager Plugin { get; }
        public UtteranceActionProposalGenerator UtterancesParser { get; }

        private UtterancesManager UtterancesManager { get; }
        private SoundsManagerModel SoundManager { get; }
        private ActionProposalsLifecycleManager ProposalsLifecycleManager { get; }
        private AgentActionsThalamusPublisher AgentActionsThalamusPublisher { get; }

        public ActionProposalFactory(AgentActionsManager plugin, UtterancesManager utterancesManager, SoundsManagerModel soundsManager) {
            this.Plugin = plugin;
            this.AgentActionsThalamusPublisher = new AgentActionsThalamusPublisher(Plugin.Name + "Effector", Plugin.Settings.Character); ;
            this.ProposalsLifecycleManager = new ActionProposalsLifecycleManager(Plugin, Plugin.Name + "ActionsManager", Plugin.Settings.Character);
            this.UtterancesParser = new UtteranceActionProposalGenerator(this, ProposalsLifecycleManager);
            this.UtterancesManager = utterancesManager;
            this.SoundManager = soundsManager;            
        }

        private string GenerateId() {
            return Guid.NewGuid().ToString();
        }

        public IActionProposal PerformUtteranceSkene(string category, string subcategory, ushort priority, string[] tagNames, string[] tagValues, int initialDelay = 0, int timeout = 0) {
            string utteranceId = GenerateId();
            var description = "PerformUtterance(" + utteranceId + ", " + category + ", " + subcategory + ")";

            var group = AgentActionGroup.Utterances;
            Action action = () => AgentActionsThalamusPublisher.PerformUtteranceFromLibrary(utteranceId, category, subcategory, tagNames, tagValues);

            var proposal = new ActionProposal(priority, group, action, description);
            proposal.InitialDelay = initialDelay;

            Action interrupt = () => AgentActionsThalamusPublisher.CancelUtterance(GenerateId());
            proposal.AddInterruptionAction(interrupt, utteranceId, timeout);

            return proposal;
        }

        public IActionProposal Gaze(string targetName, ushort priority, int initialDelay = 0, int timeout = 0) {
            var description = "Gaze(" + targetName + ")";

            var group = AgentActionGroup.Gaze;
            Action action = () => AgentActionsThalamusPublisher.GazeAtTarget(targetName);

            var proposal = new ActionProposal(priority, group, action, description);
            proposal.InitialDelay = initialDelay;

            return proposal;
        }

        public IActionProposal Animate(string animation, ushort priority, int initialDelay = 0, int timeout = 0) {
            var description = "Animate(" + animation + ")";

            ActionGroup group = AgentActionGroup.Animation;
            string animationId = GenerateId();
            Action action = () => AgentActionsThalamusPublisher.PlayAnimation(animationId, animation);

            var proposal = new ActionProposal(priority, group, action, description);
            proposal.InitialDelay = initialDelay;
            Action interrupt = () => AgentActionsThalamusPublisher.StopAnimation(animationId);
            proposal.AddInterruptionAction(interrupt, animationId, timeout);
            
            return proposal;
        }

        public IActionProposal Speak(string text, ushort priority, int initialDelay = 0, int timeout = 0) {
            var description = "Speak(" + text + ")";

            ActionGroup group = AgentActionGroup.Speech;
            string speechId = GenerateId();
            Action action = () => AgentActionsThalamusPublisher.Speak(speechId, text);

            var proposal = new ActionProposal(priority, group, action, description);
            proposal.InitialDelay = initialDelay;

            Action interrupt = () => AgentActionsThalamusPublisher.SpeakStop();
            proposal.AddInterruptionAction(interrupt, speechId, timeout);
            
            return proposal;
        }

        public IActionProposal Sound(string id, ushort priority, int initialDelay = 0, int timeout = 0) {
            string filePath = SoundManager.RequestPath(id);

            var description = "Sound(" + id + ")";

            ActionGroup group = AgentActionGroup.Speech;
            string speechId = Guid.NewGuid().ToString();

            Action action = () => SoundManager.PlaySound(filePath, speechId);
            Action interrupt = () => SoundManager.StopSound(speechId);

            var proposal = new ActionProposal(priority, group, action, description);
            proposal.InitialDelay = initialDelay;

            proposal.AddInterruptionAction(interrupt, speechId, timeout);
            
            return proposal;
        }
       
        public IActionProposal PerformUtterance(string category, string subcategory, HashSet<Tuple<string, string>> variables) {
            var utteranceSpec = UtterancesManager.FileManager.RetrieveUtterance(Plugin, category, subcategory);
            
            try {                
                var utterance = utteranceSpec.Load(UtterancesManager.SubstitutionManager, UtterancesParser, utteranceSpec.Priority, variables);

                string text = utteranceSpec.ReplacedTextWithDefaultTags;
                if (!string.IsNullOrWhiteSpace(text)) {
                    return PerformUtterance(utterance, utteranceSpec.ReplacedTextWithDefaultTags, utteranceSpec.Priority, utteranceSpec.InitialDelay, utteranceSpec.TimeOutMs);                                    
                }
            }
            catch (Exception e) {
                Plugin.LogError("Error retrieving utterance for category " + category + " due to: " + e.Message);
            }
            return null;
        }
         
        internal IActionProposal PerformUtterance(Utterance utterance, string text, ushort priority, int initialDelay = 0, int timeout = 0) {
            if(utterance == null) {
                Plugin.LogError("Provided null utterance");
                return null;
            }

            ActionGroup group = AgentActionGroup.Speech;
            string id = GenerateId();
            var description = string.Format("PerformUtterance({0}, {1})", text, id);

            ProposalsLifecycleManager.PrepareUtterance(utterance);

            Action action = () => utterance.Execute(id, AgentActionsThalamusPublisher);            
            var proposal = new ActionProposal(priority, group, action, description);
            proposal.InitialDelay = initialDelay;

            Action interrupt = () => utterance.Interrupt(AgentActionsThalamusPublisher);            
            proposal.AddInterruptionAction(interrupt, id, timeout);
            
            return proposal;
        }

        public IActionProposal HeadShake(int repetitions, double intensity, double frequency, ushort priority, int initialDelay = 0, int timeout = 0) {
            var description = string.Format("HeadShake({0}, {1}, {2})", repetitions, intensity, frequency);

            ActionGroup group = AgentActionGroup.Head;
            string headId = GenerateId();
            Action action = () => AgentActionsThalamusPublisher.HeadShake(headId, repetitions, intensity, frequency);

            var proposal = new ActionProposal(priority, group, action, description);
            proposal.InitialDelay = initialDelay;

            Action interrupt = () => AgentActionsThalamusPublisher.StopHeadShake();
            proposal.AddInterruptionAction(interrupt, headId, timeout);

            return proposal;
        }

        public IActionProposal HeadNod(int repetitions, double intensity, double frequency, ushort priority, int initialDelay = 0, int timeout = 0) {
            var description = string.Format("HeadNod({0}, {1}, {2})", repetitions, intensity, frequency);

            ActionGroup group = AgentActionGroup.Head;
            string headId = GenerateId();
            Action action = () => AgentActionsThalamusPublisher.HeadNod(headId, repetitions, intensity, frequency);

            var proposal = new ActionProposal(priority, group, action, description);
            proposal.InitialDelay = initialDelay;

            Action interrupt = () => AgentActionsThalamusPublisher.StopHeadNod();
            proposal.AddInterruptionAction(interrupt, headId, timeout);

            return proposal;
        }

        public void Dispose() {
            Plugin.LogInfo("Disposing Actions ThalamusClient Publisher");
            AgentActionsThalamusPublisher.Dispose();

            Plugin.LogInfo("Disposing Proposals Lifecycle ThalamusClient");
            ProposalsLifecycleManager.Dispose();
        }
    }
}
