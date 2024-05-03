﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Num = System.Numerics;
using System.IO;
using System.Linq;

namespace RTS_Engine;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager _sceneManager;
    private SceneCamera _sceneCamera;
    private Num.Vector3 _position = new Num.Vector3(0,0,10);
    
    private ImGuiRenderer _imGuiRenderer;
    private Num.Vector3 _clearColor = new Num.Vector3(0.0f, 0.0f, 0.0f);

    private bool isFullscreen = false;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

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

        FileManager.Initialize();
        InputManager.Initialize();
        Globals.Initialize();
        AssetManager.Initialize(Content);
        Globals.Renderer = new Renderer(Content);
        
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
#else
        byte[] bytecode = File.ReadAllBytes("Content/TesEffectComp");
        Globals.TestEffect = new Effect(_graphics.GraphicsDevice, bytecode);
#endif
        _sceneManager.AddScene(new SecondScene());
        _sceneManager.AddScene(new MapScene());
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Instance.PollInput();
        if (InputManager.Instance.IsActive(GameAction.EXIT)) Exit();
        Globals.Update(gameTime);
        
#if DEBUG
        _sceneCamera.Update(gameTime);
#endif
        _sceneManager.CurrentScene.Update(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.DepthStencilState = new DepthStencilState{DepthBufferEnable = true};
        Globals.Renderer.Render();
        
#if DEBUG
        _imGuiRenderer.BeforeLayout(gameTime);
        ImGuiLayout();
        _imGuiRenderer.AfterLayout();
#endif
        base.Draw(gameTime);
        Globals.Renderer.PrepareForNextFrame();
    }
    
    
    
#if DEBUG
    protected virtual void ImGuiLayout()
    {

        ImGui.Checkbox("Fullscreen", ref isFullscreen);
        ImGui.Separator();
        ImGui.Checkbox("Hierarchy", ref Globals.HierarchyVisible);
        ImGui.Checkbox("Inspector",ref Globals.InspectorVisible);
        ImGui.Checkbox("Scene Selection", ref Globals.SceneSelectionVisible);
        ImGui.Checkbox("Map Modifier", ref Globals.MapModifyVisible);
        ImGui.Checkbox("Show Shadow Map", ref Globals.ShowShadowMap);
        ImGui.Checkbox("Draw Meshes", ref Globals.DrawMeshes);
        ImGui.Checkbox("Draw Shadows", ref Globals.DrawShadows);


        ImGui.ColorEdit3("Background Color", ref _clearColor);
        ImGui.SliderFloat("Gamma value", ref Globals.Gamma,0.1f,8);
        ImGui.SliderFloat("Sun Power", ref Globals.LightIntensity,1,50);
        ImGui.SliderInt("Shadow Map Size", ref Globals.ShadowMapResolutionMultiplier, 0, 5);
        ImGui.Text(ImGui.GetIO().Framerate + " FPS");
        
        
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
        if (Globals.MapModifyVisible) {
            GenerateMap.MapInspector();
        }

    }
#endif
}
