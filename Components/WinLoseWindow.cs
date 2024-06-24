using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class WinLoseWindow : Component
{
    private GameAction action = GameAction.WIN;
    
    public override void Update()
    {
        GameAction result = GameManager.CheckIfGameOver();
        if (result == action)
        {
            ParentObject.ToggleChildrenActive(true);
        }
    }

    public override void Initialize()
    {
        
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>WinLoseWindow</type>");
        
        builder.Append("<active>" + Active +"</active>");

        builder.Append("<action>" + action + "</action>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        Enum.TryParse(element?.Element("action")?.Value, out GameAction result);
        action = result;
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    private bool _changeAction = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("WinLoseWindow"))
        {
            ImGui.Checkbox("WinLoseWindow active", ref Active);
            ImGui.Text("Selected action: " + action);
            if (ImGui.Button("Change action"))
            {
                _changeAction = true;
            }
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }

        if (_changeAction)
        {
            ImGui.Begin("Changing action");
            var values = Enum.GetValues(typeof(GameAction));
            foreach (GameAction type in values)
            {
                if (ImGui.Button(type.ToString()))
                {
                    action = type;
                    _changeAction = false;
                    break;
                }
            }
            if (ImGui.Button("Cancel"))
            {
                _changeAction = false;
            }
            ImGui.End();
        }
    }
#endif
}