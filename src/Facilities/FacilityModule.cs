using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using KerbalKonstructs.Core;
using KerbalKonstructs.UI;

namespace KerbalKonstructs.Modules
{

    public enum KKFacilityType 
    {
        None,
        Merchant,
        TrackingStation,
        GroundStation,
        FuelTanks,
        Hangar,
        Barracks,
        Research,
        Business,
        LandingGuide,
        TouchdownGuideL,
        TouchdownGuideR,
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
        [CFGSetting]
        public string FacilityName = "";

        [CareerSetting]
        public bool isOpen
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
        internal string defaultState = "Closed";

        private bool initialized = false;

        private static Dictionary<string, FieldInfo> myFields;
        private static Dictionary<string, PropertyInfo> myProperties;

        private static Dictionary<string, Dictionary<string, FieldInfo>> allFields = new Dictionary<string, Dictionary<string, FieldInfo>>();
        private static Dictionary<string, Dictionary<string, PropertyInfo>> allProperties = new Dictionary<string, Dictionary<string, PropertyInfo>>();

        private static HashSet<string> initializedModules = new HashSet<string>();

        private List<KKFacilitySelector> facSelector = new List<KKFacilitySelector>();

        private StaticInstance _instance = null;

        internal StaticInstance staticInstance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
                foreach (var fac in facSelector)
                {
                    fac.staticInstance = value;
                }

                
            }
        }

        // clean up the facSelector
        public void OnDestroy()
        {
            foreach (var fac in facSelector)
            {
                Destroy(fac);
            }
            //Destroy(this);
        }


        public void Awake()
        {
            DontDestroyOnLoad(this);
        }

        internal virtual KKFacility ParseConfig(ConfigNode cfgNode)
        {
            if(!initialized)
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

            foreach (var field in myProperties.Values)
            {
                if (Attribute.IsDefined(field, typeof(CFGSetting)))
                {
                    ConfigUtil.ReadCFGNode(this, field, cfgNode);
                }
                if (field.Name == "OpenCloseState")
                {
                    // openString should be set via OpenCloseState
                    defaultState = openString; 
                }
            }

            FacilityType = this.GetType().Name;

            AttachSelector();
            return this;
        }


        internal virtual void WriteConfig(ConfigNode cfgNode)
        {
            if (!initialized)
            {
                InitTypes();
            }

            // Close everything before saving.
            isOpen = false;
            myFields = allFields[this.GetType().Name];
            cfgNode.SetValue("FacilityType", this.GetType().Name,true);

            foreach (var field in myFields)
            {
                if (Attribute.IsDefined(field.Value, typeof(CFGSetting)))
                {
                    if (field.Value.GetValue(this) == null)
                    {
                        continue;
                    }

                    ConfigUtil.Write2CfgNode(this, field.Value, cfgNode);
                }
            }

            foreach (var field in myProperties)
            {
                if (field.Key == "OpenCloseState")
                {
                    cfgNode.SetValue("OpenCloseState", defaultState, true);
                    continue;
                } 

                if (Attribute.IsDefined(field.Value, typeof(CFGSetting)))
                    ConfigUtil.Write2CfgNode(this, field.Value, cfgNode);
            }

        }

        internal virtual void SaveCareerConfig(ConfigNode cfgNode)
        {
            if (!initialized)
            {
                InitTypes();
            }

            myFields = allFields[this.GetType().Name];
            myProperties = allProperties[this.GetType().Name];

            foreach (var field in myFields)
            {
                if (Attribute.IsDefined(field.Value, typeof(CareerSetting)))
                {
                    ConfigUtil.Write2CfgNode(this, field.Value, cfgNode);
                }
            }

            foreach (var field in myProperties)
            {
                if (Attribute.IsDefined(field.Value, typeof(CareerSetting)))
                {
                    ConfigUtil.Write2CfgNode(this, field.Value, cfgNode);
                }
            }
        }

        internal virtual void LoadCareerConfig(ConfigNode cfgNode)
        {
            if (!initialized)
            {
                InitTypes();
            }

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

            if (!initializedModules.Contains(this.GetType().Name))
            {

                myFields = new Dictionary<string, FieldInfo>();
                myProperties = new Dictionary<string, PropertyInfo>();

                foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    myFields.Add(field.Name, field);
                    //    Log.Normal("Parser Facility (" + this.GetType().Name + ") " + field.Name + ": " + field.FieldType.ToString());
                }

                foreach (PropertyInfo property in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    myProperties.Add(property.Name, property);
                    //   Log.Normal("Parser Facility (" + this.GetType().Name + ") " + property.Name + ": " + property.PropertyType.ToString());
                }

                allFields.Add(this.GetType().Name, myFields);
                allProperties.Add(this.GetType().Name, myProperties);
                initializedModules.Add(this.GetType().Name);
            }
            initialized = true;
        }

        internal void AttachSelector()
        {
            Type myType = this.GetType();
            if (myType.Name == "LaunchSite"  || myType.Name == "LandingGuide" || myType.Name == "TouchdownGuideL" || myType.Name == "TouchdownGuideR"  )
            {
                Log.Normal("Skipping facility mouse support for:" + myType.Name);
                return;
            }

            foreach (Collider colloder in gameObject.GetComponentsInChildren<Collider>(true).Where(col => col.isTrigger == false))
            {
                facSelector.Add(colloder.gameObject.AddComponent<KKFacilitySelector>());
            }
        }

    }

    internal class KKFacilitySelector : MonoBehaviour
    {
        // we get this passed through the facility module
        internal StaticInstance staticInstance = null;

        public void Awake()
        {
                DontDestroyOnLoad(this);
        }


        #region Unity mouse extension

        void OnMouseDown()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                //Log.Normal("Mouse pressed on: " + staticInstance.gameObject.name);
                //Log.Normal("Open Facility Window");
                FacilityManager.selectedInstance = staticInstance;
                FacilityManager.instance.Open();

            }
        }

        void OnMouseEnter()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                if (staticInstance.myFacilities[0].isOpen)
                {
                    staticInstance.HighlightObject(new Color(0.4f, 0.9f , 0.4f, 0.5f));
                }
                else
                {
                    staticInstance.HighlightObject(new Color(0.9f, 0.4f, 0.4f, 0.5f));
                }
                    
            }
        }

        void OnMouseExit()
        {
            if (HighLogic.LoadedScene == GameScenes.FLIGHT)
            {
                staticInstance.HighlightObject(Color.clear);
            }
        }
        #endregion

        // clean up the facSelector
        //public void OnDestroy()
        //{
        //    Log.Normal("Facility Selector Destroyed");
        //}

    }

}
