using ImGuiNET;

namespace RTS_Engine.Components.AI.AgentData;

public class SoldierData : AgentData
{
    
    
    
    
#if DEBUG
    public override void Inspect()
    {
        base.Inspect();
        //ImGui.DragFloat("Unit presence", ref Presence);
    }
#endif
}