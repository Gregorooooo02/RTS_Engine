using ImGuiNET;
using RTS_Engine.Components.AI.Agent_States;

namespace RTS_Engine.Components.AI.AgentData;

public class SoldierData : AgentData
{
    
    
    
    public readonly AgentState EntryState = new SoldierStart();
    
#if DEBUG
    public override void Inspect()
    {
        base.Inspect();
        //ImGui.DragFloat("Unit presence", ref Presence);
    }
#endif
}