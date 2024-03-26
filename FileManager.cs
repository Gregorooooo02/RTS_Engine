using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RTS_Engine;

public class FileManager
{
    public static FileManager Instance;
    public static void Initialize()
    {
        Instance = new FileManager();
    }
    
    public List<Keys> KeyboardKeys;
    public List<int> MouseKeys;
    
    public List<GameAction> KeyboardActions;
    
    private FileManager()
    {
        KeyboardKeys = new List<Keys>();
        KeyboardActions = new List<GameAction>();
        
        LoadKeyBindings();
    }

    private void LoadKeyBindings()
    {
        XDocument doc = XDocument.Load("ConfigFiles/KeyBindings.xml");

        if (doc != null)
        {
            List<XElement> keyBindings = (
                from element in doc.Element("Root").Element("KeyBindings").Descendants("KeyBinding") select element
            ).ToList();
            
            foreach (XElement keyBinding in keyBindings)
            {
                string key = keyBinding.Element("Key").Value;
                string action = keyBinding.Element("Action").Value;
                
                if (Enum.TryParse(key, out Keys k) && Enum.TryParse(action, out GameAction a))
                {
                    KeyboardKeys.Add(k);
                    KeyboardActions.Add(a);
                }
            }
        }
    }
    
}