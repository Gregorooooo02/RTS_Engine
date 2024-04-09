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
    public override void Update(){}

    public override void Initialize()
    {
        Active = true;
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