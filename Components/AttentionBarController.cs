using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class AttentionBarController : Component
{
    public override void Update()
    {
        ParentObject.Transform.SetLocalScaleX(0.8f * MathF.Min(GameManager.CurrentAwareness / GameManager.AwarenessLimit,1.0f)  * Globals.Ratio);
    }

    public AttentionBarController(){}
    
    public override void Initialize()
    {
        
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>AttentionBarController</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if (ImGui.CollapsingHeader("AttentionBarController"))
        {
            ImGui.Checkbox("AttentionBarController active", ref Active);
        }
    }
#endif
}