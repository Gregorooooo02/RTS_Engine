using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI.Agent_States;
using RTS_Engine.Components.AI.AgentData;
using RTS_Engine.Exceptions;

namespace RTS_Engine.Components.AI;

public class Agent : Component
{
    private static int CurrentID = 1;
    
    public bool IsHidden = false;
    
    public enum AgentType
    {
        Civilian,
        Soldier,
        EnemyBuilding,
        PlayerUnit
    }
    
    public enum State
    {
        Idle,
        Patrol,
        Attack,
        RunAway,
        Wander,
        Move
    }

    private float _turnSpeed = 2.0f;
    private bool removed = false;

    private float _occupyDistance = 5.75f;

    private float _heightOffset = 2.0f;

    public AgentData.AgentData AgentData;

    public Vector3 Position;
    public Vector2 Direction = Vector2.UnitX;
    public float DirectionOffset = 0.0f;
    
    private AgentState _currentState;
    public Dictionary<State, AgentState> AgentStates = new();

    public LayerType AgentLayer = LayerType.ENEMY;
    public AgentType Type = AgentType.Civilian;

    public GameObject UiObject = null;
    public GameObject HealthBar = null;
    public GameObject HealthBarBackground = null;
    public GameObject Icon = null;
    
    public MeshRenderer Renderer = null;
    public AnimatedMeshRenderer AnimatedRenderer = null;

    public List<int> AttackFrames = new();
    public int DeathFrame;

    private int _meatAward = 10;
    
    public int ActiveClip = 2;
    // **Unit animations**
    // Attack - 0
    // Death - 1
    // Idle - 2
    // Change to furniture - 3
    // Move - 4

    public int ActiveCivilianClip = 1;
    // **Civilian animations**
    // Death - 0
    // Flee - 1
    // Idle - 2
    // Wander - 3

    private bool _ChangeDeath = false;

    private int _deathCounter = 0;
    
    private readonly List<Point> _occupiedNodes = new();

    public int ID;

    public float AttackingRadius = 1.0f;
    
    public Agent(){}
    
    public override void Update()
    {
        if(!Active || Globals.IsPaused)return;
        
        UiObject ??= ParentObject.FindGameObjectByName("UI");
        Icon ??= UiObject?.FindGameObjectByName("Icon");
        HealthBarBackground ??= UiObject?.FindGameObjectByName("HP");
        HealthBar ??= HealthBarBackground?.Children[0];
        
        HealthBar?.Transform.SetLocalScaleX(AgentData.HpAsPercentage * Globals.Ratio);
        
        Position = ParentObject.Transform.ModelMatrix.Translation;
        if (Renderer == null || AnimatedRenderer == null)
        {
            Renderer = ParentObject.GetComponent<MeshRenderer>();
            AnimatedRenderer = ParentObject.GetComponent<AnimatedMeshRenderer>();
            if (Renderer == null && AnimatedRenderer == null)
            {
                Active = false;
                return;
            }
        }

        if (AnimatedRenderer != null && Type == AgentType.PlayerUnit)
        {
            AnimatedRenderer._skinnedModel.ActiveAnimationClip = ActiveClip;
        }
        
        if (AnimatedRenderer != null && Type == AgentType.Civilian)
        {
            AnimatedRenderer._skinnedModel.ActiveAnimationClip = ActiveCivilianClip;
        }

        if (!AgentData.Alive)
        {
            if (!removed)
            {
                //Here is code that should be executed only once when agent die
                removed = true;
                switch (AgentLayer)
                {
                    case LayerType.ENEMY:
                        Globals.AgentsManager.Enemies.Remove(this);
                        GameManager.AddMissionMeat(_meatAward);  
                        switch (Type)
                        {
                            case AgentType.Civilian:
                                Globals.AgentsManager.ClappedCivilians.Add(this);
                                break;
                            case AgentType.Soldier:
                                Globals.AgentsManager.ClappedSoldiers.Add(this);
                                break;
                            case AgentType.EnemyBuilding:
                                Globals.AgentsManager.ClappedBuildings.Add(this);
                                ChangeNodes(true);
                                break;
                        }
                        break;
                    case LayerType.PLAYER:
                        Globals.AgentsManager.Units.Remove(this);
                        Globals.AgentsManager.SelectedUnits.Remove(this);
                        Globals.AgentsManager.PlacePortraits();
                        HealthBarBackground.Active = false;
                        HealthBar.Active = false;
                        Icon.Active = false;
                        break;
                }
            }
            

            if (Type == AgentType.PlayerUnit && AnimatedRenderer != null)
            {
                if (_ChangeDeath)
                {
                    _ChangeDeath = false;
                    AnimatedRenderer._skinnedModel.ChangedClip = true;
                }
                if (ActiveClip != 1 || AnimatedRenderer._skinnedModel.AnimationController.Speed > 1.0f)
                {
                    ActiveClip = 1;
                    AnimatedRenderer._skinnedModel.ChangedClip = true;
                    AnimatedRenderer._skinnedModel.AnimationController.LoopEnabled = false;
                    AnimatedRenderer._skinnedModel.AnimationController.Speed = 1.0f;
                    _ChangeDeath = true;
                }

                _deathCounter++;
                if (_deathCounter >= DeathFrame)
                {
                    FogReveler reveler = ParentObject.GetComponent<FogReveler>();
                    if (reveler != null) reveler.Active = false;
                    ParentObject.Active = false;
                }
            }
            else
            {
                //Implement for others
                ParentObject.Active = false;
            }
            return;
        }
        
        if (AgentLayer == LayerType.ENEMY)
        {
            CheckFieldOfVision();
            if (Type != AgentType.EnemyBuilding && AnimatedRenderer != null)
            {
                AnimatedRenderer.AdditionalVisibility = Globals.FogManager.IsVisible(Position);
            }
        }
        
        try
        {
            _currentState = _currentState.UpdateState(this);
        }
        catch (NoTerrainException e)
        {
            Active = false;
            Console.WriteLine("No terrain found. Disabling agent");
        }

        if(Type == AgentType.EnemyBuilding && _occupiedNodes.Count == 0)UpdateOccupied();
        ChangeNodes(false);
    }

    private void ChangeNodes(bool clear)
    {
        if (Type == AgentType.PlayerUnit)
        {
            foreach (Point location in _occupiedNodes)
            {
                try
                {
                    if (clear)
                    {
                        if (Globals.Renderer.WorldRenderer.MapNodes[location.X, location.Y].AllyOccupantId == ID)
                        {
                            Globals.Renderer.WorldRenderer.MapNodes[location.X, location.Y].AllyOccupantId = 0;
                        }
                    }
                    else
                    {
                        if (Globals.Renderer.WorldRenderer.MapNodes[location.X, location.Y].AllyOccupantId == 0)
                        {
                            Globals.Renderer.WorldRenderer.MapNodes[location.X, location.Y].AllyOccupantId = ID;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unit out of map!");
                }
            }
        }
        else if(Type == AgentType.EnemyBuilding)
        {
            foreach (Point location in _occupiedNodes)
            {
                try
                {
                    Globals.Renderer.WorldRenderer.MapNodes[location.X, location.Y].Available = clear;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Building out of map!");
                }
               
            }
        }
    }

    private void UpdateOccupied()
    {
        _occupiedNodes.Clear();
        int leftX = (int)MathF.Ceiling(Position.X - _occupyDistance);
        int rightX = (int)(Position.X + _occupyDistance);
        
        int topY = (int)MathF.Ceiling(Position.Z - _occupyDistance);
        int bottomY = (int)(Position.Z + _occupyDistance);
        
        for (int i = leftX; i <= rightX; i++)
        {
            for (int j = topY; j <= bottomY; j++)
            {
                _occupiedNodes.Add(new Point(i,j));
            }
        }
    }
    
    public void MoveToPoint(Vector2 point, float speed)
    {
        if (Type == AgentType.PlayerUnit)
        {
            ChangeNodes(true);
            UpdateOccupied();
        }
        Vector3 offset = new Vector3(point.X - Position.X, 0, point.Y - Position.Z);
        offset.Y = PickingManager.InterpolateWorldHeight(new Vector2(Position.X + offset.X, Position.Z + offset.Z)) - Position.Y + _heightOffset;
        offset.Normalize();
        ParentObject.Transform.Move(offset * Globals.DeltaTime * speed);

        Vector2 destinationVector = new Vector2(offset.X, offset.Z);
        UpdateRotation(destinationVector);
    }

    public void UpdateRotation(Vector2 desiredDirection)
    {
        desiredDirection.Normalize();
        Direction = Vector2.Lerp(Direction,desiredDirection, Globals.DeltaTime * _turnSpeed);
        
        float angle = CivilianWander.AngleDegrees(Vector2.UnitY, Direction);
        ParentObject.Transform.SetLocalRotationY(-angle + DirectionOffset);
    }
    

    private void CheckFieldOfVision()
    {
        Vector2 offset;
        if (Type == AgentType.Civilian)
        {
            WandererData data = (WandererData)AgentData;
            if(data.Alarmed) return;
            if(data.Awareness < data.AwarenessThreshold)data.Target = null;
            foreach (Agent t in Globals.AgentsManager.Units)
            {
                if(!t.AgentData.Alive || !t.ParentObject.Active || t.IsHidden) continue;
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
                            if (!node.Available || (node.Height - Position.Y) > data.SightHeight)
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
            if(data.Awareness < data.AwarenessThreshold)data.Target = null;
            foreach (Agent t in Globals.AgentsManager.Units)
            {
                if(!t.AgentData.Alive || !t.ParentObject.Active || t.IsHidden) continue;
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

    public static Vector2 GetFirstIntersectingGridPoint(Vector2 startingPos,Vector2 direction)
    {
        float k1 = 10, k2 = 10;
        if (direction.X > 0)
        {
            k1 = -((startingPos.X % 1) - 1.0f / direction.X);
        }
        else if(direction.X < 0)
        {
            k1 = -((startingPos.X % 1) / direction.X);
        }
        if (direction.Y > 0)
        {
            k2 = -((startingPos.Y % 1) - 1.0f / direction.Y);
        }
        else if(direction.Y < 0)
        {
            k2 = -((startingPos.Y % 1) / direction.Y);
        }

        Vector2 intersectionPoint = startingPos + (k1 < k2 ? k1 : k2) * direction;
        intersectionPoint.Round();
        return intersectionPoint;
    }
    
    private List<Point> GetIntersections(Vector2 source, Vector2 destination)
    {
        List<Point> output = new();
        Vector2 direction = new Vector2(destination.X - source.X, destination.Y - source.Y);
        direction.Normalize();
        
        /*
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
        */
        Vector2 intersectionPoint = GetFirstIntersectingGridPoint(source, direction);
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
        ID = CurrentID;
        CurrentID++;
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Agent</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<agentType>" + Type + "</agentType>");
        
        builder.Append("<agentLayer>" + AgentLayer + "</agentLayer>");
        
        builder.Append("<separationDistance>" + _occupyDistance + "</separationDistance>");
        
        builder.Append("<attackingRadius>" + AttackingRadius + "</attackingRadius>");
        
        builder.Append("<meatAward>" + _meatAward + "</meatAward>");
        
        builder.Append("<heightOffset>" + _heightOffset + "</heightOffset>");
        
        builder.Append("<directionOffset>" + DirectionOffset + "</directionOffset>");
        
        builder.Append("<deathFrame>" + DeathFrame + "</deathFrame>");
        
        builder.Append("<attackFrames>");
        
        builder.Append("<count>" + AttackFrames.Count + "</count>");

        int frameID = 0;
        
        foreach (int frame in AttackFrames)
        {
            builder.Append("<f" + frameID + ">" + frame + "</f" + frameID + ">");
            frameID++;
        }
        
        builder.Append("</attackFrames>");
        
        builder.Append("<agentData>" + AgentData.Serialize() + "</agentData>");
        
        builder.Append("</component>");
        
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        Type = (AgentType)Enum.Parse(typeof(AgentType), element.Element("agentType")?.Value);
        AgentLayer = (LayerType)Enum.Parse(typeof(LayerType), element.Element("agentLayer")?.Value);
        _occupyDistance = float.TryParse(element.Element("separationDistance")?.Value, out float sep) ? sep : 1.0f;
        _heightOffset = float.TryParse(element.Element("heightOffset")?.Value, out float offset) ? offset : 2.0f;
        DirectionOffset = float.TryParse(element.Element("directionOffset")?.Value, out float dir) ? dir : 0.0f;
        AttackingRadius = float.TryParse(element.Element("attackingRadius")?.Value, out float radius) ? radius : 1.0f;
        _meatAward = int.TryParse(element.Element("meatAward")?.Value, out int meatAward) ? meatAward : 10;
        switch (Type)
        {
            case AgentType.Civilian:
                AgentData = new WandererData();
                AgentData.Deserialize(element.Element("agentData"));
                _currentState = ((WandererData)AgentData).EntryState;
                break;
            case AgentType.Soldier:
                AgentData = new SoldierData();
                AgentData.Deserialize(element.Element("agentData"));
                _currentState = ((SoldierData)AgentData).EntryState;
                break;
            case AgentType.PlayerUnit:
                AgentData = new PlayerUnitData();
                AgentData.Deserialize(element.Element("agentData"), true);
                _currentState = ((PlayerUnitData)AgentData).EntryState;
                break;
            case AgentType.EnemyBuilding:
                AgentData = new BuildingData();
                AgentData.Deserialize(element.Element("agentData"));
                _currentState = ((BuildingData)AgentData).EntryState;
                break;
        }
        
        _currentState.Initialize(this);
        switch (AgentLayer)
        {
            case LayerType.ENEMY:
                Globals.AgentsManager.Enemies.Add(this);
                break;
            case LayerType.PLAYER:
                Globals.AgentsManager.Units.Add(this);
                break;
        }
        ID = CurrentID;
        CurrentID++;
        
        DeathFrame = int.TryParse(element.Element("deathFrame")?.Value, out int frame) ? frame : 0;
        XElement attackFrames = element.Element("attackFrames");
        if(attackFrames == null) return;
        int amount = int.TryParse(attackFrames?.Element("count").Value, out int a) ? a : 0;
        for (int i = 0; i < amount; i++)
        {
            if (int.TryParse(attackFrames.Element("f" + i).Value, out int b))
            {
                AttackFrames.Add(b);
            }
        }
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
    private bool _addFrame = false;
    private int _frameToAdd = 0;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Agent"))
        {
            ImGui.Text("Agent ID: " + ID);
            ImGui.DragFloat("Occupancy distance", ref _occupyDistance);
            ImGui.DragFloat("Attacking distance", ref AttackingRadius);
            ImGui.DragFloat("Height offset", ref _heightOffset);
            ImGui.DragInt("Meat reward", ref _meatAward, 1, 0);
            ImGui.DragFloat("Direction offset", ref DirectionOffset, -180.0f, 180.0f);
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

            ImGui.DragFloat("Turn speed", ref _turnSpeed);
            AgentData.Inspect();
            ImGui.Separator();
            ImGui.InputInt("Death frame", ref DeathFrame);
            ImGui.Text("Current attack frames:");
            if (AttackFrames.Count == 0)
            {
                ImGui.Text("None");
            }
            foreach (int frame in AttackFrames)
            {
                ImGui.Text(frame.ToString());
            }

            if (ImGui.Button("Add frame"))
            {
                _addFrame = true;
            }
            if (ImGui.Button("Remove last frame") && AttackFrames.Count != 0)
            {
                AttackFrames.RemoveAt(AttackFrames.Count - 1);
            }

            if (ImGui.Button("Reset death frame"))
            {
                _deathCounter = 0;
                if (Type == AgentType.PlayerUnit)
                {
                    FogReveler reveler = ParentObject.GetComponent<FogReveler>();
                    if (reveler != null) reveler.Active = true;
                }
            }
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }

        if (_addFrame)
        {
            ImGui.Begin("Add attack frame");
            ImGui.InputInt("Frame to add", ref _frameToAdd);
            if (ImGui.Button("Add frame"))
            {
                AttackFrames.Add(_frameToAdd);
                _addFrame = false;
            }
            if (ImGui.Button("Cancel"))
            {
                _addFrame = false;
            }
            ImGui.End();
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
                        case AgentType.EnemyBuilding:
                            AgentData = new BuildingData();
                            _currentState = ((BuildingData)AgentData).EntryState;
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