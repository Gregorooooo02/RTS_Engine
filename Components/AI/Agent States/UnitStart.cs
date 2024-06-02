namespace RTS_Engine.Components.AI.Agent_States;

public class UnitStart : AgentState
{
    public override void Initialize(Agent agent)
    {
        //Add states used by units here. Example below:
        /*
        if (agent.AgentStates.TryAdd(Agent.State.Wander, new Wander()) && agent.AgentStates.TryGetValue(Agent.State.Wander, out AgentState wander))
        {
            wander.Initialize(agent);
        }
        */
        
    }

    public override AgentState UpdateState(Agent agent)
    {
        //Enter first state here. Example below
        /*
        if (agent.AgentStates.TryGetValue(Agent.State.Wander, out AgentState value))
        {
            return value;
        }
        */
        
        return this;
    }
}