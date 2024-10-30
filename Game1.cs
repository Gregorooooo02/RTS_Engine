using System;
using System.IO;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Microsoft.Xna.Framework.Audio;
using Num = System.Numerics;

namespace RTS_Engine;

public sealed class Game1 : Game
{
#if DEBUG
    private readonly int _size = 60;
    private readonly Stopwatch _performanceTimer = new();
    private readonly double[] _measurements;
    private int _shiftHead;
    private double _currentAvg;
    
    private readonly Num.Vector3 _position = new(0,0,10);
    private SceneCamera _sceneCamera;
#endif

    private ImGuiRenderer _imGuiRenderer;
    private float minFps = float.MaxValue;
    private float maxFps = float.MinValue;
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager _sceneManager;
    private bool _startInFullscreen = false;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Globals.Content = Content;
        Globals.GraphicsDeviceManager = _graphics;
#if DEBUG
        _measurements = new double[_size];
#endif

        Globals.ChangeScreenSize(_startInFullscreen ? ScreenSize.FULLSCREEN : ScreenSize.WINDOWED);
        //IsFixedTimeStep = false;
        //_graphics.SynchronizeWithVerticalRetrace = false;
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Globals.GraphicsDevice = _graphics.GraphicsDevice;
        _sceneManager = new SceneManager();
        

//#if DEBUG
        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();
//#endif
        
        GenerateMap.GenerateNoiseTexture();

        Globals.Renderer = new Renderer(Content);
        Globals.PickingManager = new PickingManager(Content);
        Globals.FogManager = new FogManager();
        Globals.AgentsManager = new AgentsManager();
        FileManager.Initialize();
        InputManager.Initialize();
        Globals.Initialize();
        AssetManager.Initialize(Content);
        base.Initialize();
        SoundEffect.DistanceScale = 5.0f;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

#if DEBUG
        _sceneCamera = new SceneCamera(_graphics.GraphicsDevice)
        {
            Position = _position
        };
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
        //_sceneManager.AddScene(new MapScene());
        _sceneManager.AddScene(FileManager.PopulateScene("Menu"));
        _sceneManager.AddScene(FileManager.PopulateScene("Loading Scene"));
        _sceneManager.AddScene(FileManager.PopulateScene("BaseScene"));
        // _sceneManager.AddScene(new MapScene());
#elif RELEASE
        _sceneManager.AddScene(FileManager.PopulateScene("Menu"));
        _sceneManager.AddScene(FileManager.PopulateScene("Loading Scene"));
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
        Globals.AgentsManager.ProjectileManager.UpdateProjectiles();
        
        _sceneManager.CurrentScene.Update(gameTime);
        
        Globals.FogManager.UpdateFog();
        
        Globals.AgentsManager.CheckForOrders();
        
        Globals.PickingManager.CheckForBuildingSelection();
        Globals.PickingManager.CheckForBuiltBuildingSelection();
        Globals.PickingManager.CheckForMissionSelection();
        
        Globals.PickingManager.Pickables.Clear();
        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
        Globals.Renderer.Render();
#if DEBUG
        _imGuiRenderer.BeforeLayout(gameTime);
        ImGuiLayout();
        _imGuiRenderer.AfterLayout();
#else   
        _imGuiRenderer.BeforeLayout(gameTime);
        ImGui.Begin("Framerate info");
        float framerate = ImGui.GetIO().Framerate;
        minFps = Math.Min(framerate, minFps);
        maxFps = Math.Max(framerate, maxFps);
        ImGui.Text(framerate + " FPS");
        ImGui.Text("Min: " + minFps + " FPS");
        ImGui.Text("Max: " + maxFps + " FPS");
        ImGui.End();
        _imGuiRenderer.AfterLayout();
#endif
        Globals.Renderer.PrepareForNextFrame();
#if DEBUG
        _measurements[_shiftHead] = (_performanceTimer.ElapsedTicks / (double)Stopwatch.Frequency) * 1000.0;
        if (_shiftHead == _size - 1) _currentAvg = AvgFromLastSec();
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

    private float Volume;
    
    private void ImGuiLayout()
    {

        ImGui.Checkbox("Fullscreen", ref _startInFullscreen);
        ImGui.Separator();
		ImGui.Checkbox("Wireframe", ref Globals.DrawWireframe);
		ImGui.Separator();
        ImGui.Checkbox("Hierarchy", ref Globals.HierarchyVisible);
        ImGui.Checkbox("Inspector",ref Globals.InspectorVisible);
        ImGui.Checkbox("Scene Selection", ref Globals.SceneSelectionVisible);
        ImGui.Checkbox("Cheat Menu", ref Globals.CheatMenuVisible);
        ImGui.Checkbox("Show Shadow Map", ref Globals.ShowShadowMap);
        ImGui.Checkbox("Show Selected Silhouettes", ref Globals.ShowSelectedSilhouettes);
        ImGui.Checkbox("Draw Meshes", ref Globals.DrawMeshes);
        ImGui.Checkbox("Draw Shadows", ref Globals.DrawShadows);
        ImGui.Checkbox("Draw Selection Frustum", ref Globals.DrawSelectFrustum);
        ImGui.Checkbox("Single picking enabled", ref Globals.PickingManager.SinglePickingActive);
        ImGui.Checkbox("Box picking enabled", ref Globals.PickingManager.BoxPickingActive);
        ImGui.Checkbox("Enemy picking enabled", ref Globals.PickingManager.EnemyPickingActive);
        ImGui.Checkbox("Ground picking enabled", ref Globals.PickingManager.GroundPickingActive);
        ImGui.Checkbox("Fog Active", ref Globals.FogManager.FogActive);
        ImGui.Checkbox("Draw Explored", ref Globals.DrawExplored);
        ImGui.Checkbox("Draw Visibility", ref Globals.DrawVisibility);
        ImGui.Separator();
        ImGui.Checkbox("Debug camera", ref Globals.DebugCamera);
        ImGui.Checkbox("Pause active", ref Globals.IsPaused);

        ImGui.SliderFloat("Gamma value", ref Globals.Gamma,0.1f,8);
        Volume = SoundEffect.MasterVolume;
        ImGui.SliderFloat("Master volume", ref Volume, 0,1);
        SoundEffect.MasterVolume = Volume;
        ImGui.Text("Master volume: " + SoundEffect.MasterVolume);
        ImGui.Text(ImGui.GetIO().Framerate + " FPS");
        ImGui.Text("Average from " + _size +"x: " + Math.Round(_currentAvg,4,MidpointRounding.AwayFromZero) + "ms");
        
        
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
        if (Globals.CheatMenuVisible)
        {
            GameManager.CheatMenu();
        }
    }
#endif
}
