namespace GraphApp.Model
{
    public class Node
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;


        public int ChildNodes { get; set; }
        public bool IsLeaf { get; set; }
        public string TolOrgLink { get; set; } = string.Empty;
        public bool Extinct { get; set; }
        public string Confidence { get; set; } = string.Empty;
        public string Phylesis { get; set; } = string.Empty;


        public double X, Y, VX, VY;
        public bool IsFixed { get; set; }
        public Vector DragOffset { get; set; } = new(0, 0);


        public List<Node> Children { get; } = new();
        public bool Collapsed { get; set; }
    }

    public record Edge(Node Source, Node Target);

    public struct Vector
    {
        public double X, Y;
        public Vector(double x, double y) { X = x; Y = y; }
        public static Vector operator -(Vector a, Vector b) => new(a.X - b.X, a.Y - b.Y);
    }
}
