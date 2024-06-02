using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class PatrolPathController : Component
{
    public List<PatrolPathUnit> PatrolPaths = new();
    
    
    public PatrolPathController(){}
    
    public PatrolPathController(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }
    
    public override void Update(){}

    public override void Initialize(){}

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>PatrolPathController</type>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element){}

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }
#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("PatrolPathController"))
        {
            ImGui.Checkbox("Path Controller active", ref Active);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }
    }
#endif
}