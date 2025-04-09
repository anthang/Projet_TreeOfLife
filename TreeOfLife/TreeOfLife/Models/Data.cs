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
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };
            using var reader = new StreamReader(_nodesFilePath);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<dynamic>();
            Nodes = new List<Node>();

            foreach (var record in records)
            {
                var node = new Node
                {
                    NodeId = int.Parse(record.node_id),
                    NodeName = record.node_name == "none" ? "" : (string)record.node_name,
                    ChildNodesCount = int.Parse(record.child_nodes),
                    IsLeaf = record.leaf_node == "1",
                    HasTolOrgLink = record.tolorg_link == "1",
                    IsExtinct = record.extinct == "1",
                    Confidence = int.Parse(record.confidence),
                    Phylesis = int.Parse(record.phylesis)
                };

                Nodes.Add(node);
            }
        }

        private void LoadLinks()
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true };
            using var reader = new StreamReader(_linksFilePath);
            using var csv = new CsvReader(reader, config);
            var records = csv.GetRecords<dynamic>();
            Links = new List<Link>();

            foreach (var record in records)
            {
                var link = new Link
                {
                    SourceNodeId = int.Parse(record.source_node_id),
                    TargetNodeId = int.Parse(record.target_node_id)
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
