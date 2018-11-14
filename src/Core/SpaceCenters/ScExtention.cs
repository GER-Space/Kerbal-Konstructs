using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalKonstructs;
using System.Reflection;

namespace KerbalKonstructs.Core
{
    class ScExtention
    {
        internal static void TuneFacilities()
        {
            MethodInfo originalCall = typeof(EditorEnumExtensions).GetMethod("GetFacility", BindingFlags.Public | BindingFlags.Static);
            MethodInfo improvedCall = typeof(KKSpaceCenter).GetMethod("GetFacility", BindingFlags.Public | BindingFlags.Static);


            if (originalCall != null && improvedCall != null)
                AsmUtils.TryDetourFromTo(originalCall, improvedCall);
            else
                Log.Normal( (originalCall != null) + " " + (improvedCall != null));
        }

    }

    static class KKSpaceCenter
    {
        public static SpaceCenterFacility GetFacility(this PSystemSetup.SpaceCenterFacility spaceCenterFac)
        {

            KerbalKonstructs.instance.lastLaunchSiteUsed = spaceCenterFac.name;
            switch (spaceCenterFac.editorFacility)
            {
                case EditorFacility.VAB:
                    return SpaceCenterFacility.LaunchPad;
                case EditorFacility.SPH:
                    return SpaceCenterFacility.Runway;
                default:
                    return SpaceCenterFacility.LaunchPad;
            }
        }
    }
}
