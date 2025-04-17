using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.VisualBasic.ApplicationServices;

namespace TreeOfLifeVisualization.Models
{
    public class Data
    {
        public List<Node> Nodes { get; private set; }
        public List<Link> Links { get; private set; }
        private readonly string _nodesFilePath;
        private readonly string _linksFilePath;

        public Data(string nodesFilePath, string linksFilePath)
        {
            _nodesFilePath = nodesFilePath;
            _linksFilePath = linksFilePath;
            LoadNodes();
            LoadLinks();
            BuildHierarchy();
        }

        private void LoadNodes()
        {
            var lines = File.ReadAllLines(_nodesFilePath);
            Nodes = new List<Node>();

            for (int i = 1; i < lines.Length; i++)
            {
                var fields = lines[i].Split(',');

                var node = new Node
                {
                    NodeId = int.Parse(fields[0]),
                    NodeName = fields[1] == "none" ? "" : fields[1],
                    ChildNodesCount = int.Parse(fields[2]),
                    IsLeaf = fields[3] == "1",
                    HasTolOrgLink = fields[4] == "1",
                    IsExtinct = fields[5] == "1",
                    Confidence = int.Parse(fields[6]),
                    Phylesis = int.Parse(fields[7])
                };

                Nodes.Add(node);
            }
        }


        private void LoadLinks()
        {
            var lines = File.ReadAllLines(_linksFilePath);
            Links = new List<Link>();

            for (int i = 1; i < lines.Length; i++)
            {
                var fields = lines[i].Split(',');

                var link = new Link
                {
                    SourceNodeId = int.Parse(fields[0]),
                    TargetNodeId = int.Parse(fields[1]),
                };

                Links.Add(link);
            }
        }

        private void BuildHierarchy()
        {
            var nodeDict = Nodes.ToDictionary(n => n.NodeId);

            foreach (var link in Links)
            {
                if (nodeDict.ContainsKey(link.SourceNodeId) && nodeDict.ContainsKey(link.TargetNodeId))
                {
                    var parentNode = nodeDict[link.SourceNodeId];
                    var childNode = nodeDict[link.TargetNodeId];
                    childNode.Parent = parentNode;
                    parentNode.Children.Add(childNode);
                }
            }
        }
    }
}
