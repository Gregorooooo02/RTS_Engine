using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;

namespace RTS_Engine;

public class InstancedRendererUnit : Component
{
    public InstancedRendererController Controller;
    private InstancedRendererController.InstanceData _data;
    public override void Update()
    {
        if (Active)
        {
            if (Controller is { ModelData: not null })
            {
                ModelData modelData = Controller.ModelData;
                if (Globals.BoundingFrustum.Contains(modelData.BoundingSphere
                        .Transform(ParentObject.Transform.ModelMatrix)) != ContainmentType.Disjoint)
                {
                    _data.SetWorld(ParentObject.Transform.ModelMatrix);
                    Controller.WorldMatrices.Add(_data);
                }
            }
            else
            {
                Initialize();
            }
        }
    }

    public InstancedRendererUnit(){}
    
    public InstancedRendererUnit(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }
    
    public override void Initialize()
    {
        Controller = ParentObject.Parent.GetComponent<InstancedRendererController>();
        if (Controller == null)
        {
            Active = false;
        }
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>InstancedRendererUnit</type>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element) {}

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("InstancedRendererUnit"))
        {
            ImGui.Checkbox("Mesh Instance active", ref Active);
            ImGui.Text("Linked with InstancedRendererController from object: " + Controller?.ParentObject.Name);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }   
    }
#endif
}