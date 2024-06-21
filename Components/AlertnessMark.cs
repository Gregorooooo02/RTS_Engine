using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine;

public class AlertnessMark : Component
{
    private Agent _agent;
    private SpiteRenderer _renderer;
    
    private Vector3 _markOffset;
    
    private SoldierData _soldierData;
    private WandererData _wandererData;
    
    private bool _isSoldier = false;
    public override void Update()
    {
        //Initialize some values
        if(!Active) return;
        
        _agent ??= ParentObject.Parent.GetComponent<Agent>();
        _renderer ??= ParentObject.GetComponent<SpiteRenderer>();
        if (_agent != null && _wandererData == null && _soldierData == null)
        {
            if (_agent.Type == Agent.AgentType.Civilian)
            {
                _wandererData = (WandererData)_agent.AgentData;
                _isSoldier = false;
            } 
            else if (_agent.Type == Agent.AgentType.Soldier)
            {
                _soldierData = (SoldierData)_agent.AgentData;
                _isSoldier = true;
            }
        }
        
        if (_agent != null && _renderer != null)
        {
            
            if (_isSoldier && _soldierData != null)
            {
                Vector3? newPos = PickingManager.CalculatePositionOnScreen(_agent.Position);
                if (newPos.HasValue && _soldierData.Alarmed)
                {
                    _renderer.Active = true;
                    ParentObject.Transform.SetLocalPosition(newPos.Value + _markOffset);
                }
                else
                {
                    _renderer.Active = false;
                }
            }
            else if (!_isSoldier && _wandererData != null)
            {
                Vector3? newPos = PickingManager.CalculatePositionOnScreen(_agent.Position);
                if (newPos.HasValue && _wandererData.Alarmed)
                {
                    _renderer.Active = true;
                    ParentObject.Transform.SetLocalPosition(newPos.Value + _markOffset);
                }
                else
                {
                    _renderer.Active = false;
                }
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
        
        builder.Append("<type>AlertnessMark</type>");
        
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
        _wandererData = null;
        _soldierData = null;
        _renderer = null;
    }

#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("AwarenessBar"))
        {
            ImGui.Checkbox("Alertness active", ref Active);
            System.Numerics.Vector3 bar = _markOffset.ToNumerics();
            if (ImGui.DragFloat3("Mark offset", ref bar,0.1f))
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