using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NodeLinkEditor.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum LinkAttribute
    {
        None,
        Start,
        End,
        Middle
    }
    public class Link
    {
        public Guid ID { get; init; } = Guid.CreateVersion7();
        public Guid StartNodeID { get; set; }
        public Guid EndNodeID { get; set; }
        public List<LinkAttribute> Attributes { get; set; } = [];
        public double StartToEndCost { get; set; } = 1.0;
        public double EndToStartCost { get; set; } = 1.0;
        public bool IsTwoWay { get; set; } = false;
    }
}
