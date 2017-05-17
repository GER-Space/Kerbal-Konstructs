using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using System.Reflection;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.Core
{

        /// <summary>
    /// Settings that are read from the Instance Config file
    /// </summary>
    internal class CFGSetting : Attribute
    {

    }

    /// <summary>
    /// Settings, that are written to the Savegame State file
    /// </summary>
    internal class CareerSetting : Attribute
    {

    }

    /// <summary>
    /// We use dictionarys for the lookup of the parameter types, because they are way faster then making reflection lookups.
    /// We use reflektion calls to scan for the  datatypes of SaticModule and StaticObject with the attribute CFGSettings
    /// We have for each cfgfile-setting a same named field in the classes, so we don't need a translation table.
    /// </summary>
    internal static class ConfigUtil
    {
        internal static bool initialized = false;

        internal static Dictionary<string, FieldInfo> modelFields = new Dictionary<string, FieldInfo>();
        internal static Dictionary<string, FieldInfo> instanceFields = new Dictionary<string, FieldInfo>();

        internal static HashSet<string> facilitiyTypes = new HashSet<string>();


        // global stuff
        private static bool bodiesInitialized = false;
        private static Dictionary<string, CelestialBody> knownBodies = new Dictionary<string, CelestialBody>();

        /// <summary>
        /// Fills up the lookup tables for the parser. 
        /// </summary>
        internal static void InitTypes()
        {
            foreach (FieldInfo field in typeof(StaticModel).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    modelFields.Add(field.Name, field);
                    //Log.Normal("Parser Model:" + field.Name + ": " + field.FieldType.ToString());
                }

            }
            foreach (FieldInfo field in typeof(StaticObject).GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    instanceFields.Add(field.Name, field);
                    //Log.Normal("Parser Instance: " + field.Name + ": " + field.FieldType.ToString());
                }
            }


            facilitiyTypes = new HashSet<string>(Enum.GetNames(typeof(KKFacilityType)));

            initialized = true;
        }


        /// <summary>
        /// Reads a setting from a ConfigNode and writes the content to the targets field with the same name
        /// </summary>
        /// <param name="target"></param>
        /// <param name="field"></param>
        /// <param name="cfgNode"></param>
        internal static void ReadCFGNode(object target, FieldInfo field, ConfigNode cfgNode)
        {
            if (!cfgNode.HasValue(field.Name))
                return;

            if (!string.IsNullOrEmpty(cfgNode.GetValue(field.Name)))
            {
                switch (field.FieldType.ToString())
                {
                    case "System.String":
                        field.SetValue(target, cfgNode.GetValue(field.Name));
                        break;
                    case "System.Int32":
                        field.SetValue(target, int.Parse(cfgNode.GetValue(field.Name)));
                        break;
                    case "System.Single":
                        field.SetValue(target, float.Parse(cfgNode.GetValue(field.Name)));
                        break;
                    case "System.Double":
                        field.SetValue(target, double.Parse(cfgNode.GetValue(field.Name)));
                        break;
                    case "System.Boolean":
                        bool result;
                        bool.TryParse(cfgNode.GetValue(field.Name), out result);
                        field.SetValue(target, result);
                        break;
                    case "UnityEngine.Vector3":
                        field.SetValue(target, ConfigNode.ParseVector3(cfgNode.GetValue(field.Name)));
                        break;
                    case "UnityEngine.Vector3d":
                        field.SetValue(target, ConfigNode.ParseVector3D(cfgNode.GetValue(field.Name)));
                        break;
                    case "CelestialBody":
                        field.SetValue(target, ConfigUtil.GetCelestialBody(cfgNode.GetValue(field.Name)));
                        break;
                    case "KerbalKonstructs.Core.SiteType":
                        SiteType value = SiteType.Any;
                        try
                        {
                            value = (SiteType)Enum.Parse(typeof(SiteType), cfgNode.GetValue(field.Name));
                            
                        } catch
                        {
                            value = SiteType.Any;
                        }
                        field.SetValue(target, value);
                        break;
                }

            }
        }

        /// <summary>
        /// Writes a setting from an object to a confignode
        /// </summary>
        /// <param name="source"></param>
        /// <param name="field"></param>
        /// <param name="cfgNode"></param>
        internal static void Write2CfgNode(object source, FieldInfo field, ConfigNode cfgNode)
        {

                switch (field.FieldType.ToString())
            {
                case "System.String":
                    cfgNode.SetValue(field.Name, (string)field.GetValue(source), true);
                    break;
                case "System.Int32":
                    cfgNode.SetValue(field.Name, (int)field.GetValue(source), true);
                    break;
                case "System.Single":
                    cfgNode.SetValue(field.Name, (float)field.GetValue(source), true);
                    break;
                case "System.Double":
                    cfgNode.SetValue(field.Name, (double)field.GetValue(source), true);
                    break;
                case "System.Boolean":
                    cfgNode.SetValue(field.Name, (bool)field.GetValue(source), true);
                    break;
                case "UnityEngine.Vector3":
                    cfgNode.SetValue(field.Name, (Vector3)field.GetValue(source), true);
                    break;
                case "UnityEngine.Vector3d":
                    cfgNode.SetValue(field.Name, (Vector3d)field.GetValue(source), true);
                    break;
                case "CelestialBody":
                    cfgNode.SetValue(field.Name, ((CelestialBody)field.GetValue(source)).name, true);
                    break;
                case "KerbalKonstructs.Core.SiteType":
                    cfgNode.SetValue(field.Name, ((SiteType)field.GetValue(source)).ToString() , true);
                    break;
            }
        }

        /// <summary>
        /// Fast convert of a bodyname to a CelestianBody object also Supports "Homeworld" as a key
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static CelestialBody GetCelestialBody(String name)
        {
            if (!bodiesInitialized)
            {
                CelestialBody[] bodies = GameObject.FindObjectsOfType(typeof(CelestialBody)) as CelestialBody[];
                foreach (CelestialBody body in bodies)
                {
                    knownBodies.Add(body.name, body);
                    if (body.isHomeWorld)
                    {
                        knownBodies.Add("HomeWorld", body);
                    }

                }
                bodiesInitialized = true;
            }
            CelestialBody returnValue = null;

            if (knownBodies.TryGetValue(name, out returnValue))
            {
                return returnValue;
            }
            else
            {
                Log.UserError("Couldn't find body \"" + name + "\"");
                return null;
            }
        }
    }
}
