using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.Agent_States;

namespace RTS_Engine.Components.AI.AgentData;

public class PlayerUnitData : AgentData
{
    public float Presence = 5.0f;

    public float WalkingSpeed = 8.0f;
    
    public float Damage = 10.0f;
    public float AttackDelay = 0.8f;
    public float MinAttackRange = 1.5f;
    public float MaxAttackRange = 2.5f;

    public float MinPointDistance = 0.5f;
    public float RepathDelay = 0.5f;

    public bool IsRanged = false;
    public float ProjectileSpeed = 25.0f;
    public float ProjectileMinDistance = 1.0f;
    public ProjectileManager.ProjectileType ProjectileType;

    public float HideTime = 2.0f;

    public Agent Target;
    
    private Vector2 _destination;
    public Vector2 Destination
    {
        get => _destination;
        set
        {
            _destination = value;
            MovementScheduled = true;
        }
    }
    public bool MovementScheduled = false;
    
    public PlayerUnitData() : base(100){}
    
    public readonly AgentState EntryState = new UnitStart();

    public override string Serialize()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(base.Serialize());
        
        builder.Append("<presence>" + Presence + "</presence>");
        
        builder.Append("<walkingSpeed>" + WalkingSpeed + "</walkingSpeed>");
        
        builder.Append("<damage>" + Damage + "</damage>");

        builder.Append("<attackDelay>" + AttackDelay + "</attackDelay>");
        
        builder.Append("<minAttackRange>" + MinAttackRange + "</minAttackRange>");
        
        builder.Append("<maxAttackRange>" + MaxAttackRange + "</maxAttackRange>");

        builder.Append("<minPointDistance>" + MinPointDistance + "</minPointDistance>");
        
        builder.Append("<repathDelay>" + RepathDelay + "</repathDelay>");
        
        builder.Append("<isRanged>" + IsRanged + "</isRanged>");
        
        builder.Append("<projectileSpeed>" + ProjectileSpeed + "</projectileSpeed>");
        
        builder.Append("<projectileMinDistance>" + ProjectileMinDistance + "</projectileMinDistance>");
        
        builder.Append("<projectileType>" + ProjectileType + "</projectileType>");
        
        builder.Append("<hideTime>" + HideTime + "</hideTime>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        base.Deserialize(element);
        
        Presence = float.TryParse(element.Element("presence")?.Value, out float presence) ? presence : 5.0f;
        WalkingSpeed = float.TryParse(element.Element("walkingSpeed")?.Value, out float walkingSpeed) ? walkingSpeed : 8.0f;
        Damage = float.TryParse(element.Element("damage")?.Value, out float damage) ? damage : 10.0f;
        AttackDelay = float.TryParse(element.Element("attackDelay")?.Value, out float attackDelay) ? attackDelay : 0.8f;
        MinAttackRange = float.TryParse(element.Element("minAttackRange")?.Value, out float minAttackRange) ? minAttackRange : 1.5f;
        MaxAttackRange = float.TryParse(element.Element("maxAttackRange")?.Value, out float maxAttackRange) ? maxAttackRange : 2.5f;
        MinPointDistance = float.TryParse(element.Element("minPointDistance")?.Value, out float minPointDistance) ? minPointDistance : 0.5f;
        RepathDelay = float.TryParse(element.Element("repathDelay")?.Value, out float repathDelay) ? repathDelay : 0.5f;
        HideTime = float.TryParse(element.Element("hideTime")?.Value, out float hideTime) ? hideTime : 2.0f;
        IsRanged = element.Element("isRanged")?.Value == "True";
        ProjectileSpeed = float.TryParse(element.Element("projectileSpeed")?.Value, out float projectileSpeed) ? projectileSpeed : 25.0f;
        ProjectileMinDistance = float.TryParse(element.Element("projectileMinDistance")?.Value, out float projectileMinDistance) ? projectileMinDistance : 1.0f;
        ProjectileType = Enum.TryParse(typeof(ProjectileManager.ProjectileType), element.Element("projectileType")?.Value,out var projectileType) ? (ProjectileManager.ProjectileType)projectileType : ProjectileManager.ProjectileType.Arrow;
    }

#if DEBUG
    private bool changeProjectileType = false;
    public override void Inspect()
    {
        base.Inspect();
        ImGui.DragFloat("Hide time", ref HideTime);
        ImGui.Separator();
        ImGui.DragFloat("Unit presence", ref Presence);
        ImGui.Separator();
        ImGui.DragFloat("Walking speed", ref WalkingSpeed);
        ImGui.Separator();
        ImGui.DragFloat("Damage", ref Damage);
        ImGui.DragFloat("Attack delay", ref AttackDelay);
        ImGui.DragFloat("Min attack range", ref MinAttackRange);
        ImGui.DragFloat("Max attack range", ref MaxAttackRange);
        ImGui.Separator();
        ImGui.DragFloat("Min point distance", ref MinPointDistance);
        ImGui.Separator();
        ImGui.DragFloat("Repath delay", ref RepathDelay,0.1f,0.3f,10);
        ImGui.Separator();
        ImGui.Checkbox("Is ranged?", ref IsRanged);
        ImGui.DragFloat("Projectile speed", ref ProjectileSpeed,0.1f, 1, 40);
        ImGui.DragFloat("Projectile min distance", ref ProjectileMinDistance,0.05f, 0.1f, 10);
        ImGui.Text("Current projectile type: " + ProjectileType);
        ImGui.SameLine();
        if (ImGui.Button("Change projectile type"))
        {
            changeProjectileType = true;
        }

        if (changeProjectileType)
        {
            ImGui.Begin("Changing projectile type");
            var values = Enum.GetValues(typeof(ProjectileManager.ProjectileType));
            foreach (ProjectileManager.ProjectileType type in values)
            {
                if (ImGui.Button(type.ToString()))
                {
                    ProjectileType = type;
                    changeProjectileType = false;
                }
            }
            if (ImGui.Button("Cancel"))
            {
                changeProjectileType = false;
            }
            ImGui.End();
        }
    }
#endif
}