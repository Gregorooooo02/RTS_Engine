using System;

namespace RTS_Engine.Components.AI.Agent_States;

public class Idle : AgentState
{
    private float IdleTimer = 0.0f;
    private bool _active = false;
    private float MaxTime;
    private Random _random = new();
    
    public AgentState Caller;
    
    public override void Initialize(Agent agent)
    {
        if (agent.AgentStates.TryAdd(Agent.State.Wander, new Wander()) && agent.AgentStates.TryGetValue(Agent.State.Wander, out AgentState value))
        {
            value.Initialize(agent);
        }
    }

    public override AgentState UpdateState(Agent agent)
    {
        if (!_active)
        {
            MaxTime = (float)_random.NextDouble() * (agent.MaxIdleTime - agent.MinIdleTime) + agent.MinIdleTime;
            _active = true;
        }
        if (_active)
        {
            IdleTimer += Globals.DeltaTime;
            if (IdleTimer >= MaxTime)
            {
                IdleTimer = 0;
                _active = false;
                return Caller;
            }
        }
        return this;
    }
}