﻿using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Num = System.Numerics;
using Animation.Animation;

namespace RTS_Engine;

public class Game1 : Game
{
#if DEBUG
    private int _size = 60;
    private readonly Stopwatch _performanceTimer = new Stopwatch();
    private double[] _measurements;
    private int _shiftHead = 0;
    private double currentAvg;
    
    private ImGuiRenderer _imGuiRenderer;
    private Num.Vector3 _position = new Num.Vector3(0,0,10);
    private SceneCamera _sceneCamera;
#endif
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager _sceneManager;
    private bool isFullscreen = false;
    private bool isWireframe = false;

    // Testing animations
    private Model _model;
    private Animations _animations;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
#if DEBUG
        _measurements = new double[_size];
#endif
        
        if (isFullscreen)
        {
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }
        else
        {
            _graphics.PreferredBackBufferWidth = 1440;
            _graphics.PreferredBackBufferHeight = 900;
        }
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Globals.GraphicsDevice = _graphics.GraphicsDevice;
        _sceneManager = new SceneManager();

        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();
        
        GenerateMap.GenerateNoiseTexture();

        Globals.Renderer = new Renderer(Content);
        Globals.PickingManager = new PickingManager();
        FileManager.Initialize();
        InputManager.Initialize();
        Globals.Initialize();
        AssetManager.Initialize(Content);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

#if DEBUG
        _sceneCamera = new SceneCamera(_graphics.GraphicsDevice);
        _sceneCamera.Position = _position;
#endif

        Globals.SpriteBatch = _spriteBatch;

#if _WINDOWS
        Globals.MainEffect = Content.Load<Effect>("PBR_Shader");
        Globals.TerrainEffect = Content.Load<Effect>("Terrain_Shader");
#else
        byte[] bytecode = File.ReadAllBytes("Content/PBR_Shader");
        Globals.MainEffect = new Effect(_graphics.GraphicsDevice, bytecode);

        bytecode = File.ReadAllBytes("Content/Terrain_Shader");
        Globals.TerrainEffect = new Effect(_graphics.GraphicsDevice, bytecode);
#endif
        // Testing animation
        _model = Content.Load<Model>("Dude/dude");
        _animations = _model.GetAnimations();
        var clip = _animations.Clips["Take 001"];
        _animations.SetClip(clip);

        _sceneManager.AddScene(new MapScene());
        _sceneManager.AddScene(new ThirdScene());
    }

    protected override void Update(GameTime gameTime)
    {
#if DEBUG
        _performanceTimer.Start();
        Globals.CameraPosition = _sceneCamera.Position;
#endif
        
        InputManager.Instance.PollInput();
        if (InputManager.Instance.IsActive(GameAction.EXIT)) Exit();
        Globals.Update(gameTime);
        
#if DEBUG
        _sceneCamera.Update(gameTime);
#endif
        _sceneManager.CurrentScene.Update(gameTime);
        Globals.PickingManager.CheckForRay();
        if (Globals.PickingManager.Picked != null)
        {
            Console.WriteLine(Globals.PickingManager.Picked.ParentObject.Name);
        }
        
        // Testing animations
        _animations.Update(gameTime.ElapsedGameTime, true, Matrix.Identity);

        base.Update(gameTime);
    }

    Stopwatch _sw = new Stopwatch();

    protected override void Draw(GameTime gameTime)
    {
        Globals.Renderer.Render();

        Matrix[] transforms = new Matrix[_model.Bones.Count];
        _model.CopyAbsoluteBoneTransformsTo(transforms);

        _sw.Reset();
        _sw.Start();
        foreach (ModelMesh mesh in _model.Meshes)
        {
            foreach (var part in mesh.MeshParts)
            {
                ((BasicEffect)part.Effect).SpecularColor = Vector3.Zero;
                ConfigureEffectMatrices((IEffectMatrices)part.Effect, Matrix.Identity, Globals.View, Globals.Projection);
                ConfigureEffectLighting((IEffectLights)part.Effect);

                part.UpdateVertices(_animations.AnimationTransforms);
            }
        }

#if DEBUG
        _imGuiRenderer.BeforeLayout(gameTime);
        ImGuiLayout();
        _imGuiRenderer.AfterLayout();
#endif
        //Idk if line below is necessary. Shit seems to work even if it's commented out so whatever
        //base.Draw(gameTime);
        Globals.Renderer.PrepareForNextFrame();
        
#if DEBUG
        _measurements[_shiftHead] = (_performanceTimer.ElapsedTicks / (double)Stopwatch.Frequency) * 1000.0;
        if (_shiftHead == _size - 1) currentAvg = AvgFromLastSec();
        _shiftHead++;
        if (_shiftHead == _size) _shiftHead = 0;
        _performanceTimer.Reset();
#endif
    }
    
    private void ConfigureEffectMatrices(IEffectMatrices effect, Matrix world, Matrix view, Matrix projection)
    {
        effect.World = world;
        effect.View = view;
        effect.Projection = projection;
    }

    private void ConfigureEffectLighting(IEffectLights effect)
    {
        effect.EnableDefaultLighting();
        effect.DirectionalLight0.Direction = Vector3.Backward;
        effect.DirectionalLight0.Enabled = true;
        effect.DirectionalLight1.Enabled = false;
        effect.DirectionalLight2.Enabled = false;
    }
    
#if DEBUG
    private double AvgFromLastSec()
    {
        double sum = 0;
        for (int i = 0; i < _size; i++)
        {
            sum += _measurements[i];
        }
        return sum / _size;
    }
    
    protected virtual void ImGuiLayout()
    {

        ImGui.Checkbox("Fullscreen", ref isFullscreen);
        ImGui.Separator();
		ImGui.Checkbox("Wireframe", ref isWireframe);
		ImGui.Separator();
        ImGui.Checkbox("Hierarchy", ref Globals.HierarchyVisible);
        ImGui.Checkbox("Inspector",ref Globals.InspectorVisible);
        ImGui.Checkbox("Scene Selection", ref Globals.SceneSelectionVisible);
        ImGui.Checkbox("Show Shadow Map", ref Globals.ShowShadowMap);
        ImGui.Checkbox("Draw Meshes", ref Globals.DrawMeshes);
        ImGui.Checkbox("Draw Shadows", ref Globals.DrawShadows);
        
        ImGui.SliderFloat("Gamma value", ref Globals.Gamma,0.1f,8);
        ImGui.SliderFloat("Sun Power", ref Globals.LightIntensity,1,50);
        ImGui.SliderInt("Shadow Map Size", ref Globals.ShadowMapResolutionMultiplier, 0, 5);
        ImGui.Text(ImGui.GetIO().Framerate + " FPS");
        ImGui.Text("Average from " + _size +"x: " + Math.Round(currentAvg,4,MidpointRounding.AwayFromZero) + "ms");
        
        
        if (Globals.HierarchyVisible)
        {
            ImGui.Begin("Hierarchy");
            _sceneManager.CurrentScene.DrawHierarchy();
            ImGui.AlignTextToFramePadding();
            ImGui.End();
        }
        if(Globals.InspectorVisible) {
            ImGui.Begin("Inspector");
            Globals.CurrentlySelectedObject?.DrawInspector();
            ImGui.End();
        }
        if (Globals.SceneSelectionVisible) {
            _sceneManager.DrawSelection();
        }
    }
#endif
}
