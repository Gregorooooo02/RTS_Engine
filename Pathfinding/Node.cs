using System.Drawing;

namespace RTS_Engine.Pathfinding;

public class Node
{
    public Point Location { get; set; }
    public bool IsWalkable { get; set; }
    public float G { get; set; }
    public float H { get; set; }
    public float F { get; set; }
    public NodeState State { get; set; }
    public Node ParentNode { get; set; }
}

public enum NodeState { Untested, Open, Closed }