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
    
    public Node(Point location, bool isWalkable)
    {
        Location = location;
        IsWalkable = isWalkable;
        State = NodeState.Untested;
    }
    
    //distance between two points
    public static float GetTraversalCost(Point location, Point otherLocation)
    {
        float deltaX = location.X - otherLocation.X;
        float deltaY = location.Y - otherLocation.Y;
        return (float)System.Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
}

public enum NodeState { Untested, Open, Closed }