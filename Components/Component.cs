using System;
using System.Xml.Linq;

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
    public abstract void Initialize();

    public abstract string ComponentToXmlString();
    public abstract void Deserialize(XElement element);
    public abstract void RemoveComponent();

#if DEBUG
     public abstract void Inspect();
#endif
}