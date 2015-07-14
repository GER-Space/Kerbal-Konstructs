using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.StaticObjects;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.Utilities
{
	public class NavUtils
	{
		public static double GetLongitude(Vector3d radialPosition)
		{
			Vector3d norm = radialPosition.normalized;
			double longitude = Math.Atan2(norm.z, norm.x);
			return (!double.IsNaN(longitude) ? longitude : 0.0);
		}

		public static double GetLatitude(Vector3d radialPosition)
		{
			double latitude = Math.Asin(radialPosition.normalized.y);
			return (!double.IsNaN(latitude) ? latitude : 0.0);
		}

		public static StaticObject GetNearestFacility(Vector3 vPosition, string sFacilityType, string sGroup = "None")
		{
			StaticObject soFacility = null;

			float fLastDist = 100000000f;
			float fDistance = 0f;
			float fNearest = 0f;

			foreach (StaticObject obj in KerbalKonstructs.instance.getStaticDB().getAllStatics())
			{
				if (sGroup != "None")
				{
					if ((string)obj.getSetting("Group") != sGroup) continue;
				}

				if ((string)obj.getSetting("FacilityType") == sFacilityType)
				{
					fDistance = Vector3.Distance(obj.gameObject.transform.position, vPosition);

					if (fDistance < fLastDist)
					{
						fNearest = fDistance;
						soFacility = obj;
					}

					fLastDist = fDistance;
				}
				else continue;
			}

			return soFacility;
		}
	}
}
