using GRETAConnection;
using RapportActionProposer.RCPluginDefinition;
using RapportAgentPlugin;
using System;
using System.ComponentModel.Composition;
using RapportActionProposer.ActionProposalDefinition;
using RapportActionProposer.ProposeStrategies;

namespace HeadMimicry {

    public class MimicHeadSettings {
        public class MimicBehaviour {
            public ushort Priority { get; set; } = 1;
            public int MinimumIntensity { get; set; } = 5;
            public int MaximumIntensity { get; set; } = 10;

            public int MinimumRepetitions { get; set; } = 2;
            public int MaximumRepetitions { get; set; } = 2;

            public int MinimumFrequency { get; set; } = 100;
            public int MaximumFrequency { get; set; } = 150;

            public double TriggerProbability { get; set; } = 1.0;

            public int TimeOutMs { get; set; } = 3500;

            public bool IsNodBehaviour { get; set; } = false;

        }

        public bool DebugMessages { get; set; } = false;
        public int MinimumDelayMsBetweenActions { get; set; } = 2500;
        public MimicBehaviour Shake { get; set; } = new MimicBehaviour() {
            Priority = 1,

            MinimumIntensity = 30,
            MaximumIntensity = 100,

            MinimumFrequency = 150,
            MaximumFrequency = 200,

            TriggerProbability = 1.0,
            IsNodBehaviour = false
        };

        public MimicBehaviour Nod { get; set; } = new MimicBehaviour() {
            Priority = 1,

            MinimumIntensity = 5,
            MaximumIntensity = 10,

            MinimumFrequency = 125,
            MaximumFrequency = 160,

            TriggerProbability = 1.0,

            IsNodBehaviour = true
        };
    }

    [Export(typeof(IRCPlugin))]
    [RCPluginMetadata(Description = "Mimics Head Nods and Shake")]
    [EffectorMetadata(ProposalsManagementStrategy = ProposeStrategyType.OneActionGlobal)]
    public class MimicHead : EffectorPlugin<MimicHeadSettings> {

        GretaPerceptionReceiver gretaPerceptionsReceiver;
        AgentActionsManager agent;
        Random random = new Random();

        public override void Init() {
            base.Init();
            (Operations.ProposeActionStrategy as OneActionGlobalProposeActionStrategy).MinimumTimeBetweenActionsMs = Settings.MinimumDelayMsBetweenActions;
        }

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

            gretaPerceptionsReceiver.gretaChannelReceiver.HeadNodEvent += GretaChannelReceiver_HeadNodEvent;
            gretaPerceptionsReceiver.gretaChannelReceiver.HeadShakeEvent += GretaChannelReceiver_HeadShakeEvent;
        }


        public override void Pause() {
            base.Pause();

            if (gretaPerceptionsReceiver.gretaChannelReceiver == null) {
                throw new Exception("GretaPerceptionsReader must be initialized first!");
            }

            gretaPerceptionsReceiver.gretaChannelReceiver.HeadNodEvent -= GretaChannelReceiver_HeadNodEvent;
            gretaPerceptionsReceiver.gretaChannelReceiver.HeadShakeEvent -= GretaChannelReceiver_HeadShakeEvent;
        }

        private void GretaChannelReceiver_HeadShakeEvent(object sender, EventArgs e) {
            if (Settings.DebugMessages)
                LogInfo("---> ShakeHead");
            MimicBehaviourAux(Settings.Shake);
        }

        private void GretaChannelReceiver_HeadNodEvent(object sender, EventArgs e) {
            if (Settings.DebugMessages)
                LogInfo("---> NodHead");
            MimicBehaviourAux(Settings.Nod);
        }

        private double GetRandomNumber(double minimum, double maximum) {
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        private int GetRandomInt(int minimum, int maximum) {
            return random.Next(minimum, maximum);
        }

        public void MimicBehaviourAux(MimicHeadSettings.MimicBehaviour behaviour) {

            if (GetRandomNumber(0, 1) <= behaviour.TriggerProbability) {
                int repetitions = GetRandomInt(behaviour.MinimumRepetitions, behaviour.MaximumRepetitions);
                int frequency = GetRandomInt(behaviour.MinimumFrequency, behaviour.MaximumFrequency);
                int intensity = GetRandomInt(behaviour.MinimumIntensity, behaviour.MaximumIntensity);
                
                if (Settings.DebugMessages)
                    LogInfo(string.Format("PROPOSING MIMIC BEHAVIOUR repetitions={0} (Min: {1} Max: {2}), intensity={6} (Min: {7} Max: {8}, frequency={3}(Min: {4} Max: {5}))",

                repetitions, behaviour.MinimumRepetitions, behaviour.MaximumRepetitions,
                frequency, behaviour.MinimumFrequency, behaviour.MaximumFrequency,
                intensity, behaviour.MinimumIntensity, behaviour.MaximumIntensity));


                IActionProposal proposal;
                if (behaviour.IsNodBehaviour) {
                    proposal = agent.Actions.HeadNod(repetitions, intensity * 0.01, frequency * 0.01, behaviour.Priority, 0, behaviour.TimeOutMs);
                }else {
                    proposal = agent.Actions.HeadShake(repetitions, intensity * 0.01, frequency * 0.01, behaviour.Priority, 0, behaviour.TimeOutMs);
                }

                ProposeAction(proposal);
            }
        }
    }
}