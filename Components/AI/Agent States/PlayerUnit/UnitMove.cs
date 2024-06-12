using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class UnitMove : AgentState
{
    private Queue<Vector2> _points;
    private Vector2 _currentPoint;
    
    Node end = null;
    private bool _pathingScheduled = false;
    private bool _pathingCompleted = false;
    private bool _pathCompleted = false;
    private bool _repath = false;
    
    public override void Initialize(Agent agent)
    {
        
    }

    public override AgentState UpdateState(Agent agent)
    {
        PlayerUnitData data = (PlayerUnitData)agent.AgentData;
        if (data.Target != null && agent.AgentStates.TryGetValue(Agent.State.Attack, out AgentState attack))
        {
            _pathCompleted = false;
            return attack;
        }
        
        if (_pathCompleted && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
        {
            _pathCompleted = false;
            return value;
        }
        
        if (_pathingCompleted && _pathingScheduled)
        {
            //When pathing completes
            _pathingScheduled = false;
            _pathingCompleted = false;
            _pathCompleted = false;
            if (end == null && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
            {
                return idle;
            }
            _points = Pathfinding.PathToQueueOfVectors(end);
            _points.Enqueue(data.Destination);
            _currentPoint = _points.Dequeue();
        }
        
        Vector2 location = new Vector2(agent.Position.X, agent.Position.Z);
        if (!_pathingScheduled && (data.MovementScheduled || _repath))
        {
            data.MovementScheduled = false;
            _repath = false;
            Vector2 direction = data.Destination - location;
            direction.Normalize();
            Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(location, direction);
            Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(data.Destination, -direction);
            
            Node start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
            Node goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
            
            Task.Factory.StartNew(() =>
            { 
                end = Pathfinding.CalculatePath(goal, start, true, agent.ID);
                _pathingCompleted = true;
            });
            _pathingScheduled = true;
        }

        if (_points != null && !_pathCompleted && !_pathingScheduled)
        {
            if (Vector2.Distance(_currentPoint, location) <= data.MinPointDistance)
            {
                if(_points.Count > 0)_currentPoint = _points.Dequeue();
                else
                {
                    _pathCompleted = true;
                }
            }
            else
            {
                agent.MoveToPoint(_currentPoint, data.WalkingSpeed);
                if (Globals.Renderer.WorldRenderer.MapNodes[(int)_currentPoint.X, (int)_currentPoint.Y].AllyOccupantID !=
                    agent.ID && Globals.Renderer.WorldRenderer.MapNodes[(int)_currentPoint.X, (int)_currentPoint.Y].AllyOccupantID != 0)
                {
                    //Repath
                    _repath = true;
                }
            }
        }
        return this;
    }
}