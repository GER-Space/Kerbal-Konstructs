using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    internal class MerchantGUI
    {


        internal static bool layoutInitialized = false;
        internal static GUIStyle LabelInfo;
        internal static Merchant selectedFacility = null;

        internal static StaticInstance lastInstance = null;

        internal static Vessel currentVessel = null;


        internal static void MerchantInterface(StaticInstance instance)
        {
            if (!layoutInitialized)
            {
                InitializeLayout();
                layoutInitialized = true;
            }

            if (instance != lastInstance)
            {
                Initialize(instance);
            }

            GUILayout.Space(2);
            GUILayout.Label("Buy or sell these resources: ", LabelInfo, GUILayout.Height(23));

            foreach (TradedResource myResource in selectedFacility.tradedResources)
            {
                currentVessel.resourcePartSet.GetConnectedResourceTotals(myResource.resource.id, out double amount, out double maxAmount, false);
                // check if we can store this resource
                if (maxAmount == 0 )
                {
                    continue;
                }
                GUILayout.Label(myResource.resource.name + ",  Buy for: "  + (myResource.resource.unitCost * myResource.multiplierBuy), GUILayout.Height(23));

                foreach (PartSet xFeedSet in currentVessel.crossfeedSets) {
                    xFeedSet.GetConnectedResourceTotals(myResource.resource.id,out double xfeedAmount, out double xfeedMax, true);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(xfeedAmount.ToString() + " of " + xfeedMax.ToString(), LabelInfo, GUILayout.Height(23));
                        GUILayout.FlexibleSpace();
                        if (GUILayout.RepeatButton("Buy", GUILayout.Height(18), GUILayout.Width(50))) {
                            double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), myResource.resource.id, -1, true);
                            // Geld abbuchen
                        }
                        if (GUILayout.RepeatButton("Sell", GUILayout.Height(18), GUILayout.Width(50))) {
                            double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), myResource.resource.id, 1, true);
                            // Geld abbuchen
                        }

                    }
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.Space(2);
            GUILayout.FlexibleSpace();


            GUILayout.BeginHorizontal();

            GUILayout.Label("Nothing here: ", GUILayout.Height(23));

            GUILayout.EndHorizontal();
            GUILayout.Space(5);
        }

        internal static void Initialize(StaticInstance instance)
        {
            selectedFacility = instance.myFacilities[0] as Merchant;
            lastInstance = instance;
            currentVessel = FlightGlobals.ActiveVessel;
        }



        internal static void InitializeLayout()
        {
            LabelInfo = new GUIStyle(GUI.skin.label);
            LabelInfo.normal.background = null;
            LabelInfo.normal.textColor = Color.white;
            LabelInfo.fontSize = 13;
            LabelInfo.fontStyle = FontStyle.Bold;
            LabelInfo.padding.left = 3;
            LabelInfo.padding.top = 0;
            LabelInfo.padding.bottom = 0;
        }

    }
}
