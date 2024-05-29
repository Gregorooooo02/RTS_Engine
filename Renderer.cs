using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    public List<AnimatedMeshRenderer> AnimatedMeshes;
    public List<SpiteRenderer> Sprites;
    public List<TextRenderer> Texts;
    public List<AnimatedSpriteRenderer> AnimatedSprites;
    public List<InstancedRendererController> InstancedRendererControllers = new();
    public List<WorldRenderer> WorldRenderers;
    public Puzzle CurrentActivePuzzle;

    public PickingManager.PickingFrustum? PickingFrustum = null;

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
        AnimatedMeshes = new List<AnimatedMeshRenderer>();
        Sprites = new List<SpiteRenderer>();
        Texts = new List<TextRenderer>();
        AnimatedSprites = new List<AnimatedSpriteRenderer>();
        WorldRenderers = new List<WorldRenderer>();
#if DEBUG
        _blank = content.Load<Texture2D>("blank");
#endif
    }
    
    public void Render()
    {
        Globals.MainEffect.Parameters["fogScale"]?.SetValue(1.0f / (Globals.FogManager.TextureSize * Globals.FogManager.FogResolution));
        
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
        Globals.GraphicsDevice.RasterizerState = Globals.DrawWireframe ? Globals.WireFrame : Globals.Solid;
        
        if(Globals.DrawShadows) DrawShadows();
        Globals.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,new Color(32,32,32,255), 1.0f,0);
        if (Globals.DrawMeshes)
        {
            DrawMeshes();
            DrawAnimatedMeshes();
        }
        DrawWorld();
        if (PickingFrustum.HasValue && Globals.DrawSelectFrustum)
        {
            PickingFrustum.Value.DrawFrustum();
        }
        
        Globals.SpriteBatch.Begin(SpriteSortMode.BackToFront);
        if(Globals.DrawExplored)Globals.SpriteBatch.Draw(Globals.FogManager._permanentMaskTarget, new Rectangle(0, 0, 600, 600), Color.White);
        if(Globals.DrawVisibility)Globals.SpriteBatch.Draw(Globals.FogManager._visibilityMaskTarget, new Rectangle(0, 0, 600, 600), Color.White);
        if(Globals.ShowShadowMap)Globals.SpriteBatch.Draw(_shadowMapRenderTarget, new Rectangle(0, 0, 600, 600), Color.White);
        Globals.PickingManager.DrawSelectionBox();
        DrawSprites();
        DrawAnimatedSprites();
        CurrentActivePuzzle?.Draw();
        Globals.SpriteBatch.End();
        Globals.SpriteBatch.Begin();
        DrawText();
        Globals.SpriteBatch.End();

#elif RELEASE
        Globals.GraphicsDevice.DepthStencilState = new DepthStencilState{DepthBufferEnable = true};

        DrawShadows();
        DrawMeshes();
        DrawAnimatedMeshes();
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
        InstancedRendererControllers.Clear();
        AnimatedMeshes.Clear();
        Sprites.Clear();
        AnimatedSprites.Clear();
        Texts.Clear();
        WorldRenderers.Clear();
        CurrentActivePuzzle = null;
    }
    
    public void PrepareForNextFrame()
    {
        //Clear list of meshes to draw
        Meshes.Clear();
        AnimatedMeshes.Clear();
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
        
        Globals.MainEffect.CurrentTechnique = Globals.MainEffect.Techniques["Instancing"];
        foreach (InstancedRendererController instanced in InstancedRendererControllers)
        {
            instanced.Draw();
        }
        Globals.MainEffect.CurrentTechnique = Globals.MainEffect.Techniques["PBR"];
        foreach (MeshRenderer renderer in Meshes)
        {
            renderer._model.Draw(renderer.ParentObject.Transform.ModelMatrix);
        }
#endif
    }

    private void DrawAnimatedMeshes()
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
        // Globals.MainEffect.Parameters["ShadowMap"]?.SetValue(Globals.DrawShadows ? _shadowMapRenderTarget : _blank);
        // Globals.MainEffect.Parameters["dirLightSpace"]?.SetValue(_lightViewProjection);
        // Globals.MainEffect.Parameters["DepthBias"].SetValue(0.005f);
        // Globals.MainEffect.Parameters["ShadowMapSize"].SetValue(ShadowMapSize);
        foreach (AnimatedMeshRenderer renderer in AnimatedMeshes)
        {
            renderer.Draw(renderer.ParentObject.Transform.ModelMatrix);
        }
#endif
    }

    private void DrawWorld()
    {
        foreach (WorldRenderer renderer in WorldRenderers)
        {
            renderer?.Draw();    
        }
    }
    
    private void DrawShadows()
    {
        float sqrtZoom = MathF.Sqrt(Globals.ZoomDegrees);
        float offset = -sqrtZoom * Globals.ZoomDegrees * 0.2f - 45;
        float size = sqrtZoom * Globals.ZoomDegrees * 0.62f;
        Vector3 lightPos = Globals.ViewPos + new Vector3(offset, 15, offset);
        _lightViewProjection = Matrix.CreateLookAt(lightPos, lightPos + new Vector3(0.5f,-1.0f,0.5f), Vector3.Up) *
                               Matrix.CreateOrthographic(size, size, 50.0f, 500 + Globals.ZoomDegrees * 10.5f);
        
        Globals.GraphicsDevice.SetRenderTarget(_shadowMapRenderTarget);
        _shadowMapGenerator.Parameters["LightViewProj"].SetValue(_lightViewProjection);
        _shadowMapGenerator.CurrentTechnique = _shadowMapGenerator.Techniques["ShadowInstanced"];
        foreach (InstancedRendererController controller in InstancedRendererControllers)
        {
            DrawShadowMapInstanced(controller);
        }
        _shadowMapGenerator.CurrentTechnique = _shadowMapGenerator.Techniques["CreateShadowMap"];
        foreach (MeshRenderer renderer in Meshes)
        {
            DrawShadowMap(renderer);
        }

        foreach (AnimatedMeshRenderer renderer in AnimatedMeshes)
        {
            DrawShadowMap(renderer);
        }
        Globals.GraphicsDevice.SetRenderTarget(null);
    }

    private void DrawShadowMapInstanced(InstancedRendererController rendererController)
    {
        if(rendererController.CurrentIndex == 0 || !rendererController.Active) return;
        DynamicVertexBuffer instanceVertexBuffer = new DynamicVertexBuffer(Globals.GraphicsDevice,
            Globals.ShadowInstanceDeclaration, rendererController.CurrentIndex, BufferUsage.WriteOnly);
        instanceVertexBuffer.SetData(rendererController.Matrices,0,rendererController.CurrentIndex, SetDataOptions.Discard);
        
        foreach (ModelMesh mesh in rendererController.ModelData.Models[rendererController.ModelData.CurrentModelIndex].Meshes)
        {
            foreach (ModelMeshPart part in mesh.MeshParts)
            {
                if (part.PrimitiveCount <= 0) continue;
                Globals.GraphicsDevice.SetVertexBuffers(
                    new VertexBufferBinding(part.VertexBuffer, part.VertexOffset,0),
                    new VertexBufferBinding(instanceVertexBuffer,0,1)
                );
                Globals.GraphicsDevice.Indices = part.IndexBuffer;
                _shadowMapGenerator.CurrentTechnique.Passes[0].Apply();
                Globals.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, part.StartIndex, part.PrimitiveCount, rendererController.CurrentIndex);
            }
        } 
    }
    
    private void DrawShadowMap(MeshRenderer renderer)
    {
        _shadowMapGenerator.Parameters["World"].SetValue(renderer.ParentObject.Transform.ModelMatrix);
        foreach (ModelMesh mesh in renderer._model.Models[renderer._model.CurrentModelIndex].Meshes)
        {
            //foreach (ModelMeshPart part in mesh.MeshParts)
            for(int i = 0;i < mesh.MeshParts.Count;i++)
            {
                var part = mesh.MeshParts[i];
                if (part.PrimitiveCount <= 0) continue;
                _shadowMapGenerator.CurrentTechnique.Passes[0].Apply();
                Globals.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                Globals.GraphicsDevice.Indices = part.IndexBuffer;
                Globals.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
            }
        } 
    }

    private void DrawShadowMap(AnimatedMeshRenderer renderer)
    {
        _shadowMapGenerator.Parameters["World"].SetValue(renderer.ParentObject.Transform.ModelMatrix);
        foreach (ModelMesh mesh in renderer._model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
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