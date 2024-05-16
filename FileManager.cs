using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework.Input;

namespace RTS_Engine;

public class FileManager
{

#region KeyBinds
public class KeyBindsData
    {
        public List<Keys> KeyboardKeys;
        public List<int> MouseKeys;
    
        public List<GameAction> KeyboardActions;
        public List<GameAction> MouseActions;

        public KeyBindsData()
        {
            KeyboardKeys = new List<Keys>();
            MouseKeys = new List<int>();
            KeyboardActions = new List<GameAction>();
            MouseActions = new List<GameAction>();
        }
    }
    
    public static KeyBindsData LoadKeyBindings()
    {
        XDocument doc = XDocument.Load("ConfigFiles/KeyBindings.xml");
        KeyBindsData output = new KeyBindsData();
        if (doc != null)
        {
            List<XElement> keyBindings = (
                from element in doc.Element("Root")?.Element("Keyboard")?.Descendants("KeyBinding") select element
            ).ToList();
            
            foreach (XElement keyBinding in keyBindings)
            {
                string key = keyBinding.Element("Key")?.Value;
                string action = keyBinding.Element("Action")?.Value;
                
                if (Enum.TryParse(key, out Keys k) && Enum.TryParse(action, out GameAction a))
                {
                    output.KeyboardKeys.Add(k);
                    output.KeyboardActions.Add(a);
                }
            }
            
            List<XElement> mouseBindings = (
                from element in doc.Element("Root")?.Element("Mouse")?.Descendants("MouseBinding") select element
            ).ToList();
            
            foreach (XElement mouseBinding in mouseBindings)
            {
                string key = mouseBinding.Element("Key")?.Value;
                string action = mouseBinding.Element("Action")?.Value;

                if (int.TryParse(key, out int k) && Enum.TryParse(action, out GameAction a))
                {
                    output.MouseKeys.Add(k);
                    output.MouseActions.Add(a);
                }
            }
        }
        return output;
    }
#endregion

#region SceneDeserialization
    public static Scene PopulateScene(string name)
    {
        Scene loadedScene = new LoadedScene();
        loadedScene.Name = name;
        loadedScene.TempName = name;
        loadedScene.SceneRoot = DeserializeScene(Globals.MainPath + "Scenes/" + name + ".xml");
        return loadedScene;
    }

    public static GameObject DeserializeScene(string filePath)
    {
        XDocument scene;
        try
        {
             scene = XDocument.Load(filePath);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return DeserializeObject(scene.Element("rootObject"));
    }

    private static GameObject DeserializeObject(XElement objectNode)
    {
        GameObject currentObject = new GameObject();
        currentObject.Name = objectNode.Element("name").Value;
        currentObject.Active = objectNode.Element("active").Value == "True";
        foreach (XElement component in objectNode.Element("components").Elements())
        {
            if (component.Element("type")?.Value == "Transform")
            {
                currentObject.Transform.Deserialize(component);
            }
            else
            {
                Component newComponent = null;
                Type t = Globals.ComponentsTypes.Find(x => x.Name.Equals(component.Element("type")?.Value));
                if (t != null)
                {
                    newComponent = (Component)Activator.CreateInstance(t);
                }
            
                if (newComponent != null)
                {
                    newComponent.Deserialize(component);
                    currentObject.AddComponent(newComponent);
                }
            }
        }
        foreach (XElement childObject in objectNode.Element("childObjects").Elements())
        {
            currentObject.AddChildObject(DeserializeObject(childObject));
        }
        return currentObject;
    }
#endregion
    
    public static FileManager Instance;
    public static void Initialize()
    {
        Instance = new FileManager();
    }

    private FileManager()
    {

    }
}