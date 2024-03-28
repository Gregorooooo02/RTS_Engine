using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTS_Engine
{
    internal class Globals
    {
        public static Globals Instance;
        public static void Initialize() { Instance = new Globals(); }


#if DEBUG
        public GameObject CurrentlySelectedObject;

        //Switches for debug windows UWU
        public bool InspectorVisible = true;
        public bool HierarchyVisible = true;
#endif
    }
}
