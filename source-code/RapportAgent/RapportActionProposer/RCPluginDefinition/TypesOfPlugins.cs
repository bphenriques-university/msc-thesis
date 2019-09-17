using RapportActionProposer.ActionProposalDefinition;
using RapportActionProposer.ProposeStrategies;

namespace RapportActionProposer.RCPluginDefinition {

    [EffectorMetadata(ProposalsManagementStrategy = ProposeStrategyType.OneActionPerGroup)]
    public abstract class EffectorPlugin : RCPlugin, IEffectorPlugin {
        public override PluginType Type => PluginType.Effector;

        protected EffectorOperations Operations { get; }

        public EffectorPlugin() : base() {
            Operations = new EffectorOperations(this);
        }

        public void ProposeAction(IActionProposal proposal) {
            Operations.ProposeAction(proposal);
        }

        public void ActionFinished(string secondaryId) {
            Operations.ActionFinished(secondaryId);
        }

        public void InterruptAction(string secondaryId) {
            Operations.InterruptAction(secondaryId);
        }

        public void ActivatePlugins(params string[] ids) {
            Operations.ActivatePlugins(ids);
        }

        public void DesactivatePlugins(params string[] ids) {
            Operations.DesactivatePlugins(ids);
        }
    }

    public abstract class PerceiverPlugin : RCPlugin, IPerceptionReceiverPlugin {
        public override PluginType Type => PluginType.Perceiver;
    }
}