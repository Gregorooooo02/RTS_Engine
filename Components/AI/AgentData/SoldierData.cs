using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using RTS_Engine.Components.AI.Agent_States;

namespace RTS_Engine.Components.AI.AgentData;

public class SoldierData : AgentData
{
    public enum PatrolType
    {
        Circle,
        GoBack
    }
    
    public string PathId = "";
    public PatrolType PatrollingType;
    
    public float Awareness = 0;
    public float AwarenessThreshold = 100.0f;
    public float AwarenessDecay = 0.1f;

    public float MinPointDistance = 0.3f;
    public float PatrollingSpeed = 7.0f;
    public float AttackingWalkingSpeed = 9.0f;
    
    public float MaxIdleTime = 5;
    public float MinIdleTime = 2;
    
    public float Damage = 10.0f;
    public float AttackDelay = 0.8f;
    public float MinAttackRange = 1.5f;
    public float MaxAttackRange = 2.5f;
    
    public float RepathDelay = 0.5f;
    
    public float SightRange = 40.0f;
    public float SightAngle = 60.0f;
    public float SightHeight = 4.0f;
    public float MinPresenceMultiplier = 0.2f;

    public float TimeToCoolDown = 10.0f;
    public float MinDistanceToCoolDown = 50.0f;
    
    public bool IsRanged = false;
    public float ProjectileSpeed = 25.0f;
    public float ProjectileMinDistance = 1.0f;
    public ProjectileManager.ProjectileType ProjectileType;

    public bool Alarmed = false;
    
    public Agent Target = null;
    public SoldierData() : base(100){}
    
    public readonly AgentState EntryState = new SoldierStart();

    public override string Serialize()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append(base.Serialize());
        
        builder.Append("<pathId>" + PathId + "</pathId>");
        builder.Append("<patrollingType>" + PatrollingType + "</patrollingType>");
        
        builder.Append("<awareness>" + Awareness + "</awareness>");
        builder.Append("<awarenessThreshold>" + AwarenessThreshold + "</awarenessThreshold>");
        builder.Append("<awarenessDecay>" + AwarenessDecay + "</awarenessDecay>");
        
        builder.Append("<minPointDistance>" + MinPointDistance + "</minPointDistance>");
        builder.Append("<patrollingSpeed>" + PatrollingSpeed + "</patrollingSpeed>");
        builder.Append("<attackingWalkingSpeed>" + AttackingWalkingSpeed + "</attackingWalkingSpeed>");
        
        builder.Append("<maxIdleTime>" + MaxIdleTime + "</maxIdleTime>");
        builder.Append("<minIdleTime>" + MinIdleTime + "</minIdleTime>");
        
        builder.Append("<damage>" + Damage + "</damage>");
        builder.Append("<attackDelay>" + AttackDelay + "</attackDelay>");
        builder.Append("<minAttackRange>" + MinAttackRange + "</minAttackRange>");
        builder.Append("<maxAttackRange>" + MaxAttackRange + "</maxAttackRange>");
        
        builder.Append("<repathDelay>" + RepathDelay + "</repathDelay>");
        
        builder.Append("<sightRange>" + SightRange + "</sightRange>");
        builder.Append("<sightAngle>" + SightAngle + "</sightAngle>");
        builder.Append("<sightHeight>" + SightHeight + "</sightHeight>");
        builder.Append("<minPresenceMultiplier>" + MinPresenceMultiplier + "</minPresenceMultiplier>");
                
        builder.Append("<isRanged>" + IsRanged + "</isRanged>");
        builder.Append("<projectileSpeed>" + ProjectileSpeed + "</projectileSpeed>");
        builder.Append("<projectileMinDistance>" + ProjectileMinDistance + "</projectileMinDistance>");
        builder.Append("<projectileType>" + ProjectileType + "</projectileType>");

        return builder.ToString();
    }

    public override void Deserialize(XElement element, bool applyBonuses = false)
    {
        base.Deserialize(element,applyBonuses);
        
        PathId = element.Element("pathId")?.Value;
        PatrollingType = Enum.TryParse(element.Element("patrollingType")?.Value, out PatrolType patrolType) ? patrolType : PatrolType.Circle;
        
        Awareness = float.TryParse(element.Element("awareness")?.Value, out float awareness) ? awareness : 0;
        AwarenessThreshold = float.TryParse(element.Element("awarenessThreshold")?.Value, out float awarenessThreshold) ? awarenessThreshold : 100.0f;
        AwarenessDecay = float.TryParse(element.Element("awarenessDecay")?.Value, out float awarenessDecay) ? awarenessDecay : 0.1f;
        
        MinPointDistance = float.TryParse(element.Element("minPointDistance")?.Value, out float minPointDistance) ? minPointDistance : 0.3f;
        PatrollingSpeed = float.TryParse(element.Element("patrollingSpeed")?.Value, out float patrollingSpeed) ? patrollingSpeed : 7.0f;
        AttackingWalkingSpeed = float.TryParse(element.Element("attackingWalkingSpeed")?.Value, out float attackingWalkingSpeed) ? attackingWalkingSpeed : 9.0f;
        
        MaxIdleTime = float.TryParse(element.Element("maxIdleTime")?.Value, out float maxIdleTime) ? maxIdleTime : 5;
        MinIdleTime = float.TryParse(element.Element("minIdleTime")?.Value, out float minIdleTime) ? minIdleTime : 2;
        
        Damage = float.TryParse(element.Element("damage")?.Value, out float damage) ? damage : 10.0f;
        AttackDelay = float.TryParse(element.Element("attackDelay")?.Value, out float attackDelay) ? attackDelay : 0.8f;
        MinAttackRange = float.TryParse(element.Element("minAttackRange")?.Value, out float minAttackRange) ? minAttackRange : 1.5f;
        MaxAttackRange = float.TryParse(element.Element("maxAttackRange")?.Value, out float maxAttackRange) ? maxAttackRange : 2.5f;
        
        RepathDelay = float.TryParse(element.Element("repathDelay")?.Value, out float repathDelay) ? repathDelay : 0.5f;
        
        SightRange = float.TryParse(element.Element("sightRange")?.Value, out float sightRange) ? sightRange : 40.0f;
        SightAngle = float.TryParse(element.Element("sightAngle")?.Value, out float sightAngle) ? sightAngle : 60.0f;
        SightHeight = float.TryParse(element.Element("sightHeight")?.Value, out float sightHeight) ? sightHeight : 4.0f;
        MinPresenceMultiplier = float.TryParse(element.Element("minPresenceMultiplier")?.Value, out float minPresenceMultiplier) ? minPresenceMultiplier : 0.2f;
        
        IsRanged = element.Element("isRanged")?.Value == "True";
        ProjectileSpeed = float.TryParse(element.Element("projectileSpeed")?.Value, out float projectileSpeed) ? projectileSpeed : 25.0f;
        ProjectileMinDistance = float.TryParse(element.Element("projectileMinDistance")?.Value, out float projectileMinDistance) ? projectileMinDistance : 1.0f;
        ProjectileType = Enum.TryParse(typeof(ProjectileManager.ProjectileType), element.Element("projectileType")?.Value,out var projectileType) ? (ProjectileManager.ProjectileType)projectileType : ProjectileManager.ProjectileType.Arrow;

    }
    
#if DEBUG
    private bool _changePatrolType = false;
    private bool changeProjectileType = false;
    public override void Inspect()
    {
        base.Inspect();
        ImGui.InputText("Path id", ref PathId, 25);
        ImGui.Text("Patrolling type: " + PatrollingType);
        if (ImGui.Button("Change patrolling type"))
        {
            _changePatrolType = true;
        }
        ImGui.Separator();
        ImGui.DragFloat("Patrolling speed", ref PatrollingSpeed);
        ImGui.DragFloat("Attack walking speed", ref AttackingWalkingSpeed);
        ImGui.Separator();
        ImGui.DragFloat("Max idle time", ref MaxIdleTime);
        ImGui.DragFloat("Min idle time", ref MinIdleTime);
        ImGui.Separator();
        ImGui.DragFloat("Min point distance", ref MinPointDistance);
        ImGui.Separator();
        ImGui.DragFloat("Damage", ref Damage);
        ImGui.DragFloat("Attack delay", ref AttackDelay);
        ImGui.DragFloat("Min attack range", ref MinAttackRange);
        ImGui.DragFloat("Max attack range", ref MaxAttackRange);
        ImGui.Separator();
        ImGui.Text("Awareness: " + Awareness);
        ImGui.DragFloat("Awareness threshold", ref AwarenessThreshold);
        ImGui.DragFloat("Awareness decay", ref AwarenessDecay);
        ImGui.Separator();
        ImGui.DragFloat("Sight range", ref SightRange);
        ImGui.DragFloat("Sight angle", ref SightAngle);
        ImGui.DragFloat("Sight height", ref SightHeight);
        ImGui.DragFloat("Min presence multiplier", ref MinPresenceMultiplier,0.01f,0,0.99f);
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
        
        if (_changePatrolType)
        {
            ImGui.Begin("Change behavior");
            var values = Enum.GetValues(typeof(PatrolType));
            foreach (PatrolType type in values)
            {
                if (ImGui.Button(type.ToString()))
                {
                    PatrollingType = type;
                    _changePatrolType = false;
                }
            }
            if (ImGui.Button("Cancel"))
            {
                _changePatrolType = false;
            }
            ImGui.End();
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