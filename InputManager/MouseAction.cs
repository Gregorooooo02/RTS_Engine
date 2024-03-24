using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RTS_Engine;

public class MouseAction : ActionData
{
    public Point StartingPosition;
    
    public MouseAction(GameAction action) : base(action)
    {
        StartingPosition = Mouse.GetState().Position;
    }

    public override void UpdateAction(bool isPressed = true)
    {
        base.UpdateAction(isPressed);
    }
}