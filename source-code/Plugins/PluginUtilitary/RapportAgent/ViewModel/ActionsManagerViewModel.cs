using RapportAgentPlugin.ViewModel.Utterances;
using System;

namespace RapportAgentPlugin.ViewModel {
    public class ActionsManagerViewModel : IDisposable {
        public AgentActionsManager Plugin { get; internal set; }

        public UtterancesManager UtterancesManager { get; private set; }
        public SoundsManagerModel SoundsManager { get; private set; }
        public ActionProposalFactory ActionProposalFactory { get; private set; }    

        internal void RefreshGUI() {
            UtterancesManager.RefreshGUI();
            SoundsManager.RefreshGUI();
        }

        internal void Init() {

            SoundsManager = new SoundsManagerModel(Plugin);
            SoundsManager.Reload();

            UtterancesManager = new UtterancesManager(Plugin);
            UtterancesManager.Reload();
            ActionProposalFactory = new ActionProposalFactory(Plugin, UtterancesManager, SoundsManager);
        }

        public void Dispose() {
            ActionProposalFactory.Dispose();
        }
    }
}
