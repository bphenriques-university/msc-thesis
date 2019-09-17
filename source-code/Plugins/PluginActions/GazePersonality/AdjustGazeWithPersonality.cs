using RapportActionProposer;
using SkeneRapportAgentConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GazePersonality
{
    [Export(typeof(IActionProposer))]
    public class AdjustGazeWithPersonality : RapportAgent {
        Timer timerUntilStopGazePerson;
        Timer timerUntilStopGazeGame;

        bool extrovert = false;

        bool _betweenTasksStage = true;
        bool betweenTasksStage { get { return _betweenTasksStage; }
            set {
                _betweenTasksStage = true;
                timerUntilStopGazePerson.Interval = GazeFaceDuration;
                timerUntilStopGazeGame.Interval = GazeGameDuration;
            }
        }

        //[Extrovert/Introvert][BetweenTasks/InTask][GazePersonDuration/GazeGameDuration]
        private double[,,] durations = new double[2, 2, 2] { 
            { { 4000, 1000 }, { 3000, 2000 } }, 
            { { 3000, 2000}, {1500, 3500 } }
        };

        double GazeFaceDuration {
            get { return durations[extrovert ? 0 : 1, betweenTasksStage ? 0 : 1, 0]; }
        }

        double GazeGameDuration {
            get { return durations[extrovert ? 0 : 1, betweenTasksStage ? 0 : 1, 1]; }
        }

        public AdjustGazeWithPersonality() : this("Adjusts Gaze Behaviour") { }
        public AdjustGazeWithPersonality(string description) : base(description){ }

        public override void Init() {
            base.Init();

            timerUntilStopGazePerson = new Timer(GazeFaceDuration);
            timerUntilStopGazePerson.AutoReset = true;
            timerUntilStopGazePerson.Elapsed += ElapsedLookingFace_event;
            
            timerUntilStopGazeGame = new Timer(GazeGameDuration);
            timerUntilStopGazeGame.AutoReset = true;
            timerUntilStopGazeGame.Elapsed += ElapsedLookingGame_event;
            
            timerUntilStopGazePerson.Start();
        }

        public override void Dispose() {
            base.Dispose();
            timerUntilStopGazePerson.Dispose();
            timerUntilStopGazeGame.Dispose();
        }

        void ElapsedLookingFace_event(object sender, ElapsedEventArgs e) {
            Log("Stopped looking at face. Looking at game");
            Gaze(1, "middleLeft");

            timerUntilStopGazePerson.Stop();
            timerUntilStopGazeGame.Start();
        }

        void ElapsedLookingGame_event(object sender, ElapsedEventArgs e) {
            Log("Stopped looking at Game. Looking at face.");
            Gaze(1, "person");

            timerUntilStopGazeGame.Stop();
            timerUntilStopGazePerson.Start();
        }
    }
}
