using Microsoft.Xna.Framework;

namespace RTS_Engine.Components.AI;

public class Node
{
    public Point Location { get; set; }
    public float CurrentCost { get;private set; }
    public Node ParentNode { get; set;}
    
    public Node(Point gridLocation, Node parentNode, float nodeCost)
    {
        Location = gridLocation;
        ParentNode = parentNode;
        CurrentCost = parentNode != null ? parentNode.CurrentCost + nodeCost : nodeCost;
    }
}