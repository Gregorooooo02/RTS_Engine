﻿using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class SoldierPatrol : AgentState
{
    private string _currentPathId = "";
    private SoldierData.PatrolType _currentPatrolType;

    private bool _reverse;

    private Queue<Vector2> _points;
    private Vector2 _currentPoint;
    
    private List<Vector2> _patrolPoints = new();
    private int _currentPatrolPointIndex;
    
    Node _end = null;
    private bool _pathingScheduled = false;
    private bool _pathingCompleted = false;
    private bool _pathCompleted = false;
    private bool _repath = false;
    private bool _searchNearby = false;
    
    private Vector2 _destination;
    
    private bool _traversing = false;
    private int _maxAttempts = 10;

    private bool _changeMove = false;
    public override void Initialize(Agent agent)
    {
        if (agent.AgentStates.TryAdd(Agent.State.Idle, new SoldierIdle()) && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
        {
            idle.Initialize(agent);
        }
        
        if (agent.AgentStates.TryAdd(Agent.State.Attack, new SoldierAttack()) && agent.AgentStates.TryGetValue(Agent.State.Attack, out AgentState attack))
        {
            attack.Initialize(agent);
        }
    }

    private void UpdateIndex()
    {
        
        if (_currentPatrolType == SoldierData.PatrolType.Circle)
        {
            _currentPatrolPointIndex++;
            if (_currentPatrolPointIndex == _patrolPoints.Count)
            {
                _currentPatrolPointIndex = 0;
            }
        }
        else
        {
            if (_reverse)
            {
                _currentPatrolPointIndex--;
                if (_currentPatrolPointIndex < 0)
                {
                    _reverse = false;
                    _currentPatrolPointIndex = 0;
                }
            }
            else
            {
                _currentPatrolPointIndex++;
                if (_currentPatrolPointIndex >= _patrolPoints.Count)
                {
                    _reverse = true;
                    _currentPatrolPointIndex = _patrolPoints.Count - 1;
                }
            }
        }
    }
    
    
    public override AgentState UpdateState(Agent agent)
    {
        SoldierData data = (SoldierData)agent.AgentData;
        Vector2 location = new Vector2(agent.Position.X, agent.Position.Z);
        if (data.Awareness >= data.AwarenessThreshold && agent.AgentStates.TryGetValue(Agent.State.Attack, out AgentState attack))
        {
            _traversing = false;
            ((SoldierAttack)attack).Target = data.Target;
            _points.Clear();
            data.Awareness = 0;
            data.Alarmed = true;
            return attack;
        }
        if (data.PathId != _currentPathId || _currentPatrolType != data.PatrollingType)
        {
            _currentPathId = data.PathId;
            _currentPatrolType = data.PatrollingType;
            List<Vector3> points = Globals.AgentsManager.PatrolManager.GetPathByName(_currentPathId);
            _patrolPoints.Clear();
            foreach (Vector3 point in points)
            {
                _patrolPoints.Add(new Vector2(point.X,point.Z));
            }
        }

        if (_pathingCompleted && _pathingScheduled)
        {
            //When pathing completes
            _pathingScheduled = false;
            _pathingCompleted = false;
            UpdateIndex();
            if(_end != null)
            {
                _pathCompleted = false;
                _points = Pathfinding.PathToQueueOfVectors(_end);
                _points.Enqueue(_destination);
                _currentPoint = _points.Dequeue();
                _traversing = true;
            }
        }
        
        if ((_points == null || _points.Count == 0) && !_pathingScheduled)
        {
            _points?.Clear();
            if (_traversing && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
            {
                _traversing = false;
                return value;
            }
            Vector2 direction = _patrolPoints[_currentPatrolPointIndex] - location;
            direction.Normalize();
            Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(location, direction);
            Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(_patrolPoints[_currentPatrolPointIndex], -direction);

            _destination = endPoint;
            
            Node start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
            Node goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
            
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            { 
                _end = Pathfinding.CalculatePath(goal, start, true, agent.ID);
                _pathingCompleted = true;
            });
            _pathingScheduled = true;
        }
        
        /*
        if (_points == null || _points.Count == 0)
        {
            if (_traversing && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
            {
                _traversing = false;
                return value;
            }
            Node end = null;
            int attempts = 0;
            Vector2 direction = _patrolPoints[_currentPatrolPointIndex] - location;
            direction.Normalize();
            Vector2 startPoint = Agent.GetFirstIntersectingGridPoint(location, direction);
            Vector2 endPoint = Agent.GetFirstIntersectingGridPoint(_patrolPoints[_currentPatrolPointIndex], -direction);
            do
            {
                Node start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
                Node goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);

                UpdateIndex();
                
                end = Pathfinding.CalculatePath(goal, start,false);
                attempts++;
                if (attempts > _maxAttempts && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
                {
                    _traversing = false;
                    return idle;
                }

            } while (end is null);
            _points = Pathfinding.PathToQueueOfVectors(end);
            _points.Enqueue(new Vector2(_patrolPoints[_currentPatrolPointIndex].X,_patrolPoints[_currentPatrolPointIndex].Y));
            _traversing = true;
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
                agent.MoveToPoint(_currentPoint, data.PatrollingSpeed);
            }
        }
        */
        
        if (_points != null && !_pathCompleted && !_pathingScheduled)
        {
            if (Vector2.Distance(_currentPoint, location) <= data.MinPointDistance)
            {
                _currentPoint = _points.Dequeue();
            }
            else
            {
                if (_changeMove)
                {
                    _changeMove = false;
                    agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
                }
                if (agent.ActiveCivilianClip != 4 || agent.AnimatedRenderer._skinnedModel.AnimationController.Speed > 1.0f)
                {
                    agent.ActiveCivilianClip = 4;
                    agent.AnimatedRenderer._skinnedModel.ChangedClip = true;
                    agent.AnimatedRenderer._skinnedModel.AnimationController.Speed = 1.0f;
                    _changeMove = true;
                }
                if (agent.PeacefulTimer >= agent.PeacefulDelay && agent.Emitter != null)
                {
                    agent.PeacefulTimer = 0;
                    agent.Emitter.PlayMove();
                }
                agent.MoveToPoint(_currentPoint, data.PatrollingSpeed);
            }
        }
        
        return this;
    }
}