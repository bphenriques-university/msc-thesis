using RapportActionProposer;
using RapportActionProposer.ActionProposalDefinition;
using System;
using System.Collections.Generic;

namespace RapportControllerLib {
    public enum ControllerStatus {
        Initializing, Importing, Ready, Running, Paused, Disposing, Disposed
    }

    public class ControllerStatusChangedEventArgs : EventArgs {
        public ControllerStatus Status { get; }
        public ControllerStatusChangedEventArgs(ControllerStatus status) {
            Status = status;
        }
    }

    public class SnapshotEventEventArgs : EventArgs {
        public IImmutableActionsSnapshot Snapshot { get; }
        public SnapshotEventEventArgs(IImmutableActionsSnapshot snapshot) {
            this.Snapshot = snapshot;
        }
    }

    public interface IRapportControllerManager : IRapportController {
        int NumberOfPlugins { get; }
        string PluginsPath { get; }
        string OptionsFolderPath { get; }

        bool StartMonitoring(int frequency);
        void StopMonitoring();
        bool IsRunning { get; }

        int ImportPlugins();
        bool IsPluginActive(string name);

        event EventHandler<PluginLoadedEventArgs> PluginLoadedEventArgs;
        event EventHandler<PluginEnableStatusChangedEventArgs> PluginEnableStatusChangedEvent;
        event EventHandler<SnapshotEventEventArgs> SnapShotEventHandler;
        event EventHandler<ControllerStatusChangedEventArgs> ControllerStatusChanged;
    }

    public interface IActionProposersManager {
        void AddAction(IActionProposal proposal);
        IActionsSnapshot CaptureSnapshot();
        void ActionFinished(string id);
        void InterruptAction(string id);

        event EventHandler<SnapshotEventEventArgs> SnapShotEventHandler;
        event EventHandler<ActionGroupsStatusChangedEventArgs> ExecutingActionGroup;
        event EventHandler<ActionGroupsStatusChangedEventArgs> ExecutedActionGroup;
        event EventHandler<ActionGroupsStatusChangedEventArgs> InterruptedActionGroup;
    }


    #region ActionSnapshots

    public interface IActionSnapshotCommon : IDisposable, IExecutable {
        int Id { get; }
        DateTime TimeStamp { get; }
    }

    public interface IImmutableActionsSnapshot : IActionSnapshotCommon {
        List<ImmutableActionProposal> Proposals { get; }
        int ExecutingProposals { get; }
        int ExecutedProposals { get; }
        int PendingProposals { get; }
        int InterruptedProposals { get; }
    }

    public interface IActionsSnapshot : IActionSnapshotCommon {
        Dictionary<ActionGroup, IActionProposal> Proposals { get; }
        Dictionary<string, IActionProposal> SecondaryIds { get; }
        List<IActionProposal> RemovedProposals { get; }

        void AddProposal(IActionProposal proposal);
        void InterruptSecondaryIdAction(string id);
        void SetHasFinished(string id);
        void MarkAsOutdated();
    }

    #endregion
}