using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    internal class StorageGUI
    {

        internal static Storage selectedFacility = null;
        internal static StaticInstance lastInstance = null;

        private static HashSet<PartResourceDefinition> allResources = new HashSet<PartResourceDefinition>();

        internal static Vessel currentVessel = null;

        private static float storedAmount = 0f;
        private static StoredResource storedResource = null;

        private static HashSet<string> blackListedResources = new HashSet<string> { "ElectricCharge", "IntakeAir"};

        private static float maxSpaceLeft = 0f;
        private static double storableUnits = 0f;


        private static Vector2 facilityscroll;
        private static float increment = 10f;


        /// <summary>
        /// Subwindows called by FacilityManager
        /// </summary>
        /// <param name="instance"></param>
        internal static void StorageInerface(StaticInstance instance)
        {
            if (instance != lastInstance)
            {
                Initialize(instance);
            }

            if (!selectedFacility.isOpen)
            {
                return;
            }

            GUILayout.Space(2);
            GUILayout.Space(2);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Stack size: ", GUILayout.Height(18));
                GUI.enabled = (increment != 1f);
                if (GUILayout.Button("1", GUILayout.Height(18), GUILayout.Width(32)))
                {
                    increment = 1f;
                }
                GUI.enabled = (increment != 10f);
                if (GUILayout.Button("10", GUILayout.Height(18), GUILayout.Width(32)))
                {
                    increment = 10f;
                }
                GUI.enabled = (increment != 100f);
                if (GUILayout.Button("100", GUILayout.Height(18), GUILayout.Width(32)))
                {
                    increment = 100f;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.enabled = true;

            GUILayout.Label("Store or retrieve these resources: ", GUILayout.Height(30));

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Volume used: ", GUILayout.Height(23));
                GUILayout.Label(selectedFacility.currentVolume.ToString() + "/" + selectedFacility.maxVolume.ToString(), UIMain.LabelInfo, GUILayout.Height(23));
                GUILayout.Space(10);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            facilityscroll = GUILayout.BeginScrollView(facilityscroll);
            {
                ShowRetrieveGUI();
            }
            GUILayout.EndScrollView();


            GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorderW, GUILayout.Height(4));
            if (!String.IsNullOrEmpty(selectedFacility.FacilityName))
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Thanks for using: ");
                    GUILayout.Box(selectedFacility.FacilityName, UIMain.Yellowtext);
                }
                GUILayout.EndHorizontal();
            }
            //GUILayout.FlexibleSpace();
        }


        /// <summary>
        /// SubSub windows components. The actual scroll view
        /// </summary>
        internal static void ShowRetrieveGUI()
        {
            foreach (PartResourceDefinition resource in allResources)
            {
                if (blackListedResources.Contains(resource.name))
                {
                    continue;
                }

                storedResource = selectedFacility.storedResources.Where(r => r.resource == resource).FirstOrDefault();
                if (storedResource == null)
                {
                    storedAmount = 0f;
                }
                else
                {
                    storedAmount = storedResource.amount;
                }

                GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorderW, GUILayout.Height(4));
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(resource.name, GUILayout.Height(23));
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Stored: ", GUILayout.Height(23));
                    GUILayout.Label((storedAmount).ToString(), UIMain.LabelInfo, GUILayout.Height(23));

                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Volume: ", GUILayout.Height(23));
                    GUILayout.Label((resource.volume * storedAmount).ToString() + "/" + selectedFacility.maxVolume.ToString(), UIMain.LabelInfo, GUILayout.Height(23));
                    //GUILayout.Space(10);
                }
                GUILayout.EndHorizontal();
                foreach (PartSet xFeedSet in currentVessel.crossfeedSets)
                {
                    xFeedSet.GetConnectedResourceTotals(resource.id, out double xfeedAmount, out double xfeedMax, true);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(Math.Round(xfeedAmount, 1).ToString() + " of " + Math.Round(xfeedMax, 1).ToString(), UIMain.LabelInfo, GUILayout.Height(18));
                        GUILayout.FlexibleSpace();

                        maxSpaceLeft = selectedFacility.maxVolume - selectedFacility.currentVolume;
                        storableUnits = Math.Min(increment, maxSpaceLeft / resource.volume);
                        GUI.enabled = (storableUnits > 0f);
                        if (GUILayout.Button("+", GUILayout.Height(18), GUILayout.Width(32)))
                        {
                            double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), resource.id, storableUnits, true);
                            StoreResource(resource, transferred);
                        }
                        if (GUILayout.RepeatButton("++", GUILayout.Height(18), GUILayout.Width(32)))
                        {
                            double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), resource.id, storableUnits, true);
                            StoreResource(resource, transferred);
                        }
                        GUI.enabled = true;
                        GUILayout.FlexibleSpace();

                        // check if we have enough space to retrieve the resource and if there is anything to retrieve
                        GUI.enabled = ((xfeedMax > 0) && (storedAmount > 0f));
                        if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(32)))
                        {

                            double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), resource.id, -Math.Min(increment, storedAmount), true);
                            StoreResource(resource, transferred);

                        }
                        if (GUILayout.RepeatButton("--", GUILayout.Height(18), GUILayout.Width(32)))
                        {
                            double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), resource.id, -Math.Min(increment, storedAmount), true);
                            StoreResource(resource, transferred);
                        }
                        GUILayout.Space(10);
                        GUI.enabled = true;


                    }
                    GUILayout.EndHorizontal();
                }
                //GUILayout.Space(4);
            }

        }


        internal static void GetVesselResources()
        {
            double amount = 0;
            double maxAmount = 0;
            foreach (PartResourceDefinition availableResource in PartResourceLibrary.Instance.resourceDefinitions)
            {
                foreach (var partSet in currentVessel.crossfeedSets)
                {
                    partSet.GetConnectedResourceTotals(availableResource.id, out amount, out maxAmount, true);
                    if (maxAmount > 0)
                    {
                        allResources.Add(availableResource);
                        break;
                    }

                }
            }
        }

        internal static void GetStoredResources()
        {
            foreach (var resource in selectedFacility.storedResources)
            {
                if (!allResources.Contains(resource.resource))
                {
                    allResources.Add(resource.resource);
                }
            }

        }

        /// <summary>
        /// Stores or retrieves a resource to the facility. Deletes the resource if nothing is left
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="amount"></param>
        internal static void StoreResource(PartResourceDefinition resource, double amount)
        {
            StoredResource myStoredResource = selectedFacility.storedResources.Where(r => r.resource == resource).FirstOrDefault();
            if (myStoredResource == null)
            {
                myStoredResource = new StoredResource { resource = resource, amount = (float)amount };
                selectedFacility.storedResources.Add(myStoredResource);
                            }
            else
            {
                myStoredResource.amount += (float)amount;
            }

            if (Math.Round(myStoredResource.amount,4) == 0f)
            {
                selectedFacility.storedResources.Remove(myStoredResource);
            }
        }

        /// <summary>
        /// init of needed vaiabled on facility change
        /// </summary>
        /// <param name="instance"></param>
        internal static void Initialize(StaticInstance instance)
        {
            selectedFacility = instance.myFacilities[0] as Storage;
            lastInstance = instance;
            currentVessel = FlightGlobals.ActiveVessel;
            GetVesselResources();
            GetStoredResources();
        }

    }
}
