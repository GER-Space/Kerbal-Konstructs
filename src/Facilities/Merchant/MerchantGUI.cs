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

            if (!selectedFacility.isOpen)
            {
                return;
            }

            GUILayout.Space(2);
            GUILayout.Label("Buy or sell these resources: ", GUILayout.Height(30));

            foreach (TradedResource myResource in selectedFacility.tradedResources)
            {
                currentVessel.resourcePartSet.GetConnectedResourceTotals(myResource.resource.id, out double amount, out double maxAmount, false);
                // check if we can store this resource
                //if (maxAmount == 0 )
                //{
                //    continue;
                //}
                GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorderW, GUILayout.Height(4));
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(myResource.resource.name, GUILayout.Height(23));
                    GUILayout.FlexibleSpace();
                    if (myResource.canBeSold)
                    {
                        GUILayout.Label("Sell for: ", GUILayout.Height(23));
                        GUILayout.Label((myResource.resource.unitCost * myResource.multiplierSell).ToString(), LabelInfo, GUILayout.Height(23));
                    }
                    else
                    {
                        GUILayout.Space(100);
                    }
                    GUILayout.FlexibleSpace();
                    if (myResource.canBeBought)
                    {
                        GUILayout.Label("Buy for: ", GUILayout.Height(23));
                        GUILayout.Label((myResource.resource.unitCost * myResource.multiplierBuy).ToString(), LabelInfo, GUILayout.Height(23));
                        GUILayout.Space(10);
                    }
                    else
                    {
                        GUILayout.Space(100);
                    }
                }
                GUILayout.EndHorizontal();
                foreach (PartSet xFeedSet in currentVessel.crossfeedSets) {
                    xFeedSet.GetConnectedResourceTotals(myResource.resource.id,out double xfeedAmount, out double xfeedMax, true);
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(Math.Round(xfeedAmount,1).ToString() + " of " + Math.Round(xfeedMax,1).ToString(), LabelInfo, GUILayout.Height(18));
                        GUILayout.FlexibleSpace();
                        if (myResource.canBeSold)
                        {
                            if (GUILayout.RepeatButton("--", GUILayout.Height(18), GUILayout.Width(32)))
                            {
                                double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), myResource.resource.id, 1, true);
                                BookCredits(transferred * myResource.resource.unitCost * myResource.multiplierSell);
                            }
                            if ( GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(32)))
                            {
                                double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), myResource.resource.id, 1, true);
                                BookCredits(transferred * myResource.resource.unitCost * myResource.multiplierSell);
                            }
                        }
                        else
                        {
                            GUILayout.Space(64);
                        }
                        GUILayout.FlexibleSpace();
                        if (myResource.canBeBought)
                        {
                            // check if we have enough money to buy the resources
                            GUI.enabled = ((HighLogic.CurrentGame.Mode != Game.Modes.CAREER) || (Funding.Instance.Funds > (myResource.resource.unitCost * myResource.multiplierBuy)));
                            if (GUILayout.Button("+", GUILayout.Height(18), GUILayout.Width(32)) || GUILayout.RepeatButton("++", GUILayout.Height(18), GUILayout.Width(32)))
                            {
                                double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), myResource.resource.id, -1, true);
                                BookCredits(transferred * myResource.resource.unitCost * myResource.multiplierBuy);
                            }
                            GUI.enabled = true;
                        }
                        else
                        {
                            GUILayout.Space(64);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                //GUILayout.Space(4);
            }

            GUILayout.Box(UIMain.tHorizontalSep, UIMain.BoxNoBorderW, GUILayout.Height(4));
            if (!String.IsNullOrEmpty(selectedFacility.FacilityName))
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Thanks for trading at: ");
                    GUILayout.Box(selectedFacility.FacilityName, UIMain.Yellowtext);
                }
            GUILayout.EndHorizontal();
        }
            //GUILayout.FlexibleSpace();
        }

        internal static void Initialize(StaticInstance instance)
        {
            selectedFacility = instance.myFacilities[0] as Merchant;
            lastInstance = instance;
            currentVessel = FlightGlobals.ActiveVessel;
        }


        private static void BookCredits(double amount)
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER)
            {
                Funding.Instance.AddFunds(amount,TransactionReasons.Vessels);
               // Log.Normal("Transaction for: " + (amount).ToString());
            }

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
