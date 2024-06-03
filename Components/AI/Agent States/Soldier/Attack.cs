using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.AgentData;

namespace RTS_Engine.Components.AI.Agent_States;

public class Attack : AgentState
{
    public Agent Target;
    
    private float _attackTimer;
    private float _timeSinceLastRepath = 10;
    
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
            return patrol;
        }
        Vector2 location = new Vector2(agent.Position.X, agent.Position.Z);
        Vector2 target = new Vector2(Target.Position.X, Target.Position.Z);
        float dist = Vector2.Distance(location, target);
        if ((_timeSinceLastRepath >= data.RepathDelay || _points.Count == 0) && (dist > data.MaxAttackRange || dist < data.MinAttackRange))
        {
            Node end = null;
            int attempts = 0;
            do
            {
                attempts++;
                if (attempts > _maxAttempts && agent.AgentStates.TryGetValue(Agent.State.Patrol, out AgentState value))
                {
                    //If pathing attempts fails, for some reason, return to patrolling
                    Target = null;
                    return value;
                }

                Node start;
                Node goal;
                if (dist > data.MaxAttackRange)
                {
                    //If it's to far, walk to target
                    start = new Node(new Point((int)location.X, (int)location.Y), null, 1);
                    goal = new Node(new Point((int)target.X, (int)target.Y), null, 1);
                    
                    end = Pathfinding.CalculatePath(goal, start);
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
            if (dist > data.MaxAttackRange || dist < data.MinAttackRange)
            {
                _attackTimer = 0;
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
                _attackTimer += Globals.DeltaTime;
                if (_attackTimer >= data.AttackDelay)
                {
                    //Successful attack
                    _attackTimer = 0;
                    Target.AgentData.DealDamage(data.Damage);
                }
            }
        }
        
        return this;
    }
}