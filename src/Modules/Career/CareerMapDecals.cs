using KerbalKonstructs.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs.Modules.Career
{
    public class CareerMapDecals
    {
        private static string decalCFGNodeName = "KKMapDecals";

        public static void SaveDecals(ConfigNode cfgNode)
        {
            // See ConfigParser.SaveMapDecalInstance

            if (cfgNode.HasNode(decalCFGNodeName)) cfgNode.RemoveNode(decalCFGNodeName);

            ConfigNode decalNode = cfgNode.AddNode(decalCFGNodeName);

            DecalsDatabase.allMapDecalInstances.Where(d => d.isInSavegame).ToList().ForEach(decal =>
            {
                Debug.Log($"Saving decal {decal.Name} in savegame");

                ConfigNode instanceNode = decalNode.AddNode("MapDecalInstance");
                ConfigParser.WriteMapDecalConfig(decal, instanceNode);
            });
        }

        public static void LoadDecals(ConfigNode cfgNode)
        {
            RemoveAllDecals();

            if (!cfgNode.HasNode(decalCFGNodeName)) return;

            ConfigNode decalNode = cfgNode.GetNode(decalCFGNodeName);

            foreach (ConfigNode instanceNode in decalNode.GetNodes())
            {
                LoadDecal(instanceNode);
            }
        }

        public static void RemoveAllDecals()
        {
            List<MapDecalInstance> decals = DecalsDatabase.allMapDecalInstances.Where(d => d.isInSavegame).ToList();

            List<CelestialBody> changedBodies = new List<CelestialBody>();

            decals.ForEach(d =>
            {
                d.gameObject.transform.parent = null;
                d.mapDecal.transform.parent = null;
                d.gameObject.DestroyGameObject();
                DecalsDatabase.DeleteMapDecalInstance(d);

                if (!changedBodies.Contains(d.CelestialBody)) changedBodies.Add(d.CelestialBody);
            });

            changedBodies.ForEach(b => b.pqsController.RebuildSphere());
        }

        public static void LoadDecal(ConfigNode cfgNode)
        {
            MapDecalInstance newMapDecalInstance = new MapDecalInstance();
            newMapDecalInstance.isInSavegame = true;

            ConfigParser.ParseMapDecalConfig(newMapDecalInstance, cfgNode);

            HashSet<CelestialBody> bodies2Update = new HashSet<CelestialBody>();

            // remove all instances where no planet was assigned
            foreach (MapDecalInstance instance in DecalsDatabase.allMapDecalInstances)
            {
                if (instance.CelestialBody == null)
                {
                    Log.Normal("No valid CelestialBody found: removing MapDecal instance " + instance.configPath);
                    DecalsDatabase.DeleteMapDecalInstance(instance);
                    continue;
                }
                else
                {
                    //Log.Normal("Loaded MapDecal instance " + instance.Name);
                    if (!bodies2Update.Contains(instance.CelestialBody))
                    {
                        bodies2Update.Add(instance.CelestialBody);
                    }


                    instance.mapDecal.transform.position = instance.CelestialBody.GetWorldSurfacePosition(instance.Latitude, instance.Longitude, instance.AbsolutOffset);
                    instance.mapDecal.transform.up = instance.CelestialBody.GetSurfaceNVector(instance.Latitude, instance.Longitude);

                    instance.Update(false);

                    instance.Group = DecalsDatabase.GetCloesedCenter(instance.mapDecal.transform.position).Group;
                }

            }
            // Rebuild spheres on all plants with new MapDecals
            foreach (CelestialBody body in bodies2Update)
            {
                Log.Normal("Rebuilding PQS sphere on: " + body.name);
                body.pqsController.RebuildSphere();
            }
        }
    }
}
