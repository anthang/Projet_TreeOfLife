using System;
using System.Collections.Generic;

namespace TreeOfLifeVisualization.Models
{
    public class Node
    {
        public int NodeId { get; set; }
        public string NodeName { get; set; }
        public int ChildNodesCount { get; set; }
        public bool IsLeaf { get; set; }
        public bool HasTolOrgLink { get; set; }
        public bool IsExtinct { get; set; }
        public int Confidence { get; set; }
        public int Phylesis { get; set; }

        public Node Parent { get; set; }
        public List<Node> Children { get; set; } = new List<Node>();
    }
}
