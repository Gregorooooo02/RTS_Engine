using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Point = System.Drawing.Point;

namespace RTS_Engine;

public class FogReveler : Component
{
    private Vector3 _previousPos;
    public int RevealRadius = 7;
    private int PreviousRadius = 7;
    public bool Changed = true;

    public Point PreviousPosition;
    public Point CurrentPosition;

    public Texture2D Track;

    private bool _added = false;
    public bool OneUse = false;
    
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
            if (PreviousRadius != RevealRadius)
            {
                PreviousRadius = RevealRadius;
                Track = Globals.FogManager.GetCircleTexture(RevealRadius);
            }
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
                Track = Globals.FogManager.GetCircleTexture(RevealRadius);
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
        
        builder.Append("<oneUse>" + OneUse +"</oneUse>");
        
        builder.Append("<revealRadius>" + RevealRadius +"</revealRadius>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        OneUse = element.Element("oneUse")?.Value == "True";
        RevealRadius = int.TryParse(element.Element("revealRadius")?.Value, out int revealRadius) ? revealRadius : 7;
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
            ImGui.Checkbox("Is used only once?", ref OneUse);
            ImGui.InputInt("Reveal radius", ref RevealRadius);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }   
    }
    #endif
}