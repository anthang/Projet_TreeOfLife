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

            // --- modèle & vue
            _graph = GraphLoader.LoadFromCsv("Data/nodes.csv", "Data/links.csv");
            _view = new GraphView(_graph) { Dock = DockStyle.Fill };
            Controls.Add(_view);

            // --- Historique gauche
            var histBox = new GroupBox { Text = "Historique", Dock = DockStyle.Left, Width = 180, Padding = new Padding(5) };
            _historyList = new ListBox { Dock = DockStyle.Fill };
            histBox.Controls.Add(_historyList);
            Controls.Add(histBox);
            _historyList.SelectedIndexChanged += HistorySelected;

            // --- Recherche haut
            var top = new Panel { Dock = DockStyle.Top, Height = 35, Padding = new Padding(5) };
            _searchBox = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "ID ou titre…" };
            _searchBtn = new Button { Text = "Go", Dock = DockStyle.Right, Width = 60 };
            top.Controls.AddRange(new Control[] { _searchBox, _searchBtn });
            Controls.Add(top);
            _searchBtn.Click += (_, _) => Search();
            _searchBox.KeyDown += (_, e) => { if (e.KeyCode == Keys.Enter) { Search(); e.Handled = true; } };

            // --- Colonne droite
            var right = new Panel { Dock = DockStyle.Right, Width = 280, Padding = new Padding(5) };
            Controls.Add(right);

            // 1) Formes
            _shapePanel = new GroupBox { Text = "Formes", Dock = DockStyle.Top, Height = 145, Padding = new Padding(5) };
            right.Controls.Add(_shapePanel);
            AddShape("Ovale", NodeShape.Circle, true);
            AddShape("Carré", NodeShape.Square);
            AddShape("Rectangle", NodeShape.Rectangle);
            AddShape("Triangle", NodeShape.Triangle);

            // 2) Options couleurs
            _optionsPanel = new GroupBox { Text = "Options couleurs", Dock = DockStyle.Top, Height = 155, Padding = new Padding(5) };
            right.Controls.Add(_optionsPanel);
            AddColorOption("Par défaut", NodeColorMode.Default, true);
            AddColorOption("Extinct", NodeColorMode.Extinct);
            AddColorOption("Tol link", NodeColorMode.TolLink);
            AddColorOption("Confidence", NodeColorMode.Confidence);
            AddColorOption("Phylesis", NodeColorMode.Phylesis);

            // 3) Infos
            var info = new GroupBox { Text = "Infos nœud", Dock = DockStyle.Top, Height = 140, Padding = new Padding(5) };
            right.Controls.Add(info);
            _phy = Make(info); _conf = Make(info); _extinct = Make(info);
            _link = Make(info); _name = Make(info); _id = Make(info);

            // events
            _view.NodeClicked += n => { ShowInfo(n); Highlight(n); AddHistory(n); };
            _physics = new PhysicsController(_graph, _view); _physics.Start();
        }

        // ---------- aides UI ----------
        private static Label Make(Control host)
        {
            var l = new Label { Dock = DockStyle.Top, Height = 20, AutoEllipsis = true };
            host.Controls.Add(l); host.Controls.SetChildIndex(l, 0);
            return l;
        }

        private void AddShape(string text, NodeShape shp, bool init = false)
        {
            var cb = new CheckBox { Text = text, Dock = DockStyle.Top, Checked = init };
            cb.CheckedChanged += (_, _) =>
            {
                if (!cb.Checked) return;
                foreach (var o in _shapePanel.Controls.OfType<CheckBox>().Where(c => c != cb)) o.Checked = false;
                _view.CurrentShape = shp; _view.Invalidate();
            };
            _shapePanel.Controls.Add(cb);
            if (init) _view.CurrentShape = shp;
        }

        private void AddColorOption(string text, NodeColorMode mode, bool init = false)
        {
            var rb = new RadioButton { Text = text, Dock = DockStyle.Top, Checked = init };
            rb.CheckedChanged += (_, _) => { if (rb.Checked) { _view.ColorMode = mode; _view.Invalidate(); } };
            _optionsPanel.Controls.Add(rb); _optionsPanel.Controls.SetChildIndex(rb, 0);
            if (init) _view.ColorMode = mode;
        }

        // ---------- Historique ----------
        private void AddHistory(Node n)
        {
            string entry = $"{n.Title} : {n.Id}";
            _historyList.Items.Remove(entry);
            _historyList.Items.Insert(0, entry);
        }

        private void HistorySelected(object? s, EventArgs e)
        {
            if (_historyList.SelectedItem is not string txt) return;
            int colon = txt.LastIndexOf(':'); if (colon < 0) return;
            if (!int.TryParse(txt[(colon + 1)..].Trim(), out int id)) return;
            if (!_graph.ById.TryGetValue(id, out var node)) return;
            ShowInfo(node); Highlight(node);
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

        // ---------- surbrillance chemin ----------
        private void Highlight(Node target)
        {
            var path = new HashSet<Node>();
            var parent = _graph.Edges.ToDictionary(e => e.Target, e => e.Source);
            for (var cur = target; cur != null; parent.TryGetValue(cur, out cur)) path.Add(cur);
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

            AddHistory(target); ShowInfo(target); Highlight(target);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _physics.Stop(); base.OnFormClosing(e);
        }
    }
}
