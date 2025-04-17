using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
        private List<Node> _ancestors = new List<Node>();

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
            treeView1.DrawMode = TreeViewDrawMode.OwnerDrawText;
            treeView1.DrawNode += treeview_ancestor;

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

            visited.Add(model.NodeId);
            string text = string.IsNullOrEmpty(model.NodeName) ? $"Node {model.NodeId}" : model.NodeName;
            TreeNode treeNode = new TreeNode(text) { Tag = model };

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
                listBox1.Items.Insert(0, model.NodeName + ":" + model.NodeId.ToString());
            }
        }

        private void listbox_select(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string node_select = listBox1.SelectedItem.ToString();
                var split = node_select.Split(':');
                int node_id = int.Parse(split[1]);
                var nodes = new List<Node>();
                _controller.GetAncestor(node_id, nodes);
                _ancestors = nodes;
                treeView1.Invalidate();
                var node = _controller.GetNodeById(node_id);
                if (node != null)
                {
                    string info = $"ID: {node.NodeId}\r\n" +
                                  $"Nom: {node.NodeName}\r\n" +
                                  $"Extinct? {node.IsExtinct}\r\n" +
                                  $"Confidence: {node.Confidence}\r\n" +
                                  $"Phylesis: {node.Phylesis}\r\n";
                    labelInfo.Text = info;
                }
            }


        }

        private void treeview_ancestor(object sender, DrawTreeNodeEventArgs e)
        {
            if (e.Node.Tag is Node node)
            {
                Font nodeFont = e.Node.NodeFont ?? e.Node.TreeView.Font;
                Color foreColor = e.Node.ForeColor;
                Rectangle nodeBounds = e.Bounds;

                if (_ancestors.Any(n => n.NodeId == node.NodeId))
                {
                    using (SolidBrush brush = new SolidBrush(Color.Yellow))
                    {
                        e.Graphics.FillRectangle(brush, nodeBounds);
                    }
                }
                else
                {
                    e.Graphics.FillRectangle(SystemBrushes.Window, nodeBounds);
                }

                TextRenderer.DrawText(e.Graphics, e.Node.Text, nodeFont, nodeBounds, foreColor, TextFormatFlags.GlyphOverhangPadding);
            }
            else
            {
                e.DrawDefault = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _ancestors.Clear();
            treeView1.Invalidate();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox tb = sender as TextBox;
            var node = new Node();
            List<Node> nodes = new List<Node>();
            if (e.KeyCode == Keys.Enter)
            {
                if (tb != null)
                {
                    if (_controller.GetNodeByName(tb.Text) != null)
                    {

                        node = _controller.GetNodeByName(tb.Text);

                        _controller.GetAncestor(node.NodeId, nodes);
                        _ancestors = nodes;
                        treeView1.Invalidate();

                    }
                    else if (_controller.GetNodeById(int.Parse(tb.Text)) != null)
                    {

                        node = _controller.GetNodeById(int.Parse(tb.Text));
                        _controller.GetAncestor(node.NodeId, nodes);
                        _ancestors = nodes;
                        treeView1.Invalidate();
                    }
                    else
                    {
                        return;
                    }

                }


            }

        }
    }
}
