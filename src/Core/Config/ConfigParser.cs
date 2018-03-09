using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using System.IO;

namespace KerbalKonstructs.Core
{

    internal static class ConfigParser
    {
        internal static void ParseModelConfig(StaticModel target, ConfigNode cfgNode)
        {
            if (!ConfigUtil.initialized)
                ConfigUtil.InitTypes();

            foreach (var field in ConfigUtil.modelFields.Values)
            {
                ConfigUtil.ReadCFGNode(target, field, cfgNode);
            }
        }

        internal static void WriteModelConfig(StaticModel model, ConfigNode cfgNode)
        {

            foreach (var modelsetting in ConfigUtil.modelFields)
            {
                if (modelsetting.Value.GetValue(model) == null)
                {
                    continue;
                }

                if (modelsetting.Key == "mesh")
                {
                    continue;
                }

                if (cfgNode.HasValue(modelsetting.Key))
                {
                    cfgNode.RemoveValue(modelsetting.Key);
                }

                switch (modelsetting.Value.FieldType.ToString())
                {
                    case "System.String":
                        if (string.IsNullOrEmpty((string)modelsetting.Value.GetValue(model)))
                        {
                            continue;
                        }
                        cfgNode.AddValue(modelsetting.Key, (string)modelsetting.Value.GetValue(model));
                        break;
                    case "System.Int32":
                        if ((int)modelsetting.Value.GetValue(model) == 0)
                        {
                            continue;
                        }
                        cfgNode.AddValue(modelsetting.Key, (int)modelsetting.Value.GetValue(model));
                        break;
                    case "System.Single":
                        if ((float)modelsetting.Value.GetValue(model) == 0)
                        {
                            continue;
                        }
                        cfgNode.AddValue(modelsetting.Key, (float)modelsetting.Value.GetValue(model));
                        break;
                    case "System.Double":
                        if ((double)modelsetting.Value.GetValue(model) == 0)
                        {
                            continue;
                        }
                        cfgNode.AddValue(modelsetting.Key, (double)modelsetting.Value.GetValue(model));
                        break;
                    case "System.Boolean":
                        cfgNode.AddValue(modelsetting.Key, (bool)modelsetting.Value.GetValue(model));
                        break;
                    case "UnityEngine.Vector3":
                        cfgNode.AddValue(modelsetting.Key, (Vector3)modelsetting.Value.GetValue(model));
                        break;
                    case "UnityEngine.Vector3d":
                        cfgNode.AddValue(modelsetting.Key, (Vector3d)modelsetting.Value.GetValue(model));
                        break;
                    case "CelestialBody":
                        cfgNode.AddValue(modelsetting.Key, ((CelestialBody)modelsetting.Value.GetValue(model)).name);
                        break;
                }

            }
        }

        /// <summary>
        /// Reads the ConfigNode and sets the values into the instance
        /// </summary>
        /// <param name="target"></param>
        /// <param name="cfgNode"></param>
        internal static void ParseInstanceConfig(StaticInstance target, ConfigNode cfgNode)
        {
            if (!ConfigUtil.initialized)
            {
                ConfigUtil.InitTypes();
            }

            foreach (var field in ConfigUtil.instanceFields.Values)
            {
                ConfigUtil.ReadCFGNode(target, field, cfgNode);
            }
        }

        /// <summary>
        /// Writes out all the data of the instance To a ConfigNode (+ the Facilities)
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="cfgNode"></param>
        internal static void WriteInstanceConfig(StaticInstance instance, ConfigNode cfgNode)
        {

            foreach (var instanceSetting in ConfigUtil.instanceFields)
            {
                if (instanceSetting.Value.GetValue(instance) == null)
                {
                    continue;
                }
                if (instanceSetting.Key == "FacilityType")
                {
                    continue;
                }
                if (instanceSetting.Key == "GrasColor" && (Color)instanceSetting.Value.GetValue(instance) == Color.clear)
                {
                    continue;
                }

                ConfigUtil.Write2CfgNode(instance, instanceSetting.Value, cfgNode);

            }

            if (instance.hasFacilities)
            {
                for (int i = 0; i < instance.myFacilities.Count; i++)
                {

                    ConfigNode facNode = cfgNode.AddNode("Facility");
                    instance.myFacilities[i].WriteConfig(facNode);
                }
            }

            if (instance.hasLauchSites)
            {
                ConfigNode lsNode = cfgNode.AddNode("LaunchSite");
                instance.launchSite.WriteConfig(lsNode);
            }

        }

        /// <summary>
        /// Parser for MapDecalInstance Objects
        /// </summary>
        /// <param name="target"></param>
        /// <param name="cfgNode"></param>
        internal static void ParseMapDecalConfig(MapDecalInstance target, ConfigNode cfgNode)
        {
            if (!ConfigUtil.initialized)
                ConfigUtil.InitTypes();

            foreach (var field in ConfigUtil.mapDecalFields.Values)
            {
                ConfigUtil.ReadCFGNode(target, field, cfgNode);
            }
        }

        /// <summary>
        /// Parser for MapDecalInstance Objects
        /// </summary>
        /// <param name="target"></param>
        /// <param name="cfgNode"></param>
        internal static void ParseDecalsMapConfig(MapDecalsMap target, ConfigNode cfgNode)
        {
            if (!ConfigUtil.initialized)
                ConfigUtil.InitTypes();

            foreach (var field in ConfigUtil.mapDecalsMapFields.Values)
            {
                ConfigUtil.ReadCFGNode(target, field, cfgNode);
            }
        }

        /// <summary>
        /// Writer for MapDecalObjects
        /// </summary>
        /// <param name="mapDecalInstance"></param>
        /// <param name="cfgNode"></param>
        internal static void WriteMapDecalConfig(MapDecalInstance mapDecalInstance, ConfigNode cfgNode)
        {

            foreach (var mapDecalSetting in ConfigUtil.mapDecalFields)
            {
                if (mapDecalSetting.Value.GetValue(mapDecalInstance) == null)
                    continue;

                ConfigUtil.Write2CfgNode(mapDecalInstance, mapDecalSetting.Value, cfgNode);

            }
        }

        /// <summary>
        /// Loads all KK_MAPDecals and places them on the planets
        /// </summary>
        internal static void LoadAllMapDecals()
        {
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("KK_MapDecal");

            foreach (UrlDir.UrlConfig conf in configs)
            {
                //create new Instance and Register in Database
                MapDecalInstance newMapDecalInstance = new MapDecalInstance();
                // Load Settings into instance
                ConfigParser.ParseMapDecalConfig(newMapDecalInstance, conf.config);
                // set the configpath for saving
                newMapDecalInstance.configPath = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg";
            }

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
                    Log.Normal("Loaded MapDecal instance " + instance.Name);
                    if (!bodies2Update.Contains(instance.CelestialBody))
                    {
                        bodies2Update.Add(instance.CelestialBody);
                    }


                    instance.mapDecal.transform.position = instance.CelestialBody.GetWorldSurfacePosition(instance.Latitude, instance.Longitude, instance.AbsolutOffset);
                    instance.mapDecal.transform.up = instance.CelestialBody.GetSurfaceNVector(instance.Latitude, instance.Longitude);

                    instance.Update(false);
                }

            }
            // Rebuild spheres on all plants with new MapDecals
            foreach (CelestialBody body in bodies2Update)
            {
                Log.Normal("Rebuilding PQS sphere on: " + body.name);
                body.pqsController.RebuildSphere();
            }

        }

        /// <summary>
        /// Writes out a mapdecal config
        /// </summary>
        /// <param name="instance"></param>
        internal static void SaveMapDecalInstance(MapDecalInstance instance)
        {

            if (instance.configPath == null)
            {
                instance.configPath = KerbalKonstructs.newInstancePath + "/KK_MapDecal_" + instance.Name + ".cfg";
                if (System.IO.File.Exists(instance.configPath))
                {
                    instance.configPath = KerbalKonstructs.newInstancePath + "/KK_MapDecal_" + instance.Name + "_" + Guid.NewGuid() + ".cfg";
                }
            }

            ConfigNode masterNode = new ConfigNode("");
            ConfigNode instanceNode = new ConfigNode("KK_MapDecal");

            WriteMapDecalConfig(instance, instanceNode);

            masterNode.AddNode(instanceNode);

            masterNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + instance.configPath, "Generated by Kerbal Konstructs");

        }



        internal static void LoadAllMapDecalMaps()
        {
            UrlDir.UrlConfig[] configs = GameDatabase.Instance.GetConfigs("KK_DecalsMap");

            foreach (UrlDir.UrlConfig conf in configs)
            {
                //create new Instance and Register in Database
                MapDecalsMap newMapDecalInstance = ScriptableObject.CreateInstance<MapDecalsMap>();
                // Load Settings into instance
                ParseDecalsMapConfig(newMapDecalInstance, conf.config);

                newMapDecalInstance.path = Path.GetDirectoryName(Path.GetDirectoryName(conf.url));
                newMapDecalInstance.mapTexture = GameDatabase.Instance.GetTexture(newMapDecalInstance.path + "/" + newMapDecalInstance.Image, false);

                if (newMapDecalInstance.mapTexture == null)
                {
                    Log.UserError("Image File " + newMapDecalInstance.path + "/" + newMapDecalInstance.Image + " could not be loaded");
                    continue;
                }

                if (newMapDecalInstance.UseAsHeighMap)
                {
                    newMapDecalInstance.CreateMap(MapSO.MapDepth.Greyscale, newMapDecalInstance.mapTexture);
                    newMapDecalInstance.isHeightMap = true;
                }
                else
                {
                    newMapDecalInstance.CreateMap(MapSO.MapDepth.RGBA, newMapDecalInstance.mapTexture);
                }
                Log.Normal("DecalsMap " + newMapDecalInstance.Name + " imported: " + (newMapDecalInstance.isHeightMap? "as HeighMap" : "as ColorMap"));

                newMapDecalInstance.map = newMapDecalInstance as MapSO;
                DecalsDatabase.RegisterMap(newMapDecalInstance);
            }
        }


        /// <summary>
        /// this saves the pointer references to thier configfiles.
        /// </summary>
        /// <param name="pathname"></param>
        internal static void SaveInstanceByCfg(string pathname)
        {
            Log.Normal("Saving File: " + pathname);
            StaticInstance[] allInstances = StaticDatabase.allStaticInstances.Where(instance => instance.configPath == pathname).ToArray();
            StaticInstance firstInstance = allInstances.First();
            ConfigNode instanceConfig = null;

            ConfigNode staticNode = new ConfigNode("STATIC");

            if (firstInstance.configUrl == null) //this are newly spawned instances
            {
                instanceConfig = new ConfigNode("STATIC");
                instanceConfig.AddValue("pointername", firstInstance.model.name);
            }
            else
            {
                //instanceConfig = GameDatabase.Instance.GetConfigNode(firstInstance.configUrl.url);
                //instanceConfig.RemoveNodes("Instances");
                //instanceConfig.RemoveValues();
                instanceConfig = new ConfigNode("STATIC");
                instanceConfig.AddValue("pointername", firstInstance.model.name);

            }

            staticNode.AddNode(instanceConfig);
            foreach (StaticInstance instance in allInstances)
            {
                ConfigNode inst = new ConfigNode("Instances");
                ConfigParser.WriteInstanceConfig(instance, inst);
                instanceConfig.nodes.Add(inst);
            }
            staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + firstInstance.configPath, "Generated by Kerbal Konstructs");
        }



    }
}
