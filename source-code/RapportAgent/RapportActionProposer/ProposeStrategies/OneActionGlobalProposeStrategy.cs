using RapportActionProposer.ActionProposalDefinition;
using RapportActionProposer.RCPluginDefinition;
using System;
using System.Threading.Tasks;

namespace RapportActionProposer.ProposeStrategies {
    public class OneActionGlobalProposeActionStrategy : IProposeActionStrategy {
        public IEffectorPlugin Plugin { get; }
        public int MinimumTimeBetweenActionsMs { get; set; } = 0;
        private bool AgentIsPerforming { get; set; } = false;

        private DateTime LastActionTime => LastProposal != null ? LastProposal.TimeStart : DateTime.Now;
        private IActionProposal LastProposal { get; set; } = null;
        public bool DidElapsedMinimumTime => (DateTime.Now - LastActionTime).TotalMilliseconds >= MinimumTimeBetweenActionsMs;

        public OneActionGlobalProposeActionStrategy(IEffectorPlugin plugin) {
            this.Plugin = plugin;
        }

        public void ProposeAction(IEffectorPlugin plugin, IActionProposal proposal) {
            if ((!AgentIsPerforming && DidElapsedMinimumTime) || ProposedPriorityIsHigher(proposal)) {

                proposal.Executing += Proposal_Executing;
                proposal.Executed += Proposal_ExecutedOrInterrupted;
                proposal.Interrupted += Proposal_ExecutedOrInterrupted;

                Task.Factory.StartNew(() => Plugin.RapportController.ProposeAction(proposal));
            }
        }

        private bool ProposedPriorityIsHigher(IActionProposal proposal) {
            return LastProposal == null || proposal.Priority > LastProposal.Priority;
        }

        private void Proposal_ExecutedOrInterrupted(object sender, ProposalChangedStatusEventArgs e) {
            AgentIsPerforming = false;
        }

        private void Proposal_Executing(object sender, ProposalChangedStatusEventArgs e) {
            LastProposal = e.Proposal;
            AgentIsPerforming = true;
        }
    }
}
