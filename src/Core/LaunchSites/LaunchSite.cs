using System;
using UnityEngine;
using KerbalKonstructs.API;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.Core
{
    public class LaunchSite 
    {
        public string LaunchSiteName;

        public string author;
        public SiteType type;
        public Texture logo = null;
        public Texture icon = null;
        public string description;

        public string category;
        public float openCost = 0f;
        public float closeValue = 0f;

        public string openCloseState = "Closed";
        public string favouriteSite = "No";
        public float missionCount = 0f;
        public string missionLog = "Too busy to keep this log. Signed Gene Kerman.";

        public float refLon;
        public float refLat;
        public float refAlt;
        public float siteLength;
        public float siteWidth;
        public float launchRefund = 0f;
        public float recoveryFactor = 0f;
        public float recoveryRange = 0f;
        public CelestialBody body;

        public string nation = "United Kerbin";

        public GameObject gameObject;
        public PSystemSetup.SpaceCenterFacility facility = null;

        internal bool isHidden = false;

        public LaunchSite()
        {
        }

        /// <summary>
        /// Creates an Launchsite out of an StaticInstance (and its settings)
        /// </summary>
        /// <param name="instance"></param>
        public LaunchSite(StaticObject instance, PSystemSetup.SpaceCenterFacility newFacility)
        {
            Texture logo = null;
            Texture icon = null;

            if (instance.settings.ContainsKey("LaunchSiteLogo"))
            {
                string sLogoPath = (string)instance.getSetting("LaunchSiteLogo");
                logo = GameDatabase.Instance.GetTexture(sLogoPath, false);

                if (logo == null)
                    logo = GameDatabase.Instance.GetTexture(instance.model.path + "/" + instance.getSetting("LaunchSiteLogo"), false);
            }
            // use default logo
            if (logo == null)
                logo = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/DefaultSiteLogo", false);

            if (instance.settings.ContainsKey("LaunchSiteIcon"))
            {
                string sIconPath = (string)instance.getSetting("LaunchSiteIcon");
                icon = GameDatabase.Instance.GetTexture(sIconPath, false);

                if (icon == null)
                    icon = GameDatabase.Instance.GetTexture(instance.model.path + "/" + instance.getSetting("LaunchSiteIcon"), false);
            }

            this.LaunchSiteName = (string)instance.getSetting("LaunchSiteName");
            this.author = (instance.settings.ContainsKey("LaunchSiteAuthor")) ? (string)instance.getSetting("LaunchSiteAuthor") : instance.model.author;
            this.type = (SiteType)instance.getSetting("LaunchSiteType");
            this.logo = logo;
            this.icon = icon;
            this.description = (string)instance.getSetting("LaunchSiteDescription");
            this.category = (string)instance.getSetting("Category");
            this.openCost = (float)instance.getSetting("OpenCost");
            this.closeValue = (float)instance.getSetting("CloseValue");
            this.body = instance.body;
            this.refLon = (float)Math.Round((NavUtils.GetLongitude(instance.pqsCity.repositionRadial) * KKMath.rad2deg), 2);
            this.refLat = (float)Math.Round((NavUtils.GetLatitude(instance.pqsCity.repositionRadial) * KKMath.rad2deg), 2);
            this.refAlt = (float)instance.getSetting("RadiusOffset");
            this.siteLength = (instance.settings.ContainsKey("LaunchSiteLength")) ?
                                    (float)instance.getSetting("LaunchSiteLength") : (float)instance.model.DefaultLaunchSiteLength;
            this.siteWidth = (instance.settings.ContainsKey("LaunchSiteWidth")) ?
                                    (float)instance.getSetting("LaunchSiteWidth") : (float)instance.model.DefaultLaunchSiteWidth;
            this.launchRefund = (float)instance.getSetting("LaunchRefund");
            this.recoveryFactor = (float)instance.getSetting("RecoveryFactor");
            this.recoveryRange = (float)instance.getSetting("RecoveryRange");
            this.gameObject = instance.gameObject;
            this.facility = newFacility;
            this.missionLog = "No log";

            if (string.Equals((string)instance.getSetting("LaunchSiteIsHidden"), "true", StringComparison.CurrentCultureIgnoreCase))
                this.isHidden = true;

        }
    }

    public enum SiteType
    {
        VAB,
        SPH,
        Any
    }
}
