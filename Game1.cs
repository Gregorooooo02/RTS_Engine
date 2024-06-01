using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using RTS_Engine.Components.AI;
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
    
    Queue<Point> points;
    private Point CurrentPoint;
#endif
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager _sceneManager;
    private bool isFullscreen = false;
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Globals.Content = Content;
        Globals.GraphicsDeviceManager = _graphics;
        

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
        //_sceneManager.AddScene(new MapScene());
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
        
        Globals.FogManager.UpdateFog();

        /*
        try
        {
            Transform transform = _sceneManager.CurrentScene.SceneRoot.Children[2].Transform;
            if (InputManager.Instance.GetMouseAction(GameAction.RMB)?.state == ActionState.RELEASED)
            {
                Vector3? point = Globals.PickingManager.PickGround(InputManager.Instance.MousePosition, 0.1f);
                if (point.HasValue)
                {
                    Console.WriteLine(point.Value);
                    Node start = new Node(new Point((int)transform._pos.X, (int)transform._pos.Z), null, 1);
                    Node goal = new Node(new Point((int)point.Value.X, (int)point.Value.Z), null, 1);
                    
                    points = RTS_Engine.Components.AI.Pathfinding.PathToQueueOfPoints(RTS_Engine.Components.AI.Pathfinding.CalculatePath(goal,start));
                    CurrentPoint = points.Dequeue();
                }
            }

            if (points is not null)
            {
                MapNode node = Globals.Renderer.WorldRenderer.MapNodes[CurrentPoint.X, CurrentPoint.Y];
                Vector3 offset = new Vector3(node.Location.X - transform._pos.X, node.Height - transform._pos.Y,
                    node.Location.Y - transform._pos.Z);
                
                if (offset.Length() <= 0.1f  && points.Count > 0)
                {
                    CurrentPoint = points.Dequeue();
                }
                offset.Normalize();
                transform.Move(offset * 10.0f * Globals.DeltaTime);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        */
        

        base.Update(gameTime);
    }
    
    protected override void Draw(GameTime gameTime)
    {
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
#endif
}
