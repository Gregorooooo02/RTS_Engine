namespace RTS_Engine.Components.AI.Agent_States;

public class SoldierStart : AgentState
{
    public override void Initialize(Agent agent)
    {
        if (agent.AgentStates.TryAdd(Agent.State.Patrol, new SoldierPatrol()) && agent.AgentStates.TryGetValue(Agent.State.Patrol, out AgentState patrol))
        {
            patrol.Initialize(agent);
        }
    }

    public override AgentState UpdateState(Agent agent)
    {
        if (agent.AgentStates.TryGetValue(Agent.State.Patrol, out AgentState patrol))
        {
            return patrol;
        }
        return this;
    }
}