using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using RTS_Engine.Exceptions;

namespace RTS_Engine.Components.AI;

public static class Pathfinding
{
    public static int ClosedLimit = 50000;
    
    private static float Euclidan(Node n, Node goal)
    {
        return new Vector2(n.Location.X - goal.Location.X, n.Location.Y - goal.Location.Y).Length();
    }

    private static float Manhattan(Node n, Node goal)
    {
        return MathF.Abs(n.Location.X - goal.Location.X) + MathF.Abs(n.Location.Y - goal.Location.Y);
    }
    
    private static List<Node> GetNeighbors(Node n)
    {
        if (Globals.Renderer.WorldRenderer is null) throw new NoTerrainException("There isn't any terrain to pick the nodes from!");
        List<Node> output = new();
        byte nodeConnections = Globals.Renderer.WorldRenderer.MapNodes[n.Location.X, n.Location.Y].Connections;
        //It's assumed that each node has up to 8 neighbors.
        for (int i = 1; i >= -1; i--)
        {
            for (int j = 1; j >= -1; j--)
            {
                if(i == 0 && j == 0) continue;
                if((nodeConnections & 1) > 0)
                    output.Add(
                        new Node(
                            new Point(n.Location.X + i, n.Location.Y + j), 
                            n, 
                            Globals.Renderer.WorldRenderer.MapNodes[n.Location.X + i, n.Location.Y + j].NodeCost));
                nodeConnections >>= 1;
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
    
    public static Node CalculatePath(Node goal, Node start)
    {
        NodeComparer comparer = new NodeComparer();
        PriorityQueue<Node, float> open = new PriorityQueue<Node, float>();
        HashSet<Node> explored = new HashSet<Node>(comparer);
        open.Enqueue(start,0);
        while (open.Count > 0)
        {
            if (explored.Count > ClosedLimit) return null;
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
                        open.Enqueue(n,n.CurrentCost + Manhattan(n,goal));
                    }
                }
            }
        }
        return null;
    }

    public static List<Point> PathToListOfPoints(Node node)
    {
        List<Point> output = new List<Point>();
        Node v = node;
        while (v.ParentNode != null)
        {
            output.Add(v.Location);
            v = v.ParentNode;
        }
        output.Add(v.Location);
        output.Reverse();
        return output;
    }

    public static Queue<Point> PathToQueueOfPoints(Node node)
    {
        return new Queue<Point>(PathToListOfPoints(node));
    }

    public static List<Vector2> PathToListOfVectors(Node node)
    {
        List<Vector2> output = new List<Vector2>();
        Node v = node;
        while (v.ParentNode != null)
        {
            output.Add(new Vector2(v.Location.X,v.Location.Y));
            v = v.ParentNode;
        }
        output.Add(new Vector2(v.Location.X,v.Location.Y));
        output.Reverse();
        return output;
    }

    public static Queue<Vector2> PathToQueueOfVectors(Node node)
    {
        return new Queue<Vector2>(PathToListOfVectors(node));
    }

}