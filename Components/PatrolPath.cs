using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace RTS_Engine;

public class PatrolPath : Component
{
    public PatrolPath(){}
    
    public override void Update()
    {
        if(!Active) return;
        if (!Globals.AgentsManager.PatrolManager.PatrolPaths.Contains(this))
        {
            Globals.AgentsManager.PatrolManager.PatrolPaths.Add(this);
        }
    }

    public List<Vector3> GetPathPoints()
    {
        List<Vector3> output = new();
        foreach (GameObject child in ParentObject.Children)
        {
            output.Add(child.Transform.ModelMatrix.Translation);
        }
        return output;
    }
    
    public override void Initialize(){}

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>PatrolPathUnit</type>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element) {}

    public override void RemoveComponent()
    {
        Globals.AgentsManager.PatrolManager.PatrolPaths.Remove(this);
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("PatrolPathUnit"))
        {
            ImGui.Checkbox("Path Unit active", ref Active);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
    }
#endif
}