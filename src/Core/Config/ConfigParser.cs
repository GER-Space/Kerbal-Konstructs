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

            foreach (var key in ConfigUtil.modelFields.Keys)
            {

                if (!string.IsNullOrEmpty(cfgNode.GetValue(key)))
                {

                    if (ConfigUtil.IsString(key))
                    {
                        ConfigUtil.modelFields[key].SetValue(target, cfgNode.GetValue(key));
                    }
                    if (ConfigUtil.IsInt(key))
                    {
                        ConfigUtil.modelFields[key].SetValue(target, int.Parse(cfgNode.GetValue(key)));
                    }
                    if (ConfigUtil.IsFloat(key))
                    {
                        ConfigUtil.modelFields[key].SetValue(target, float.Parse(cfgNode.GetValue(key)));
                    }
                    if (ConfigUtil.IsDouble(key))
                    {
                        ConfigUtil.modelFields[key].SetValue(target, double.Parse(cfgNode.GetValue(key)));
                    }
                    if (ConfigUtil.IsBool(key))
                    {
                        bool result;
                        bool.TryParse(cfgNode.GetValue(key), out result);
                        ConfigUtil.modelFields[key].SetValue(target, result);
                    }
                    if (ConfigUtil.IsVector3(key))
                    {
                        ConfigUtil.modelFields[key].SetValue(target, ConfigNode.ParseVector3(cfgNode.GetValue(key)));
                    }
                }
            }

        }

        internal static void WriteModelConfig(StaticModel model, ConfigNode cfgNode)
        {
            if (!ConfigUtil.initialized)
                ConfigUtil.InitTypes();

            foreach (var modelsetting in ConfigUtil.modelFields)
            {
                if (modelsetting.Value.GetValue(model) == null)
                    continue;

                string key = modelsetting.Key;
                if (modelsetting.Key == "mesh") continue;

                if (cfgNode.HasValue(modelsetting.Key))
                {
                    cfgNode.RemoveValue(modelsetting.Key);

                    if (ConfigUtil.IsString(key))
                    {
                        cfgNode.AddValue(modelsetting.Key, (string)modelsetting.Value.GetValue(model));
                    }
                    if (ConfigUtil.IsInt(key))
                    {
                        cfgNode.AddValue(modelsetting.Key, (int)modelsetting.Value.GetValue(model));
                    }
                    if (ConfigUtil.IsFloat(key))
                    {
                        cfgNode.AddValue(modelsetting.Key, (float)modelsetting.Value.GetValue(model));
                    }
                    if (ConfigUtil.IsDouble(key))
                    {
                        cfgNode.AddValue(modelsetting.Key, (double)modelsetting.Value.GetValue(model));
                    }
                    if (ConfigUtil.IsBool(key))
                    {
                        cfgNode.AddValue(modelsetting.Key, (bool)modelsetting.Value.GetValue(model));
                    }
                    if (ConfigUtil.IsVector3(key))
                    {
                        cfgNode.AddValue(modelsetting.Key, (Vector3)modelsetting.Value.GetValue(model));
                    }
                }
            }



        }

    }
}
