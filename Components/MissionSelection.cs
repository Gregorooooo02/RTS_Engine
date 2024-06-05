using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class MissionSelection : Component
{
    public override void Update()
    {
        if (!Active) return;
        
    }
    
    public override void Initialize() {}

    public void OnClick()
    {
        ActivateMissionWindow();
    }

    public void ActivateMissionWindow()
    {
        GameObject missionWindow = ParentObject.Children.Find(child => child.FindGameObjectByName("Mission_Window") != null);
        if (missionWindow != null)
        {
            missionWindow.Active = true;
            Globals.PickingManager.PlayerBuildingPickingActive = false;
        }
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>MissionSelection</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = bool.Parse(element.Element("active").Value);
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("Mission Selection"))
        {
            ImGui.Checkbox("Active", ref Active);
            ImGui.Text("Quick mission selection component.");
            ImGui.Text("Object responsible: " + ParentObject.Name);
        }
    }
}