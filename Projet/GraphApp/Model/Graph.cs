using GraphApp.Model;

public class Graph
{
    public List<Node> Nodes { get; } = new();
    public List<Edge> Edges { get; } = new();

    // NEW ?? acc�s rapide et racine
    public Dictionary<int, Node> ById { get; } = new();
    public Node? Root { get; set; }
}
