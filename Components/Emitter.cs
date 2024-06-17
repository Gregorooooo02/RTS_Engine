using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace RTS_Engine;

public class Emitter : Component
{
    
    // public enum UnitType
    // {
    //     Knight,
    //     Civillian,
    //     Chandelier,
    //     Chair,
    //     Umulung
    // }
    //
    // public enum SoundType
    // {
    //     Idle,
    //     Death,
    //     Attack,
    //     Hit,
    //     Move,
    //     Fight
    // }

    public AudioEmitter AudioEmitter;
    public AudioListener Listener;
    public static SoundEffect Sound;
    public SoundEffectInstance SoundEffectInstance = Sound.CreateInstance();
    public override void Initialize()
    {
        Active = true;
        AudioEmitter = new AudioEmitter();
        Globals.AudioManager.Emitters.Add(AudioEmitter);
        Listener = Globals.Listener;
    }
    
    public Emitter(){}

    public void PlaySound()
    {
        SoundEffectInstance.Play();
    }
    public override void Update()
    {
        Listener = Globals.Listener;
        SoundEffectInstance.Apply3D(Listener, AudioEmitter);
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Emitter</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    
    
    
#if DEBUG   
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Emitter"))
        {
            ImGui.Checkbox("Emitter active", ref Active);
            if (ImGui.Button("Remove component"))
            {
                ParentObject.RemoveComponent(this);
            }
            
            System.Numerics.Vector3 pos = AudioEmitter.Position.ToNumerics();
            if (ImGui.DragFloat3("Position", ref pos,0.1f))
            {
                AudioEmitter.Position = pos;
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


