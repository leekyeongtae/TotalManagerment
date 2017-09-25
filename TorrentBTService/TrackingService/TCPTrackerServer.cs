using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TorrentBTService
{
    public class PeerMetaData
    {
        public int Idx;
        public string uri;
        public long uploadSpeed;
        public TcpClient handler;
    }

    public class TCPTrackerServer : IDisposable
    {
        private int port;
        private int listen;

        IPEndPoint endPoint;
        TcpListener sSocket;

        private UDPTrackerQuery Query;
        private StateThread StateObject;
        private Thread WorkSocketThread;

        public List<PeerMetaData> PeerTable;


        public TCPTrackerServer(int port = 8900, int listen = 1)
        {
            this.port = port;
            this.listen = listen;

            endPoint = new IPEndPoint(IPAddress.Any, port);
            PeerTable = new List<PeerMetaData>();
            Query = new UDPTrackerQuery(PeerTable);
        }

        public void Start()
        {
            sSocket = new TcpListener(endPoint);
            sSocket.Start(listen);
            Query.Start();

            StateObject.StartMessageDispacher = true;
            StateObject.WaitMessageDispacher = false;

            WorkSocketThread = new Thread(delegate ()
            {
                while (StateObject.StartMessageDispacher)
                {
                    Thread.Sleep(1);

                    PeerMetaData MetaData = new PeerMetaData();

                    MetaData.handler = sSocket.AcceptTcpClient();
                    MetaData.Idx = PeerTable.Count;
                    MetaData.uri = MetaData.handler.Client.RemoteEndPoint.ToString().Split(':')[0];
                    PeerTable.Add(MetaData);

                    while (StateObject.WaitMessageDispacher) { Thread.Sleep(1); }

                    Thread AceptThread = new Thread(delegate ()
                    {
                        AcceptDispather(MetaData);
                    });

                    AceptThread.Start();
                }
            });

            WorkSocketThread.Start();
        }

        private void AcceptDispather(PeerMetaData ConnObject)
        {
            using (NetworkStream stream = ConnObject.handler.GetStream())
            {
                byte[] buffer = new byte[128];
                int ReceiveBytes = 0;
                long TickTime = 0;

                while (ConnObject.handler.Connected)
                {
                    Thread.Sleep(1);
                    try
                    {
                        if (TickTime == 1000)
                        {
                            stream.Write(new byte[] { 0x01, 0x01, 0x0A }, 0, 3);
                            do
                            {
                                ReceiveBytes = stream.Read(buffer, 0, buffer.Length);
                            }
                            while (stream.DataAvailable);

                            ConnObject.uploadSpeed = BitConverter.ToInt64(buffer, 0);

                            Array.Clear(buffer, 0, buffer.Length);
                            TickTime = 0;
                        }
                    }
                    catch
                    {
                        break;
                    }
                    TickTime++;
                }
            }

            ConnObject.handler.Close();
            PeerTable.RemoveAt(ConnObject.Idx);

        }
        public void Resume()
        {
            StateObject.WaitMessageDispacher = false;
        }

        public void Stop()
        {
            StateObject.WaitMessageDispacher = true;
        }

        public void Dispose()
        {
            WorkSocketThread.Abort();
            sSocket.Stop();
            Query.Dispose();
        }
    }
}
