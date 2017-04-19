using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalKonstructs;
using KerbalKonstructs.Core;
using System.IO;

namespace KerbalKonstructs.Modules
{
    internal static class CareerState
    {
        private static Dictionary<string, Dictionary<String, String>> parsedConfig = new Dictionary<string, Dictionary<string, string>>();
        private static Dictionary<string, string> config = new Dictionary<string, string>();

        private static Dictionary<string, Dictionary<String, String>> parsedLSConfig = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// fills the parsedConfig struckture for LoadFacilities() function
        /// </summary>
        private static void ParseFacConfig()
        {
            parsedConfig.Clear();
            string saveConfigPath = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/KKFacilities.cfg";

            string position = "";
            string facType = "";

            ConfigNode rootNode = new ConfigNode();

            if (!File.Exists(saveConfigPath))
            {
                ConfigNode GameNode = rootNode.AddNode("GAME");
                ConfigNode ScenarioNode = GameNode.AddNode("SCENARIO");
                ScenarioNode.AddValue("Name", "KKStatics");
                rootNode.Save(saveConfigPath);
                return;
            }
            rootNode = ConfigNode.Load(saveConfigPath);
            ConfigNode gameNode = rootNode.GetNode("GAME");

            foreach (ConfigNode scenarioNode in gameNode.GetNodes("SCENARIO"))
            {

                foreach (ConfigNode facNode in scenarioNode.GetNodes("KKStatic"))
                {
                    Dictionary<string, string> config = new Dictionary<string, string>();
                    position = CareerUtils.KeyFromString(facNode.GetValue("RadialPosition"));
                    facType = facNode.GetValue("FacilityType");
                    if (parsedConfig.ContainsKey(position))
                    {
                        Log.UserError("duplicate posistion key found in file: " + position);
                        continue;
                    }
                    foreach (string key in CareerUtils.ParametersForFacility(facType))
                    {
                        if (facNode.HasValue(key))
                        {
                            config.Add(key, facNode.GetValue(key));
                        }
                    }
                    parsedConfig.Add(position, config);

                }

            }

        }



        /// <summary>
        /// Loads the state of all facilities from the .cfg file
        /// </summary>
        internal static void LoadFacilities()
        {
            ParseFacConfig();
            foreach (StaticObject instance in KerbalKonstructs.instance.getStaticDB().getAllStatics())
            {

                if (String.IsNullOrEmpty((string)instance.getSetting("FacilityType")) || String.Equals(((string)instance.getSetting("FacilityType")), "None", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                string instanceKey = CareerUtils.KeyFromString(instance.pqsCity.repositionRadial.ToString());
                // check if we have a config loaded with the same radial position
                if (parsedConfig.ContainsKey(instanceKey) )
                {
                    config.Clear();
                    config = parsedConfig[instanceKey];

                    foreach (string key in config.Keys.ToList())
                    {
                        if (CareerUtils.IsString(key))
                        {
                            instance.setSetting(key, config[key]);
                            //Log.Normal("Setting: " +(string)instance.getSetting("FacilityType") + " " + key + " to: " + config[key]);
                        }
                        if (CareerUtils.IsFloat(key))
                        {
                            instance.setSetting(key, float.Parse(config[key]));
                            //Log.Normal("Setting: " + (string)instance.getSetting("FacilityType") + " " + key + " to: " + config[key]);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// Parses the LaunchSite config for the LoadLaunchSites function
        /// </summary>
        internal static void ParseLSConfig()
        {
            string saveConfigPath = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/KK.cfg";

            parsedLSConfig.Clear();

            ConfigNode rootNode = new ConfigNode();

            if (!File.Exists(saveConfigPath))
            {
                // Save the current config here
                //ConfigNode SitesNode = rootNode.AddNode("LAUNCHSITES");
                //rootNode.Save(saveConfigPath);
                return;
            }
            rootNode = ConfigNode.Load(saveConfigPath);
            ConfigNode sitesNodes = rootNode.GetNode("LAUNCHSITES");

            foreach (ConfigNode sitesNode in sitesNodes.GetNodes())
            {
                Dictionary<string, string> config = new Dictionary<string, string>();
                foreach (string key in CareerUtils.ParametersForLaunchSite())
                {
                    if (sitesNode.HasValue(key))
                    {
                        config.Add(key, sitesNode.GetValue(key));
                    }
                }
                if (!parsedLSConfig.ContainsKey(sitesNode.name))
                    parsedLSConfig.Add(sitesNode.name, config);
            }

        }


        /// <summary>
        /// Loads the state of the LauchSites
        /// </summary>
        internal static void LoadLaunchSites()
        {
            ParseLSConfig();
            foreach (LaunchSite site in LaunchSiteManager.AllLaunchSites)
            {

                string name = CareerUtils.LSKeyFromName(site.name);

                if (parsedLSConfig.ContainsKey(name))
                {
                    config = parsedLSConfig[name];

                    if (config.ContainsKey("openclosestate"))
                    {
                        site.openclosestate = config["openclosestate"];
                    }
                    if (config.ContainsKey("favouritesite"))
                    {
                        site.favouritesite = config["favouritesite"];
                    }
                    if (config.ContainsKey("missionlog"))
                    {
                        site.missionlog = config["missionlog"];
                    }
                    if (config.ContainsKey("missioncount"))
                    {
                        site.missioncount = float.Parse(config["missioncount"]);
                    }
                }

            }
            


        }

        /// <summary>
        /// Loads the state of all facilities and LaunchSites
        /// </summary>
        internal static void Load()
        {
            LoadFacilities();
            LoadLaunchSites();

        }

        /// <summary>
        /// Makes Backups of the StateFiles
        /// </summary>
        internal static void BackupSaves()
        {
            string facSave = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/KKFacilities.cfg";
            string facSaveBackup = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/KKFacilitiesBack.cfg";


            string lsSave = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/KK.cfg";
            string lsSaveBackup = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/KKBack.cfg";

            if (File.Exists(facSave))
            {
                File.Copy(facSave, facSaveBackup, true);
            }

            if (File.Exists(lsSave))
            {
                File.Copy(lsSave, lsSaveBackup, true);
            }
        }

        /// <summary>
        /// saves the facility settings to the cfg file
        /// </summary>
        internal static void SaveFacilities()
        {
            string facSave = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/KKFacilities.cfg";

            ConfigNode rootNode = new ConfigNode();
            ConfigNode gameNode = rootNode.AddNode("GAME");
            ConfigNode scenarioNode = gameNode.AddNode("SCENARIO");

            foreach (StaticObject instance in KerbalKonstructs.instance.getStaticDB().getAllStatics())
            {
                string facilityType = (string)instance.getSetting("FacilityType");
                if (String.IsNullOrEmpty(facilityType) || String.Equals(facilityType, "None", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }
                ConfigNode instanceNode = scenarioNode.AddNode("KKStatic");
                instanceNode.SetValue("RadialPosition", instance.pqsCity.repositionRadial.ToString(), true);
                instanceNode.SetValue("FacilityType", facilityType, true);

                foreach (string parameter in CareerUtils.ParametersForFacility(facilityType))
                {

                    if (CareerUtils.IsString(parameter))
                    {
                        instanceNode.SetValue(parameter, (string)instance.getSetting(parameter), true);
                    }
                    if (CareerUtils.IsFloat(parameter))
                    {
                        instanceNode.SetValue(parameter, ((float)instance.getSetting(parameter)).ToString(), true);
                    }
                }
            }
            rootNode.Save(facSave);
        }


        internal static void SaveLaunchsites()
        {
            string name = null;
            string saveConfigPath = KSPUtil.ApplicationRootPath + "saves/" + HighLogic.SaveFolder + "/KK.cfg";

            ConfigNode rootNode = new ConfigNode();
            ConfigNode lsNodes = rootNode.AddNode("LAUNCHSITES");

            foreach (LaunchSite site in LaunchSiteManager.AllLaunchSites)
            {
                name = CareerUtils.LSKeyFromName(site.name);
                ConfigNode lsNode = lsNodes.AddNode(name);
                lsNode.SetValue("openclosestate", site.openclosestate, true);
                lsNode.SetValue("favouritesite", site.favouritesite, true);
                lsNode.SetValue("missioncount", site.missioncount.ToString(), true);
                lsNode.SetValue("missionlog", site.missionlog, true);
            }
            rootNode.Save(saveConfigPath);
        }

        internal static void Save()
        {
            BackupSaves();
            SaveFacilities();
            SaveLaunchsites();
        }


    }
}
