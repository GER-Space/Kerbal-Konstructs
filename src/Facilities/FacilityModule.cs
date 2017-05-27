using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{

    public enum KKFacilityType 
    {
        None,
        TrackingStation,
        GroundStation,
        FuelTanks,
        Hangar,
        Barracks,
        RadarStation,
        Research,
        Business,
        LandingGuide,
        TouchdownGuideL,
        TouchdownGuideR
    } 



    public abstract class KKFacility : MonoBehaviour
    {
        //use the attribute to select wich setting is read d

        [CFGSetting]
        public float OpenCost = 0f;
        [CFGSetting]
        public float CloseValue = 0f;
        [CFGSetting]
        internal string FacilityType;

        [CareerSetting]
        public  bool isOpen
        {
            get
            {
                if (openState == true || openString == "Open" || CareerUtils.isSandboxGame || OpenCost == 0f)
                {
                    return true;
                } else
                {
                    return false;
                }
            }
            set
            {
                openState = value;
                if (openState == true)
                {
                    OpenCloseState = "Open";
                } else
                {
                    OpenCloseState = "Closed";
                }
            }
        }


        [CFGSetting]
        public string OpenCloseState
        {
            get {
                if (isOpen)
                {
                    return "Open";
                }
                else
                {
                    return "Closed";
                }
            }
            set {
                openString = value;
                if (value == "Open")
                {
                    openState = true;
                } else
                {
                    openState = false;
                }

            }
        }


        private bool openState = false;
        private string openString = "Closed";

        private static Dictionary<string, FieldInfo> myFields;
        private static Dictionary<string, PropertyInfo> myProperties;

        private static Dictionary<string, Dictionary<string, FieldInfo>> allFields = new Dictionary<string, Dictionary<string, FieldInfo>>();
        private static Dictionary<string, Dictionary<string, PropertyInfo>> allProperties = new Dictionary<string, Dictionary<string, PropertyInfo>>();

        private static HashSet<string> initializedModules = new HashSet<string>();




        internal virtual KKFacility ParseConfig(ConfigNode cfgNode)
        {
            if(!initializedModules.Contains(this.GetType().Name))
            {
                InitTypes();
            }

            myFields = allFields[this.GetType().Name];

            foreach (var field in myFields.Values)
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    ConfigUtil.ReadCFGNode(this, field, cfgNode);
                }
            }

            FacilityType = this.GetType().Name;
            return this;
        }


        internal virtual void WriteConfig(ConfigNode cfgNode)
        {

            myFields = allFields[this.GetType().Name];
            cfgNode.SetValue("FacilityType", this.GetType().Name,true);

            foreach (var field in myFields)
            {
                if (Attribute.IsDefined(field.Value, typeof(CFGSetting)))
                {
                    if (field.Value.GetValue(this) == null)
                        continue;
                    ConfigUtil.Write2CfgNode(this, field.Value, cfgNode);

                }
            }
        }

        internal virtual void SaveCareerConfig(ConfigNode cfgNode)
        {
            myFields = allFields[this.GetType().Name];
            myProperties = allProperties[this.GetType().Name];

            foreach (var field in myFields)
            {
                if (Attribute.IsDefined(field.Value, typeof(CareerSetting)))
                    ConfigUtil.Write2CfgNode(this, field.Value, cfgNode);
            }

            foreach (var field in myProperties)
            {
                if (Attribute.IsDefined(field.Value, typeof(CareerSetting)))
                    ConfigUtil.Write2CfgNode(this, field.Value, cfgNode);
            }

        }

        internal virtual void LoadCareerConfig(ConfigNode cfgNode)
        {
            myFields = allFields[this.GetType().Name];
            myProperties = allProperties[this.GetType().Name];

            foreach (var field in myFields.Values)
            {
                if (Attribute.IsDefined(field, typeof(CareerSetting)))
                {
                    ConfigUtil.ReadCFGNode(this, field, cfgNode);
                }
            }
            foreach (var field in myProperties.Values)
            {
                if (Attribute.IsDefined(field, typeof(CareerSetting)))
                {
                    ConfigUtil.ReadCFGNode(this, field, cfgNode);
                }
            }

        }


        private void InitTypes()
        {
            myFields = new Dictionary<string, FieldInfo>();
            myProperties = new Dictionary<string, PropertyInfo>();

            foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                myFields.Add(field.Name, field);
                Log.Normal("Parser Facility (" + this.GetType().Name + ") " + field.Name + ": " + field.FieldType.ToString());
            }

            foreach (PropertyInfo property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                myProperties.Add(property.Name, property);
                Log.Normal("Parser Facility (" + this.GetType().Name + ") " + property.Name + ": " + property.PropertyType.ToString());
            }

            allFields.Add(this.GetType().Name, myFields);
            allProperties.Add(this.GetType().Name, myProperties);
            initializedModules.Add(this.GetType().Name);
        }
    }
}
