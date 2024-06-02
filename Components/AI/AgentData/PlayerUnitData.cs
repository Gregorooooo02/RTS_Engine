using ImGuiNET;
using RTS_Engine.Components.AI.Agent_States;

namespace RTS_Engine.Components.AI.AgentData;

public class PlayerUnitData : AgentData
{
    public float Presence = 5.0f;
    
    
    
    
    public readonly AgentState EntryState = new UnitStart();
    
#if DEBUG
    public override void Inspect()
    {
        base.Inspect();
        ImGui.DragFloat("Unit presence", ref Presence);

    }
#endif
}