using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            set
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
        public bool LaunchSiteIsHidden
        {
            get
            {
                return (_isHidden && !WasSeen && !openState  && ! KerbalKonstructs.instance.discoverAllBases);
            }
            set 
            {
                _isHidden = value;
            }
        }

        private bool _isHidden = false;

        //[CFGSetting]
        public bool WasSeen = false;

        [CFGSetting]
        public bool ILSIsActive = false;
        [CFGSetting]
        public bool ToggleLaunchPositioning = false;

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
                LaunchSiteCategory tmpCat;
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
                if ((staticInstance.destructible != null && staticInstance.destructible.IsDestroyed) || staticInstance.isDestroyed)
                {
                    return false;
                }

                if (KerbalKonstructs.instance.disableCareerStrategyLayer)
                {
                    return true;
                }

                if (openState == true || openString == "Open" )
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
                    WasSeen = true;
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

        public StaticInstance staticInstance = null;

        internal Texture logo = null;
        internal Texture icon = null;

        public LaunchSiteCategory sitecategory = LaunchSiteCategory.Other;

        public float refLon;
        public float refLat;
        public float refAlt;
        public CelestialBody body;

        internal PSystemSetup.SpaceCenterFacility spaceCenterFacility = null;

        private List<KKLaunchSiteSelector> facSelector = new List<KKLaunchSiteSelector>();

        internal bool isSquad = false;

        internal FinePrint.Waypoint wayPoint = null;

        public void ParseLSConfig(StaticInstance instance, ConfigNode cfgNode)
        {
            staticInstance = instance;
            body = staticInstance.CelestialBody;

            if (cfgNode != null)
            {
                LaunchSiteParser.ParseConfig(this, cfgNode);
            }




            if (!string.IsNullOrEmpty(LaunchSiteLogo))
            {
                logo = GameDatabase.Instance.GetTexture(LaunchSiteLogo, false);

                if (logo == null)
                {
                    logo = GameDatabase.Instance.GetTexture(staticInstance.model.path + "/" + LaunchSiteLogo, false);
                }
            }
            // use default logo
            if (logo == null)
            {
                logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);
            }

            if (!string.IsNullOrEmpty(LaunchSiteIcon))
            {

                icon = GameDatabase.Instance.GetTexture(LaunchSiteIcon, false);
                if (icon == null)
                {
                    icon = GameDatabase.Instance.GetTexture(staticInstance.model.path + "/" + LaunchSiteIcon, false);
                }
            }

            refLon = (float)Math.Round(KKMath.GetLongitudeInDeg(staticInstance.RadialPosition), 2);
            refLat = (float)Math.Round(KKMath.GetLatitudeInDeg(staticInstance.RadialPosition), 2);

            refAlt = (float)staticInstance.CelestialBody.GetAltitude(staticInstance.position);

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
        internal void ResetToDefaultState()
        {
            WasSeen = false;
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
            facSelector.Clear();
            foreach (Collider colloder in staticInstance.mesh.GetComponentsInChildren<Collider>(true).Where(col => col.isTrigger == false))
            {
                KKLaunchSiteSelector selector = colloder.gameObject.AddComponent<KKLaunchSiteSelector>();
                selector.staticInstance = staticInstance;
                facSelector.Add(selector);
                selector.enabled = true;

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
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && !InputLockManager.IsLocked(ControlTypes.KSC_FACILITIES))
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
