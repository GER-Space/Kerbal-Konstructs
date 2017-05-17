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

		public List<StaticInstance> groupInstances = new List<StaticInstance>();
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

		public void AddStatic(StaticInstance obj)
		{
			groupInstances.Add(obj);
			UpdateCacheSettings();
		}

		public void RemoveStatic(StaticInstance obj)
		{
			groupInstances.Remove(obj);
			UpdateCacheSettings();
		}

        public void UpdateCacheSettings()
        {
            float highestVisibility = 0;
            float furthestDist = 0;

            centerPoint = Vector3.zero;
            StaticInstance soCenter = null;
            Vector3 vRadPos = Vector3.zero;


            // FIRST ONE IS THE CENTER
            centerPoint = groupInstances[0].gameObject.transform.position;
            vRadPos = (Vector3)groupInstances[0].RadialPosition;
            groupInstances[0].GroupCenter = "true";
            soCenter = groupInstances[0];



            for (int i = 0; i < groupInstances.Count; i++)
            {

                if (groupInstances[i] != soCenter) groupInstances[i].GroupCenter = "false";

                if (groupInstances[i].VisibilityRange > highestVisibility)
                    highestVisibility = groupInstances[i].VisibilityRange;

                float dist = Vector3.Distance(centerPoint, groupInstances[i].gameObject.transform.position);

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
            for (int i = 0; i < groupInstances.Count; i++)
            {
                InstanceUtil.SetActiveRecursively(groupInstances[i], false);
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


            foreach (StaticInstance instance in groupInstances)
			{
				dist = Vector3.Distance(instance.gameObject.transform.position, playerPos);
				visible = (dist < instance.VisibilityRange);

				string sFacType = instance.FacilityType;

				if (sFacType == "Hangar")
				{
					if (visible) HangarGUI.CacheHangaredCraft(instance);
				}

				if (sFacType == "LandingGuide")
				{
					if (visible) KerbalKonstructs.GUI_Landinguide.drawLandingGuide(instance);
					else
						KerbalKonstructs.GUI_Landinguide.drawLandingGuide(null);
				}

				if (sFacType == "TouchdownGuideL")
				{
					if (visible) KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideL(instance);
					else
						KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideL(null);
				}

				if (sFacType == "TouchdownGuideR")
				{
					if (visible) KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideR(instance);
					else
						KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideR(null);
				}

				if (sFacType == "CityLights")
				{
					if (dist < 65000f)
					{
                        InstanceUtil.SetActiveRecursively(instance, false);
						return;
					}
				}
			
				if (visible)
                    InstanceUtil.SetActiveRecursively(instance, true);
				else
                    InstanceUtil.SetActiveRecursively(instance, false);
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

		internal void DeleteObject(StaticInstance obj)
		{
			if (groupInstances.Contains(obj))
			{
				groupInstances.Remove(obj);
				MonoBehaviour.Destroy(obj.gameObject);
			}
			else
			{
                Log.Debug("StaticGroup deleteObject tried to delete an object that doesn't exist in this group!");
			}
		}

		public List<StaticInstance> GetStatics()
		{
			return groupInstances;
		}
	}
}
