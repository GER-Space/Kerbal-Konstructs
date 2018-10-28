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
        public String name;
        public String bodyName;

        private List<StaticInstance> _groupInstances = new List<StaticInstance>();
        public StaticInstance[]  groupInstances = new StaticInstance[]{};

        internal GroupCenter groupCenter = null;

        public Vector3 centerPoint = Vector3.zero;
		public float visibilityRange = KerbalKonstructs.localGroupRange * 2;
        public bool isActive = false;

		public StaticGroup(String name, String body)
		{
			this.name = name;
			bodyName = body;
			centerPoint = Vector3.zero;
			visibilityRange = 0f;
            groupCenter = StaticDatabase.allCenters[body + "_" + name];
            // FIRST ONE IS THE CENTER
            centerPoint = groupCenter.gameObject.transform.position;
        }

		public void AddStatic(StaticInstance instance)
		{
			_groupInstances.Add(instance);
            groupInstances = _groupInstances.ToArray();
            UpdateCacheSettings();
		}

		public void RemoveStatic(StaticInstance instance)
		{
			_groupInstances.Remove(instance);
            groupInstances = _groupInstances.ToArray();
            UpdateCacheSettings();
		}


        public void UpdateCacheSettings()
        {
            float highestVisibility = 0;
            float furthestDist = 0;
            

            for (int i = 0; i < groupInstances.Length; i++)
            {

                if (groupInstances[i].VisibilityRange > highestVisibility)
                {
                    highestVisibility = groupInstances[i].VisibilityRange;
                }

                float dist = Vector3.Distance(centerPoint, groupInstances[i].gameObject.transform.position);

                if (dist > furthestDist)
                {
                    furthestDist = dist;
                }
            }

            VesselRanges.Situation unloadRange = PhysicsGlobals.Instance.VesselRangesDefault.flying;
            visibilityRange = Math.Max(unloadRange.unload,highestVisibility) + (furthestDist * 2);
        }


		public void Deactivate()
		{
            for (int i = 0; i < groupInstances.Length; i++)
            {
                InstanceUtil.SetActiveRecursively(groupInstances[i], false);
			}
            groupCenter.gameObject.SetActive(false);
        }


        /// <summary>
        /// gets called every second, when in flight by KerbalKonsructs.updateCache (InvokeRepeating) and through StaticDatabase.UpdateCache
        /// </summary>
        /// <param name="playerPos"></param>
        public void ActivateGroupMembers()
        {
            if (isActive)
            {
                return;
            }

            isActive = true;

            Log.Normal("Activate calles for group: " + name);

            foreach (StaticInstance instance in groupInstances)
            {
                string sFacType = instance.FacilityType;

                if (sFacType == "Hangar")
                {
                    HangarGUI.CacheHangaredCraft(instance);
                }

                if (sFacType == "LandingGuide")
                {
                    LandingGuideUI.instance.drawLandingGuide(instance);
                }

                if (sFacType == "TouchdownGuideL")
                {
                    LandingGuideUI.instance.drawTouchDownGuideL(instance);
                }

                if (sFacType == "TouchdownGuideR")
                {
                    LandingGuideUI.instance.drawTouchDownGuideR(instance);
                }

                InstanceUtil.SetActiveRecursively(instance, true);
            }
            groupCenter.gameObject.SetActive(true);
        }

        /// <summary>
        /// Deactivates the Statics of an Group
        /// </summary>
        public void DeactivateGroupMembers()
        {
            if (!isActive)
            {
                return;
            }

            Log.Normal("Deactivate calles for group: " + name);

            isActive = false;
            foreach (StaticInstance instance in groupInstances)
            {
                string sFacType = instance.FacilityType;

                if (sFacType == "Hangar")
                {
                    HangarGUI.CacheHangaredCraft(instance);
                }

                if (sFacType == "LandingGuide")
                {
                    LandingGuideUI.instance.drawLandingGuide(null);
                }

                if (sFacType == "TouchdownGuideL")
                {
                    LandingGuideUI.instance.drawTouchDownGuideL(null);
                }

                if (sFacType == "TouchdownGuideR")
                {
                    LandingGuideUI.instance.drawTouchDownGuideR(null);
                }

                InstanceUtil.SetActiveRecursively(instance, false);
            }
            groupCenter.gameObject.SetActive(false);
        }


        internal void DeleteObject(StaticInstance obj)
		{
			if (_groupInstances.Contains(obj))
			{
				_groupInstances.Remove(obj);
                groupInstances = _groupInstances.ToArray();

                MonoBehaviour.Destroy(obj.gameObject);
			}
			else
			{
                Log.Debug("StaticGroup deleteObject tried to delete an object that doesn't exist in this group!");
			}
		}

		public StaticInstance [] GetStatics()
		{
			 return groupInstances;
		}
	}
}
