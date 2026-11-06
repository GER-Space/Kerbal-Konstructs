using System.Collections.Generic;
using UnityEngine;

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

        internal static void ResetAll()
        {
            List<CelestialBody> dirtyBodies = new List<CelestialBody>();
            foreach (var mapdecal in allMapDecalInstances)
            {
                mapdecal.gameObject.transform.parent = null;
                mapdecal.mapDecal.transform.parent = null;
                mapdecal.gameObject.DestroyGameObject();
                if (!dirtyBodies.Contains(mapdecal.CelestialBody))
                {
                    dirtyBodies.Add(mapdecal.CelestialBody);
                }
            }
            foreach (CelestialBody body in dirtyBodies)
            {
                body.pqsController.RebuildSphere();
            }
            heightMapList.Clear();
            colorMapList.Clear();
            allHeightMaps.Clear();
            allColorMaps.Clear();
            _allMapDecalInstances.Clear();
            allMapDecalInstances = new MapDecalInstance[0];

        }

        public static MapDecalInstance[] GetAllMapDecalInstances()
        {
            return allMapDecalInstances;
        }

        internal static void RegisterMap(MapDecalsMap decalMap)
        {
            if (decalMap.isHeightMap)
            {

                if (heightMapList.ContainsKey(decalMap.Name))
                {
                    Log.UserInfo("duplicate DecalMap name: " + decalMap.Name + " ,found");
                    return;
                }
                else
                {
                    heightMapList.Add(decalMap.Name, decalMap);
                    allHeightMaps.Add(decalMap);
                }
            }
            else
            {

                if (colorMapList.ContainsKey(decalMap.Name))
                {
                    Log.UserInfo("duplicate DecalMap name: " + decalMap.Name + " ,found");
                    return;
                }
                else
                {
                    colorMapList.Add(decalMap.Name, decalMap);
                    allColorMaps.Add(decalMap);
                }
            }


        }

        internal static MapDecalsMap GetHeightMapByName(string name)
        {
            if (!heightMapList.ContainsKey(name))
            {
                Log.UserWarning("No HeightMap found with name: " + name);
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

        internal static GroupCenter GetCloesedCenter(Vector3 myPosition)
        {
            if (StaticDatabase.allGroupCenters.Length == 0)
            {
                return null;
            }

            GroupCenter closest = StaticDatabase.allGroupCenters[0];
            float dist = Vector3.Distance(myPosition, closest.gameObject.transform.position);
            foreach (GroupCenter center in StaticDatabase.allGroupCenters)
            {
                if (Vector3.Distance(myPosition, center.gameObject.transform.position) < dist)
                {
                    dist = Vector3.Distance(myPosition, center.gameObject.transform.position);
                    closest = center;
                }
            }

            return closest;
        }

    }
}
