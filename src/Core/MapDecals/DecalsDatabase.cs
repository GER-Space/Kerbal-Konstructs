using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.Core
{
    public static class DecalsDatabase
    {
        private static Dictionary<string, MapDecalsMap> heightMapList = new Dictionary<string, MapDecalsMap>();
        private static Dictionary<string, MapDecalsMap> colorMapList = new Dictionary<string, MapDecalsMap>();

        internal static List<MapDecalsMap> allHeightMaps = new List<MapDecalsMap>();
        internal static List<MapDecalsMap> allColorMaps = new List<MapDecalsMap>();


        //make the list private, so nobody does easily add or remove from it. the array is updated in the Add and Remove functions
        // arrays are always optimized (also in foreach loops)
        private static List<MapDecalInstance> _allMapDecalInstances = new List<MapDecalInstance>();
        internal static MapDecalInstance[] allMapDecalInstances = new MapDecalInstance[0];


        /// <summary>
        /// Adds the Instance to the instances and Group lists. Also sets the PQSCity.name
        /// </summary>
        /// <param name="instance"></param>
        internal static void RegisterMapDecalInstance(MapDecalInstance instance)
        {
            _allMapDecalInstances.Add(instance);
            allMapDecalInstances = _allMapDecalInstances.ToArray();

        }


        /// <summary>
        /// Removes a Instance from the group and instance lists.
        /// </summary>
        /// <param name="instance"></param>
        internal static void DeleteMapDecalInstance(MapDecalInstance instance)
        {
            if (_allMapDecalInstances.Contains(instance))
            {
                _allMapDecalInstances.Remove(instance);
                allMapDecalInstances = _allMapDecalInstances.ToArray();
                Log.Debug("MapDecal instace " + instance.Name + " removed from Database");
            }

        }

        public static MapDecalInstance[] GetAllMapDecalInstances()
        {
            return allMapDecalInstances;
        }

        internal static void RegisterMap(MapDecalsMap decalMap)
        {
            if (decalMap.isHeightMap)
            {

                if (heightMapList.ContainsKey(decalMap.name))
                {
                    Log.UserInfo("duplicate DecalMap name: " + decalMap.name + " ,found");
                    return;
                }
                else
                {
                    heightMapList.Add(decalMap.name, decalMap);
                    allHeightMaps.Add(decalMap);
                }
            }
            else
            {

                if (colorMapList.ContainsKey(decalMap.name))
                {
                    Log.UserInfo("duplicate DecalMap name: " + decalMap.name + " ,found");
                    return;
                }
                else
                {
                    colorMapList.Add(decalMap.name, decalMap);
                    allColorMaps.Add(decalMap);
                }
            }


        }

        internal static MapDecalsMap GetHeightMapByName(string name)
        {
            if (!heightMapList.ContainsKey(name))
            {
                Log.UserError("No HeightMap found with name: " + name);
                return null;
            }
            else
            {
                return heightMapList[name];
            }
        }

        internal static MapDecalsMap GetColorMapByName(string name)
        {
            if (!colorMapList.ContainsKey(name))
            {
                Log.UserError("No ColorMap found with name: " + name);
                return null;
            }
            else
            {
                return colorMapList[name];
            }
        }


    }
}
