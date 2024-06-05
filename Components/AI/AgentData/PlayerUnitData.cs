using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.Agent_States;

namespace RTS_Engine.Components.AI.AgentData;

public class PlayerUnitData : AgentData
{
    public float Presence = 5.0f;

    public float WalkingSpeed = 8.0f;
    
    public float Damage = 10.0f;
    public float AttackDelay = 0.8f;

    public float MinPointDistance = 0.5f;

    public Agent Target;


    private Vector2 _destination;
    public Vector2 Destination
    {
        get => _destination;
        set
        {
            _destination = value;
            MovementScheduled = true;
        }
    }
    public bool MovementScheduled = false;
    
    public PlayerUnitData() : base(100){}
    
    public readonly AgentState EntryState = new UnitStart();

    public override string Serialize()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(base.Serialize());
        
        builder.Append("<presence>" + Presence + "</presence>");
        
        builder.Append("<walkingSpeed>" + WalkingSpeed + "</walkingSpeed>");
        
        builder.Append("<damage>" + Damage + "</damage>");

        builder.Append("<attackDelay>" + AttackDelay + "</attackDelay>");

        builder.Append("<minPointDistance>" + MinPointDistance + "</minPointDistance>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        base.Deserialize(element);
        
        Presence = float.TryParse(element.Element("presence")?.Value, out float presence) ? presence : 5.0f;
        WalkingSpeed = float.TryParse(element.Element("walkingSpeed")?.Value, out float walkingSpeed) ? walkingSpeed : 8.0f;
        Damage = float.TryParse(element.Element("damage")?.Value, out float damage) ? damage : 10.0f;
        AttackDelay = float.TryParse(element.Element("attackDelay")?.Value, out float attackDelay) ? attackDelay : 0.8f;
        MinPointDistance = float.TryParse(element.Element("minPointDistance")?.Value, out float minPointDistance) ? minPointDistance : 0.5f;
    }

#if DEBUG
    public override void Inspect()
    {
        base.Inspect();
        ImGui.DragFloat("Unit presence", ref Presence);
        ImGui.Separator();
        ImGui.DragFloat("Walking speed", ref WalkingSpeed);
        ImGui.Separator();
        ImGui.DragFloat("Damage", ref Damage);
        ImGui.DragFloat("Attack delay", ref AttackDelay);
        
    }
#endif
}