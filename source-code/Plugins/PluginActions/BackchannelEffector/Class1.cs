using GRETAConnection;
using RapportActionProposer.RCPluginDefinition;
using RapportAgentPlugin;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using HelpersForNet;
using RapportActionProposer;

namespace BackchannelEffector
{
    public class BackchannelSettings {
        public ushort BackchannelTriggerPriority { get; set; } = 1;

        public List<string> CompatibleIntensies { get; set; } = new List<string>() {
            "rise", "fall", "rise_fall", "fall_rise"
        };

        public double TriggerProbability { get; set; } = 0.5;

        public double MinimumIntensity { get; set; } = 0.6;
        public double MaximumIntensity { get; set; } = 0.8;

        public int MinimuMRepetitions { get; set; } = 1;
        public int MaximumRepetitions { get; set; } = 2;

        public double MinimumFrequency { get; set; } = 0.6;
        public double MaximumFrequency { get; set; } = 0.8;

        public bool DebugMessages { get; set; } = true;
    }

    [Export(typeof(IRCPlugin))]
    public class BackchannelPlugin : EffectorPlugin<BackchannelSettings> {

        GretaPerceptionReceiver gretaPerceptionsReceiver;
        AgentActionsManager agent;
        Random random = new Random();

        bool AgentIsPerforming = false;

        public BackchannelPlugin() : base("Mimics Head Nods and Shake") { }

        public override void InitDependencies() {
            base.InitDependencies();

            agent = RapportController.GetPlugin<AgentActionsManager>();
            gretaPerceptionsReceiver = RapportController.GetPlugin<GretaPerceptionReceiver>();
        }

        public override void Start() {
            base.Start();

            if (gretaPerceptionsReceiver.gretaChannelReceiver == null) {
                throw new Exception("GretaPerceptionsReader must be initialized first!");
            }

            gretaPerceptionsReceiver.gretaChannelReceiver.BackchannelEvent += GretaChannelReceiver_BackchannelEvent1;

            //receive animation finished events
            RapportController.ExecutedActionGroup += HandleAnimationDidFinishedOrInterrupted;
            RapportController.InterruptedActionGroup += HandleAnimationDidFinishedOrInterrupted;
        }

        public override void Dispose() {
            base.Dispose();
            if (gretaPerceptionsReceiver.gretaChannelReceiver != null) {
                gretaPerceptionsReceiver.gretaChannelReceiver.BackchannelEvent -= GretaChannelReceiver_BackchannelEvent1;
            }

            RapportController.ExecutedActionGroup -= HandleAnimationDidFinishedOrInterrupted;
            RapportController.InterruptedActionGroup -= HandleAnimationDidFinishedOrInterrupted;
        }

        private void HandleAnimatingFacialExpression(object sender, ActionGroupsStatusChangedEventArgs e) {
            if (e.Proposal.ProposerId == Name) {
                AgentIsPerforming = true;
            }
        }

        private void HandleAnimationDidFinishedOrInterrupted(object sender, ActionGroupsStatusChangedEventArgs e) {
            AgentIsPerforming = false;
        }

        private double GetRandomNumber(double minimum, double maximum) {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private void GretaChannelReceiver_BackchannelEvent1(object sender, BackchannelEventArgs e) {
            LogDebug("GretaChannelReceiver_BackchannelEvent1");
            string intensity = e.Intensity;

            if (Settings.DebugMessages)
                LogDebug("GretaChannelReceiver_BackchannelEvent1");

            if (!AgentIsPerforming && Settings.CompatibleIntensies.Contains(intensity) && GetRandomNumber(0, 1) <= Settings.TriggerProbability) {
                var intensityValue = random.GetRandomNumber(Settings.MinimumIntensity, Settings.MaximumIntensity);
                var frequency = random.GetRandomNumber(Settings.MinimumFrequency, Settings.MaximumFrequency);
                var repetitions = random.GetRandomNumber(Settings.MinimuMRepetitions, Settings.MaximumRepetitions);

                if (Settings.DebugMessages)
                    LogDebug("PROPOSING BACKCHANNEL");

                var proposal = agent.Actions.HeadNod(repetitions, intensityValue, frequency, Settings.BackchannelTriggerPriority);
                proposal.Executing += Proposal_Executing;
                RapportController.ProposeAction(proposal);
            }
            else {
                LogError("UNRECOGNIZED BACKCHANNEL INTENSITY " + intensity); LogError("INTENSITY " + intensity + " is not compatible!");
            }
        }

        private void Proposal_Executing(object sender, RapportActionProposer.ActionProposalDefinition.ProposalChangedStatusEventArgs e) {
            AgentIsPerforming = true;
        }
    }
}
