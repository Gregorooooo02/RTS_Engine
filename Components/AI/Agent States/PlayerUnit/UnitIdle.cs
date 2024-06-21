using System;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class UnitIdle : AgentState
{
    private bool _changeIdle = false;
    private bool _changeHidden = false;

    private float currentHideTime = 0;
    
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
        if ((agent.ActiveClip != 2 || agent.AnimatedRenderer._skinnedModel.AnimationController.Speed > 1.0f) && currentHideTime < data.HideTime)
        {
            agent.ActiveClip = 2;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 1.0f;
            _changeIdle = true;
        }
        
        if (_changeHidden)
        {
            _changeHidden = false;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            agent.IsHidden = true;
        }
        if ((agent.ActiveClip != 3 || agent.AnimatedRenderer._skinnedModel.AnimationController.Speed > 1.0f) && currentHideTime >= data.HideTime)
        {
            agent.ActiveClip = 3;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 1.0f;
            agent.AnimatedRenderer._skinnedModel.AnimationController.LoopEnabled = false;
            _changeHidden = true;
        }

        currentHideTime += Globals.DeltaTime;
        if (data.MovementScheduled && agent.AgentStates.TryGetValue(Agent.State.Move,out AgentState move))
        {
            currentHideTime = 0;
            agent.AnimatedRenderer._skinnedModel.AnimationController.LoopEnabled = true;
            agent.IsHidden = false;
            return move;
        }
        if (data.Target != null && agent.AgentStates.TryGetValue(Agent.State.Attack, out AgentState attack))
        {
            currentHideTime = 0;
            agent.AnimatedRenderer._skinnedModel.AnimationController.LoopEnabled = true;
            agent.IsHidden = false;
            return attack;
        }
        return this;
    }
}