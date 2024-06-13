using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class UnitAttack : AgentState
{
    private float _attackTimer;
    private float _timeSinceLastRepath = 10;
    
    Node end = null;
    
    private bool _pathingScheduled = false;
    private bool _pathingCompleted = false;
    private bool _pathCompleted = false;
    private bool _repath = false;
    private bool _SearchNearby = false;
    
    private Queue<Vector2> _points;
    private Vector2 _currentPoint;
    private readonly int _maxAttempts = 10;
    
    public override void Initialize(Agent agent)
    {
        
    }

    public override AgentState UpdateState(Agent agent)
    {
        PlayerUnitData data = (PlayerUnitData)agent.AgentData;
        /*
        if (data.MovementScheduled && agent.AgentStates.TryGetValue(Agent.State.Move,out AgentState move))
        {
            data.Target = null;
            return move;
        }
        if ((data.Target == null || !data.Target.AgentData.Alive) && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
        {
            //TODO: Implement retargeting here
            data.Target = null;
            return idle;
        }
        Vector2 location = new Vector2(agent.Position.X, agent.Position.Z);
        Vector2 target = new Vector2(data.Target.Position.X, data.Target.Position.Z);
        float dist = Vector2.Distance(location, target);
        Vector2 direction = target - location;
        direction.Normalize();
        if ((_timeSinceLastRepath >= data.RepathDelay || _points.Count == 0) && (dist > data.MaxAttackRange || dist < data.MinAttackRange))
        {
            Node end = null;
            int attempts = 0;
            
            Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(location, direction);
            Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(target, -direction);
            do
            {
                attempts++;
                if (attempts > _maxAttempts && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
                {
                    //If pathing to target attempts fails for some reason return to idle
                    data.Target = null;
                    return value;
                }

                Node start;
                Node goal;
                if (dist > data.MaxAttackRange)
                {
                    //If it's to far, walk to target
                    start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
                    goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
                    
                    end = Pathfinding.CalculatePath(goal, start,true,agent.ID);
                }
                else if (dist < data.MinAttackRange)
                {
                    //If it's to close, walk away from target
                    float awayDist = ((data.MaxAttackRange + data.MinAttackRange) / 2.0f) - dist;
                    Vector2 offset = new Vector2(location.X - target.X, location.Y - target.Y);
                    offset.Normalize();
                    offset *= awayDist;
                    
                    start = new Node(new Point((int)location.X, (int)location.Y), null, 1);
                    goal = new Node(new Point((int)(location.X + offset.X), (int)(location.Y + offset.Y)), null, 1);
                    
                    end = Pathfinding.CalculatePath(goal, start);
                }
                
            } while (end is null);
            _points = Pathfinding.PathToQueueOfVectors(end);
            _points.Enqueue(target);
            _currentPoint = _points.Dequeue();
            _timeSinceLastRepath = 0;
        }
        else
        {
            _timeSinceLastRepath += Globals.DeltaTime;
            _attackTimer += Globals.DeltaTime;
            if (dist > data.MaxAttackRange || dist < data.MinAttackRange)
            {
                //_attackTimer = 0;
                if (Vector2.Distance(location,_currentPoint) <= data.MinPointDistance)
                {
                    _currentPoint = _points.Dequeue();
                }
                else
                {
                    agent.MoveToPoint(_currentPoint, data.WalkingSpeed);
                }
            }
            else
            {
                float angle = CivilianWander.AngleDegrees(agent.Direction, direction);
                if (MathF.Abs(angle) < 5.0f)
                {
                    if (_attackTimer >= data.AttackDelay)
                    {
                        //Successful attack
                        _attackTimer = 0;
                        if (data.Target.Type == Agent.AgentType.Soldier)
                        {
                            SoldierData soldierData = (SoldierData)data.Target.AgentData;
                            if (!soldierData.Alarmed)
                            {
                                soldierData.Awareness = soldierData.AwarenessThreshold * 2;
                                soldierData.Target = agent;
                            }
                        }
                        else
                        {
                            WandererData wandererData = (WandererData)data.Target.AgentData;
                            if (!wandererData.Alarmed)
                            {
                                wandererData.Awareness = wandererData.AwarenessThreshold * 2;
                                wandererData.Target = agent;
                            }
                        }
                        data.Target.AgentData.DealDamage(data.Damage);
                    }
                }
                else
                {
                    agent.UpdateRotation(direction);
                }
            }
        }
        */
        if (data.MovementScheduled && agent.AgentStates.TryGetValue(Agent.State.Move,out AgentState move))
        {
            data.Target = null;
            return move;
        }
        if ((data.Target == null || !data.Target.AgentData.Alive) && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
        {
            //TODO: Implement retargeting here
            data.Target = null;
            return idle;
        }
        
        Vector2 location = new Vector2(agent.Position.X, agent.Position.Z);
        Vector2 target = new Vector2(data.Target.Position.X, data.Target.Position.Z);
        float dist = Vector2.Distance(location, target);
        Vector2 direction = target - location;
        direction.Normalize();
        
        if (_pathingCompleted && _pathingScheduled)
        {
            //When pathing completes
            _pathingScheduled = false;
            _pathingCompleted = false;
            _pathCompleted = false;
            if (end == null)
            {
                _repath = true;
                _SearchNearby = true;
            }
            else
            {
                _points = Pathfinding.PathToQueueOfVectors(end);
                _points.Enqueue(target);
                _currentPoint = _points.Dequeue();
                _timeSinceLastRepath = 0;
            }
        }
        
        if ((_timeSinceLastRepath >= data.RepathDelay || _points.Count == 0 || _repath) && (dist > data.MaxAttackRange || dist < data.MinAttackRange))
        {
            _repath = false;
            Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(location, direction);
            Node start;
            Node goal;
            if (dist > data.MaxAttackRange)
            {
                Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(target, -direction);
                //If it's too far, walk to target
                if (_SearchNearby)
                {
                    _SearchNearby = false;
                    Point? newPoint = Pathfinding.GetFirstNearbyFreePoint(target, agent.ID);
                    if (newPoint.HasValue)
                    {
                        Console.WriteLine("Repathing to: " + newPoint.Value + " Previously pathed to: " + endPoint);
                        goal = new Node(newPoint.Value, null, 1);
                    }
                    else if(agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState exit))
                    {
                        return exit;
                    }
                    else
                    {
                        return this;
                    }
                }
                else
                {
                    
                    goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
                }
                start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
                
                Task.Factory.StartNew(() =>
                { 
                    end = Pathfinding.CalculatePath(goal, start, true, agent.ID);
                    _pathingCompleted = true;
                });
                _pathingScheduled = true;
            }
            else if (dist < data.MinAttackRange)
            {
                //If it's to close, walk away from target
                float awayDist = ((data.MaxAttackRange + data.MinAttackRange) / 2.0f) - dist;
                Vector2 offset = new Vector2(location.X - target.X, location.Y - target.Y);
                offset.Normalize();
                offset *= awayDist;
                
                start = new Node(new Point((int)location.X, (int)location.Y), null, 1);
                goal = new Node(new Point((int)(location.X + offset.X), (int)(location.Y + offset.Y)), null, 1);
                
                Task.Factory.StartNew(() =>
                { 
                    end = Pathfinding.CalculatePath(goal, start, true, agent.ID);
                    _pathingCompleted = true;
                });
                _pathingScheduled = true;
            }
        }
        
        if (_points != null && !_pathCompleted && !_pathingScheduled)
        {
            _timeSinceLastRepath += Globals.DeltaTime;
            _attackTimer += Globals.DeltaTime;
            if (dist > data.MaxAttackRange || dist < data.MinAttackRange)
            {
                if (Vector2.Distance(_currentPoint, location) <= data.MinPointDistance)
                {
                    if (_points.Count < 4 && end != null)
                    {
                        int id = Globals.Renderer.WorldRenderer.MapNodes[end.Location.X, end.Location.Y].AllyOccupantID;
                        if (id != agent.ID && id != 0)
                        {
                            _repath = true;
                            _SearchNearby = true;
                        }
                    }
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
            else
            {
                float angle = CivilianWander.AngleDegrees(agent.Direction, direction);
                if (MathF.Abs(angle) < 5.0f)
                {
                    if (_attackTimer >= data.AttackDelay)
                    {
                        //Successful attack
                        _attackTimer = 0;
                        if (data.Target.Type == Agent.AgentType.Soldier)
                        {
                            SoldierData soldierData = (SoldierData)data.Target.AgentData;
                            if (!soldierData.Alarmed)
                            {
                                soldierData.Awareness = soldierData.AwarenessThreshold * 2;
                                soldierData.Target = agent;
                            }
                        }
                        else
                        {
                            WandererData wandererData = (WandererData)data.Target.AgentData;
                            if (!wandererData.Alarmed)
                            {
                                wandererData.Awareness = wandererData.AwarenessThreshold * 2;
                                wandererData.Target = agent;
                            }
                        }
                        data.Target.AgentData.DealDamage(data.Damage);
                    }
                }
                else
                {
                    agent.UpdateRotation(direction);
                }
            }
        }
        
        return this;
    }
}