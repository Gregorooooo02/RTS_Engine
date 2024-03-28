using System;

namespace RTS_Engine;

public abstract class Component
{
    public GameObject ParentObject;
    public bool Active = true;
    
    public bool IsType(Type type)
    {
        return GetType() == type;
    }
    public abstract void Update();

    public abstract void Draw();
    public abstract void Initialize();

#if DEBUG
     public abstract void Inspect();
#endif
}