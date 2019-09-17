using RapportActionProposer.ActionProposalDefinition;
using RapportActionProposer.ProposeStrategies;
using System.Threading.Tasks;

namespace RapportActionProposer.RCPluginDefinition {
    public class EffectorOperations : IEffectorMethods {
        public IEffectorPlugin Plugin { get; }

        public IProposeActionStrategy ProposeActionStrategy { get; set; }

        public EffectorOperations(IEffectorPlugin plugin) {
            this.Plugin = plugin;

            var effectorAttributes = (EffectorMetadata[])Plugin.GetType().GetCustomAttributes(typeof(EffectorMetadata), true);
            if (effectorAttributes.Length > 0)
                ProposeActionStrategy = ProposeActionsFactory.CreateStrategy(effectorAttributes[0].ProposalsManagementStrategy, Plugin);            
        }

        public void ProposeAction(IActionProposal proposal) {
            if (proposal == null)
                return;

            proposal.ProposerId = Plugin.Id;
            ProposeActionStrategy.ProposeAction(Plugin, proposal);
        }

        public void ActionFinished(string secondaryId) {
            Task.Factory.StartNew(() => Plugin.RapportController.ActionFinished(secondaryId));
        }

        public void InterruptAction(string secondaryId) {
            Task.Factory.StartNew(() => Plugin.RapportController.InterruptAction(secondaryId));
        }

        public void ActivatePlugins(params string[] ids) {
            Task.Factory.StartNew(() => Plugin.RapportController.ActivatePlugins(ids));
        }

        public void DesactivatePlugins(params string[] ids) {
            Task.Factory.StartNew(() => Plugin.RapportController.DesactivatePlugins(ids));
        }
    }
}
