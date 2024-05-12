using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class PickingManager
{
    public readonly List<Pickable> Pickables = new();
    public bool IncludeZeroDist = false;
    public List<Pickable> Picked = new() ;

    public struct PickingFrustum
    {
        private Plane[] planes;

#if DEBUG
        public VertexPositionColor[] Vertices;
        public int[] Indices;
        public BasicEffect BasicEffect;
#endif
        
        private static short IntersectMask = 0b0000101010101010;
        private static short InsideMask = 0b0000010101010101;
        public bool Intersects(BoundingSphere sphere)
        {
            short result = 0;
            for (int i = 0; i < 6; i++)
            {
                var temp = (short)planes[i].Intersects(sphere);
                //Console.WriteLine(temp);
                result += temp;
                result <<= 2;
                
            }
            result >>= 2;
            //Console.WriteLine(result);
            return (result & IntersectMask) > 0 || (result & InsideMask) == InsideMask;
        }
        
        public PickingFrustum(Vector3 topFrontLeft, Vector3 topFrontRight, Vector3 topBackLeft, Vector3 topBackRight,
            Vector3 bottomFrontLeft, Vector3 bottomFrontRight, Vector3 bottomBackLeft, Vector3 bottomBackRight)
        {
            BasicEffect = new BasicEffect(Globals.GraphicsDevice);
            BasicEffect.World = Matrix.Identity;
            
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
    
    public void CheckForRay()
    {
        Picked.Clear();
        MouseAction action = InputManager.Instance.GetMouseAction(GameAction.LMB);
        if (action is { state: ActionState.RELEASED, duration: <= 5})
        {
             Ray? ray = CalculateMouseRay(InputManager.Instance.MousePosition);
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
                 if(candidate != null)Picked.Add(candidate);
             }
        } else if (action is { state: ActionState.RELEASED, duration: > 5 })
        {
            PickingFrustum? frustum = CalculatePickingFrustum(action);
            if (frustum.HasValue)
            {
                foreach (Pickable entity in Pickables)
                {
                    if (frustum.Value.Intersects(entity.Renderer._model.BoundingSphere.Transform(entity.ParentObject.Transform.ModelMatrix)))
                    {
                        Picked.Add(entity); 
                    }
                }

                Globals.Renderer.PickingFrustum = frustum;
            }
        }
        Pickables.Clear();
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
                Vector3 topFrontLeft, topFrontRight, topBackLeft, topBackRight, bottomFrontLeft,
                    bottomFrontRight, bottomBackLeft, bottomBackRight;
                topFrontLeft = topLeft.Value.Position + topLeft.Value.Direction * topLeft.Value.Intersects(Globals.BoundingFrustum.Near).Value;
                
                topBackLeft = topLeft.Value.Position + topLeft.Value.Direction * topLeft.Value.Intersects(Globals.BoundingFrustum.Far).Value;

                topFrontRight = topRight.Value.Position + topRight.Value.Direction * topRight.Value.Intersects(Globals.BoundingFrustum.Near).Value;
                
                topBackRight = topRight.Value.Position + topRight.Value.Direction * topRight.Value.Intersects(Globals.BoundingFrustum.Far).Value;
                
                bottomFrontLeft = bottomLeft.Value.Position + bottomLeft.Value.Direction * bottomLeft.Value.Intersects(Globals.BoundingFrustum.Near).Value;
                
                bottomBackLeft = bottomLeft.Value.Position + bottomLeft.Value.Direction * bottomLeft.Value.Intersects(Globals.BoundingFrustum.Far).Value;
                
                bottomFrontRight = bottomRight.Value.Position + bottomRight.Value.Direction * bottomRight.Value.Intersects(Globals.BoundingFrustum.Near).Value;
                
                bottomBackRight = bottomRight.Value.Position + bottomRight.Value.Direction * bottomRight.Value.Intersects(Globals.BoundingFrustum.Far).Value;
                
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
}