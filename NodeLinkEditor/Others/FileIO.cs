using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using Newtonsoft.Json;
using NodeLinkEditor.Models;
using NodeLinkEditor.ViewModels;
using System.Text;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace NodeLinkEditor.Others
{
    public class FileIO
    {
        class JsonDate
        {
            public string YamlFileName { get; set; } = string.Empty;
            public required List<Node> Nodes { get; set; }
            public required List<Link> Links { get; set; }
            public required List<HelperLine> Lines { get; set; }
        }
        public class MapYaml
        {
            public required string Image { get; set; }
            public required double Resolution { get; set; }
            public required double[] Origin { get; set; } = new double[3];
        }
        // Node/Link/対応するmapのyamlファイルパスを保存する
        public static void SaveNodeLinkToJson(string filePath, MapData mapData, ObservableCollection<NodeViewModel> nodes, ObservableCollection<LinkViewModel> links, ObservableCollection<HelperLineViewModel> lines)
        {
            var data = new JsonDate
            {
                YamlFileName = mapData.YamlFilePath,
                Nodes = [.. nodes.Select(n => n.GetNodeCopy())],
                Links = [.. links.Select(l => l.GetLinkCopy())],
                Lines = [.. lines.Select(l => l.GetHelperLineCopy())],
            };
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
        // Node/Linkと対応するmapを読み込む
        public static (MapData, ObservableCollection<NodeViewModel>, ObservableCollection<LinkViewModel>, ObservableCollection<HelperLineViewModel>) LoadNodeLinkFromJson(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<JsonDate>(json);
            if (data == null)
            {
                throw new Exception("Failed to deserialize JSON data.");
            }
            var nodes = new ObservableCollection<NodeViewModel>();
            data.Nodes.ForEach(n =>
            {
                var node = new NodeViewModel(n);
                nodes.Add(node);
            });
            var links = new ObservableCollection<LinkViewModel>();
            foreach (var l in data.Links)
            {
                var startNode = nodes.FirstOrDefault(n => n.ID == l.StartNodeID);
                var endNode = nodes.FirstOrDefault(n => n.ID == l.EndNodeID);
                if (startNode == null || endNode == null)
                {
                    throw new Exception($"Link {l.ID} has invalid node references.");
                }
                var link = new LinkViewModel(l, startNode, endNode);
                links.Add(link);
            }
            var lines = new ObservableCollection<HelperLineViewModel>();
            foreach (var l in data.Lines)
            { lines.Add(new HelperLineViewModel(l)); }
            if (data.YamlFileName == null || data.YamlFileName == string.Empty)
            {
                return (new MapData(), nodes, links, lines);
            }
            var mapData = LoadMapFromYaml(data.YamlFileName);
            return (mapData, nodes, links, lines);
        }

        // mapのyamlからmapのpgmファイルを読み込む
        public static MapData LoadMapFromYaml(string filePath)
        {
            var mapYaml = LoadMapYaml(filePath);
            var pgmFile = Path.Combine(Path.GetDirectoryName(filePath)!, mapYaml.Image);
            (var width, var height, var bitMapSource) = LoadMapFromPgm(pgmFile);
            return new MapData
            {
                YamlFilePath = filePath,
                MapImage = bitMapSource,
                Width = width,
                Height = height,
                Resolution = mapYaml.Resolution,
                Origin = mapYaml.Origin,
            };
        }
        // mapのpgmファイルを読み込む
        private static (int, int, BitmapSource) LoadMapFromPgm(string filePath)
        {
            (var width, var height, var pixels) = PgmImage.LoadPgmImage(filePath);
            var pf = PixelFormats.Gray8;
            int rawStride = (width * pf.BitsPerPixel + 7) / 8;
            var MapImage = BitmapSource.Create(width, height, 96, 96, PixelFormats.Gray8, null, pixels, rawStride);
            return (width, height, MapImage);
        }
        // mapのyamlファイルを読み込む
        private static MapYaml LoadMapYaml(string filePath)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
            return deserializer.Deserialize<MapYaml>(File.ReadAllText(filePath, Encoding.UTF8));
        }

        public class SettingsYaml
        {
            public double NodeInterval { get; set; } = 3;
            public double IntersectionInterval { get; set; } = 1;
            public string MqttBroker { get; set; } = "localhost";
            public int MqttPort { get; set; } = 1883;
        }
        public static void SaveSettingsYaml(string fileName, SettingsYaml settingsParam)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreFields()
                .Build();
            using var writer = new StreamWriter(fileName, false, Encoding.UTF8);
            serializer.Serialize(writer, settingsParam);
        }
        public static SettingsYaml LoadSettingsYaml(string filePath)
        {
            var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
            try
            {
                return deserializer.Deserialize<SettingsYaml>(File.ReadAllText(filePath, Encoding.UTF8));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading settings file: {ex.Message}");
                return new SettingsYaml();
            }

        }
    }
}
