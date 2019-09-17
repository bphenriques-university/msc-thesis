using RapportActionProposer.ActionProposalDefinition;

namespace RapportAgentPlugin {
    public class AgentActionGroup : ActionGroup {
        public static ActionGroup Speech => new AgentActionGroup("Speech");
        public static ActionGroup Utterances => new AgentActionGroup("Utterances");
        public static ActionGroup Gaze => new AgentActionGroup("Gaze");
        public static ActionGroup Animation => new AgentActionGroup("Animation");
        public static ActionGroup Head => new AgentActionGroup("Head");

        private AgentActionGroup(string type) : base(type) { }
    }
}
