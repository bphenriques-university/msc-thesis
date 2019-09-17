using RapportActionProposer.RCPluginDefinition;
using System;
using System.Globalization;

namespace GRETAConnection {

    public class EmotionEventArgs :EventArgs {
        public double Intensity { get; }
        public EmotionEventArgs(double intensity) {
            this.Intensity = intensity;
        }
    }

    public class BackchannelEventArgs : EventArgs {
        public string Intensity { get; }
        public BackchannelEventArgs(string intensity) {
            this.Intensity = intensity;
        }
    }

    public class GretaChannelReader : IUDPMessageReader {

        public event EventHandler<EmotionEventArgs> HappyEvent = delegate { };
        public event EventHandler<EmotionEventArgs> SadEvent = delegate { };
        public event EventHandler<EmotionEventArgs> SurpriseEvent = delegate { };

        public event EventHandler HeadShakeEvent = delegate { };
        public event EventHandler HeadNodEvent = delegate { };

        public event EventHandler<BackchannelEventArgs> BackchannelEvent = delegate { };

        private UDPClient Client { get; }
        private IRCPlugin Plugin { get; }
        public GretaChannelReader(IRCPlugin plugin, string ip, int port) {
            this.Plugin = plugin;
            this.Client = new UDPClient(this, ip, port);
        }

        public void Start() {
            Client.Start();
        }

        public void Stop() {
            Client.Stop();
        }

        public void Read(string msg) {
            msg = msg.Trim();

            string[] parts = msg.Split(' ');

            if (parts.Length >= 1) {
                string msgType = parts[0];

                switch (msgType) {
                    case "MIMIC":
                        if (parts[1].ToLower().Contains("nod")) {
                            HeadNodEventAux();
                        }
                        else {
                            HeadShakeEventAux();
                        }
                        break;
                    case "BACKCHANNEL":
                        string bcintensity = parts[1];
                        BackchannelEventAux(bcintensity);

                        break;
                    case "EXPRESSION":
                        string[] parameters = parts[1].Split('=');
                        string emotion = parameters[0];
                        double intensity = double.Parse(parameters[1], CultureInfo.InvariantCulture); //parse . as decimal

                        if (emotion == "happy") {
                            HappyEventAux(intensity);
                        }
                        else if (emotion == "sad") {
                            SadEventAux(intensity);
                        }
                        else if (emotion == "surprised") {
                            SurprisedEventAux(intensity);
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        private void BackchannelEventAux(string bcintensity) {
            //Plugin.LogDebug("BACKCHANNEL EVENT, sending to " + BackchannelEvent.GetInvocationList().Length + " subscribers: " + bcintensity);
            var args = new BackchannelEventArgs(bcintensity);
            foreach (EventHandler<BackchannelEventArgs> receiver in BackchannelEvent.GetInvocationList()) {
                receiver.BeginInvoke(this, args, null, null);
            }
        }

        private DateTime previousHeadBehaviour = DateTime.Now;
        private void HeadNodEventAux() {
            DateTime now = DateTime.Now;

            //minimum time between head behaviours
            if((now - previousHeadBehaviour).TotalMilliseconds > 3500) {
                var args = new EventArgs();
                foreach (EventHandler receiver in HeadNodEvent.GetInvocationList()) {
                    receiver.BeginInvoke(this, args, null, null);
                }
                previousHeadBehaviour = now;
            }
        }

        private void HeadShakeEventAux() {
            DateTime now = DateTime.Now;

            //minimum time between head behaviours
            if ((now - previousHeadBehaviour).TotalMilliseconds > 3500) {
                var args = new EventArgs();
                foreach (EventHandler receiver in HeadShakeEvent.GetInvocationList())
                    receiver.BeginInvoke(this, args, null, null);
                previousHeadBehaviour = now;
            }
        }

        private void HappyEventAux(double intensity) {
            //Plugin.LogDebug("HAPPY EVENT");
            var args = new EmotionEventArgs(intensity);
            foreach (EventHandler<EmotionEventArgs> receiver in HappyEvent.GetInvocationList())
                receiver.BeginInvoke(this, args, null, null);
        }

        private void SurprisedEventAux(double intensity) {
            //Plugin.LogDebug("SURPRISE EVENT");
            var args = new EmotionEventArgs(intensity);
            foreach (EventHandler<EmotionEventArgs> receiver in SurpriseEvent.GetInvocationList())
                receiver.BeginInvoke(this, args, null, null);
        }

        private void SadEventAux(double intensity) {
            //Plugin.LogDebug("SAD EVENT");
            var args = new EmotionEventArgs(intensity);
            foreach (EventHandler<EmotionEventArgs> receiver in SadEvent.GetInvocationList())
                receiver.BeginInvoke(this, args, null, null);
        }
    }
}
