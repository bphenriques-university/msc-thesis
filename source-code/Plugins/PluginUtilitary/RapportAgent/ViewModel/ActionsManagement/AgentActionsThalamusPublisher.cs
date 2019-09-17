using EmoteCommonMessages;
using Thalamus;
using Thalamus.BML;

namespace RapportAgentPlugin {
    public interface IRCActionsThalamusMessages : IAction {
        void HeadShake(string id, int repetitions, double intensity, double frequency);
        void HeadNod(string id, int repetitions, double intensity, double frequency);
        void StopHeadShake();
        void StopHeadNod();
    }

    public interface IAgentInteractions : IFMLSpeech, ISpeakActions, IGazeStateActions, IAnimationActions, IRCActionsThalamusMessages { }

    public class AgentActionsThalamusPublisher : ThalamusClient, IAgentInteractions {
        private class AgentActionsThalamusWsPublisher : IAgentInteractions {
            dynamic publisher;

            public AgentActionsThalamusWsPublisher(dynamic publisher) {
                this.publisher = publisher;
            }

            public void CancelUtterance(string id) {
                publisher.CancelUtterance(id);
            }

            public void GazeAtScreen(double x, double y) {
                publisher.GazeAtScreen(x, y);
            }

            public void GazeAtTarget(string targetName) {
                publisher.GazeAtTarget(targetName);
            }

            public void GlanceAtScreen(double x, double y) {
                publisher.GlanceAtScreen(x, y);
            }

            public void GlanceAtTarget(string targetName) {
                publisher.GlanceAtTarget(targetName);
            }

            public void PlayAnimation(string id, string animation) {
                publisher.PlayAnimation(id, animation);
            }

            public void Speak(string id, string text) {
                publisher.Speak(id, text);
            }

            public void SpeakStop() {
                publisher.SpeakStop();
            }

            public void StopAnimation(string id) {
                publisher.StopAnimation(id);
            }

            public void PerformUtterance(string id, string utterance, string category) {
                publisher.PerformUtteranceFromLibrary(id, utterance, category);
            }

            public void PerformUtteranceFromLibrary(string id, string category, string subcategory, string[] tagNames, string[] tagValues) {
                publisher.PerformUtteranceFromLibrary(id, category, subcategory, tagNames, tagValues);
            }

            public void SpeakBookmarks(string id, string[] text, string[] bookmarks) {
                publisher.SpeakBookmarks(id, text, bookmarks);
            }

            public void PlayAnimationQueued(string id, string animation) {
                publisher.PlayAnimationQueued(id, animation);
            }

            public void HeadShake(string id, int repetitions, double intensity, double frequency) {
                publisher.HeadShake(id, repetitions, intensity, frequency);
            }

            public void HeadNod(string id, int repetitions, double intensity, double frequency) {
                publisher.HeadNod(id, repetitions, intensity, frequency);
            }

            public void StopHeadShake() {
                publisher.StopHeadShake();
            }

            public void StopHeadNod() {
                publisher.StopHeadNod();
            }
        }
        private AgentActionsThalamusWsPublisher wsPublisher;

        public AgentActionsThalamusPublisher(string clientName, string character) : base(clientName, character) {
            SetPublisher<IAgentInteractions>();
            wsPublisher = new AgentActionsThalamusWsPublisher(Publisher);
        }

        public void CancelUtterance(string id) {
            wsPublisher.CancelUtterance(id);
        }

        public void GazeAtScreen(double x, double y) {
            wsPublisher.GazeAtScreen(x, y);
        }

        public void GazeAtTarget(string targetName) {
            wsPublisher.GazeAtTarget(targetName);
        }

        public void GlanceAtScreen(double x, double y) {
            wsPublisher.GlanceAtScreen(x, y);
        }

        public void GlanceAtTarget(string targetName) {
            wsPublisher.GlanceAtTarget(targetName);
        }

        public void HeadNod(string id, int repetitions, double intensity, double frequency) {
            wsPublisher.HeadNod(id, repetitions, intensity, frequency);
        }

        public void HeadShake(string id, int repetitions, double intensity, double frequency) {
            wsPublisher.HeadShake(id, repetitions, intensity, frequency);
        }

        public void PerformUtterance(string id, string utterance, string category) {
            wsPublisher.PerformUtterance(id, utterance, category);
        }

        public void PerformUtteranceFromLibrary(string id, string category, string subcategory, string[] tagNames, string[] tagValues) {
            wsPublisher.PerformUtteranceFromLibrary(id, category, subcategory, tagNames, tagValues);
        }

        public void PlayAnimation(string id, string animation) {
            wsPublisher.PlayAnimation(id, animation);
        }

        public void PlayAnimationQueued(string id, string animation) {
            wsPublisher.PlayAnimationQueued(id, animation);
        }

        public void Speak(string id, string text) {
            wsPublisher.Speak(id, text);
        }

        public void SpeakBookmarks(string id, string[] text, string[] bookmarks) {
            wsPublisher.SpeakBookmarks(id, text, bookmarks);
        }

        public void SpeakStop() {
            wsPublisher.SpeakStop();
        }

        public void StopAnimation(string id) {
            wsPublisher.StopAnimation(id);
        }

        public void StopHeadNod() {
            wsPublisher.StopHeadNod();
        }

        public void StopHeadShake() {
            wsPublisher.StopHeadShake();
        }
    }
}