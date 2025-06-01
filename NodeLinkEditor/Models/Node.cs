using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NodeLinkEditor.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NodeAttribute
    {
        None,
        Start,
        End,
        Middle,
        Intersection,
    }
    public class Node
    {
        public Guid ID { get; init; } = Guid.CreateVersion7();
        public required double X { get; set; }
        public required double Y { get; set; }
        public List<Guid> AssociatedNodes { get; set; } = [];
        public List<NodeAttribute> Attributes { get; set; } = [];
    }
}
