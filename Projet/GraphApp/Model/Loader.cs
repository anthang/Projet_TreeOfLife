using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphApp.Model;

namespace GraphApp.Model
{
    public static class GraphLoader
    {
        // --------------------------------------------------------------------
        //  Fonction utile : découpe une ligne CSV en tenant compte des guillemets
        //  (vraiment basique, mais ça évite les bugs quand le titre contient des ,)
        // --------------------------------------------------------------------
        private static List<string> SplitCsv(string line)
        {
            var cells = new List<string>();
            bool inQuotes = false;
            var cur = "";

            foreach (char c in line)
            {
                if (c == '"')            // on ouvre / ferme les guillemets
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    cells.Add(cur);
                    cur = "";
                }
                else
                {
                    cur += c;
                }
            }
            cells.Add(cur);              // dernier champ
            return cells;
        }

        // --------------------------------------------------------------------
        //  Charge les deux fichiers CSV
        // --------------------------------------------------------------------
        public static Graph LoadFromCsv(string nodesCsv, string linksCsv)
        {
            var g = new Graph();
            var dict = new Dictionary<int, Node>();

            // ---------- NODES ----------
            foreach (var line in File.ReadLines(nodesCsv).Skip(1)) // on saute l'entête
            {
                var p = SplitCsv(line);

                // si la ligne est mauvaise, on l’ignore
                if (p.Count < 8) continue;

                int id;
                int childs;
                if (!int.TryParse(p[0], out id)) continue;
                if (!int.TryParse(p[2], out childs)) childs = 0;

                var n = new Node
                {
                    Id = id,
                    Title = p[1],
                    ChildNodes = childs,
                    IsLeaf = p[3] == "1",
                    TolOrgLink = p[4],
                    Extinct = p[5] == "1",
                    Confidence = p[6],
                    Phylesis = p[7]
                };

                dict[id] = n;
                g.ById[id] = n;
                g.Nodes.Add(n);
            }

            // ---------- LINKS ----------
            foreach (var line in File.ReadLines(linksCsv).Skip(1))
            {
                var p = line.Split(',');
                if (p.Length != 2) continue;

                int srcId, tgtId;
                if (!int.TryParse(p[0], out srcId)) continue;
                if (!int.TryParse(p[1], out tgtId)) continue;
                if (!dict.ContainsKey(srcId) || !dict.ContainsKey(tgtId)) continue;

                var src = dict[srcId];
                var tgt = dict[tgtId];

                src.Children.Add(tgt);
                g.Edges.Add(new Edge(src, tgt));
            }

            // ---------- ROOT ----------
            var targets = g.Edges.Select(e => e.Target.Id).ToHashSet();
            g.Root = g.Nodes.First(n => !targets.Contains(n.Id));

            // ---------- POSITION DE DÉPART ----------
            double r = 1000;
            double step = 2 * Math.PI / g.Nodes.Count;
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
