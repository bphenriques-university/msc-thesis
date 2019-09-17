using System;
using System.Threading;

namespace RapportActionProposer.ActionProposalDefinition {
    
    public class ActionProposal : IActionProposal {
        private volatile static uint _id_generated = 0;
        public uint Id { get; } = ++_id_generated % uint.MaxValue;

        public string ProposerId { get; set; }

        public string Description { get; }
        public ushort Priority { get; }
        public ActionGroup Group { get; }

        public string SecondaryId { get; private set; }

        public bool Expired => TimeEnd > ExpirationTime;

        public bool HasExecuted => Status == ActionState.Executed;
        public bool HasBeenInterrupted => Status == ActionState.Interrupted;
        public bool IsExecuting => Status == ActionState.Executing;
        public bool IsPending => Status == ActionState.Pending;

        public Action ExecutionMethod { get; }

        public Action InterruptMethod { get; private set; }
        public bool IsInterruptible => TimeoutMs > 0 && TimeoutMs < int.MaxValue && InterruptMethod != null;
        public double TimeoutMs { get; private set; } = int.MaxValue;

        public DateTime TimeStart { get; private set; } = DateTime.MinValue;
        public DateTime ExpirationTime { get; private set; } = DateTime.MaxValue;
        public DateTime TimeEnd { get; private set; } = DateTime.MaxValue;

        private double _initialDelay = 0;
        public double InitialDelay {
            get { return _initialDelay; }
            set {
                _initialDelay = Math.Max(0, value);
            }
        }

        //events
        public event EventHandler<ProposalChangedStatusEventArgs> Executing = delegate { };
        public event EventHandler<ProposalChangedStatusEventArgs> Executed = delegate { };
        public event EventHandler<ProposalChangedStatusEventArgs> Interrupted = delegate { };
        public event EventHandler<ProposalActionsContainsErrorsEventArgs> ErrorInExecution = delegate { };

        private ActionState _status = ActionState.Pending;
        public ActionState Status {
            get {
                return _status;
            }
            set {
                if(value != _status) {
                    _status = value;
                    var args = new ProposalChangedStatusEventArgs(this);
                    switch (value) {
                        case ActionState.Executing:
                            TimeStart = DateTime.Now;
                            ExpirationTime = TimeStart.AddMilliseconds(InitialDelay + TimeoutMs);

                            foreach (EventHandler<ProposalChangedStatusEventArgs> receiver in Executing.GetInvocationList())
                                receiver.BeginInvoke(this, args, null, null);
                            break;
                        case ActionState.Interrupted:
                            TimeEnd = DateTime.Now;
                            foreach (EventHandler<ProposalChangedStatusEventArgs> receiver in Interrupted.GetInvocationList())
                                receiver.BeginInvoke(this, args, null, null);
                            break;
                        case ActionState.Executed:
                            TimeEnd = DateTime.Now;
                            foreach (EventHandler<ProposalChangedStatusEventArgs> receiver in Executed.GetInvocationList())
                                receiver.BeginInvoke(this, args, null, null);
                            break;
                    }
                }
            }
        }        

        public ActionProposal(ushort priority, ActionGroup group, Action execution, string description) {
            this.Priority = priority;
            this.Group = group;
            this.ExecutionMethod = execution;
            this.Description = description;
        }

        public void AddInterruptionAction(Action interruption, string secondaryId, double timeOutMs) {
            SecondaryId = secondaryId;
            InterruptMethod = interruption;
            TimeoutMs = timeOutMs;           
        }
        
        public void Delay() {
            if (InitialDelay > 0) {
                //more accurate than thread sleep
                var timelock = new object();
                lock (timelock) { Monitor.Wait(timelock, TimeSpan.FromMilliseconds(InitialDelay)); }
            }
        }

        private void ExecuteActionMethod() {
            try {
                ExecutionMethod();
            }
            catch (Exception e) {
                var args = new ProposalActionsContainsErrorsEventArgs(this, e);
                foreach (EventHandler<ProposalActionsContainsErrorsEventArgs> receiver in ErrorInExecution.GetInvocationList())
                    receiver.BeginInvoke(this, args, null, null);
            }
        }

        public void Execute() {
            Status = ActionState.Executing;
            
            Delay();

            if (!HasBeenInterrupted && !HasExecuted) //sanitize check
                ExecuteActionMethod();

            if (!IsInterruptible)
                MarkAsExecuted();
        }

        public void MarkAsExecuted() {
            Status = ActionState.Executed;
        }

        public void Interrupt() {
            if (Status == ActionState.Executing && IsInterruptible) {
                InterruptMethod();
                Status = ActionState.Interrupted;
            }
        }
    }
}