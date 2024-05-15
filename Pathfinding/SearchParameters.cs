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
        List<Node> nextNodes = GetAdjacentWalkableNodes(currentNode, nodes);
        nextNodes.Sort((node1, node2) => node1.F.CompareTo(node2.F));
        foreach (var nextNode in nextNodes)
        {
            
        }

        return true;
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
                else
                {
                    // If it's untested, set the parent and flag it as 'Open' for consideration
                    node.ParentNode = fromNode;
                    node.State = NodeState.Open;
                    walkableNodes.Add(node);
                }
            }
        }
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
    
    private void assignCosts(Point startingPoint, Point endPoint)
    {
        for (int i = 0; i < 128; i++)
        {
            for (int j = 0; j < 128; j++)
            {
                Node node = Globals.Nodes[i, j];
                node.H = Node.GetTraversalCost(node.Location, endPoint);
                node.G = Node.GetTraversalCost(node.Location, startingPoint);
                node.F = node.G + node.H;
                if (Map[i, j] == false)
                {
                    Globals.Nodes[i, j].IsWalkable = false;
                }
            }
        }
       
    }
}