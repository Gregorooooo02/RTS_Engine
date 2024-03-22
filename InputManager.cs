using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RTS_Engine;

public enum GameAction
{
    FORWARD,
    BACKWARD,
    LEFT,
    RIGHT,
    EXIT,
    LMB,
    MMB,
    RMB
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
        _rawKeyboardToAction = new Dictionary<Keys, GameAction>();
        _rawMouseToAction = new Dictionary<int, GameAction>();
        _actions = new List<ActionData>();
        _mouseActions = new List<MouseAction>();
        PopulateDict();
    }
    
    //Other
    public Point MousePosition;
    
    private List<ActionData> _actions;
    private List<MouseAction> _mouseActions;
    private Dictionary<Keys, GameAction> _rawKeyboardToAction;
    private Dictionary<int, GameAction> _rawMouseToAction;

    public void PopulateDict()
    {
        //open config file here and fill dictionary with it
        _rawKeyboardToAction.Clear();
        _rawKeyboardToAction.Add(Keys.W,GameAction.FORWARD);
        _rawKeyboardToAction.Add(Keys.S,GameAction.BACKWARD);
        _rawKeyboardToAction.Add(Keys.A,GameAction.LEFT);
        _rawKeyboardToAction.Add(Keys.D,GameAction.RIGHT);
        _rawKeyboardToAction.Add(Keys.Escape,GameAction.EXIT);
        
        _rawMouseToAction.Clear();
        _rawMouseToAction.Add(0,GameAction.LMB);
        _rawMouseToAction.Add(1,GameAction.MMB);
        _rawMouseToAction.Add(2,GameAction.RMB);
    }

    public void PollInput()
    {
        //Poll current mouse position
        MousePosition = Mouse.GetState().Position;
        
        //Poll current frame keyboard inputs
        Keys[] input = Keyboard.GetState().GetPressedKeys();
        List<GameAction> currentActions = new List<GameAction>();
        foreach (Keys k in input)
        {
            if (_rawKeyboardToAction.TryGetValue(k, out GameAction action))
            {
                ActionData a = _actions.Find(x => x.action == action);
                if (a != null)
                {
                    a.UpdateAction();
                }
                else
                {
                    _actions.Add(new ActionData(action));
                }
                currentActions.Add(action);
            }
        }

        //Poll current frame mouse inputs
        MouseState mouse = Mouse.GetState();
        if (mouse.LeftButton == ButtonState.Pressed)
        {
            if (_rawMouseToAction.TryGetValue(0, out GameAction action))
            {
                ActionData a = _mouseActions.Find(x => x.action == action);
                if (a != null)
                {
                    a.UpdateAction();
                }
                else
                {
                    _mouseActions.Add(new MouseAction(action));
                }
                currentActions.Add(action);
            }
        }
        if (mouse.MiddleButton == ButtonState.Pressed)
        {
            if (_rawMouseToAction.TryGetValue(1, out GameAction action))
            {
                ActionData a = _mouseActions.Find(x => x.action == action);
                if (a != null)
                {
                    a.UpdateAction();
                }
                else
                {
                    _mouseActions.Add(new MouseAction(action));
                }
                currentActions.Add(action);
            }
        }
        if (mouse.RightButton == ButtonState.Pressed)
        {
            if (_rawMouseToAction.TryGetValue(2, out GameAction action))
            {
                ActionData a = _mouseActions.Find(x => x.action == action);
                if (a != null)
                {
                    a.UpdateAction();
                }
                else
                {
                    _mouseActions.Add(new MouseAction(action));
                }
                currentActions.Add(action);
            }
        }
        

        //Update no longer active keyboard actions
        for (int i = _actions.Count - 1;i >= 0;i--)
        {
            ActionData a = _actions[i];
            if (!currentActions.Contains(a.action))
            {
                if (a.state == ActionState.PRESSED)
                {
                    a.UpdateAction(false);
                }
                else
                {
                    _actions.Remove(a);
                }
            }
        }
        
        //Update no longer active mouse actions
        for (int i = _mouseActions.Count - 1;i >= 0;i--)
        {
            MouseAction a = _mouseActions[i];
            if (!currentActions.Contains(a.action))
            {
                if (a.state == ActionState.PRESSED)
                {
                    a.UpdateAction(false);
                }
                else
                {
                    _mouseActions.Remove(a);
                }
            }
        }
        
    }

    public bool IsActive(GameAction action)
    {
        return GetAction(action) != null || GetMouseAction(action) != null;
    }

    public ActionData GetAction(GameAction action)
    {
        return _actions.Find(x => x.action == action);
    }

    public MouseAction GetMouseAction(GameAction action)
    {
        return _mouseActions.Find(x => x.action == action);
    }
}