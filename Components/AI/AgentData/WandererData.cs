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
    public float RepathDelay = 2.0f;

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