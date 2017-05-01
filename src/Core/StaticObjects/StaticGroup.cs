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

        /// <summary>
        /// Makes all objects invisible
        /// </summary>
		public void CacheAll()
        {
            for (int i = 0; i < childObjects.Count; i++)
            {
                childObjects[i].SetActive(false);
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
            string sFacType = "";

            for (int i = 0; i < childObjects.Count; i++)
            {
                dist = Vector3.Distance(childObjects[i].gameObject.transform.position, playerPos);
                visible = (dist < (float)childObjects[i].getSetting("VisibilityRange"));

                sFacType = (string)childObjects[i].getSetting("FacilityType");

                if (sFacType == "Hangar")
                {
                    if (visible) HangarGUI.CacheHangaredCraft(childObjects[i]);
                }

                if (sFacType == "LandingGuide")
                {
                    if (visible) KerbalKonstructs.GUI_Landinguide.drawLandingGuide(childObjects[i]);
                    else
                        KerbalKonstructs.GUI_Landinguide.drawLandingGuide(null);
                }

                if (sFacType == "TouchdownGuideL")
                {
                    if (visible) KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideL(childObjects[i]);
                    else
                        KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideL(null);
                }

                if (sFacType == "TouchdownGuideR")
                {
                    if (visible) KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideR(childObjects[i]);
                    else
                        KerbalKonstructs.GUI_Landinguide.drawTouchDownGuideR(null);
                }

                if (sFacType == "CityLights")
                {
                    if (dist < 65000f)
                    {
                        visible = true;
                    }
                }

                if (visible)
                    childObjects[i].SetActive(true);
                else
                    childObjects[i].SetActive(false);
            }
        }

        public Vector3 GetCenter()
        {
            return centerPoint;
        }

        public float GetVisibilityRange()
        {
            return visibilityRange;
        }

        public String GetGroupName()
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
