using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using ImGuiNET;

namespace RTS_Engine;

public class GameObject
{
    public string Name = "NewObject";
    public bool Active = true;
    private bool _wasActive;

    public Transform Transform;
    private List<Component> _components = new();

    public List<GameObject> Children {get; private set;} = new(); 
    public GameObject Parent {get; private set;}

    public GameObject()
    {
        Transform = new Transform(this);
        _wasActive = Active;
    }

    public void Update()
    {
        if (!Active && _wasActive)
        {
            SetChildrenInactive(this);
        } 
        else if (Active && !_wasActive)
        {
            SetChildrenActive(this);    
        }
        
        if (Active)
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
        
        _wasActive = Active;
    }
    
    public T GetComponent<T>() where T : Component
    {
        return (T)_components.Find(x => x.GetType() == typeof(T));
    }
    
    public bool HasComponent<T>() where T : Component
    {
        return _components.Any(x => x.GetType() == typeof(T));
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
        component.ParentObject = this;
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
        ClearObject(gameObject);
        gameObject.Parent = null; 
    }

    public static void ClearObject(GameObject gameObject)
    {
        gameObject.Parent.Children.Remove(gameObject);
        for (int i = gameObject.Children.Count - 1; i >= 0 ; i--)
        {
            ClearObject(gameObject.Children[i]);
        }
        for (int i = gameObject._components.Count - 1; i >= 0 ; i--)
        {
            gameObject._components[i].RemoveComponent();
        }
        gameObject.Transform = null;
    }
    
    private void SetChildrenInactive(GameObject parent)
    {
        foreach (var child in parent.Children)
        {
            child.Active = false;
            SetChildrenInactive(child);
        }
    }
    
    private void SetChildrenActive(GameObject parent)
    {
        foreach (var child in parent.Children)
        {
            child.Active = true;
            SetChildrenActive(child);
        }
    }
    
    public GameObject FindGameObjectByName(string name)
    {
        if (Name == name) return this;
        
        foreach (var child in Children)
        {
            GameObject result = child.FindGameObjectByName(name);
            if (result != null) return result;
        }

        return null;
    }
    
    public void ToggleGameObjectActiveState(string name)
    {
        GameObject gameObject = FindGameObjectByName(name);
        
        if (gameObject != null)
        {
            gameObject.Active = !gameObject.Active;
        }
    }
    
    public void ToggleParentActiveState()
    {
        Parent.Active = !Parent.Active;
        
        if (Parent.Active)
        {
            SetChildrenActive(Parent);
        }
        else
        {
            SetChildrenInactive(Parent);
        }
    }

    public string SaveSceneToXml()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("<rootObject>");
        builder.Append("<name>" + Name + "</name>");
        builder.Append("<active>" + Active +"</active>");
        builder.Append("<components>");
        builder.Append(Transform.ComponentToXmlString());
        foreach (Component c in _components)
        {
            builder.Append(c.ComponentToXmlString());
        }
        builder.Append("</components>");
        builder.Append("<childObjects>");
        foreach (GameObject gameObject in Children)
        {
            builder.Append(gameObject.ObjectToXmlString());
        }
        builder.Append("</childObjects>");
        builder.Append("</rootObject>");
        return builder.ToString();
    }

    private string ObjectToXmlString()
    {
        StringBuilder builder = new StringBuilder();
        builder.Append("<object>");
        builder.Append("<name>" + Name + "</name>");
        builder.Append("<active>" + Active +"</active>");
        builder.Append("<components>");
        builder.Append(Transform.ComponentToXmlString());
        foreach (Component c in _components)
        {
            builder.Append(c.ComponentToXmlString());
        }
        builder.Append("</components>");
        builder.Append("<childObjects>");
        foreach (GameObject gameObject in Children)
        {
            builder.Append(gameObject.ObjectToXmlString());
        }
        builder.Append("</childObjects>");
        builder.Append("</object>");
        return builder.ToString();
    }

    public void LoadPrefab(string name)
    {
        AddChildObject(FileManager.DeserializeScene(name));
    }

    public static GameObject GetPrefab(string name)
    {
        return FileManager.DeserializeScene(name);
    }
    
#if DEBUG
    private bool addingOpen = false;
    private bool savingPrefab = false;
    private bool loadPrefab = false;
    private string prefabName;
    public void DrawTree()
    {
        ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.OpenOnArrow;
        if(Children.Count == 0)
        {
            flags |= ImGuiTreeNodeFlags.Leaf;
        }
        if(this == Globals.CurrentlySelectedObject)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }
        string _name = "#";
        if(this.Name != "")
        {
            _name = this.Name; 
        }
        bool node_open = ImGui.TreeNodeEx(_name, flags);
        if(ImGui.IsItemClicked() && !ImGui.IsItemToggledOpen()) Globals.CurrentlySelectedObject = this;
        if(node_open)
        {
            foreach(GameObject g in Children)
            {
                g.DrawTree();
            }
            ImGui.TreePop();
        }
    }
    private void SaveAsPrefab(string name)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(SaveSceneToXml());
        XDocument prefab = XDocument.Parse(builder.ToString());
#if _WINDOWS
        StreamWriter streamWriter = new StreamWriter(Globals.MainPath + "/Prefabs/" + name + ".xml");
#else
        StreamWriter streamWriter = new StreamWriter(Globals.MainPath + "Prefabs/" + name + ".xml");
#endif
        prefab.Save(streamWriter);
        streamWriter.Close();
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
                if(Globals.CurrentlySelectedObject == this)
                {
                    Globals.CurrentlySelectedObject = null;
                }
            }
        }
        if(ImGui.Button("Add child object"))
        {
            AddChildObject(new GameObject());
        }
        if(ImGui.Button("Save as prefab"))
        {
                prefabName = Name;
                savingPrefab = true;
        }
        if(ImGui.Button("Add prefab as child"))
        {
            Globals.UpdatePrefabList();
            loadPrefab = true;
        }
        ImGui.Text("");
        Transform.Inspect();
        for(int i = _components.Count  - 1;i >= 0;i--)
        {
            _components[i].Inspect();
        }
        ImGui.Separator();
        if(ImGui.Button("Add component"))
        {
            addingOpen = true;
        }
        ImGui.End();

        if (savingPrefab)
        {
            ImGui.Begin("Save prefab");
            ImGui.InputText("Prefab name", ref prefabName, 20);
            if (ImGui.Button("Save"))
            {
                SaveAsPrefab(prefabName);
                savingPrefab = false;
            }
            if (ImGui.Button("Cancel"))
            {
                savingPrefab = false;
            }
            ImGui.End();
        }

        if (loadPrefab)
        {
            ImGui.Begin("Load and add prefab");
            foreach(string path in Globals.AvailablePrefabs)
            {
                if (ImGui.Button(path))
                {
                    LoadPrefab(path);
                    loadPrefab = false;
                }
            }

            if (ImGui.Button("Cancel"))
            {
                loadPrefab = false;
            }
            ImGui.End();
        }

        if (addingOpen)
        {
            ImGui.Begin("Add component");
            var method = typeof(GameObject).GetMethod("AddComponent",Type.EmptyTypes);
            foreach(Type t in Globals.ComponentsTypes)
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