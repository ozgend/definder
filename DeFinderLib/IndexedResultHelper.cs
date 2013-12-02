using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace denolk.DeFinder
{

    internal static class IndexedResultHelper
    {

        internal static List<string> GetContents(string directory, bool rebuild)
        {
            var data = GetIndexedResults();
            if (data != null && data.ContainsKey(directory) && !rebuild)
            {
                return data[directory];
            }
            else
            {
                var list = GetDirectoryContents(directory);
                SetIndexedResults(data, directory, list);
                return list;
            }
        }

        private static void SetIndexedResults(Dictionary<string, List<string>> data, string directory, List<string> list)
        {
            data[directory] = list;
            try
            {
                var format = new BinaryFormatter();
                using (var stream = new MemoryStream())
                {
                    format.Serialize(stream, data);
                    var bytes = stream.ToArray();
                    File.WriteAllBytes(Strings.FILEPATH, bytes);
                }
            }
            catch { }
        }

        private static Dictionary<string, List<string>> GetIndexedResults()
        {
            var data = new Dictionary<string, List<string>>();
            try
            {
                using (var stream = new FileStream(Strings.FILEPATH, FileMode.Open))
                {
                    var formatter = new BinaryFormatter();
                    data = formatter.Deserialize(stream) as Dictionary<string, List<string>>;
                }
            }
            catch { }
            return data;
        }

        private static List<string> GetDirectoryContents(string directory)
        {
            var list = new List<string>();
            try
            {
                list = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories).ToList();
            }
            catch { }
            return list;
        }

    }
}
