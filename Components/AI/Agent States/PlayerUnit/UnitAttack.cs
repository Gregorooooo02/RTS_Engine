using System;
using System.Collections.Generic;
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
    private bool _attacking = false;

    private Vector2 finalPoint;
    private Queue<Vector2> _points;
    private Vector2 _currentPoint;
    
    public override void Initialize(Agent agent)
    {
        
    }

    public override AgentState UpdateState(Agent agent)
    {
        PlayerUnitData data = (PlayerUnitData)agent.AgentData;
        if (data.MovementScheduled && agent.AgentStates.TryGetValue(Agent.State.Move,out AgentState move))
        {
            data.Target = null;
            agent.ActiveClip = 4;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 2.0f;
            return move;
        }
        if ((data.Target == null || !data.Target.AgentData.Alive) && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
        {
            //TODO: Implement retargeting here
            data.Target = null;
            agent.ActiveClip = 2;
            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
            agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 1.0f;
            return idle;
        }
        
        Vector2 location = new Vector2(agent.Position.X, agent.Position.Z);
        Vector2 target = new Vector2(data.Target.Position.X, data.Target.Position.Z);
        float dist = Vector2.Distance(location, target) - data.Target.AttackingRadius;
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
                _points.Enqueue(finalPoint);
                _currentPoint = _points.Dequeue();
                _timeSinceLastRepath = 0;
            }
        }
        
        if (!_pathingScheduled && (_timeSinceLastRepath >= data.RepathDelay || _points.Count == 0 || _repath) && (dist > data.MaxAttackRange || dist < data.MinAttackRange))
        {
            _repath = false;
            Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(location, direction);
            Node start;
            Node goal;
            if (dist > data.MaxAttackRange)
            {
                Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(target - data.Target.AttackingRadius * direction, -direction);
                if (!Globals.Renderer.WorldRenderer.MapNodes[(int)endPoint.X, (int)endPoint.Y].Available)
                    _SearchNearby = true;
                //If it's too far, walk to target
                if (_SearchNearby)
                {
                    agent.ActiveClip = 4;
                    agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
                    agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 2.0f;
                    _SearchNearby = false;
                    Point? newPoint = Pathfinding.GetFirstNearbyFreePoint(target, agent.ID);
                    if (newPoint.HasValue)
                    {
                        goal = new Node(newPoint.Value, null, 1);
                    }
                    else if(agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState exit))
                    {
                        agent.ActiveClip = 2;
                        agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
                        agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 1.0f;
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
                
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                { 
                    end = Pathfinding.CalculatePath(goal, start, true, agent.ID);
                    finalPoint = target;
                    _pathingCompleted = true;
                });
                _pathingScheduled = true;
            }
            else if (dist < data.MinAttackRange)
            {
                //If it's to close, walk away from target
                agent.ActiveClip = 4;
                agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
                agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 2.0f;
                
                float awayDist = data.MaxAttackRange;
                Vector2 offset = new Vector2(location.X - target.X, location.Y - target.Y);
                offset.Normalize();
                offset *= awayDist;
                
                Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(location + offset, -direction);
                
                start = new Node(new Point((int)location.X, (int)location.Y), null, 1);
                goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
                
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                { 
                    end = Pathfinding.CalculatePath(goal, start, true, agent.ID);
                    finalPoint = location + offset;
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
                            agent.ActiveClip = 4;
                            agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
                            agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 2.0f;
                            _repath = true;
                            _SearchNearby = true;
                        }
                    }
                    if(_points.Count > 0)_currentPoint = _points.Dequeue();
                    else
                    {
                        agent.ActiveClip = 2;
                        agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
                        agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 1.0f;
                        _pathCompleted = true;
                    }
                }
                else
                {
                    agent.MoveToPoint(_currentPoint, data.WalkingSpeed);
                    if (Globals.Renderer.WorldRenderer.MapNodes[(int)_currentPoint.X, (int)_currentPoint.Y].AllyOccupantId !=
                        agent.ID && Globals.Renderer.WorldRenderer.MapNodes[(int)_currentPoint.X, (int)_currentPoint.Y].AllyOccupantId != 0)
                    {
                        //Repath
                        agent.ActiveClip = 4;
                        agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
                        agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 2.0f;
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
                        agent.ActiveClip = 0;
                        agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
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
                        else if(data.Target.Type == Agent.AgentType.Civilian)
                        {
                            WandererData wandererData = (WandererData)data.Target.AgentData;
                            if (!wandererData.Alarmed)
                            {
                                wandererData.Awareness = wandererData.AwarenessThreshold * 2;
                                wandererData.Target = agent;
                            }
                        }
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