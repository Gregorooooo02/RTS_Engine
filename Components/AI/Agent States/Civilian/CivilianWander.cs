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
        Vector2 temp = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        temp.Normalize();
        return temp;
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
        Vector2 agentPosition = new Vector2(agent.Position.X, agent.Position.Z);
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
                if (attempts > _maxAttempts && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
                {
                    _traversing = false;
                    ((CivilianIdle)idle).Caller = this;
                    return idle;
                }
                offset = RandomUnitVector2();
                Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(agentPosition, offset);
                Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(agentPosition + (offset * data.WanderingDistance), -offset);
                offset *= data.WanderingDistance;
                attempts++;

                if ((int)endPoint.X < 0
                    || (int)(endPoint.Y) < 0
                    || (int)(endPoint.X) > Globals.Renderer.WorldRenderer.MapNodes.GetLength(0) - 1
                    || (int)(endPoint.Y) > Globals.Renderer.WorldRenderer.MapNodes.GetLength(1) - 1
                    || Globals.Renderer.WorldRenderer.MapNodes[(int)endPoint.X,(int)endPoint.Y] == null)
                {
                    continue;
                }
                
                Node start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
                Node goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
            
                end = Pathfinding.CalculatePath(goal, start);
                
            } while (end is null || end.CurrentCost > data.MaxWanderingDistance);
            _points = Pathfinding.PathToQueueOfVectors(end);
            _points.Enqueue(new Vector2(agentPosition.X + offset.X,agentPosition.Y + offset.Y));
            _traversing = true;
            _currentPoint = _points.Dequeue();
        }
        else
        {
            if (Vector2.Distance(_currentPoint,agentPosition) <= data.MinPointDistance)
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