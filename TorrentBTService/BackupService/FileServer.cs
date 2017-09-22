using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TorrentBTService
{
    struct StateThread
    {
        public bool WaitMessageDispacher;
        public bool StartMessageDispacher;
    }

    public class FileServer : IDisposable
    {
        private int port;
        private int listen;

        IPEndPoint endPoint;
        TcpListener sSocket;

        private StateThread StateObject;
        private TorrentManage Torrent;
        private Thread WorkSocketThread;


        public FileServer(int port = 8800,int listen = 1)
        {
            this.port = port;
            this.listen = listen;

            endPoint = new IPEndPoint(IPAddress.Any, port);
            Torrent = new TorrentManage();
        }

        public void Start()
        {
            sSocket = new TcpListener(endPoint);
            sSocket.Start(listen);

            StateObject.StartMessageDispacher = true;
            StateObject.WaitMessageDispacher = false;

            WorkSocketThread = new Thread(delegate () {
                while(StateObject.StartMessageDispacher)
                {
                    Thread.Sleep(1);

                    TcpClient Handler = sSocket.AcceptTcpClient();

                    while (StateObject.WaitMessageDispacher) { Thread.Sleep(1); }

                    Thread AceptThread = new Thread(delegate () {
                        AcceptDispather(Handler);
                    });

                    AceptThread.Start();
                }
            });
        }

        private void AcceptDispather(TcpClient Handler)
        {
            using (NetworkStream stream = Handler.GetStream())
            {
                string Torrentpath = AppDomain.CurrentDomain.BaseDirectory + "\\Torrents\\" + GetNowTime() + ".torrent";
                string Savepath = AppDomain.CurrentDomain.BaseDirectory + "\\Files\\";

                using (FileStream output = File.Create(Torrentpath))
                {
                    var buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, bytesRead);
                    }
                }

                Handler.Close();
                Torrent.addTorrent(Torrentpath, Savepath);
            }
        }
        public void Resume()
        {
            StateObject.WaitMessageDispacher = false;
        }

        public void Stop()
        {
            StateObject.WaitMessageDispacher = true;
        }

        public string GetNowTime()
        {
            return DateTime.Now.Day.ToString() + "d" + DateTime.Now.Hour.ToString() + "h" + DateTime.Now.Minute.ToString() + "m" + DateTime.Now.Second.ToString() + "s";
        }

        public void Dispose()
        {
            WorkSocketThread.Abort();
            Torrent.Dispose();
            sSocket.Stop();
        }
    }
}
