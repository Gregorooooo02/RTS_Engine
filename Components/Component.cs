using System;

namespace RTS_Engine;

public abstract class Component
{
    public GameObject ParentObject;
    
    public bool IsType(Type type)
    {
        return GetType() == type;
    }
    public abstract void Update();
}