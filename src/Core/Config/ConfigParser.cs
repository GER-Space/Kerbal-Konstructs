using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;

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
                    continue;

                ;
                if (modelsetting.Key == "mesh") continue;

                if (cfgNode.HasValue(modelsetting.Key))
                    cfgNode.RemoveValue(modelsetting.Key);


                switch (modelsetting.Value.FieldType.ToString())
                {
                    case "System.String":
                        if (string.IsNullOrEmpty((string)modelsetting.Value.GetValue(model)))
                            continue;
                        cfgNode.AddValue(modelsetting.Key, (string)modelsetting.Value.GetValue(model));
                        break;
                    case "System.Int32":
                        if ((int)modelsetting.Value.GetValue(model) == 0)
                            continue;
                        cfgNode.AddValue(modelsetting.Key, (int)modelsetting.Value.GetValue(model));
                        break;
                    case "System.Single":
                        if ((float)modelsetting.Value.GetValue(model) == 0)
                            continue;
                        cfgNode.AddValue(modelsetting.Key, (float)modelsetting.Value.GetValue(model));
                        break;
                    case "System.Double":
                        if ((double)modelsetting.Value.GetValue(model) == 0)
                            continue;
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
        internal static void ParseInstanceConfig(StaticObject target, ConfigNode cfgNode)
        {
            if (!ConfigUtil.initialized)
                ConfigUtil.InitTypes();

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
        internal static void WriteInstanceConfig(StaticObject instance, ConfigNode cfgNode)
        {

            foreach (var instanceSetting in ConfigUtil.instanceFields)
            {
                if (instanceSetting.Value.GetValue(instance) == null)
                    continue;

                if (instanceSetting.Key == "FacilityType")
                    continue;

                switch (instanceSetting.Value.FieldType.ToString())
                {
                    case "System.String":
                        if (string.IsNullOrEmpty((string)instanceSetting.Value.GetValue(instance)))
                            continue;
                        cfgNode.SetValue(instanceSetting.Key, (string)instanceSetting.Value.GetValue(instance), true);
                        break;
                    case "System.Int32":
                        if ((int)instanceSetting.Value.GetValue(instance) == 0)
                            continue;
                        cfgNode.SetValue(instanceSetting.Key, (int)instanceSetting.Value.GetValue(instance), true);
                        break;
                    case "System.Single":
                        if ((float)instanceSetting.Value.GetValue(instance) == 0)
                            continue;
                        cfgNode.SetValue(instanceSetting.Key, (float)instanceSetting.Value.GetValue(instance), true);
                        break;
                    case "System.Double":
                        if ((double)instanceSetting.Value.GetValue(instance) == 0)
                            continue;
                        cfgNode.SetValue(instanceSetting.Key, (double)instanceSetting.Value.GetValue(instance), true);
                        break;
                    case "System.Boolean":
                        cfgNode.SetValue(instanceSetting.Key, (bool)instanceSetting.Value.GetValue(instance), true);
                        break;
                    case "UnityEngine.Vector3":
                        cfgNode.SetValue(instanceSetting.Key, (Vector3)instanceSetting.Value.GetValue(instance), true);
                        break;
                    case "UnityEngine.Vector3d":
                        cfgNode.SetValue(instanceSetting.Key, (Vector3d)instanceSetting.Value.GetValue(instance), true);
                        break;
                    case "CelestialBody":
                        cfgNode.SetValue(instanceSetting.Key, ((CelestialBody)instanceSetting.Value.GetValue(instance)).name, true);
                        break;
                }

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



    }
}
