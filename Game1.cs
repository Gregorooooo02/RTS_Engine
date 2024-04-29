using System;
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
    private BasicEffect _basicEffect;
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
        string test = "owo/uwu";
        int index = test.LastIndexOf('/');
        if(index != -1)test = test.Substring(0,index);
        Console.WriteLine(test);
        
        
        
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
        
        
        foreach (VertexElement element in AssetManager.DefaultModel.Model.Meshes[0].MeshParts[0].VertexBuffer.VertexDeclaration.GetVertexElements())
        {
            Console.WriteLine(element.VertexElementUsage);
        }
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

#if _WINDOWS
        Globals.MainEffect = Content.Load<Effect>("PBR_Shader");
#else
        byte[] bytecode = File.ReadAllBytes("Content/TesEffectComp");
        Globals.TestEffect = new Effect(_graphics.GraphicsDevice, bytecode);
#endif
        Vector3[] lightPositions = { new(0,-5,0), new(10,10,100)};
        //Globals.TestEffect.CurrentTechnique = Globals.TestEffect.Techniques["Test"];
        Globals.MainEffect.Parameters["lightPositions"]?.SetValue(lightPositions);
        
        // TODO: use this.Content to load your game content here
        _sceneManager.AddScene(new SecondScene());
        //_sceneManager.AddScene(new ThirdScene());
        _sceneManager.AddScene(new MapScene());
    }

    protected override void Update(GameTime gameTime)
    {

        InputManager.Instance.PollInput();
        if (InputManager.Instance.IsActive(GameAction.EXIT)) Exit();
        
        // Console.WriteLine(InputManager.Instance.GetAction(GameAction.FORWARD)?.duration);
        //Console.WriteLine(InputManager.Instance.MousePosition);
        
        // TODO: Add your update logic here
        base.Update(gameTime);
        Globals.Update(gameTime);
        
#if DEBUG
        _sceneCamera.Update(gameTime);
#endif
        _sceneManager.CurrentScene.Update(gameTime);

        
        Globals.BoundingFrustum = new BoundingFrustum(Globals.View * Globals.Projection);
        Globals.MainEffect.Parameters["View"]?.SetValue(Globals.View);
        Globals.MainEffect.Parameters["Projection"]?.SetValue(Globals.Projection);
        Globals.MainEffect.Parameters["viewPos"]?.SetValue(_sceneCamera.Position);
        Globals.MainEffect.Parameters["gamma"]?.SetValue(Globals.Gamma);
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
#if DEBUG
    protected virtual void ImGuiLayout()
    {

        ImGui.Checkbox("Fullscreen", ref isFullscreen);
        ImGui.Separator();
        ImGui.Checkbox("Hierarchy", ref Globals.HierarchyVisible);
        ImGui.Checkbox("Inspector",ref Globals.InspectorVisible);
        ImGui.Checkbox("Scene Selection", ref Globals.SceneSelectionVisible);
        ImGui.Checkbox("Map Modifier", ref Globals.MapModifyVisible);

        ImGui.ColorEdit3("Background Color", ref _clearColor);
        ImGui.SliderFloat("Gamma value", ref Globals.Gamma,1,5);
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
