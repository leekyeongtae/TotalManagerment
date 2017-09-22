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
        }
    }
}
