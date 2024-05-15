using System.Drawing;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class Pathfinder : Component
{

    public Point[,] nodes;
    public override void Update()
    {
      
    }
    
    public override void Initialize()
    {
        Active = true;
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Pathhfiner</type>");
        
        builder.Append("</component>");
        return builder.ToString();
    }
    
    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }
    
    public override void Deserialize(XElement element)
    {}
    
#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Pathfinder"))
        {
            ImGui.Checkbox("Pathfinder active", ref Active);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
    }
#endif
}