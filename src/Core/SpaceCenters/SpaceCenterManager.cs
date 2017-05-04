using System;
using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.API;
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



		public static void getClosestSpaceCenter(Vessel vessel, out SpaceCenter ClosestCenter, out float ClosestDistance, 
			out float RecoveryFactor, out float RecoveryRange, out string BaseName)
		{
			CustomSpaceCenter closest = null;
            SpaceCenter sc = null;

            var smallestDist = (float)SpaceCenter.Instance.GreatCircleDistance(SpaceCenter.Instance.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));
            Log.Normal("Distance to KSC is " + smallestDist);

			bool isCareer = CareerUtils.isCareerGame;


			string sOpenCloseState = "Closed";
			string sBaseName = "";
			float fMyBaseRecovFact = 0f;
			float fMyBaseRecovRang = 0f;

			foreach (CustomSpaceCenter csc in spaceCenters)
			{
				if (isCareer)
				{
					string OpenCloseState;
					float OpenCost;
					// ASH Get openclosestate of launchsite with same name as space centre
					LaunchSiteManager.getSiteOpenCloseState(csc.SpaceCenterName, out OpenCloseState, out OpenCost);
					sOpenCloseState = OpenCloseState;
				}

				StaticObject myBase = csc.getStaticObject();
				if ((float)myBase.getSetting("RecoveryFactor") == 0) continue;
                sc = csc.getSpaceCenter();
                //float dist = Vector3.Distance(position, csc.getStaticObject().gameObject.transform.position);
                var dist = (float)sc.GreatCircleDistance(sc.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));

				if (dist < smallestDist)
				{
					bool bBaseIsOpen = true;
					if (sOpenCloseState == "Closed" || sOpenCloseState == "ClosedLocked" || sOpenCloseState == "OpenLocked") bBaseIsOpen = false;

					if (isCareer && !bBaseIsOpen)
					{ }
					else
					{
						closest = csc;
						smallestDist = dist;
						fMyBaseRecovFact = (float)myBase.getSetting("RecoveryFactor");
						fMyBaseRecovRang = (float)myBase.getSetting("RecoveryRange");
						sBaseName = (string)myBase.getSetting("LaunchSiteName");
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

			ClosestCenter = sc;
			ClosestDistance = smallestDist;
			RecoveryFactor = fMyBaseRecovFact;
			RecoveryRange = fMyBaseRecovRang;
			BaseName = sBaseName;
		}
	}
}