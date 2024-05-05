using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Num = System.Numerics;
using System.IO;

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

    private bool isFullscreen = false;
    private bool isWireframe = false;
    
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
        _basicEffect = new BasicEffect(_graphics.GraphicsDevice);
        // TODO: Add your initialization logic here
        _sceneManager = new SceneManager();

        _imGuiRenderer = new ImGuiRenderer(this);

        _imGuiRenderer.RebuildFontAtlas();

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
        Globals.GraphicsDevice = _graphics.GraphicsDevice;
        Globals.BasicEffect = _basicEffect;

#if _WINDOWS
        Globals.TestEffect = Content.Load<Effect>("TestEffect");
#else
        // byte[] bytecode = File.ReadAllBytes("Content/TesEffectComp");
        // Globals.TestEffect = new Effect(_graphics.GraphicsDevice, bytecode);
#endif
        Globals.TestEffect.CurrentTechnique = Globals.TestEffect.Techniques["Test"];
        Globals.TestEffect.Parameters["Tx"].SetValue(Content.Load<Texture2D>("sprite"));
        
        // TODO: use this.Content to load your game content here
        _sceneManager.AddScene(new MapScene());
        _sceneManager.AddScene(new SecondScene());
        _sceneManager.AddScene(new ThirdScene());
    }

    protected override void Update(GameTime gameTime)
    {

        InputManager.Instance.PollInput();
        if (InputManager.Instance.IsActive(GameAction.EXIT)) Exit();
        
        // Console.WriteLine(InputManager.Instance.GetAction(GameAction.FORWARD)?.duration);
        // Console.WriteLine(InputManager.Instance.MousePosition);
        
        // TODO: Add your update logic here
        base.Update(gameTime);
        Globals.Update(gameTime);
        
#if DEBUG
        _sceneCamera.Update(gameTime);
#endif
        _sceneManager.CurrentScene.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        var d = new DepthStencilState();
        d.DepthBufferEnable = true;
        d.DepthBufferWriteEnable = true;
        GraphicsDevice.DepthStencilState = d;
        
        GraphicsDevice.Clear(new Color(_clearColor));
        // TODO: Add your drawing code here
#if RELEASE
        _basicEffect.World = Matrix.Identity;
        _basicEffect.View = Globals.View;
        _basicEffect.Projection = Globals.Projection;
#elif DEBUG
        _basicEffect.World = _sceneCamera.World;
        _basicEffect.View = _sceneCamera.View;
        _basicEffect.Projection = _sceneCamera.Projection;
#endif
        RasterizerState rasterizerState = new RasterizerState();
        if (isWireframe)
        {
			rasterizerState.FillMode = FillMode.WireFrame;
        }
		else
		{
			rasterizerState.FillMode = FillMode.Solid;
		}

		GraphicsDevice.RasterizerState = rasterizerState;
        
        _spriteBatch.Begin();
        _sceneManager.CurrentScene.Draw(_basicEffect.View, _basicEffect.Projection);
        _spriteBatch.End();

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
        ImGui.Checkbox("Fullscreen", ref isFullscreen);
        ImGui.Separator();
		ImGui.Checkbox("Wireframe", ref isWireframe);
		ImGui.Separator();
        ImGui.Checkbox("Hierarchy", ref Globals.HierarchyVisible);
        ImGui.Checkbox("Inspector",ref Globals.InspectorVisible);
        ImGui.Checkbox("Scene Selection", ref Globals.SceneSelectionVisible);
#endif
        ImGui.ColorEdit3("Background Color", ref _clearColor);
        ImGui.Text(ImGui.GetIO().Framerate + " FPS");

#if DEBUG
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
#endif
    }
}
