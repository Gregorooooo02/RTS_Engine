using System;
using System.Collections.Generic;
using System.Linq;

namespace RTS_Engine;

public class GameObject
{
    public Transform Transform;
    private List<Component> _components = new();
    
    public List<GameObject> Children = new();
    public GameObject Parent;

    public GameObject()
    {
        Transform = new Transform(this);
    }

    public void Update()
    {
        Transform.Update();
        //Update all components
        foreach (Component c in _components)
        {
            c.Update();
        }
        
        //Update all children
        foreach (GameObject gameObject in Children)
        {
            gameObject.Update();
        }
    }

    public void Draw()
    {
        Transform.Draw();
        //'Draw' all components
        foreach(Component c in _components)
        {
            c.Draw();
        }

        //Propegate throuth all children
        foreach (GameObject gameObject in Children)
        {
            gameObject.Draw();
        }
    }

    public T GetComponent<T>() where T : Component
    {
        return (T)_components.Find(x => x.GetType() == typeof(T));
    }
    
    
    public void AddComponent<T>() where T : Component,new()
    {
        if(typeof(T) == typeof(Transform))return;
        
        T component = new T();
        component.ParentObject = this;
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
        if(component.GetType() == typeof(Transform))return;
        _components.Add(component);
    }

    public bool RemoveComponent(Component component)
    {
        return _components.Remove(component);
    }
}