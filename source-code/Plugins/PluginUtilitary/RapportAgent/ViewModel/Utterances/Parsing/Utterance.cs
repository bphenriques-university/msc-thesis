using RapportActionProposer.ActionProposalDefinition;
using RapportActionProposer.RCPluginDefinition;
using System;
using System.Collections.Generic;

namespace RapportAgentPlugin {
    public class Utterance {
        //argument
        public List<string> Speechs { get; }

        public ActionProposalsLifecycleManager ÁctionProposalsManager { get; }
        public List<IActionProposal> BookMarkProposals { get; }
        public IActionProposal InitialProposal { get; }

        //generated
        public string Id = Guid.NewGuid().ToString();
        public Dictionary<string, IActionProposal> BookMarkToActionProposal { get; }
        public List<string> BookmarksIds { get; }

        //control
        private IActionProposal activeProposal;
        private bool interrupted = false;

        public Utterance(ActionProposalsLifecycleManager manager, List<string> speechs, List<IActionProposal> proposals, IActionProposal initialProposal) {
            this.Speechs = speechs;
            this.ÁctionProposalsManager = manager;            
            this.BookMarkProposals = proposals;
            this.InitialProposal = initialProposal;

            BookMarkToActionProposal = new Dictionary<string, IActionProposal>();
            BookmarksIds = new List<string>();

            BookMarkToActionProposal.Clear();
            foreach (var bookmark in BookMarkProposals) {
                string id = Guid.NewGuid().ToString();
                BookmarksIds.Add(id);
                BookMarkToActionProposal[id] = bookmark;
            }
        }

        public void Interrupt(AgentActionsThalamusPublisher thalamusClient) {
            thalamusClient.SpeakStop();

            interrupted = true;
            if (activeProposal != null)
                activeProposal.Interrupt();

            ÁctionProposalsManager.HandleUtteranceFinishedOrInterrupted(Id);
        }

        public void Execute(string id, AgentActionsThalamusPublisher thalamusClient) {
            interrupted = false;
            activeProposal = InitialProposal;

            if(InitialProposal != null) {
                InitialProposal.Execute();
            }
            thalamusClient.SpeakBookmarks(id, Speechs.ToArray(), BookmarksIds.ToArray());
        }

        public IActionProposal GetNextActionProposal(IEffectorPlugin proposer, string id) {
            if (!interrupted && BookMarkToActionProposal.ContainsKey(id)) {
                IActionProposal proposal = BookMarkToActionProposal[id];
                activeProposal = proposal;
                return proposal;
            }

            return null;
        }
    }
}
