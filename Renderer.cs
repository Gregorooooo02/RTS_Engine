using System;
using System.IO;
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
    public static int ShadowMapSize = 256;
    private int currentMultiplier;
    
    private Effect _shadowMapGenerator;
    private Matrix _lightViewProjection;
    
    //Render targets-----------------------------------
    private RenderTarget2D _shadowMapRenderTarget;
    
    //-------------------------------------------------
    
    //Things to render in current frame
    public List<MeshRenderer> Meshes;
    public List<SpiteRenderer> Sprites;
    public List<TextRenderer> Texts;
    public List<AnimatedSpriteRenderer> AnimatedSprites;
    public WorldRenderer WorldMesh;
    //

    public Renderer(ContentManager content)
    {
        _shadowMapRenderTarget = new RenderTarget2D(Globals.GraphicsDevice, ShadowMapSize, ShadowMapSize, true, SurfaceFormat.Single,
            DepthFormat.Depth24);

#if _WINDOWS
        _shadowMapGenerator = content.Load<Effect>("ShadowMaps");
#else
        byte[] bytecode = File.ReadAllBytes("Content/ShadowMaps");
        _shadowMapGenerator = new Effect(Globals.GraphicsDevice, bytecode);
#endif
        Meshes = new List<MeshRenderer>();
        Sprites = new List<SpiteRenderer>();
        Texts = new List<TextRenderer>();
        AnimatedSprites = new List<AnimatedSpriteRenderer>();
        WorldMesh = new WorldRenderer();
#if DEBUG
        _blank = content.Load<Texture2D>("blank");
#endif
    }

    public void Render()
    {
        //TODO: Maybe change Rendering to use parameters from one frame. Now View and Projection that are used are one frame newer then BoundingFrustum.
        //TODO: In that case Renderer scene would be, visually, one frame behind game's logic but, if not changed, there might be
        //TODO: some visual inaccuracies with frustum culling if camera moves too fast.
        Globals.MainEffect.Parameters["View"]?.SetValue(Globals.View);
        Globals.MainEffect.Parameters["Projection"]?.SetValue(Globals.Projection);
        Globals.MainEffect.Parameters["viewPos"]?.SetValue(Globals.ViewPos);
        Globals.MainEffect.Parameters["gamma"]?.SetValue(Globals.Gamma);
        Globals.MainEffect.Parameters["dirLightIntesity"]?.SetValue(Globals.LightIntensity);
        
#if DEBUG
        Globals.GraphicsDevice.DepthStencilState = new DepthStencilState{DepthBufferEnable = true};
        
        if(Globals.DrawShadows) DrawShadows();
        Globals.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,new Color(32,32,32,255), 1.0f,0);
        if(Globals.DrawMeshes) DrawMeshes();
        DrawWorld();
        
        Globals.SpriteBatch.Begin();
        if(Globals.ShowShadowMap)Globals.SpriteBatch.Draw(_shadowMapRenderTarget, new Rectangle(0, 0, 600, 600), Color.White);
        DrawSprites();
        DrawAnimatedSprites();
        DrawText();
        Globals.SpriteBatch.End();

#elif RELEASE
        Globals.GraphicsDevice.DepthStencilState = new DepthStencilState{DepthBufferEnable = true};

        DrawShadows();
        DrawMeshes();
        DrawWorld();

        Globals.SpriteBatch.Begin();
        DrawSprites();
        DrawAnimatedSprites();
        DrawText();
        Globals.SpriteBatch.End();
#endif
    }

    private void DrawAnimatedSprites()
    {
        foreach (AnimatedSpriteRenderer gif in AnimatedSprites)
        {
            gif.Draw();
        }
    }
    
    private void DrawSprites()
    {
        foreach (SpiteRenderer spite in Sprites)
        {
            spite.Draw();
        }
    }

    private void DrawText()
    {
        foreach (TextRenderer text in Texts)
        {
            text.Draw();
        }
    }

    public void Clear()
    {
        Meshes.Clear();
        Sprites.Clear();
        AnimatedSprites.Clear();
        Texts.Clear();
    }
    
    public void PrepareForNextFrame()
    {
        //Clear list of meshes to draw
        Meshes.Clear();
        //Prepare camera frustum for next frame culling
        Globals.BoundingFrustum = new BoundingFrustum(Globals.View * Globals.Projection);
        
#if DEBUG
        if (currentMultiplier != Globals.ShadowMapResolutionMultiplier)
        {
            currentMultiplier = Globals.ShadowMapResolutionMultiplier;
            ShadowMapSize = 256 * (int)Math.Pow(2, currentMultiplier);
            _shadowMapRenderTarget = new RenderTarget2D(Globals.GraphicsDevice, ShadowMapSize, ShadowMapSize, true, SurfaceFormat.Single,
                DepthFormat.Depth24);
        }
#endif
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
        Globals.MainEffect.Parameters["DepthBias"].SetValue(0.005f);
        Globals.MainEffect.Parameters["ShadowMapSize"].SetValue(ShadowMapSize);
        foreach (MeshRenderer renderer in Meshes)
        {
            renderer._model.Draw(renderer.ParentObject.Transform.ModelMatrix);
        }

#endif
    }

    private void DrawWorld()
    {
        WorldMesh.Draw();   
    }
    
    private void DrawShadows()
    {
        Vector3 lightPos = Globals.ViewPos + new Vector3(-60, 15, -60);
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
        foreach (ModelMesh mesh in renderer._model.Models[renderer._model.CurrentModelIndex].Meshes)
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