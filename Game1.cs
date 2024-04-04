using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Num = System.Numerics;

namespace RTS_Engine;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager _sceneManager;
    private SceneCamera _sceneCamera;
    private BasicEffect _basicEffect;
    
    private Num.Vector3 _position = new Num.Vector3(0,0,10);
    
    private ImGuiRenderer _imGuiRenderer;
    private Num.Vector3 _clearColor = new Num.Vector3(0.0f, 0.0f, 0.0f);
    
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1440;
        _graphics.PreferredBackBufferHeight = 900;
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _basicEffect = new BasicEffect(_graphics.GraphicsDevice);
        // TODO: Add your initialization logic here
        _sceneManager = new SceneManager();
    
        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();

        InputManager.Initialize();
        Globals.Initialize();
        AssetManager.Initialize(Content);
        base.Initialize();


        _gameObject = new GameObject();
        _gameObject.AddComponent<MeshRenderer>();
        GameObject gameObject2 = new GameObject();
        gameObject2.AddComponent<MeshRenderer>();
        gameObject2.Transform.SetLocalPosition(new Vector3(4, 0, 0));
        _gameObject.AddChildObject(gameObject2);

    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _sceneCamera = new SceneCamera(_graphics.GraphicsDevice);
        _sceneCamera.Position = _position;
        Globals.Instance.SpriteBatch = _spriteBatch;

        // TODO: use this.Content to load your game content here
        _sceneManager.AddScene(new SecondScene());
        _sceneManager.AddScene(new BaseScene());
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Instance.PollInput();
        if (InputManager.Instance.IsActive(GameAction.EXIT)) Exit();
        
        Console.WriteLine(InputManager.Instance.GetAction(GameAction.FORWARD)?.duration);
        
        // TODO: Add your update logic here
        base.Update(gameTime);
        _sceneCamera.Update(gameTime);
        _sceneManager.GetCurrentScene().Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(_clearColor));
        // TODO: Add your drawing code here
        _basicEffect.World = Matrix.Identity;
        _basicEffect.View = _sceneCamera.View;
        _basicEffect.Projection = _sceneCamera.Projection;

        _sceneManager.GetCurrentScene().Draw(_basicEffect.View, _basicEffect.Projection);

#if DEBUG
        _imGuiRenderer.BeforeLayout(gameTime);
        ImGuiLayout();
        _imGuiRenderer.AfterLayout();
#endif
        base.Draw(gameTime);
    }

    protected virtual void ImGuiLayout()
    {
#if DEBUG
        ImGui.Checkbox("Hierarchy", ref Globals.Instance.HierarchyVisible);
        ImGui.Checkbox("Inspector",ref Globals.Instance.InspectorVisible);
        ImGui.Checkbox("Scene Selection", ref Globals.Instance.SceneSelectionVisible);
#endif

        ImGui.Text("Change the color of the background");
        ImGui.ColorEdit3("Background Color", ref _clearColor);
        ImGui.SliderFloat3("Camera position", ref _position,-100,100);
        ImGui.Text(ImGui.GetIO().Framerate + " FPS");

#if DEBUG
        if (Globals.Instance.HierarchyVisible)
        {
            ImGui.Begin("Hierarchy");
            _sceneManager.GetCurrentScene().DrawHierarchy();
            ImGui.AlignTextToFramePadding();
            ImGui.End();
        }
        if(Globals.Instance.InspectorVisible) {
            ImGui.Begin("Inspector");
            Globals.Instance.CurrentlySelectedObject?.DrawInspector();
            ImGui.End();
        }
        if (Globals.Instance.SceneSelectionVisible) {
            _sceneManager.DrawSelection();
        }
#endif
    }
}
