using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI;
using Point = Microsoft.Xna.Framework.Point;

namespace RTS_Engine.Pathfinding;

public class PathfindingManager
{
    //TODO: Move some of the implementation to component. Make Map 2D array global so individual pathfinding components can access it.
    
    public short[][] Map;
    
    private float Euclidan(Node n, Node goal)
    {
        return new Vector2(n.Location.X - goal.Location.X, n.Location.Y - goal.Location.Y).Length();
    }


    private List<Node> GetNeighbors(Node n)
    {
        List<Node> output = new();
        //It's assumed that each node has up to 8 neighbors.
        for (int i = -1; i <= 1; i ++)
        {
            for (int j = -1; j <= 1; j ++)
            {
                if(i == 0 && j == 0) continue;
                try
                {
                    short value = Map[n.Location.X][n.Location.Y];
                    if(value > 10000) continue; //Skip unwalkable nodes
                    output.Add(new Node(new Point(n.Location.X + i, n.Location.Y + j),n,value));
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }
        return output;
    }

    private class NodeComparer : IEqualityComparer<Node>
    {
        public bool Equals(Node x, Node y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return x.Location == y.Location;
        }

        public int GetHashCode(Node obj)
        {
            return obj.Location.GetHashCode();
        }
    }
    
    public Node CalculatePath(Node goal, Node start)
    {
        NodeComparer comparer = new NodeComparer();
        PriorityQueue<Node, float> open = new PriorityQueue<Node, float>();
        HashSet<Node> explored = new HashSet<Node>(comparer);
        open.Enqueue(start,0);
        while (open.Count > 0)
        {
            var v = open.Dequeue();
            if (!explored.Contains(v))
            {
                if (comparer.Equals(v,goal)) return v;
                explored.Add(v);
                var neighbors = GetNeighbors(v);
                foreach (Node n in neighbors)
                {
                    if (!explored.Contains(n))
                    {
                        open.Enqueue(n,n.CurrentCost + Euclidan(n,goal));
                    }
                }
            }
        }
        return null;
    }

    public List<Point> PathToListOfPoints(Node node)
    {
        List<Point> output = new List<Point>();
        Node v = node;
        while (v.ParentNode != null)
        {
            output.Add(v.Location);
            v = v.ParentNode;
        }
        output.Reverse();
        return output;
    }


}