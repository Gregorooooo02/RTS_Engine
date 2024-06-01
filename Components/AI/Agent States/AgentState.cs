namespace RTS_Engine.Components.AI.Agent_States;

public abstract class AgentState
{
    public Agent.State State;
    public abstract void Initialize(Agent agent);
    public abstract AgentState UpdateState(Agent agent);
}