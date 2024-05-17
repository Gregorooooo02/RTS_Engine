using System;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class Button : Component
{
    public SpiteRenderer ButtonVisual = null;
    private GameAction _buttonAction = GameAction.EXIT;
    public override void Update()
    {
        if (Active)
        {
            if (ButtonVisual != null)
            {
                if (ButtonVisual.Active)
                {
                    MouseAction action = InputManager.Instance.GetMouseAction(GameAction.LMB);
                    //Check if the LMB was pressed
                    if (action is {state: ActionState.PRESSED, duration: <= 0})
                    {
                        //Check if mouse cursor is over the button
                        if (action.StartingPosition.X >= ParentObject.Transform._pos.X
                            && action.StartingPosition.X <= ParentObject.Transform._pos.X + ButtonVisual.Sprite.Width * ParentObject.Transform._scl.X
                            && action.StartingPosition.Y >= ParentObject.Transform._pos.Y
                            && action.StartingPosition.Y <= ParentObject.Transform._pos.Y + ButtonVisual.Sprite.Height * ParentObject.Transform._scl.Y)
                        {
                            InputManager.Instance._actions.Add(new ActionData(_buttonAction));
                            Globals.HitUI = true;
                        }
                    }
                }
            }
            else
            {
                Initialize();
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
        
        builder.Append("<action>"+ _buttonAction +"</action>");
        
        builder.Append("</component>");
        return builder.ToString();
    }

    public override void Deserialize(XElement element)
    {
        Active = element.Element("active")?.Value == "True";
        Enum.TryParse(element?.Element("action")?.Value, out GameAction action);
        _buttonAction = action;
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
            ImGui.Text("Linked with SpiteRenderer from object: " + ButtonVisual?.ParentObject.Name);
            ImGui.Text("Current function: " + _buttonAction);
            if (ImGui.Button("Change button function"))
            {
                changingFunction = true;
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