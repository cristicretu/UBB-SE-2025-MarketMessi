using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marketplace_SE.Service
{
    public class Node()
    {
        public int? Id { get; set; } // Nullable
        // Button Text
        public required string ButtonLabel { get; set; }
        // Label Text
        public required string LabelText { get; set; }
        // Response Text
        public required string Response { get; set; }
        // Children of current node
        public List<Node> Children { get; set; } = new List<Node>();
    }
}