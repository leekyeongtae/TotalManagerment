using MonoTorrent.Common;
using MonoTorrent.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TorrentBTService
{
    internal class TorrentManage : IDisposable
    {
        private List<TorrentFile> torrentFiles = new List<TorrentFile>();

        public List<string> TotalNames 
        {
            get
            {
                List<string> value = new List<string>();

                foreach(TorrentFile tF in torrentFiles)
                {
                    value.Add(tF.name);
                }

                return value;
            }
        }

        public void addTorrent(string TorrentPath, string SavePath)
        {
            torrentFiles.Add(new TorrentFile(TorrentPath, SavePath, new IPEndPoint(IPAddress.Any, 6969)));
            torrentFiles[torrentFiles.Count - 1].Start();
        }

        public void delTorrent(int idx)
        {
            if (idx >= 0 && idx < torrentFiles.Count)
                torrentFiles[idx].Dispose();
        }

        public void Dispose()
        {
            foreach(TorrentFile tF in torrentFiles)
            {
                tF.Stop();

                while (tF.torrentState != TorrentState.Stopped) {  }

                tF.Dispose();
            }
        }
    }
}
