using RapportActionProposer.ProposeStrategies;
using System;

namespace RapportActionProposer.RCPluginDefinition {
    [AttributeUsage(AttributeTargets.Class)]
    public class RCPluginMetadata : Attribute {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEssential { get; set; } = false;
        public Type WindowType { get; set; } = null;
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class EffectorMetadata : Attribute {
        public ProposeStrategyType ProposalsManagementStrategy { get; set; } = ProposeStrategyType.Unrestricted;
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ConfigurablePluginMetadata : Attribute {
        public bool SaveOnDispose { get; set; } = false;
    }
}