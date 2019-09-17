using RapportActionProposer.RCPluginDefinition;
using System.ComponentModel.Composition;

namespace GRETAConnection {

    public class ConnectionInformation {
        public string Ip { get; set; } = "localhost";
        public int Port { get; set; } = 1102;
    }

    [Export(typeof(IRCPlugin))]
    [RCPluginMetadata(Description = "Retrieves HeadNod, HeadShake, Happy, Sad and Surprise perceptions")]
    public class GretaPerceptionReceiver : PerceiverPlugin<ConnectionInformation> {
        public GretaChannelReader gretaChannelReceiver;

        public override void Start() {
            base.Start();

            string ip = Settings.Ip;
            int port = Settings.Port;
            gretaChannelReceiver = new GretaChannelReader(this, ip, port);
            LogInfo("Started monitorization of Greta at " + ip + ":" + port);
            gretaChannelReceiver.Start();        
        }

        public override void Dispose() {
            base.Dispose();

            if(gretaChannelReceiver != null)
                gretaChannelReceiver.Stop();

            gretaChannelReceiver = null;
        }
    }
}
