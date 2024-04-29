using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

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

    public abstract string ComponentToXmlString();
    public abstract void Deserialize(XElement element);

#if DEBUG
     public abstract void Inspect();
#endif
}