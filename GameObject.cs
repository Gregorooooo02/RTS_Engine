using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class GameObject
{
    public string Name = "NewObject";
    public bool Active = true;

    public Transform Transform;
    private List<Component> _components = new();

    public List<GameObject> Children {get; private set;} = new(); 
    public GameObject Parent {get; private set;}

    public GameObject()
    {
        Transform = new Transform(this);
    }

    public void Update()
    {
        if (!Active) return;
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
        if(!Active) return;
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

    public void AddChildObject(GameObject gameObject)
    {
        Children.Add(gameObject);
        gameObject.Parent = this;
    }

    public void RemoveChildObject(GameObject gameObject)
    {
        Children.Remove(gameObject);
        gameObject.Parent = null; 
    }

#if DEBUG
    private bool addingOpen = false;
    public void DrawTree()
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
        if(Children.Count == 0)
        {
            flags |= ImGuiTreeNodeFlags.Leaf;
        }
        if(this == Globals.Instance.CurrentlySelectedObject)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }
        string _name = "#";
        if(this.Name != "")
        {
            _name = this.Name; 
        }
        bool node_open = ImGui.TreeNodeEx(_name, flags);
        if(ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen()) Globals.Instance.CurrentlySelectedObject = this;
        if(node_open)
        {
            foreach(GameObject g in Children)
            {
                g.DrawTree();
            }
            ImGui.TreePop();
        }
    }

    public void DrawInspector()
    {
        ImGui.Begin("Inspector");
        ImGui.Checkbox("Active", ref Active);
        ImGui.InputText("Object name", ref Name, 20);
        if(ImGui.Button("Delete GameObject"))
        {
            if(Parent != null)
            {
                Parent.RemoveChildObject(this);
                if(Globals.Instance.CurrentlySelectedObject == this)
                {
                    Globals.Instance.CurrentlySelectedObject = null;
                }
            }
        }
        if(ImGui.Button("Add child object"))
        {
            AddChildObject(new GameObject());
        }
        ImGui.Text("");
        Transform.Inspect();
        for(int i = _components.Count  - 1;i >= 0;i--)
        {
            _components[i].Inspect();
        }
        if(ImGui.Button("Add component"))
        {
            addingOpen = true;
        }
        ImGui.End();

        if (addingOpen)
        {
            ImGui.Begin("Add component");
            var method = typeof(GameObject).GetMethod("AddComponent",Type.EmptyTypes);
            foreach(Type t in Globals.Instance.ComponentsTypes)
            {
                if (ImGui.Button(t.Name))
                {
                    var kek = method.MakeGenericMethod(t);
                    kek.Invoke(this, null);
                    addingOpen = false;
                }
            }
            if (ImGui.Button("Cancel"))
            {
                addingOpen = false;
            }
            ImGui.End();
        }
    }
#endif
}