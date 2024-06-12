using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class Audio : Component
{
    public override void Initialize()
    {
        Active = true;
    }
    
    public Audio(){}
    
    public override void Update()
    {
        throw new NotImplementedException();
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Audio</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("</component>");
        return builder.ToString();
    }
    
    
#if DEBUG   
public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Audio"))
        {
            ImGui.Checkbox("Audio active", ref Active);
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
        }
    }
#endif
    
    public override void Deserialize(XElement element){}
    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }
}