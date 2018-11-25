using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using KerbalKonstructs;
using KerbalKonstructs.Career;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Addons;
using KerbalKonstructs.UI;
using UnityEngine;

namespace KerbalKonstructs.Modules
{
    class CareerObjects
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
                    ConfigNode instanceNode = buildingNode.AddNode("Instance");

                    instanceNode.AddValue("UUID", instance.UUID);
                    instanceNode.AddValue("ModelName", instance.model.name);
                    instanceNode.AddValue("Body", instance.CelestialBody.name);
                    instanceNode.AddValue("Position", instance.RadialPosition);
                    instanceNode.AddValue("Altitude", instance.RadiusOffset);
                    instanceNode.AddValue("Rotation", instance.RotationAngle);
                    instanceNode.AddValue("IsScanable", instance.isScanable);
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

            instance.Orientation = Vector3.up;
            instance.heighReference = HeightReference.Terrain;
            instance.VisibilityRange = (PhysicsGlobals.Instance.VesselRangesDefault.flying.unload + 3000);

            instance.Group = "Career";

            instance.RadialPosition = ConfigNode.ParseVector3(cfgNode.GetValue("Position"));

            instance.model = StaticDatabase.GetModelByName(cfgNode.GetValue("ModelName"));

            if (instance.model == null)
            {
                Log.UserError("LoadFromSave: Canot find model named: " + cfgNode.GetValue("ModelName"));
                instance = null;
                return;
            }
            instance.mesh = UnityEngine.Object.Instantiate(instance.model.prefab);

            instance.UUID = cfgNode.GetValue("UUID");

            instance.CelestialBody = ConfigUtil.GetCelestialBody(cfgNode.GetValue("Body"));

            instance.RadiusOffset = float.Parse(cfgNode.GetValue("Altitude"));
            instance.RotationAngle = float.Parse(cfgNode.GetValue("Rotation"));

            instance.RefLatitude = KKMath.GetLatitudeInDeg(instance.RadialPosition);
            instance.RefLongitude = KKMath.GetLongitudeInDeg(instance.RadialPosition);

            InstanceUtil.CreateGroupCenterIfMissing(instance);

            if (cfgNode.HasValue("IsScanable"))
            {
                bool.TryParse(cfgNode.GetValue("IsScanable"), out instance.isScanable);
            }

            bool oldLegacySpawn = KerbalKonstructs.convertLegacyConfigs;

            instance.SpawnObject();

            KerbalKonstructs.convertLegacyConfigs = oldLegacySpawn;
        }

    }
}
