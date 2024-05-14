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

    // private bool Search(Node currentNode)
    // {
    //     List<Node> nextNodes = GetAdjacentWalkableNodes(currentNode);
    //     nextNodes.Sort((node1, node2) => node1.F.CompareTo(node2.F));
    //     foreach (var nextNode in nextNodes)
    //     {
    //         
    //     }
    // }
    
    // private List<Node> GetAdjacentWalkableNodes(Node fromNode)
    // {
    //     List<Node> walkableNodes = new List<Node>();
    //     IEnumerable<Point> nextLocations = GetAdjacentLocations(fromNode.Location);
    //     
    //     foreach (var location in nextLocations)
    //     {
    //         int x = location.X;
    //         int y = location.Y;
    //         
    //         if (x < 0 || x >= Map.GetLength(0) || y < 0 || y >= Map.GetLength(1))
    //             continue; 
    //         
    //         // Node node = this.nodes[x, y];
    //             
    //         
    //         
    //         
    //         
    //     }
    //     return walkableNodes;
    // }
}