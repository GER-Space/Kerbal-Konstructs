using KerbalKonstructs.Core;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs.Modules
{
    public class CareerObjects
    {

        private static string buildingCFGNodeName = "KKBuildings";

        public static void SaveBuildings(ConfigNode cfgNode)
        {

            if (cfgNode.HasNode(buildingCFGNodeName))
            {
                cfgNode.RemoveNode(buildingCFGNodeName);
            }
            ConfigNode buildingNode = cfgNode.AddNode(buildingCFGNodeName);

            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                if (instance.isInSavegame)
                {
                    Debug.Log($"Saving instance {instance.UUID} ({instance.groupCenterName}) in savegame");
                    ConfigNode instanceNode = buildingNode.AddNode("Instance");
                    instanceNode.AddValue("ModelName", instance.model.name);

                    ConfigParser.WriteInstanceConfig(instance, instanceNode);
                }
            }
        }

        public static void LoadBuildings(ConfigNode cfgNode)
        {
            RemoveAllBuildings();

            ConfigNode buildingNode;
            if (cfgNode.HasNode(buildingCFGNodeName))
            {
                buildingNode = cfgNode.GetNode(buildingCFGNodeName);
            }
            else
            {
                return;
            }


            foreach (ConfigNode instanceNode in buildingNode.GetNodes())
            {
                LoadBuilding(instanceNode);
            }
        }

        public static void RemoveAllBuildings()
        {
            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                if (instance.isInSavegame)
                {
                    instance.Destroy();
                }
            }
        }

        public static void RemoveBuilding(StaticInstance instance)
        {
            if (instance.isInSavegame)
            {
                instance.Destroy();
            }
        }

        public static void LoadBuilding(ConfigNode cfgNode)
        {
            StaticInstance instance = new StaticInstance();

            instance.isInSavegame = true;

            Debug.Log($"ModelName: {cfgNode.GetValue("ModelName")}");
            instance.model = StaticDatabase.GetModelByName(cfgNode.GetValue("ModelName"));
            if (instance.model == null)
            {
                Log.UserError("LoadFromSave: Canot find model named: " + cfgNode.GetValue("ModelName"));
                instance = null;
                return;
            }
            ConfigParser.ParseInstanceConfig(instance, cfgNode);
            if (instance.CelestialBody == null)
            {
                instance = null;
                return;
            }

            if (instance.Group == null)
            {
                instance = null;
                return;
            }

            instance.Orientate();

            if (!StaticDatabase.HasInstance(instance))
            {
                instance.Destroy();
                return;
            }

            LaunchSiteManager.AttachLaunchSite(instance, cfgNode);
            KerbalKonstructs.AttachFacilities(instance, cfgNode);



            instance.grassColor2Configs = cfgNode.GetNodes("GrassColor2").ToList();

            // update the references
            foreach (var facility in instance.myFacilities)
            {
                facility.staticInstance = instance;
            }

            instance.Deactivate();
        }

    }
}
