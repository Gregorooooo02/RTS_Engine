using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI;

namespace RTS_Engine;

public class HiddenIndicator : Component
{
private Agent _agent;
    private SpiteRenderer _renderer;
    
    private Vector3 _markOffset;
    public override void Update()
    {
        //Initialize some values
        if(!Active) return;
        
        _agent ??= ParentObject.Parent.GetComponent<Agent>();
        _renderer ??= ParentObject.GetComponent<SpiteRenderer>();
        
        if (_agent != null && _renderer != null)
        {
            Vector3? newPos = PickingManager.CalculatePositionOnScreen(_agent.Position);
            if (newPos.HasValue && _agent.IsHidden)
            {
                _renderer.Active = true;
                ParentObject.Transform.SetLocalPosition(newPos.Value + _markOffset);
            }
            else
            {
                _renderer.Active = false;
            }

            if (!_agent.AgentData.Alive)
            {
                _renderer.Active = false;
                Active = false;
            }
        }
    }

    public override void Initialize()
    {
        
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>HiddenIndicator</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<markOffset>");
        builder.Append("<x>" + _markOffset.X + "</x>");
        builder.Append("<y>" + _markOffset.Y + "</y>");
        builder.Append("<z>" + _markOffset.Z + "</z>");
        builder.Append("</markOffset>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        XElement offset = element.Element("markOffset");
        _markOffset = new Vector3(float.Parse(offset.Element("x").Value), float.Parse(offset.Element("y").Value), float.Parse(offset.Element("z").Value));
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
        _agent = null;
        _renderer = null;
    }

#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("HiddenIndicator"))
        {
            ImGui.Checkbox("Hidden indicator active", ref Active);
            System.Numerics.Vector3 bar = _markOffset.ToNumerics();
            if (ImGui.DragFloat3("Indicator offset", ref bar,0.1f))
            {
                _markOffset = bar;
            }
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
    }
#endif
}