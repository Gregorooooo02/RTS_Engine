using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework.Audio;

namespace RTS_Engine;

public class Emitter : Component
{
    
    private UnitType Type;
    
    private AudioEmitter AudioEmitter;
    
    private AudioListener Listener;
    private List<SoundEffect> Idle = new List<SoundEffect>();
    private List<SoundEffect> Move = new List<SoundEffect>();
    private List<SoundEffect> Attack = new List<SoundEffect>();
    private List<SoundEffect> Damage = new List<SoundEffect>();
    private List<SoundEffect> Flee = new List<SoundEffect>();
    private float volume;
    private float scale;
    
    private SoundEffectInstance IdleInstance;
    private SoundEffectInstance MoveInstance;
    private SoundEffectInstance AttackInstance;
    private SoundEffectInstance DamageInstance;
    private SoundEffectInstance FleeInstance;
    
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
        Initialize();
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
                SetFurnitureInstances();
                break;
            case UnitType.Chair:
                Idle = AssetManager.ChairIdle;
                Move = AssetManager.ChairMove;
                Attack = AssetManager.ChairAttack;
                SetFurnitureInstances();
                break;
            case UnitType.Candle:
                Idle = AssetManager.CandleIdle;
                Move = AssetManager.CandleMove;
                Attack = AssetManager.CandleAttack;
                SetFurnitureInstances();
                break;
            case UnitType.Chandelier:
                Idle = AssetManager.ChandelierIdle;
                Move = AssetManager.ChandelierMove;
                Attack = AssetManager.ChandelierAttack;
                SetFurnitureInstances();
                break;
            case UnitType.MiniCabinet:
                Idle = AssetManager.MiniCabinetIdle;
                Move = AssetManager.MiniCabinetMove;
                Attack = AssetManager.MiniCabinetAttack;
                SetFurnitureInstances();
                break;
            case UnitType.Wardrobe:
                Idle = AssetManager.WardrobeIdle;
                Move = AssetManager.WardrobeMove;
                Attack = AssetManager.WardrobeAttack;
                SetFurnitureInstances();
                break;
            case UnitType.Archer:
                Idle = AssetManager.ArcherIdle;
                Attack = AssetManager.ArcherAttack;
                Damage = AssetManager.ArcherDamage;
                SetKnightArcherInstances();
                break;
            case UnitType.Civilian:
                Flee = AssetManager.CivilianFlee;
                Idle = AssetManager.CivilianIdle;
                Damage = AssetManager.CivilianDamage;
                SetCivilianInstances();
                break;
            case UnitType.Knight:
                Idle = AssetManager.KnightIdle;
                Attack = AssetManager.KnightAttack;
                Damage = AssetManager.KnightDamage;
                SetKnightArcherInstances();
                break;
            default:
                Console.WriteLine("No sound effect for this unit type");
                break;
        }

        void SetFurnitureInstances()
        {
            IdleInstance = RandomSound(Idle).CreateInstance();
            MoveInstance = RandomSound(Move).CreateInstance();
            AttackInstance = RandomSound(Attack).CreateInstance();
        }

        void SetKnightArcherInstances()
        {
            IdleInstance = RandomSound(Idle).CreateInstance();
            AttackInstance = RandomSound(Attack).CreateInstance();
            DamageInstance = RandomSound(Damage).CreateInstance();
        }

        void SetCivilianInstances()
        {
            DamageInstance = RandomSound(Damage).CreateInstance();
            FleeInstance = RandomSound(Flee).CreateInstance();
            IdleInstance = RandomSound(Idle).CreateInstance();
        }
    }

    public SoundEffect RandomSound(List<SoundEffect> sounds)
    {
        Random random = new Random();
        int index = random.Next(sounds.Count);
        return sounds[index];
    }
    public void PlayIdle()
    {
        IdleInstance = RandomSound(Idle).CreateInstance();
        IdleInstance.Play();
    }
    
    public void PlayMove()
    {
        MoveInstance = RandomSound(Move).CreateInstance();
        MoveInstance.Play();
    }
    
    public void PlayAttack()
    {
        AttackInstance = RandomSound(Attack).CreateInstance();
        AttackInstance.Play();
    }

    public void PlayWander()
    {
        IdleInstance = RandomSound(Idle).CreateInstance();
        IdleInstance.Play();
    }

    public void PlayDamage()
    {
        DamageInstance = RandomSound(Damage).CreateInstance();
        DamageInstance.Play();
    }

    public void PlayFlee()
    {
        FleeInstance = RandomSound(Flee).CreateInstance();
        FleeInstance.Play();
    }
    
    public static void PlayMissionTheme()
    {
        AudioManager.PlayMissionTheme();
    }
    public override void Update()
    {
        Listener = Globals.Listener;
        // EmitterPosition = ParentObject.Transform.Pos;

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
        
        builder.Append("<emitterType>" + Type + "</emitterType>");
        
        builder.Append("<emitterVolume>" + volume + "</emitterVolume>");
        
        builder.Append("<emitterScale>" + scale + "</emitterScale>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        SetType((UnitType)Enum.Parse(typeof(UnitType), element.Element("emitterType")?.Value));
        volume = float.TryParse(element.Element("emitterVolume")?.Value, out float vol) ? vol : 1.0f;
        scale = float.TryParse(element.Element("emitterScale")?.Value, out float sc) ? sc : 1.0f;
        //SoundEffect.DistanceScale = scale;
    }
    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
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
            
            if (ImGui.Button("Play theme"))
            {
                PlayMissionTheme();
            }
            
            ImGui.Text("Change unit type:");

            var values = Enum.GetValues(typeof(UnitType));

            foreach (UnitType type in values)
            {
                if (ImGui.Button(type.ToString()))
                {
                    SetType(type);
                }
            }
            
            if(ImGui.Button("Archer"))
                SetType(UnitType.Archer);
            
            if(ImGui.Button("Civilian"))
                SetType(UnitType.Civilian);
            
            if(ImGui.Button("Knight"))
                SetType(UnitType.Knight);
            
            ImGui.Text("Unit type: " + Type);
            
            if (ImGui.Button("PlayIdle"))
            {
                PlayIdle();
            }
            
            if (ImGui.Button("PlayMove"))
            {
                PlayMove();
            }
            
            if (ImGui.Button("PlayAttack"))
            {
                PlayAttack();
            }
            
            if (ImGui.Button("PlayDamage"))
            {
                PlayDamage();
            }
            
            if (ImGui.Button("PlayFlee"))
            {
                PlayFlee();
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
}


