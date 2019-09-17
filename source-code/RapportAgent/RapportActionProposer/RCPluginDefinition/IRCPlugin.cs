using RapportActionProposer.ActionProposalDefinition;
using System;

namespace RapportActionProposer.RCPluginDefinition {
    public interface IPluginLogger {
        void LogInfo(string msg);
        void LogError(string msg);
        void LogWarn(string msg);
        void LogDebug(string msg);
        void LogFatal(string msg);
    }

    public enum PluginType {
        Effector, Perceiver, Utilitary
    }

    public interface ISettingsProvider {
        bool SaveOnDispose { get; }
        void Save();
        void Load();
    }

    public interface IRCPlugin : IDisposable, IPluginLogger {
        IRapportController RapportController { get; }
        string Id { get; }
        string Name { get; }
        string Description { get; }
        PluginType Type { get; }
        bool IsEssential { get; }
        string OptionFolderPath { get; }
        string OptionsFileName { get; }

        bool ProvidesGUI { get; }
        void ShowGUI();

        void Configure(IRapportController controller, string optionsFolder);

        void Init();
        void InitDependencies();
        void Start();
        void Pause();
    }

    public interface IProposeActionStrategy {
        void ProposeAction(IEffectorPlugin plugin, IActionProposal proposal);
    }

    public interface IEffectorMethods {
        void ProposeAction(IActionProposal proposal);
        void ActionFinished(string secondaryId);
        void InterruptAction(string secondaryId);
        void ActivatePlugins(params string[] ids);
        void DesactivatePlugins(params string[] ids);
    }

    public interface IEffectorPlugin : IRCPlugin, IEffectorMethods { }

    public interface IPerceptionReceiverPlugin : IRCPlugin { }
}