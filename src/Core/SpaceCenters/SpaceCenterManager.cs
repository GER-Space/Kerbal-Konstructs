using System;
using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.Core
{
    public class SpaceCenterManager
    {
        public static List<CustomSpaceCenter> spaceCenters = new List<CustomSpaceCenter>();
        public static SpaceCenter KSC;

        public static void setKSC()
        {
            KSC = SpaceCenter.Instance;
        }

        public static void AddSpaceCenter(CustomSpaceCenter csc)
        {
            spaceCenters.Add(csc);
        }

        public static void RemoveSpaceCenter(CustomSpaceCenter csc)
        {
            spaceCenters.Remove(csc);
        }

        internal static CustomSpaceCenter GetCSC(string name)
        {
            foreach (var csc in spaceCenters)
            {
                if (csc.SpaceCenterName == name)
                    return csc;
            }
            return null;
        }
    }
}