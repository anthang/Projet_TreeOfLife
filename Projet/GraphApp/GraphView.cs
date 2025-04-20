using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraphApp.Model;

namespace GraphApp
{
    public enum NodeShape { Circle, Square, Rectangle, Triangle }
    public enum NodeColorMode { Default, Extinct, TolLink, Confidence, Phylesis }

    public class GraphView : Panel
    {
        private readonly Graph _graph;

        public HashSet<Node> Highlighted { get; set; } = new();
        public NodeShape CurrentShape { get; set; } = NodeShape.Circle;
        public NodeColorMode ColorMode { get; set; } = NodeColorMode.Default;

        public event Action<Node>? NodeClicked;      // ← événement exposé

        // zoom / pan
        private float _scale = 1f;
        private PointF _trans = new(0, 0);
        private bool _panning;
        private Point _panStart;

        // ---------- constructeur attendu par MainForm ----------
        public GraphView(Graph graph)
        {
            _graph = graph;
            DoubleBuffered = true;

            MouseWheel += (_, e) =>
            {
                _scale = Math.Clamp(_scale * (e.Delta > 0 ? 1.1f : 1 / 1.1f), 0.1f, 5f);
                Invalidate();
            };
            MouseDown += OnDown;
            MouseMove += OnMove;
            MouseUp += OnUp;
        }

        // ---------- souris ----------
        private void OnDown(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) { _panning = true; _panStart = e.Location; Cursor = Cursors.Hand; return; }

            var n = HitTest(e.Location);
            if (e.Button == MouseButtons.Left && n != null)
                NodeClicked?.Invoke(n);
        }

        private void OnMove(object? s, MouseEventArgs e)
        {
            if (_panning && e.Button == MouseButtons.Right)
            {
                _trans.X += e.X - _panStart.X;
                _trans.Y += e.Y - _panStart.Y;
                _panStart = e.Location;
                Invalidate();
            }
        }

        private void OnUp(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) { _panning = false; Cursor = Cursors.Default; }
        }

        // ---------- dessin ----------
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(_trans.X, _trans.Y);
            e.Graphics.ScaleTransform(_scale, _scale);

            foreach (var ed in _graph.Edges)
                e.Graphics.DrawLine(Pens.Gray, ed.Source.XF(), ed.Source.YF(), ed.Target.XF(), ed.Target.YF());

            foreach (var n in _graph.Nodes)
                DrawNode(e.Graphics, n);
        }

        private void DrawNode(Graphics g, Node n)
        {
            float r = 25f;
            Brush brush;

            if (Highlighted.Contains(n))
                brush = Brushes.Yellow;
            else
                brush = ColorMode switch
                {
                    NodeColorMode.Extinct => n.Extinct
                                            ? Brushes.Red                  // éteint
                                            : Brushes.Green,               // vivant

                  
                    NodeColorMode.TolLink => string.IsNullOrWhiteSpace(n.TolOrgLink)
                                             ? new SolidBrush(ColorTranslator.FromHtml("#8E8E8E"))   // gris : pas de lien
                                             : new SolidBrush(ColorTranslator.FromHtml("#2980B9")),  // bleu : lien présent



                    // -------- Nuance 0‑1‑2 --------
                    NodeColorMode.Confidence => n.Confidence switch
                    {
                        "2" => new SolidBrush(ColorTranslator.FromHtml("#27AE60")), // vert
                        "1" => new SolidBrush(ColorTranslator.FromHtml("#F39C12")), // orange
                        _ => new SolidBrush(ColorTranslator.FromHtml("#E74C3C")), // rouge
                    },

                    NodeColorMode.Phylesis => n.Phylesis switch
                    {
                        "2" => new SolidBrush(ColorTranslator.FromHtml("#1ABC9C")), // cyan
                        "1" => new SolidBrush(ColorTranslator.FromHtml("#3498DB")), // bleu
                        _ => new SolidBrush(ColorTranslator.FromHtml("#9B59B6")), // violet
                    },

                    _ /* Default */          => Brushes.MediumPurple
                };

            // --- forme ---
            switch (CurrentShape)
            {
                case NodeShape.Circle:
                    g.FillEllipse(brush, n.XF() - r, n.YF() - r, 2 * r, 2 * r);
                    g.DrawEllipse(Pens.Black, n.XF() - r, n.YF() - r, 2 * r, 2 * r);
                    break;
                case NodeShape.Square:
                    g.FillRectangle(brush, n.XF() - r, n.YF() - r, 2 * r, 2 * r);
                    g.DrawRectangle(Pens.Black, n.XF() - r, n.YF() - r, 2 * r, 2 * r);
                    break;
                case NodeShape.Rectangle:
                    g.FillRectangle(brush, n.XF() - r, n.YF() - r, 2 * r, 2 * r);
                    g.DrawRectangle(Pens.Black, n.XF() - r, n.YF() - r, 2 * r, 2 * r);
                    break;
                case NodeShape.Triangle:
                    PointF[] tri = { new(n.XF(), n.YF() - r),
                             new(n.XF() + r, n.YF() + r),
                             new(n.XF() - r, n.YF() + r) };
                    g.FillPolygon(brush, tri);
                    g.DrawPolygon(Pens.Black, tri);
                    break;
            }
        }


        // ---------- utilitaires ----------
        private Node? HitTest(Point p)
        {
            var w = ToWorld(p);
            foreach (var n in _graph.Nodes)
            {
                float dx = w.X - n.XF(), dy = w.Y - n.YF();
                if (dx * dx + dy * dy <= 25f * 25f) return n;
            }
            return null;
        }

        private PointF ToWorld(Point p) => new((p.X - _trans.X) / _scale, (p.Y - _trans.Y) / _scale);

        private static Color HashColor(string s)
        {
            if (string.IsNullOrEmpty(s)) return Color.Gray;
            int h = s.GetHashCode();
            return Color.FromArgb(255, 80 + (h & 0x7F),
                                       80 + ((h >> 7) & 0x7F),
                                       80 + ((h >> 14) & 0x7F));
        }
    }

    static class NodeExt
    {
        public static float XF(this Node n) => (float)n.X;
        public static float YF(this Node n) => (float)n.Y;
    }
}
