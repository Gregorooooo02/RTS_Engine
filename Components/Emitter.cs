using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace RTS_Engine;

public class Emitter : Component
{

    public AudioEmitter AudioEmitter;
    private AudioListener listener;
    public static SoundEffect _soundEffect;
    public static SoundEffectInstance _soundEffectInstance;
    public override void Initialize()
    {
        Active = true;
        AudioEmitter = new AudioEmitter();
        AudioEmitter.Position = new Vector3(0f, 0f, 0f);
        listener = new AudioListener();
        listener.Position = new Vector3(0, 0, 0);
        _soundEffect = AssetManager.DefaultAmbientMusic;
        _soundEffectInstance = _soundEffect.CreateInstance();
        _soundEffectInstance.IsLooped = true;
        
    }
    
    public Emitter(){}
    
    private void PlaySound()
    {
        
    }
    
    public override void Update()
    {
        AudioEmitter.Position = AudioEmitter.Position + new Vector3(10.0f, 0 , 0);
        _soundEffectInstance.Apply3D(listener, AudioEmitter);
    }
    
    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Emitter</type>");
        
        builder.Append("<active>" + Active + "</active>");
        
        builder.Append("<position>" + AudioEmitter.Position + "</position>");
        
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
            if (ImGui.Button("Play sound"))
            {
                // listener.Position = new Vector3(0, 0, 0);
                
                _soundEffectInstance.Play();
            }
            
            if (ImGui.Button("Move Listener"))
            {
                listener.Position =  new Vector3(listener.Position.X - 1000.0f, 0, 0);
                

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


