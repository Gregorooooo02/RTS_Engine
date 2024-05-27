﻿using Microsoft.Xna.Framework;
using Point = System.Drawing.Point;

namespace RTS_Engine.Pathfinding;

public class Node
{
    public Point Location { get; set; }
    public float CurrentCost { get;private set; }
    public Node ParentNode { get; set;}
    
    public Node(Point gridLocation, Node parentNode, short nodeCost)
    {
        Location = gridLocation;
        ParentNode = parentNode;
        CurrentCost = parentNode.CurrentCost + nodeCost;
    }
}