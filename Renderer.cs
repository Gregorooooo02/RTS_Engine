using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RTS_Engine.Components.AI;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine;

public class Renderer
{
    #if DEBUG
    private Texture2D _blank;
    #endif
    
    //TODO: Clean up this class
    public static int ShadowMapSize = 4096;
    public static int FloodPasses = -1;
    private readonly float _floodThreshold = 35.0f;
    private bool _target1 = false;
    private bool _enemyTarget1 = false;
    
    private Effect _shadowMapGenerator;
    private Effect _outlineGenerator;
    private Effect _postprocessMerge;
    private Matrix _lightViewProjection;
    
    //Render targets-----------------------------------
    private RenderTarget2D _sceneRenderTarget;
    private RenderTarget2D _shadowMapRenderTarget;
    private RenderTarget2D _outlineTargetAlly1;
    private RenderTarget2D _outlineTargetAlly2;
    private RenderTarget2D _outlineTargetEnemy1;
    private RenderTarget2D _outlineTargetEnemy2;
    
    //-------------------------------------------------
    
    //Things to render in current frame
    public List<MeshRenderer> Meshes;
    public List<AnimatedMeshRenderer> AnimatedMeshes;
    public List<SpiteRenderer> Sprites;
    public List<TextRenderer> Texts;
    public List<AnimatedSpriteRenderer> AnimatedSprites;
    public List<InstancedRendererController> InstancedRendererControllers = new();
    public WorldRenderer WorldRenderer;
    public Puzzle CurrentActivePuzzle;

    public PickingManager.PickingFrustum? PickingFrustum = null;

    public Renderer(ContentManager content)
    {
        int width = Globals.GraphicsDevice.PresentationParameters.BackBufferWidth;
        int height = Globals.GraphicsDevice.PresentationParameters.BackBufferHeight;
        
        _sceneRenderTarget = new RenderTarget2D(Globals.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
        _shadowMapRenderTarget = new RenderTarget2D(Globals.GraphicsDevice, ShadowMapSize, ShadowMapSize, true, SurfaceFormat.Single,
            DepthFormat.Depth24);
        _outlineTargetAlly1 = new RenderTarget2D(Globals.GraphicsDevice, width, height, false, SurfaceFormat.Bgr565,
            DepthFormat.Depth16);
        _outlineTargetAlly2 = new RenderTarget2D(Globals.GraphicsDevice, width, height, false, SurfaceFormat.Bgr565,
            DepthFormat.Depth16);
        _outlineTargetEnemy1 = new RenderTarget2D(Globals.GraphicsDevice, width, height, false, SurfaceFormat.Bgr565,
            DepthFormat.Depth16);
        _outlineTargetEnemy2 = new RenderTarget2D(Globals.GraphicsDevice, width, height, false, SurfaceFormat.Bgr565,
            DepthFormat.Depth16);

#if _WINDOWS
        _shadowMapGenerator = content.Load<Effect>("ShadowMaps");
        _outlineGenerator = content.Load<Effect>("Outlines");
        _postprocessMerge = content.Load<Effect>("MergeShader");
#else
        byte[] bytecode = File.ReadAllBytes("Content/ShadowMaps");
        _shadowMapGenerator = new Effect(Globals.GraphicsDevice, bytecode);
        
        bytecode = File.ReadAllBytes("Content/Outlines");
        _outlineGenerator = new Effect(Globals.GraphicsDevice, bytecode);
        
        bytecode = File.ReadAllBytes("Content/MergeShader");
        _postprocessMerge = new Effect(Globals.GraphicsDevice, bytecode);
#endif
        Meshes = new List<MeshRenderer>();
        AnimatedMeshes = new List<AnimatedMeshRenderer>();
        Sprites = new List<SpiteRenderer>();
        Texts = new List<TextRenderer>();
        AnimatedSprites = new List<AnimatedSpriteRenderer>();
        WorldRenderer = null;

        _postprocessMerge.Parameters["AllyColor"].SetValue(new Color(255,255,255,255).ToVector4());
        _postprocessMerge.Parameters["EnemyColor"].SetValue(new Color(255,0,0,255).ToVector4());
#if DEBUG
        _blank = content.Load<Texture2D>("blank");
#endif
    }

    private void Resize()
    {
        int width = Globals.GraphicsDevice.PresentationParameters.BackBufferWidth;
        int height = Globals.GraphicsDevice.PresentationParameters.BackBufferHeight;
        _sceneRenderTarget = new RenderTarget2D(Globals.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.Depth24);
        _outlineTargetAlly1 = new RenderTarget2D(Globals.GraphicsDevice, width, height, false, SurfaceFormat.Bgr565,
            DepthFormat.Depth16);
        _outlineTargetAlly2 = new RenderTarget2D(Globals.GraphicsDevice, width, height, false, SurfaceFormat.Bgr565,
            DepthFormat.Depth16);
        _outlineTargetEnemy1 = new RenderTarget2D(Globals.GraphicsDevice, width, height, false, SurfaceFormat.Bgr565,
            DepthFormat.Depth16);
        _outlineTargetEnemy2 = new RenderTarget2D(Globals.GraphicsDevice, width, height, false, SurfaceFormat.Bgr565,
            DepthFormat.Depth16);
    }
    
    public void Render()
    {
        if (Globals.GraphicsDeviceManager.PreferredBackBufferWidth != _sceneRenderTarget.Width) Resize();
        
        Globals.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,new Color(32,32,32,255), 1.0f,0);
        
        Globals.MainEffect.Parameters["View"]?.SetValue(Globals.View);
        Globals.MainEffect.Parameters["Projection"]?.SetValue(Globals.Projection);
        Globals.MainEffect.Parameters["viewPos"]?.SetValue(Globals.ViewPos);
        Globals.MainEffect.Parameters["gamma"]?.SetValue(Globals.Gamma);
#if DEBUG
        
        RenderOutlines();
        
        Globals.GraphicsDevice.DepthStencilState = new DepthStencilState{DepthBufferEnable = true};
        Globals.GraphicsDevice.RasterizerState = Globals.DrawWireframe ? Globals.WireFrame : Globals.Solid;
        
        //--------------------Render the shadow map--------------------
        if(Globals.DrawShadows) DrawShadows();
        //-------------------------------------------------------------
        
        //--------------------Render the scene into the render target--------------------
        Globals.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
        Globals.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,new Color(32,32,32,255), 1.0f,0);
        if (Globals.DrawMeshes)
        {
            DrawMeshes();
            DrawAnimatedMeshes();
            DrawWorld();
        }
        if (PickingFrustum.HasValue && Globals.DrawSelectFrustum)
        {
            PickingFrustum.Value.DrawFrustum();
        }
        //--------------------------------------------------------------------------------
        
        //--------------------Draw rendered scene target to screen while applying postprocessing--------------------
        Globals.GraphicsDevice.SetRenderTarget(null);
        if (Globals.AgentsManager.SelectedUnits.Count > 0)
        {
            _postprocessMerge.Parameters["AllyOutlineMask"].SetValue(_target1 ? _outlineTargetAlly2 : _outlineTargetAlly1);
            _postprocessMerge.Parameters["EnemyOutlineMask"].SetValue(_enemyTarget1 ? _outlineTargetEnemy2 : _outlineTargetEnemy1);
            Globals.SpriteBatch.Begin(SpriteSortMode.Immediate,null,null,null,null,_postprocessMerge);
            Globals.SpriteBatch.Draw(_sceneRenderTarget,Vector2.Zero,Color.White);
            Globals.SpriteBatch.End();
        }
        else
        {
            Globals.SpriteBatch.Begin(SpriteSortMode.Immediate);
            Globals.SpriteBatch.Draw(_sceneRenderTarget,Vector2.Zero,Color.White);
            Globals.SpriteBatch.End();
        }
        
        //-----------------------------------------------------------------------------------------------------------
        
        //--------------------Draw sprites--------------------
        Globals.SpriteBatch.Begin(SpriteSortMode.BackToFront);
        if(Globals.ShowSelectedSilhouettes) Globals.SpriteBatch.Draw(_outlineTargetEnemy1, new Rectangle(0, 0, 600, 600), Color.White);
        if(Globals.DrawExplored)Globals.SpriteBatch.Draw(Globals.FogManager.PermanentMaskTarget, new Rectangle(0, 0, 600, 600), Color.White);
        if(Globals.DrawVisibility)Globals.SpriteBatch.Draw(Globals.FogManager.VisibilityMaskTarget, new Rectangle(0, 0, 600, 600), Color.White);
        if(Globals.ShowShadowMap)Globals.SpriteBatch.Draw(_shadowMapRenderTarget, new Rectangle(0, 0, 600, 600), Color.White);
        Globals.PickingManager.DrawSelectionBox();
        DrawSprites();
        DrawAnimatedSprites();
        CurrentActivePuzzle?.Draw();
        Globals.SpriteBatch.End();
        //----------------------------------------------------
        
        //--------------------Draw text--------------------
        Globals.SpriteBatch.Begin();
        DrawText();
        Globals.SpriteBatch.End();
        //-------------------------------------------------
#elif RELEASE
        Globals.GraphicsDevice.DepthStencilState = new DepthStencilState{DepthBufferEnable = true};

        //--------------------Render the shadow map--------------------
        DrawShadows();
        //-------------------------------------------------------------
        
        //--------------------Render the scene into the render target--------------------
        Globals.GraphicsDevice.SetRenderTarget(_sceneRenderTarget);
        Globals.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer,new Color(32,32,32,255), 1.0f,0);
        DrawMeshes();
        DrawAnimatedMeshes();
        DrawWorld();
        Globals.GraphicsDevice.SetRenderTarget(null);
        //--------------------------------------------------------------------------------

        //--------------------Draw rendered scene target to screen while applying postprocessing--------------------
        Globals.SpriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,_postprocessMerge);
        Globals.SpriteBatch.Draw(_sceneRenderTarget,Vector2.Zero,Color.White);
        Globals.SpriteBatch.End();
        //-----------------------------------------------------------------------------------------------------------

        //--------------------Draw sprites--------------------
        Globals.SpriteBatch.Begin(SpriteSortMode.BackToFront);
        Globals.PickingManager.DrawSelectionBox();
        DrawSprites();
        DrawAnimatedSprites();
        CurrentActivePuzzle?.Draw();
        Globals.SpriteBatch.End();
        //----------------------------------------------------

        //--------------------Draw text--------------------
        Globals.SpriteBatch.Begin();
        DrawText();
        Globals.SpriteBatch.End();
        //-------------------------------------------------
#endif
    }

    private void RenderOutlines()
    {
        //Render outlines for selected player units
        if (Globals.AgentsManager.SelectedUnits.Count > 0)
        {
            //FloodPasses = Globals.ZoomDegrees < _floodThreshold ? 0 : -1;
            Globals.GraphicsDevice.SetRenderTarget(_outlineTargetAlly1);
            _outlineGenerator.CurrentTechnique = _outlineGenerator.Techniques["Silhouettes"];
            _outlineGenerator.Parameters["Projection"].SetValue(Globals.Projection);
            _outlineGenerator.Parameters["View"].SetValue(Globals.View);
            foreach (Agent agent in Globals.AgentsManager.SelectedUnits)
            {
                if (agent.Renderer != null)
                {
                    _outlineGenerator.Parameters["World"].SetValue(agent.ParentObject.Transform.ModelMatrix);
                    foreach (ModelMesh mesh in agent.Renderer._model.Models[agent.Renderer._model.CurrentModelIndex].Meshes)
                    {
                        //foreach (ModelMeshPart part in mesh.MeshParts)
                        for(int i = 0;i < mesh.MeshParts.Count;i++)
                        {
                            var part = mesh.MeshParts[i];
                            if (part.PrimitiveCount <= 0) continue;
                            _outlineGenerator.CurrentTechnique.Passes[0].Apply();
                            Globals.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                            Globals.GraphicsDevice.Indices = part.IndexBuffer;
                            Globals.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                        }
                    } 
                }
            }
            Globals.GraphicsDevice.SetRenderTarget(_outlineTargetAlly2);
            _outlineGenerator.CurrentTechnique = _outlineGenerator.Techniques["UVExtract"];
            Globals.SpriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,_outlineGenerator);
            Globals.SpriteBatch.Draw(_outlineTargetAlly1,Vector2.Zero,Color.White);
            Globals.SpriteBatch.End();
            Globals.GraphicsDevice.SetRenderTarget(_outlineTargetAlly1);
            _outlineGenerator.CurrentTechnique = _outlineGenerator.Techniques["Init"];
            _outlineGenerator.Parameters["parameters"]?.SetValue(new Vector4(0,0, 1.0f / _outlineTargetAlly1.Width, 1.0f / _outlineTargetAlly1.Height));
            Globals.SpriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,_outlineGenerator);
            Globals.SpriteBatch.Draw(_outlineTargetAlly2,Vector2.Zero,Color.White);
            Globals.SpriteBatch.End();
            _outlineGenerator.CurrentTechnique = _outlineGenerator.Techniques["JumpFlood"];
            _target1 = false;
            for (int i = FloodPasses; i >= 0; i--)
            {
                _outlineGenerator.Parameters["StepWidth"]?.SetValue(MathF.Pow(2, i) + 0.2f);
                if (_target1)
                {
                    _target1 = false;
                    Globals.GraphicsDevice.SetRenderTarget(_outlineTargetAlly1);
                    Globals.SpriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,_outlineGenerator);
                    Globals.SpriteBatch.Draw(_outlineTargetAlly2,Vector2.Zero,Color.White);
                    Globals.SpriteBatch.End();
                }
                else
                {
                    _target1 = true;
                    Globals.GraphicsDevice.SetRenderTarget(_outlineTargetAlly2);
                    Globals.SpriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,_outlineGenerator);
                    Globals.SpriteBatch.Draw(_outlineTargetAlly1,Vector2.Zero,Color.White);
                    Globals.SpriteBatch.End();
                }
            }
            Globals.GraphicsDevice.SetRenderTarget(_outlineTargetEnemy1);
            _outlineGenerator.CurrentTechnique = _outlineGenerator.Techniques["Silhouettes"];
            foreach (Agent agent in Globals.AgentsManager.SelectedUnits)
            {
                PlayerUnitData data = (PlayerUnitData)agent.AgentData;
                if (data.Target is { Renderer: not null })
                {
                    _outlineGenerator.Parameters["World"].SetValue(data.Target.ParentObject.Transform.ModelMatrix);
                    foreach (ModelMesh mesh in data.Target.Renderer._model.Models[data.Target.Renderer._model.CurrentModelIndex].Meshes)
                    {
                        //foreach (ModelMeshPart part in mesh.MeshParts)
                        for(int i = 0;i < mesh.MeshParts.Count;i++)
                        {
                            var part = mesh.MeshParts[i];
                            if (part.PrimitiveCount <= 0) continue;
                            _outlineGenerator.CurrentTechnique.Passes[0].Apply();
                            Globals.GraphicsDevice.SetVertexBuffer(part.VertexBuffer);
                            Globals.GraphicsDevice.Indices = part.IndexBuffer;
                            Globals.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, part.VertexOffset, part.StartIndex, part.PrimitiveCount);
                        }
                    } 
                }
            }
            Globals.GraphicsDevice.SetRenderTarget(_outlineTargetEnemy2);
            _outlineGenerator.CurrentTechnique = _outlineGenerator.Techniques["UVExtract"];
            Globals.SpriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,_outlineGenerator);
            Globals.SpriteBatch.Draw(_outlineTargetEnemy1,Vector2.Zero,Color.White);
            Globals.SpriteBatch.End();
            Globals.GraphicsDevice.SetRenderTarget(_outlineTargetEnemy1);
            _outlineGenerator.CurrentTechnique = _outlineGenerator.Techniques["Init"];
            _outlineGenerator.Parameters["parameters"]?.SetValue(new Vector4(0,0, 1.0f / _outlineTargetAlly1.Width, 1.0f / _outlineTargetAlly1.Height));
            Globals.SpriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,_outlineGenerator);
            Globals.SpriteBatch.Draw(_outlineTargetEnemy2,Vector2.Zero,Color.White);
            Globals.SpriteBatch.End();
            _outlineGenerator.CurrentTechnique = _outlineGenerator.Techniques["JumpFlood"];
            _enemyTarget1 = false;
            for (int i = FloodPasses; i >= 0; i--)
            {
                _outlineGenerator.Parameters["StepWidth"]?.SetValue(MathF.Pow(2, i) + 0.2f);
                if (_enemyTarget1)
                {
                    _enemyTarget1 = false;
                    Globals.GraphicsDevice.SetRenderTarget(_outlineTargetEnemy1);
                    Globals.SpriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,_outlineGenerator);
                    Globals.SpriteBatch.Draw(_outlineTargetEnemy2,Vector2.Zero,Color.White);
                    Globals.SpriteBatch.End();
                }
                else
                {
                    _enemyTarget1 = true;
                    Globals.GraphicsDevice.SetRenderTarget(_outlineTargetEnemy2);
                    Globals.SpriteBatch.Begin(SpriteSortMode.Deferred,null,null,null,null,_outlineGenerator);
                    Globals.SpriteBatch.Draw(_outlineTargetEnemy1,Vector2.Zero,Color.White);
                    Globals.SpriteBatch.End();
                }
            }
        }
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
        WorldRenderer = null;
        CurrentActivePuzzle = null;
    }
    
    public void PrepareForNextFrame()
    {
        //Clear list of meshes to draw
        Meshes.Clear();
        AnimatedMeshes.Clear();
        //Prepare camera frustum for next frame culling
        Globals.BoundingFrustum = new BoundingFrustum(Globals.View * Globals.Projection);
    }
    
    private void DrawMeshes()
    {
#if RELEASE
        Globals.MainEffect.Parameters["ShadowMap"]?.SetValue(_shadowMapRenderTarget);
        Globals.MainEffect.Parameters["dirLightSpace"]?.SetValue(_lightViewProjection);
        //Globals.MainEffect.Parameters["DepthBias"].SetValue(0.02f);
        //Globals.MainEffect.Parameters["ShadowMapSize"].SetValue(ShadowMapSize);

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
#elif DEBUG
        Globals.MainEffect.Parameters["ShadowMap"]?.SetValue(Globals.DrawShadows ? _shadowMapRenderTarget : _blank);
        Globals.MainEffect.Parameters["dirLightSpace"]?.SetValue(_lightViewProjection);
        // Globals.MainEffect.Parameters["DepthBias"]?.SetValue(0.005f);
        // Globals.MainEffect.Parameters["ShadowMapSize"].SetValue(ShadowMapSize);
        
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
        //Globals.MainEffect.Parameters["DepthBias"].SetValue(0.02f);
        //Globals.MainEffect.Parameters["ShadowMapSize"].SetValue(ShadowMapSize);
        foreach (MeshRenderer renderer in Meshes)
        {
            renderer._model.Draw(renderer.ParentObject.Transform.ModelMatrix);
        }
#elif DEBUG
        Globals.MainEffect.Parameters["ShadowMap"]?.SetValue(Globals.DrawShadows ? _shadowMapRenderTarget : _blank);
        Globals.MainEffect.Parameters["dirLightSpace"]?.SetValue(_lightViewProjection);
        // Globals.MainEffect.Parameters["DepthBias"].SetValue(0.005f);
        // Globals.MainEffect.Parameters["ShadowMapSize"].SetValue(ShadowMapSize);
        
        Globals.MainEffect.CurrentTechnique = Globals.MainEffect.Techniques["PBR"];
        foreach (AnimatedMeshRenderer renderer in AnimatedMeshes)
        {
            renderer._skinnedModel.Draw(renderer.ParentObject.Transform.ModelMatrix);
        }
#endif
    }

    private void DrawWorld()
    {
        if (WorldRenderer != null)
        {
            Globals.TerrainEffect.Parameters["ShadowMap"]?.SetValue(Globals.DrawShadows ? _shadowMapRenderTarget : _blank);
            Globals.TerrainEffect.Parameters["dirLightSpace"]?.SetValue(_lightViewProjection);
            Globals.TerrainEffect.Parameters["gamma"]?.SetValue(Globals.Gamma);
            WorldRenderer.Draw();    
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
        WorldRenderer?.DrawShadows(_shadowMapGenerator);
        _shadowMapGenerator.CurrentTechnique = _shadowMapGenerator.Techniques["CreateShadowMap"];
        foreach (MeshRenderer renderer in Meshes)
        {
            DrawShadowMap(renderer);
        }
        
        _shadowMapGenerator.CurrentTechnique = _shadowMapGenerator.Techniques["ShadowSkinned"];
        foreach (AnimatedMeshRenderer renderer in AnimatedMeshes)
        {
            DrawShadowMap(renderer);
        }
        _shadowMapGenerator.CurrentTechnique = _shadowMapGenerator.Techniques["ShadowInstanced"];
        foreach (InstancedRendererController controller in InstancedRendererControllers)
        {
            DrawShadowMapInstanced(controller);
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
            for (int i = 0;i < mesh.MeshParts.Count;i++)
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
        _shadowMapGenerator.Parameters["BoneTransforms"].SetValue(renderer._skinnedModel.AnimationController.SkinnedBoneTransforms);
        foreach (ModelMesh mesh in renderer._skinnedModel.SkinnedModels[renderer._skinnedModel.CurrentModelIndex].Model.Meshes)
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