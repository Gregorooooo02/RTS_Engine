using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class PatrolPathManager
{
    public List<PatrolPath> PatrolPaths = new();

    public List<Vector3> GetPathByIndex(int pathIndex)
    {
        if (pathIndex < 0 || pathIndex >= PatrolPaths.Count) return null;
        return PatrolPaths[pathIndex].GetPathPoints();
    }

    public List<Vector3> GetPathByName(string name)
    {
        return PatrolPaths.Find((path => path.ParentObject.Name == name))?.GetPathPoints();
    }
}