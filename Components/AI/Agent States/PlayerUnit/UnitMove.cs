using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class UnitMove : AgentState
{
    private Queue<Vector2> _points;
    private Vector2 _currentPoint;
    private readonly int _maxAttempts = 10;
    public override void Initialize(Agent agent)
    {
        
    }

    public override AgentState UpdateState(Agent agent)
    {
        PlayerUnitData data = (PlayerUnitData)agent.AgentData;
        if (data.Target != null && agent.AgentStates.TryGetValue(Agent.State.Attack, out AgentState attack))
        {
            return attack;
        }

        if (_points is { Count: 0 } && !data.MovementScheduled && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
        {
            return value;
        }
        Vector2 location = new Vector2(agent.Position.X, agent.Position.Z);
        if (data.MovementScheduled)
        {
            data.MovementScheduled = false;
            Vector2 direction = data.Destination - location;
            direction.Normalize();
            Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(location, direction);
            Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(data.Destination, -direction);
            Node end = null;
            int attempts = 0;
            do
            {
                attempts++;
                if (attempts > _maxAttempts && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
                {
                    //If pathing attempts fails, for some reason, return to idle
                    return idle;
                }
                
                
                Node start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
                Node goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
                
                end = Pathfinding.CalculatePath(goal, start);
                
            } while (end is null);
            _points = Pathfinding.PathToQueueOfVectors(end);
            _points.Enqueue(data.Destination);
            _currentPoint = _points.Dequeue();
        }
        else
        {
            if (Vector2.Distance(_currentPoint, location) <= data.MinPointDistance)
            {
                _currentPoint = _points.Dequeue();
            }
            else
            {
                agent.MoveToPoint(_currentPoint, data.WalkingSpeed);
            }
        }
        return this;
    }
}