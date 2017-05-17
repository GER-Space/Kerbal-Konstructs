using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    public class FuelTanksGUI
    {
        public static Boolean bLqFIn = false;
        public static Boolean bLqFOut = false;
        public static Boolean bOxFIn = false;
        public static Boolean bOxFOut = false;
        public static Boolean bMoFIn = false;
        public static Boolean bMoFOut = false;
        public static Boolean PartSelected = false;

        public static Boolean bOrderedLqF = false;
        public static Boolean bOrderedOxF = false;
        public static Boolean bOrderedMoF = false;

        public static PartResource SelectedResource = null;
        public static Part SelectedTank = null;

        public static float fLqFMax = 0;
        public static float fLqFCurrent = 0;
        public static float fOxFMax = 0;
        public static float fOxFCurrent = 0;
        public static float fMoFMax = 0;
        public static float fMoFCurrent = 0;

        public static float fTransferRate = 0.01f;

        public static string fOxFAmount = "0.00";
        public static string fLqFAmount = "0.00";
        public static string fMoFAmount = "0.00";

        public static Vector2 scrollPos3;
        public static Vector2 scrollPos4;
        public static GUIStyle LabelInfo;
        public static GUIStyle BoxInfo;

        public static string getResourceAlt(StaticInstance instance, string sOriginal)
        {
            string sAlt = sOriginal;

            FuelTanks myTank = instance.myFacilities[0] as FuelTanks;

            if (sOriginal == "LiquidFuel") sAlt = myTank.LqFAlt;
            if (sOriginal == "Oxidizer") sAlt = myTank.OxFAlt;
            if (sOriginal == "MonoPropellant") sAlt = myTank.MoFAlt;

            if (sAlt != "") return sAlt;
            else
                return sOriginal;
        }

        public static void FuelTanksInterface(StaticInstance selectedObject)
        {
            string smessage = "";

            FuelTanks myTank = selectedObject.myFacilities[0] as FuelTanks;

            string sFacilityName = selectedObject.model.title;
            string sFacilityRole = selectedObject.FacilityType;

            string sResource1 = "LiquidFuel";
            string sResource2 = "Oxidizer";
            string sResource3 = "MonoPropellant";

            fLqFMax = myTank.LqFMax;
            fLqFCurrent = myTank.LqFCurrent;
            fOxFMax = myTank.OxFMax;
            fOxFCurrent = myTank.OxFCurrent;
            fMoFMax = myTank.MoFMax;
            fMoFCurrent = myTank.MoFCurrent;

            float fPurchaseRate = fTransferRate * 100f;

            LabelInfo = new GUIStyle(GUI.skin.label);
            LabelInfo.normal.background = null;
            LabelInfo.normal.textColor = Color.white;
            LabelInfo.fontSize = 13;
            LabelInfo.fontStyle = FontStyle.Bold;
            LabelInfo.padding.left = 3;
            LabelInfo.padding.top = 0;
            LabelInfo.padding.bottom = 0;

            BoxInfo = new GUIStyle(GUI.skin.box);
            BoxInfo.normal.textColor = Color.cyan;
            BoxInfo.fontSize = 13;
            BoxInfo.padding.top = 2;
            BoxInfo.padding.bottom = 1;
            BoxInfo.padding.left = 5;
            BoxInfo.padding.right = 5;
            BoxInfo.normal.background = null;

            if (!FlightGlobals.ActiveVessel.Landed)
            {
                GUILayout.Box("A vessel must be landed to use this facility.", BoxInfo);
                LockFuelTank();
            }

            var vDist = Vector3.Distance(selectedObject.gameObject.transform.position, FlightGlobals.ActiveVessel.transform.position);

            if ((double)vDist < KerbalKonstructs.instance.facilityUseRange)
            { }
            else
            {
                GUILayout.Box("A vessel must be in range to use this facility." + Environment.NewLine + "Come " + Math.Round(vDist - KerbalKonstructs.instance.facilityUseRange, 1) + "m closer", BoxInfo);
                LockFuelTank();
            }

            sResource1 = getResourceAlt(selectedObject, "LiquidFuel");
            sResource2 = getResourceAlt(selectedObject, "Oxidizer");
            sResource3 = getResourceAlt(selectedObject, "MonoPropellant");

            GUILayout.Space(3);
            GUILayout.Label("Fuel Stores", LabelInfo);
            scrollPos4 = GUILayout.BeginScrollView(scrollPos4);
            if (fLqFMax > 0)
            {
                GUILayout.Label(sResource1, LabelInfo);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Max ", LabelInfo);
                GUI.enabled = false;
                GUILayout.TextField(string.Format("{0}", fLqFMax), GUILayout.Height(18));
                GUI.enabled = true;
                GUILayout.Label("Current ", LabelInfo);
                GUI.enabled = false;
                GUILayout.TextField(fLqFCurrent.ToString("#0.00"), GUILayout.Height(18));
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Order", GUILayout.Height(18)))
                {
                    LockFuelTank();
                    bOrderedLqF = true;
                }
                GUI.enabled = !bLqFIn;
                if (GUILayout.Button("In", GUILayout.Height(18)))
                {
                    bLqFIn = true;
                    bLqFOut = false;
                }
                GUI.enabled = !bLqFOut;
                if (GUILayout.Button("Out", GUILayout.Height(18)))
                {
                    bLqFOut = true;
                    bLqFIn = false;
                }
                GUI.enabled = bLqFIn || bLqFOut;
                if (GUILayout.Button("Stop", GUILayout.Height(18)))
                {
                    bLqFIn = false;
                    bLqFOut = false;
                    smessage = "Fuel transfer stopped";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            if (bOrderedLqF)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.RepeatButton("-", GUILayout.Height(18)))
                {
                    fLqFAmount = (float.Parse(fLqFAmount) - fPurchaseRate).ToString();
                    if ((float.Parse(fLqFAmount)) < 0f) fLqFAmount = "0.00";
                }
                GUI.enabled = false;
                GUILayout.TextField(fLqFAmount, GUILayout.Height(18));
                GUI.enabled = true;
                if (GUILayout.RepeatButton("+", GUILayout.Height(18)))
                {
                    fLqFAmount = (float.Parse(fLqFAmount) + fPurchaseRate).ToString();
                    if ((float.Parse(fLqFAmount)) > (fLqFMax - fLqFCurrent)) fLqFAmount = (fLqFMax - fLqFCurrent).ToString();
                }

                if (GUILayout.Button("Max", GUILayout.Height(18)))
                {
                    fLqFAmount = (fLqFMax - fLqFCurrent).ToString();
                    if ((float.Parse(fLqFAmount)) < 0f) fLqFAmount = "0.00";
                }

                float flqFPrice = 0.5f;

                float fLqFCost = (float.Parse(fLqFAmount)) * flqFPrice;
                GUILayout.Label("Cost: " + fLqFCost.ToString("#0") + " \\F", LabelInfo);
                if (GUILayout.Button("Buy", GUILayout.Height(18)))
                {
                    if (myTank.LqFCurrent + (float.Parse(fLqFAmount)) > fLqFMax)
                    {
                        MiscUtils.HUDMessage("Insufficient fuel capacity!", 10, 0);
                        fLqFAmount = "0.00";
                    }
                    else
                    {
                        if (MiscUtils.isCareerGame())
                        {
                            double currentfunds = Funding.Instance.Funds;

                            if (fLqFCost > currentfunds)
                            {
                                MiscUtils.HUDMessage("Insufficient funds!", 10, 0);
                            }
                            else
                            {
                                Funding.Instance.AddFunds(-fLqFCost, TransactionReasons.Cheating);
                                myTank.LqFCurrent = myTank.LqFCurrent + (float.Parse(fLqFAmount));
                            }
                        }
                        else
                        {
                            myTank.LqFCurrent = myTank.LqFCurrent + (float.Parse(fLqFAmount));
                        }
                    }

                }
                if (GUILayout.Button("Done", GUILayout.Height(18)))
                {
                    bOrderedLqF = false;
                }
                GUILayout.EndHorizontal();
            }

            if (fOxFMax > 0)
            {
                GUILayout.Label(sResource2, LabelInfo);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Max ", LabelInfo);
                GUI.enabled = false;
                GUILayout.TextField(string.Format("{0}", fOxFMax), GUILayout.Height(18));
                GUI.enabled = true;
                GUILayout.Label("Current ", LabelInfo);
                GUI.enabled = false;
                GUILayout.TextField(fOxFCurrent.ToString("#0.00"), GUILayout.Height(18));
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Order", GUILayout.Height(18)))
                {
                    LockFuelTank();
                    bOrderedOxF = true;
                }
                GUI.enabled = !bOxFIn;
                if (GUILayout.Button("In", GUILayout.Height(18)))
                {
                    bOxFIn = true;
                    bOxFOut = false;
                }
                GUI.enabled = !bOxFOut;
                if (GUILayout.Button("Out", GUILayout.Height(18)))
                {
                    bOxFOut = true;
                    bOxFIn = false;
                }
                GUI.enabled = bOxFIn || bOxFOut;
                if (GUILayout.Button("Stop", GUILayout.Height(18)))
                {
                    bOxFIn = false;
                    bOxFOut = false;
                    smessage = "Fuel transfer stopped";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            if (bOrderedOxF)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.RepeatButton("-", GUILayout.Height(18)))
                {
                    fOxFAmount = (float.Parse(fOxFAmount) - fPurchaseRate).ToString();
                    if ((float.Parse(fOxFAmount)) < 0f) fOxFAmount = "0.00";
                }
                GUI.enabled = false;
                GUILayout.TextField(fOxFAmount, GUILayout.Height(18));
                GUI.enabled = true;
                if (GUILayout.RepeatButton("+", GUILayout.Height(18)))
                {
                    fOxFAmount = (float.Parse(fOxFAmount) + fPurchaseRate).ToString();
                    if ((float.Parse(fOxFAmount)) > (fOxFMax - fOxFCurrent)) fOxFAmount = (fOxFMax - fOxFCurrent).ToString();
                }

                if (GUILayout.Button("Max", GUILayout.Height(18)))
                {
                    fOxFAmount = (fOxFMax - fOxFCurrent).ToString();
                    if ((float.Parse(fOxFAmount)) < 0f) fOxFAmount = "0.00";
                }

                float fOxFPrice = 1.5f;

                float fOxFCost = (float.Parse(fOxFAmount)) * fOxFPrice;
                GUILayout.Label("Cost: " + fOxFCost.ToString("#0") + " \\F", LabelInfo);
                if (GUILayout.Button("Buy", GUILayout.Height(18)))
                {
                    if (myTank.OxFCurrent + (float.Parse(fOxFAmount)) > fOxFMax)
                    {
                        MiscUtils.HUDMessage("Insufficient fuel capacity!", 10, 0);
                        fOxFAmount = "0.00";
                    }
                    else
                    {
                        if (MiscUtils.isCareerGame())
                        {
                            double currentfunds = Funding.Instance.Funds;

                            if (fOxFCost > currentfunds)
                            {
                                MiscUtils.HUDMessage("Insufficient funds!", 10, 0);
                            }
                            else
                            {
                                Funding.Instance.AddFunds(-fOxFCost, TransactionReasons.Cheating);
                                myTank.OxFCurrent = myTank.OxFCurrent + (float.Parse(fOxFAmount));
                            }
                        }
                        else
                        {
                            myTank.OxFCurrent = myTank.OxFCurrent + (float.Parse(fOxFAmount));
                        }
                    }

                }
                if (GUILayout.Button("Done", GUILayout.Height(18)))
                {
                    bOrderedOxF = false;
                }
                GUILayout.EndHorizontal();
            }

            if (fMoFMax > 0)
            {
                GUILayout.Label(sResource3, LabelInfo);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Max ", LabelInfo);
                GUI.enabled = false;
                GUILayout.TextField(string.Format("{0}", fMoFMax), GUILayout.Height(18));
                GUI.enabled = true;
                GUILayout.Label("Current ", LabelInfo);
                GUI.enabled = false;
                GUILayout.TextField(fMoFCurrent.ToString("#0.00"), GUILayout.Height(18));
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Order", GUILayout.Height(18)))
                {
                    LockFuelTank();
                    bOrderedMoF = true;
                }
                GUI.enabled = !bMoFIn;
                if (GUILayout.Button("In", GUILayout.Height(18)))
                {
                    bMoFIn = true;
                    bMoFOut = false;
                }
                GUI.enabled = !bMoFOut;
                if (GUILayout.Button("Out", GUILayout.Height(18)))
                {
                    bMoFOut = true;
                    bMoFIn = false;
                }
                GUI.enabled = bMoFIn || bMoFOut;
                if (GUILayout.Button("Stop", GUILayout.Height(18)))
                {
                    bMoFIn = false;
                    bMoFOut = false;
                    smessage = "Fuel transfer stopped";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();
            }

            if (bOrderedMoF)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.RepeatButton("-", GUILayout.Height(18)))
                {
                    fMoFAmount = (float.Parse(fMoFAmount) - fPurchaseRate).ToString();
                    if ((float.Parse(fMoFAmount)) < 0f) fMoFAmount = "0.00";
                }
                GUI.enabled = false;
                GUILayout.TextField(fMoFAmount, GUILayout.Height(18));
                GUI.enabled = true;
                if (GUILayout.RepeatButton("+", GUILayout.Height(18)))
                {
                    fMoFAmount = (float.Parse(fMoFAmount) + fPurchaseRate).ToString();
                    if ((float.Parse(fMoFAmount)) > (fMoFMax - fMoFCurrent)) fMoFAmount = (fMoFMax - fMoFCurrent).ToString();
                }

                if (GUILayout.Button("Max", GUILayout.Height(18)))
                {
                    fMoFAmount = (fMoFMax - fMoFCurrent).ToString();
                    if ((float.Parse(fMoFAmount)) < 0f) fMoFAmount = "0.00";
                }

                float fMoFPrice = 1.2f;

                float fMoFCost = (float.Parse(fMoFAmount)) * fMoFPrice;
                GUILayout.Label("Cost: " + fMoFCost.ToString("#0") + " \\F", LabelInfo);
                if (GUILayout.Button("Buy", GUILayout.Height(18)))
                {
                    if (myTank.MoFCurrent + (float.Parse(fMoFAmount)) > fMoFMax)
                    {
                        MiscUtils.HUDMessage("Insufficient fuel capacity!", 10, 0);
                        fMoFAmount = "0.00";
                    }
                    else
                    {
                        if (MiscUtils.isCareerGame())
                        {
                            double currentfunds = Funding.Instance.Funds;

                            if (fMoFCost > currentfunds)
                            {
                                MiscUtils.HUDMessage("Insufficient funds!", 10, 0);
                            }
                            else
                            {
                                Funding.Instance.AddFunds(-fMoFCost, TransactionReasons.Cheating);
                                myTank.MoFCurrent = myTank.MoFCurrent + (float.Parse(fMoFAmount));
                            }
                        }
                        else
                        {
                            myTank.MoFCurrent = myTank.MoFCurrent + (float.Parse(fMoFAmount));
                        }
                    }

                }
                if (GUILayout.Button("Done", GUILayout.Height(18)))
                {
                    bOrderedMoF = false;
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            if (fOxFMax > 0 || fLqFMax > 0 || fMoFMax > 0)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Transfer Rate", LabelInfo);

                GUI.enabled = (fTransferRate != 0.01f);
                if (GUILayout.Button("x1", GUILayout.Height(18)))
                {
                    fTransferRate = 0.01f;
                    smessage = "Fuel transfer rate set to x1";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUI.enabled = (fTransferRate != 0.04f);
                if (GUILayout.Button("x4", GUILayout.Height(18)))
                {
                    fTransferRate = 0.04f;
                    smessage = "Fuel transfer rate set to x4";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUI.enabled = (fTransferRate != 0.1f);
                if (GUILayout.Button("x10", GUILayout.Height(18)))
                {
                    fTransferRate = 0.1f;
                    smessage = "Fuel transfer rate set to x10";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUI.enabled = (fTransferRate != 0.25f);
                if (GUILayout.Button("x25", GUILayout.Height(18)))
                {
                    fTransferRate = 0.25f;
                    smessage = "Fuel transfer rate set to x25";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUI.enabled = (fTransferRate != 1.0f);
                if (GUILayout.Button("x100", GUILayout.Height(18)))
                {
                    fTransferRate = 1.0f;
                    smessage = "Fuel transfer rate set to x100";
                    MiscUtils.HUDMessage(smessage, 10, 2);
                }
                GUI.enabled = true;
                GUILayout.EndHorizontal();

                if (!FlightGlobals.ActiveVessel.isEVA && FlightGlobals.ActiveVessel.Landed)
                {
                    GUILayout.Label(FlightGlobals.ActiveVessel.vesselName + "'s Tanks", LabelInfo);

                    scrollPos3 = GUILayout.BeginScrollView(scrollPos3);
                    foreach (Part fTank in FlightGlobals.ActiveVessel.parts)
                    {
                        foreach (PartResource rResource in fTank.Resources)
                        {
                            if (rResource.resourceName == sResource1 || rResource.resourceName == sResource2 || rResource.resourceName == sResource3)
                            {
                                if (SelectedTank == fTank && SelectedResource == rResource)
                                    PartSelected = true;
                                else
                                    PartSelected = false;

                                GUILayout.BeginHorizontal();
                                GUILayout.Box("" + fTank.gameObject.name, GUILayout.Height(18));
                                GUILayout.Box("" + rResource.resourceName, GUILayout.Height(18));
                                GUILayout.EndHorizontal();

                                GUILayout.BeginHorizontal();
                                GUILayout.Label("Fuel", LabelInfo);
                                GUI.enabled = false;
                                GUILayout.TextField("" + rResource.amount.ToString("#0.00"), GUILayout.Height(18));
                                GUI.enabled = true;

                                GUI.enabled = !PartSelected;
                                if (GUILayout.Button(" Select ", GUILayout.Height(18)))
                                {
                                    SelectedResource = rResource;
                                    SelectedTank = fTank;
                                }

                                GUI.enabled = PartSelected;
                                if (GUILayout.Button("Deselect", GUILayout.Height(18)))
                                {
                                    SelectedResource = null;
                                    SelectedTank = null;
                                }
                                GUI.enabled = true;
                                GUILayout.EndHorizontal();
                            }
                            else
                                continue;
                        }
                    }
                    GUILayout.EndScrollView();

                    GUI.enabled = true;

                    if (SelectedResource != null && SelectedTank != null)
                    {
                        if (bMoFOut || bOxFOut || bLqFOut)
                            doFuelOut(selectedObject);
                        if (bMoFIn || bOxFIn || bLqFIn)
                            doFuelIn(selectedObject);
                    }
                }
            }

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        public static void doFuelOut(StaticInstance selectedObject)
        {
            FuelTanks myTank = selectedObject.myFacilities[0] as FuelTanks;
            if (SelectedResource == null) return;
            if (SelectedTank == null) return;

            string sResource1 = getResourceAlt(selectedObject, "LiquidFuel");
            string sResource2 = getResourceAlt(selectedObject, "Oxidizer");
            string sResource3 = getResourceAlt(selectedObject, "MonoPropellant");

            if (SelectedResource.resourceName == sResource3 && !bMoFOut) return;
            if (SelectedResource.resourceName == sResource1 && !bLqFOut) return;
            if (SelectedResource.resourceName == sResource2 && !bOxFOut) return;

            if (SelectedResource.resourceName == sResource3 && fMoFCurrent <= 0) return;
            if (SelectedResource.resourceName == sResource1 && fLqFCurrent <= 0) return;
            if (SelectedResource.resourceName == sResource2 && fOxFCurrent <= 0) return;

            if (SelectedResource.amount >= SelectedResource.maxAmount) return;

            float dStaticFuel;

            SelectedResource.amount = SelectedResource.amount + fTransferRate;
            if (SelectedResource.amount > SelectedResource.maxAmount) SelectedResource.amount = SelectedResource.maxAmount;

            if (SelectedResource.resourceName == sResource3)
            {
                dStaticFuel = myTank.MoFCurrent - fTransferRate;
                if (dStaticFuel < 0) dStaticFuel = 0;
                myTank.MoFCurrent = dStaticFuel;
            }
            if (SelectedResource.resourceName == sResource1)
            {
                dStaticFuel = myTank.LqFCurrent - fTransferRate;
                if (dStaticFuel < 0) dStaticFuel = 0;
                myTank.LqFCurrent =  dStaticFuel;
            }
            if (SelectedResource.resourceName == sResource2)
            {
                dStaticFuel = myTank.OxFCurrent - fTransferRate;
                if (dStaticFuel < 0) dStaticFuel = 0;
                myTank.OxFCurrent =  dStaticFuel;
            }
        }

        public static void doFuelIn(StaticInstance selectedObject)
        {
            FuelTanks myTank = selectedObject.myFacilities[0] as FuelTanks;
            if (SelectedResource == null) return;
            if (SelectedTank == null) return;

            string sResource1 = getResourceAlt(selectedObject, "LiquidFuel");
            string sResource2 = getResourceAlt(selectedObject, "Oxidizer");
            string sResource3 = getResourceAlt(selectedObject, "MonoPropellant");

            if (SelectedResource.resourceName == sResource3 && !bMoFIn) return;
            if (SelectedResource.resourceName == sResource1 && !bLqFIn) return;
            if (SelectedResource.resourceName == sResource2 && !bOxFIn) return;

            if (SelectedResource.resourceName == sResource3 && fMoFCurrent >= fMoFMax) return;
            if (SelectedResource.resourceName == sResource1 && fLqFCurrent >= fLqFMax) return;
            if (SelectedResource.resourceName == sResource2 && fOxFCurrent >= fOxFMax) return;

            if (SelectedResource.amount <= 0) return;

            float dStaticFuel;

            SelectedResource.amount = SelectedResource.amount - fTransferRate;
            if (SelectedResource.amount < 0) SelectedResource.amount = 0;

            if (SelectedResource.resourceName == sResource3)
            {
                dStaticFuel = myTank.MoFCurrent + fTransferRate;
                if (dStaticFuel > fMoFMax) dStaticFuel = fMoFMax;
                myTank.MoFCurrent =  dStaticFuel;
            }
            if (SelectedResource.resourceName == sResource1)
            {
                dStaticFuel = myTank.LqFCurrent + fTransferRate;
                if (dStaticFuel > fLqFMax) dStaticFuel = fLqFMax;
                myTank.LqFCurrent = dStaticFuel;
            }
            if (SelectedResource.resourceName == sResource2)
            {
                dStaticFuel = myTank.OxFCurrent + fTransferRate;
                if (dStaticFuel > fOxFMax) dStaticFuel = fOxFMax;
                myTank.OxFCurrent = dStaticFuel;
            }
        }

        public static void LockFuelTank()
        {
            SelectedResource = null;
            SelectedTank = null;
            bLqFIn = false;
            bLqFOut = false;
            bOxFIn = false;
            bOxFOut = false;
            bMoFIn = false;
            bMoFOut = false;
        }
    }
}
