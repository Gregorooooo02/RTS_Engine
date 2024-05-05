using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace RTS_Engine;

public class Layer : Component
{
    private string name;
    private string layerType = Globals.LayerType.DEFAULT.ToString();
    
    
    public override void Initialize()
    {
        Active = true;
        name = ParentObject.Name + "Layer";
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Layer</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("<model>" + name + "</model>");
        
        builder.Append("<layerType>" + layerType + "</layerType>");
        
        builder.Append("</component>");
        return builder.ToString();
    }
    
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Layer"))
        {
            ImGui.Checkbox("Layer active", ref Active);
            ImGui.Text(name);
            ImGui.Text("Layer type: " + layerType);
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
        }
    }
    
    public override void Deserialize(XElement element){}

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }
    
    public Layer(){}
    
    public void SetLayerType(string layerType)
    {
        this.layerType = layerType;
    }
    
    public override void Update(){}
}