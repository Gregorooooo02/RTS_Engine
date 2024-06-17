using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class Marker : Component
{
    private bool _markerActive = false;

    private float currentTime = 0;
    private float maxTime = 2.0f;

    private float yOffset = 2.0f;

    private Vector3 restingPosition = new Vector3(0,100000,0);

    private MeshRenderer _meshRenderer = null;
    
    public override void Update()
    {
        Globals.AgentsManager.Marker = this;
        if (_markerActive)
        {
            currentTime += Globals.DeltaTime;
            if (currentTime >= maxTime)
            {
                _markerActive = false;
                ParentObject.Active = false;
            }
        }
    }

    public void PlaceMarker(Vector3 position)
    {
        _meshRenderer ??= ParentObject.GetComponent<MeshRenderer>();
        _markerActive = true;
        ParentObject.Transform.SetLocalPosition(position);
        ParentObject.Transform.Move(new Vector3(0,yOffset,0));
        ParentObject.Active = true;
        currentTime = 0;
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
        
        builder.Append("<maxTime>" + maxTime + "</maxTime>");
        
        builder.Append("<yOffset>" + yOffset + "</yOffset>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        maxTime = float.TryParse(element.Element("maxTime")?.Value, out float time) ? time : 1.5f;
        yOffset = float.TryParse(element.Element("yOffset")?.Value, out float y) ? y : 2.0f;
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
            ImGui.DragFloat("Active time", ref maxTime);
            ImGui.DragFloat("Height offset", ref yOffset);
        }
    }
#endif
}