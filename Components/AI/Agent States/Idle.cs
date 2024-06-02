using System;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class Idle : AgentState
{
    private float _idleTimer = 0.0f;
    private bool _active = false;
    private float _maxTime;
    private readonly Random _random = new();
    
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
        WandererData data = (WandererData)agent.AgentData;
        if (!_active)
        {
            _maxTime = (float)_random.NextDouble() * (data.MaxIdleTime - data.MinIdleTime) + data.MinIdleTime;
            _active = true;
        }
        if (_active)
        {
            _idleTimer += Globals.DeltaTime;
            if (_idleTimer >= _maxTime)
            {
                _idleTimer = 0;
                _active = false;
                return Caller;
            }
        }
        return this;
    }
}