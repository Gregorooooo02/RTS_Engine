using System;
using System.Collections.Generic;

namespace RTS_Engine;

public class GameObject
{
    private List<Component> components;
    
    private List<GameObject> _children;
    private GameObject _parent;

    public Component GetComponent(Type type)
    {
        throw new System.NotImplementedException();
    }
    public void Update()
    {
        //Update all components
        foreach (Component c in components)
        {
            c.Update();
        }
        
        //Update all children
        foreach (GameObject gameObject in _children)
        {
            gameObject.Update();
        }
        
    }
    
}