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

                try
                {
                    if (((int)endPoint.X < 0 || (int)endPoint.Y < 0 ||
                         (int)endPoint.X > Globals.Renderer.WorldRenderer.MapNodes.GetLength(0) - 1 ||
                         (int)endPoint.Y > Globals.Renderer.WorldRenderer.MapNodes.GetLength(1) - 1) || Globals.Renderer.WorldRenderer.MapNodes[(int)endPoint.X,(int)endPoint.Y] == null)
                    {
                        angle *= -1;
                        if (angle >= 0)
                        {
                            angle += 0.2617993878f;
                        }
                        continue;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Length: " + Globals.Renderer.WorldRenderer.MapNodes.Length);
                    Console.WriteLine("Length in x: " + Globals.Renderer.WorldRenderer.MapNodes.GetLength(0));
                    Console.WriteLine("Length in y: " + Globals.Renderer.WorldRenderer.MapNodes.GetLength(1));
                    Console.WriteLine(Globals.Renderer.WorldRenderer.MapNodes.Length);
                    Console.WriteLine(endPoint);
                    throw;
                }
                
                Node start = new Node(new Point((int)startPoint.X, (int)startPoint.Y), null, 1);
                Node goal = new Node(new Point((int)endPoint.X, (int)endPoint.Y), null, 1);
            
                end = Pathfinding.CalculatePath(goal, start,false);
            } while (end is null);
            _points = Pathfinding.PathToQueueOfVectors(end);
            _points.Enqueue(_destination);
            _currentPoint = _points.Dequeue();
            //Console.WriteLine(_currentPoint);
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
        return this;
    }
}