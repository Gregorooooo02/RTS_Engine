using System;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class SoldierIdle : AgentState
{
    private float _idleTimer = 0.0f;
    private bool _active = false;
    private float _maxTime;
    private readonly Random _random = new();
    
    public override void Initialize(Agent agent)
    {
        
    }

    public override AgentState UpdateState(Agent agent)
    {
        SoldierData data = (SoldierData)agent.AgentData;
        if (data.Awareness >= data.AwarenessThreshold && agent.AgentStates.TryGetValue(Agent.State.Attack, out AgentState attack))
        {
            ((SoldierAttack)attack).Target = data.Target;
            data.Awareness = 0;
            data.Alarmed = true;
            return attack;
        }
        if (!_active)
        {
            _maxTime = (float)_random.NextDouble() * (data.MaxIdleTime - data.MinIdleTime) + data.MinIdleTime;
            _active = true;
        }
        if (_active)
        {
            _idleTimer += Globals.DeltaTime;
            if (_idleTimer >= _maxTime && agent.AgentStates.TryGetValue(Agent.State.Patrol,out AgentState patrol))
            {
                _idleTimer = 0;
                _active = false;
                return patrol;
            }
        }
        return this;
    }
}