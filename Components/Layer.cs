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
    public LayerType layer = LayerType.DEFAULT;
    
    public override void Initialize()
    {
        Active = true;
    }
    
    public Layer(){}
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Layer</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("<layerType>" + layer + "</layerType>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

#if DEBUG
    private bool changingLayer = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Layer"))
        {
            ImGui.Checkbox("Layer active", ref Active);
            ImGui.Text("Layer: " + layer);
            if (ImGui.Button("Change layer"))
            {
                changingLayer = true;
            }
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
        }
        if (changingLayer)
        {
            var values = Enum.GetValues(typeof(LayerType));
            ImGui.Begin("Change Layer");
            foreach (LayerType currentLayer in values)
            {
                if (ImGui.Button(currentLayer.ToString()))
                {
                    layer = currentLayer;
                    changingLayer = false;
                }
            }
            if (ImGui.Button("Cancel"))
            {
                changingLayer = false;
            }
            ImGui.End();
        }
    }
#endif
    
    public override void Deserialize(XElement element){}

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }
    
    public override void Update(){}
}