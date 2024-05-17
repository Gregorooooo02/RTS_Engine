using System.Drawing;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using RTS_Engine.Pathfinding;

namespace RTS_Engine;

public class Pathfinder : Component
{
    public Node[,] nodes;
    public override void Update()
    {
      
    }
    
    public override void Initialize()
    {
        nodes = new Node[10, 10];
        Active = true;
        // example map, delete later
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                if (i == 2 && j == 2 || i == 3 && j == 2 || i == 3 && j == 3 || i == 3 && j == 4 || i == 3 && j == 5 || i == 4 && j == 5)
                {
                    nodes[i, j] = new Node(new Point(i, j), false);
                    continue;
                }
                {
                    nodes[i, j] = new Node(new Point(i, j), true);
                }
            }
        }
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Pathfinder</type>");
        
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