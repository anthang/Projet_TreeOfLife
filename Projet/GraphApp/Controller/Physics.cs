using System;
using System.Linq;
using System.Windows.Forms;
using GraphApp.Model;

namespace GraphApp.Controller
{
    public class PhysicsController
    {
        public void Pause() => _timer.Enabled = false;
        public void Resume() => _timer.Enabled = true;

        private readonly Graph _graph;
        private readonly Control _invalidator;
        private readonly System.Windows.Forms.Timer _timer;

        // constantes (layout force?directed)
        private const double RepulsionK = 80_000;
        private const double AttractionK = 0.1;
        private const double Damping = 0.85;
        private const double IdealLen = 140;
        private const double TimeStep = 0.02;

        public PhysicsController(Graph graph, Control invalidator)
        {
            _graph = graph;
            _invalidator = invalidator;
            _timer = new System.Windows.Forms.Timer { Interval = 16 };
            _timer.Tick += (_, __) => Step();
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();

        // -----------------------------------------------------------
        private void Step()
        {
            var nodes = _graph.Nodes.Where(IsVisible).ToList();

            // ---------- répulsion ----------
            for (int i = 0; i < nodes.Count; i++)
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    var a = nodes[i]; var b = nodes[j];
                    double dx = a.X - b.X, dy = a.Y - b.Y;
                    double distSq = dx * dx + dy * dy + 0.01;
                    double force = RepulsionK / distSq;
                    double dist = Math.Sqrt(distSq);
                    double fx = force * dx / dist, fy = force * dy / dist;

                    if (!a.IsFixed) { a.VX += fx * TimeStep; a.VY += fy * TimeStep; }
                    if (!b.IsFixed) { b.VX -= fx * TimeStep; b.VY -= fy * TimeStep; }
                }

            // ---------- attraction (ressorts) ----------
            foreach (var e in _graph.Edges.Where(e => IsVisible(e.Source) && IsVisible(e.Target)))
            {
                var a = e.Source; var b = e.Target;
                double dx = b.X - a.X, dy = b.Y - a.Y;
                double dist = Math.Sqrt(dx * dx + dy * dy) + 0.01;
                double force = AttractionK * (dist - IdealLen);
                double fx = force * dx / dist, fy = force * dy / dist;

                if (!a.IsFixed) { a.VX += fx * TimeStep; a.VY += fy * TimeStep; }
                if (!b.IsFixed) { b.VX -= fx * TimeStep; b.VY -= fy * TimeStep; }
            }

            // ---------- mise à jour positions ----------
            foreach (var n in nodes)
            {
                if (n.IsFixed) continue;
                n.VX *= Damping; n.VY *= Damping;
                n.X += n.VX; n.Y += n.VY;
            }

            _invalidator.Invalidate();
        }

        // -----------------------------------------------------------
        // Visible = nœud dont tous les ancêtres sont dépliés
        private bool IsVisible(Node n)
        {
            for (var cur = n; cur != null;
                 cur = _graph.Edges.FirstOrDefault(e => e.Target == cur)?.Source)
            {
                if (cur.Collapsed && cur != n) return false;
            }
            return true;
        }
    }
}
