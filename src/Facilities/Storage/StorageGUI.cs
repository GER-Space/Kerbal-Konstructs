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

            GUILayout.Label("Store or retrieve these resources: ", GUILayout.Height(30));


            ShowRetrieveGUI();
            


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


        internal static void ShowStoreGUI()
        {

        }

        internal static void ShowRetrieveGUI()
        {
            foreach (PartResourceDefinition resource in allResources)
            {
                //currentVessel.resourcePartSet.GetConnectedResourceTotals(myResource.id, out double amount, out double maxAmount, false);
                // check if we can store this resource
                //if (maxAmount == 0 )
                //{
                //    continue;
                //}
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
                    GUILayout.Label((resource.volume * storedAmount).ToString(), UIMain.LabelInfo, GUILayout.Height(23));
                    GUILayout.Space(10);
                }
                GUILayout.EndHorizontal();
                foreach (PartSet xFeedSet in currentVessel.crossfeedSets)
                {
                    xFeedSet.GetConnectedResourceTotals(resource.id, out double xfeedAmount, out double xfeedMax, true);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(Math.Round(xfeedAmount, 1).ToString() + " of " + Math.Round(xfeedMax, 1).ToString(), UIMain.LabelInfo, GUILayout.Height(18));
                        GUILayout.FlexibleSpace();

                        if (GUILayout.RepeatButton("++", GUILayout.Height(18), GUILayout.Width(32)))
                        {
                            double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), resource.id, 1, true);
                            ResourceStore(resource, transferred);
                        }
                        if (GUILayout.Button("+", GUILayout.Height(18), GUILayout.Width(32)))
                        {
                            double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), resource.id, 1, true);
                            ResourceStore(resource, transferred);
                        }

                        GUILayout.FlexibleSpace();

                        // check if we have enough space to store the resources and if we have enough stored
                        GUI.enabled = (storedAmount > 0f);
                        if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(32)) || GUILayout.RepeatButton("--", GUILayout.Height(18), GUILayout.Width(32)))
                        {
                            if (storedAmount > 0f)
                            {
                                double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), resource.id, -Math.Min(1,storedAmount), true);
                                ResourceStore(resource, transferred);
                            }


                        }
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


        internal static void ResourceStore(PartResourceDefinition resource, double amount)
        {
            StoredResource myStoredResource = selectedFacility.storedResources.Where(r => r.resource == resource).FirstOrDefault();
            if (myStoredResource == null)
            {
                selectedFacility.storedResources.Add(new StoredResource { resource = resource, amount = (float)amount });
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
