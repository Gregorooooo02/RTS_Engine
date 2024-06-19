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
            agent.ActiveCivilianClip = 4;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            return value;
        }
        return this;
    }
}