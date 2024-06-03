using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class CivilianWander : AgentState
{
    private static readonly Random Random = new();
    private Queue<Vector2> _points;
    private Vector2 _currentPoint;
    private bool _traversing = false;
    private int _maxAttempts = 10;
    
    public CivilianWander()
    {
        State = Agent.State.Wander;
    }
    
    public override void Initialize(Agent agent)
    {
        //Add to agent any AgentState that this state needs.In example below the state needs Idle state so it looks for it and if it's not present adds it.
        /*
        if (agent.AgentStates.TryAdd(Agent.State.Idle, new Idle()) && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
        {
            value.Initialize(agent);
        }
        */
        
        if (agent.AgentStates.TryAdd(Agent.State.Idle, new CivilianIdle()) && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
        {
            value.Initialize(agent);
        }
        
        if (agent.AgentStates.TryAdd(Agent.State.RunAway, new CivilianFlee()) && agent.AgentStates.TryGetValue(Agent.State.RunAway, out AgentState flee))
        {
            flee.Initialize(agent);
        }
    }


    public static Vector2 RandomUnitVector2()
    {
        float angle = Random.Next(360);
        angle *= (MathF.PI / 180.0f);
        return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
    }

    public static float Distance(Vector2 point, Vector3 position)
    {
        return new Vector2(point.X - position.X, point.Y - position.Z).Length();
    }

    public static float AngleDegrees(Vector2 first, Vector2 second)
    {
        return MathF.Atan2(first.X * second.Y - first.Y * second.X, first.X * second.X + first.Y * second.Y) * (180.0f / MathF.PI);
    }
    
    public override AgentState UpdateState(Agent agent)
    {
        Vector3 agentPosition = agent.ParentObject.Transform.ModelMatrix.Translation;
        WandererData data = (WandererData)agent.AgentData;
        if (data.Awareness > data.AwarenessThreshold && agent.AgentStates.TryGetValue(Agent.State.RunAway,out AgentState flee))
        {
            _traversing = false;
            ((CivilianFlee)flee).Target = data.Target;
            data.Alarmed = true;
            data.Awareness = 0;
            _points.Clear();
            return flee;
        }
        
        if (_points == null || _points.Count == 0)
        {
            if (_traversing && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
            {
                _traversing = false;
                ((CivilianIdle)value).Caller = this;
                return value;
            }
            Node end = null;
            Vector2 offset;
            int attempts = 0;
            do
            {
                offset = RandomUnitVector2() * data.WanderingDistance;
                attempts++;
                
                if (attempts > _maxAttempts && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
                {
                    _traversing = false;
                    ((CivilianIdle)idle).Caller = this;
                    return idle;
                }
                
                if((int)agentPosition.X + offset.X < 0 || (int)(agentPosition.Z + offset.Y) < 0) continue;
                
                Node start = new Node(new Point((int)agentPosition.X, (int)agentPosition.Z), null, 1);
                Node goal = new Node(new Point((int)(agentPosition.X + offset.X), (int)(agentPosition.Z + offset.Y)), null, 1);
            
                end = Pathfinding.CalculatePath(goal, start);
                
            } while (end is null || end.CurrentCost > data.MaxWanderingDistance);
            _points = Pathfinding.PathToQueueOfVectors(end);
            _points.Enqueue(new Vector2(agentPosition.X + offset.X,agentPosition.Z + offset.Y));
            _traversing = true;
            _currentPoint = _points.Dequeue();
        }
        else
        {
            if (Distance(_currentPoint, agentPosition) <= data.MinPointDistance)
            {
                _currentPoint = _points.Dequeue();
            }
            else
            {
                agent.MoveToPoint(_currentPoint, data.WanderingSpeed);
            }
        }

        return this;
    }
    
}