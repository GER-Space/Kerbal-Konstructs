using System;
using UnityEngine;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.Core
{
    public class LaunchSite
    {
        public float OpenCost;
        public float CloseValue ;
        public string OpenCloseState = "Closed";

        [CFGSetting]
        public string LaunchSiteName;
        [CFGSetting]
        public string LaunchSiteAuthor;
        [CFGSetting]
        public SiteType LaunchSiteType;
        [CFGSetting]
        public Texture LaunchSiteLogo = null;
        [CFGSetting]
        public Texture LaunchSiteIcon = null;
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



        public float refLon;
        public float refLat;
        public float refAlt;
        public CelestialBody body;

        public string nation = "United Kerbin";

        public GameObject lsGameObject;
        public PSystemSetup.SpaceCenterFacility facility = null;



        public LaunchSite()
        {
        }

        /// <summary>
        /// Creates an Launchsite out of an StaticInstance (and its settings)
        /// </summary>
        /// <param name="instance"></param>
        public LaunchSite(StaticObject instance, PSystemSetup.SpaceCenterFacility newFacility)
        {

            if (instance.settings.ContainsKey("LaunchSiteLogo"))
            {
                string sLogoPath = (string)instance.getSetting("LaunchSiteLogo");
                LaunchSiteLogo = GameDatabase.Instance.GetTexture(sLogoPath, false);

                if (LaunchSiteLogo == null)
                    LaunchSiteLogo = GameDatabase.Instance.GetTexture(instance.model.path + "/" + instance.getSetting("LaunchSiteLogo"), false);
            }
            // use default logo
            if (LaunchSiteLogo == null)
                LaunchSiteLogo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);

            if (instance.settings.ContainsKey("LaunchSiteIcon"))
            {
                string sIconPath = (string)instance.getSetting("LaunchSiteIcon");
                LaunchSiteIcon = GameDatabase.Instance.GetTexture(sIconPath, false);

                if (LaunchSiteIcon == null)
                    LaunchSiteIcon = GameDatabase.Instance.GetTexture(instance.model.path + "/" + instance.getSetting("LaunchSiteIcon"), false);
            }

            this.LaunchSiteName = (string)instance.getSetting("LaunchSiteName");
            this.LaunchSiteAuthor = (instance.settings.ContainsKey("LaunchSiteAuthor")) ? (string)instance.getSetting("LaunchSiteAuthor") : instance.model.author;
            this.LaunchSiteType = (SiteType)instance.getSetting("LaunchSiteType");
            this.LaunchSiteDescription = (string)instance.getSetting("LaunchSiteDescription");
            this.Category = (string)instance.getSetting("Category");
            this.OpenCost = (float)instance.getSetting("OpenCost");
            this.CloseValue = (float)instance.getSetting("CloseValue");
            this.body = instance.body;
            this.refLon = (float)Math.Round((NavUtils.GetLongitude(instance.pqsCity.repositionRadial) * KKMath.rad2deg), 2);
            this.refLat = (float)Math.Round((NavUtils.GetLatitude(instance.pqsCity.repositionRadial) * KKMath.rad2deg), 2);
            this.refAlt = (float)instance.getSetting("RadiusOffset");
            this.LaunchSiteLength = (instance.settings.ContainsKey("LaunchSiteLength")) ?
                                    (float)instance.getSetting("LaunchSiteLength") : (float)instance.model.DefaultLaunchSiteLength;
            this.LaunchSiteWidth = (instance.settings.ContainsKey("LaunchSiteWidth")) ?
                                    (float)instance.getSetting("LaunchSiteWidth") : (float)instance.model.DefaultLaunchSiteWidth;
            this.LaunchRefund = (float)instance.getSetting("LaunchRefund");
            this.RecoveryFactor = (float)instance.getSetting("RecoveryFactor");
            this.RecoveryRange = (float)instance.getSetting("RecoveryRange");
            this.lsGameObject = instance.gameObject;
            this.facility = newFacility;
            this.MissionLog = "No log";

            if (string.Equals((string)instance.getSetting("LaunchSiteIsHidden"), "true", StringComparison.CurrentCultureIgnoreCase))
                this.LaunchSiteIsHidden = true;

        }
    }

    public enum SiteType
    {
        VAB,
        SPH,
        Any
    }
}
