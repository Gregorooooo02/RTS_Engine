using System;
using System.ComponentModel.Design.Serialization;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RTS_Engine;

public class SceneCamera
{
    private GraphicsDevice _graphicsDevice = null;

    private MouseState _mouseState = default(MouseState);
    private KeyboardState _keyboardState = default(KeyboardState);

    public float MovementSpeed { get; set; } = 0.01f;
    public float RotationSpeed { get; set; } = 0.01f;

    public float fovDegrees = 45.0f;
    public float nearPlane = 0.05f;
    public float farPlane = 2000.0f;

    private bool isMouseLookUsed = true;

    private int fpsKeyboardLayout = 1;
    private int cameraType = 0;

    public const int CAM_UI_OPTION_FPS = 0;
    public const int CAM_UI_OPTION_EDITOR = 1;

    public const int CAM_FIXED = 0;
    public const int CAM_FREE = 1;

    public SceneCamera(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        UpdateWorldAndView();
        UpdateProjectionMatrix(graphicsDevice, fovDegrees);
    }

    public void CameraUI(int UIOption) {
        this.fpsKeyboardLayout = UIOption;
    }

    public void CameraType(int cameraType) {
        this.cameraType = cameraType;
    }

    private Vector3 up = Vector3.Up;
    private Matrix camerasWorld = Matrix.Identity;
    private Matrix viewMatrix = Matrix.Identity;
    private Matrix projectionMatrix = Matrix.Identity;

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
    public void Update(GameTime gameTime) {
        if (fpsKeyboardLayout == CAM_UI_OPTION_FPS) {
            UpdateFPSKeyboardLayout(gameTime);
        }
        if (fpsKeyboardLayout == CAM_UI_OPTION_EDITOR) {
            UpdateEditorKeyboardLayout(gameTime);
        }
    }

    /// <summary>
    /// Update the camera with the FPS keyboard layout.
    /// </summary>
    private void UpdateFPSKeyboardLayout(GameTime gameTime) {
        MouseState mState = Mouse.GetState();
        KeyboardState kState = Keyboard.GetState();

        // Moving the camera with WASD
        if (kState.IsKeyDown(Keys.W)) 
        {
            MoveForward(gameTime);
        }
        else if (kState.IsKeyDown(Keys.S)) 
        {
            MoveBackward(gameTime);
        }

        if (kState.IsKeyDown(Keys.A)) 
        {
            MoveLeft(gameTime);
        }
        else if (kState.IsKeyDown(Keys.D)) 
        {
            MoveRight(gameTime);
        }

        // Move the camera up and down with Q and E
        if (kState.IsKeyDown(Keys.Q)) 
        {
            if (cameraType == CAM_FIXED)
            {
                MoveUpWorld(gameTime);
            }

            if (cameraType == CAM_FREE)
            {
                MoveUp(gameTime);
            }
        }
        else if (kState.IsKeyDown(Keys.E)) 
        {
            if (cameraType == CAM_FIXED)
            {
                MoveDownWorld(gameTime);
            }

            if (cameraType == CAM_FREE)
            {
                MoveDown(gameTime);
            }
        }

        if (mState.LeftButton == ButtonState.Pressed)
        {
            if (isMouseLookUsed == false)
            {
                isMouseLookUsed = true;
            } else
            {
                isMouseLookUsed = false;
            }
        }

        if (isMouseLookUsed)
        {
            Vector2 diff = mState.Position.ToVector2() - _mouseState.Position.ToVector2();

            if (diff.X != 0)
            {
                RotateLeftRight(gameTime, diff.X);
            }
            if (diff.Y != 0)
            {
                RotateUpDown(gameTime, diff.Y);
            }
        }
        _mouseState = mState;
        _keyboardState = kState;
    }

    /// <summary>
    /// Update the camera with the Editor keyboard layout.
    /// </summary>
    private void UpdateEditorKeyboardLayout(GameTime gameTime) {
        MouseState mState = Mouse.GetState();
        KeyboardState kState = Keyboard.GetState();

        if (kState.IsKeyDown(Keys.E))
        {
            MoveForward(gameTime);
        }
        else if (kState.IsKeyDown(Keys.Q))
        {
            MoveBackward(gameTime);
        }

        if (kState.IsKeyDown(Keys.W))
        {
            RotateUp(gameTime);
        }
        else if (kState.IsKeyDown(Keys.S))
        {
            RotateDown(gameTime);
        }

        if (kState.IsKeyDown(Keys.A))
        {
            RotateLeft(gameTime);
        }
        else if (kState.IsKeyDown(Keys.D))
        {
            RotateRight(gameTime);
        }

        if (kState.IsKeyDown(Keys.Left))
        {
            MoveLeft(gameTime);
        }
        else if (kState.IsKeyDown(Keys.Right))
        {
            MoveRight(gameTime);
        }

        if (kState.IsKeyDown(Keys.Up))
        {
            MoveUp(gameTime);
        }
        else if (kState.IsKeyDown(Keys.Down))
        {
            MoveDown(gameTime);
        }

        if (kState.IsKeyDown(Keys.Z))
        {
            if (cameraType == CAM_FREE)
            {
                RotateRollCounterClockwise(gameTime);
            }
        }
        else if (kState.IsKeyDown(Keys.C))
        {
            if (cameraType == CAM_FREE)
            {
                RotateRollClockwise(gameTime);
            }
        }

        if (mState.RightButton == ButtonState.Pressed)
        {
            isMouseLookUsed = true;
        }
        else
        {
            isMouseLookUsed = false;
        }

        if (isMouseLookUsed)
        {
            Vector2 diff = mState.Position.ToVector2() - _mouseState.Position.ToVector2();

            if (diff.X != 0)
            {
                RotateLeftRight(gameTime, diff.X);
            }
            if (diff.Y != 0)
            {
                RotateUpDown(gameTime, diff.Y);
            }
        }
        _mouseState = mState;
        _keyboardState = kState;
    }

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

#region Rotating the camera
    public void RotateUp(GameTime gameTime)
    {
        var radians = RotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Right, MathHelper.ToRadians(radians));
        LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
        UpdateWorldAndView();
    }

    public void RotateDown(GameTime gameTime)
    {
        var radians = -RotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Right, MathHelper.ToRadians(radians));
        LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
        UpdateWorldAndView();
    }

    public void RotateLeft(GameTime gameTime)
    {
        var radians = RotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Up, MathHelper.ToRadians(radians));
        LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
        UpdateWorldAndView();
    }

    public void RotateRight(GameTime gameTime)
    {
        var radians = -RotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Up, MathHelper.ToRadians(radians));
        LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
        UpdateWorldAndView();
    }

    public void RotateUpDown(GameTime gameTime, float amount)
    {
        var radians = amount * -RotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Right, MathHelper.ToRadians(radians));
        LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
        UpdateWorldAndView();
    }

    public void RotateLeftRight(GameTime gameTime, float amount)
    {
        var radians = amount * -RotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        Matrix matrix = Matrix.CreateFromAxisAngle(camerasWorld.Up, MathHelper.ToRadians(radians));
        LookAtDirection = Vector3.TransformNormal(LookAtDirection, matrix);
        UpdateWorldAndView();
    }

    public void RotateRollClockwise(GameTime gameTime)
    {
        var radians = MathHelper.ToRadians(RotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds);
        var pos = camerasWorld.Translation;

        camerasWorld *= Matrix.CreateFromAxisAngle(camerasWorld.Forward, radians);
        camerasWorld.Translation = pos;

        UpdateWorldAndView();
    }

    public void RotateRollCounterClockwise(GameTime gameTime)
    {
        var radians = MathHelper.ToRadians(-RotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds);
        var pos = camerasWorld.Translation;

        camerasWorld *= Matrix.CreateFromAxisAngle(camerasWorld.Forward, radians);
        camerasWorld.Translation = pos;

        UpdateWorldAndView();
    }
#endregion
}
