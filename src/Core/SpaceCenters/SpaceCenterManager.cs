using System;
using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.Core
{
	public class SpaceCenterManager
	{
		public static List<CustomSpaceCenter> spaceCenters = new List<CustomSpaceCenter>();
		public static SpaceCenter KSC;

		public static void setKSC()
		{
			KSC = SpaceCenter.Instance;
		}

		public static void addSpaceCenter(CustomSpaceCenter csc)
		{
			spaceCenters.Add(csc);
		}

        internal static CustomSpaceCenter GetCSC(string name)
        {
            foreach (var csc in spaceCenters)
            {
                if (csc.SpaceCenterName == name)
                    return csc;
            }
            return null;
        }

        public static void getClosestSpaceCenter(Vessel vessel, out SpaceCenter closestCenter, out float closestDistance, 
			out float RecoveryFactor, out float RecoveryRange, out string BaseName)
		{
			CustomSpaceCenter closest = null;
            SpaceCenter sc = null;

            var smallestDist = (float)SpaceCenter.Instance.GreatCircleDistance(SpaceCenter.Instance.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));
            Log.Normal("Distance to KSC is " + smallestDist);

			bool isCareer = CareerUtils.isCareerGame;

			string sBaseName = "";
			float fMyBaseRecovFact = 0f;
			float fMyBaseRecovRang = 0f;


            foreach (CustomSpaceCenter csc in spaceCenters)
			{

				if (csc.staticInstance.launchSite.RecoveryFactor == 0) continue;
                sc = csc.getSpaceCenter();
                //float dist = Vector3.Distance(position, csc.getStaticObject().gameObject.transform.position);
                var dist = (float)sc.GreatCircleDistance(sc.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));

				if (dist < smallestDist)
				{
					if (isCareer && csc.staticInstance.launchSite.isOpen )
					{
                        closest = csc;
						smallestDist = dist;
						fMyBaseRecovFact = csc.staticInstance.launchSite.RecoveryFactor;
						fMyBaseRecovRang = csc.staticInstance.launchSite.RecoveryRange;
						sBaseName = csc.staticInstance.launchSite.LaunchSiteName;
						// Debug.Log("KK: closest updated to " + closest.SpaceCenterName + ", distance " + smallestDist);
					}
				}
			}

			

			if (closest == null) 
				sc = null;
			else
			{
			    Log.Normal("closest Spacecenter is " + closest.SpaceCenterName);
				sc = closest.getSpaceCenter();
			}

            Log.Normal("smallestDist is " + smallestDist);
            Log.Normal("returning closest space centre: " + sc.name);


			if (smallestDist < 1) smallestDist = 0;
			if (sc == null)
			{
				sc = KSC;
				fMyBaseRecovFact = 100;
				sBaseName = "KSC";
			}

			closestCenter = sc;
			closestDistance = smallestDist;
			RecoveryFactor = fMyBaseRecovFact;
			RecoveryRange = fMyBaseRecovRang;
			BaseName = sBaseName;
		}
	}
}