using RapportActionProposer.ActionProposalDefinition;
using RapportActionProposer.RCPluginDefinition;
using System;

namespace RapportActionProposer {
    public interface IExecutable {
        void Execute();
    }

    public interface IInterruptable {
        void Interrupt();
    }

    public interface ILogger {
        void LogInfo(IRCPlugin proposer, string msg);
        void LogError(IRCPlugin proposer, string msg);
        void LogWarn(IRCPlugin proposer, string msg);
        void LogDebug(IRCPlugin proposer, string msg);
        void LogFatal(IRCPlugin proposer, string msg);
    }

    public interface IRapportController : IDisposable, ILogger {
        void ProposeAction(IActionProposal proposal);
        void ActionFinished(string id);
        void InterruptAction(string id);

        T GetPlugin<T>() where T : IRCPlugin;

        void ActivatePlugins(params string[] ids);
        void DesactivatePlugins(params string[] ids);

        event EventHandler<ActionGroupsStatusChangedEventArgs> ExecutingActionGroup;
        event EventHandler<ActionGroupsStatusChangedEventArgs> ExecutedActionGroup;
        event EventHandler<ActionGroupsStatusChangedEventArgs> InterruptedActionGroup;
    }
}
