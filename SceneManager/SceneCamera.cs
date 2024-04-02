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

    public float MovementSpeed { get; set; } = 0.1f;
    public float RotationSpeed { get; set; } = 0.1f;

    public float fovDegrees = 45.0f;
    public float nearPlane = 0.05f;
    public float farPlane = 2000.0f;

    private float yMouseAngle = 0.0f;
    private float xMouseAngle = 0.0f;
    private bool isMouseLookUsed = true;

    private int fpsKeyboardLayout = 1;
    private int cameraType = 1;

    public const int CAM_UI_OPTION_FPS = 0;
    public const int CAM_UI_OPTION_EDITOR = 1;

    public const int CAM_FIXED = 0;
    public const int CAM_FREE = 1;

    public SceneCamera(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
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
        }
    }

    public Vector3 Forward
    {
        get { return camerasWorld.Forward; }
        set
        {
            camerasWorld.Forward = value;
        }
    }

    public Vector3 Up
    {
        get { return camerasWorld.Up; }
        set
        {
            camerasWorld.Up = value;
        }
    }

    public Vector3 LookAtDirection
    {
        get { return camerasWorld.Forward; }
        set
        {
            camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, value, up); }
    }

    public Vector3 TargetPostionToLookAt
    {
        set
        {
            camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, Vector3.Normalize(value - camerasWorld.Translation), up);
        }
    }

    public Matrix LookAtTheTargetMatrix
    {
        set
        {
            camerasWorld = Matrix.CreateWorld(camerasWorld.Translation, Vector3.Normalize(value.Translation - camerasWorld.Translation), up);
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
}
