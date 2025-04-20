using System.Linq;
using GraphApp.Model;

namespace GraphApp.Model
{
    public static class GraphLoader
    {
        public static Graph LoadFromCsv(string nodesCsv, string linksCsv)
        {
            var g = new Graph();
            var dict = new Dictionary<int, Node>();

            // ---------- NODES ----------
            foreach (var line in File.ReadLines(nodesCsv).Skip(1))
            {
                var p = line.Split(',');

                var n = new Node
                {
                    Id = int.Parse(p[0]),
                    Title = p[1],
                    ChildNodes = int.Parse(p[2]),
                    IsLeaf = p[3] == "1",
                    TolOrgLink = p[4],
                    Extinct = p[5] == "1",
                    Confidence = p[6],
                    Phylesis = p[7]
                };
                dict[n.Id] = n;
                g.ById[n.Id] = n;
                g.Nodes.Add(n);
            }

            // ---------- LINKS ----------
            foreach (var line in File.ReadLines(linksCsv).Skip(1))
            {
                var p = line.Split(',');
                var src = dict[int.Parse(p[0])];
                var tgt = dict[int.Parse(p[1])];
                src.Children.Add(tgt);
                g.Edges.Add(new Edge(src, tgt));
            }

            // ---------- ROOT ----------
            var targets = g.Edges.Select(e => e.Target.Id).ToHashSet();
            g.Root = g.Nodes.First(n => !targets.Contains(n.Id));

            // ---------- Layout cercle ----------
            double r = 1000, step = 2 * Math.PI / g.Nodes.Count;
            int i = 0;
            foreach (var n in g.Nodes)
            {
                n.X = r * Math.Cos(i * step);
                n.Y = r * Math.Sin(i * step);
                i++;
            }
            return g;
        }
    }
}
