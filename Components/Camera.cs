using System;
using System.Text;
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
    
    private Vector3 up = Vector3.Up;
    
    private float angle = -45.0f;
    
    public float fovDegrees = 45.0f;
    public float nearPlane = 0.05f;
    public float farPlane = 2000.0f;

    // private int fpsKeyboardLayout = 1;
    private int cameraType = 1;

    public const int CAM_UI_OPTION_FPS = 1;
    public const int CAM_UI_OPTION_EDITOR = 0;

    public const int CAM_FIXED = 0;
    public const int CAM_FREE = 1;
    
    public override void Initialize()
    {
        Active = true;
        name = ParentObject.Name + "Camera";
        Globals.World = Matrix.Identity;
        Globals.Projection = Matrix.Identity;
        Globals.View = Matrix.Identity;
        UpdateWorldAndView();
        UpdateProjectionMatrix(fovDegrees, nearPlane, farPlane);
        RotateUpDown(angle);
    }
    
    public override void Draw(){}
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Camera</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("<model>" + name + "</model>");
        
        builder.Append("<angle>" + angle + "</angle>");
        
        builder.Append("<fovDegrees>" + fovDegrees + "</fovDegrees>");
        
        builder.Append("<nearPlane>" + nearPlane + "</nearPlane>");
        
        builder.Append("<farPlane>" + farPlane +"</farPlane>");
        
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
    
    private void RotateUpDown(float angle)
    {
        Matrix rotation = Matrix.CreateFromAxisAngle(Globals.World.Right, MathHelper.ToRadians(angle));
        Globals.World = rotation * Globals.World;
        UpdateWorldAndView();
    }
    
    public Camera()
    {
        UpdateWorldAndView();
        UpdateProjectionMatrix(fovDegrees, nearPlane, farPlane);
    }
    
    // public void CameraUI(int UIOption) {
    //     this.fpsKeyboardLayout = UIOption;
    // }
    
    public void CameraType(int cameraType) {
        this.cameraType = cameraType;
    }
    
    public Vector3 Position 
    {
        get { return Globals.World.Translation; }
        set
        {
            Globals.World.Translation = value;
            UpdateWorldAndView();
        }
    }
    
    public Vector3 Forward
    {
        get { return Globals.World.Forward; }
        set
        {
            Globals.World = Matrix.CreateWorld(Globals.World.Translation, value, up);
            UpdateWorldAndView();
        }
    }
    
    public Vector3 Up
    {
        get { return up; }
        set
        {
            up = value;
            Globals.World = Matrix.CreateWorld(Globals.World.Translation, Globals.World.Forward, up);
            UpdateWorldAndView();
        }
    }
    
    public Vector3 LookAtDirection
    {
        get { return Globals.World.Forward; }
        set
        {
            Globals.World = Matrix.CreateWorld(Globals.World.Translation, value, up); 
            UpdateWorldAndView();
        }
    }
    
    public Vector3 TargetPostionToLookAt
    {
        set
        {
            Globals.World = Matrix.CreateWorld(Globals.World.Translation, Vector3.Normalize(value - Globals.World.Translation), up);
            UpdateWorldAndView();
        }
    }
    
    public Matrix LookAtTheTargetMatrix
    {
        set
        {
            Globals.World = Matrix.CreateWorld(Globals.World.Translation, Vector3.Normalize(value.Translation - Globals.World.Translation), up);
            UpdateWorldAndView();
        }
    }
    
    public Matrix World
    {
        get { return Globals.World; }
        set
        {
            Globals.World = value;
            Globals.View = Matrix.CreateLookAt(Globals.World.Translation, Globals.World.Forward + Globals.World.Translation, Globals.World.Up);
        }
    }
    
    public Matrix View
    {
        get { return Globals.View; }
    }

    public Matrix Projection
    {
        get { return Globals.Projection; }
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
            up = Globals.World.Up;
        }

        Globals.World = Matrix.CreateWorld(Globals.World.Translation, Globals.World.Forward, up);
        Globals.View = Matrix.CreateLookAt(Globals.World.Translation, Globals.World.Forward + Globals.World.Translation, Globals.World.Up);
    }
    
    /// <summary>
    /// Changes the perspective matrix to a new near, far and field of view.
    /// </summary>
    public void UpdateProjectionMatrix(float fov) 
    {
        Globals.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fov), Globals.GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);
    }

    /// <summary>
    /// Changes the perspective matrix to a new near, far and field of view.
    /// The projection matrix is only set up once at the start of the game.
    /// </summary>
    public void UpdateProjectionMatrix(float fov, float near, float far) 
    {
        this.fovDegrees = fov;
        this.nearPlane = near;
        this.farPlane = far;

        Globals.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fovDegrees), Globals.GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);
    }

    /// <summary>
    /// Update the camera
    /// </summary>
    public override void Update()
    {
        Position = ParentObject.Transform._pos;
        UpdateProjectionMatrix(fovDegrees, nearPlane, farPlane);
        
        // For safety reasons
        if (fovDegrees < 1.0f) fovDegrees = 1.0f;
        if (fovDegrees > 89.0f) fovDegrees = 89.0f;
    }
}