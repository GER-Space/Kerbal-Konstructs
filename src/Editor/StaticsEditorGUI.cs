using KerbalKonstructs.Core;
using KerbalKonstructs.UI2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    /// <summary>
    /// Shared state of the statics editor: selected object, snap target, active group,
    /// the groups near the vessel and mouse selection.
    /// The old IMGUI "Statics Editor" window is retired; its parts are UI2 windows now
    /// (Editor Mode toolbar, Ctrl+K: UI2.ModelList, UI2.InstanceList, UI2.GroupList, UI2.MapDecalList,
    /// Save and Export buttons).
    /// </summary>
    public class StaticsEditorGUI
    {

        private static StaticsEditorGUI _instance = null;

        public static StaticsEditorGUI instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new StaticsEditorGUI();

                }
                return _instance;
            }
        }


        public static StaticInstance selectedObject = null;
        public StaticInstance selectedObjectPrevious = null;
        public StaticInstance snapTargetInstance = null;

        private static GroupCenter activeGroup = null;
        private static GroupCenter[] localGroups;

        /// <summary>
        /// The groups near the vessel, as found by ResetLocalGroupList (used by UI2.GroupList)
        /// </summary>
        internal static GroupCenter[] LocalGroups
        {
            get
            {
                return localGroups;
            }
        }

        internal static GrassColorPresetUI2.ColorPreset2 defaultGrassPreset = new GrassColorPresetUI2.ColorPreset2();

        //static need only be loaded once per three seconnds
        private static float lastloaded = 0f;
        internal static StaticInstance[] allStaticInstances;


        /// <summary>
        /// True while the editor is open (the Editor Mode toolbar, Ctrl+K)
        /// </summary>
        public bool IsOpen()
        {
            return KKEditorLogic.IsEditorOpen();
        }


        /// <summary>
        /// Opens or closes the editor (the Editor Mode toolbar). Kept for the Ctrl+K shortcut.
        /// </summary>
        public void ToggleEditor()
        {
            UI2.EditorModeSelector.Toggle();
        }


        /// <summary>
        /// Removes the highlight of the snap target and clears it
        /// </summary>
        internal static void ClearSnapTarget()
        {
            if (instance.snapTargetInstance != null)
            {
                instance.snapTargetInstance.HighlightObject(Color.clear);
                instance.snapTargetInstance = null;
            }
        }


        /// <summary>
        /// Sets the localGroups
        /// </summary>
        internal static void ResetLocalGroupList()
        {
            List<GroupCenter> foundList = new List<GroupCenter>();
            if (FlightGlobals.ActiveVessel != null)
                foreach (var groupCenter in StaticDatabase.allGroupCenters)
                {

                    if (groupCenter.CelestialBody != FlightGlobals.currentMainBody)
                    {
                        continue;
                    }
                    if (Vector3.Distance(FlightGlobals.ActiveVessel.transform.position, groupCenter.gameObject.transform.position) > KerbalKonstructs.localGroupRange)
                    {
                        continue;
                    }

                    foundList.Add(groupCenter);
                }

            localGroups = foundList.ToArray();
        }


        internal static GroupCenter GetCloesedCenter(Vector3 myPosition)
        {
            if (localGroups == null || localGroups.Length == 0)
            {
                return null;
            }
            if (activeGroup != null)
            {
                return activeGroup;
            }

            GroupCenter closest = localGroups[0];
            float dist = Vector3.Distance(myPosition, closest.gameObject.transform.position);
            foreach (GroupCenter center in localGroups)
            {
                if (Vector3.Distance(myPosition, center.gameObject.transform.position) < dist)
                {
                    dist = Vector3.Distance(myPosition, center.gameObject.transform.position);
                    closest = center;
                }
            }

            return closest;
        }


        internal static void ResetInstancesList()
        {
            lastloaded = 0f;
        }


        private static void UpdateInstances()
        {
            if ((Time.time - lastloaded) > 2f)
            {
                lastloaded = Time.time;
                allStaticInstances = StaticDatabase.allStaticInstances;
            }
        }

        /// <summary>
        /// Selects Object under the mouse curser.
        /// </summary>
        internal void SelectMouseObject()
        {
            UpdateInstances();
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            ;
            int layerMask = ~0;

            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return;
            }

            StaticInstance myHitinstance = GetRootFromHit(hit.collider.gameObject);

            if (myHitinstance == null)
            {
                Log.Normal("No RootObject found");
                return;
            }
            else
            {
                if (KerbalKonstructs.selectedInstance != null)
                {
                    KerbalKonstructs.DeselectObject(true, true);
                }
                //Log.Normal("Try to select Object: " + myHitinstance.mesh.name);
                myHitinstance.HighlightObject(XKCDColors.Green_Yellow);
                KerbalKonstructs.SelectInstance(myHitinstance, true);
                if (!InstanceEditor.instance.IsOpen())
                {
                    InstanceEditor.instance.Open();
                }
            }

        }



        internal static void SetActiveGroup(GroupCenter group)
        {
            activeGroup = group;
        }

        internal static GroupCenter GetActiveGroup()
        {
            return activeGroup;
        }

        /// <summary>
        /// tries to find a Static Object attached to a child GameObject.
        /// </summary>
        /// <param name="foundObject"></param>
        /// <returns></returns>
        private StaticInstance GetRootFromHit(GameObject foundObject)
        {
            for (int i = 0; i < 10; i++)
            {
                if (allStaticInstances.Where(x => x.gameObject == foundObject).FirstOrDefault() == null)
                {
                    if (foundObject.transform.parent != null)
                    {
                        foundObject = foundObject.transform.parent.gameObject;
                    }
                }
                else
                {
                    return allStaticInstances.Where(x => x.gameObject == foundObject).First();
                }

            }
            // we didn't find any root object, so we return null
            return null;
        }

    }
}
