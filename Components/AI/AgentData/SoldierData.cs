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
    
    public int pathID;
    public PatrolType PatrollingType;

    public float MinPointDistance = 0.3f;
    public float PatrollingSpeed = 7.0f;
    
    public float MaxIdleTime = 5;
    public float MinIdleTime = 2;
    
    
    public SoldierData() : base(){}


    
    public readonly AgentState EntryState = new SoldierStart();
    
#if DEBUG
    private bool _changePatrolType = false;
    public override void Inspect()
    {
        base.Inspect();
        ImGui.InputInt("Patrol path ID", ref pathID);
        ImGui.Text("Patrolling type: " + PatrollingType);
        if (ImGui.Button("Change patrolling type"))
        {
            _changePatrolType = true;
        }
        ImGui.Separator();
        ImGui.DragFloat("Patrolling speed", ref PatrollingSpeed);
        ImGui.Separator();
        ImGui.DragFloat("Max idle time", ref MaxIdleTime);
        ImGui.DragFloat("Min idle time", ref MinIdleTime);
        ImGui.Separator();
        ImGui.DragFloat("Min point distance", ref MinPointDistance);
        
        
        
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