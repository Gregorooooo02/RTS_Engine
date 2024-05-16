using System;
using System.Collections.Generic;
using System.Drawing;

namespace RTS_Engine.Pathfinding;

public class SearchParameters
{
    public Point StartLocation { get; set; }
    public Point EndLocation { get; set; }
    public bool[,] Map { get; set; }
    
    public SearchParameters(Point startLocation, Point endLocation, bool[,] map)
    {
        StartLocation = startLocation;
        EndLocation = endLocation;
        Map = map;
        
    }

    private bool Search(Node currentNode, Node[,] nodes)
    {
        currentNode.State = NodeState.Closed;
        List<Node> nextNodes = GetAdjacentWalkableNodes(currentNode, nodes);
        nextNodes.Sort((node1, node2) => node1.F.CompareTo(node2.F));
        foreach (var nextNode in nextNodes)
        {
            if (nextNode.Location == EndLocation)
            {
                return true;
            }
            else
            {
                if(Search(nextNode, nodes))
                    return true;
            }
           
        }
        return false;
    }
    
    private List<Node> GetAdjacentWalkableNodes(Node fromNode, Node[,] nodes)
    {
        List<Node> walkableNodes = new List<Node>();
        List<Point> nextLocations = GetAdjacentLocations(fromNode.Location);
        
        foreach (var location in nextLocations)
        {
            int x = location.X;
            int y = location.Y;
            
            if (x < 0 || x >= Map.GetLength(0) || y < 0 || y >= Map.GetLength(1))
                continue;

            Node node = nodes[x, y];

            if (!node.IsWalkable)
                continue;
            
            if (node.State == NodeState.Closed)
                continue;
            
            if (node.State == NodeState.Open)
            {

                float traversalCost = Node.GetTraversalCost(node.Location, fromNode.Location);
                float gTemp = fromNode.G + traversalCost;
                if (gTemp < node.G)
                {
                    node.ParentNode = fromNode;
                    walkableNodes.Add(node);
                    
                }
            }
            else
            {
                // If it's untested, set the parent and flag it as 'Open' for consideration
                node.ParentNode = fromNode;
                node.State = NodeState.Open;
                walkableNodes.Add(node);
            }
        }
        
        // Console.WriteLine(walkableNodes.Count);
        return walkableNodes;
    }
        

    // gets the adjacent locations of a point
    private List<Point> GetAdjacentLocations(Point startingPoint)
    {
        List<Point> adjacentLocations = new List<Point>();
        
        adjacentLocations.Add(new Point(startingPoint.X - 1, startingPoint.Y + 1));
        adjacentLocations.Add(new Point(startingPoint.X - 1, startingPoint.Y));
        adjacentLocations.Add(new Point(startingPoint.X - 1, startingPoint.Y - 1));
        adjacentLocations.Add(new Point(startingPoint.X, startingPoint.Y - 1));
        adjacentLocations.Add(new Point(startingPoint.X + 1, startingPoint.Y - 1));
        adjacentLocations.Add(new Point(startingPoint.X + 1, startingPoint.Y));
        adjacentLocations.Add(new Point(startingPoint.X + 1, startingPoint.Y + 1));
        adjacentLocations.Add(new Point(startingPoint.X, startingPoint.Y + 1));
        return adjacentLocations;
    }
    
    private void assignCosts(Point startingPoint, Point endPoint, int width, int height, Node[,] nodes)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Node node = nodes[i, j];
                node.H = Node.GetTraversalCost(node.Location, endPoint);
                node.G = Node.GetTraversalCost(node.Location, startingPoint);
                node.F = node.G + node.H;
                if (Map[i, j] == false)
                {
                    nodes[i, j].IsWalkable = false;
                }
            }
        }
       
    }
    
    public List<Point> FindPath(Node[,] nodes)
    {
        assignCosts(StartLocation, EndLocation, Map.GetLength(0), Map.GetLength(1), nodes);
        List<Point> path = new List<Point>();
        bool success = Search(nodes[StartLocation.X, StartLocation.Y], nodes);
        if (success)
        {
            Node node = nodes[EndLocation.X, EndLocation.Y];
            while (node.ParentNode != null)
            {
                path.Add(node.Location);
                node = node.ParentNode;
            }
            path.Reverse();
        }
        return path;
    }
}