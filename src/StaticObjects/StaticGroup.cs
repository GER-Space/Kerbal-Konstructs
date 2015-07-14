using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace KerbalKonstructs.StaticObjects
{
	class StaticGroup
	{
		private String groupName;
		private String bodyName;

		private List<StaticObject> childObjects = new List<StaticObject>();
		private Vector3 centerPoint = Vector3.zero;
		private float visiblityRange = 0;
		public Boolean alwaysActive = false;
		public Boolean active = false;

		public StaticGroup(String name, String body)
		{
			groupName = name;
			bodyName = body;
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
			Vector3 center = Vector3.zero;
			foreach (StaticObject obj in childObjects)
			{
				if ((float) obj.getSetting("VisibilityRange") > highestVisibility)
					highestVisibility = (float) obj.getSetting("VisibilityRange");

				center += obj.gameObject.transform.position;
			}
			center /= childObjects.Count;

			foreach (StaticObject obj in childObjects)
			{
				float dist = Vector3.Distance(center, obj.gameObject.transform.position);
				if (dist > furthestDist)
					furthestDist = dist;
			}

			visiblityRange = highestVisibility + furthestDist;
			centerPoint = center;
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
				// obj.gameObject.SetActive(false);

				SetActiveRecursively(obj.gameObject, false);
			}
		}

		public void updateCache(Vector3 playerPos)
		{
			foreach (StaticObject obj in childObjects)
			{
				float dist = Vector3.Distance(obj.gameObject.transform.position, playerPos);
				bool visible = (dist < (float) obj.getSetting("VisibilityRange"));
				if (visible != obj.gameObject.activeSelf)
				{
					// Debug.Log("KK: Setting " + obj.gameObject.name + " to visible =" + visible);
					// obj.gameObject.SetActive(visible);
					SetActiveRecursively(obj.gameObject, visible);

					// ASH 06112014
					// What if SetActive isn't actually properly activating children?
					// Transform[] gameObjectList = obj.gameObject.GetComponentsInChildren<Transform>(true);
					// List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.renderer != null select t.gameObject).ToList();
					
					/* foreach (GameObject renderer in rendererList)
					{
						// Debug.Log("KK: Child activeself is " + renderer.activeSelf);
						bool childVisible = renderer.activeSelf;
						if (childVisible != true)
						{
							// Debug.Log("KK: Setting child active!");
							renderer.SetActive(true);
						}
					} */
				}
			}
		}

		public Vector3 getCenter()
		{
			return centerPoint;
		}

		public float getVisibilityRange()
		{
			return visiblityRange;
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
				Debug.Log("KK: Tried to delete an object that doesn't exist in this group!");
			}
		}

		public List<StaticObject> getStatics()
		{
			return childObjects;
		}
	}
}
