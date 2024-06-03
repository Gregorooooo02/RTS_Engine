using System;
using ImGuiNET;
using RTS_Engine.Components.AI.Agent_States;

namespace RTS_Engine.Components.AI.AgentData;

public class SoldierData : AgentData
{
    public enum PatrolType
    {
        Circle,
        GoBack
    }
    
    public int PathId;
    public PatrolType PatrollingType;
    
    
    public float Awareness = 0;
    public float AwarenessThreshold = 100.0f;
    public float AwarenessDecay = 0.1f;

    public float MinPointDistance = 0.3f;
    public float PatrollingSpeed = 7.0f;
    public float AttackingWalkingSpeed = 9.0f;
    
    public float MaxIdleTime = 5;
    public float MinIdleTime = 2;
    
    public float Damage = 10.0f;
    public float AttackDelay = 0.8f;
    public float MinAttackRange = 1.5f;
    public float MaxAttackRange = 2.5f;
    
    public float RepathDelay = 2.0f;
    
    public float SightRange = 40.0f;
    public float SightAngle = 60.0f;
    public float SightHeight = 4.0f;
    public float MinPresenceMultiplier = 0.2f;

    public bool Alarmed = false;
    
    public Agent Target = null;
    public SoldierData() : base(100){}


    
    public readonly AgentState EntryState = new SoldierStart();
    
#if DEBUG
    private bool _changePatrolType = false;
    public override void Inspect()
    {
        base.Inspect();
        ImGui.InputInt("Patrol path ID", ref PathId);
        ImGui.Text("Patrolling type: " + PatrollingType);
        if (ImGui.Button("Change patrolling type"))
        {
            _changePatrolType = true;
        }
        ImGui.Separator();
        ImGui.DragFloat("Patrolling speed", ref PatrollingSpeed);
        ImGui.DragFloat("Attack walking speed", ref AttackingWalkingSpeed);
        ImGui.Separator();
        ImGui.DragFloat("Max idle time", ref MaxIdleTime);
        ImGui.DragFloat("Min idle time", ref MinIdleTime);
        ImGui.Separator();
        ImGui.DragFloat("Min point distance", ref MinPointDistance);
        ImGui.Separator();
        ImGui.DragFloat("Damage", ref Damage);
        ImGui.DragFloat("Attack delay", ref AttackDelay);
        ImGui.DragFloat("Min attack range", ref MinAttackRange);
        ImGui.DragFloat("Max attack range", ref MaxAttackRange);
        ImGui.Separator();
        ImGui.Text("Awareness: " + Awareness);
        ImGui.DragFloat("Awareness threshold", ref AwarenessThreshold);
        ImGui.DragFloat("Awareness decay", ref AwarenessDecay);
        ImGui.Separator();
        ImGui.DragFloat("Sight range", ref SightRange);
        ImGui.DragFloat("Sight angle", ref SightAngle);
        ImGui.DragFloat("Sight height", ref SightHeight);
        ImGui.DragFloat("Min presence multiplier", ref MinPresenceMultiplier,0.01f,0,0.99f);
        
        if (_changePatrolType)
        {
            ImGui.Begin("Change behavior");
            var values = Enum.GetValues(typeof(PatrolType));
            foreach (PatrolType type in values)
            {
                if (ImGui.Button(type.ToString()))
                {
                    PatrollingType = type;
                    _changePatrolType = false;
                }
            }
            if (ImGui.Button("Cancel"))
            {
                _changePatrolType = false;
            }
            ImGui.End();
        }
    }
#endif
}