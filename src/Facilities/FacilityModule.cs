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
        [CareerSetting]
        public string OpenCloseState = "Closed";
        [CFGSetting]
        public float OpenCost;
        [CFGSetting]
        public float CloseValue;

        internal string facilityType; 

        private static Dictionary<string, FieldInfo> myFields;
        private static Dictionary<string, Dictionary<string, FieldInfo>> allFields = new Dictionary<string, Dictionary<string, FieldInfo>>();

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

            facilityType = this.GetType().Name;
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

            foreach (var field in myFields)
            {
                if (Attribute.IsDefined(field.Value, typeof(CareerSetting)))
                    ConfigUtil.Write2CfgNode(this, field.Value, cfgNode);
            }

        }

        internal virtual void LoadCareerConfig(ConfigNode cfgNode)
        {
            myFields = allFields[this.GetType().Name];

            foreach (var field in myFields.Values)
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

            foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                myFields.Add(field.Name, field);
                //Log.Normal("Parser Facility (" + this.GetType().Name + ") " + field.Name + ": " + field.FieldType.ToString());
            }

            allFields.Add(this.GetType().Name, myFields);
            initializedModules.Add(this.GetType().Name);
        }
    }
}
