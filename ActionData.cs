namespace RTS_Engine;

public enum ActionState
{
    PRESSED,
    RELEASED
}

public class ActionData
{
    public ActionState state;
    public int duration;
    public GameAction action;

    public ActionData(GameAction action)
    {
        duration = 0;
        state = ActionState.PRESSED;
        this.action = action;
    }

    public void UpdateAction(bool isPressed = true)
    {
        if (isPressed)
        {
            duration++;
        }
        else
        {
            state = ActionState.RELEASED;
        }
    }
}