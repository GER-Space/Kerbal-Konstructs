using KerbalKonstructs.StaticObjects;
using System;
using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.API;
using KerbalKonstructs.LaunchSites;

namespace KerbalKonstructs.SpaceCenters
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



		public static void getClosestSpaceCenter(Vector3 position, out SpaceCenter ClosestCenter, out float ClosestDistance, 
			out float RecoveryFactor, out float RecoveryRange, out string BaseName)
		{
			CustomSpaceCenter closest = null;

			float smallestDist = Vector3.Distance(KSC.gameObject.transform.position, position);
			// Debug.Log("KK: Distance to KSC is " + smallestDist);

			bool isCareer = false;

			if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
			{
				if (!KerbalKonstructs.instance.disableCareerStrategyLayer)
				{
					isCareer = true;
					PersistenceFile<LaunchSite>.LoadList(LaunchSiteManager.AllLaunchSites, "LAUNCHSITES", "KK");
				}
			}

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

				float dist = Vector3.Distance(position, csc.getStaticObject().gameObject.transform.position);

				if (dist < smallestDist)
				{
					if (isCareer && sOpenCloseState == "Closed")
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

			SpaceCenter sc;

			if (closest == null) 
				sc = null;
			else
			{
				// Debug.Log("KK: closest is " + closest.SpaceCenterName);
				sc = closest.getSpaceCenter();
			}

			// Debug.Log("KK: smallestDist is " + smallestDist);
			// Debug.Log("KK: returning closest space centre: " + sc.name);


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