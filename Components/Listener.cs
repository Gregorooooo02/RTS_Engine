using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework.Audio;

namespace RTS_Engine;

public class Listener : Component
{

    public AudioListener AudioListener;
    public override void Initialize()
    {
        Active = true;
        AudioListener = new AudioListener();
        Globals.Listener = AudioListener;
    }
    
    public Listener(){}

    public override void Update()
    {
        AudioListener.Position = ParentObject.Transform.Pos;
        Globals.Listener = AudioListener;
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Listener</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("<position>" + AudioListener.Position + "</position>");
        
        builder.Append("</component>");
        return builder.ToString();
    }
    
#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Listener"))
        {
            ImGui.Checkbox("Listener active", ref Active);
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
        }
    }
#endif
        
        public override void Deserialize(XElement element){}
        public override void RemoveComponent()
        {
            ParentObject.RemoveComponent(this);
        }
}