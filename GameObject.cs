using System;
using System.Collections.Generic;
using System.Linq;

namespace RTS_Engine;

public class GameObject
{
    private List<Component> _components = new();
    
    private List<GameObject> _children = new();
    private GameObject _parent;

    public void Update()
    {
        //Update all components
        foreach (Component c in _components)
        {
            c.Update();
        }
        
        //Update all children
        foreach (GameObject gameObject in _children)
        {
            gameObject.Update();
        }
    }
    
    public T GetComponent<T>() where T : Component
    {
        return (T)_components.Find(x => x.GetType() == typeof(T));
    }
    
    
    public void AddComponent<T>() where T : Component,new()
    {
        T component = new T
        {
            ParentObject = this
        };
        component.Initialize();
        _components.Add(component);
    }
    
    public void RemoveFirstComponentOfType<T>() where T : Component
    {
        _components.Remove(_components.FirstOrDefault(x => x.GetType() == typeof(T)));
    }
    
    public Component GetComponent(Type type)
    {
        return _components.Find(x => x.GetType() == type);
    }
    public void AddComponent(Component component)
    {
        _components.Add(component);
    }

    public bool RemoveComponent(Component component)
    {
        return _components.Remove(component);
    }
}