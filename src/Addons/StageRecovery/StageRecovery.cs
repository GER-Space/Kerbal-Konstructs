using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;

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
            if (!KerbalKonstructs.instance.disableRemoteRecovery && CareerUtils.isCareerGame)
            {
                Log.Normal("OnVesselRecoveryRequested is career");
                // Change the Space Centre to the nearest open base
                double dist = 0d;

                SpaceCenter spaceCenter = null;
                SpaceCenter closestSpaceCenter = SpaceCenter.Instance;

                double smallestDist = SpaceCenterManager.KSC.GreatCircleDistance(vessel.mainBody.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));
                Log.Normal("Distance to KSC is " + smallestDist);

                foreach (CustomSpaceCenter csc in SpaceCenterManager.spaceCenters)
                {
                    try
                    {
                        //Log.Normal("Checking LS: " + csc.SpaceCenterName);
                        if (!csc.isOpen)
                        {
                            //Log.Normal("Ignoring closed SC: " + csc.SpaceCenterName);
                            continue;
                        }

                        spaceCenter = csc.GetSpaceCenter();
                        dist = spaceCenter.GreatCircleDistance(spaceCenter.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));
                        Log.Normal("distance is: " + dist);
                        if (dist < smallestDist)
                        {
                            closestSpaceCenter = spaceCenter;
                            smallestDist = dist;
                            Log.Normal("KK: closest updated to " + csc.SpaceCenterName + ", distance " + smallestDist);
                        }

                    }
                    catch
                    {

                        Log.UserWarning("Error Processing... " + csc.SpaceCenterName);
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


        /// <summary>
        /// Gameevent handle. This is called after the Recovery
        /// </summary>
        /// <param name="vessel"></param>
		public static void PostRecovery(Vessel vessel)
        {
            Log.Normal("onVesselRecovered called");
            if (!KerbalKonstructs.instance.disableRemoteRecovery && CareerUtils.isCareerGame)
            {
                //Log.Normal("Resetting SpaceCenter to KSC");
                SpaceCenter.Instance = SpaceCenterManager.KSC;
            }
        }
    }
}
