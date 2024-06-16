using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.AgentData;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace RTS_Engine.Components.AI.Agent_States;

public class CivilianFlee : AgentState
{
    public Agent Target;
    private float _timeSinceLastRepath = 10;
    private Vector2 _destination;
    
    private Queue<Vector2> _points;
    private Vector2 _currentPoint;
    private readonly int _maxAttempts = 10;
    private int _currentAttempts = 0;
    
    Node _end = null;
    private bool _pathingScheduled = false;
    private bool _pathingCompleted = false;
    private bool _pathCompleted = false;
    private bool _repath = false;
    private bool _searchNearby = false;

    private float _angle = 0;
    
    public override void Initialize(Agent agent)
    {
        
    }

    public override AgentState UpdateState(Agent agent)
    {
        WandererData data = (WandererData)agent.AgentData;
        Vector2 target = new Vector2(Target.Position.X, Target.Position.Z);
        Vector2 location = new Vector2(agent.Position.X, agent.Position.Z);
        float distance = Vector2.Distance(target,location);
        if ((distance > data.FledDistance || Target == null) && agent.AgentStates.TryGetValue(Agent.State.Wander,out AgentState wander1))
        {
            Target = null;
            data.Alarmed = false;
            return wander1;
        }

        /*
        if (Vector2.Distance(_destination, location) <= data.FleeingSpeed || _timeSinceLastRepath >= data.RepathDelay)
        {
            Node end = null;
            int attempts = 0;
            Vector2 direction = location - target;
            direction.Normalize();
            Vector2 currentOffset = direction * data.FleeingDistance;
            Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(location, direction);
            float angle = 0;
            do
            {
                if (attempts > _maxAttempts && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle) && agent.AgentStates.TryGetValue(Agent.State.Wander, out AgentState wander))
                {
                    //If flee attempts fails return to idle
                    ((CivilianIdle)idle).Caller = wander;
                    Target = null;
                    data.Alarmed = false;
                    return idle;
                }
                if (angle != 0)
                {
                    currentOffset.X = MathF.Cos(angle) * direction.X - MathF.Sin(angle) * direction.Y;
                    currentOffset.Y = MathF.Sin(angle) * direction.X + MathF.Cos(angle) * direction.Y;
                    currentOffset *= data.FleeingDistance;
                }
                Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(location + currentOffset, -direction);
                attempts++;
                
                _destination = location + currentOffset;
                
                if (((int)endPoint.X < 1 || (int)endPoint.Y < 1 ||
                     (int)endPoint.X > Globals.Renderer.WorldRenderer.MapNodes.GetLength(0) - 2 ||
                     (int)endPoint.Y > Globals.Renderer.WorldRenderer.MapNodes.GetLength(1) - 2) || !Globals.Renderer.WorldRenderer.MapNodes[(int)endPoint.X,(int)endPoint.Y].Available)
                {
                    angle *= -1;
                    if (angle >= 0)
                    {
                        angle += 0.2617993878f;
                    }
                    continue;
                }
                
                Node start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
                Node goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
            
                end = Pathfinding.CalculatePath(goal, start,false);
            } while (end is null);
            _points = Pathfinding.PathToQueueOfVectors(end);
            _points.Enqueue(_destination);
            _currentPoint = _points.Dequeue();
            _timeSinceLastRepath = 0;
        }
        else
        {
            _timeSinceLastRepath += Globals.DeltaTime;
            if (Vector2.Distance(_currentPoint, location) <= data.MinPointDistance)
            {
                _currentPoint = _points.Dequeue();
            }
            else
            {
                agent.MoveToPoint(_currentPoint, data.FleeingSpeed);
            }
        }
        */
        
        if (_pathingCompleted && _pathingScheduled)
        {
            //When pathing completes
            
            _pathingScheduled = false;
            _pathingCompleted = false;
            if (_end == null && _currentAttempts < _maxAttempts)
            {
                //TODO: Schedule repathing here
                _repath = true;
                _currentAttempts++;
                _angle *= -1;
                if (_angle >= 0)
                {
                    _angle += 0.2617993878f;
                }
            } 
            else if (_end == null && _currentAttempts >= _maxAttempts && agent.AgentStates.TryGetValue(Agent.State.Wander, out AgentState wander))
            {
                Target = null;
                data.Alarmed = false;
                return wander;
            }
            else if(_end != null)
            {
                _pathCompleted = false;
                _currentAttempts = 0;
                _angle = 0;
                _points = Pathfinding.PathToQueueOfVectors(_end);
                _points.Enqueue(_destination);
                _currentPoint = _points.Dequeue();
                _timeSinceLastRepath = 0;
            }
        }
        
        if ((Vector2.Distance(_destination, location) <= data.FleeingSpeed || _timeSinceLastRepath >= data.RepathDelay || _points.Count == 0 || _repath) && !_pathingScheduled)
        {
            Node goal = null;
            Node start;
            
            _repath = false;
            Vector2 direction = location - target;
            direction.Normalize();
            Vector2 currentOffset;
            
            currentOffset.X = MathF.Cos(_angle) * direction.X - MathF.Sin(_angle) * direction.Y;
            currentOffset.Y = MathF.Sin(_angle) * direction.X + MathF.Cos(_angle) * direction.Y;
            currentOffset *= data.FleeingDistance;
            
            Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(location, direction);
            Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(location + currentOffset, -direction);
            
            if (!Globals.Renderer.WorldRenderer.MapNodes[(int)endPoint.X, (int)endPoint.Y].Available)
                _searchNearby = true;
            
            if (_searchNearby)
            {
                _searchNearby = false;
                Point? newPoint = Pathfinding.GetFirstNearbyFreePoint(target, agent.ID);
                if (newPoint.HasValue)
                {
                    goal = new Node(newPoint.Value, null, 1);
                }
            }
            goal ??= new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
            start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
            
            _destination = location + currentOffset;
            
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            { 
                _end = Pathfinding.CalculatePath(goal, start, true, agent.ID);
                _pathingCompleted = true;
            });
            _pathingScheduled = true;
        }

        if (_points != null && !_pathCompleted && !_pathingScheduled)
        {
            _timeSinceLastRepath += Globals.DeltaTime;
            if (Vector2.Distance(_currentPoint, location) <= data.MinPointDistance)
            {
                _currentPoint = _points.Dequeue();
            }
            else
            {
                agent.MoveToPoint(_currentPoint, data.FleeingSpeed);
            }
        }
        return this;
    }
}