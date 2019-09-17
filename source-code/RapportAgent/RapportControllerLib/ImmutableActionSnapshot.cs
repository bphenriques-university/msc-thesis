using RapportActionProposer.ActionProposalDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RapportControllerLib {
    public class ImmutableActionsSnapshot : IImmutableActionsSnapshot {
        public IActionsSnapshot Original;

        public int Id => Original.Id;
        public DateTime TimeStamp => Original.TimeStamp;
        public List<ImmutableActionProposal> Proposals { get; }

        public int ExecutingProposals { get; private set; }
        public int ExecutedProposals { get; private set; }
        public int PendingProposals { get; private set; }
        public int InterruptedProposals { get; private set; }

        public ImmutableActionsSnapshot(IActionsSnapshot snapshot) {
            this.Original = snapshot;

            Proposals = new List<ImmutableActionProposal>();
            foreach (var pair in snapshot.Proposals) {
                //not supposed to happen but sometimes, for some reason, the proposal reference becomes null
                if (pair.Value != null) {
                    Proposals.Add(new ImmutableActionProposal(pair.Value));
                    AddCount(pair.Value.Status);
                }
            }

            foreach(var removedProposal in snapshot.RemovedProposals) {
                //not supposed to happen but sometimes, for some reason, the proposal reference becomes null
                if (removedProposal != null) {
                    Proposals.Add(new ImmutableActionProposal(removedProposal));
                    AddCount(removedProposal.Status);
                }
            }
        }

        private void AddCount(ActionState state) {
            switch (state) {
                case ActionState.Pending:
                    PendingProposals++;
                    break;
                case ActionState.Executing:
                    ExecutingProposals++;
                    break;
                case ActionState.Executed:
                    ExecutedProposals++;
                    break;
                case ActionState.Interrupted:
                    InterruptedProposals++;
                    break;
            }
        }
        
        public void Execute() {
            var alreadyExecuted = new HashSet<ActionGroup>();
            foreach (var proposal in Proposals.Where((ImmutableActionProposal proposal) => !alreadyExecuted.Contains(proposal.Group))) {
                Task.Factory.StartNew(() => proposal.Execute()); Task.Factory.StartNew(() => proposal.Execute());
                alreadyExecuted.Add(proposal.Group);
            }
        }

        #region DisposablePattern
        private bool _disposed = false;

        ~ImmutableActionsSnapshot() {
            Dispose(false); //unsafe disposal
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed)
                return;

            if (disposing) {
                Proposals.Clear();
            }

            _disposed = true;
        }
        #endregion
    }
}
