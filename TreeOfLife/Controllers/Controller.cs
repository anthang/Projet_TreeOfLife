using System;
using System.Collections.Generic;
using System.Linq;
using TreeOfLifeVisualization.Models;

namespace TreeOfLifeVisualization.Controllers
{
    public class Controller
    {
        private readonly Data _data;

        public List<Node> Racine { get; private set; }

        public Controller(string nodesPath, string linksPath)
        {
            _data = new Data(nodesPath, linksPath);
            Racine = _data.Nodes.Where(n => n.NodeId == 1).ToList();
        }

        public Node GetNodeById(int id)
        {
            try
            {
                return _data.Nodes.FirstOrDefault(n => n.NodeId == id);
            }
            catch (Exception)
            {
                
                Console.WriteLine($"ID invalid");
                return null;
            }
        }

        public Node GetNodeByName(string name)
        {
            try
            {
                return _data.Nodes.FirstOrDefault(n => n.NodeName == name);
            }
            catch (Exception)
            {
                Console.WriteLine($"nom invalid");
                return null;

            }
        }
    }
}
