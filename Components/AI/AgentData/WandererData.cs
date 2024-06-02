using ImGuiNET;

namespace RTS_Engine.Components.AI.AgentData;

public class WandererData : AgentData
{
    public float WanderingSpeed = 5; 
    public float FleeingSpeed = 7;

    public float MaxIdleTime = 5;
    public float MinIdleTime = 2;
    
    public float MaxWanderingDistance = 50.0f;
    public float WanderingDistance = 25.0f;
    public float MinPointDistance = 0.5f;
    
    public WandererData(){}
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

        ImGui.DragFloat("Max idle time", ref MaxIdleTime);
        ImGui.DragFloat("Min idle time", ref MinIdleTime);
        
        ImGui.DragFloat("Max wandering distance", ref MaxWanderingDistance);
        ImGui.DragFloat("Wandering distance", ref WanderingDistance);
        ImGui.DragFloat("Min point distance", ref MinPointDistance);
    }
#endif
}