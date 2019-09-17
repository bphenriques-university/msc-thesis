using RapportActionProposer.ActionProposalDefinition;
using System;
using System.Collections.Generic;
using RapportActionProposer;
using Log4NetWrapperLite;

namespace RapportControllerLib {
    public class ActionProposersManager : IActionProposersManager {
        #region events
        public event EventHandler<SnapshotEventEventArgs> SnapShotEventHandler = delegate { };
        public event EventHandler<ActionGroupsStatusChangedEventArgs> ExecutingActionGroup {
            add { ActionsSnapshot.GlobalExecutingActionGroup += value; }
            remove { ActionsSnapshot.GlobalExecutingActionGroup -= value; }
        }
        public event EventHandler<ActionGroupsStatusChangedEventArgs> ExecutedActionGroup {
            add { ActionsSnapshot.GlobalExecutedActionGroup += value; }
            remove { ActionsSnapshot.GlobalExecutedActionGroup -= value; }
        }
        public event EventHandler<ActionGroupsStatusChangedEventArgs> InterruptedActionGroup {
            add { ActionsSnapshot.GlobalInterruptedActionGroup += value; }
            remove { ActionsSnapshot.GlobalInterruptedActionGroup -= value; }
        }

        #endregion

        private IActionsSnapshot snapshot = new ActionsSnapshot();
        private readonly object proposalsLock = new object();
        private readonly Dictionary<ActionGroup, object> actionGroupLocks = new Dictionary<ActionGroup, object>();

        public void AddAction(IActionProposal proposal){
            ActionGroup key = proposal.Group;
            if (!actionGroupLocks.ContainsKey(key))
                actionGroupLocks[key] = new object();            

            lock (actionGroupLocks[key]) lock (proposalsLock) {
                snapshot.AddProposal(proposal);
            }
        }
        
        public IActionsSnapshot CaptureSnapshot() {
            IActionsSnapshot currentSnapshot = null;
            
            lock (proposalsLock) {
                currentSnapshot = snapshot;

                //merge old fields
                snapshot = new ActionsSnapshot(currentSnapshot);
            }

            currentSnapshot.MarkAsOutdated();

            if(currentSnapshot.Proposals.Count > 0) {
                var args = new SnapshotEventEventArgs(new ImmutableActionsSnapshot(currentSnapshot));
                foreach (EventHandler<SnapshotEventEventArgs> ev in SnapShotEventHandler.GetInvocationList())
                    ev.BeginInvoke(this, args, null, null);

            }

            return currentSnapshot;
        }

        public void ActionFinished(string id) {
            snapshot.SetHasFinished(id);
        }

        public void InterruptAction(string id) {
            snapshot.InterruptSecondaryIdAction(id);
        }       
    }
}