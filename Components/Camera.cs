using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class Camera : Component
{
    public bool IsWorldCamera = false;
    
    private float _zoomSpeed = 8.0f;
    private float _lerpSpeed = 3.5f;
    private int _previousScrollValue;
    public float FovDegrees = 45.0f;
    public float _targetFov = 45.0f;
    private float _zoomMin = 25.0f;
    private float _zoomMax = 75.0f;

    private float _cameraSpeed = 20.0f;

    private const float NearPlane = 0.5f;
    private const float FarPlane = 2000.0f;

    public float _aboveGroundOffset = 40.0f;
    private float currentHeight = -10f;
    private float _heightLerpSpeed = 6.5f;
    
    private bool _keyboardControl = true;
    private bool _mouseControl = true;
    private bool _scrollControl = true;

    public override void Initialize()
    {
        UpdateCameraMatrices();
        ParentObject.Transform.SetLocalRotation(new(-75, 25, 0));
        ParentObject.AddComponent<Listener>();
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Camera</type>");
        
        builder.Append("<active>" + Active + "</active>");
              
        builder.Append("<fovDegrees>" + FovDegrees + "</fovDegrees>");
        
        builder.Append("<zoomMin>" + _zoomMin + "</zoomMin>");
        
        builder.Append("<zoomMax>" + _zoomMax + "</zoomMax>");
        
        builder.Append("<lerpSpeed>" + _lerpSpeed + "</lerpSpeed>");
        
        builder.Append("<zoomSpeed>" + _zoomSpeed + "</zoomSpeed>");
        
        builder.Append("<cameraSpeed>" + _cameraSpeed + "</cameraSpeed>");
        
        builder.Append("<keyboardControl>" + _keyboardControl + "</keyboardControl>");
        
        builder.Append("<mouseControl>" + _mouseControl + "</mouseControl>");
        
        builder.Append("<scrollControl>" + _scrollControl + "</scrollControl>");
        
        builder.Append("<aboveGroundOffset>" + _aboveGroundOffset + "</aboveGroundOffset>");
        
        builder.Append("<heightLerpSpeed>" + _heightLerpSpeed + "</heightLerpSpeed>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        FovDegrees = float.TryParse(element.Element("fovDegress")?.Value, out float degrees) ? degrees : 45.0f;
        _zoomMin = float.TryParse(element.Element("zoomMin")?.Value, out float zoomMin) ? zoomMin : 25.0f;
        _zoomMax = float.TryParse(element.Element("zoomMax")?.Value, out float zoomMax) ? zoomMax : 75.0f;
        _lerpSpeed = float.TryParse(element.Element("lerpSpeed")?.Value, out float lerpSpeed) ? lerpSpeed : 3.5f;
        _zoomSpeed = float.TryParse(element.Element("zoomSpeed")?.Value, out float zoomSpeed) ? zoomSpeed : 8.0f;
        _cameraSpeed = float.TryParse(element.Element("cameraSpeed")?.Value, out float cameraSpeed) ? cameraSpeed : 20.0f;
        _aboveGroundOffset = float.TryParse(element.Element("aboveGroundOffset")?.Value, out float aboveGroundOffset) ? aboveGroundOffset : 20.0f;
        _heightLerpSpeed = float.TryParse(element.Element("heightLerpSpeed")?.Value, out float heightLerpSpeed) ? heightLerpSpeed : 6.0f;
        _keyboardControl = element.Element("keyboardControl")?.Value == "True";
        _mouseControl = element.Element("mouseControl")?.Value == "True";
        _scrollControl = element.Element("scrollControl")?.Value == "True";
    }
    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Camera"))
        {
            ImGui.Checkbox("Camera active", ref Active);
            ImGui.DragFloat("Zoom speed", ref _zoomSpeed, 0.02f,0.1f);
            ImGui.DragFloat("Zoom Lerp speed", ref _lerpSpeed, 0.05f);
            ImGui.DragFloat("Camera speed", ref _cameraSpeed, 0.05f);
            ImGui.DragFloat("Minimum zoom", ref _zoomMin, 0.1f);
            ImGui.DragFloat("Maximum zoom", ref _zoomMax, 0.1f);
            ImGui.DragFloat("Height above ground", ref _aboveGroundOffset, 0.1f, 0.5f);
            ImGui.DragFloat("Height Lerp speed", ref _heightLerpSpeed, 0.25f, 0.01f, 25.00f);
            ImGui.Checkbox("Keyboard control", ref _keyboardControl);
            ImGui.Checkbox("Mouse control", ref _mouseControl);
            ImGui.Checkbox("Scroll control", ref _scrollControl);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
    }
#endif

    public Camera(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }

    public Camera(){}
    
    
    private void UpdateCameraMatrices()
    {
        Globals.ZoomDegrees = FovDegrees;
        Globals.ViewPos = ParentObject.Transform.ModelMatrix.Translation;
        Globals.View = Matrix.CreateLookAt(Globals.ViewPos, ParentObject.Transform.ModelMatrix.Forward + Globals.ViewPos, Vector3.Up);
        Globals.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(FovDegrees), Globals.GraphicsDevice.Viewport.AspectRatio, NearPlane, FarPlane);
        Globals.CurrentCamera = this;
    }

    public void MoveCameraToPosition(Vector3 position)
    {
        if(!IsWorldCamera) return;
        Vector3 newPos = position + new Vector3(20, 0, 5);
        if (newPos.X < 0 || newPos.Z < 0 || newPos.X >= Globals.Renderer.WorldRenderer.MapNodes.GetLength(0) - 1 ||
            newPos.Z >= Globals.Renderer.WorldRenderer.MapNodes.GetLength(1) - 1)
        {
            newPos = position;
        }
        ParentObject.Transform.SetLocalPosition(newPos);
        float newHeight = _aboveGroundOffset +
                          PickingManager.InterpolateWorldHeight(new Vector2(ParentObject.Transform.Pos.X,
                              ParentObject.Transform.Pos.Z));
        currentHeight = currentHeight < 0 ? ParentObject.Transform.Pos.Y : MathHelper.Lerp(currentHeight,newHeight,_heightLerpSpeed * Globals.DeltaTime);
        ParentObject.Transform.SetLocalPositionY(currentHeight);
    }

    public void MoveCameraToPosition(Vector3 position, WorldRenderer world)
    {
        if(!IsWorldCamera) return;
        Vector3 newPos = position + new Vector3(20, 0, 5);
        if (newPos.X < 0 || newPos.Z < 0 || newPos.X >= world.MapNodes.GetLength(0) - 1 ||
            newPos.Z >= world.MapNodes.GetLength(1) - 1)
        {
            newPos = position;
        }
        ParentObject.Transform.SetLocalPosition(newPos);
        float newHeight = _aboveGroundOffset +
                          PickingManager.InterpolateWorldHeight(new Vector2(ParentObject.Transform.Pos.X,
                              ParentObject.Transform.Pos.Z),world);
        currentHeight = currentHeight < 0 ? ParentObject.Transform.Pos.Y : MathHelper.Lerp(currentHeight,newHeight,_heightLerpSpeed * Globals.DeltaTime);
        ParentObject.Transform.SetLocalPositionY(currentHeight);
    }
    
    private void MoveCamera()
    {
        if(!_keyboardControl) return;
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
        Vector3 forward = ParentObject.Transform.ModelMatrix.Forward;
        forward.Y = 0;
        forward.Normalize();
        Vector3 right = Vector3.Cross(forward, Vector3.Up) ;
        right.Y = 0;
        right.Normalize();
        Vector3 combined = Vector3.Add(forward * x, right * y);
        if(x + y != 0 || x * y != 0)combined.Normalize();
        Vector3 newPos = combined * Globals.DeltaTime * _cameraSpeed + ParentObject.Transform.Pos;
        
        if (IsWorldCamera)
        {
            if (newPos.X < 0 || newPos.Z < 0 || newPos.X >= Globals.Renderer.WorldRenderer.MapNodes.GetLength(0) - 1 ||
                newPos.Z >= Globals.Renderer.WorldRenderer.MapNodes.GetLength(1) - 1)
            {
                return;
            }
            ParentObject.Transform.SetLocalPosition(newPos);
            float newHeight = _aboveGroundOffset +
                              PickingManager.InterpolateWorldHeight(new Vector2(ParentObject.Transform.Pos.X,
                                  ParentObject.Transform.Pos.Z));
            currentHeight = currentHeight < 0 ? ParentObject.Transform.Pos.Y : MathHelper.Lerp(currentHeight,newHeight,_heightLerpSpeed * Globals.DeltaTime);
            ParentObject.Transform.SetLocalPositionY(currentHeight);
        }
        else
        {
            ParentObject.Transform.SetLocalPosition(newPos);
        }
    }
    
    private void UpdateFov()
    {
        if(!_scrollControl) return;
        if(InputManager.Instance.ScrollWheel != _previousScrollValue)
        {
            _targetFov = FovDegrees + _zoomSpeed * Math.Sign(_previousScrollValue - InputManager.Instance.ScrollWheel);
            _previousScrollValue = InputManager.Instance.ScrollWheel;
            
        }
        if (MathF.Abs(_targetFov - FovDegrees) > 0.1f) FovDegrees = MathHelper.Lerp(FovDegrees, _targetFov, Globals.DeltaTime * _lerpSpeed);
        FovDegrees = Math.Clamp(FovDegrees, _zoomMin, _zoomMax);
    }

    /// <summary>
    /// Update the camera
    /// </summary>
    public override void Update()
    {
        if(!Active) return;
        MoveCamera();
        UpdateFov();
        UpdateCameraMatrices();
    }
}