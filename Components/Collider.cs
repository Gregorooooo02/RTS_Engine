using System.Text;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using ImGuiNET;

namespace RTS_Engine;

public class Collider : Component
{
    public bool Active;
    private string name;
    public bool isColliding = false;
    public BoundingSphere sphere;

    public override void Update()
    {
        for (int meshIndex1 = 0; meshIndex1 < ParentObject.GetComponent<MeshRenderer>().GetModel().Meshes.Count; meshIndex1++)
        {
            sphere = ParentObject.GetComponent<MeshRenderer>().GetModel().Meshes[meshIndex1].BoundingSphere;
            sphere = sphere.Transform(ParentObject.Transform.ModelMatrix);
            sphere.Radius = ParentObject.Transform._scl.X;
        }
    }

    public Collider(GameObject parentObject)
    {
        ParentObject = parentObject;
        Initialize();
    }
    
    public Collider()
    {
        
    }

    public override void Initialize()
    {
        Active = true;
        name = ParentObject.Name + "_collider";
    }
    
    public override void Draw(Matrix _view, Matrix _projection){}
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>MeshRenderer</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<model>" + name +"</model>");
        
        builder.Append("</component>");
        return builder.ToString();
    }
    
    public override void Deserialize(XElement element){}
    
    public override void Inspect(){}
}