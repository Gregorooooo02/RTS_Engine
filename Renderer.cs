using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine;

public class Renderer
{
    #if DEBUG
    private Texture2D _blank;
    #endif
    
    //TODO: Clean up this class
    public static int ShadowMapSize = 2048;
    
    private Effect _shadowMapGenerator;
    private Matrix _lightViewProjection;
    
    //Render targets-----------------------------------
    private RenderTarget2D _shadowMapRenderTarget;
    
    
    //-------------------------------------------------
    
    //Things to render in current frame
    public List<MeshRenderer> Meshes;

    //

    public Renderer(ContentManager content)
    {
        _shadowMapRenderTarget = new RenderTarget2D(Globals.GraphicsDevice, ShadowMapSize, ShadowMapSize, true, SurfaceFormat.Single,
            DepthFormat.Depth24);
        _shadowMapGenerator = content.Load<Effect>("ShadowMaps");
        Meshes = new List<MeshRenderer>();
        
        #if DEBUG
        _blank = content.Load<Texture2D>("blank");
#endif
    }

    public void Render()
    {
        
        
#if DEBUG
        if(Globals.DrawShadows) DrawShadows();
        Globals.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,new Color(32,32,32,255), 1.0f,0);
        if(Globals.DrawMeshes) DrawMeshes();
        
        if(!Globals.ShowShadowMap) return;
        Globals.SpriteBatch.Begin();
        Globals.SpriteBatch.Draw(_shadowMapRenderTarget, new Rectangle(0, 0, 600, 600), Color.White);
        Globals.SpriteBatch.End();
#elif RELEASE
        DrawShadows();
        DrawMeshes();
#endif
    }

    public void PrepareForNextFrame()
    {
        Meshes.Clear();
    }
    
    private void DrawMeshes()
    {
#if RELEASE
        Globals.MainEffect.Parameters["ShadowMap"]?.SetValue(_shadowMapRenderTarget);
        Globals.MainEffect.Parameters["dirLightSpace"]?.SetValue(_lightViewProjection);
        Globals.MainEffect.Parameters["DepthBias"].SetValue(0.02f);
        Globals.MainEffect.Parameters["ShadowMapSize"].SetValue(ShadowMapSize);
        foreach (MeshRenderer renderer in Meshes)
        {
            renderer._model.Draw(renderer.ParentObject.Transform.ModelMatrix);
        }
#elif DEBUG
        Globals.MainEffect.Parameters["ShadowMap"]?.SetValue(Globals.DrawShadows ? _shadowMapRenderTarget : _blank);
        Globals.MainEffect.Parameters["dirLightSpace"]?.SetValue(_lightViewProjection);
        Globals.MainEffect.Parameters["DepthBias"].SetValue(0.02f);
        Globals.MainEffect.Parameters["ShadowMapSize"].SetValue(ShadowMapSize);
        foreach (MeshRenderer renderer in Meshes)
        {
            renderer._model.Draw(renderer.ParentObject.Transform.ModelMatrix);
        }

#endif
    }
    
    private void DrawShadows()
    {
        Vector3 lightPos = Globals.viewPos + new Vector3(-60, 15, -60);
        _lightViewProjection = Matrix.CreateLookAt(lightPos, lightPos + new Vector3(0.5f,-1.0f,0.5f), Vector3.Up) *
                               Matrix.CreateOrthographic(200, 200, 0.1f, 700);
        
        Globals.GraphicsDevice.SetRenderTarget(_shadowMapRenderTarget);
        _shadowMapGenerator.Parameters["LightViewProj"].SetValue(_lightViewProjection);
        foreach (MeshRenderer renderer in Meshes)
        {
            DrawShadowMap(renderer);
        }
        Globals.GraphicsDevice.SetRenderTarget(null);
    }

    private void DrawShadowMap(MeshRenderer renderer)
    {
        _shadowMapGenerator.Parameters["World"].SetValue(renderer.ParentObject.Transform.ModelMatrix);
        foreach (ModelMesh mesh in renderer._model.Model.Meshes)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                if (part.PrimitiveCount <= 0) continue;
                _shadowMapGenerator.CurrentTechnique.Passes[0].Apply();
                Globals.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                Globals.GraphicsDevice.Indices = part.IndexBuffer;
                Globals.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                
            }
        } 
    }

}