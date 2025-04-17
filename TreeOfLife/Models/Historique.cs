using System;
using System.Collections.Generic;
using TreeOfLifeVisualization.Models;

namespace TreeOfLife.Models
{
    public class NodeReduit : Node
    {
        public new int NodeId { get; set; }
        public new string NodeName { get; set; }
        public List<NodeReduit> Historic { get; set; }

        public NodeReduit()
        {
            Historic = new List<NodeReduit>();
        }

        public void Add(Node node)
        {
            try
            {
                NodeReduit nodeR = new NodeReduit
                {
                    NodeId = node.NodeId,
                    NodeName = node.NodeName
                };
                Historic.Insert(0,nodeR);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erreur lors de l'ajout dans l'historique : " + ex.Message);
            }
        }
    }
}
