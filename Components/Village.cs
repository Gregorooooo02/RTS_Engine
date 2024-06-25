using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using RTS_Engine.Components.AI;

namespace RTS_Engine;

public class Village : Component
{
    private float _attentionBonus = 50.0f;
    
    public override void Update()
    {
        if(!Active) return;
        foreach (GameObject child in ParentObject.Children)
        {
            Agent agent = child.GetComponent<Agent>();
            if(agent != null && agent.AgentData.Alive)
            {
                return;
            }
        }
        GameManager.ChangeAwareness(-_attentionBonus);
        Active = false;
    }

    public override void Initialize()
    {
        
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Village</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("<attentionBonus>" + _attentionBonus + "</attentionBonus>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        _attentionBonus = float.TryParse(element.Element("attentionBonus")?.Value, out float sep) ? sep : 50.0f;
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Village"))
        {
            ImGui.Checkbox("Village active", ref Active);
            ImGui.DragFloat("Attention bonus", ref _attentionBonus);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
    }
#endif
}