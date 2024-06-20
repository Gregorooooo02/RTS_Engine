using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine.Components.AI.AgentData;

public class AgentData
{
    public float Hp;
    public float MaxHp;
    
    public float HpAsPercentage => Hp / MaxHp;

    public bool Alive = true;
    
    public AgentData()
    {
        Hp = 100.0f; 
        Hp = MaxHp;
    }
    
    public AgentData(float maxHp)
    {
        MaxHp = maxHp;
        Hp = MaxHp;
    }

    public void DealDamage(float damage)
    {
        if(!Alive) return;
        Hp -= damage;
        if (Hp <= 0) Alive = false;
    }

    public virtual string Serialize()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<hp>" + Hp + "</hp>");
        
        builder.Append("<maxHp>" + MaxHp + "</maxHp>");
        
        builder.Append("<alive>" + Alive + "</alive>");
        
        return builder.ToString();
    }

    public virtual void Deserialize(XElement element)
    {
        Hp = float.TryParse(element.Element("hp")?.Value, out float hp) ? hp : 100.0f;
        MaxHp = float.TryParse(element.Element("maxHp")?.Value, out float maxHp) ? maxHp : 100.0f;
        Alive = !bool.TryParse(element.Element("alive")?.Value, out bool alive) || alive;
    }

#if DEBUG
    public virtual void Inspect()
    {
        ImGui.DragFloat("Max HP", ref MaxHp);
        ImGui.DragFloat("HP", ref Hp);
        ImGui.Text("Current Hp: " + Hp);
        ImGui.SameLine();
        if (ImGui.Button("Heal"))
        {
            Hp = MaxHp;
            Alive = true;
        }
        ImGui.SameLine();
        if (ImGui.Button("Kill"))
        {
            DealDamage(MaxHp);
        }
    }
#endif
}