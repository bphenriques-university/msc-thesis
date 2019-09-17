using RapportActionProposer.ActionProposalDefinition;
using RapportActionProposer.RCPluginDefinition;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RapportActionProposer.ProposeStrategies {
    public class OneActionPerGroupProposeActionStrategy : IProposeActionStrategy {
        private class GroupMetadata {
            public ActionGroup Group { get; }
            public int MinimumTimeBetweenActionsMs { get; set; } = 0;
            private DateTime LastActionTime => LastProposal != null ? LastProposal.TimeStart : DateTime.Now;
            private IActionProposal LastProposal { get; set; } = null;
            public bool AgentIsPerforming { get; private set; } = false;
            public bool DidElapsedMinimumTime => (DateTime.Now - LastActionTime).TotalMilliseconds >= MinimumTimeBetweenActionsMs;


            public GroupMetadata(ActionGroup group) {
                this.Group = group;
            }

            public override bool Equals(object obj) {
                if (obj == null || GetType() != obj.GetType())
                    return false;

                return (obj as ActionGroup).Type.Equals(Group);
            }

            public override int GetHashCode() {
                return Group.GetHashCode();
            }

            public bool ProposedPriorityIsHigher(IActionProposal proposal) {
                return LastProposal == null || proposal.Priority > LastProposal.Priority;
            }

            public void ExecutedOrInterrupted(IActionProposal proposal) {
                AgentIsPerforming = false;
            }

            public void Executing(IActionProposal proposal) {
                LastProposal = proposal;
                AgentIsPerforming = true;
            }
        }

        public IEffectorPlugin Plugin { get; }
        private Dictionary<ActionGroup, GroupMetadata> GroupsMetadata { get; } = new Dictionary<ActionGroup, GroupMetadata>();

        public OneActionPerGroupProposeActionStrategy(IEffectorPlugin plugin) {
            this.Plugin = plugin;
        }

        public void ProposeAction(IEffectorPlugin plugin, IActionProposal proposal) {
            var group = proposal.Group;
            GroupMetadata groupMetadata = null;
            if (GroupsMetadata.ContainsKey(group))
                groupMetadata = GroupsMetadata[group];            
            else {
                GroupsMetadata.Add(group, new GroupMetadata(group));
                groupMetadata = GroupsMetadata[group];
            }


            if ((!groupMetadata.AgentIsPerforming && groupMetadata.DidElapsedMinimumTime) || 
                groupMetadata.ProposedPriorityIsHigher(proposal)) {

                proposal.Executing += Proposal_Executing;
                proposal.Executed += Proposal_ExecutedOrInterrupted;
                proposal.Interrupted += Proposal_ExecutedOrInterrupted;

                Task.Factory.StartNew(() => Plugin.RapportController.ProposeAction(proposal));
            }
        }

        public void SetTimeout(ActionGroup group, int ms) {
            if (GroupsMetadata.ContainsKey(group))
                GroupsMetadata[group].MinimumTimeBetweenActionsMs = ms;           
        }

        private void Proposal_ExecutedOrInterrupted(object sender, ProposalChangedStatusEventArgs e) {
            GroupsMetadata[e.Proposal.Group].ExecutedOrInterrupted(e.Proposal);
        }

        private void Proposal_Executing(object sender, ProposalChangedStatusEventArgs e) {
            GroupsMetadata[e.Proposal.Group].Executing(e.Proposal);
        }
    }
}