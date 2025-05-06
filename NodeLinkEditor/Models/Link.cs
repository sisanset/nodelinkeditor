using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeLinkEditor.Models
{
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
    }
}
