using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using UnityEngine;

using KodeUI;

namespace KerbalKonstructs.UI
{
    internal class MerchantGUI : VerticalLayout
    {

        internal static Merchant selectedFacility = null;
        internal static StaticInstance lastInstance = null;

        internal static Vessel currentVessel = null;

        private static Vector2 facilityscroll;
        private static float increment = 10f;

		public override void CreateUI()
		{
			base.CreateUI();
		}

		public void UpdateUI(StaticInstance instance)
		{
		}

        internal static void MerchantInterface(StaticInstance instance)
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

            GUILayout.Label("Buy or sell these resources: ", GUILayout.Height(30));

            GUILayout.Space(2);
            facilityscroll = GUILayout.BeginScrollView(facilityscroll);
            {
                foreach (TradedResource myResource in selectedFacility.tradedResources)
                {
                    currentVessel.resourcePartSet.GetConnectedResourceTotals(myResource.resource.id, out double amount, out double maxAmount, false);
                    // check if we can store this resource
                    //if (maxAmount == 0 )
                    //{
                    //    continue;
                    //}
                    GUILayout.Box(UIMain.tHorizontalSep.texture, UIMain.BoxNoBorderW, GUILayout.Height(4));
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(myResource.resource.name, GUILayout.Height(23), GUILayout.Width(120));
                        GUILayout.FlexibleSpace();
                        if (myResource.canBeSold)
                        {
                            GUILayout.Label("Sell for: ", GUILayout.Height(23));
                            GUILayout.Label((myResource.resource.unitCost * myResource.multiplierSell).ToString(), UIMain.LabelInfo, GUILayout.Height(23));
                        }
                        else
                        {
                            GUILayout.Space(100);
                        }
                        GUILayout.FlexibleSpace();
                        if (myResource.canBeBought)
                        {
                            GUILayout.Label("Buy for: ", GUILayout.Height(23));
                            GUILayout.Label((myResource.resource.unitCost * myResource.multiplierBuy).ToString(), UIMain.LabelInfo, GUILayout.Height(23));
                            GUILayout.Space(10);
                        }
                        else
                        {
                            GUILayout.Space(100);
                        }
                    }
                    GUILayout.EndHorizontal();
                    foreach (PartSet xFeedSet in currentVessel.crossfeedSets)
                    {
                        xFeedSet.GetConnectedResourceTotals(myResource.resource.id, out double xfeedAmount, out double xfeedMax, true);
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label(Math.Round(xfeedAmount, 1).ToString() + " of " + Math.Round(xfeedMax, 1).ToString(), UIMain.LabelInfo, GUILayout.Height(18), GUILayout.Width(120));
                            GUILayout.FlexibleSpace();
                            if (myResource.canBeSold)
                            {
                                if (GUILayout.RepeatButton("--", GUILayout.Height(18), GUILayout.Width(32)))
                                {
                                    double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), myResource.resource.id, increment, true);
                                    BookCredits(transferred * myResource.resource.unitCost * myResource.multiplierSell);
                                }
                                if (GUILayout.Button("-", GUILayout.Height(18), GUILayout.Width(32)))
                                {
                                    double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), myResource.resource.id, increment, true);
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
                                GUI.enabled = ((HighLogic.CurrentGame.Mode != Game.Modes.CAREER) || (Funding.Instance.Funds > (myResource.resource.unitCost * increment * myResource.multiplierBuy)));
                                if (GUILayout.Button("+", GUILayout.Height(18), GUILayout.Width(32)) || GUILayout.RepeatButton("++", GUILayout.Height(18), GUILayout.Width(32)))
                                {
                                    double transferred = xFeedSet.RequestResource(xFeedSet.GetParts().ToList().First(), myResource.resource.id, -increment, true);
                                    BookCredits(transferred * myResource.resource.unitCost * myResource.multiplierBuy);
                                }
                                GUI.enabled = true;
                            }
                            else
                            {
                                GUILayout.Space(64);
                            }
                        }
                        GUILayout.Space(16);
                        GUILayout.EndHorizontal();
                    }
                    //GUILayout.Space(4);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.Box(UIMain.tHorizontalSep.texture, UIMain.BoxNoBorderW, GUILayout.Height(4));
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
    }
}
