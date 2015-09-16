using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.StaticObjects
{
	class StaticGroup
	{
		public String groupName;
		public String bodyName;

		public List<StaticObject> childObjects = new List<StaticObject>();
		public Vector3 centerPoint = Vector3.zero;
		public float visibilityRange = 0;
		public Boolean alwaysActive = false;
		public Boolean active = false;
		public Boolean bLiveUpdate = false;

		public StaticGroup(String name, String body)
		{
			groupName = name;
			bodyName = body;
			centerPoint = Vector3.zero;
			visibilityRange = 0f; 
		}

		public void addStatic(StaticObject obj)
		{
			childObjects.Add(obj);
			updateCacheSettings();
		}

		public void removeStatic(StaticObject obj)
		{
			childObjects.Remove(obj);
			updateCacheSettings();
		}

		public void updateCacheSettings()
		{
			float highestVisibility = 0;
			float furthestDist = 0;

			centerPoint = Vector3.zero;

			foreach (StaticObject obj in childObjects)
			{
				// FIRST ONE IS THE CENTER
				centerPoint = obj.gameObject.transform.position;
				break;
			}

			foreach (StaticObject obj in childObjects)
			{
				if ((float)obj.getSetting("VisibilityRange") > highestVisibility)
					highestVisibility = (float)obj.getSetting("VisibilityRange");

				float dist = Vector3.Distance(centerPoint, obj.gameObject.transform.position);
				
				if (dist > furthestDist)
					furthestDist = dist;
			}

			visibilityRange = highestVisibility + (furthestDist * 2);
		}

		public static void SetActiveRecursively(GameObject rootObject, bool active)
		{
			rootObject.SetActive(active);

			foreach (Transform childTransform in rootObject.transform)
			{
				SetActiveRecursively(childTransform.gameObject, active);
			}
		}

		public void cacheAll()
		{
			foreach (StaticObject obj in childObjects)
			{
				SetActiveRecursively(obj.gameObject, false);
			}
		}

		public void updateCache(Vector3 playerPos)
		{
			foreach (StaticObject obj in childObjects)
			{
				float dist = Vector3.Distance(obj.gameObject.transform.position, playerPos);
				bool visible = (dist < (float) obj.getSetting("VisibilityRange"));

				string sFacType = (string)obj.getSetting("FacilityType");

				if (sFacType == "Hangar")
				{
					string sInStorage = (string)obj.getSetting("InStorage");
					string sInStorage2 = (string)obj.getSetting("TargetID");
					string sInStorage3 = (string)obj.getSetting("TargetType");
					
					foreach (Vessel vVesselStored in FlightGlobals.Vessels)
					{
						if (vVesselStored == null) continue;
						if (!vVesselStored.loaded) continue;
						if (vVesselStored.vesselType == VesselType.SpaceObject) continue;
						if (vVesselStored.vesselType == VesselType.Debris) continue;
						if (vVesselStored.vesselType == VesselType.EVA) continue;
						if (vVesselStored.vesselType == VesselType.Flag) continue;
						if (vVesselStored.vesselType == VesselType.Unknown) continue;

						string sHangarSpace = "None";
						// If a vessel is hangared
						if (vVesselStored.id.ToString() == sInStorage)
							sHangarSpace = "InStorage";
						if (vVesselStored.id.ToString() == sInStorage2)
							sHangarSpace = "TargetID";
						if (vVesselStored.id.ToString() == sInStorage3)
							sHangarSpace = "TargetType";

						if (sHangarSpace != "None")
						{
							if (vVesselStored == FlightGlobals.ActiveVessel)
							{
								// Craft has been taken control
								// Empty the hangar
								obj.setSetting(sHangarSpace, "None");
								PersistenceUtils.saveStaticPersistence(obj);
							}
							else
							{
								// Hide the vessel - it is in the hangar
								if (vVesselStored != null)
								{
									foreach (Part p in vVesselStored.Parts)
									{
										if (p != null && p.gameObject != null)
											p.gameObject.SetActive(false);
										else
											continue;
									}

									vVesselStored.MakeInactive();
									vVesselStored.enabled = false;

									if (vVesselStored.loaded)
										vVesselStored.Unload();
								}
							}
						}
					}
				}

				if (sFacType == "CityLights")
				{
					if (dist < 60000f)
						SetActiveRecursively(obj.gameObject, false);
					else
					{
						if (visible)
							SetActiveRecursively(obj.gameObject, true);
					}
				}
				else
				{				
					if (visible)
						SetActiveRecursively(obj.gameObject, true);
				}
			}
		}

		public Vector3 getCenter()
		{
			return centerPoint;
		}

		public float getVisibilityRange()
		{
			return visibilityRange;
		}

		public String getGroupName()
		{
			return groupName;
		}

		internal void deleteObject(StaticObject obj)
		{
			if (childObjects.Contains(obj))
			{
				childObjects.Remove(obj);
				MonoBehaviour.Destroy(obj.gameObject);
			}
			else
			{
				if (KerbalKonstructs.instance.DebugMode)
					Debug.Log("KK: StaticGroup deleteObject tried to delete an object that doesn't exist in this group!");
			}
		}

		public List<StaticObject> getStatics()
		{
			return childObjects;
		}
	}
}
