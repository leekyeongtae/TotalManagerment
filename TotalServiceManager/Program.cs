using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorrentBTService;

namespace TotalServiceManager
{
    class Program
    {
        static void Main(string[] args)
        {
            FileServer _FILE_BAK_SERVICE_ = new FileServer();
            TCPTrackerServer _PEER_CTL_SERVICE_ = new TCPTrackerServer();

            _FILE_BAK_SERVICE_.Start();
            _PEER_CTL_SERVICE_.Start();

            System.Threading.Thread MainQueue = new System.Threading.Thread(delegate () {
                while (true)
                {
                    Console.Clear();

                    Console.WriteLine("=================================================================================\nSeeding\t\tDownload\t\tStoped\t\tRecived\t\tUploading");
                    Console.WriteLine("{0}\t\t{1}\t\t\t{2}\t\t{3}\t\t{4}", _FILE_BAK_SERVICE_.Torrent.TotalSeed, _FILE_BAK_SERVICE_.Torrent.TotalDownload, _FILE_BAK_SERVICE_.Torrent.TotalStop, _FILE_BAK_SERVICE_.Torrent.TotalNames.Count(), _FILE_BAK_SERVICE_.Torrent.TotalUpload);
                    Console.WriteLine("=================================================================================\n\n\nConnected Users\t\tMaxUpSpeed(kbps)");

                    foreach (PeerMetaData pmd in _PEER_CTL_SERVICE_.PeerTable) {
                        Console.WriteLine("{0}\t\t{1}", pmd.uri, pmd.uploadSpeed);
                    }
                    System.Threading.Thread.Sleep(1000);
                }
            });
            MainQueue.Start();
        }
    }
}
