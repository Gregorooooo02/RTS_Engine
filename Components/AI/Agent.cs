using System;
using System.Collections.Generic;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.Agent_States;
using RTS_Engine.Components.AI.AgentData;
using RTS_Engine.Exceptions;

namespace RTS_Engine.Components.AI;

public class Agent : Component
{
    public enum AgentType
    {
        Civilian,
        Soldier,
        PlayerUnit
    }
    
    public enum State
    {
        Idle,
        Patrol,
        Attack,
        RunAway,
        Wander
    }
    
    public float TurnSpeed = 2.0f;

    public AgentData.AgentData AgentData;

    public Vector3 Position;
    public Vector2 Direction = Vector2.UnitX;
    
    private AgentState _currentState;
    public Dictionary<State, AgentState> AgentStates = new();

    public LayerType AgentLayer = LayerType.ENEMY;
    public AgentType Type = AgentType.Civilian;
    
    public Agent(){}
    
    public override void Update()
    {
        if(!Active)return;
        Position = ParentObject.Transform.ModelMatrix.Translation;
        if (!AgentData.Alive)
        {
            ParentObject.Active = false;
            switch (AgentLayer)
            {
                case LayerType.ENEMY:
                    Globals.AgentsManager.Enemies.Remove(this);
                    break;
                case LayerType.PLAYER:
                    Globals.AgentsManager.Units.Remove(this);
                    break;
            }
            return;
        }
        
        if (AgentLayer == LayerType.ENEMY)
        {
            CheckFieldOfVision();
        }
        
        try
        {
            if(AgentLayer == LayerType.ENEMY)UpdateState();
        }
        catch (NoTerrainException e)
        {
            Active = false;
            Console.WriteLine("No terrain found. Disabling agent");
        }
    }

    private void UpdateState()
    {
        _currentState = _currentState.UpdateState(this);
    }

    public void MoveToPoint(Vector2 point, float speed)
    {
        Vector3 agentPosition = ParentObject.Transform.ModelMatrix.Translation;
        Vector3 offset = new Vector3(point.X - agentPosition.X, 0, point.Y - agentPosition.Z);
        offset.Y = PickingManager.InterpolateWorldHeight(new Vector2(agentPosition.X + offset.X, agentPosition.Z + offset.Z)) - agentPosition.Y + 2.0f;
        offset.Normalize();
        ParentObject.Transform.Move(offset * Globals.DeltaTime * speed);

        Vector2 destinationVector = new Vector2(offset.X, offset.Z);
        destinationVector.Normalize();
        Direction = Vector2.Lerp(Direction,destinationVector, Globals.DeltaTime * TurnSpeed);
        
        float angle = CivilianWander.AngleDegrees(Vector2.UnitY, Direction);
        ParentObject.Transform.SetLocalRotationY(-angle);
    }

    private void CheckFieldOfVision()
    {
        Vector2 offset;
        if (Type == AgentType.Civilian)
        {
            WandererData data = (WandererData)AgentData;
            if(data.Alarmed) return;
            data.Target = null;
            foreach (Agent t in Globals.AgentsManager.Units)
            {
                //Calculate offset vector from this civilian to player unit from the list
                offset = new Vector2(t.Position.X - Position.X, t.Position.Z - Position.Z);
                float length = offset.Length();
                //Console.WriteLine(MathF.Abs(Wander.AngleDegrees(Direction,offset)));
                //Using if's condition order, first check if checked player unit is in sight range and if it is
                //then check the angle between civilian's direction vector and offset vector.
                //If absolute value of this angle is smaller than half of civilian's fov that means that it's in sight 
                if (length <= data.SightRange && MathF.Abs(CivilianWander.AngleDegrees(Direction,offset)) <= data.SightAngle / 2.0f)
                {
                    //This means checked unit is in vision cone
                    //Now check if sight line is not obstructed by terrain or static props
                    //First lets check the difference in height between them, if it's too high target is not visible
                    if (MathF.Abs(t.Position.Y - Position.Y) <= data.SightHeight)
                    {
                        //Get coordinates of all point that the line of sight "intersects with"
                        List<Point> points = GetIntersections(new Vector2(Position.X, Position.Z),
                            new Vector2(t.Position.X, t.Position.Z));
                        bool success = true;
                        //Then use this point to determine visibility
                        foreach (Point point in points)
                        {
                            MapNode node = Globals.Renderer.WorldRenderer.MapNodes[point.X, point.Y];
                            if (node == null || (node.Height - Position.Y) > data.SightHeight)
                            {
                                //if null then node doesn't exist there implying an obstacle
                                //if second condition passes then it means that there is a significant enough incline in terrain to obstruct line of sight
                                success = false;
                                break;
                            }
                        }
                        if (success)
                        {
                            PlayerUnitData playerUnitData = (PlayerUnitData)t.AgentData;
                            data.Target = t;
                            data.Awareness += playerUnitData.Presence * ((1.0f - (length / data.SightRange)) * (1.0f - data.MinPresenceMultiplier) + data.MinPresenceMultiplier);
                        }
                    }
                }
            }
            if(data.Awareness > 0)data.Awareness -= data.AwarenessDecay;
        } 
        else if (Type == AgentType.Soldier)
        {
            SoldierData data = (SoldierData)AgentData;
            if(data.Alarmed) return;
            data.Target = null;
            foreach (Agent t in Globals.AgentsManager.Units)
            {
                //Calculate offset vector from this civilian to player unit from the list
                offset = new Vector2(t.Position.X - Position.X, t.Position.Z - Position.Z);
                float length = offset.Length();
                //Console.WriteLine(MathF.Abs(Wander.AngleDegrees(Direction,offset)));
                //Using if's condition order, first check if checked player unit is in sight range and if it is
                //then check the angle between civilian's direction vector and offset vector.
                //If absolute value of this angle is smaller than half of civilian's fov that means that it's in sight 
                if (length <= data.SightRange && MathF.Abs(CivilianWander.AngleDegrees(Direction,offset)) <= data.SightAngle / 2.0f)
                {
                    //This means checked unit is in vision cone
                    //Now check if sight line is not obstructed by terrain or static props
                    //First lets check the difference in height between them, if it's too high target is not visible
                    if (MathF.Abs(t.Position.Y - Position.Y) <= data.SightHeight)
                    {
                        //Get coordinates of all point that the line of sight "intersects with"
                        List<Point> points = GetIntersections(new Vector2(Position.X, Position.Z),
                            new Vector2(t.Position.X, t.Position.Z));
                        bool success = true;
                        //Then use this point to determine visibility
                        foreach (Point point in points)
                        {
                            MapNode node = Globals.Renderer.WorldRenderer.MapNodes[point.X, point.Y];
                            if (node == null || (node.Height - Position.Y) > data.SightHeight)
                            {
                                //if null then node doesn't exist there implying an obstacle
                                //if second condition passes then it means that there is a significant enough incline in terrain to obstruct line of sight
                                success = false;
                                break;
                            }
                        }
                        if (success)
                        {
                            PlayerUnitData playerUnitData = (PlayerUnitData)t.AgentData;
                            data.Target = t;
                            data.Awareness += playerUnitData.Presence * ((1.0f - (length / data.SightRange)) * (1.0f - data.MinPresenceMultiplier) + data.MinPresenceMultiplier);
                        }
                    }
                }
            }
            if(data.Awareness > 0)data.Awareness -= data.AwarenessDecay;
        }
        
    }

    private List<Point> GetIntersections(Vector2 source, Vector2 destination)
    {
        List<Point> output = new();
        Vector2 direction = new Vector2(destination.X - source.X, destination.Y - source.Y);
        direction.Normalize();
        
        float k1 = 10, k2 = 10;
        if (direction.X > 0)
        {
            k1 = -((source.X % 1) - 1.0f / direction.X);
        }
        else if(direction.X < 0)
        {
            k1 = -((source.X % 1) / direction.X);
        }
        if (direction.Y > 0)
        {
            k2 = -((source.Y % 1) - 1.0f / direction.Y);
        }
        else if(direction.Y < 0)
        {
            k2 = -((source.Y % 1) / direction.Y);
        }

        Vector2 intersectionPoint = source + (k1 < k2 ? k1 : k2) * direction;
        intersectionPoint.Round();
        //Now 'intersectionPoint' holds coordinates of the first point on the "line"
        
        float ratio;
        output.Add(new Point((int)intersectionPoint.X,(int)intersectionPoint.Y));
        if (direction.X > direction.Y)
        {
            ratio = direction.Y / direction.X;
            int y;
            for (int x = output[0].X; x < destination.X; x++)
            {
                y = (int)Math.Round((x - output[0].X)* ratio + output[0].Y);
                output.Add(new Point(x, y));
            }
        }
        else
        {
            ratio = direction.X / direction.Y;
            int x;
            for (int y = output[0].Y; y < destination.Y; y++)
            {
                x = (int)Math.Round((y - output[0].Y) * ratio + output[0].X);
                output.Add(new Point(x, y));
            }
        }
        return output;
    }
    
    public override void Initialize()
    {
        AgentData = new WandererData();
        _currentState = ((WandererData)AgentData).EntryState;
        _currentState.Initialize(this);
    }

    public override string ComponentToXmlString()
    {
        throw new System.NotImplementedException();
    }

    public override void Deserialize(XElement element)
    {
        throw new System.NotImplementedException();
    }

    public override void RemoveComponent()
    {
        switch (AgentLayer)
        {
            case LayerType.ENEMY:
                Globals.AgentsManager.Enemies.Remove(this);
                break;
            case LayerType.PLAYER:
                Globals.AgentsManager.Units.Remove(this);
                break;
        }
        ParentObject.RemoveComponent(this);
    }

#if DEBUG
    private bool _changingBehavior = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Agent"))
        {
            ImGui.Checkbox("Agent active", ref Active);
            ImGui.Text("Agent type: " + AgentLayer);
            ImGui.SameLine();
            if (ImGui.Button("Switch side"))
            {
                switch (AgentLayer)
                {
                    case LayerType.ENEMY:
                        Globals.AgentsManager.Enemies.Remove(this);
                        Globals.AgentsManager.Units.Add(this);
                        break;
                    case LayerType.PLAYER:
                        Globals.AgentsManager.Enemies.Add(this);
                        Globals.AgentsManager.Units.Remove(this);
                        break;
                }
                AgentLayer = AgentLayer == LayerType.ENEMY ? LayerType.PLAYER : LayerType.ENEMY;
            }
            ImGui.Text("Current state: " + _currentState);
            if (ImGui.Button("Switch behavior type"))
            {
                _changingBehavior = true;
            }
            AgentData.Inspect();
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }

        if (_changingBehavior)
        {
            ImGui.Begin("Change behavior");
            var values = Enum.GetValues(typeof(AgentType));
            foreach (AgentType type in values)
            {
                if (ImGui.Button(type.ToString()))
                {
                    if (Type == type)
                    {
                        _changingBehavior = false;
                        ImGui.End();
                        return;
                    }
                    Type = type;
                    switch (type)
                    {
                        case AgentType.Civilian:
                            AgentData = new WandererData();
                            _currentState = ((WandererData)AgentData).EntryState;
                            break;
                        case AgentType.Soldier:
                            AgentData = new SoldierData();
                            _currentState = ((SoldierData)AgentData).EntryState;
                            break;
                        case AgentType.PlayerUnit:
                            AgentData = new PlayerUnitData();
                            _currentState = ((PlayerUnitData)AgentData).EntryState;
                            break;
                    }
                    AgentStates.Clear();
                    _currentState.Initialize(this);
                    _changingBehavior = false;
                }
            }
            if (ImGui.Button("Cancel"))
            {
                _changingBehavior = false;
            }
            ImGui.End();
        }
    }
#endif
}