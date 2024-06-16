using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class PickingManager
{
    public bool SinglePickingActive = false;
    public bool BoxPickingActive = false;
    public bool EnemyPickingActive = false;
    public bool GroundPickingActive = false;
    
    public bool PlayerBuildingPickingActive = true;
    public bool PlayerMissionSelectPickingActive = true;
    public readonly List<Pickable> Pickables = new();
    public bool IncludeZeroDist = false;
    private const int HoldThreshold = 5;

    public bool PickedUnits = false;
    public bool PickedEnemy = false;
    public bool PickedBuilding = false;
    public bool PickedMissionSelect = false;
    
    #region Visual Parameters
    private Texture2D _selectionBox;
    private Rectangle selectionBoxArea;
    private Color _boxColor = new(128,128,128,64);
    private bool shouldDraw = false;
    #endregion

    public PickingManager(ContentManager content)
    {
        _selectionBox = content.Load<Texture2D>("blank");
    }

    public void DrawSelectionBox()
    {
        if(shouldDraw)Globals.SpriteBatch.Draw(_selectionBox,selectionBoxArea,_boxColor);
    }
    
    public struct PickingFrustum
    {
        private Plane[] planes;

#if DEBUG
        public VertexPositionColor[] Vertices;
        public int[] Indices;
        public BasicEffect BasicEffect;
#endif
        public bool Intersects(BoundingSphere sphere)
        {
            short result = 0;
            for (int i = 0; i < 6; i++)
            {
                if ((int)planes[i].Intersects(sphere) > 0)
                {
                    result++;
                }
            }
            return result == 6;
        }
        
        public PickingFrustum(Vector3 topFrontLeft, Vector3 topFrontRight, Vector3 topBackLeft, Vector3 topBackRight,
            Vector3 bottomFrontLeft, Vector3 bottomFrontRight, Vector3 bottomBackLeft, Vector3 bottomBackRight)
        {
            planes = new Plane[6];
            
            //Front
            planes[0] = new Plane(topFrontLeft, topFrontRight, bottomFrontLeft);
            //Back
            planes[1] = new Plane(topBackRight, topBackLeft, bottomBackLeft);
            //Top
            planes[2] = new Plane(topFrontRight, topFrontLeft, topBackLeft);
            //Bottom
            planes[3] = new Plane(bottomFrontLeft,bottomFrontRight,bottomBackLeft);
            //Left
            planes[4] = new Plane(topBackLeft,topFrontLeft,bottomBackLeft);
            //Right
            planes[5] = new Plane(topFrontRight,topBackRight,bottomBackRight);

            Vector3 avg = topFrontLeft + topFrontRight + topBackLeft + topBackRight + bottomFrontLeft +
                          bottomFrontRight + bottomBackLeft + bottomBackRight;
            avg /= 8;

            for (int i = 0; i < planes.Length;i++)
            {
                Vector3 pointOnPlane = planes[i].Normal * -planes[i].D;
                Vector3 planeToAvg = avg - pointOnPlane;
                if (Vector3.Dot(planeToAvg, planes[i].Normal) > 0)
                {
                    planes[i].Normal = -planes[i].Normal;
                    planes[i].D = -planes[i].D;
                    planes[i].Normalize();
                }
            }
#if DEBUG
            BasicEffect = new BasicEffect(Globals.GraphicsDevice);
            BasicEffect.World = Matrix.Identity;
            Vertices = new VertexPositionColor[8];
            Vertices[0] = new VertexPositionColor(topFrontLeft, Color.Red);
            Vertices[1] = new VertexPositionColor(topFrontRight, Color.Red);
            Vertices[2] = new VertexPositionColor(topBackLeft, Color.Red);
            Vertices[3] = new VertexPositionColor(topBackRight, Color.Red);
            
            Vertices[4] = new VertexPositionColor(bottomFrontLeft, Color.Red);
            Vertices[5] = new VertexPositionColor(bottomFrontRight, Color.Red);
            Vertices[6] = new VertexPositionColor(bottomBackLeft, Color.Red);
            Vertices[7] = new VertexPositionColor(bottomBackRight, Color.Red);
            
            Indices = new []{
                0, 2, 1, //top
                2, 3, 1,
                5, 4, 0, //front
                5, 0, 1,
                6, 3, 2, //back
                3, 6, 7,
                4, 5, 6, //bottom
                6, 5, 7,
                2, 0, 6, //left
                6, 0, 4,
                1, 3, 5, //right
                5, 3, 7
            };
#endif
        }
#if DEBUG
        public void DrawFrustum()
        {
            BasicEffect.View = Globals.View;
            BasicEffect.Projection = Globals.Projection;
            Globals.GraphicsDevice.RasterizerState = Globals.WireFrame;
            foreach (EffectPass pass in BasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Globals.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList,Vertices,0,Vertices.Length,Indices,0, Indices.Length / 3);
            }
        }
#endif
        
        
    }
    
    public List<Pickable> PickUnits()
    {
        PickedUnits = false;
        List<Pickable> picked = new();
        MouseAction action = InputManager.Instance.GetMouseAction(GameAction.LMB);
        if (action is { state: ActionState.RELEASED, duration: <= HoldThreshold} && SinglePickingActive)
        {
             Ray? ray = CalculateMouseRay(InputManager.Instance.MousePosition);
             if (ray.HasValue)
             {
                 float minDist = 10000.0f;
                 Pickable candidate = null;
                 foreach (Pickable entity in Pickables)
                 {
                     if(entity.Type != Pickable.PickableType.Unit) continue;
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
                 if(candidate != null)picked.Add(candidate);
             }

             PickedUnits = true;
        } 
        else if (action is { state: ActionState.RELEASED, duration: > HoldThreshold} && BoxPickingActive)
        {
            PickingFrustum? frustum = CalculatePickingFrustum(action);
            if (frustum.HasValue)
            {
                foreach (Pickable entity in Pickables)
                {
                    if (entity.Type == Pickable.PickableType.Unit && frustum.Value.Intersects(entity.Renderer._model.BoundingSphere.Transform(entity.ParentObject.Transform.ModelMatrix)))
                    {
                        picked.Add(entity); 
                    }
                }
                Globals.Renderer.PickingFrustum = frustum;
            }
            PickedUnits = true;
        }
        shouldDraw = action is { state: ActionState.PRESSED, duration: > HoldThreshold} && BoxPickingActive;
        if (shouldDraw)
        {
            Point currentMousePos = InputManager.Instance.MousePosition;
            Point startMousePos = action.StartingPosition;
            Point dimensions;
            dimensions.X = Math.Abs(currentMousePos.X - startMousePos.X);
            dimensions.Y = Math.Abs(currentMousePos.Y - startMousePos.Y);
            Point origin = (startMousePos + currentMousePos);
            origin.X = (origin.X / 2) - (dimensions.X / 2);
            origin.Y = (origin.Y / 2) - (dimensions.Y / 2);
            selectionBoxArea.Size = dimensions;
            selectionBoxArea.Location = origin;
        }
        return picked;
    }

    public Pickable PickEnemy()
    {
        PickedEnemy = false;
        if (!EnemyPickingActive) return null;
        MouseAction action = InputManager.Instance.GetMouseAction(GameAction.RMB);
        if (action is {state: ActionState.RELEASED})
        {
            PickedEnemy = true;
            Ray? ray = CalculateMouseRay(InputManager.Instance.MousePosition);
            if (ray.HasValue)
            {
                float minDist = 10000.0f;
                Pickable candidate = null;
                foreach (Pickable entity in Pickables)
                {
                    if(entity.Type != Pickable.PickableType.Enemy) continue;
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
                return candidate;
            }
        }
        return null;
    }

    public Pickable PickBuilding()
    {
        PickedBuilding = false;
        if (!PlayerBuildingPickingActive) return null;
        MouseAction action = InputManager.Instance.GetMouseAction(GameAction.LMB);
        if (action is { state: ActionState.RELEASED })
        {
            PickedBuilding = true;
            Ray? ray = CalculateMouseRay(InputManager.Instance.MousePosition);
            if (ray.HasValue)
            {
                float minDist = 10000.0f;
                Pickable candidate = null;
                
                foreach (Pickable entity in Pickables)
                {
                    if(entity.Type != Pickable.PickableType.Building) continue;
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
                return candidate;
            }
        }
        return null;
    }

    public Pickable PickMissionSelect()
    {
        PickedMissionSelect = false;
        if (!PlayerMissionSelectPickingActive) return null;
        MouseAction action = InputManager.Instance.GetMouseAction(GameAction.LMB);
        if (action is { state : ActionState.RELEASED })
        {
            PickedMissionSelect = true;
            Ray? ray = CalculateMouseRay(InputManager.Instance.MousePosition);
            if (ray.HasValue)
            {
                float minDist = 10000.0f;
                Pickable candidate = null;

                foreach (Pickable entity in Pickables)
                {
                    if (entity.Type != Pickable.PickableType.MissionSelect) continue;
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
                return candidate;
            }
        }
        return null;
    }

    public void CheckForBuildingSelection()
    {
        Pickable pickedBuilding = Globals.PickingManager.PickBuilding();

        if (pickedBuilding is { Type : Pickable.PickableType.Building })
        {
            Building temp = pickedBuilding.ParentObject.GetComponent<Building>();
            temp?.OnClick();
        }
    }
    
    public void CheckForMissionSelection()
    {
        Pickable pickedMission = Globals.PickingManager.PickMissionSelect();

        if (pickedMission is { Type : Pickable.PickableType.MissionSelect })
        {
            MissionSelection temp = pickedMission.ParentObject.GetComponent<MissionSelection>();
            temp?.OnClick();
        }
    }

    private PickingFrustum? CalculatePickingFrustum(MouseAction action)
    {
            Point currentMousePos = InputManager.Instance.MousePosition;
            Point startMousePos = action.StartingPosition;
            Point mix1 = new Point(currentMousePos.X, startMousePos.Y);
            Point mix2 = new Point(startMousePos.X, currentMousePos.Y);
            if (currentMousePos.X < 0 || currentMousePos.X > Globals.GraphicsDeviceManager.PreferredBackBufferWidth 
                                      || currentMousePos.Y < 0 || currentMousePos.Y > Globals.GraphicsDeviceManager.PreferredBackBufferHeight 
                                      || startMousePos.X < 0 || startMousePos.X > Globals.GraphicsDeviceManager.PreferredBackBufferWidth 
                                      || startMousePos.Y < 0 || startMousePos.Y > Globals.GraphicsDeviceManager.PreferredBackBufferHeight) return null;
            
            Ray? topLeft;
            Ray? topRight;
            Ray? bottomLeft;
            Ray? bottomRight;
            
            //Selection from top left to bottom right
            if (currentMousePos.X > startMousePos.X && currentMousePos.Y > startMousePos.Y)
            {
                topLeft = CalculateMouseRay(startMousePos);
                topRight = CalculateMouseRay(mix1);
                bottomLeft = CalculateMouseRay(mix2);
                bottomRight = CalculateMouseRay(currentMousePos);
            }
            //Selection from bottom left to top right
            else if (currentMousePos.X > startMousePos.X && currentMousePos.Y < startMousePos.Y)
            {
                topLeft = CalculateMouseRay(mix2);
                topRight = CalculateMouseRay(currentMousePos);
                bottomLeft = CalculateMouseRay(startMousePos);
                bottomRight = CalculateMouseRay(mix1);
            }
            //Selection from top right to bottom left
            else if (currentMousePos.X < startMousePos.X && currentMousePos.Y > startMousePos.Y)
            {
                topLeft = CalculateMouseRay(mix1);
                topRight = CalculateMouseRay(startMousePos);
                bottomLeft = CalculateMouseRay(currentMousePos);
                bottomRight = CalculateMouseRay(mix2);
            }
            //Selection from bottom right to top left
            else if (currentMousePos.X < startMousePos.X && currentMousePos.Y < startMousePos.Y)
            {
                topLeft = CalculateMouseRay(currentMousePos);
                topRight = CalculateMouseRay(mix2);
                bottomLeft = CalculateMouseRay(mix1);
                bottomRight = CalculateMouseRay(startMousePos);
            }
            //None - so if any of the two coordinates are the same
            else
            {
                return null;
            }
            if (topLeft.HasValue && topRight.HasValue && bottomRight.HasValue && bottomLeft.HasValue)
            {
                var topFrontLeft = topLeft.Value.Position + topLeft.Value.Direction * topLeft.Value.Intersects(Globals.BoundingFrustum.Near).Value;
                var topBackLeft = topLeft.Value.Position + topLeft.Value.Direction * topLeft.Value.Intersects(Globals.BoundingFrustum.Far).Value;
                var topFrontRight = topRight.Value.Position + topRight.Value.Direction * topRight.Value.Intersects(Globals.BoundingFrustum.Near).Value;
                var topBackRight = topRight.Value.Position + topRight.Value.Direction * topRight.Value.Intersects(Globals.BoundingFrustum.Far).Value;
                var bottomFrontLeft = bottomLeft.Value.Position + bottomLeft.Value.Direction * bottomLeft.Value.Intersects(Globals.BoundingFrustum.Near).Value;
                var bottomBackLeft = bottomLeft.Value.Position + bottomLeft.Value.Direction * bottomLeft.Value.Intersects(Globals.BoundingFrustum.Far).Value;
                var bottomFrontRight = bottomRight.Value.Position + bottomRight.Value.Direction * bottomRight.Value.Intersects(Globals.BoundingFrustum.Near).Value;
                var bottomBackRight = bottomRight.Value.Position + bottomRight.Value.Direction * bottomRight.Value.Intersects(Globals.BoundingFrustum.Far).Value;
                return new PickingFrustum(topFrontLeft, topFrontRight, topBackLeft, topBackRight,
                    bottomFrontLeft, bottomFrontRight, bottomBackLeft, bottomBackRight);
            }
            return null;
    }

    private Ray? CalculateMouseRay(Point mousePosition)
    {
        if (mousePosition.X < 0 || mousePosition.X > Globals.GraphicsDeviceManager.PreferredBackBufferWidth || mousePosition.Y < 0 || mousePosition.Y > Globals.GraphicsDeviceManager.PreferredBackBufferHeight) return null;
        Vector4 clipCoords = new Vector4(((2.0f * mousePosition.X) / Globals.GraphicsDeviceManager.PreferredBackBufferWidth) - 1.0f,1.0f - ((2.0f * mousePosition.Y) / Globals.GraphicsDeviceManager.PreferredBackBufferHeight),-1f,1f);
        Vector4 eyeSpace = Vector4.Transform(clipCoords,Matrix.Invert(Globals.Projection));
        eyeSpace.Z = -1.0f;
        eyeSpace.W = 0.0f;
        Vector4 worldSpace = Vector4.Transform(eyeSpace, Matrix.Invert(Globals.View));
        Vector3 direction = new Vector3(worldSpace.X, worldSpace.Y, worldSpace.Z);
        direction.Normalize();
        return new Ray(Globals.ViewPos, direction);
    }

    public Vector3? PickGround(Point mousePosition, float heightGrace, int maxTries = 10)
    {
        if (!GroundPickingActive) return null;
        if (Globals.Renderer.WorldRenderer is null) throw new InvalidOperationException("Can't pick the ground because there is no ground to pick.");
        Ray? mouseRay = CalculateMouseRay(mousePosition);
        if (mouseRay.HasValue)
        {
            Ray ray = mouseRay.Value;
            float h = 0;
            int tries = 0;
            while (true)
            {
                //Find k that satisfies the equation: P.Y + k * D.Y = h
                //where: 
                //P.Y - Y element of ray position
                //D.Y - Y element of ray direction
                //h - tested height
                float k = ((ray.Position.Y - h) / -ray.Direction.Y);
            
                //Using calculated k find intersection point between ray and tested height
                Vector2 intersectionPoint = new Vector2(ray.Position.X + k * ray.Direction.X,
                    ray.Position.Z + k * ray.Direction.Z);
                if (intersectionPoint.X < 0 ||
                    intersectionPoint.X > Globals.Renderer.WorldRenderer.MapNodes.Length - 1 ||
                    intersectionPoint.Y < 0 || intersectionPoint.Y > Globals.Renderer.WorldRenderer.MapNodes.Length - 1)
                {
                    return null;
                }
                
                //Calculate weighted average of height values in neighboring vertices
                float avg = InterpolateWorldHeight(intersectionPoint);

                //If calculated avg value is close enough return the position of the intersection.
                if (MathF.Abs(avg - h) <= heightGrace) return new Vector3(intersectionPoint.X, avg, intersectionPoint.Y);
                //Else try again with new height value
                h = avg;
                tries++;
                if(tries > maxTries) return new Vector3(intersectionPoint.X, avg, intersectionPoint.Y);
            }
        }
        return null;
    }

    public static float InterpolateWorldHeight(Vector2 location)
    {
        //Calculate indexes of vertices between which the provided location is
        int xDown = (int)MathF.Floor(location.X);
        int xUp = xDown + 1;
        int zDown = (int)MathF.Floor(location.Y);
        int zUp = zDown + 1;

        //Calculate weights for vertices
        float weightX = location.X % 1;
        float weightZ = location.Y % 1;
            
        //Prepare height data from selected vertices
        float height1 = Globals.Renderer.WorldRenderer.MapNodes[xDown, zDown].Height;
        float height2 = Globals.Renderer.WorldRenderer.MapNodes[xDown, zUp].Height;
        float height3 = Globals.Renderer.WorldRenderer.MapNodes[xUp, zDown].Height;
        float height4 = Globals.Renderer.WorldRenderer.MapNodes[xUp, zUp].Height;

        //Calculate weighted average of height values in selected vertices.
        return (height1 * (1 - weightX) + height1 * (1 - weightZ) + 
                height2 * (1 - weightX) + height2 * weightZ + 
                height3 * weightX + height3 * (1 - weightZ) +
                height4 * weightX + height4 * weightZ) / 4.0f;
    }

    public static float InterpolateWorldHeight(Vector2 location, WorldRenderer world)
    {
        //Calculate indexes of vertices between which the provided location is
        int xDown = (int)MathF.Floor(location.X);
        int xUp = xDown + 1;
        int zDown = (int)MathF.Floor(location.Y);
        int zUp = zDown + 1;

        //Calculate weights for vertices
        float weightX = location.X % 1;
        float weightZ = location.Y % 1;
            
        //Prepare height data from selected vertices
        float height1 = world.MapNodes[xDown, zDown].Height;
        float height2 = world.MapNodes[xDown, zUp].Height;
        float height3 = world.MapNodes[xUp, zDown].Height;
        float height4 = world.MapNodes[xUp, zUp].Height;

        //Calculate weighted average of height values in selected vertices.
        return (height1 * (1 - weightX) + height1 * (1 - weightZ) + 
                height2 * (1 - weightX) + height2 * weightZ + 
                height3 * weightX + height3 * (1 - weightZ) +
                height4 * weightX + height4 * weightZ) / 4.0f;
    }
}