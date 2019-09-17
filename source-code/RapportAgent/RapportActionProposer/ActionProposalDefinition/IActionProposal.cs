using System;

public enum ActionState {
    Pending, Executing, Executed, Interrupted
}

namespace RapportActionProposer.ActionProposalDefinition {
    public class ProposalChangedStatusEventArgs : EventArgs {
        public IActionProposal Proposal { get; }
        public ProposalChangedStatusEventArgs(IActionProposal proposal) {
            this.Proposal = proposal;
        }
    }

    public class ProposalActionsContainsErrorsEventArgs : EventArgs {
        public IActionProposal Proposal { get; }
        public Exception Exception { get; }
        public ProposalActionsContainsErrorsEventArgs(IActionProposal proposal, Exception exception) {
            this.Proposal = proposal;
            this.Exception = exception;
        }
    }

    public interface IActionProposal : IActionProposalCommon {
        Action ExecutionMethod { get; }
        Action InterruptMethod { get; }

        string ProposerId { get; set; }
        double InitialDelay { get; set; }

        void AddInterruptionAction(Action interruption, string secondaryId, double timeOutMs = 5000);
        void MarkAsExecuted();

        //events
        event EventHandler<ProposalChangedStatusEventArgs> Executing;
        event EventHandler<ProposalChangedStatusEventArgs> Executed;
        event EventHandler<ProposalChangedStatusEventArgs> Interrupted;
        event EventHandler<ProposalActionsContainsErrorsEventArgs> ErrorInExecution;
    }

    public interface IActionProposalCommon : IExecutable, IInterruptable {
        uint Id { get; }
        string Description { get; }
        ActionGroup Group { get; }
        ushort Priority { get; }
        string SecondaryId { get; } //e.g., animation id, speech id
        bool IsInterruptible { get; }

        //controll variables
        DateTime TimeStart { get; }
        DateTime ExpirationTime { get; }
        DateTime TimeEnd { get; }
        double TimeoutMs { get; }
        bool Expired { get; }

        ActionState Status { get; }
        bool HasExecuted { get; }
        bool HasBeenInterrupted { get; }
        bool IsExecuting { get; }
        bool IsPending { get; }

        void Delay();
    }

    public interface IImmutableActionProposal : IActionProposalCommon, IExecutable {
        string ProposerId { get; }
        double InitialDelay { get; }
    }
}
