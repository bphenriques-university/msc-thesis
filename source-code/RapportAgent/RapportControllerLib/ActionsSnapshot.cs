using RapportActionProposer;
using RapportActionProposer.ActionProposalDefinition;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Log4NetWrapperLite;
using System.Linq;

namespace RapportControllerLib {
    public class ActionsSnapshot : IActionsSnapshot {
        private static int _id_generated = 0;
        public int Id { get; } = ++_id_generated % int.MaxValue;

        public DateTime TimeStamp { get; private set; }

        public Dictionary<ActionGroup, IActionProposal> Proposals { get; } = new Dictionary<ActionGroup, IActionProposal>();
        public Dictionary<string, IActionProposal> SecondaryIds { get; } = new Dictionary<string, IActionProposal>();
        public List<IActionProposal> RemovedProposals { get; } = new List<IActionProposal>();
 

        public static event EventHandler<ActionGroupsStatusChangedEventArgs> GlobalExecutingActionGroup;
        public static event EventHandler<ActionGroupsStatusChangedEventArgs> GlobalExecutedActionGroup;
        public static event EventHandler<ActionGroupsStatusChangedEventArgs> GlobalInterruptedActionGroup;

        public ActionsSnapshot() { }
        public ActionsSnapshot(IActionsSnapshot snapshot) {
            SecondaryIds = snapshot.SecondaryIds;
            Parallel.ForEach(snapshot.Proposals, keyValuePair => {
                var proposal = keyValuePair.Value;

                //required due to a crash, do not know the cause exactly
                if (proposal != null) {
                    if (proposal.IsPending)
                        Proposals[proposal.Group] = proposal;                    
                    else if (proposal.IsInterruptible && !proposal.HasBeenInterrupted) {
                        //the task has expired but wasn't interrupted
                        if (HasProposalExpired(proposal)) {
                            Logger.Info("Action " + proposal.Description + "(" + proposal.SecondaryId + ") has expired, interrupting!");
                            InterruptProposal(proposal);
                        }
                        else if (!proposal.HasExecuted)
                            Proposals[proposal.Group] = proposal;
                        
                    }
                }
            });
        }

        private bool HasProposalExpired(IActionProposal proposal) {
            return DateTime.Now > proposal.ExpirationTime;
        }


        public void InterruptSecondaryIdAction(string id) {
            if (SecondaryIds.ContainsKey(id))
                InterruptProposal(SecondaryIds[id]);            
        }

        public void AddProposal(IActionProposal proposal) {
            if (proposal == null)
                return;

            var group = proposal.Group;
            bool add = false;

            if (Proposals.ContainsKey(group)) {
                var bestProposal = Proposals[group];
                if (bestProposal.IsInterruptible && HasProposalExpired(bestProposal)) {
                    Logger.Info(bestProposal.Description + " has expired");
                    InterruptProposal(bestProposal);

                    add = true;
                }
                else if (proposal.Priority >= bestProposal.Priority) {
                    if (bestProposal.IsInterruptible) {
                        Logger.Debug(bestProposal.Description + " is being interrupted by higher priority: " + proposal.Description);
                        InterruptProposal(bestProposal);
                    }

                    add = true;
                }

                if(add)
                    RemovedProposals.Add(bestProposal);
            }
            else
                add = true;

            if (add) {
                Proposals[group] = proposal;
                if (proposal.IsInterruptible && !SecondaryIds.ContainsKey(proposal.SecondaryId))
                    SecondaryIds.Add(proposal.SecondaryId, proposal);             
            }
        }

        public void InterruptProposal(IActionProposal proposal) {
            Task.Factory.StartNew(proposal.Interrupt);
            if(SecondaryIds.ContainsKey(proposal.SecondaryId))
                SecondaryIds.Remove(proposal.SecondaryId);            
        }

        public void MarkAsOutdated() {
            TimeStamp = DateTime.Now;
        }     

        private void ExecuteProposal(IActionProposal proposal) {
            Task.Factory.StartNew(() => {
                Logger.Info("\t [Snapshot " + Id + "] DO: " + proposal.Id + " - " + proposal.Description);
                proposal.ErrorInExecution += Proposal_ErrorInExecution;
                proposal.Interrupted += Proposal_Interrupted;
                proposal.Executed += Proposal_Executed;
                proposal.Executing += Proposal_Executing;
                proposal.Execute();
            });
        }

        private void Proposal_Executing(object sender, ProposalChangedStatusEventArgs e) {
            var args = new ActionGroupsStatusChangedEventArgs(e.Proposal, e.Proposal.Status);
            foreach (EventHandler<ActionGroupsStatusChangedEventArgs> receiver in GlobalExecutingActionGroup.GetInvocationList())
                receiver.BeginInvoke(this, args, null, null);
        }

        private void Proposal_Executed(object sender, ProposalChangedStatusEventArgs e) {
            var args = new ActionGroupsStatusChangedEventArgs(e.Proposal, e.Proposal.Status);
            foreach (EventHandler<ActionGroupsStatusChangedEventArgs> receiver in GlobalExecutedActionGroup.GetInvocationList())
                receiver.BeginInvoke(this, args, null, null);
        }

        private void Proposal_Interrupted(object sender, ProposalChangedStatusEventArgs e) {
            var args = new ActionGroupsStatusChangedEventArgs(e.Proposal, e.Proposal.Status);
            foreach (EventHandler<ActionGroupsStatusChangedEventArgs> receiver in GlobalInterruptedActionGroup.GetInvocationList())
                receiver.BeginInvoke(this, args, null, null);
        }

        private void Proposal_ErrorInExecution(object sender, ProposalActionsContainsErrorsEventArgs e) {
            Logger.Error("Error during execution of " + e.Proposal.Description + " due to: " + e.Exception.Message);
        }

        public void SetHasFinished(string secondaryId) {
            if (SecondaryIds.ContainsKey(secondaryId)) {
                var proposal = SecondaryIds[secondaryId];
                if (!proposal.HasBeenInterrupted){
                    Logger.Debug("Setting " + secondaryId + " has finished");
                    proposal.MarkAsExecuted();                    
                }                        
            }
        }

        public void Execute() {
            foreach (var proposal in Proposals.Values.Where((IActionProposal p) => p.Status == ActionState.Pending))
                ExecuteProposal(proposal);
        }
        
        #region DisposablePattern
        private bool _disposed = false;

        ~ActionsSnapshot() {
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
                SecondaryIds.Clear();
            }

            _disposed = true;
        }
        #endregion
    }
}