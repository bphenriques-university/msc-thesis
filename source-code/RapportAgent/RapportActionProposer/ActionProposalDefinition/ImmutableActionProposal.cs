using System;

namespace RapportActionProposer.ActionProposalDefinition {

    public class ImmutableActionProposal : IImmutableActionProposal {
        public IActionProposal Original { get; }

        public uint Id => Original.Id;
        public string Description => Original.Description;
        public ActionGroup Group => Original.Group;
        public ushort Priority => Original.Priority;
        public bool IsInterruptible => Original.IsInterruptible;

        public double InitialDelay => Original.InitialDelay;
        public DateTime TimeStart => Original.TimeStart;
        public DateTime ExpirationTime => Original.ExpirationTime;
        public DateTime TimeEnd => Original.TimeEnd;
        public double TimeoutMs => Original.TimeoutMs;
        public string ActionId => Original.SecondaryId;
        public string ProposerId => Original.ProposerId;
        public string SecondaryId => Original.SecondaryId;

        //mutable fields
        public ActionState Status { get;}
        public bool HasExecuted { get; }
        public bool HasBeenInterrupted { get; }
        public bool IsExecuting { get; }
        public bool IsPending { get; }
        public bool Expired => Original.Expired;

        public ImmutableActionProposal(IActionProposal proposal) {
            this.Original = proposal;
            Status = Original.Status;
            HasExecuted = Original.HasExecuted;
            HasBeenInterrupted = Original.HasBeenInterrupted;
            IsExecuting = Original.IsExecuting;
            IsPending = Original.IsPending;
        }

        public void Execute() {
            Delay();
            Original.ExecutionMethod();
        }

        public void Delay() {
            Original.Delay();
        }

        public void Interrupt() {
            Original.Interrupt();
        }
    }
}