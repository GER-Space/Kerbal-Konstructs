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
    public class LaunchSite
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
        public float LaunchSiteLength;
        [CFGSetting]
        public float LaunchSiteWidth;
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
                if (openState == true || openString == "Open" || CareerUtils.isSandboxGame || OpenCost == 0f)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            private set
            {
                openState = value;
                if (openState == true)
                {
                    OpenCloseState = "Open";
                }
                else
                {
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
        internal PSystemSetup.SpaceCenterFacility facility = null;


        internal void ParseLSConfig(StaticInstance instance, ConfigNode cfgNode)
        {
            if (cfgNode != null)
            {
               LaunchSiteParser.ParseConfig(this,cfgNode);
            }

            lsGameObject = instance.gameObject;
            // this is might be slow
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

            refAlt = staticInstance.RadiusOffset;

        }



        /// <summary>
        /// Open the faclility/Launchsite
        /// </summary>
        internal void SetOpen()
        {
            LaunchSiteManager.OpenLaunchSite(this);
            isOpen = true;
        }

        /// <summary>
        /// Close a facility/LaunchSite
        /// </summary>
        internal void SetClosed()
        {
            LaunchSiteManager.CloseLaunchSite(this);
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
