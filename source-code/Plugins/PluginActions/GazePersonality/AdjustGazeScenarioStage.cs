using RapportActionProposer.ProposeStrategies;
using RapportActionProposer.RCPluginDefinition;
using RapportAgentPlugin;
using System.ComponentModel.Composition;
using System.Timers;

namespace GazePersonality {

    public class GazeSettings {
        public class GazeBehaviour {
            public ushort LookFacePriority { get; set; } = 1;
            public ushort LookGamePriority { get; set; } = 1;

            public string GameTarget { get; set; } = "bottomFront";
            public string PersonTarget { get; set; } = "middleFront";

            public double LookGameBetweenTasksDuration { get; set; } = 2500;
            public double LookFaceBetweenTasksDuration { get; set; } = 2500;

            public double LookGameInTasksDuration { get; set; } = 2500;
            public double LookFaceInTasksDuration { get; set; } = 2500;
        }

        public bool Introvert { get; set; } = false;
        public bool IsBetweenTasks { get; set; } = true;
        public bool Debug { get; set; } = false;

        public GazeBehaviour ExtrovertBehaviour { get; set; } = new GazeBehaviour() {
            LookFacePriority = 1,
            LookGamePriority = 1,
            LookFaceBetweenTasksDuration = 3910,
            LookGameBetweenTasksDuration = 1010,

            LookFaceInTasksDuration = 2660,
            LookGameInTasksDuration = 4040,
        };

        public GazeBehaviour IntrovertBehaviour { get; set; } = new GazeBehaviour() {
            LookFacePriority = 1,
            LookFaceBetweenTasksDuration = 1590,
            LookGameBetweenTasksDuration = 6210,

            LookFaceInTasksDuration = 570,
            LookGameInTasksDuration = 11650,
        };
    }

    [Export(typeof(IRCPlugin))]
    [RCPluginMetadata(Description = "Adjusts Gaze Behaviour")]
    public class AdjustGazeScenarioStage : EffectorPlugin<GazeSettings> {
        #region Fields
        private AgentActionsManager agent;

        private Timer timerUntilStopGazePerson;
        private Timer timerUntilStopGazeGame;

        private GazeSettings.GazeBehaviour currentGazeBehaviour;

        private bool _betweenTasksStage = true;
        public bool BetweenTasksStage { get { return _betweenTasksStage; }
            set {                
                _betweenTasksStage = value;
                timerUntilStopGazePerson.Interval = _betweenTasksStage ? currentGazeBehaviour.LookFaceBetweenTasksDuration : currentGazeBehaviour.LookFaceInTasksDuration;
                timerUntilStopGazeGame.Interval = _betweenTasksStage ? currentGazeBehaviour.LookGameBetweenTasksDuration : currentGazeBehaviour.LookGameInTasksDuration;                
            }
        }

        #endregion

        public override void Init() {
            base.Init();

            timerUntilStopGazePerson = new Timer();
            timerUntilStopGazePerson.AutoReset = true;
            timerUntilStopGazePerson.Elapsed += ElapsedLookingFace_event;

            timerUntilStopGazeGame = new Timer();
            timerUntilStopGazeGame.AutoReset = true;
            timerUntilStopGazeGame.Elapsed += ElapsedLookingGame_event;
        }

        public override void InitDependencies() {
            base.InitDependencies();

            agent = RapportController.GetPlugin<AgentActionsManager>();
            currentGazeBehaviour = Settings.Introvert ? Settings.IntrovertBehaviour : Settings.ExtrovertBehaviour;           
            BetweenTasksStage = Settings.IsBetweenTasks;
        }

        public override void Start() {
            base.Start();
            timerUntilStopGazePerson.Start();
        }

        public override void Pause() {
            base.Pause();

            timerUntilStopGazePerson.Stop();
            timerUntilStopGazeGame.Stop();
        }

        public override void Dispose() {
            base.Dispose();
            timerUntilStopGazePerson.Dispose();
            timerUntilStopGazeGame.Dispose();
        }        

        void ElapsedLookingFace_event(object sender, ElapsedEventArgs e) {
            if(Settings.Debug)
                LogDebug("Stopped looking at face. Looking at game");

            timerUntilStopGazePerson.Stop();
            timerUntilStopGazeGame.Start();

            ProposeAction(agent.Actions.Gaze(currentGazeBehaviour.PersonTarget, currentGazeBehaviour.LookFacePriority, 0, 10000));
        }

        void ElapsedLookingGame_event(object sender, ElapsedEventArgs e) {
            if(Settings.Debug)
                LogDebug("Stopped looking at Game. Looking at face.");

            timerUntilStopGazeGame.Stop();
            timerUntilStopGazePerson.Start();

            ProposeAction(agent.Actions.Gaze(currentGazeBehaviour.GameTarget, currentGazeBehaviour.LookGamePriority, 0, 10000));
        }
    }
}
