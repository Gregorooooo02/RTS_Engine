using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using RTS_Engine.Components.AI.Agent_States;

namespace RTS_Engine.Components.AI.AgentData;

public class WandererData : AgentData
{
    public float Awareness = 0;
    public float AwarenessThreshold = 100.0f;
    public float AwarenessDecay = 0.1f;

    public float SightRange = 40.0f;
    public float SightAngle = 60.0f;
    public float SightHeight = 4.0f;
    public float MinPresenceMultiplier = 0.2f;
    
    public float WanderingSpeed = 5; 
    public float FleeingSpeed = 7;

    public float MaxIdleTime = 5;
    public float MinIdleTime = 2;
    
    public float MaxWanderingDistance = 50.0f;
    public float WanderingDistance = 25.0f;
    public float MinPointDistance = 0.5f;
    
    public float FledDistance = 50.0f;
    public float FleeingDistance = 40.0f;
    public float RepathDelay = 0.5f;

    public bool Alarmed = false;

    public Agent Target = null;
    public readonly AgentState EntryState = new CivilianStart();
    
    public WandererData() : base(100){}
    public WandererData(float maxHp, float wanderingSpeed, float fleeingSpeed, float maxIdleTime, float minIdleTime, float maxWanderingDistance, float wanderingDistance, float minPointDistance) : base(maxHp)
    {
        WanderingSpeed = wanderingSpeed;
        FleeingSpeed = fleeingSpeed;
        MaxIdleTime = maxIdleTime;
        MinIdleTime = minIdleTime;

        MaxWanderingDistance = maxWanderingDistance;
        WanderingDistance = wanderingDistance;
        MinPointDistance = minPointDistance;
    }
    
    public override string Serialize()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append(base.Serialize());
        
        builder.Append("<awareness>" + Awareness + "</awareness>");
        builder.Append("<awarenessThreshold>" + AwarenessThreshold + "</awarenessThreshold>");
        builder.Append("<awarenessDecay>" + AwarenessDecay + "</awarenessDecay>");
        
        builder.Append("<sightRange>" + SightRange + "</sightRange>");
        builder.Append("<sightAngle>" + SightAngle + "</sightAngle>");
        builder.Append("<sightHeight>" + SightHeight + "</sightHeight>");
        builder.Append("<minPresenceMultiplier>" + MinPresenceMultiplier + "</minPresenceMultiplier>");
        
        builder.Append("<wanderingSpeed>" + WanderingSpeed + "</wanderingSpeed>");
        builder.Append("<fleeingSpeed>" + FleeingSpeed + "</fleeingSpeed>");
        
        builder.Append("<maxIdleTime>" + MaxIdleTime + "</maxIdleTime>");
        builder.Append("<minIdleTime>" + MinIdleTime + "</minIdleTime>");
        
        builder.Append("<maxWanderingDistance>" + MaxWanderingDistance + "</maxWanderingDistance>");
        builder.Append("<wanderingDistance>" + WanderingDistance + "</wanderingDistance>");
        builder.Append("<minPointDistance>" + MinPointDistance + "</minPointDistance>");
        
        builder.Append("<fledDistance>" + FledDistance + "</fledDistance>");
        builder.Append("<fleeingDistance>" + FleeingDistance + "</fleeingDistance>");
        builder.Append("<repathDelay>" + RepathDelay + "</repathDelay>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        base.Deserialize(element);

        Awareness = float.TryParse(element.Element("awareness")?.Value, out float awareness) ? awareness : 0;
        AwarenessThreshold = float.TryParse(element.Element("awarenessThreshold")?.Value, out float awarenessThreshold) ? awarenessThreshold : 100.0f;
        AwarenessDecay = float.TryParse(element.Element("awarenessDecay")?.Value, out float awarenessDecay) ? awarenessDecay : 0.1f;

        SightRange = float.TryParse(element.Element("sightRange")?.Value, out float sightRange) ? sightRange : 40.0f;
        SightAngle = float.TryParse(element.Element("sightAngle")?.Value, out float sightAngle) ? sightAngle : 60.0f;
        SightHeight = float.TryParse(element.Element("sightHeight")?.Value, out float sightHeight) ? sightHeight : 4.0f;
        MinPresenceMultiplier = float.TryParse(element.Element("minPresenceMultiplier")?.Value, out float minPresenceMultiplier) ? minPresenceMultiplier : 0.2f;

        WanderingSpeed = float.TryParse(element.Element("wanderingSpeed")?.Value, out float wanderingSpeed) ? wanderingSpeed : 5;
        FleeingSpeed = float.TryParse(element.Element("fleeingSpeed")?.Value, out float fleeingSpeed) ? fleeingSpeed : 7;

        MaxIdleTime = float.TryParse(element.Element("maxIdleTime")?.Value, out float maxIdleTime) ? maxIdleTime : 5;
        MinIdleTime = float.TryParse(element.Element("minIdleTime")?.Value, out float minIdleTime) ? minIdleTime : 2;

        MaxWanderingDistance = float.TryParse(element.Element("maxWanderingDistance")?.Value, out float maxWanderingDistance) ? maxWanderingDistance : 50.0f;
        WanderingDistance = float.TryParse(element.Element("wanderingDistance")?.Value, out float wanderingDistance) ? wanderingDistance : 25.0f;
        MinPointDistance = float.TryParse(element.Element("minPointDistance")?.Value, out float minPointDistance) ? minPointDistance : 0.5f;

        FledDistance = float.TryParse(element.Element("fledDistance")?.Value, out float fledDistance) ? fledDistance : 50.0f;
        FleeingDistance = float.TryParse(element.Element("fleeingDistance")?.Value, out float fleeingDistance) ? fleeingDistance : 40.0f;
        RepathDelay = float.TryParse(element.Element("repathDelay")?.Value, out float repathDelay) ? repathDelay : 0.5f;
    }

#if DEBUG
    public override void Inspect()
    {
        base.Inspect();
        ImGui.DragFloat("Wandering speed", ref WanderingSpeed);
        ImGui.DragFloat("Fleeing speed", ref FleeingSpeed);
        ImGui.Separator();
        ImGui.DragFloat("Max idle time", ref MaxIdleTime);
        ImGui.DragFloat("Min idle time", ref MinIdleTime);
        ImGui.Separator();
        ImGui.DragFloat("Max wandering distance", ref MaxWanderingDistance);
        ImGui.DragFloat("Wandering distance", ref WanderingDistance);
        ImGui.DragFloat("Min point distance", ref MinPointDistance);
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
        ImGui.DragFloat("Min fled distance", ref FledDistance);
        ImGui.DragFloat("Fleeing distance", ref FleeingDistance);
        ImGui.DragFloat("Repath delay", ref RepathDelay);
    }
#endif
}