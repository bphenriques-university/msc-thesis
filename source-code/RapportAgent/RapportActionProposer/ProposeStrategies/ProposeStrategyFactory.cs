using RapportActionProposer.RCPluginDefinition;
using System;

namespace RapportActionProposer.ProposeStrategies {
    public enum ProposeStrategyType {
        Unrestricted,
        OneActionGlobal,
        OneActionPerGroup
    }

    public static class ProposeActionsFactory {
        public static IProposeActionStrategy CreateStrategy(ProposeStrategyType strategy, IEffectorPlugin plugin) {
            if (plugin == null)
                throw new ArgumentException("Plugin must not be null");            

            switch (strategy) {
                case ProposeStrategyType.Unrestricted:
                    return new UnrestrictedProposeActionStrategy(plugin);
                case ProposeStrategyType.OneActionGlobal:
                    return new OneActionGlobalProposeActionStrategy(plugin);
                case ProposeStrategyType.OneActionPerGroup:
                    return new OneActionPerGroupProposeActionStrategy(plugin);
            }

            throw new ArgumentException("Invalid ProposeStrategy");
        }
    }
}