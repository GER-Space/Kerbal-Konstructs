using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Core;
using KerbalKonstructs.API;

namespace KerbalKonstructs.Addons
{
    class StageRecovery
    {
        internal static void AttachStageRecovery()
        {
            if (StageRecoveryWrapper.isAvailable)
            {
                StageRecoveryWrapper.AddRecoveryProcessingStartListener(PreRecovery);
                StageRecoveryWrapper.AddRecoveryProcessingFinishListener(PostRecovery);
            }
        }
        /// <summary>
        /// StageRecovery handle. This is called first.
        /// </summary>
        /// <param name="data"></param>
        public static void PreRecovery(Vessel vessel)
        {
            Log.Normal("OnVesselRecoveryRequested");
            if (!KerbalKonstructs.instance.disableRemoteRecovery)
            {
                if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
                {
                    Log.Normal("OnVesselRecoveryRequested is career");
                    // Change the Space Centre to the nearest open base
                    double dist = 0d;
                    double smallestDist = 0d;
                    string openCloseState;
                    float OpenCost;

                    SpaceCenter spaceCenter = null;
                    SpaceCenter closestSpaceCenter = SpaceCenter.Instance;

                    smallestDist = SpaceCenter.Instance.GreatCircleDistance(SpaceCenter.Instance.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));
                    Log.Normal("Distance to KSC is " + smallestDist);

                    foreach (CustomSpaceCenter csc in SpaceCenterManager.spaceCenters)
                    {

                        // ASH Get openclosestate of launchsite with same name as space centre
                        LaunchSiteManager.getSiteOpenCloseState(csc.SpaceCenterName, out openCloseState, out OpenCost);

                        StaticObject myBase = csc.getStaticObject();
                        if ((float)myBase.getSetting("RecoveryFactor") == 0) continue;
                        spaceCenter = csc.getSpaceCenter();
                        dist = spaceCenter.GreatCircleDistance(spaceCenter.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));

                        if (dist < smallestDist)
                        {
                            bool bBaseIsOpen = true;
                            if (openCloseState == "Closed" || openCloseState == "ClosedLocked" || openCloseState == "OpenLocked") bBaseIsOpen = false;

                            if (bBaseIsOpen)
                            {
                                closestSpaceCenter = spaceCenter;
                                smallestDist = dist;
                                // Debug.Log("KK: closest updated to " + closest.SpaceCenterName + ", distance " + smallestDist);
                            }
                        }
                    }

                    // set the Spacecenter to the closest SpaceCenter, because StageRecovery uses this. We revert this later on the PostRecovery function
                    SpaceCenter.Instance = closestSpaceCenter;
                    Log.Normal("SpaceCenter set to: " + closestSpaceCenter.name);

                    if (SpaceCenter.Instance == null)
                    {
                        Log.Normal("no Spacecenter for recovery found");
                        SpaceCenter.Instance = SpaceCenterManager.KSC;
                    }
                }
            }
        }


        /// <summary>
        /// Gameevent handle. This is called after the Recovery
        /// </summary>
        /// <param name="vessel"></param>
		public static void PostRecovery(Vessel vessel)
        {
            Log.Normal("onVesselRecovered called");
            if (!KerbalKonstructs.instance.disableRemoteRecovery)
            {
                if (vessel == null)
                {
                    Log.Warning("onVesselRecovered vessel was null");
                    if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
                    {
                        SpaceCenter.Instance = SpaceCenterManager.KSC;
                    }
                    return;
                }
                if (MiscUtils.CareerStrategyEnabled(HighLogic.CurrentGame))
                {
                    // Put the KSC back as the Space Centre
                    // Not needed as it is buggy in the moment
                    Log.Debug("Resetting SpaceCenter to KSC");
                    SpaceCenter.Instance = SpaceCenterManager.KSC;  
                }
            }
        }
    }
}
