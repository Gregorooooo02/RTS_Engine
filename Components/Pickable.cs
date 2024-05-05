using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class Pickable : Component
{
    public MeshRenderer Renderer = null;
    
    public override void Update()
    {
        if (Active)
        {
            if (Renderer is { IsVisible: true })
            {
                Globals.PickingManager.Pickables.Add(this);
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
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {}

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }
#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Pickable"))
        {
            ImGui.Checkbox("Pickable active", ref Active);
            ImGui.Text("Linked with MeshRenderer from object: " + Renderer?.ParentObject.Name);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }   
    }
#endif
}