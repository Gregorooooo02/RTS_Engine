using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class Marker : Component
{
    private bool _markerActive = false;

    private float _currentTime = 0;
    private float _maxTime = 2.0f;

    private float _yOffset = 2.0f;
    
    public override void Update()
    {
        Globals.AgentsManager.Marker = this;
        if (_markerActive)
        {
            _currentTime += Globals.DeltaTime;
            if (_currentTime >= _maxTime)
            {
                _markerActive = false;
                ParentObject.Active = false;
            }
        }
    }

    public void PlaceMarker(Vector3 position)
    {
        _markerActive = true;
        ParentObject.Transform.SetLocalPosition(position);
        ParentObject.Transform.Move(new Vector3(0,_yOffset,0));
        ParentObject.Active = true;
        _currentTime = 0;
    }
    
    public override void Initialize()
    {
        
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Marker</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<maxTime>" + _maxTime + "</maxTime>");
        
        builder.Append("<yOffset>" + _yOffset + "</yOffset>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        _maxTime = float.TryParse(element.Element("maxTime")?.Value, out float time) ? time : 1.5f;
        _yOffset = float.TryParse(element.Element("yOffset")?.Value, out float y) ? y : 2.0f;
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("Marker"))
        {
            ImGui.Checkbox("Marker active", ref Active);
            ImGui.DragFloat("Active time", ref _maxTime);
            ImGui.DragFloat("Height offset", ref _yOffset);
        }
    }
#endif
}