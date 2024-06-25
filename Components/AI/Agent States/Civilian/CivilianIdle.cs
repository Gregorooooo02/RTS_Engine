using System;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class CivilianIdle : AgentState
{
    private float _idleTimer = 0.0f;
    private bool _active = false;
    private float _maxTime;
    private readonly Random _random = new();

    private bool _changeIdle = false;
    
    public AgentState Caller;
    
    public override void Initialize(Agent agent)
    {
        if (agent.AgentStates.TryAdd(Agent.State.Wander, new CivilianWander()) && agent.AgentStates.TryGetValue(Agent.State.Wander, out AgentState value))
        {
            value.Initialize(agent);
        }
    }

    public override AgentState UpdateState(Agent agent)
    {
        if (_changeIdle)
        {
            _changeIdle = false;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
        }
        if (agent.ActiveCivilianClip != 2 || agent.AnimatedRenderer._skinnedModel.AnimationController.Speed > 1.0f)
        {
            agent.ActiveCivilianClip = 2;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 1.0f;
            _changeIdle = true;
        }
        
        WandererData data = (WandererData)agent.AgentData;
        if (data.Awareness > data.AwarenessThreshold && agent.AgentStates.TryGetValue(Agent.State.RunAway,out AgentState flee))
        {
            ((CivilianFlee)flee).Target = data.Target;
            data.Alarmed = true;
            data.Awareness = 0;
            _active = false;
            _idleTimer = 0;
            return flee;
        }
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