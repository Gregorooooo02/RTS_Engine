﻿using System.Collections.Generic;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.Agent_States;

namespace RTS_Engine.Components.AI;

public class Agent : Component
{
    public enum State
    {
        Idle,
        Patrol,
        Attack,
        RunAway,
        Wander
    }
    
    #region Agent Parameters

    public float WalkingSpeed = 10.0f;
    public float MaxWanderingDistance = 50.0f;
    public float WanderingDistance = 25.0f;
    public float MinPointDistance = 0.5f;

    public float MinIdleTime = 0.1f;
    public float MaxIdleTime = 5.0f;

    #endregion

    public Vector2 TargetPosition;
    
    public Dictionary<State, AgentState> AgentStates = new();
    
    private AgentState _currentState;
    
    public Agent(){}
    
    public override void Update()
    {
        if(!Active)return;
        UpdateState();
    }

    private void UpdateState()
    {
        _currentState = _currentState.UpdateState(this);
    }
    
    public override void Initialize()
    {
        _currentState = new Wander();
        AgentStates.Add(State.Wander,_currentState);
        _currentState.Initialize(this);
    }

    public override string ComponentToXmlString()
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(XElement element)
    {
        throw new System.NotImplementedException();
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Agent"))
        {
            ImGui.Checkbox("Agent active", ref Active);
            ImGui.Text("Current state: " + _currentState);
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }   
    }
#endif
}