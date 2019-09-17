namespace RapportActionProposer.ActionProposalDefinition {

    public abstract class ActionGroup {
        public string Type { get; }

        public ActionGroup(string type) {
            this.Type = type;
        }

        public override string ToString() {
            return Type.ToString();
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType())
                return false;

            return (obj as ActionGroup).Type.Equals(Type);
        }

        public override int GetHashCode() {            
            return Type.GetHashCode();
        }
    }    
}