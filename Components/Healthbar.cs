using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI;

namespace RTS_Engine;

public class Healthbar : Component
{
    private Agent _agent = null;
    private GameObject _barBck = null;
    private GameObject _barFill = null;

    private Vector3 _fillingOffset;
    private Vector3 _barOffset;
    
    public override void Update()
    {
        _agent ??= ParentObject.Parent.GetComponent<Agent>();
        _barBck ??= ParentObject.Children[0];
        _barFill ??= ParentObject.Children[1];
        
        if (_agent != null && _barBck != null && _barFill != null)
        {
            if (!Active || _agent.AgentData.Hp >= _agent.AgentData.MaxHp || _agent.AgentData.Hp <= 0)
            {
                _barBck.Active = false;
                _barFill.Active = false;
                return;
            }
            _barBck.Active = true;
            _barFill.Active = true;
            Vector3? newPos = PickingManager.CalculatePositionOnScreen(_agent.Position);
            if (newPos.HasValue)
            {
                _barBck.Transform.SetLocalPosition(newPos.Value + _barOffset);
                _barFill.Transform.SetLocalPosition(newPos.Value + _fillingOffset);
                _barFill.Transform.SetLocalScaleX(MathF.Min(_agent.AgentData.Hp / _agent.AgentData.MaxHp, 1.0f) * _barFill.Transform.Scl.Z);
            }
            else
            {
                _barBck.Active = false;
                _barFill.Active = false;
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
        
        builder.Append("<type>Healthbar</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<barOffset>");
        builder.Append("<x>" + _barOffset.X + "</x>");
        builder.Append("<y>" + _barOffset.Y + "</y>");
        builder.Append("<z>" + _barOffset.Z + "</z>");
        builder.Append("</barOffset>");
        
        builder.Append("<fillingOffset>");
        builder.Append("<x>" + _fillingOffset.X + "</x>");
        builder.Append("<y>" + _fillingOffset.Y + "</y>");
        builder.Append("<z>" + _fillingOffset.Z + "</z>");
        builder.Append("</fillingOffset>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        XElement offsetBar = element.Element("barOffset");
        _barOffset = new Vector3(float.Parse(offsetBar.Element("x").Value), float.Parse(offsetBar.Element("y").Value), float.Parse(offsetBar.Element("z").Value));
        XElement offsetFill = element.Element("fillingOffset");
        _fillingOffset = new Vector3(float.Parse(offsetFill.Element("x").Value),float.Parse(offsetFill.Element("y").Value),float.Parse(offsetFill.Element("z").Value));
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Healthbar"))
        {
            ImGui.Checkbox("Healthbar active", ref Active);
            System.Numerics.Vector3 bar = _barOffset.ToNumerics();
            if (ImGui.DragFloat3("Bar offset", ref bar,0.1f))
            {
                _barOffset = bar;
            }
            System.Numerics.Vector3 fill = _fillingOffset.ToNumerics();
            if (ImGui.DragFloat3("Filling offset", ref fill,0.1f))
            {
                _fillingOffset = fill;
            }
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
    }
#endif
}
