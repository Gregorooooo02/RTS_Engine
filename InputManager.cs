using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace RTS_Engine;

public enum GameAction
{
    FORWARD,
    BACKWARD,
    LEFT,
    RIGHT,
    EXIT
}

public class InputManager
{
    //Singleton part
    public static InputManager Instance;
    public static void Initialize()
    {
        Instance = new InputManager();
    }
    private InputManager()
    {
        RawToAction = new Dictionary<Keys, GameAction>();
        Actions = new List<ActionData>();
        PopulateDict();
    }
    
    //Other
    private List<ActionData> Actions;
    private Dictionary<Keys, GameAction> RawToAction;

    public void PopulateDict()
    {
        //open config file here and fill dictionary with it
        RawToAction.Clear();
        RawToAction.Add(Keys.W,GameAction.FORWARD);
        RawToAction.Add(Keys.S,GameAction.BACKWARD);
        RawToAction.Add(Keys.A,GameAction.LEFT);
        RawToAction.Add(Keys.D,GameAction.RIGHT);
        RawToAction.Add(Keys.Escape,GameAction.EXIT);
    }

    public void PollInput()
    {
        //Poll current frame inputs
        Keys[] input = Keyboard.GetState().GetPressedKeys();
        List<GameAction> currentActions = new List<GameAction>();
        foreach (Keys k in input)
        {
            if (RawToAction.TryGetValue(k, out GameAction action))
            {
                ActionData a = Actions.Find(x => x.action == action);
                if (a != null)
                {
                    a.UpdateAction();
                }
                else
                {
                    Actions.Add(new ActionData(action));
                }
                currentActions.Add(action);
            }
        }

        //Update no longer active actions
        for (int i = Actions.Count - 1;i >= 0;i--)
        {
            ActionData a = Actions[i];
            if (!currentActions.Contains(a.action))
            {
                if (a.state == ActionState.PRESSED)
                {
                    a.UpdateAction(false);
                }
                else
                {
                    Actions.Remove(a);
                }
            }
        }
    }

    public bool IsActive(GameAction action)
    {
        return GetAction(action) != null;
    }

    public ActionData GetAction(GameAction action)
    {
        return Actions.Find(x => x.action == action);
    }
}