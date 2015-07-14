using KerbalKonstructs.SpaceCenters;
using KerbalKonstructs.StaticObjects;
using System.Reflection;
using UnityEngine;
using KerbalKonstructs.LaunchSites;

namespace KerbalKonstructs
{
	public class CustomSpaceCenter
	{
		public string SpaceCenterName;

		private SpaceCenter spaceCenter;
		private StaticObject staticObject;
		private GameObject gameObject;
		
		public static void CreateFromLaunchsite(string name, GameObject go)
		{
			StaticObject staticObject = KerbalKonstructs.instance.getStaticDB().getStaticFromGameObject(go);
			if (staticObject != null)
			{
				var csc = new CustomSpaceCenter();
				csc.SpaceCenterName = name;
				csc.staticObject = staticObject;
				csc.gameObject = go;

				// Debug.Log("KK: CreateFromLaunchsite added Space Center " + name);
				SpaceCenterManager.addSpaceCenter(csc);
			}
			else
			{
				// Debug.Log("KK: CreateFromLaunchsite failed because staticObject is null.");
			}
		}

		public SpaceCenter getSpaceCenter()
		{
			if (spaceCenter == null)
			{
				spaceCenter = gameObject.AddComponent<SpaceCenter>();
				spaceCenter.cb = (CelestialBody)staticObject.getSetting("CelestialBody");
				spaceCenter.name = SpaceCenterName;

				// Debug.Log("KK: getSpaceCenter set spaceCenter.name to " + SpaceCenterName);

				FieldInfo lat = spaceCenter.GetType().GetField("\u0002", BindingFlags.NonPublic | BindingFlags.Instance);
				lat.SetValue(spaceCenter, spaceCenter.cb.GetLatitude(gameObject.transform.position));
				FieldInfo lon = spaceCenter.GetType().GetField("\u0003", BindingFlags.NonPublic | BindingFlags.Instance);
				lon.SetValue(spaceCenter, spaceCenter.cb.GetLongitude(gameObject.transform.position));
				FieldInfo srfVector = spaceCenter.GetType().GetField("\u0004", BindingFlags.NonPublic | BindingFlags.Instance);
				srfVector.SetValue(spaceCenter, spaceCenter.cb.GetRelSurfaceNVector(spaceCenter.Latitude, spaceCenter.Longitude));
			}
			else
			{
				// Debug.Log("KK: getSpaceCenter was not null.");
			}

			return spaceCenter;
		}

		public StaticObject getStaticObject()
		{
			return staticObject;
		}

	}
}