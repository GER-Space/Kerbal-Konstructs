using System;
using UnityEngine;
using KerbalKonstructs.API;
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
        public string LaunchSiteName;
        [CFGSetting]
        public string LaunchSiteAuthor;
        [CFGSetting]
        public SiteType LaunchSiteType;
        [CFGSetting]
        public string LaunchSiteLogo = null;
        [CFGSetting]
        public string LaunchSiteIcon = null;
        [CFGSetting]
        public string LaunchSiteDescription;
        [CFGSetting]
        public string Category;
        [CFGSetting]
        public float LaunchSiteLength;
        [CFGSetting]
        public float LaunchSiteWidth;
        [CFGSetting]
        public float LaunchRefund = 0f;
        [CFGSetting]
        public float RecoveryFactor = 0f;
        [CFGSetting]
        public float RecoveryRange = 0f;
        [CFGSetting]
        public bool LaunchSiteIsHidden = false;

        [CareerSetting]
        public string favouriteSite = "No";
        [CareerSetting]
        public float MissionCount = 0f;
        [CareerSetting]
        public string MissionLog = "No Log";


        public Texture logo = null;
        public Texture icon = null;


        public float refLon;
        public float refLat;
        public float refAlt;
        public CelestialBody body;

        internal StaticObject parentInstance;


        public GameObject lsGameObject;
        public PSystemSetup.SpaceCenterFacility facility = null;



        public LaunchSite()
        {
        }

        ///// <summary>
        ///// Creates an Launchsite out of an StaticInstance (and its settings)
        ///// </summary>
        ///// <param name="instance"></param>
        //public LaunchSite(StaticObject instance, PSystemSetup.SpaceCenterFacility newFacility)
        //{

        //    if (instance.settings.ContainsKey("LaunchSiteLogo"))
        //    {
        //        string sLogoPath = (string)instance.getSetting("LaunchSiteLogo");
        //        logo = GameDatabase.Instance.GetTexture(sLogoPath, false);

        //        if (logo == null)
        //            logo = GameDatabase.Instance.GetTexture(instance.model.path + "/" + instance.getSetting("LaunchSiteLogo"), false);
        //    }
        //    // use default logo
        //    if (logo == null)
        //        logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);

        //    if (instance.settings.ContainsKey("LaunchSiteIcon"))
        //    {
        //        string sIconPath = (string)instance.getSetting("LaunchSiteIcon");
        //        icon = GameDatabase.Instance.GetTexture(sIconPath, false);

        //        if (icon == null)
        //            icon = GameDatabase.Instance.GetTexture(instance.model.path + "/" + instance.getSetting("LaunchSiteIcon"), false);
        //    }

        //    this.LaunchSiteName = (string)instance.getSetting("LaunchSiteName");
        //    this.LaunchSiteAuthor = (instance.settings.ContainsKey("LaunchSiteAuthor")) ? (string)instance.getSetting("LaunchSiteAuthor") : instance.model.author;
        //    this.LaunchSiteType = (SiteType)instance.getSetting("LaunchSiteType");

        //    this.LaunchSiteDescription = (string)instance.getSetting("LaunchSiteDescription");
        //    this.Category = (string)instance.getSetting("Category");
        //    this.OpenCost = (float)instance.getSetting("OpenCost");
        //    this.CloseValue = (float)instance.getSetting("CloseValue");
        //    this.body = instance.CelestialBody;
        //    this.refLon = (float)Math.Round((NavUtils.GetLongitude(instance.pqsCity.repositionRadial) * KKMath.rad2deg), 4);
        //    this.refLat = (float)Math.Round((NavUtils.GetLatitude(instance.pqsCity.repositionRadial) * KKMath.rad2deg), 4);
        //    this.refAlt = (float)instance.getSetting("RadiusOffset");
        //    this.LaunchSiteLength = (instance.settings.ContainsKey("LaunchSiteLength")) ?
        //                            (float)instance.getSetting("LaunchSiteLength") : (float)instance.model.DefaultLaunchSiteLength;
        //    this.LaunchSiteWidth = (instance.settings.ContainsKey("LaunchSiteWidth")) ?
        //                            (float)instance.getSetting("LaunchSiteWidth") : (float)instance.model.DefaultLaunchSiteWidth;
        //    this.LaunchRefund = (float)instance.getSetting("LaunchRefund");
        //    this.RecoveryFactor = (float)instance.getSetting("RecoveryFactor");
        //    this.RecoveryRange = (float)instance.getSetting("RecoveryRange");
        //    this.lsGameObject = instance.gameObject;
        //    this.facility = newFacility;
        //    this.MissionLog = "No log";

        //    if (string.Equals((string)instance.getSetting("LaunchSiteIsHidden"), "true", StringComparison.CurrentCultureIgnoreCase))
        //        this.LaunchSiteIsHidden = true;

        //}

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


            refLon = (float)Math.Round((NavUtils.GetLongitude(launchSite.parentInstance.pqsCity.repositionRadial) * KKMath.rad2deg), 2);
            refLat = (float)Math.Round((NavUtils.GetLatitude(launchSite.parentInstance.pqsCity.repositionRadial) * KKMath.rad2deg), 2);
          
            refAlt = (float)launchSite.parentInstance.RadiusOffset;


            return launchSite;
        }


    }


    public enum SiteType
    {
        VAB,
        SPH,
        Any
    }
}
