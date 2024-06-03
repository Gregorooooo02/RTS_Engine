namespace RTS_Engine.Components.AI.Agent_States;

public class CivilianStart : AgentState
{
    public override void Initialize(Agent agent)
    {
        if (agent.AgentStates.TryAdd(Agent.State.Wander, new CivilianWander()) && agent.AgentStates.TryGetValue(Agent.State.Wander, out AgentState wander))
        {
            wander.Initialize(agent);
        }
    }

    public override AgentState UpdateState(Agent agent)
    {
        if (agent.AgentStates.TryGetValue(Agent.State.Wander, out AgentState value))
        {
            return value;
        }
        return this;
    }
}