using System;
using System.Diagnostics;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RTS_Engine;

public class Camera : Component
{
    public float cameraSpeed = 20.0f;
    public float fovDegrees = 45.0f;
    public float nearPlane = 0.05f;
    public float farPlane = 2000.0f;
    
    public override void Initialize()
    {
        UpdateCameraMatrices();
        ParentObject.Transform.SetLocalRotation(new(-45, 45, 0));
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Camera</type>");
        
        builder.Append("<active>" + Active + "</active>");
              
        builder.Append("<fovDegrees>" + fovDegrees + "</fovDegrees>");
        
        builder.Append("<nearPlane>" + nearPlane + "</nearPlane>");
        
        builder.Append("<farPlane>" + farPlane +"</farPlane>");
        
        builder.Append("</component>");
        return builder.ToString();
    }
    
    public override void Deserialize(XElement element){}
    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Camera"))
        {
            ImGui.Checkbox("Camera active", ref Active);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
    }
   
    public Camera(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }

    public Camera(){}
    
    /// <summary>
    /// Changes the perspective matrix to a new near, far and field of view.
    /// The projection matrix is only set up once at the start of the game.
    /// </summary>
    public void UpdateCameraMatrices() 
    {
        Globals.View = Matrix.CreateLookAt(ParentObject.Transform._pos, ParentObject.Transform.ModelMatrix.Forward + ParentObject.Transform._pos, Vector3.Up);
        Globals.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(fovDegrees), Globals.GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);
    }

    private void MoveCamera()
    {
        int x = 0, y = 0;
        if(InputManager.Instance.IsActive(GameAction.FORWARD)){
            x += 1;
        }
        if (InputManager.Instance.IsActive(GameAction.BACKWARD))
        {
            x -= 1;
        }
        if (InputManager.Instance.IsActive(GameAction.RIGHT))
        {
            y += 1; 
        }
        if (InputManager.Instance.IsActive(GameAction.LEFT))
        {
            y -= 1;
        }
        Vector3 right = Vector3.Cross(ParentObject.Transform.ModelMatrix.Forward, ParentObject.Transform.ModelMatrix.Up) ;
        right.Y = 0;
        right.Normalize();
        Vector3 forward = ParentObject.Transform.ModelMatrix.Forward;
        forward.Y = 0;
        forward.Normalize();
        Vector3 combined = Vector3.Add(forward * x, right * y);
        if(x + y != 0 || x * y != 0)combined.Normalize();
        ParentObject.Transform.Move(combined * Globals.DeltaTime * cameraSpeed);

        
    }

    /// <summary>
    /// Update the camera
    /// </summary>
    public override void Update()
    {
        if(!Active) return;
        MoveCamera();
        UpdateCameraMatrices();
        
        // For safety reasons
        if (fovDegrees < 1.0f) fovDegrees = 1.0f;
        if (fovDegrees > 89.0f) fovDegrees = 89.0f;
    }
}