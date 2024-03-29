﻿using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ImGuiNET;
using Num = System.Numerics;

namespace RTS_Engine;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Model _model;
    private Texture2D _texture;
    
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

        _imGuiRenderer = new ImGuiRenderer(this);
        _imGuiRenderer.RebuildFontAtlas();

        InputManager.Initialize();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _model = Content.Load<Model>("SimpleShip/Ship");
        _texture = Content.Load<Texture2D>("smile");

        // TODO: use this.Content to load your game content here

        
    }

    protected override void Update(GameTime gameTime)
    {
        InputManager.Instance.PollInput();
        if (InputManager.Instance.IsActive(GameAction.EXIT)) Exit();
        
        Console.WriteLine(InputManager.Instance.GetAction(GameAction.FORWARD)?.duration);

        // TODO: Add your update logic here

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(_clearColor));
        
        _view = Matrix.CreateLookAt(
        _position,
        new Vector3(0.0f),
        -Vector3.UnitY);
        DrawModel(_model, _world, _view, _projection);

        _spriteBatch.Begin();
        _spriteBatch.Draw(_texture, new Rectangle(0,0,500,500), Color.White);
        _spriteBatch.End();

        // TODO: Add your drawing code here
        _imGuiRenderer.BeforeLayout(gameTime);
        ImGuiLayout();
        _imGuiRenderer.AfterLayout();

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
        ImGui.Text("Change the color of the background");
        ImGui.ColorEdit3("Background Color", ref _clearColor);
        ImGui.SliderFloat3("Camera position", ref _position,-10,10);
        ImGui.Text(ImGui.GetIO().Framerate + " FPS");
    }

  
}
