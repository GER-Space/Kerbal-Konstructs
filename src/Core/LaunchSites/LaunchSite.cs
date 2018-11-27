using System;
using UnityEngine;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Modules;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using KerbalKonstructs.UI;

namespace KerbalKonstructs.Core
{



    public class KKLaunchSite
    {
        [CFGSetting]
        public float OpenCost = 0f;
        [CFGSetting]
        public float CloseValue = 0f;
        [CFGSetting]
        internal string FacilityType;
        [CFGSetting]
        public string FacilityName = "";
        [CFGSetting]
        public string OpenCloseState
        {
            get
            {
                if (isOpen)
                {
                    return "Open";
                }
                else
                {
                    return "Closed";
                }
            }
            private set
            {
                openString = value;
                if (value == "Open")
                {
                    openState = true;
                }
                else
                {
                    openState = false;
                }

            }
        }
        [CFGSetting]
        public string LaunchPadTransform;
        [CFGSetting]
        public string LaunchSiteName = "";
        [CFGSetting]
        public string LaunchSiteAuthor = "";
        [CFGSetting]
        public SiteType LaunchSiteType = SiteType.Any;
        [CFGSetting]
        public string LaunchSiteLogo = null;
        [CFGSetting]
        public string LaunchSiteIcon = null;
        [CFGSetting]
        public string LaunchSiteDescription;
        [CFGSetting]
        public float LaunchSiteLength = 0f;
        [CFGSetting]
        public float LaunchSiteWidth = 0f;
        [CFGSetting]
        public float LaunchSiteHeight = 0f;
        [CFGSetting]
        public float MaxCraftMass = 0f;
        [CFGSetting]
        public int MaxCraftParts = 0;
        [CFGSetting]
        public float InitialCameraRotation = 90f;
        [CFGSetting]
        public bool LaunchSiteIsHidden = false;
        [CFGSetting]
        public bool ILSIsActive = false;
        // legacy API for KTC (needed for KSP 1.2.2 & 1.3.1)
        [CFGSetting]
        public string Category
        {
            get
            {
                return sitecategory.ToString();
            }
            set
            {
                LaunchSiteCategory tmpCat = LaunchSiteCategory.Other;
                try
                {
                    tmpCat = (LaunchSiteCategory)Enum.Parse(typeof(LaunchSiteCategory), value);

                }
                catch
                {
                    tmpCat = LaunchSiteCategory.Other;
                }
                sitecategory = tmpCat;

            }
        }

        [CareerSetting]
        public bool isOpen
        {
            get
            {
                if (staticInstance.destructible != null && staticInstance.destructible.IsDestroyed)
                {
                    return false;
                }

                if (openState == true || openString == "Open" || CareerUtils.isSandboxGame || OpenCost == 0f )
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                openState = value;
                if (openState == true)
                {
                    //LaunchSiteManager.OpenLaunchSite(this);
                    OpenCloseState = "Open";
                }
                else
                {
                    //LaunchSiteManager.CloseLaunchSite(this);
                    OpenCloseState = "Closed";
                }
            }
        }
        [CareerSetting]
        public string favouriteSite = "No";
        [CareerSetting]
        public float MissionCount = 0f;
        [CareerSetting]
        public string MissionLog = "No Log";



        private bool openState = false;
        internal string openString = "Closed";
        internal string defaultState = "Closed";

        internal StaticInstance staticInstance = null;

        internal Texture logo = null;
        internal Texture icon = null;

        internal LaunchSiteCategory sitecategory = LaunchSiteCategory.Other;

        internal float refLon;
        internal float refLat;
        internal float refAlt;
        internal CelestialBody body;

        internal GameObject lsGameObject;
        internal PSystemSetup.SpaceCenterFacility spaceCenterFacility = null;

        private List<KKLaunchSiteSelector> facSelector = new List<KKLaunchSiteSelector>();

        internal bool isSquad = false;

        internal FinePrint.Waypoint wayPoint = null;

        internal void ParseLSConfig(StaticInstance instance, ConfigNode cfgNode)
        {
            if (cfgNode != null)
            {
               LaunchSiteParser.ParseConfig(this,cfgNode);
            }

            lsGameObject = instance.mesh;
            staticInstance = instance;
            body = staticInstance.CelestialBody;

            if (!string.IsNullOrEmpty(LaunchSiteLogo))
            {
                logo = GameDatabase.Instance.GetTexture(LaunchSiteLogo, false);

                if (logo == null)
                    logo = GameDatabase.Instance.GetTexture(staticInstance.model.path + "/" + LaunchSiteLogo, false);
            }
            // use default logo
            if (logo == null)
                logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);

            if (!string.IsNullOrEmpty(LaunchSiteIcon))
            {

                icon = GameDatabase.Instance.GetTexture(LaunchSiteIcon, false);

                if (icon == null)
                    icon = GameDatabase.Instance.GetTexture(staticInstance.model.path + "/" + LaunchSiteIcon, false);
            }

            refLon = (float)Math.Round(KKMath.GetLongitudeInDeg(staticInstance.RadialPosition), 2);
            refLat = (float)Math.Round(KKMath.GetLatitudeInDeg(staticInstance.RadialPosition), 2);

            refAlt = (float)staticInstance.CelestialBody.GetAltitude(staticInstance.position);
            
            AttachSelector();

        }



        /// <summary>
        /// Open the faclility/Launchsite
        /// </summary>
        internal void SetOpen()
        {
            isOpen = true;
        }

        /// <summary>
        /// Close a facility/LaunchSite
        /// </summary>
        internal void SetClosed()
        {
            isOpen = false;
        }

        // Resets the facility/LaunchSite to its default state
        internal  void ResetToDefaultState()
        {
            if (OpenCloseState != defaultState)
            {
                if (isOpen)
                {
                    SetClosed();
                }
                else
                {
                    SetOpen();
                }
            }
        }

        /// <summary>
        /// Attaches the MouseSelector to a LaunchSite
        /// </summary>
        internal void AttachSelector()
        {
            foreach (Collider colloder in lsGameObject.GetComponentsInChildren<Collider>(true).Where(col => col.isTrigger == false))
            {
                KKLaunchSiteSelector selector = colloder.gameObject.AddComponent<KKLaunchSiteSelector>();
                selector.staticInstance = staticInstance;
                facSelector.Add(selector);
            }
        }
    }

    internal class KKLaunchSiteSelector : MonoBehaviour
    {
        // we get this passed through the facility module
        internal StaticInstance staticInstance = null;

        #region Unity mouse extension

        void OnMouseDown()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && ! InputLockManager.IsLocked(ControlTypes.KSC_FACILITIES))
            {
                EditorFacility facility;
                staticInstance.HighlightObject(Color.clear);

                if (staticInstance.launchSite.LaunchSiteType == SiteType.VAB)
                {
                    facility = EditorFacility.VAB;
                }
                else
                {
                    facility = EditorFacility.SPH;
                }
                

                EditorDriver.StartupBehaviour = EditorDriver.StartupBehaviours.START_CLEAN;
                EditorDriver.StartEditor(facility);
            }
        }

        void OnMouseEnter()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            {
                try
                {
                    if (this.gameObject == null)
                    {
                        Destroy(this);
                    }
                    if (staticInstance == null)
                    {
                        staticInstance = InstanceUtil.GetStaticInstanceForGameObject(this.gameObject);
                    }
                    if (staticInstance == null)
                    {
                        Log.UserInfo("Cound not determin instance for mouse selector");
                        Destroy(this);
                    }

                    if (staticInstance.launchSite.isOpen)
                    {
                        staticInstance.HighlightObject(new Color(0.4f, 0.9f, 0.4f, 0.5f));
                    }
                    else
                    {
                        staticInstance.HighlightObject(new Color(0.9f, 0.4f, 0.4f, 0.5f));
                    }
                }
                catch
                {
                    Destroy(this);
                }


            }
        }

        void OnMouseExit()
        {
            //if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
            //{
                staticInstance.HighlightObject(Color.clear);
            //}
        }
        #endregion


    }


    public enum LaunchSiteCategory
    {
        Runway,
        RocketPad,
        Helipad,
        Waterlaunch,
        Other
    }

    public enum SiteType
    {
        VAB,
        SPH,
        Any
    }
}
