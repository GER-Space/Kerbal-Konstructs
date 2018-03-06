using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using KerbalKonstructs.Utilities;
using UnityEngine;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.UI
{
    class FacilityManager : KKWindow
    {
        private static FacilityManager _instance = null;
        internal static FacilityManager instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FacilityManager();

                }
                return _instance;
            }
        }


        Rect targetSelectorRect = new Rect(640, 120, 220, 420);
        public static Rect facilityManagerRect = new Rect(150, 75, 320, 670);

        public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep3", false);

        public static StaticInstance selectedInstance = null;


        float fAlt = 0f;


        public Boolean bHalfwindow = false;
        public Boolean bHalvedWindow = false;


        string sFacilityName = "Unknown";
        string sFacilityType = "Unknown";

        private string sPurpose = "";

        // string sOreTransferAmount = "0";

        Vector3 objectPos = new Vector3(0, 0, 0);

        double disObjectLat = 0;
        double disObjectLon = 0;

        GUIStyle Yellowtext;
        GUIStyle KKWindow;
        GUIStyle DeadButton;
        GUIStyle DeadButtonRed;
        GUIStyle BoxNoBorder;
        GUIStyle LabelInfo;
        GUIStyle ButtonSmallText;

        private bool layoutInitialized = false;


        public override void Close()
        {
            if (KerbalKonstructs.instance.selectedObject != null)
                KerbalKonstructs.instance.deselectObject(true, true);
            base.Close();
        }

        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                if (KerbalKonstructs.instance.selectedObject != null)
                    KerbalKonstructs.instance.deselectObject(true, true);
            }


            StaticInstance soObject = KerbalKonstructs.instance.selectedObject;
            KKWindow = new GUIStyle(GUI.skin.window);
            KKWindow.padding = new RectOffset(3, 3, 5, 5);

            if (bHalfwindow)
            {
                if (!bHalvedWindow)
                {
                    facilityManagerRect = new Rect(facilityManagerRect.xMin, facilityManagerRect.yMin, facilityManagerRect.width, facilityManagerRect.height - 200);
                    bHalvedWindow = true;
                }
            }

            if (!bHalfwindow)
            {
                if (bHalvedWindow)
                {
                    facilityManagerRect = new Rect(facilityManagerRect.xMin, facilityManagerRect.yMin, facilityManagerRect.width, facilityManagerRect.height + 200);
                    bHalvedWindow = false;
                }
            }

            facilityManagerRect = GUI.Window(0xB01B2B5, facilityManagerRect, drawFacilityManagerWindow, "", KKWindow);

        }

        void drawFacilityManagerWindow(int windowID)
        {
           
            if (!layoutInitialized)
            {
                InitializeLayout();
                layoutInitialized = true;
            }

            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(16));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Facility Manager", DeadButton, GUILayout.Height(16));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(16)))
                {
                    selectedInstance = null;
                    this.Close();
                    return;

                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(2);

            if (selectedInstance != null)
            {
                sFacilityType = selectedInstance.FacilityType;

                if (sFacilityType == "GroundStation")
                {
                    sFacilityName = "Ground Station";
                    bHalfwindow = true;
                }
                else
                    sFacilityName = selectedInstance.model.title;

                GUILayout.Box("" + sFacilityName, Yellowtext);
                GUILayout.Space(5);

                fAlt = selectedInstance.RadiusOffset;

                objectPos = KerbalKonstructs.instance.getCurrentBody().transform.InverseTransformPoint(selectedInstance.gameObject.transform.position);
                disObjectLat = KKMath.GetLatitudeInDeg(objectPos);
                disObjectLon = KKMath.GetLongitudeInDeg(objectPos);

                if (disObjectLon < 0) disObjectLon = disObjectLon + 360;

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5);
                    GUILayout.Label("Alt. " + fAlt.ToString("#0.0") + "m", LabelInfo);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Lat. " + disObjectLat.ToString("#0.000"), LabelInfo);
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Lon. " + disObjectLon.ToString("#0.000"), LabelInfo);
                    GUILayout.Space(5);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                sPurpose = "";

                switch (selectedInstance.facilityType)
                {
                    case KKFacilityType.Hangar:
                        {
                            sPurpose = "Craft can be stored in this building for launching from the base at a later date. The building has limited space.";
                            bHalfwindow = true;
                            break;
                        }
                    case KKFacilityType.Barracks:
                        {
                            sPurpose = "This facility provides a temporary home for base-staff. Other facilities can draw staff from the pool available at this facility.";
                            bHalfwindow = true;
                            break;
                        }
                    case KKFacilityType.Research:
                        {
                            sPurpose = "This facility carries out research and generates Science.";
                            bHalfwindow = true;
                            break;
                        }
                    case KKFacilityType.Business:
                        {
                            sPurpose = "This facility carries out business related to the space program in order to generate Funds.";
                            bHalfwindow = true;
                            break;
                        }
                    case KKFacilityType.GroundStation:
                        {
                            sPurpose = "This facility can be a GroundStation for RemoteTech/CommNet";
                            bHalfwindow = true;
                            break;
                        }
                    case KKFacilityType.Merchant:
                        {
                            sPurpose = "You can buy and sell Resources here";
                            bHalfwindow = false;
                            break;
                        }
                    case KKFacilityType.Storage:
                        {
                            sPurpose = "You can store Resources here";
                            bHalfwindow = false;
                            break;
                        }

                }

                GUILayout.Label(sPurpose, LabelInfo);
                GUILayout.Space(2);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(3);

                SharedInterfaces.OpenCloseFacility(selectedInstance);

                GUILayout.Space(2);
                GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                GUILayout.Space(3);

                if (selectedInstance.myFacilities[0].isOpen)
                {


                    switch (selectedInstance.facilityType)
                    {
                        case KKFacilityType.GroundStation:
                            TrackingStationGUI.TrackingInterface(selectedInstance);
                            break;
                        case KKFacilityType.Hangar:
                            HangarGUI.HangarInterface(selectedInstance);
                            break;
                        case KKFacilityType.Research:
                        case KKFacilityType.Business:
                            ProductionGUI.ProductionInterface(selectedInstance, sFacilityType);
                            break;
                        case KKFacilityType.Merchant:
                            MerchantGUI.MerchantInterface(selectedInstance);
                            break;
                        case KKFacilityType.Storage:
                            StorageGUI.StorageInerface(selectedInstance);
                            break;
                    }
                    GUILayout.Space(2);
                    GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                    GUILayout.Space(2);
                    StaffGUI.StaffingInterface(selectedInstance);             
                }
            }

            GUILayout.FlexibleSpace();
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(3);

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        private void InitializeLayout()
        {
            DeadButton = new GUIStyle(GUI.skin.button);
            DeadButton.normal.background = null;
            DeadButton.hover.background = null;
            DeadButton.active.background = null;
            DeadButton.focused.background = null;
            DeadButton.normal.textColor = Color.white;
            DeadButton.hover.textColor = Color.white;
            DeadButton.active.textColor = Color.white;
            DeadButton.focused.textColor = Color.white;
            DeadButton.fontSize = 14;
            DeadButton.fontStyle = FontStyle.Bold;

            DeadButtonRed = new GUIStyle(GUI.skin.button);
            DeadButtonRed.normal.background = null;
            DeadButtonRed.hover.background = null;
            DeadButtonRed.active.background = null;
            DeadButtonRed.focused.background = null;
            DeadButtonRed.normal.textColor = Color.red;
            DeadButtonRed.hover.textColor = Color.yellow;
            DeadButtonRed.active.textColor = Color.red;
            DeadButtonRed.focused.textColor = Color.red;
            DeadButtonRed.fontSize = 12;
            DeadButtonRed.fontStyle = FontStyle.Bold;

            BoxNoBorder = new GUIStyle(GUI.skin.box);
            BoxNoBorder.normal.background = null;
            BoxNoBorder.normal.textColor = Color.white;

            Yellowtext = new GUIStyle(GUI.skin.box);
            Yellowtext.normal.textColor = Color.yellow;
            Yellowtext.normal.background = null;

            LabelInfo = new GUIStyle(GUI.skin.label);
            LabelInfo.normal.background = null;
            LabelInfo.normal.textColor = Color.white;
            LabelInfo.fontSize = 13;
            LabelInfo.fontStyle = FontStyle.Bold;
            LabelInfo.padding.left = 3;
            LabelInfo.padding.top = 0;
            LabelInfo.padding.bottom = 0;

            ButtonSmallText = new GUIStyle(GUI.skin.button);
            ButtonSmallText.fontSize = 12;
            ButtonSmallText.fontStyle = FontStyle.Normal;
        }

    }
}