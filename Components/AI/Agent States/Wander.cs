﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace RTS_Engine.Components.AI.Agent_States;

public class Wander : AgentState
{
    private static readonly Random Random = new();
    private Queue<Point> _points;
    private Point _currentPoint;
    private bool _traversing = false;
    
    public Wander()
    {
        State = Agent.State.Wander;
    }
    
    public override void Initialize(Agent agent)
    {
        //Add to agent any AgentState that this state needs.In example below the state needs Wander state so it looks for it and if it's not present add it.
        /*
        if (agent.AgentStates.Find((x) => x.State == Agent.EnemyState.Wander) == null)
        {
            AgentState temp = new Wander();
            agent.AgentStates.Add(temp);
            temp.Initialize(agent);
        }
        */
        
        if (agent.AgentStates.TryAdd(Agent.State.Idle, new Idle()) && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
        {
            value.Initialize(agent);
        }
    }


    public static Vector2 RandomUnitVector2()
    {
        float angle = Random.Next(360);
        angle *= (MathF.PI / 180.0f);
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    public static float Distance(Point point, Vector3 position)
    {
        return new Vector2(point.X - position.X, point.Y - position.Z).Length();
    }
    public override AgentState UpdateState(Agent agent)
    {
        Vector3 agentPosition = agent.ParentObject.Transform.ModelMatrix.Translation;
        if (_points == null || _points.Count == 0)
        {
            if (_traversing && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
            {
                _traversing = false;
                ((Idle)value).Caller = this;
                return value;
            }
            Node end = null;
            do
            {
                Vector2 offset = RandomUnitVector2() * agent.WanderingDistance;
                
                if((int)agentPosition.X + offset.X < 0 || (int)(agentPosition.Z + offset.Y) < 0) continue;
                
                Node start = new Node(new Point((int)agentPosition.X, (int)agentPosition.Z), null, 1);
                Node goal = new Node(new Point((int)(agentPosition.X + offset.X), (int)(agentPosition.Z + offset.Y)), null, 1);
            
                end = Pathfinding.CalculatePath(goal, start);
            } while (end is null || end.CurrentCost > agent.MaxWanderingDistance);
            _points = Pathfinding.PathToQueueOfPoints(end);
            _traversing = true;
            _currentPoint = _points.Dequeue();
        }
        else
        {
            if (Distance(_currentPoint, agentPosition) <= agent.MinPointDistance)
            {
                _currentPoint = _points.Dequeue();
            }
            else
            {
                Vector3 offset = new Vector3(_currentPoint.X - agentPosition.X, 0, _currentPoint.Y - agentPosition.Z);
                offset.Y =
                    PickingManager.InterpolateWorldHeight(new Vector2(agentPosition.X + offset.X,
                        agentPosition.Z + offset.Z)) - agentPosition.Y;
                agent.ParentObject.Transform.Move(offset * Globals.DeltaTime * agent.WalkingSpeed);
            }
        }

        return this;
    }
}