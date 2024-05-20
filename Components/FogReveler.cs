using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Point = System.Drawing.Point;

namespace RTS_Engine;

public class FogReveler : Component
{
    private Vector3 _previousPos;
    public float RevealRadius = 7.0f;
    public bool Changed = true;

    public Point PreviousPosition;
    public Point CurrentPosition;

    private bool _added = false;
    
    public FogReveler(){}
    
    public override void Update()
    {
        if (!Active)
        {
            if (_added)
            {
                Globals.FogManager.Revelers.Remove(this);
                _added = false;
            }
        }
        else
        {
            if (_added)
            {
                if (!_previousPos.Equals(ParentObject.Transform.ModelMatrix.Translation))
                {
                    _previousPos = ParentObject.Transform.ModelMatrix.Translation;
                    Changed = true;
                    Globals.FogManager.Changed = true;
                    CurrentPosition = new Point((int)ParentObject.Transform.ModelMatrix.Translation.X, (int)ParentObject.Transform.ModelMatrix.Translation.Z);
                }
            }
            else
            {
                Globals.FogManager.Revelers.Add(this);
                _added = true;
            }
        }
    }

    public override void Initialize(){}

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>FogReveler</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<revealRadius>" + RevealRadius +"</revealRadius>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        RevealRadius = float.TryParse(element.Element("revealRadius")?.Value, out float revealRadius) ? revealRadius : 7.0f;
    }

    public override void RemoveComponent()
    {
        if (_added) Globals.FogManager.Revelers.Remove(this);

        ParentObject.RemoveComponent(this);
    }

    #if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Fog Reveler"))
        {
            ImGui.Checkbox("Reveler active", ref Active);
            ImGui.DragFloat("Reveal radius", ref RevealRadius);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }   
    }
    #endif
}