using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KerbalKonstructs;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.Core
{
    class LaunchSiteButton : MonoBehaviour
    {
        public string siteName;


        public void LauchVessel()
        {
            LaunchSiteManager.setLaunchSite(LaunchSiteManager.GetLaunchSiteByName(siteName));
            EditorLogic.fetch.launchVessel();
        }

        public void SetDefault(bool value)
        {
            if (value)
            {
                LaunchSiteManager.setLaunchSite(LaunchSiteManager.GetLaunchSiteByName(siteName));
                MiscUtils.HUDMessage(siteName + " has been set as the launchsite", 10, 0);

                if (EditorDriver.editorFacility == EditorFacility.VAB)
                {
                    KerbalKonstructs.instance.defaultVABlaunchsite = siteName;
                }
                else
                {
                    KerbalKonstructs.instance.defaultSPHlaunchsite = siteName;
                }
            }
        }
    }
}
