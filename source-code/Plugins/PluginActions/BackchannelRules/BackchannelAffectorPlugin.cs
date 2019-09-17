using GRETAConnection;
using RapportActionProposer.RCPluginDefinition;
using System;
using System.ComponentModel.Composition;
using RapportAgentPlugin;
using RapportActionProposer.ProposeStrategies;

namespace BackchannelRules
{
    public class BackchannelSettings {
        public ushort BackchannelTriggerPriority { get; set; } = 1;

        public double TriggerProbability { get; set; } = 1.0;

        public int MinimumIntensity { get; set; } = 5;
        public int MaximumIntensity { get; set; } = 10;

        public int MinimumRepetitions { get; set; } = 2;
        public int MaximumRepetitions { get; set; } = 2;

        public int MinimumFrequency { get; set; } = 125;
        public int MaximumFrequency { get; set; } = 160;

        public int DelayMsVocalization { get; set; } = 175;
        public int MinimumDelayMsBetweenActions { get; set; } = 5000;

        public bool DebugMessages { get; set; } = false;
    }

    [Export(typeof(IRCPlugin))]
    [RCPluginMetadata(Description = "Mimics Head Nods and Shake")]
    [EffectorMetadata(ProposalsManagementStrategy = ProposeStrategyType.OneActionGlobal)]
    public class BackchannelAffectorPlugin : EffectorPlugin<BackchannelSettings> {

        GretaPerceptionReceiver gretaPerceptionsReceiver;
        AgentActionsManager agent;
        Random random = new Random();

        public override void InitDependencies() {
            base.InitDependencies();

            agent = RapportController.GetPlugin<AgentActionsManager>();
            gretaPerceptionsReceiver = RapportController.GetPlugin<GretaPerceptionReceiver>();
        }

        public override void Init() {
            base.Init();
            (Operations.ProposeActionStrategy as OneActionGlobalProposeActionStrategy).MinimumTimeBetweenActionsMs = Settings.MinimumDelayMsBetweenActions;
        }

        public override void Start() {
            base.Start();
            if (gretaPerceptionsReceiver.gretaChannelReceiver == null)
                throw new Exception("GretaPerceptionsReader must be initialized first!");
            
            gretaPerceptionsReceiver.gretaChannelReceiver.BackchannelEvent += HandleBackchannelEvent;
        }

        public override void Pause() {
            base.Pause();
            if (gretaPerceptionsReceiver.gretaChannelReceiver == null)
                throw new Exception("GretaPerceptionsReader must be initialized first!");
            
            gretaPerceptionsReceiver.gretaChannelReceiver.BackchannelEvent -= HandleBackchannelEvent;
        }

        private double GetRandomNumber(double minimum, double maximum) {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private int GetRandomInt(int minimum, int maximum) {
            return random.Next(minimum, maximum);
        }

        private void HandleBackchannelEvent(object sender, BackchannelEventArgs e) {
            if (GetRandomNumber(0, 1) <= Settings.TriggerProbability) {
                var intensityValue = GetRandomInt(Settings.MinimumIntensity, Settings.MaximumIntensity);
                var frequency = GetRandomInt(Settings.MinimumFrequency, Settings.MaximumFrequency);
                var repetitions = GetRandomInt(Settings.MinimumRepetitions, Settings.MaximumRepetitions);

                if (Settings.DebugMessages)
                    LogDebug("PROPOSING BACKCHANNEL.");

                //proposing head nod
                var headProposal = agent.Actions.HeadNod(repetitions, intensityValue * 0.01, frequency * 0.01, Settings.BackchannelTriggerPriority, 0, 4000);
                RapportController.ProposeAction(headProposal);

                //proposing vocalization
                var vocalizationproposal = agent.Actions.Speak("Hmm Hmm", Settings.BackchannelTriggerPriority, Settings.DelayMsVocalization, 5000);
                RapportController.ProposeAction(vocalizationproposal);
            }
        }
    }
}