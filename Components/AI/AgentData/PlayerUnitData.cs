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
    public float MinAttackRange = 1.5f;
    public float MaxAttackRange = 2.5f;

    public float MinPointDistance = 0.5f;
    public float RepathDelay = 2.0f;

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
        ImGui.DragFloat("Min attack range", ref MinAttackRange);
        ImGui.DragFloat("Max attack range", ref MaxAttackRange);
        ImGui.Separator();
        ImGui.DragFloat("Min point distance", ref MinPointDistance);
        ImGui.Separator();
        ImGui.DragFloat("Repath delay", ref RepathDelay,0.1f,0.3f,10);
    }
#endif
}