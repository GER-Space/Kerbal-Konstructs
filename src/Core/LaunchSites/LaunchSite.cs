using System;
using UnityEngine;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.Core
{
    public class LaunchSite : KKFacility
    {
        //public string OpenCloseState = "Closed";
        //public float OpenCost;
        //public float CloseValue;

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
        public LaunchSiteCategory Category = LaunchSiteCategory.Other;
        [CFGSetting]
        public float LaunchSiteLength;
        [CFGSetting]
        public float LaunchSiteWidth;
        [CFGSetting]
        public bool LaunchSiteIsHidden = false;
        [CFGSetting]
        public bool ILSIsActive = false;

        [CareerSetting]
        public string favouriteSite = "No";
        [CareerSetting]
        public float MissionCount = 0f;
        [CareerSetting]
        public string MissionLog = "No Log";


        internal Texture logo = null;
        internal Texture icon = null;


        internal float refLon;
        internal float refLat;
        internal float refAlt;
        internal CelestialBody body;

        internal StaticInstance parentInstance;


        internal GameObject lsGameObject;
        internal PSystemSetup.SpaceCenterFacility facility = null;



        internal override KKFacility ParseConfig(ConfigNode node)
        {
            LaunchSite launchSite = base.ParseConfig(node) as LaunchSite;
            launchSite.lsGameObject = launchSite.gameObject;
            // this is might be slow
            launchSite.parentInstance = InstanceUtil.GetStaticInstanceForGameObject(lsGameObject);
            launchSite.body = parentInstance.CelestialBody;

            if ( ! string.IsNullOrEmpty(LaunchSiteLogo) )
            {
                logo = GameDatabase.Instance.GetTexture(LaunchSiteLogo, false);

                if (logo == null)
                    logo = GameDatabase.Instance.GetTexture(launchSite.parentInstance.model.path + "/" + launchSite.LaunchSiteLogo, false);
            }
            // use default logo
            if (logo == null)
                logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);

            if (! string.IsNullOrEmpty(LaunchSiteIcon))
            {
                
                icon = GameDatabase.Instance.GetTexture(LaunchSiteIcon, false);

                if (icon == null)
                    icon = GameDatabase.Instance.GetTexture(launchSite.parentInstance.model.path + "/" + launchSite.LaunchSiteIcon, false);
            }

            refLon = (float)Math.Round(KKMath.GetLongitudeInDeg(launchSite.parentInstance.RadialPosition), 2);
            refLat = (float)Math.Round(KKMath.GetLatitudeInDeg(launchSite.parentInstance.RadialPosition), 2);
          
            refAlt = launchSite.parentInstance.RadiusOffset;


            return launchSite;
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
