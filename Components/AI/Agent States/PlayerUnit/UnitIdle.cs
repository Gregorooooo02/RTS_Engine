using System;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class UnitIdle : AgentState
{
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
        if (data.MovementScheduled && agent.AgentStates.TryGetValue(Agent.State.Move,out AgentState move))
        {
            agent.ActiveClip = 4;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            return move;
        }
        if (data.Target != null && agent.AgentStates.TryGetValue(Agent.State.Attack, out AgentState attack))
        {
            agent.ActiveClip = 0;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            return attack;
        }
        return this;
    }
}