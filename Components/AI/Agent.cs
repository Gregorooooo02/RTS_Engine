using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.Agent_States;
using RTS_Engine.Components.AI.AgentData;
using RTS_Engine.Exceptions;

namespace RTS_Engine.Components.AI;

public class Agent : Component
{
    public enum AgentType
    {
        Civilian,
        Soldier,
        PlayerUnit
    }
    
    public enum State
    {
        Idle,
        Patrol,
        Attack,
        RunAway,
        Wander
    }
    
    public float WalkingSpeed = 10.0f;
    public float MaxWanderingDistance = 50.0f;
    public float WanderingDistance = 25.0f;
    public float MinPointDistance = 0.5f;

    public float MinIdleTime = 0.1f;
    public float MaxIdleTime = 5.0f;
    
    public float TurnSpeed = 2.0f;

    public AgentData.AgentData AgentData = new WandererData();
    
    public Vector2 Direction = Vector2.UnitX;
    
    private AgentState _currentState;
    public Dictionary<State, AgentState> AgentStates = new();

    public LayerType AgentLayer = LayerType.ENEMY;
    
    public Agent(){}
    
    public override void Update()
    {
        if(!Active)return;
        try
        {
            UpdateState();
        }
        catch (NoTerrainException e)
        {
            Active = false;
            Console.WriteLine("No terrain found. Disabling agent");
        }

        if (AgentLayer == LayerType.ENEMY)
        {
            //TODO: Do the line of sight here
        }
        
    }

    private void UpdateState()
    {
        _currentState = _currentState.UpdateState(this);
    }

    public void MoveToPoint(Point point, float speed)
    {
        Vector3 agentPosition = ParentObject.Transform.ModelMatrix.Translation;
        Vector3 offset = new Vector3(point.X - agentPosition.X, 0, point.Y - agentPosition.Z);
        offset.Y = PickingManager.InterpolateWorldHeight(new Vector2(agentPosition.X + offset.X, agentPosition.Z + offset.Z)) - agentPosition.Y + 2.0f;
        ParentObject.Transform.Move(offset * Globals.DeltaTime * speed);

        Vector2 destinationVector = new Vector2(offset.X, offset.Z);
        destinationVector.Normalize();
        Direction = Vector2.Lerp(Direction,destinationVector, Globals.DeltaTime * TurnSpeed);
        
        float angle = Wander.AngleDegrees(-Vector2.UnitY, Direction);
        ParentObject.Transform.SetLocalRotationY(angle);
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
            ImGui.Text("Agent type: " + AgentLayer);
            ImGui.SameLine();
            if (ImGui.Button("Switch type"))
            {
                AgentLayer = AgentLayer == LayerType.ENEMY ? LayerType.PLAYER : LayerType.ENEMY;
            }
            ImGui.Text("Current state: " + _currentState);
            AgentData.Inspect();
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }   
    }
#endif
}