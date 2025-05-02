using System;
using System.Linq;
using System.Windows.Forms;
using GraphApp.Controller;
using GraphApp.Model;

namespace GraphApp
{
    public class MainForm : Form
    {
        private readonly Graph _graph;
        private readonly GraphView _view;
        private readonly PhysicsController _physics;

        private readonly TextBox _searchBox;
        private readonly Button _searchBtn;

        private readonly GroupBox _shapePanel;
        private readonly GroupBox _optionsPanel;
        private readonly ListBox _historyList;

        private readonly Label _id, _name, _link, _extinct, _conf, _phy;

        public MainForm()
        {
            Text = "Tree Of Life - WinForms";
            Width = 1200; Height = 800; DoubleBuffered = true;

            // -------- mod�le & vue --------
            _graph = GraphLoader.LoadFromCsv("Data/treeoflife_nodes_simplified.csv", "Data/treeoflife_links_simplified.csv");
            _view = new GraphView(_graph) { Dock = DockStyle.Fill };
            Controls.Add(_view);

            // -------- Historique --------
            var histBox = new GroupBox { Text = "Historique", Dock = DockStyle.Left, Width = 180, Padding = new Padding(5) };
            _historyList = new ListBox { Dock = DockStyle.Fill };
            histBox.Controls.Add(_historyList); Controls.Add(histBox);
            _historyList.SelectedIndexChanged += HistorySelected;

            // -------- Recherche --------
            var top = new Panel { Dock = DockStyle.Top, Height = 35, Padding = new Padding(5) };
            _searchBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "ID ou titre�" };
            _searchBtn = new Button { Text = "Go", Dock = DockStyle.Right, Width = 60 };
            top.Controls.AddRange(new Control[] { _searchBox, _searchBtn });
            Controls.Add(top);
            _searchBtn.Click += (_, _) => Search();
            _searchBox.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { Search(); e.Handled = true; } };

            // -------- Colonne droite --------
            var right = new Panel { Dock = DockStyle.Right, Width = 280, Padding = new Padding(5) };
            Controls.Add(right);

            // --- Formes ---
            _shapePanel = new GroupBox { Text = "Formes", Dock = DockStyle.Top, Height = 170, Padding = new Padding(5) };
            right.Controls.Add(_shapePanel);
            AddShape("Cercle", NodeShape.Circle, true);
            AddShape("Ovale", NodeShape.Oval);
            AddShape("Carr�", NodeShape.Square);
            AddShape("Rectangle", NodeShape.Rectangle);
            AddShape("Triangle", NodeShape.Triangle);

            // --- Options couleurs ---
            _optionsPanel = new GroupBox { Text = "Options couleurs", Dock = DockStyle.Top, Height = 155, Padding = new Padding(5) };
            right.Controls.Add(_optionsPanel);
            AddColorOption("Par d�faut", NodeColorMode.Default, true);
            AddColorOption("Extinct", NodeColorMode.Extinct);
            AddColorOption("Tol link", NodeColorMode.TolLink);
            AddColorOption("Confidence", NodeColorMode.Confidence);
            AddColorOption("Phylesis", NodeColorMode.Phylesis);

            // --- Infos n�ud ---
            var info = new GroupBox { Text = "Infos n�ud", Dock = DockStyle.Top, Height = 140, Padding = new Padding(5) };
            right.Controls.Add(info);
            _phy = Make(info); _conf = Make(info); _extinct = Make(info);
            _link = Make(info); _name = Make(info); _id = Make(info);

            // -------- Physique & �v�nements --------
            _physics = new PhysicsController(_graph, _view);

            _view.NodeClicked += n => { ShowInfo(n); HighlightPath(n); AddHistory(n); };
            //_view.DragStarted += () => _physics.Pause();
            //_view.DragEnded += () => _physics.Resume();

            _physics.Start();
        }

        // ---------- UI helpers ----------
        private static Label Make(Control host)
        {
            var l = new Label { Dock = DockStyle.Top, Height = 20, AutoEllipsis = true };
            host.Controls.Add(l); host.Controls.SetChildIndex(l, 0);
            return l;
        }

        private void AddShape(string text, NodeShape shape, bool initial = false)
        {
            var cb = new CheckBox { Text = text, Dock = DockStyle.Top, Checked = initial };
            cb.CheckedChanged += (_, _) =>
            {
                if (!cb.Checked) return;
                foreach (var other in _shapePanel.Controls.OfType<CheckBox>().Where(b => b != cb))
                    other.Checked = false;
                _view.CurrentShape = shape; _view.Invalidate();
            };
            _shapePanel.Controls.Add(cb);
            if (initial) _view.CurrentShape = shape;
        }

        private void AddColorOption(string text, NodeColorMode mode, bool initial = false)
        {
            var rb = new RadioButton { Text = text, Dock = DockStyle.Top, Checked = initial };
            rb.CheckedChanged += (_, _) => { if (rb.Checked) { _view.ColorMode = mode; _view.Invalidate(); } };
            _optionsPanel.Controls.Add(rb); _optionsPanel.Controls.SetChildIndex(rb, 0);
        }

        // ---------- Historique ----------
        private void AddHistory(Node n)
        {
            string entry = $"{n.Title} : {n.Id}";
            _historyList.Items.Remove(entry);
            _historyList.Items.Insert(0, entry);
        }

        private void HistorySelected(object? sender, EventArgs e)
        {
            if (_historyList.SelectedItem is not string s) return;
            int colon = s.LastIndexOf(':'); if (colon < 0) return;
            if (!int.TryParse(s[(colon + 1)..].Trim(), out int id)) return;
            if (!_graph.ById.TryGetValue(id, out var node)) return;

            ShowInfo(node); HighlightPath(node);
        }

        // ---------- Infos ----------
        private void ShowInfo(Node n)
        {
            _id.Text = $"ID           : {n.Id}";
            _name.Text = $"Nom          : {n.Title}";
            _link.Text = $"ToL link     : {n.TolOrgLink}";
            _extinct.Text = $"Extinct      : {(n.Extinct ? "Oui" : "Non")}";
            _conf.Text = $"Confidence   : {n.Confidence}";
            _phy.Text = $"Phylesis     : {n.Phylesis}";
        }

        // ---------- Highlight path ----------
        private void HighlightPath(Node target)
        {
            var path = new HashSet<Node>();
            var parent = _graph.Edges.ToDictionary(e => e.Target, e => e.Source);
            for (var cur = target; cur != null; parent.TryGetValue(cur, out cur))
                path.Add(cur);

            _view.Highlighted = path; _view.Invalidate();
        }

        // ---------- Recherche ----------
        private void Search()
        {
            string q = _searchBox.Text.Trim(); if (q == "") return;

            Node? target = int.TryParse(q, out int id) && _graph.ById.TryGetValue(id, out var n)
                         ? n
                         : _graph.Nodes.FirstOrDefault(n => n.Title.Contains(q, StringComparison.OrdinalIgnoreCase));
            if (target == null) return;

            AddHistory(target); ShowInfo(target); HighlightPath(target);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _physics.Stop();
            base.OnFormClosing(e);
        }
    }
}
