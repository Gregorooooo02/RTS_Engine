using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class Pickable : Component
{
    public enum PickableType
    {
        Enemy,
        Unit,
        EnemyBuilding,
        Building
    }
    
    public MeshRenderer Renderer = null;

    public PickableType Type;
    
    public override void Update()
    {
        if (Active)
        {
            if (Renderer != null)
            {
                if (Renderer.IsVisible && Renderer.Active)
                {
                    Globals.PickingManager.Pickables.Add(this);
                }
            }
            else
            {
                Initialize();
            }
        }
    }
    
    public override void Initialize()
    {
        Renderer = ParentObject.GetComponent<MeshRenderer>();
        if (Renderer == null)
        {
            Active = false;
        }
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Pickable</type>");
        
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
    private bool _changeType = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Pickable"))
        {
            ImGui.Checkbox("Pickable active", ref Active);
            ImGui.Text("Linked with MeshRenderer from object: " + Renderer?.ParentObject.Name);
            if (ImGui.Button("Change type"))
            {
                _changeType = true;
            }
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }

        if (_changeType)
        {
            ImGui.Begin("Changing type");
            var values = Enum.GetValues(typeof(PickableType));
            foreach (PickableType type in values)
            {
                if (ImGui.Button(type.ToString()))
                {
                    Type = type;
                    _changeType = false;
                    break;
                }
            }
            if (ImGui.Button("Cancel"))
            {
                _changeType = false;
            }
            ImGui.End();
        }
    }
#endif
}