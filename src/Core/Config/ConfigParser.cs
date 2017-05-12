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

    }
}
