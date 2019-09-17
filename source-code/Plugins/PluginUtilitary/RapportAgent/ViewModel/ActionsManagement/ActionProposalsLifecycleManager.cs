using EmoteCommonMessages;
using System.Collections.Generic;
using Thalamus;
using Thalamus.BML;

namespace RapportAgentPlugin {
    public interface SpeechBookmarksEventsSubscribe : ISpeakEvents, IAnimationEvents, IFMLSpeechEvents, ISpeakDetailEvents { }

    public class ActionProposalsLifecycleManager : ThalamusClient, SpeechBookmarksEventsSubscribe {
        private AgentActionsManager Plugin { get; }
        private Dictionary<string, Utterance> BookmarkToUtterance { get; } = new Dictionary<string, Utterance>();        

        public ActionProposalsLifecycleManager(AgentActionsManager plugin, string name, string character) : base(name, character) {
            this.Plugin = plugin;
        }

        #region bookmarks

        public void Bookmark(string id) {
            if (BookmarkToUtterance.ContainsKey(id)) {
                var utterance = BookmarkToUtterance[id];
                var proposal = utterance.GetNextActionProposal(Plugin, id);
                if (proposal != null) {
                    Plugin.ProposeAction(proposal);
                }

                BookmarkToUtterance.Remove(id);
            }
            else {
                Plugin.LogDebug("Can't find bookmark with id: " + id);
            }
        }

        public void Viseme(int viseme, int nextViseme, double visemePercent, double nextVisemePercent) { /* empty on purpose */ }

        #endregion

        #region SkeneUtterances
        public void CancelUtterance(string id) {
            Plugin.InterruptAction(id);
        }

        public void UtteranceFinished(string id) {
            Plugin.ActionFinished(id);
        }

        public void UtteranceStarted(string id) { /* empty on purpose */ }

        #endregion

        #region Speech

        public void SpeakStarted(string id) { /* empty on purpose */ }

        public void SpeakFinished(string id) {
            //can be either a speech (normal) or a utterance
            Plugin.LogDebug("Handling end of speech server with id: " + id);
            Plugin.ActionFinished(id);
            HandleUtteranceFinishedOrInterrupted(id);
        }
        #endregion

        #region Animations

        public void StopAnimation(string id) {
            Plugin.InterruptAction(id);
        }

        public void AnimationStarted(string id) { /* empty on purpose */ }

        public void AnimationFinished(string id) {
            Plugin.ActionFinished(id);
        }
        #endregion


        public void HandleUtteranceFinishedOrInterrupted(string id) {
            Plugin.LogDebug("INSIDE UTTERANCE: Handling UtteranceFinished Or Interrupted: " + id);
            foreach (var pair in BookmarkToUtterance) {
                //remove every utterance except this one
                if (pair.Value.Id != id) {
                    BookmarkToUtterance.Remove(pair.Key);
                }
            }
        }
        
        internal void PrepareUtterance(Utterance utterance) {
            //associate this action id to the utterance
            foreach (string bookmark in utterance.BookmarksIds) {
                if (!BookmarkToUtterance.ContainsKey(bookmark))
                    BookmarkToUtterance.Add(bookmark, utterance);
            }
        }
    }
}
