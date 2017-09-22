using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace TorrentBTService
{
    internal class UDPTrackerQuery : IDisposable
    {
        private int port;
        private int listen;

        IPEndPoint endPoint;
        EndPoint remoteEP;
        Socket sSocket;

        private List<PeerMetaData> PeerTable;
        private StateThread StateObject;
        private Thread WorkSocketThread;

        internal UDPTrackerQuery(List<PeerMetaData> PeerTable, int port = 8900, int listen = 1)
        {
            this.port = port;
            this.listen = listen;

            this.endPoint = new IPEndPoint(IPAddress.Any, port);
            this.remoteEP = (EndPoint)this.endPoint;
            this.PeerTable = PeerTable;
        }

        public void Start()
        {
            sSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sSocket.Bind(endPoint);

            int ReceiveBytes = 0;
            byte[] buffer = new byte[4096];

            StateObject.StartMessageDispacher = true;
            StateObject.WaitMessageDispacher = false;

            WorkSocketThread = new Thread(delegate () {

                StringBuilder sb = new StringBuilder();

                while (StateObject.StartMessageDispacher)
                {
                    Thread.Sleep(1);

                    while (StateObject.WaitMessageDispacher) { Thread.Sleep(1); }

                    ReceiveBytes = sSocket.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref remoteEP);
                    if (ReceiveBytes > 0)
                    {
                        sb.Append(Encoding.ASCII.GetString(buffer, 0, ReceiveBytes));

                        if(sb.ToString().IndexOf("<START>") != -1)
                        {
                            if(sb.ToString().IndexOf("<END>") != -1)
                            {
                                List<string> Parser = new List<string>();
                                foreach (string x in sb.Replace("<START>", "").Replace("<END>", "").ToString().Split("\r\n".ToArray(), StringSplitOptions.RemoveEmptyEntries))
                                {
                                    // 정렬 알고리즘
                                    Parser.Add(x);
                                }
                            }
                        }
                    }
                }
            });
            WorkSocketThread.Start();
        }

        public void Dispose()
        {
            sSocket.Close();
        }
    }
}
