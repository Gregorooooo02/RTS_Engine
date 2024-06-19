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
        
        if (agent.AgentStates.TryAdd(Agent.State.Idle, new UnitIdle()) && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
        {
            idle.Initialize(agent);
        }
        
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
        if (agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
        {
            agent.ActiveClip = 2;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 1.0f;
            return idle;
        }
        
        return this;
    }
}