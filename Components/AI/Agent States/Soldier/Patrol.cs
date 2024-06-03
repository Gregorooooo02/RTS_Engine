using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class Patrol : AgentState
{
    private int _currentPathId = -1;
    private SoldierData.PatrolType _currentPatrolType;

    private bool _reverse;

    private Queue<Vector2> _points;
    private Vector2 _currentPoint;
    
    private List<Vector2> _patrolPoints = new();
    private int _currentPatrolPointIndex;
    
    private bool _traversing = false;
    private int _maxAttempts = 10;
    public override void Initialize(Agent agent)
    {
        if (agent.AgentStates.TryAdd(Agent.State.Idle, new SoldierIdle()) && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle))
        {
            idle.Initialize(agent);
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
        if (data.pathID != _currentPathId || _currentPatrolType != data.PatrollingType)
        {
            _currentPathId = data.pathID;
            _currentPatrolType = data.PatrollingType;
            List<Vector3> points = Globals.AgentsManager.PatrolManager.GetPathByIndex(_currentPathId);
            _patrolPoints.Clear();
            foreach (Vector3 point in points)
            {
                _patrolPoints.Add(new Vector2(point.X,point.Z));
            }
        }
        
        if (_points == null || _points.Count == 0)
        {
            if (_traversing && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState value))
            {
                _traversing = false;
                return value;
            }
            Node end = null;
            int attempts = 0;
            do
            {
                Node start = new Node(new Point((int)location.X, (int)location.Y), null, 1);
                Node goal = new Node(new Point((int)(_patrolPoints[_currentPatrolPointIndex].X), (int)(_patrolPoints[_currentPatrolPointIndex].Y)), null, 1);

                UpdateIndex();
                
                end = Pathfinding.CalculatePath(goal, start);
                
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
        return this;
    }
}