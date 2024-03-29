using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ImGuiNET;
using Num = System.Numerics;
using System.Diagnostics;

namespace RTS_Engine;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SceneManager _sceneManager;
    
    private Num.Vector3 _position = new Num.Vector3(0,0,10);
    private Matrix _world = Matrix.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f));
    private Matrix _view = Matrix.CreateLookAt(
        new Vector3(0.0f, 0.0f, 10.0f),
        new Vector3(0.0f),
        -Vector3.UnitY);
    private Matrix _projection =
        Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 1440.0f / 900.0f, 0.1f, 100.0f); 
    
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
        // TODO: Add your initialization logic here
        _sceneManager = new SceneManager();

        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();

        InputManager.Initialize();
        Globals.Initialize(Content);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        // TODO: use this.Content to load your game content here
        _sceneManager.AddScene(new BaseScene());
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Instance.PollInput();
        if (InputManager.Instance.IsActive(GameAction.EXIT)) Exit();
        
        Console.WriteLine(InputManager.Instance.GetAction(GameAction.FORWARD)?.duration);

        // TODO: Add your update logic here
        base.Update(gameTime);
        _sceneManager.GetCurrentScene().Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(_clearColor));
        
        _view = Matrix.CreateLookAt(_position, new Vector3(0.0f), -Vector3.UnitY);

        // TODO: Add your drawing code here
        _sceneManager.GetCurrentScene().Draw();

#if DEBUG
        _imGuiRenderer.BeforeLayout(gameTime);
        ImGuiLayout();
        _imGuiRenderer.AfterLayout();
#endif
        base.Draw(gameTime);
    }

    private void DrawModel(Model model, Matrix wrld, Matrix vw, Matrix proj)
    {
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.World = wrld;
                effect.View = vw;
                effect.Projection = proj;
            }
            mesh.Draw();
        }
    }

    protected virtual void ImGuiLayout()
    {
#if DEBUG
        ImGui.Checkbox("Hierarchy", ref Globals.Instance.HierarchyVisible);
        ImGui.Checkbox("Inspector",ref Globals.Instance.InspectorVisible);
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
#endif
    }
}
