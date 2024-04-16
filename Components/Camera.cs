﻿using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RTS_Engine;

public class Camera : Component
{
    private string name;
    
    private GraphicsDevice _graphicsDevice = null;

    //private MouseState _mouseState = default(MouseState);
    //private KeyboardState _keyboardState = default(KeyboardState);
    
    private Vector3 up = Vector3.Up;
    private Matrix camerasWorld = Matrix.Identity;
    private Matrix viewMatrix = Matrix.Identity;
    private Matrix projectionMatrix = Matrix.Identity;
    
    public float MovementSpeed { get; set; } = 0.01f;
    
    public float fovDegrees = 45.0f;
    public float nearPlane = 0.05f;
    public float farPlane = 2000.0f;

    private int fpsKeyboardLayout = 1;
    private int cameraType = 1;

    public const int CAM_UI_OPTION_FPS = 1;
    public const int CAM_UI_OPTION_EDITOR = 0;

    public const int CAM_FIXED = 0;
    public const int CAM_FREE = 1;
    
    public override void Initialize()
    {
        Active = true;
        name = ParentObject.Name + "Camera";
    }
    
    public override void Draw(Matrix _view, Matrix _projection){}
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Camera</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<model>" + name +"</model>");
        
        builder.Append("</component>");
        return builder.ToString();
    }
    
    public override void Deserialize(XElement element){}
    
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Camera"))
        {
            ImGui.Checkbox("Camera active", ref Active);
            ImGui.Text(name);
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
        }
    }
    
    public Camera()
    {
        UpdateWorldAndView();
        UpdateProjectionMatrix(Globals.GraphicsDevice, fovDegrees);
    }
    
    public void CameraUI(int UIOption) {
        this.fpsKeyboardLayout = UIOption;
    }
    
    public void CameraType(int cameraType) {
        this.cameraType = cameraType;
    }
    
    public Vector3 Position 
    {
        get { return camerasWorld.Translation; }
        set
        {
            camerasWorld.Translation = value;
            UpdateWorldAndView();
        }
    }
    
    public Vector3 Forward
    {
        get { return camerasWorld.Forward; }
        set
        {
            camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, value, up);
            UpdateWorldAndView();
        }
    }
    
    public Vector3 Up
    {
        get { return up; }
        set
        {
            up = value;
            camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, camerasWorld.Forward, up);
            UpdateWorldAndView();
        }
    }
    
    public Vector3 LookAtDirection
    {
        get { return camerasWorld.Forward; }
        set
        {
            camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, value, up); 
            UpdateWorldAndView();
        }
    }
    
    public Vector3 TargetPostionToLookAt
    {
        set
        {
            camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, Vector3.Normalize(value - camerasWorld.Translation), up);
            UpdateWorldAndView();
        }
    }
    
    public Matrix LookAtTheTargetMatrix
    {
        set
        {
            camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, Vector3.Normalize(value.Translation - camerasWorld.Translation), up);
            UpdateWorldAndView();
        }
    }
    
    public Matrix World
    {
        get { return camerasWorld; }
        set
        {
            camerasWorld = value;
            viewMatrix = Matrix.CreateLookAt(camerasWorld.Translation, camerasWorld.Forward + camerasWorld.Translation, camerasWorld.Up);
        }
    }
    
    public Matrix View
    {
        get { return viewMatrix; }
    }

    public Matrix Projection
    {
        get { return projectionMatrix; }
    }
    
    /// <summary>
    /// Update the matrices of the camera when the camera is moved or rotated.
    /// </summary>
    private void UpdateWorldAndView()
    {
        if (cameraType == CAM_FIXED) {
            up = Vector3.Up;
        }
        if (cameraType == CAM_UI_OPTION_EDITOR) {
            up = camerasWorld.Up;
        }

        camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, camerasWorld.Forward, up);
        viewMatrix = Matrix.CreateLookAt(camerasWorld.Translation, camerasWorld.Forward + camerasWorld.Translation, camerasWorld.Up);
    }
    
    /// <summary>
    /// Changes the perspective matrix to a new near, far and field of view.
    /// </summary>
    public void UpdateProjectionMatrix(GraphicsDevice graphicsDevice, float fovDegrees) 
    {
        projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fovDegrees), graphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);
    }

    /// <summary>
    /// Changes the perspective matrxi to a new near, far and field of view.
    /// The projection matrix is only set up once at the start of the game.
    /// </summary>
    public void UpdateProjectionMatrix(float fov, float near, float far) 
    {
        this.fovDegrees = fov;
        this.nearPlane = near;
        this.farPlane = far;

        projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fovDegrees), _graphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);
    }
    
    /// <summary>
    /// Update the camera
    /// </summary>
    public override void Update() {}

#region Moving the camera
    public void MoveForward(GameTime gameTime)
    {
        Position += (camerasWorld.Forward * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveBackward(GameTime gameTime)
    {
        Position += (camerasWorld.Backward * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveLeft(GameTime gameTime)
    {
        Position += (camerasWorld.Left * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveRight(GameTime gameTime)
    {
        Position += (camerasWorld.Right * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveUp(GameTime gameTime)
    {
        Position += (camerasWorld.Up * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveDown(GameTime gameTime)
    {
        Position += (camerasWorld.Down * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveForwardWorld(GameTime gameTime)
    {
        Position += (Vector3.Forward * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveBackwardWorld(GameTime gameTime)
    {
        Position += (Vector3.Backward * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveLeftWorld(GameTime gameTime)
    {
        Position += (Vector3.Left * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveRightWorld(GameTime gameTime)
    {
        Position += (Vector3.Right * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveUpWorld(GameTime gameTime)
    {
        Position += (Vector3.Up * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }

    public void MoveDownWorld(GameTime gameTime)
    {
        Position += (Vector3.Down * MovementSpeed) * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
    }
#endregion

}