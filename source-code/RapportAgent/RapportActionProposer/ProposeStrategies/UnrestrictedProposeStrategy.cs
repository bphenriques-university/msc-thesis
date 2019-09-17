using RapportActionProposer.ActionProposalDefinition;
using RapportActionProposer.RCPluginDefinition;
using System.Threading.Tasks;

namespace RapportActionProposer.ProposeStrategies {
    public class UnrestrictedProposeActionStrategy : IProposeActionStrategy {
        public IEffectorPlugin Plugin { get; }

        public UnrestrictedProposeActionStrategy(IEffectorPlugin plugin) {
            this.Plugin = plugin;
        }

        public void ProposeAction(IEffectorPlugin plugin, IActionProposal proposal) {
            Task.Factory.StartNew(() => Plugin.RapportController.ProposeAction(proposal));
        }
    }
}
