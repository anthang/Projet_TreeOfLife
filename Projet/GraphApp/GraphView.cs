using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GraphApp.Model;

namespace GraphApp
{
    public enum NodeShape { Circle, Oval, Square, Rectangle, Triangle }
    public enum NodeColorMode { Default, Extinct, TolLink, Confidence, Phylesis }

    public class GraphView : Panel
    {
        private readonly Graph _graph;

        public HashSet<Node> Highlighted { get; set; } = new();
        public NodeShape CurrentShape { get; set; } = NodeShape.Circle;
        public NodeColorMode ColorMode { get; set; } = NodeColorMode.Default;

        public event Action<Node>? NodeClicked;

        // zoom / pan
        private float _scale = 1f;
        private PointF _trans = new(0, 0);
        private bool _panning;
        private Point _panStart;

        // drag
        private Node? _dragNode;
        private PointF _dragOffset;

        public GraphView(Graph graph)
        {
            _graph = graph;
            DoubleBuffered = true;

            MouseWheel += (_, e) =>
            {
                _scale = Math.Clamp(_scale * (e.Delta > 0 ? 1.1f : 1 / 1.1f), .1f, 5f);
                Invalidate();
            };

            MouseDown += OnDown;
            MouseMove += OnMove;
            MouseUp += OnUp;
        }

        // ---------- souris ----------
        private void OnDown(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)          // pan
            {
                _panning = true;
                _panStart = e.Location;
                Cursor = Cursors.Hand;
                return;
            }

            var (node, toggle) = HitTest(e.Location);
            if (node == null) return;

            if (toggle)                                  // pli / dépli
            {
                node.Collapsed = !node.Collapsed;
                Invalidate();
                return;
            }

            if (e.Button == MouseButtons.Left)           // drag
            {
                NodeClicked?.Invoke(node);
                _dragNode = node;

                var w = ToWorld(e.Location);
                _dragOffset = new PointF(w.X - node.XF(), w.Y - node.YF());

                node.IsFixed = true;                     // bloque dans la physique
                Cursor = Cursors.SizeAll;
            }
        }

        private void OnMove(object? s, MouseEventArgs e)
        {
            if (_panning && e.Button == MouseButtons.Right)
            {
                _trans.X += e.X - _panStart.X;
                _trans.Y += e.Y - _panStart.Y;
                _panStart = e.Location;
                Invalidate();
                return;
            }

            if (_dragNode != null && e.Button == MouseButtons.Left)
            {
                var w = ToWorld(e.Location);
                _dragNode.X = w.X - _dragOffset.X;
                _dragNode.Y = w.Y - _dragOffset.Y;
                Invalidate();
            }
        }

        private void OnUp(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                _panning = false;
                Cursor = Cursors.Default;
            }

            if (e.Button == MouseButtons.Left && _dragNode != null)
            {
                _dragNode.IsFixed = false;
                _dragNode = null;
                Cursor = Cursors.Default;
            }
        }

        // ---------- dessin ----------
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.TranslateTransform(_trans.X, _trans.Y);
            e.Graphics.ScaleTransform(_scale, _scale);

            foreach (var ed in _graph.Edges.Where(ed => Visible(ed.Source) && Visible(ed.Target)))
                e.Graphics.DrawLine(Pens.Gray,
                                    ed.Source.XF(), ed.Source.YF(),
                                    ed.Target.XF(), ed.Target.YF());

            foreach (var n in _graph.Nodes.Where(Visible))
                DrawNode(e.Graphics, n);
        }

        private void DrawNode(Graphics g, Node n)
        {
            float r = 25f;

            Brush b = Highlighted.Contains(n)
                ? Brushes.Blue
                : ColorMode switch
                {
                    NodeColorMode.Extinct => n.Extinct switch
                    {
                        true => Brushes.Red,
                        false => Brushes.Green,
                    },
                    NodeColorMode.TolLink => n.TolOrgLink switch 
                    {
                                                 "0" => Brushes.Red,
                                                 "1"=> Brushes.Green,
                                                 
                             },

                    NodeColorMode.Confidence => n.Confidence switch
                  
                    {
                        "0" => Brushes.Red,
                        "1" => Brushes.Yellow,
                        "2" => Brushes.Green,
                    },

                    NodeColorMode.Phylesis => n.Phylesis switch

                    {
                        "0" => Brushes.Red,
                        "1" => Brushes.Yellow,
                        "2" => Brushes.Green,
                    },


                    _ => Brushes.MediumPurple
                };

            
            switch (CurrentShape)
            {
                case NodeShape.Circle:
                    g.FillEllipse(b, n.XF() - r, n.YF() - r, 2 * r, 2 * r);
                    g.DrawEllipse(Pens.Black, n.XF() - r, n.YF() - r, 2 * r, 2 * r);
                    break;

                case NodeShape.Oval:
                    g.FillEllipse(b, n.XF() - 1.5f * r, n.YF() - r, 3 * r, 2 * r);
                    g.DrawEllipse(Pens.Black, n.XF() - 1.5f * r, n.YF() - r, 3 * r, 2 * r);
                    break;

                case NodeShape.Square:
                    g.FillRectangle(b, n.XF() - r, n.YF() - r, 2 * r, 2 * r);
                    g.DrawRectangle(Pens.Black, n.XF() - r, n.YF() - r, 2 * r, 2 * r);
                    break;

                case NodeShape.Rectangle:
                    g.FillRectangle(b, n.XF() - r, n.YF() - r, 2 * r, 3 * r);
                    g.DrawRectangle(Pens.Black, n.XF() - r, n.YF() - r, 2 * r, 3 * r);
                    break;

                case NodeShape.Triangle:
                    PointF[] tri =
                    {
                        new(n.XF(),      n.YF() - r),
                        new(n.XF() + r,  n.YF() + r),
                        new(n.XF() - r,  n.YF() + r)
                    };
                    g.FillPolygon(b, tri);
                    g.DrawPolygon(Pens.Black, tri);
                    break;
            }

            // bouton pliage
            if (n.Children.Any())
            {
                float s = 10;
                var rect = new RectangleF(n.XF() + r, n.YF() - s / 2, s, s);

                g.FillRectangle(Brushes.White, rect);
                g.DrawRectangle(Pens.Black, rect.X, rect.Y, s, s);
                g.DrawLine(Pens.Black,
                           rect.X + 2, rect.Y + s / 2,
                           rect.Right - 2, rect.Y + s / 2);
                if (n.Collapsed)
                    g.DrawLine(Pens.Black,
                               rect.X + s / 2, rect.Y + 2,
                               rect.X + s / 2, rect.Bottom - 2);
            }

            // --- NOUVEAU : titre sous le nœud ------------------------
            if (!string.IsNullOrWhiteSpace(n.Title))
            {
                const float padding = 4f;
                float textY = n.YF() + r + padding;

                using var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Near
                };

                g.DrawString(n.Title, Font, Brushes.Black,
                             new PointF(n.XF(), textY), sf);
            }
        }

        // ---------- utilitaires ----------
        private (Node? node, bool toggle) HitTest(Point p)
        {
            var w = ToWorld(p);

            foreach (var n in _graph.Nodes)
            {
                float r = 25f;

                if (n.Children.Any())
                {
                    var rect = new RectangleF(n.XF() + r, n.YF() - 5, 10, 10);
                    if (rect.Contains(w)) return (n, true);
                }

                float dx = w.X - n.XF();
                float dy = w.Y - n.YF();
                if (dx * dx + dy * dy <= r * r) return (n, false);
            }

            return (null, false);
        }

        private bool Visible(Node n)
        {
            for (var cur = n; cur != null;
                 cur = _graph.Edges.FirstOrDefault(e => e.Target == cur)?.Source)
            {
                if (cur.Collapsed && cur != n) return false;
            }

            return true;
        }

        private PointF ToWorld(Point p) =>
            new((p.X - _trans.X) / _scale,
                (p.Y - _trans.Y) / _scale);
    }

    static class NodeExt
    {
        public static float XF(this Node n) => (float)n.X;
        public static float YF(this Node n) => (float)n.Y;
    }
}
