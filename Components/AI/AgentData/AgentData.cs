using ImGuiNET;

namespace RTS_Engine.Components.AI.AgentData;

public class AgentData
{
    public float Hp = 100.0f;
    public float MaxHp;

    public bool Alive = true;
    
    public AgentData()
    {
        Hp = MaxHp;
    }
    
    public AgentData(float maxHp)
    {
        MaxHp = maxHp;
        Hp = MaxHp;
    }

    public void DealDamage(float damage)
    {
        Hp -= damage;
        if (Hp <= 0) Alive = false;
    }

#if DEBUG
    public virtual void Inspect()
    {
        ImGui.DragFloat("Max HP", ref MaxHp);
        ImGui.Text("Current Hp: " + Hp);
    }
#endif
}