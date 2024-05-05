using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class PickingManager
{
    public readonly List<Pickable> Pickables = new();
    public bool IncludeZeroDist = false;
    public Pickable Picked = null;
    
    public void CheckForRay()
    {
        Picked = null;
        MouseAction action = InputManager.Instance.GetMouseAction(GameAction.LMB);
        if (action is { state: ActionState.RELEASED })
        {
             Ray? ray = CalculateMouseRay();
             if (ray.HasValue)
             {
                 float minDist = 10000.0f;
                 Pickable candidate = null;
                 foreach (Pickable entity in Pickables)
                 {
                     BoundingSphere sphere =
                         entity.Renderer._model.BoundingSphere.Transform(entity.ParentObject.Transform.ModelMatrix);
                     float? dist = sphere.Intersects(ray.Value);
                     
                     if (dist.HasValue)
                     {
                         
                         if ((IncludeZeroDist && dist.Value < minDist) || (!IncludeZeroDist && dist.Value < minDist && dist.Value > 0))
                         {
                             minDist = dist.Value;
                             candidate = entity;
                         }
                     }
                 }
                 Pickables.Clear();
                 Picked = candidate;
             }
        }
        Pickables.Clear();
    }

    private Ray? CalculateMouseRay()
    {
        Point mousePosition = InputManager.Instance.MousePosition;
        if (mousePosition.X < 0 || mousePosition.X > 1440 || mousePosition.Y < 0 || mousePosition.Y > 900) return null;
        //TODO: Change values to use global variable for screen dimensions
        Vector4 clipCoords = new Vector4(((2.0f * mousePosition.X) / 1440.0f) - 1.0f,1.0f - ((2.0f * mousePosition.Y) / 900f),-1f,1f);
        Vector4 eyeSpace = Vector4.Transform(clipCoords,Matrix.Invert(Globals.Projection));
        eyeSpace.Z = -1.0f;
        eyeSpace.W = 0.0f;
        Vector4 worldSpace = Vector4.Transform(eyeSpace, Matrix.Invert(Globals.View));
        Vector3 direction = new Vector3(worldSpace.X, worldSpace.Y, worldSpace.Z);
        direction.Normalize();
        return new Ray(Globals.ViewPos, direction);
    }
}