using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLinkEditor.Models
{
    public enum NodeAttribute
    {
        None,
        Start,
        End,
        Middle
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
