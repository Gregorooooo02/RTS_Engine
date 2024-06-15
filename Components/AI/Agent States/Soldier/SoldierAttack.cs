using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class SoldierAttack : AgentState
{
    public Agent Target;

    private float _cooldownTimer;
    private float _attackTimer;
    private float _timeSinceLastRepath = 10;
    
    private bool _pathingScheduled = false;
    private bool _pathingCompleted = false;
    private bool _pathCompleted = false;
    private bool _repath = false;
    private bool _SearchNearby = false;
    private bool _attacking = false;
    
    Node end = null;
    
    private Queue<Vector2> _points;
    private Vector2 _currentPoint;
    private readonly int _maxAttempts = 10;
    
    public override void Initialize(Agent agent)
    {
        
    }

    public override AgentState UpdateState(Agent agent)
    {
        SoldierData data = (SoldierData)agent.AgentData;
        if ((Target == null || !Target.AgentData.Alive) && agent.AgentStates.TryGetValue(Agent.State.Patrol,out AgentState patrol))
        {
            data.Alarmed = false;
            Target = null;
            return patrol;
        }
        Vector2 location = new Vector2(agent.Position.X, agent.Position.Z);
        Vector2 target = new Vector2(Target.Position.X, Target.Position.Z);
        float dist = Vector2.Distance(location, target);
        if (dist > data.MinDistanceToCoolDown)
        {
            _cooldownTimer += Globals.DeltaTime;
            if (_cooldownTimer >= data.TimeToCoolDown && agent.AgentStates.TryGetValue(Agent.State.Patrol,out AgentState back))
            {
                data.Alarmed = false;
                Target = null;
                return back;
            }
        }
        else
        {
            _cooldownTimer = 0;
        }
        Vector2 direction = target - location;
        direction.Normalize();
        
        
        
        /*
        if ((_timeSinceLastRepath >= data.RepathDelay || _points.Count == 0) && (dist > data.MaxAttackRange || dist < data.MinAttackRange))
        {
            Node end = null;
            int attempts = 0;
            Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(location, direction);
            Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(target, -direction);
            do
            {
                attempts++;
                if (attempts > _maxAttempts && agent.AgentStates.TryGetValue(Agent.State.Patrol, out AgentState value))
                {
                    //If pathing attempts fails, for some reason, return to patrolling
                    data.Alarmed = false;
                    Target = null;
                    return value;
                }

                Node start;
                Node goal;
                if (dist > data.MaxAttackRange)
                {
                    //If it's to far, walk to target
                    start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
                    goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
                    
                    end = Pathfinding.CalculatePath(goal, start);
                }
                else if (dist < data.MinAttackRange)
                {
                    //If it's to close, walk away from target
                    float awayDist = ((data.MaxAttackRange + data.MinAttackRange) / 2.0f) - dist;
                    Vector2 offset = new Vector2(location.X - target.X, location.Y - target.Y);
                    offset.Normalize();
                    offset *= awayDist;
                    
                    start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
                    goal = new Node(new Point((int)(location.X + offset.X), (int)(location.Y + offset.Y)), null, 1);
                    
                    end = Pathfinding.CalculatePath(goal, start ,false);
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
                    agent.MoveToPoint(_currentPoint, data.AttackingWalkingSpeed);
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
                        PlayerUnitData playerUnitData = (PlayerUnitData)Target.AgentData;
                        playerUnitData.Target ??= agent;
                        _attackTimer = 0;
                        Target.AgentData.DealDamage(data.Damage);
                    }
                }
                else
                {
                    agent.UpdateRotation(direction);
                }
            }
        }
        */
        
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
                Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(target - data.Target.AttackingRadius * direction, -direction);
                //If it's too far, walk to target
                if (_SearchNearby)
                {
                    _SearchNearby = false;
                    Point? newPoint = Pathfinding.GetFirstNearbyFreePoint(target, agent.ID);
                    if (newPoint.HasValue)
                    {
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
            float diff = data.MaxAttackRange - data.MinAttackRange;
            if (((dist > data.MaxAttackRange || dist < data.MinAttackRange) && _attacking) || ((dist > data.MaxAttackRange - diff / 3.0f || dist < data.MinAttackRange + diff / 3.0f) && !_attacking))
            {
                _attacking = false;
                if (Vector2.Distance(_currentPoint, location) <= data.MinPointDistance)
                {
                    if (_points.Count < 4 && end != null)
                    {
                        int id = Globals.Renderer.WorldRenderer.MapNodes[end.Location.X, end.Location.Y].AllyOccupantId;
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
                    agent.MoveToPoint(_currentPoint, data.AttackingWalkingSpeed);
                    if (Globals.Renderer.WorldRenderer.MapNodes[(int)_currentPoint.X, (int)_currentPoint.Y].AllyOccupantId !=
                        agent.ID && Globals.Renderer.WorldRenderer.MapNodes[(int)_currentPoint.X, (int)_currentPoint.Y].AllyOccupantId != 0)
                    {
                        //Repath
                        _repath = true;
                    }
                }
            }
            else
            {
                _attacking = true;
                float angle = CivilianWander.AngleDegrees(agent.Direction, direction);
                if (MathF.Abs(angle) < 5.0f)
                {
                    //TODO: After adding animations remember to modify the if statement below to check for specific animation frame
                    if (_attackTimer >= data.AttackDelay)
                    {
                        //Successful attack
                        _attackTimer = 0;
                        if (data.IsRanged)
                        {
                            Globals.AgentsManager.ProjectileManager.AddProjectile(ProjectileManager.ProjectileType.Arrow,data.Target,data.ProjectileSpeed,data.ProjectileMinDistance, data.Damage, agent.Position);
                        }
                        else
                        {
                            data.Target.AgentData.DealDamage(data.Damage);
                        }
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