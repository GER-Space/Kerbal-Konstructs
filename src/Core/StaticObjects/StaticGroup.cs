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

        internal StaticInstance groupCenter = null;

        public Vector3 centerPoint = Vector3.zero;
		public float visibilityRange = 0;
        public bool isActive = false;

		public StaticGroup(String name, String body)
		{
			this.name = name;
			bodyName = body;
			centerPoint = Vector3.zero;
			visibilityRange = 0f; 
		}

		public void AddStatic(StaticInstance obj)
		{
			_groupInstances.Add(obj);
            groupInstances = _groupInstances.ToArray();
            UpdateCacheSettings();
		}

		public void RemoveStatic(StaticInstance obj)
		{
			_groupInstances.Remove(obj);
            groupInstances = _groupInstances.ToArray();
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
            groupCenter = groupInstances[0];
            centerPoint = groupInstances[0].gameObject.transform.position;
            vRadPos = (Vector3)groupInstances[0].RadialPosition;
            groupInstances[0].GroupCenter = "true";
            soCenter = groupInstances[0];

            for (int i = 0; i < groupInstances.Length; i++)
            {

                if (groupInstances[i] != soCenter) groupInstances[i].GroupCenter = "false";

                if (groupInstances[i].VisibilityRange > highestVisibility)
                    highestVisibility = groupInstances[i].VisibilityRange;

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
		}

        /// <summary>
        /// gets called every second, when in flight by KerbalKonsructs.updateCache (InvokeRepeating) and through StaticDatabase.UpdateCache
        /// </summary>
        /// <param name="playerPos"></param>
		public void CheckUngrouped(Vector3 playerPos)
		{
            //Log.Normal("Check ungrouped assets");
            foreach (StaticInstance instance in groupInstances)
			{
                float dist = Vector3.Distance(instance.gameObject.transform.position, playerPos);
                bool visible = (dist < visibilityRange);
                string sFacType = instance.FacilityType;

				if (sFacType == "Hangar")
				{
                        HangarGUI.CacheHangaredCraft(instance);
                }

				if (sFacType == "LandingGuide")
				{
					if (visible)
                    {
                        LandingGuideUI.instance.drawLandingGuide(instance);
                    }
                    else
                    {
                        LandingGuideUI.instance.drawLandingGuide(null);
                    }
                }

				if (sFacType == "TouchdownGuideL")
				{
					if (visible)
                    {
                        LandingGuideUI.instance.drawTouchDownGuideL(instance);
                    }
                    else
                    {
                        LandingGuideUI.instance.drawTouchDownGuideL(null);
                    }
                }

				if (sFacType == "TouchdownGuideR")
				{
					if (visible)
                    {
                        LandingGuideUI.instance.drawTouchDownGuideR(instance);
                    }
                    else
                    {
                        LandingGuideUI.instance.drawTouchDownGuideR(null);
                    }
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
                {
                    InstanceUtil.SetActiveRecursively(instance, true);
                }
                else
                {
                    InstanceUtil.SetActiveRecursively(instance, false);
                }
            }
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
