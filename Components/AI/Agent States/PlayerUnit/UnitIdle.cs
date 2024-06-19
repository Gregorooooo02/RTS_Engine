using System;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class UnitIdle : AgentState
{
    private bool _changeIdle = false;
    
    public override void Initialize(Agent agent)
    {
        if (agent.AgentStates.TryAdd(Agent.State.Move, new UnitMove()) && agent.AgentStates.TryGetValue(Agent.State.Move, out AgentState move))
        {
            move.Initialize(agent);
        }
        if (agent.AgentStates.TryAdd(Agent.State.Attack, new UnitAttack()) && agent.AgentStates.TryGetValue(Agent.State.Attack, out AgentState attack))
        {
            attack.Initialize(agent);
        }
    }

    public override AgentState UpdateState(Agent agent)
    {
        PlayerUnitData data = (PlayerUnitData)agent.AgentData;
        if (_changeIdle)
        {
            _changeIdle = false;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
        }
        if (agent.ActiveClip != 2 || agent.AnimatedRenderer._skinnedModel.AnimationController.Speed > 1.0f)
        {
            agent.ActiveClip = 2;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 1.0f;
            _changeIdle = true;
        }
        if (data.MovementScheduled && agent.AgentStates.TryGetValue(Agent.State.Move,out AgentState move))
        {
            //agent.ActiveClip = 4;
            //agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            //agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 2.0f;
            return move;
        }
        if (data.Target != null && agent.AgentStates.TryGetValue(Agent.State.Attack, out AgentState attack))
        {
            //agent.ActiveClip = 0;
            //agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            return attack;
        }
        return this;
    }
}