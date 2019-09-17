using RapportActionProposer.RCPluginDefinition;

namespace RapportAgentPlugin.ViewModel.Sounds {

    public class SoundFileInfo {
        public string Id { get; }
        public string Location { get; }
        
        public SoundFileInfo(string id, string location) {
            this.Id = id;
            this.Location = location;
        }

        internal void Run(IEffectorPlugin plugin, ActionProposalFactory manager) {
            var proposal = manager.Sound(Id, 0, 0, 60000);
            if(proposal != null) {
                plugin.ProposeAction(proposal);
            }
        }
    }
}
