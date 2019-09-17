using HelpersForNet;
using RapportActionProposer.ProposeStrategies;
using RapportActionProposer.RCPluginDefinition;
using RapportAgentPlugin.ViewModel;
using RapportAgentPlugin.ViewModel.Utterances;
using System.ComponentModel.Composition;

namespace RapportAgentPlugin {

    public class AgentActionsManagerSettings {
        public class UtteranceVariableDefinition {
            public string Variable { get; set;  }
            public string Replacement { get; set; }
        }

        public string Character { get; set; } = "QuickNumbers";
        public string UtteranceFileName { get; set; } = UtterancesManager.DEFAULT_UTTERANCES_FILE_NAME;
        public string SoundsFolder { get; set; } = SoundsManagerModel.DEFAULT_DIRECTORY_NAME;
        public string UtterancesFolder { get; set; } = UtterancesManager.DEFAULT_DIRECTORY_NAME;
        public SerializableDictionary<string, string> AvailableReplacementVariables { get; set; } = new SerializableDictionary<string, string>();
    }
       
    [Export(typeof(IRCPlugin))]
    [RCPluginMetadata(Description = "Emys Actions Wrapper and Manager", IsEssential = true, WindowType = typeof(MainWindow))]
    [EffectorMetadata(ProposalsManagementStrategy = ProposeStrategyType.Unrestricted)]
    [ConfigurablePluginMetadata(SaveOnDispose = true)]
    public sealed class AgentActionsManager : EffectorPlugin<AgentActionsManagerSettings> {

        internal ActionsManagerViewModel Model { get; private set; }
        public ActionProposalFactory Actions => Model.ActionProposalFactory;        

        public override void Init() {
            base.Init();

            Model = Singleton<ActionsManagerViewModel>.Instance;
            Model.Plugin = this;
            Model.Init();
        }

        public override void Dispose() {
            base.Dispose();
            Model.Dispose();
        }
    }
}