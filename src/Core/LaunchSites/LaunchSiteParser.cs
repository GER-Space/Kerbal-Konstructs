using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace KerbalKonstructs.Core
{
    class LaunchSiteParser
    {
        private static bool initialized = false;

        private static Dictionary<string, FieldInfo> launchSiteFields;
        private static Dictionary<string, PropertyInfo> launchSiteProperties;



        internal static void ParseConfig(KKLaunchSite site, ConfigNode cfgNode)
        {
            if (!initialized)
            {
                InitTypes();
            }

            foreach (var field in launchSiteFields.Values)
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    ConfigUtil.ReadCFGNode(site, field, cfgNode);
                }
            }

            foreach (var field in launchSiteProperties.Values)
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    ConfigUtil.ReadCFGNode(site, field, cfgNode);
                }
                if (field.Name == "OpenCloseState")
                {
                    site.defaultState = site.openString;
                }
            }

            site.FacilityType = site.GetType().Name;
        }


        internal static void WriteConfig(KKLaunchSite site, ConfigNode cfgNode)
        {
            if (!initialized)
            {
                InitTypes();
            }

            // Close everything before saving.
            site.SetClosed();
            cfgNode.SetValue("FacilityType", site.GetType().Name, true);

            foreach (var field in launchSiteFields)
            {
                if (Attribute.IsDefined(field.Value, typeof(CFGSetting)))
                {
                    if (field.Value.GetValue(site) == null)
                    {
                        continue;
                    }

                    ConfigUtil.Write2CfgNode(site, field.Value, cfgNode);
                }
            }

            foreach (var field in launchSiteProperties)
            {
                if (field.Key == "OpenCloseState")
                {
                    cfgNode.SetValue("OpenCloseState", site.defaultState, true);
                    continue;
                }

                if (Attribute.IsDefined(field.Value, typeof(CFGSetting)))
                    ConfigUtil.Write2CfgNode(site, field.Value, cfgNode);
            }

        }

        internal static void SaveCareerConfig(KKLaunchSite site, ConfigNode cfgNode)
        {
            if (!initialized)
            {
                InitTypes();
            }

            foreach (var field in launchSiteFields)
            {
                if (Attribute.IsDefined(field.Value, typeof(CareerSetting)))
                {
                    ConfigUtil.Write2CfgNode(site, field.Value, cfgNode);
                }
            }

            foreach (var field in launchSiteProperties)
            {
                if (Attribute.IsDefined(field.Value, typeof(CareerSetting)))
                {
                    ConfigUtil.Write2CfgNode(site, field.Value, cfgNode);
                }
            }
        }

        internal static void LoadCareerConfig(KKLaunchSite site, ConfigNode cfgNode)
        {
            if (!initialized)
            {
                InitTypes();
            }

            foreach (var field in launchSiteFields.Values)
            {
                if (Attribute.IsDefined(field, typeof(CareerSetting)))
                {
                    ConfigUtil.ReadCFGNode(site, field, cfgNode);
                }
            }
            foreach (var field in launchSiteProperties.Values)
            {
                if (Attribute.IsDefined(field, typeof(CareerSetting)))
                {
                    ConfigUtil.ReadCFGNode(site, field, cfgNode);
                }
            }

        }

        private static void InitTypes()
        {
                launchSiteFields = new Dictionary<string, FieldInfo>();
                launchSiteProperties = new Dictionary<string, PropertyInfo>();

                foreach (FieldInfo field in typeof(KKLaunchSite).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    launchSiteFields.Add(field.Name, field);
                    //    Log.Normal("Parser Facility (" + site.GetType().Name + ") " + field.Name + ": " + field.FieldType.ToString());
                }

                foreach (PropertyInfo property in typeof(KKLaunchSite).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    launchSiteProperties.Add(property.Name, property);
                    //   Log.Normal("Parser Facility (" + site.GetType().Name + ") " + property.Name + ": " + property.PropertyType.ToString());
                }

            
            initialized = true;
        }


    }
}
