using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace RTS_Engine;

public class Emitter : Component
{
    
    public UnitType Type;
    public AudioEmitter AudioEmitter;
    public AudioListener Listener;
    public static SoundEffect Idle;
    public static SoundEffect Move;
    public static SoundEffect Attack;
    private float volume = 1.0f;
    private float scale = 1.0f;
    
    public SoundEffectInstance IdleInstance;
    public SoundEffectInstance MoveInstance;
    public SoundEffectInstance AttackInstance;
    public override void Initialize()
    {
        Active = true;
        AudioEmitter = new AudioEmitter();
        Listener = Globals.Listener;
        SetType(Type);
    }

    public Emitter(UnitType type)
    {
        Type = type;
    }
    
    public Emitter()
    {
        
    }
    
    private void SetType(UnitType type)
    {
        Type = type;

        switch (type)
        {
            case UnitType.Cabinet:
                Idle = AssetManager.CabinetIdle;
                Move = AssetManager.CabinetMove;
                Attack = AssetManager.CabinetAttack;
                break;
            case UnitType.Chair:
                Idle = AssetManager.ChairIdle;
                Move = AssetManager.ChairMove;
                Attack = AssetManager.ChairAttack;
                break;
            case UnitType.Candle:
                Idle = AssetManager.CandleIdle;
                Move = AssetManager.CandleMove;
                Attack = AssetManager.CandleAttack;
                break;
            case UnitType.Chandelier:
                Idle = AssetManager.ChandelierIdle;
                Move = AssetManager.ChandelierMove;
                Attack = AssetManager.ChandelierAttack;
                break;
            case UnitType.MiniCabinet:
                Idle = AssetManager.MiniCabinetIdle;
                Move = AssetManager.MiniCabinetMove;
                Attack = AssetManager.MiniCabinetAttack;
                break;
            case UnitType.Wardrobe:
                Idle = AssetManager.WardrobeIdle;
                Move = AssetManager.WardrobeMove;
                Attack = AssetManager.WardrobeAttack;
                break;
            default:
                Console.WriteLine("No sound effect for this unit type");
                break;
        }
        
        IdleInstance = Idle.CreateInstance();
        MoveInstance = Move.CreateInstance();
        AttackInstance = Attack.CreateInstance();
        
    }

    public void PlayIdle()
    {
        IdleInstance.Play();
    }
    
    public void PlayMove()
    {
        MoveInstance.Play();
    }
    
    public void PlayAttack()
    {
        AttackInstance.Play();
    }
    public override void Update()
    {
        Listener = Globals.Listener;
        AudioEmitter.Position = ParentObject.Transform.Pos;
        MoveInstance.Apply3D(Listener, AudioEmitter);
        IdleInstance.Apply3D(Listener, AudioEmitter);
        AttackInstance.Apply3D(Listener, AudioEmitter);
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
            
            ImGui.Text("Change unit type:");
            
            if(ImGui.Button("Cabinet"))
                SetType(UnitType.Cabinet);
            
            if(ImGui.Button("Candle"))
                SetType(UnitType.Candle);
            
            if(ImGui.Button("Chair"))
                SetType(UnitType.Chair);
            
            if(ImGui.Button("Chandelier"))
                SetType(UnitType.Chandelier);
            
            if(ImGui.Button("MiniCabinet"))
                SetType(UnitType.MiniCabinet);
            
            if(ImGui.Button("Wardrobe"))
                SetType(UnitType.Wardrobe);
            
            ImGui.Text("Unit type: " + Type);
            
            if (ImGui.Button("PlayIdle"))
            {
                IdleInstance.Play();
            }
            
            if (ImGui.Button("PlayMove"))
            {
                MoveInstance.Play();
            }
            
            if (ImGui.Button("PlayAttack"))
            {
                AttackInstance.Play();
            }

            if (ImGui.DragFloat("Volume", ref volume, 0.01f, 0f, 1f))
            {
                IdleInstance.Volume = volume;
                MoveInstance.Volume = volume;
                AttackInstance.Volume = volume;
            }
            
            if (ImGui.DragFloat("Distance Scale", ref scale, 0.1f, 0f, 100f))
            {
                SoundEffect.DistanceScale = scale;
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


