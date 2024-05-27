﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Microsoft.Xna.Framework.Input;
using Num = System.Numerics;
using RTS.Animation;

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
    
    private Bloom _bloom;
    private int _bloomSettingsIndex = 0;
    
    KeyboardState lastKeyboardState = new KeyboardState();
    KeyboardState currentKeyboardState = new KeyboardState();
        
    GamePadState lastGamePadState = new GamePadState();
    GamePadState currentGamePadState = new GamePadState();
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Globals.Content = Content;
        Globals.GraphicsDeviceManager = _graphics;
        
        // _bloom = new Bloom(this);
        // Components.Add(_bloom);

#if DEBUG
        _measurements = new double[_size];
#endif
        
        if (isFullscreen)
        {
            //Globals.ChangeScreenSize(ScreenSize.FULLSCREEN);
        }
        else
        {
            Globals.ChangeScreenSize(ScreenSize.WINDOWED);
        }
        //IsFixedTimeStep = false;
        //_graphics.SynchronizeWithVerticalRetrace = false;
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Globals.GraphicsDevice = _graphics.GraphicsDevice;
        _sceneManager = new SceneManager();

#if DEBUG
        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();
#endif
        
        GenerateMap.GenerateNoiseTexture();

        Globals.Renderer = new Renderer(Content);
        Globals.PickingManager = new PickingManager(Content);
        Globals.FogManager = new FogManager();
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
#if DEBUG
        _sceneManager.AddScene(new MapScene());
        _sceneManager.AddScene(FileManager.PopulateScene("Menu"));
        _sceneManager.AddScene(FileManager.PopulateScene("BaseScene"));
        //_sceneManager.AddScene(new MapScene());
#elif RELEASE
        _sceneManager.AddScene(FileManager.PopulateScene("Menu"));
        _sceneManager.AddScene(FileManager.PopulateScene("BaseScene"));
#endif
    }

    protected override void Update(GameTime gameTime)
    {
#if DEBUG
        _performanceTimer.Start();
#endif
        InputManager.Instance.PollInput();
        if (InputManager.Instance.IsActive(GameAction.EXIT)) Exit();
        Globals.Update(gameTime);

        _sceneManager.CheckForSceneChanges();
        
#if DEBUG
        if(Globals.DebugCamera)_sceneCamera.Update(gameTime);
#endif
        _sceneManager.CurrentScene.Update(gameTime);
        Globals.PickingManager.CheckForRay();
        
        //if(Globals.FogManager.UpdateFogAsync().IsCompleted)Globals.FogManager.UpdateFogAsync();
        Globals.FogManager.UpdateFog();
        
        
        foreach (Pickable yes in Globals.PickingManager.Picked)
        {
            Console.WriteLine(yes.ParentObject.Name);
        }
        
        HandleInput();

        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        // _bloom.BeginDraw();
        
        Globals.Renderer.Render();
#if DEBUG
        _imGuiRenderer.BeforeLayout(gameTime);
        ImGuiLayout();
        _imGuiRenderer.AfterLayout();
#endif
        Globals.Renderer.PrepareForNextFrame();
        
#if DEBUG
        _measurements[_shiftHead] = (_performanceTimer.ElapsedTicks / (double)Stopwatch.Frequency) * 1000.0;
        if (_shiftHead == _size - 1) currentAvg = AvgFromLastSec();
        _shiftHead++;
        if (_shiftHead == _size) _shiftHead = 0;
        _performanceTimer.Reset();
#endif
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
		ImGui.Checkbox("Wireframe", ref Globals.DrawWireframe);
		ImGui.Separator();
        ImGui.Checkbox("Hierarchy", ref Globals.HierarchyVisible);
        ImGui.Checkbox("Inspector",ref Globals.InspectorVisible);
        ImGui.Checkbox("Scene Selection", ref Globals.SceneSelectionVisible);
        ImGui.Checkbox("Show Shadow Map", ref Globals.ShowShadowMap);
        ImGui.Checkbox("Draw Meshes", ref Globals.DrawMeshes);
        ImGui.Checkbox("Draw Shadows", ref Globals.DrawShadows);
        ImGui.Checkbox("Draw Selection Frustum", ref Globals.DrawSelectFrustum);
        ImGui.Checkbox("Single picking enabled", ref Globals.PickingManager.SinglePickingActive);
        ImGui.Checkbox("Box picking enabled", ref Globals.PickingManager.BoxPickingActive);
        ImGui.Checkbox("Fog Active", ref Globals.FogManager.FogActive);
        ImGui.Checkbox("Draw Explored", ref Globals.DrawExplored);
        ImGui.Checkbox("Draw Visibility", ref Globals.DrawVisibility);
        ImGui.Separator();
        ImGui.Checkbox("Debug camera", ref Globals.DebugCamera);

        ImGui.SliderFloat("Gamma value", ref Globals.Gamma,0.1f,8);
        ImGui.SliderFloat("Sun Power", ref Globals.LightIntensity,1,50);
        ImGui.SliderInt("Shadow Map Size", ref Globals.ShadowMapResolutionMultiplier, 0, 5);
        ImGui.InputInt("Fog resolution factor", ref Globals.FogManager.FogResolution);
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
    
    private void HandleInput()
    {
        lastKeyboardState = currentKeyboardState;
        lastGamePadState = currentGamePadState;

        currentKeyboardState = Keyboard.GetState();
        currentGamePadState = GamePad.GetState(PlayerIndex.One);

        // Check for exit.
        if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
            currentGamePadState.Buttons.Back == ButtonState.Pressed)
        {
            Exit();
        }

        // Switch to the next bloom settings preset?
        if ((currentGamePadState.Buttons.A == ButtonState.Pressed &&
             lastGamePadState.Buttons.A != ButtonState.Pressed) ||
            (currentKeyboardState.IsKeyDown(Keys.F) &&
             lastKeyboardState.IsKeyUp(Keys.F)))
        {
            _bloomSettingsIndex = (_bloomSettingsIndex + 1) %
                                 BloomSettings.PresetSettings.Length;
         
            _bloom.Settings = BloomSettings.PresetSettings[_bloomSettingsIndex];
            _bloom.Visible = true;
        }

        // Toggle bloom on or off?
        if ((currentGamePadState.Buttons.B == ButtonState.Pressed &&
             lastGamePadState.Buttons.B != ButtonState.Pressed) ||
            (currentKeyboardState.IsKeyDown(Keys.B) &&
             lastKeyboardState.IsKeyUp(Keys.B)))
        {
            _bloom.Visible = !_bloom.Visible;
        }

        // Cycle through the intermediate buffer debug display modes?
        if ((currentGamePadState.Buttons.X == ButtonState.Pressed &&
             lastGamePadState.Buttons.X != ButtonState.Pressed) ||
            (currentKeyboardState.IsKeyDown(Keys.X) &&
             lastKeyboardState.IsKeyUp(Keys.X)))
        {
            _bloom.Visible = true;
            _bloom.ShowBuffer++;

            if (_bloom.ShowBuffer > Bloom.IntermediateBuffer.FinalResult)
                _bloom.ShowBuffer= 0;
        }
    }
#endif
}
