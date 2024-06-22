using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using RTS_Engine.Components.AI;
using Quaternion = System.Numerics.Quaternion;

namespace RTS_Engine;

public class Button : Component
{
    public SpiteRenderer ButtonVisual = null;
    private GameAction _buttonAction = GameAction.EXIT;

    private Vector3 _pos = new();
    private Vector3 _scale = new();

    public string GameObjectName = "Root";

    public bool IsPressed = false;

    private bool _unitPortrait = false;
    private bool _isUnitSelect = false;
    private int _unitID = 0;
    private Agent _agent;
    
    public override void Update()
    {
        if (Active)
        {
            if (ButtonVisual != null)
            {
                
                if (ButtonVisual.Active)
                {
                    MouseAction action = InputManager.Instance.GetMouseAction(GameAction.LMB);
                    if (_unitPortrait)
                    {
                        _agent ??= ParentObject.Parent.Parent.GetComponent<Agent>();
                        if (action is { state: ActionState.RELEASED} && _agent != null)
                        {
                            if (ButtonVisual.useLocalPosition)
                            {
                                _pos.X = ParentObject.Transform.Pos.X;
                                _pos.Y = ParentObject.Transform.Pos.Y;
                                _scale.X = ParentObject.Transform.Scl.X;
                                _scale.Y = ParentObject.Transform.Scl.Y;
                            }
                            if (action.StartingPosition.X >= _pos.X
                                    && action.StartingPosition.X <= _pos.X + ButtonVisual.Sprite.Width * _scale.X
                                    && action.StartingPosition.Y >= _pos.Y
                                    && action.StartingPosition.Y <= _pos.Y + ButtonVisual.Sprite.Height * _scale.Y)
                            {
                                Globals.HitUI = true;
                                if (InputManager.Instance.IsActive(GameAction.CTRL))
                                {
                                    if (!Globals.AgentsManager.SelectedUnits.Contains(_agent))
                                    {
                                        Globals.AgentsManager.SelectedUnits.Add(_agent);
                                        AgentsManager.ChangeUnitSelection(_agent, true);
                                    }
                                }
                                else
                                {
                                    Globals.AgentsManager.DeselectAllUnits();
                                    Globals.AgentsManager.SelectedUnits.Add(_agent);
                                    AgentsManager.ChangeUnitSelection(_agent, true);
                                }
                                return;
                            }
                        }
                        MouseAction action2 = InputManager.Instance.GetMouseAction(GameAction.MMB);
                        if (action2 is { state: ActionState.RELEASED } && _agent != null)
                        {
                            if (ButtonVisual.useLocalPosition)
                            {
                                _pos.X = ParentObject.Transform.Pos.X;
                                _pos.Y = ParentObject.Transform.Pos.Y;
                                _scale.X = ParentObject.Transform.Scl.X;
                                _scale.Y = ParentObject.Transform.Scl.Y;
                            }
                            if (action2.StartingPosition.X >= _pos.X
                                && action2.StartingPosition.X <= _pos.X + ButtonVisual.Sprite.Width * _scale.X
                                && action2.StartingPosition.Y >= _pos.Y
                                && action2.StartingPosition.Y <= _pos.Y + ButtonVisual.Sprite.Height * _scale.Y)
                            {
                                Globals.CurrentCamera.MoveCameraToPosition(_agent.Position);
                            }
                        }
                        return;
                    }
                    //Check if the LMB was pressed
                    if (action is {state: ActionState.RELEASED})
                    {
                        if (ButtonVisual.useLocalPosition)
                        {
                            _pos.X = ParentObject.Transform.Pos.X;
                            _pos.Y = ParentObject.Transform.Pos.Y;
                            _scale.X = ParentObject.Transform.Scl.X;
                            _scale.Y = ParentObject.Transform.Scl.Y;
                        }
                        else
                        {
                            ParentObject.Transform.ModelMatrix.Decompose(out Vector3 scale, out Microsoft.Xna.Framework.Quaternion k,
                                out Vector3 translation);
                            _pos.X = translation.X;
                            _pos.Y = translation.Y;
                            _scale.X = scale.X;
                            _scale.Y = scale.Y;
                        }
                        
                        //Check if mouse cursor is over the button
                        if (action.StartingPosition.X >= _pos.X
                            && action.StartingPosition.X <= _pos.X + ButtonVisual.Sprite.Width * _scale.X
                            && action.StartingPosition.Y >= _pos.Y
                            && action.StartingPosition.Y <= _pos.Y + ButtonVisual.Sprite.Height * _scale.Y)
                        {
                            Globals.HitUI = true;
                            InputManager.Instance._actions.Add(new ActionData(_buttonAction));
                            IsPressed = true;
                            
                            if (_buttonAction == GameAction.TOGGLE_ACTIVE)
                            {
                                ParentObject.ToggleGameObjectActiveState(GameObjectName);
                            }

                            if (_buttonAction == GameAction.TOGGLE_ACTIVE_PARENT)
                            {
                                ParentObject.ToggleParentActiveState();
                            }

                            if (_buttonAction == GameAction.CANCEL_MISSION_SELECT)
                            {
                                ParentObject.ToggleParentActiveState();
                                Globals.PickingManager.PlayerBuildingPickingActive = true;
                                Globals.PickingManager.PlayerBuildingBuiltPickingActive = true;
                            }


                            if (_isUnitSelect)
                            {
                                if (_buttonAction == GameAction.CONFIRM)
                                {
                                    int count = Convert.ToString(GameManager.UnitsSelectedForMission,2).ToCharArray().Count(c => c == '1');
                                    if (count < 2)
                                    {
                                        SelectAndDeselect(_buttonAction);
                                        _buttonAction = GameAction.DECLINE;
                                        GameManager.UnitsSelectedForMission += (byte)(1 << _unitID);
                                    }
                                }
                                else if (_buttonAction == GameAction.DECLINE)
                                {
                                    SelectAndDeselect(_buttonAction);
                                    _buttonAction = GameAction.CONFIRM;
                                    GameManager.UnitsSelectedForMission -= (byte)(1 << _unitID);
                                }
                            }
                            else
                            {
                                if (_buttonAction == GameAction.CONFIRM)
                                {
                                    SelectAndDeselect(_buttonAction);
                                    _buttonAction = GameAction.DECLINE;
                                }
                                else if (_buttonAction == GameAction.DECLINE)
                                {
                                    SelectAndDeselect(_buttonAction);
                                    _buttonAction = GameAction.CONFIRM;
                                }
                            }
                            
                        }
                    }
                    else
                    {
                        IsPressed = false;
                    }
                }
            }
            else
            {
                Initialize();
            }
        }
    }
    
    public void SelectAndDeselect(GameAction action)
    {
        string currentSpriteName = ButtonVisual.Sprite.Name;
        string newSpriteName;

        if (action == GameAction.CONFIRM)
        {
            if (!currentSpriteName.EndsWith("Selected"))
            {
                newSpriteName = currentSpriteName.Replace("Normal", "Selected");
                ButtonVisual.LoadSprite(newSpriteName);
            }
        }
        else if (action == GameAction.DECLINE)
        {
            if (currentSpriteName.EndsWith("Selected"))
            {
                newSpriteName = currentSpriteName.Replace("Selected", "Normal");
                ButtonVisual.LoadSprite(newSpriteName);
            }
        }
    }

    public override void Initialize()
    {
        ButtonVisual = ParentObject.GetComponent<SpiteRenderer>();
        if (ButtonVisual == null)
        {
            Active = false;
        }
    }

    public override string ComponentToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        
        builder.Append("<component>");
        
        builder.Append("<type>Button</type>");
        
        builder.Append("<active>" + Active +"</active>");
        
        builder.Append("<portrait>" + _unitPortrait +"</portrait>");
        
        builder.Append("<isUnitSelect>" + _isUnitSelect +"</isUnitSelect>");
        
        builder.Append("<unitID>" + _unitID +"</unitID>");
        
        builder.Append("<action>"+ _buttonAction +"</action>");

        if (_buttonAction == GameAction.TOGGLE_ACTIVE)
        {
            builder.Append("<linkedObject>"+ GameObjectName +"</linkedObject>");    
        }
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        _unitPortrait = element.Element("portrait")?.Value == "True";
        _isUnitSelect = element.Element("isUnitSelect")?.Value == "True";
        Enum.TryParse(element?.Element("action")?.Value, out GameAction action);
        _buttonAction = action;
        GameObjectName = element?.Element("linkedObject")?.Value;
        _unitID = int.TryParse(element.Element("unitID")?.Value, out int id) ? id : 0;
    }

    public override void RemoveComponent()
    {
        ParentObject.RemoveComponent(this);
    }
    
    #if DEBUG
    private bool changingFunction = false;
    public override void Inspect()
    {
        if(ImGui.CollapsingHeader("Button"))
        {
            ImGui.Checkbox("Button active", ref Active);
            ImGui.Checkbox("Unit portrait", ref _unitPortrait);
            ImGui.Checkbox("Unit selection", ref _isUnitSelect);
            ImGui.SliderInt("Unit ID", ref _unitID, 0, 2);
            ImGui.Text("Linked with SpiteRenderer from object: " + ButtonVisual?.ParentObject.Name);
            ImGui.Text("Current function: " + _buttonAction);
            if (ImGui.Button("Change button function"))
            {
                changingFunction = true;
            }

            if (_buttonAction == GameAction.TOGGLE_ACTIVE)
            {
                ImGui.InputText("GameObject name", ref GameObjectName, 100);
            }
            
            if (ImGui.Button("Remove component"))
            {
                RemoveComponent();
            }
        }

        if (changingFunction)
        {
            var values = Enum.GetValues(typeof(GameAction));
            ImGui.Begin("Change button function");
            foreach (GameAction action in values)
            {
                if (ImGui.Button(action.ToString()))
                {
                    _buttonAction = action;
                    changingFunction = false;
                }
            }
            if (ImGui.Button("Cancel"))
            {
                changingFunction = false;
            }
            ImGui.End();
        }
    }
    #endif
}