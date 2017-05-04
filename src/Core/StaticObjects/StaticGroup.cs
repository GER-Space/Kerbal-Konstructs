using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.UI;

namespace KerbalKonstructs.Core
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

		public void AddStatic(StaticObject obj)
		{
			childObjects.Add(obj);
			UpdateCacheSettings();
		}

		public void RemoveStatic(StaticObject obj)
		{
			childObjects.Remove(obj);
			UpdateCacheSettings();
		}

        public void UpdateCacheSettings()
        {
            float highestVisibility = 0;
            float furthestDist = 0;

            centerPoint = Vector3.zero;
            StaticObject soCenter = null;
            Vector3 vRadPos = Vector3.zero;


            // FIRST ONE IS THE CENTER
            centerPoint = childObjects[0].gameObject.transform.position;
            vRadPos = (Vector3)childObjects[0].getSetting("RadialPosition");
            childObjects[0].setSetting("GroupCenter", "true");
            soCenter = childObjects[0];



            for (int i = 0; i < childObjects.Count; i++)
            {
                childObjects[i].setSetting("RefCenter", vRadPos);

                if (childObjects[i] != soCenter) childObjects[i].setSetting("GroupCenter", "false");

                if ((float)childObjects[i].getSetting("VisibilityRange") > highestVisibility)
                    highestVisibility = (float)childObjects[i].getSetting("VisibilityRange");

                float dist = Vector3.Distance(centerPoint, childObjects[i].gameObject.transform.position);

                if (dist > furthestDist)
                    furthestDist = dist;
            }

            visibilityRange = highestVisibility + (furthestDist * 2);
        }

		public static void SetActiveRecursively(GameObject rootObject, bool active)
		{
            rootObject.SetActive(active);
            var transforms = rootObject.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                transforms[i].gameObject.SetActive(active);
            }
        }

		public void CacheAll()
		{
            for (int i = 0; i < childObjects.Count; i++)
            {
                InstanceUtil.SetActiveRecursively(childObjects[i], false);
			}
		}

        /// <summary>
        /// gets called every second, when in flight by KerbalKonsructs.updateCache (InvokeRepeating)
        /// </summary>
        /// <param name="playerPos"></param>
		public void UpdateCache(Vector3 playerPos)
		{
            float dist = 0f;
            bool visible = false;


            foreach (StaticObject obj in childObjects)
			{
				dist = Vector3.Distance(obj.gameObject.transform.position, playerPos);
				visible = (dist < (float) obj.getSetting("VisibilityRange"));

				string sFacType = (string)obj.getSetting("FacilityType");

				if (sFacType == "Hangar")
				{
					if (visible) HangarGUI.CacheHangaredCraft(obj);
				}

				if (sFacType == "LandingGuide")
				{
					if (visible) KerbalKonstructs.GUI_Landinguide.drawLandingGuide(obj);
					else
						KerbalKonstructs.GUI_Landinguide.drawLandingGuide(null);
				}

				if (sFacType == "TouchdownGuideL")
				{
					if (visible) KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideL(obj);
					else
						KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideL(null);
				}

				if (sFacType == "TouchdownGuideR")
				{
					if (visible) KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideR(obj);
					else
						KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideR(null);
				}

				if (sFacType == "CityLights")
				{
					if (dist < 65000f)
					{
                        InstanceUtil.SetActiveRecursively(obj, false);
						return;
					}
				}
			
				if (visible)
                    InstanceUtil.SetActiveRecursively(obj, true);
				else
                    InstanceUtil.SetActiveRecursively(obj, false);
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

		internal void DeleteObject(StaticObject obj)
		{
			if (childObjects.Contains(obj))
			{
				childObjects.Remove(obj);
				MonoBehaviour.Destroy(obj.gameObject);
			}
			else
			{
                Log.Debug("StaticGroup deleteObject tried to delete an object that doesn't exist in this group!");
			}
		}

		public List<StaticObject> GetStatics()
		{
			return childObjects;
		}
	}
}
