using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TreeOfLife.Models;
using TreeOfLifeVisualization.Controllers;
using TreeOfLifeVisualization.Models;

namespace TreeOfLifeVisualization.Views
{
    public partial class View : Form
    {
        private Controller _controller;
        private NodeReduit NHistoric;  

        public View()
        {
            InitializeComponent();
        }

        private void MainView_Load(object sender, EventArgs e)
        {
           
            NHistoric = new NodeReduit();

            
            string nodesCsvPath = @"C:\Users\antho\Desktop\2024-2025\Programmation_Interface\2025-projet-G1\treeoflife_nodes_simplified.csv";
            string linksCsvPath = @"C:\Users\antho\Desktop\2024-2025\Programmation_Interface\2025-projet-G1\treeoflife_links_simplified.csv";

            _controller = new Controller(nodesCsvPath, linksCsvPath);

            BuildTreeView();
        }

        private void BuildTreeView()
        {
            treeView1.Nodes.Clear();

            foreach (var rootNodeModel in _controller.Racine)
            {
                var rootTreeNode = CreateTreeNode(rootNodeModel);
                treeView1.Nodes.Add(rootTreeNode);
            }
        }

        private TreeNode CreateTreeNode(Node model, HashSet<int> visited = null)
        {
            if (visited == null)
                visited = new HashSet<int>();

            if (visited.Contains(model.NodeId))
            {
                return new TreeNode($"[Cycle détecté: {model.NodeName}]");
            }

            visited.Add(model.NodeId);

            string text = string.IsNullOrEmpty(model.NodeName)
                            ? $"Node {model.NodeId}"
                            : model.NodeName;

            TreeNode treeNode = new TreeNode(text)
            {
                Tag = model 
            };

            foreach (var child in model.Children)
            {
                treeNode.Nodes.Add(CreateTreeNode(child, visited));
            }

            return treeNode;
        }

        private void treeView1_Select(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null && e.Node.Tag is Node model)
            {
                string info = $"ID: {model.NodeId}\r\n" +
                              $"Nom: {model.NodeName}\r\n" +
                              $"Extinct? {model.IsExtinct}\r\n" +
                              $"Confidence: {model.Confidence}\r\n" +
                              $"Phylesis: {model.Phylesis}\r\n";
                labelInfo.Text = info;

    
                NHistoric.Add(model);

         
                listBox1.Items.Insert(0,model.NodeName + ":" + model.NodeId.ToString());
            }
        }
    }
}
