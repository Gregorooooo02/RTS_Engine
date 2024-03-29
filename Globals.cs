﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RTS_Engine
{
    internal class Globals
    {
        public static Globals Instance;
        public static void Initialize(ContentManager content) 
        {
            Instance = new Globals();
            Instance.defaultModel = content.Load<Model>("defaultCube");
        }

        public Model defaultModel;

        private Globals()
        {
#if DEBUG
            ComponentsTypes = GetAllComponents();
#endif
        }

#if DEBUG
        private List<Type> GetAllComponents()
        {
            Type baseType = typeof(Component);
            Type transform = typeof(Transform);
            Assembly assembly = Assembly.GetExecutingAssembly();
            return assembly.GetTypes().Where(x => baseType.IsAssignableFrom(x) && x != baseType && x != transform).ToList();
        }
        
        public List<Type> ComponentsTypes;
        public GameObject CurrentlySelectedObject;

        //Switches for debug windows UWU
        public bool InspectorVisible = true;
        public bool HierarchyVisible = true;
#endif
    }
}
