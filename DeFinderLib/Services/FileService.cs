using denolk.DeFinder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace denolk.DeFinder.Services
{
    public class FileService
    {
        private List<SearchModel> _searches { get; set; }
        private List<string> _fileList { get; set; }
        private Stopwatch _stopwatch { get; set; }

        public FileService()
        {
            _searches = new List<SearchModel>();
            _fileList = new List<string>();
            _stopwatch = new Stopwatch();
        }

        public async Task<List<string>> GetFilesAsync(List<string> paths, bool rebuild)
        {
            return await Task.Run(() =>
            {
                return GetFiles(paths, rebuild);
            }, CancellationToken.None);
        }

        public List<string> GetFiles(List<string> paths, bool rebuild)
        {
            _stopwatch.Restart();
            _searches = new List<SearchModel>();
            foreach (var p in paths)
            {
                var search = new SearchModel(p);
                search.Populate(rebuild);
                _searches.Add(search);
            }

            _fileList = new List<string>();
            if (_searches != null)
            {
                foreach (var s in _searches)
                {
                    _fileList.AddRange(s.Filenames);
                }
            }
            _stopwatch.Stop();
            return _fileList;
        }

        public List<string> Filter(string keyword)
        {
            keyword = keyword.ToLowerInvariant();
            var list = new List<string>();
            if (keyword.Length < 3)
            {
                list = _fileList;
            }
            else if (keyword.Contains("*."))
            {
                keyword = keyword.Replace("*", "");
                list = _fileList.Where(f => f.ToLowerInvariant().EndsWith(keyword)).ToList();
            }
            else
            {
                list = _fileList.Where(f => f.ToLowerInvariant().Contains(keyword)).ToList();
            }
            return list;
        }

        public void Open(List<string> filenames, bool isDirectory, bool openWith)
        {
            foreach (var f in filenames)
            {
                if (openWith)
                {
                    string command = string.Format("shell32.dll, OpenAs_RunDLL {0}", f);
                    Process.Start("rundll32.exe", command);
                }
                else
                {
                    var target = f;
                    if (isDirectory)
                    {
                        target = Path.GetDirectoryName(f);
                    }
                    Process.Start(target);
                }
            }
        }

        public double GetElapsedSeconds()
        {
            return _stopwatch.Elapsed.TotalSeconds;
        }

        public string GetPerformanceSummary()
        {
            var summary = string.Format("{0} results in {1:0.00} seconds", _fileList.Count, GetElapsedSeconds());
            return summary;
        }

    }
}
