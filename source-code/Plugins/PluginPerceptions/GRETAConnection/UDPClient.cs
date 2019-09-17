using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GRETAConnection {
    interface IUDPMessageReader {
        void Read(string msg);
    }

    class UDPClient : IDisposable {
        private IUDPMessageReader udpReader;
        private Thread oThread;

        private UdpClient udpClient = null;

        private IPEndPoint remoteIPEndPoint;
        private bool shutdownFlag = false;

        public UDPClient(IUDPMessageReader udpMesssageReader, string serverIp, int serverPort) {
            this.udpReader = udpMesssageReader;
            remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(serverIp.ToLower() == "localhost" ? "127.0.0.1" : serverIp), serverPort);
        }

        public void Stop() {
            Console.WriteLine("Stopping");
            Dispose();
        }

        public void Start() {
            udpClient = new UdpClient();
            Console.WriteLine("Establishing connection with the server... ");
            byte[] sendData = Encoding.ASCII.GetBytes("CONN");
            udpClient.Send(sendData, sendData.Length, remoteIPEndPoint);

            //start receiving packets
            shutdownFlag = false;
            oThread = new Thread(new ThreadStart(StartMonitor));
            oThread.Start();
        }

        private void StartMonitor() {
            while (!shutdownFlag) {
                try {
                    var receivedData = udpClient.Receive(ref remoteIPEndPoint);
                    udpReader.Read(Encoding.ASCII.GetString(receivedData));
                }
                catch (Exception) { /* intentionally ignored */ }
            }

            Console.WriteLine("Shutting down");
        }

        public void Dispose() {
            shutdownFlag = true;
            udpClient.Close();
        }
    }
}
