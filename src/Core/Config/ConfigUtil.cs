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

        internal static Dictionary<string, FieldInfo> mapDecalFields = new Dictionary<string, FieldInfo>();
        internal static Dictionary<string, FieldInfo> mapDecalsMapFields = new Dictionary<string, FieldInfo>();

        internal static Dictionary<string, FieldInfo> groupCenterFields = new Dictionary<string, FieldInfo>();


        internal static HashSet<string> facilitiyTypes = new HashSet<string>();


        // global stuff
        private static bool bodiesInitialized = false;
        private static Dictionary<string, CelestialBody> knownBodies = new Dictionary<string, CelestialBody>();

        /// <summary>
        /// Fills up the lookup tables for the parser. 
        /// </summary>
        internal static void InitTypes()
        {
            foreach (FieldInfo field in typeof(StaticModel).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    modelFields.Add(field.Name, field);
                    //Log.Normal("Parser Model:" + field.Name + ": " + field.FieldType.ToString());
                }

            }
            foreach (FieldInfo field in typeof(StaticInstance).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    instanceFields.Add(field.Name, field);
                   // Log.Normal("Parser Instance: " + field.Name + ": " + field.FieldType.ToString());
                }
            }

            foreach (FieldInfo field in typeof(MapDecalInstance).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    mapDecalFields.Add(field.Name, field);
                }
            }


            foreach (FieldInfo field in typeof(MapDecalsMap).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    mapDecalsMapFields.Add(field.Name, field);
                }
            }

            foreach (FieldInfo field in typeof(GroupCenter).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    groupCenterFields.Add(field.Name, field);
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
                    case "UnityEngine.Color":
                        field.SetValue(target, ConfigNode.ParseColor(cfgNode.GetValue(field.Name)));
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
                    case "KerbalKonstructs.Core.LaunchSiteCategory":
                        LaunchSiteCategory category = LaunchSiteCategory.Other;
                        try
                        {
                            category = (LaunchSiteCategory)Enum.Parse(typeof(LaunchSiteCategory), cfgNode.GetValue(field.Name));

                        }
                        catch
                        {
                            category = LaunchSiteCategory.Other;
                        }
                        field.SetValue(target, category);
                        break;
                }

            }
        }

        /// <summary>
        /// Reads a property setting from a ConfigNode and writes the content to the targets field with the same name
        /// </summary>
        /// <param name="target"></param>
        /// <param name="property"></param>
        /// <param name="cfgNode"></param>
        internal static void ReadCFGNode(object target, PropertyInfo property, ConfigNode cfgNode)
        {
            if (!cfgNode.HasValue(property.Name))
                return;

            if (!string.IsNullOrEmpty(cfgNode.GetValue(property.Name)))
            {
                switch (property.PropertyType.ToString())
                {
                    case "System.String":
                        property.SetValue(target, cfgNode.GetValue(property.Name), null);
                        break;
                    case "System.Int32":
                        property.SetValue(target, int.Parse(cfgNode.GetValue(property.Name)), null);
                        break;
                    case "System.Single":
                        property.SetValue(target, float.Parse(cfgNode.GetValue(property.Name)), null);
                        break;
                    case "System.Double":
                        property.SetValue(target, double.Parse(cfgNode.GetValue(property.Name)), null);
                        break;
                    case "System.Boolean":
                        bool result;
                        bool.TryParse(cfgNode.GetValue(property.Name), out result);
                        property.SetValue(target, result, null);
                        break;
                    case "UnityEngine.Vector3":
                        property.SetValue(target, ConfigNode.ParseVector3(cfgNode.GetValue(property.Name)), null);
                        break;
                    case "UnityEngine.Vector3d":
                        property.SetValue(target, ConfigNode.ParseVector3D(cfgNode.GetValue(property.Name)), null);
                        break;
                    case "CelestialBody":
                        property.SetValue(target, ConfigUtil.GetCelestialBody(cfgNode.GetValue(property.Name)), null);
                        break;
                    case "UnityEngine.Color":
                        property.SetValue(target, ConfigNode.ParseColor(cfgNode.GetValue(property.Name)), null);
                        break;
                    case "KerbalKonstructs.Core.SiteType":
                        SiteType value = SiteType.Any;
                        try
                        {
                            value = (SiteType)Enum.Parse(typeof(SiteType), cfgNode.GetValue(property.Name));

                        }
                        catch
                        {
                            value = SiteType.Any;
                        }
                        property.SetValue(target, value, null);
                        break;
                    case "KerbalKonstructs.Core.LaunchSiteCategory":
                        LaunchSiteCategory category = LaunchSiteCategory.Other;
                        try
                        {
                            category = (LaunchSiteCategory)Enum.Parse(typeof(LaunchSiteCategory), cfgNode.GetValue(property.Name));

                        }
                        catch
                        {
                            category = LaunchSiteCategory.Other;
                        }
                        property.SetValue(target, category, null);
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
                case "UnityEngine.Color":
                    cfgNode.SetValue(field.Name, (Color)field.GetValue(source), true);
                    break;
                case "CelestialBody":
                    cfgNode.SetValue(field.Name, ((CelestialBody)field.GetValue(source)).name, true);
                    break;
                case "KerbalKonstructs.Core.SiteType":
                    cfgNode.SetValue(field.Name, ((SiteType)field.GetValue(source)).ToString() , true);
                    break;
                case "KerbalKonstructs.Core.LaunchSiteCategory":
                    cfgNode.SetValue(field.Name, ((LaunchSiteCategory)field.GetValue(source)).ToString(), true);
                    break;
            }
        }

        /// <summary>
        /// Writes a property setting from an object to a confignode
        /// </summary>
        /// <param name="source"></param>
        /// <param name="property"></param>
        /// <param name="cfgNode"></param>
        internal static void Write2CfgNode(object source, PropertyInfo property, ConfigNode cfgNode)
        {
            switch (property.PropertyType.ToString())
            {
                case "System.String":
                    cfgNode.SetValue(property.Name, (string)property.GetValue(source, null), true);
                    break;
                case "System.Int32":
                    cfgNode.SetValue(property.Name, (int)property.GetValue(source, null), true);
                    break;
                case "System.Single":
                    cfgNode.SetValue(property.Name, (float)property.GetValue(source, null), true);
                    break;
                case "System.Double":
                    cfgNode.SetValue(property.Name, (double)property.GetValue(source, null), true);
                    break;
                case "System.Boolean":
                    cfgNode.SetValue(property.Name, (bool)property.GetValue(source, null), true);
                    break;
                case "UnityEngine.Vector3":
                    cfgNode.SetValue(property.Name, (Vector3)property.GetValue(source, null), true);
                    break;
                case "UnityEngine.Vector3d":
                    cfgNode.SetValue(property.Name, (Vector3d)property.GetValue(source, null), true);
                    break;
                case "UnityEngine.Color":
                    cfgNode.SetValue(property.Name, (Color)property.GetValue(source, null), true);                    
                    break;
                case "CelestialBody":
                    cfgNode.SetValue(property.Name, ((CelestialBody)property.GetValue(source, null)).name, true);
                    break;
                case "KerbalKonstructs.Core.SiteType":
                    cfgNode.SetValue(property.Name, ((SiteType)property.GetValue(source, null)).ToString(), true);
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
