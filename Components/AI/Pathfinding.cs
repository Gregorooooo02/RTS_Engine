using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using RTS_Engine.Exceptions;

namespace RTS_Engine.Components.AI;

public static class Pathfinding
{
    public static int ClosedLimit = 5000;
    
    private static float Euclidan(Node n, Node goal)
    {
        return new Vector2(n.Location.X - goal.Location.X, n.Location.Y - goal.Location.Y).Length();
    }

    private static float Manhattan(Node n, Node goal)
    {
        return MathF.Abs(n.Location.X - goal.Location.X) + MathF.Abs(n.Location.Y - goal.Location.Y);
    }
    
    private static List<Node> GetNeighbors(Node n, bool isAlly, int id)
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
                if (
                    (nodeConnections & 1) > 0 && 
                    ((
                         isAlly && (Globals.Renderer.WorldRenderer.MapNodes[n.Location.X + i, n.Location.Y + j].AllyOccupantID == id || Globals.Renderer.WorldRenderer.MapNodes[n.Location.X + i, n.Location.Y + j].AllyOccupantID == 0)) 
                     ||(!isAlly && (Globals.Renderer.WorldRenderer.MapNodes[n.Location.X + i, n.Location.Y + j].EnemyOccupantID == id || Globals.Renderer.WorldRenderer.MapNodes[n.Location.X + i, n.Location.Y + j].EnemyOccupantID == 0))))
                {
                    output.Add(
                        new Node(
                            new Point(n.Location.X + i, n.Location.Y + j), 
                            n, 
                            Globals.Renderer.WorldRenderer.MapNodes[n.Location.X + i, n.Location.Y + j].NodeCost));
                }
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
    
    public static Node CalculatePath(Node goal, Node start, bool isAlly = true, int id = 1)
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
                var neighbors = GetNeighbors(v, isAlly, id);
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

    public static Point? GetFirstNearbyFreePoint(Vector2 location, int id)
    {
        //Calculate indexes of vertices between which the provided location is
        int xDown = (int)MathF.Floor(location.X);
        int zDown = (int)MathF.Floor(location.Y);

        int xBoundary = Globals.Renderer.WorldRenderer.MapNodes.GetLength(0) - 1;
        int zBoundary = Globals.Renderer.WorldRenderer.MapNodes.GetLength(1) - 1;
        
        int currentDirectionIterations = 0;
        int currentIterationLimit = 1;
        bool changeLimit = false;
        int direction = 0;
        int offsetX = 0, offsetZ = 0;
        int iterations = 0;
        while (iterations < 16) 
        {
            if (xDown + offsetX > 0 && xDown + offsetX < xBoundary && zDown + offsetZ > 0 &&
                zDown + offsetZ < zBoundary)
            {
                //The point falls within the terrain
                //Check for occupancy
                int occupantId = Globals.Renderer.WorldRenderer.MapNodes[xDown + offsetX, zDown + offsetZ].AllyOccupantID;
                if (occupantId == 0 || occupantId == id)
                {
                    return new Point(xDown + offsetX, zDown + offsetZ);
                }
            }
            if (currentIterationLimit == currentDirectionIterations)
            {
                currentDirectionIterations = 0;
                if (direction == 3)
                {
                    direction = 0;
                }
                else
                {
                    direction++;
                }
                if (changeLimit)
                {
                    changeLimit = false;
                    currentIterationLimit++;
                }
                else
                {
                    changeLimit = true;
                }
            }
            switch (direction)
            {
                case 0:
                {
                    offsetX++;
                    break;
                }
                case 1:
                {
                    offsetZ++;
                    break;
                }
                case 2:
                {
                    offsetX--;
                    break;
                }
                case 3:
                {
                    offsetZ--;
                    break;
                }
            }
            currentDirectionIterations++;
            iterations++;
        }
        return null;
    }
}