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
    public class TorrentManage : IDisposable
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
        public int TotalUpload
        {
            get
            {
                int value = 0;

                foreach (TorrentFile tF in torrentFiles)
                {
                    if(tF.uploadSpeed > 0)
                        value++;
                }

                return value;
            }
        }


        public int TotalSeed
        {
            get
            {
                int ret = 0;
                foreach(TorrentFile tF in torrentFiles)
                {
                    if(tF.torrentState == TorrentState.Seeding)
                    {
                        ret++;
                    }
                }
                return ret;
            }
        }

        public int TotalStop
        {
            get
            {
                int ret = 0;
                foreach (TorrentFile tF in torrentFiles)
                {
                    if (tF.torrentState == TorrentState.Stopped)
                    {
                        ret++;
                    }
                }
                return ret;
            }
        }

        public int TotalDownload
        {
            get
            {
                int ret = 0;
                foreach (TorrentFile tF in torrentFiles)
                {
                    if (tF.torrentState == TorrentState.Downloading)
                    {
                        ret++;
                    }
                }
                return ret;
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

        public List<int> IsDelay()
        {
            List<int> idx = new List<int>();
            for(int i = 0; i< torrentFiles.Count; i++)
            {
                if(torrentFiles[i].manager.Peers.Seeds + torrentFiles[i].manager.Peers.Leechs == 0 && torrentFiles[i].torrentState != TorrentState.Seeding)
                {
                    idx.Add(i);
                }
            }
            return idx;
        }

        public void RestartAll()
        {
            foreach (TorrentFile tF in torrentFiles)
            {
                tF.Stop();
                tF.Start();
            }
        }

        public void StopTorrent(int idx)
        {
            if (idx >= 0 && idx < torrentFiles.Count)
                torrentFiles[idx].Stop();
        }

        public void ResumeTorrent(int idx)
        {
            if (idx >= 0 && idx < torrentFiles.Count)
                torrentFiles[idx].Start();
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
