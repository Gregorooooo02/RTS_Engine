using Microsoft.Xna.Framework;

namespace RTS_Engine.Components.AI;

public class MapNode
{
    public Point Location;

    public float NodeCost;

    public float Height;

    public byte Connections;
    
    public int AllyOccupantId = 0;

    public MapNode(Point location,float height, float nodeCost = 1.0f)
    {
        Location = location;
        Height = height;
        NodeCost = nodeCost;
    }
}