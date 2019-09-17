using RapportActionProposer.ActionProposalDefinition;
using RapportActionProposer.RCPluginDefinition;
using System;

namespace RapportActionProposer {
    public class ActionGroupsStatusChangedEventArgs : EventArgs {
        public IActionProposal Proposal { get; }
        public ActionState State { get; }

        public ActionGroupsStatusChangedEventArgs(IActionProposal proposal, ActionState state) {
            this.Proposal = proposal;
            this.State = state;
        }
    }
    
    public class PluginEnableStatusChangedEventArgs : EventArgs {
        public IRCPlugin Plugin { get; }
        public bool Active { get; }
        public string ErrorMessage { get; }
        public bool ContainsError => !string.IsNullOrEmpty(ErrorMessage);

        public PluginEnableStatusChangedEventArgs(IRCPlugin plugin, bool active, string errorMessage = "") {
            this.Plugin = plugin;
            this.Active = active;
            this.ErrorMessage = errorMessage;
        }
    }

    public class PluginLoadedEventArgs : EventArgs {
        public IRCPlugin Plugin { get; }
        public bool Enabled { get; }
        public string ErrorMsg { get; }

        public PluginLoadedEventArgs(IRCPlugin plugin, string errorMsg, bool enabled) {
            this.Plugin = plugin;
            this.Enabled = enabled;
            this.ErrorMsg = errorMsg;
        }
    }
}
