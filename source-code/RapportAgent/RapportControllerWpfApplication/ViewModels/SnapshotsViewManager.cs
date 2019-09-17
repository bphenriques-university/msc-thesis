using RapportActionProposer.ActionProposalDefinition;
using RapportControllerLib;
using RapportControllerWpfApplication.ViewModels.ContextMenu;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace RapportControllerWpfApplication.ViewModels {
    
    public class SnapshotsViewManager : IDisposable, INotifyPropertyChanged {

        #region fields

        private ObservableCollection<RecordOptionTime> _availableRecordingTimes = new ObservableCollection<RecordOptionTime>();
        public ObservableCollection<RecordOptionTime> AvailableRecordingTimes {
            get { return _availableRecordingTimes; }
            set {
                _availableRecordingTimes = value;
                OnPropertyChanged("AvailableRecordingTimes");
            }
        }

        private ObservableCollection<IImmutableActionsSnapshot> _snapshots = new ObservableCollection<IImmutableActionsSnapshot>();
        public ObservableCollection<IImmutableActionsSnapshot> Snapshots {
            get { return _snapshots; }
            set {
                _snapshots = value;
                OnPropertyChanged("Snapshots");                
            }
        }

        private bool _ignoreExecuting = true;
        public bool IgnoreExecuting {
            get { return _ignoreExecuting; }
            set {
                _ignoreExecuting = value;
                previousOnlyContainedExecuting = false;
                OnPropertyChanged("IgnoreExecuting");
            }
        }

        #endregion

        public SnapshotsViewManager() {
            AddRecordTimeEntry(new RecordOptionTime("Don't Record", 0));
            AddRecordTimeEntry(new RecordOptionTime("5 Seconds", 5));
            AddRecordTimeEntry(new RecordOptionTime("10 Seconds", 10));
            AddRecordTimeEntry(new RecordOptionTime("15 Seconds", 15));
            AddRecordTimeEntry(new RecordOptionTime("30 Seconds", 30));
            AddRecordTimeEntry(new RecordOptionTime("5 Minutes", 5*60));
            AddRecordTimeEntry(new RecordOptionTime("15 Minutes", 15 * 60));

            //default value
            var defaultOption = new RecordOptionTime("Forever", int.MaxValue);
            defaultOption.Toggle();
            AddRecordTimeEntry(defaultOption);            
        }

        private void AddRecordTimeEntry(RecordOptionTime recordTime) {
            AvailableRecordingTimes.Add(recordTime);
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private void OnPropertyChanged(string property) {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        private bool previousOnlyContainedExecuting = false;
        public void AddSnapshot(IImmutableActionsSnapshot snapshot) {
            int maximumRecordingTime = RecordOptionTime.SelectedRecordingTime.Time;
            if (maximumRecordingTime > 0 && snapshot.Proposals.Count > 0) {
                IImmutableActionProposal first = snapshot.Proposals.First();

                for (int i = 0; i < Snapshots.Count; i++) {
                    IImmutableActionsSnapshot p = Snapshots.ElementAt(i);
                    if ((snapshot.TimeStamp - p.TimeStamp).TotalSeconds >= maximumRecordingTime) {
                        Snapshots.RemoveAt(i);
                        continue;
                    }
                    //remaining objects are updated
                    break;
                }

                bool add = true;
                if (IgnoreExecuting) {
                    bool containsOnlyExecuting = snapshot.InterruptedProposals + snapshot.PendingProposals + snapshot.ExecutedProposals == 0;
                    add = !previousOnlyContainedExecuting || !containsOnlyExecuting;
                    previousOnlyContainedExecuting = containsOnlyExecuting;
                }

                if (add)
                    Snapshots.Add(snapshot);
            }
        }

        public void Clear() {
            foreach(IImmutableActionsSnapshot p in Snapshots)
                p.Dispose();            
            Snapshots.Clear();
        }

        public void Dispose() {
            Clear();
        }
    }
}