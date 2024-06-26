using System.Text;
using System.Xml.Linq;

namespace RTS_Engine;

public class AmbientMusic : Component
{
    public override void Initialize()
    {
        Active = true;
    }
    
    public AmbientMusic(){}
    
    public override void Update()
    {
        if (Globals.CurrentSceneType == SceneType.BASE)
        {
            AudioManager.PlayBaseTheme();
        }
        else if (Globals.CurrentSceneType == SceneType.MISSION)
        {
            AudioManager.StopBaseTheme();
            AudioManager.PlayMissionTheme();
        }
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>AmbientMusic</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Inspect() {}
    
    public override void Deserialize(XElement element){}
    
    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }
    
}