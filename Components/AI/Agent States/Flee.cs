using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.AgentData;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace RTS_Engine.Components.AI.Agent_States;

public class Flee : AgentState
{
    public Agent Target;
    private float TimeSinceLastRepath = 10;
    private Vector2 Destination;
    
    private Queue<Point> _points;
    private Point _currentPoint;
    private int _maxAttempts = 10;
    
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

        if (Vector2.Distance(Destination, location) <= data.FleeingSpeed || TimeSinceLastRepath >= data.RepathDelay)
        {
            Node end = null;
            int attempts = 0;
            do
            {
                Vector2 offset = location - target;
                offset.Normalize();
                offset *= data.FleeingDistance;
                

                Destination = location + offset;
                
                //TODO: Try changing direction in this if
                if((int)(location.X + offset.X) < 0 || (int)(location.Y + offset.Y) < 0) continue;
                
                Node start = new Node(new Point((int)location.X, (int)location.Y), null, 1);
                Node goal = new Node(new Point((int)(location.X + offset.X), (int)(location.Y + offset.Y)), null, 1);
            
                end = Pathfinding.CalculatePath(goal, start);
                attempts++;
                if (attempts > _maxAttempts && agent.AgentStates.TryGetValue(Agent.State.Idle, out AgentState idle) && agent.AgentStates.TryGetValue(Agent.State.Wander, out AgentState wander))
                {
                    Console.WriteLine(attempts);
                    //If flee attempts fails return to idle
                    ((Idle)idle).Caller = wander;
                    Target = null;
                    data.Alarmed = false;
                    return idle;
                }
            } while (end is null);
            _points = Pathfinding.PathToQueueOfPoints(end);
            _currentPoint = _points.Dequeue();
            //Console.WriteLine(_currentPoint);
            TimeSinceLastRepath = 0;
        }
        else
        {
            TimeSinceLastRepath += Globals.DeltaTime;
            if (Wander.Distance(_currentPoint, agent.Position) <= data.MinPointDistance)
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