using RapportActionProposer.RCPluginDefinition;
using System.ComponentModel.Composition;

namespace UserInformationHolder {

    public class UserInformationSetting {
        public class BiographySetting {
            public string Name { get; set; } = "Human";
            public int Age { get; set; } = -1;
        }

        public BiographySetting Biography { get; set; } = new BiographySetting();
        public bool Introvert { get; set; } = false;
    }

    [Export(typeof(IRCPlugin))]
    public class UserInformation : PerceiverPlugin<UserInformationSetting> {
        public UserInformation() : base("Contains relevant User Information") { }

        public bool Introvert => Settings.Introvert;
        public string HumanName => Settings.Biography.Name;
        public int Age => Settings.Biography.Age;

        public override void Start() {
            base.Start();

            LogError(Settings == null ? "NULL1" : "NOT NULL1");
            LogError(Settings.Biography == null ? "NULL2" : "NOT NULL2");

            LogInfo("New User entered: " + HumanName + " with age " + Age + " and he is " + (Introvert ? "Introvert" : "Extrovert"));            
        }
    }
}