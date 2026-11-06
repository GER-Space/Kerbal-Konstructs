using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;


namespace KerbalKonstructs.Core
{

    internal static class ConfigParser
    {

        internal static string exportTime;

        internal static void ParseModelConfig(StaticModel target, ConfigNode cfgNode)
        {
            if (!ConfigUtil.initialized)
            {
                ConfigUtil.InitTypes();
            }

            foreach (var field in ConfigUtil.modelFields.Values)
            {
                ConfigUtil.ReadCFGNode(target, field, cfgNode);
            }

            foreach (ConfigNode variNode in cfgNode.GetNodes("VARIANT"))
            {
                Log.Normal("found variant: " + variNode.GetValue("name"));
                ModelVariant variant = new ModelVariant(target, variNode);

                if (string.IsNullOrEmpty(variant.name))
                {
                    Log.UserError("model " + target.name + " has a variant without a name");
                    continue;
                }

                if (!target.variants.ContainsKey(variant.name))
                {
                    Log.Normal("added variant: " + variant.name);
                    target.variants.Add(variant.name, variant);
                }
                else
                {
                    Log.UserError("model " + target.name + " already has a vaiant with name: " + variant.name);
                }

            }

        }

        internal static void WriteModelConfig(StaticModel model, ConfigNode cfgNode)
        {

            foreach (var modelsetting in ConfigUtil.modelFields)
            {
                try
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
                catch
                {
                    Log.UserError("error in writing modelconfig: " + model.name + " key: " + modelsetting.Key);
                    throw;
                }

                foreach (ModelVariant vaiant in model.variants.Values)
                {
                    cfgNode.AddNode("VARIANT", vaiant.GetConfig());
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

                // Remove legacy GrassColor values
                //if ((instanceSetting.Key == "GrasColor") || (instanceSetting.Key == "GrasTexture"))
                //{
                //    continue;
                //}

                // No Longer save Legacy Values
                if (instanceSetting.Key == "RadialPosition" || instanceSetting.Key == "RadiusOffset" || instanceSetting.Key == "RotationAngle" || instanceSetting.Key == "RefLatitude" || instanceSetting.Key == "RefLongitude" || instanceSetting.Key == "IsRelativeToTerrain")
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
                LaunchSiteParser.WriteConfig(instance.launchSite, lsNode);
            }

            // save unsaved grassSettings before we write everything to the instance node
            if (instance.isSpawned)
            {
                GrassColor2[] grassArray = instance.mesh.GetComponents<GrassColor2>();
                if (grassArray.Length > 0)
                {
                    instance.grassColor2Configs.Clear();
                    foreach (GrassColor2 grassColor in grassArray)
                    {
                        instance.grassColor2Configs.Add(grassColor.GiveConfig());
                    }
                }
            }
            foreach (ConfigNode grassNode in instance.grassColor2Configs)
            {
                cfgNode.AddNode("GrassColor2", grassNode);
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
            {
                ConfigUtil.InitTypes();
            }

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
            {
                ConfigUtil.InitTypes();
            }

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
                if (mapDecalSetting.Value.GetValue(mapDecalInstance) != null)
                {
                    ConfigUtil.Write2CfgNode(mapDecalInstance, mapDecalSetting.Value, cfgNode);
                }
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

        /// <summary>
        /// Writes out a mapdecal config
        /// </summary>
        /// <param name="instance"></param>
        internal static void SaveMapDecalInstance(MapDecalInstance instance)
        {
            if (instance.isInSavegame) return;

            if (!Directory.Exists(KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath))
            {
                Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath);
            }

            if (instance.configPath == null)
            {
                instance.configPath = KerbalKonstructs.newInstancePath + "/KK_MapDecal_" + instance.Name + ".cfg";
                if (System.IO.File.Exists(KSPUtil.ApplicationRootPath + "GameData/" + instance.configPath))
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

                newMapDecalInstance.path = Path.GetDirectoryName(Path.GetDirectoryName(conf.url)).Replace('\\', '/');
                newMapDecalInstance.mapTexture = KKGraphics.GetTexture(newMapDecalInstance.path + "/" + newMapDecalInstance.Image, false);

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
                //Log.Normal("DecalsMap " + newMapDecalInstance.Name + " imported: " + (newMapDecalInstance.isHeightMap? "as HeighMap" : "as ColorMap"));

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

            if (string.IsNullOrEmpty(pathname))
            {
                Log.UserWarning("Trying to save a static insance with null configpath. ");
                return;
            }

            Log.Debug("Saving File: " + pathname);
            StaticInstance[] allInstances = StaticDatabase.allStaticInstances.Where(instance => instance.configPath == pathname && !instance.isInSavegame).ToArray();

            ConfigNode staticNode = new ConfigNode("STATIC");

            if (allInstances.Length > 0)
            {
                StaticInstance firstInstance = allInstances.First();
                ConfigNode instanceConfig = null;


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
            }

            staticNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + pathname, "Generated by Kerbal Konstructs");
        }

        /// <summary>
        /// Loads all Group Centers from the Files
        /// </summary>
        internal static void LoadAllGroupCenter()
        {

            foreach (UrlDir.UrlConfig conf in GameDatabase.Instance.GetConfigs("KK_GroupCenter"))
            {
                GroupCenter center = new GroupCenter();
                center.ParseCFGNode(conf.config);
                center.configPath = conf.url.Substring(0, conf.url.LastIndexOf('/')) + ".cfg";
                center.configUrl = conf;

                center.Spawn();


            }
        }

        internal static void ExportAll()
        {
            exportTime = "Export_" + DateTime.Now.ToString("yyyy-MM-dd_HHmm-ss" + "/");
            ExportAllGroups();
            ExportPQSMapDecals();
            CreateZips();
        }



        internal static void ExportAllGroups()
        {
            string basePath = KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + exportTime;
            HashSet<string> parsedModels = new HashSet<string>();

            foreach (GroupCenter group in StaticDatabase.allGroupCenters)
            {
                if (group.isInSavegame) continue;

                parsedModels.Clear();

                if (!System.IO.Directory.Exists(basePath))
                {
                    System.IO.Directory.CreateDirectory(basePath);
                }

                if (!System.IO.Directory.Exists(basePath + group.Group))
                {
                    System.IO.Directory.CreateDirectory(basePath + group.Group);
                }

                if (!group.isBuiltIn)
                {
                    ConfigNode masterNode = new ConfigNode("");
                    ConfigNode childNode = new ConfigNode("KK_GroupCenter");
                    group.WriteConfig(childNode);
                    masterNode.AddNode(childNode);
                    masterNode.Save(basePath + group.Group + "/" + group.Group + "_center.cfg", "Generated by Kerbal Konstructs");
                }
                foreach (StaticInstance instance in group.childInstances)
                {
                    if (instance.isInSavegame) continue;

                    if (parsedModels.Contains(instance.model.name))
                    {
                        continue;
                    }
                    parsedModels.Add(instance.model.name);
                    StaticInstance[] allInstances = group.childInstances.Where(inst => inst.model.name == instance.model.name).ToArray();

                    ConfigNode instanceConfig = null;
                    ConfigNode staticNode = new ConfigNode("STATIC");

                    instanceConfig = new ConfigNode("STATIC");
                    instanceConfig.AddValue("pointername", instance.model.name);
                    staticNode.AddNode(instanceConfig);
                    foreach (StaticInstance modelInstance in allInstances)
                    {
                        ConfigNode inst = new ConfigNode("Instances");
                        ConfigParser.WriteInstanceConfig(modelInstance, inst);
                        instanceConfig.nodes.Add(inst);
                    }
                    staticNode.Save(basePath + group.Group + "/" + instance.model.name + ".cfg", "Generated by Kerbal Konstructs");

                }
            }
        }

        internal static void ExportPQSMapDecals()
        {
            string basePath = KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + exportTime;

            foreach (MapDecalInstance mapDecalInstance in DecalsDatabase.allMapDecalInstances)
            {

                mapDecalInstance.Group = DecalsDatabase.GetCloesedCenter(mapDecalInstance.gameObject.transform.position).Group;

                if (!System.IO.Directory.Exists(basePath + mapDecalInstance.Group))
                {
                    System.IO.Directory.CreateDirectory(basePath + mapDecalInstance.Group);
                }

                ConfigNode masterNode = new ConfigNode("");
                ConfigNode instanceNode = new ConfigNode("KK_MapDecal");

                WriteMapDecalConfig(mapDecalInstance, instanceNode);

                masterNode.AddNode(instanceNode);

                masterNode.Save(basePath + mapDecalInstance.Group + "/" + mapDecalInstance.Name + "_PQSDecal.cfg", "Generated by Kerbal Konstructs");

            }
        }


        internal static void CreateZips()
        {

            string basePath = KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + exportTime;
            string allFileName = "AllExports" + DateTime.Now.ToString("_yyyy-MMdd-HHmm-ss") + ".zip";


            ZipFile.CreateFromDirectory(basePath, KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/ExportedInstances/" + allFileName, System.IO.Compression.CompressionLevel.Fastest, false);

            foreach (string dirname in Directory.GetDirectories(basePath))
            {

                string groupFilename = dirname + ".zip";

                if (File.Exists(groupFilename))
                {
                    File.Delete(groupFilename);
                }

                ZipFile.CreateFromDirectory(dirname, groupFilename, System.IO.Compression.CompressionLevel.Fastest, true);
                Directory.Delete(dirname, true);
            }
        }

    }
}
