using denolk.DeFinder.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace denolk.DeFinder.Models
{
    [Serializable]
    internal class SearchModel
    {
        public string Path { get; set; }
        public List<string> Filenames { get; set; }

        public SearchModel(string path)
        {
            Path = path;
        }

        public void Populate(bool rebuild)
        {
            Filenames = IndexedResultHelper.GetContents(Path,rebuild);
        }

        public byte[] ToBytes()
        {
            byte[] bytes;
            var format = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                format.Serialize(stream, this);
                bytes = stream.ToArray();
            }
            return bytes;
        }

        public Dictionary<string, List<string>> ToDictionary()
        {
            var dictionary = new Dictionary<string, List<string>>();
            dictionary[this.Path] = this.Filenames;
            return dictionary;
        }

    }
}
