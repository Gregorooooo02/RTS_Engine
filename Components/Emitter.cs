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
    public static SoundEffect sound;
    public SoundEffectInstance soundEffectInstance = sound.CreateInstance();
    public override void Initialize()
    {
        Active = true;
        AudioEmitter = new AudioEmitter();
        Globals.AudioManager.Emitters.Add(AudioEmitter);
    }
    
    public Emitter(){}

    // public void PlaySound(SoundType sound)
    // {
    //     sound switch
    //     {
    //         // SoundType.Idle => Globals.AudioManager.
    //         // SoundType.Death => Globals.AudioManager.
    //         // SoundType.Attack => Globals.AudioManager.
    //         // SoundType.Hit => Globals.AudioManager.
    //         // SoundType.Move => Globals.AudioManager.
    //         // SoundType.Fight => Globals.AudioManager.soundEffectInstance.Play()
    //         _ => throw new ArgumentOutOfRangeException(nameof(sound), sound, null)
    //     };
    // }
    public override void Update()
    {
        soundEffectInstance.Apply3D(AudioEmitter, AudioListener);
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


