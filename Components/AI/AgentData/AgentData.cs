using ImGuiNET;

namespace RTS_Engine.Components.AI.AgentData;

public class AgentData
{
    public float Hp;
    public float MaxHp;


    public AgentData()
    {
        MaxHp = 100;
        Hp = MaxHp;
    }
    
    public AgentData(float maxHp)
    {
        MaxHp = maxHp;
        Hp = MaxHp;
    }

#if DEBUG
    public virtual void Inspect()
    {
        ImGui.DragFloat("Max HP", ref MaxHp);
        ImGui.Text("Current Hp: " + Hp);
    }
#endif
}