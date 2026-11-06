using KerbalKonstructs.Core;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.UI
{
    internal class BaseBossFlight : KKWindow
    {
        private static BaseBossFlight _instance = null;
        internal static BaseBossFlight instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BaseBossFlight();

                }
                return _instance;
            }
        }


        public StaticInstance selectedObject = null;

        public Texture tHorizontalSep = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/horizontalsep3", false);
        public Texture tIconClosed = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteclosed", false);
        public Texture tIconOpen = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteopen", false);
        public Texture tToggle = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteopen", false);
        public Texture tToggle2 = GameDatabase.Instance.GetTexture("KerbalKonstructs/Assets/siteopen", false);
        private Rect managerRect = new Rect(10, 25, 320, 520);


        public float iFundsOpen2 = 0f;

        public bool managingFacility = false;
        public bool foundingBase = false;
        public bool bIsOpen = false;

        public bool bShowFacilities = false;
        private static List<StaticInstance> allFacilities = new List<StaticInstance>();

        public static bool bChangeTarget = false;

        public static string sTargetType = "None";
        private Vector2 scrollPos;
        private GUIStyle KKWindow;
        private GUIStyle DeadButton;
        private GUIStyle DeadButtonRed;
        private GUIStyle BoxNoBorder;
        private GUIStyle LabelInfo;

        internal string Base;
        internal string Base2;
        internal float Range;
        internal KKLaunchSite lBase;
        internal KKLaunchSite lBase2;
        internal string smessage = "";
        internal string sClosed;
        internal float fOpenCost;

        private bool isInitialized = false;

        public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                return;
            }

            KKWindow = new GUIStyle(GUI.skin.window)
            {
                padding = new RectOffset(3, 3, 5, 5)
            };

            //if (obj != null)
            //{
            //    if (selectedObject != obj)
            //        EditorGUI.updateSelection(obj);
            //}

            managerRect = GUI.Window(0xB00B1E2, managerRect, DrawBaseManagerWindow, "", KKWindow);
        }

        public override void Close()
        {
            bShowFacilities = false;
            FacilityManager.instance.Close();
            base.Close();
        }

        private void DrawBaseManagerWindow(int windowID)
        {

            if (!isInitialized)
            {
                InitializeLayout();
            }


            GUILayout.BeginHorizontal();
            {
                GUI.enabled = false;
                GUILayout.Button("-KK-", DeadButton, GUILayout.Height(16));

                GUILayout.FlexibleSpace();

                GUILayout.Button("Inflight Base Boss", DeadButton, GUILayout.Height(16));

                GUILayout.FlexibleSpace();

                GUI.enabled = true;

                if (GUILayout.Button("X", DeadButtonRed, GUILayout.Height(16)))
                {
                    bShowFacilities = false;
                    this.Close();
                    return;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(1);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));

            GUILayout.Space(5);
            GUILayout.Box("Flight Tools", BoxNoBorder);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(2);
                GUILayout.Label("Landing Guide ", LabelInfo);
                if (GUILayout.Button(LandingGuideUI.instance.IsOpen() ? tIconOpen : tIconClosed, GUILayout.Height(18), GUILayout.Width(56)))
                {
                    KerbalKonstructs.instance.UpdateCache();
                    LandingGuideUI.instance.Toggle();
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label("NGS ", LabelInfo);

                if (NavGuidanceSystem.instance.IsOpen())
                {
                    tToggle2 = tIconOpen;
                }
                else
                {
                    tToggle2 = tIconClosed;
                }
                if (GUILayout.Button(tToggle2, GUILayout.Height(18), GUILayout.Width(18)))
                {
                    NavGuidanceSystem.instance.Toggle();
                }


                GUILayout.Space(2);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(2);

            GUILayout.Box("Selected Base", BoxNoBorder);

            GUILayout.Space(2);


            GUILayout.BeginHorizontal();
            {
                string sNearestbase = "";
                LaunchSiteManager.getNearestBase(FlightGlobals.ActiveVessel.GetTransform().position, out Base, out Base2, out Range, out lBase, out lBase2);

                if (FlightGlobals.ActiveVessel.altitude > 75000)
                {
                    GUILayout.Label("No base's beacon in range at this altitude.", LabelInfo);
                }
                else
                if (Base == "")
                {
                    GUILayout.Label("No nearest base found.", LabelInfo);
                }
                else
                {
                    if (Range < 10000)
                    {
                        sNearestbase = Base + " at " + Range.ToString("#0.0") + " m";
                    }
                    else
                    {
                        sNearestbase = Base + " at " + (Range / 1000).ToString("#0.0") + " km";
                    }

                    GUILayout.Space(5);
                    GUILayout.Label("Nearest Base: ", LabelInfo);
                    GUILayout.Label(sNearestbase, LabelInfo, GUILayout.Width(150));

                    if (NavGuidanceSystem.instance.IsOpen())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("NGS", GUILayout.Height(21)))
                        {
                            NavGuidanceSystem.setTargetSite(lBase);

                            smessage = "NGS set to " + Base;
                            MiscUtils.HUDMessage(smessage, 10, 2);
                        }
                    }
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(2);
            GUILayout.Box("Base Status", BoxNoBorder);

            if (MiscUtils.isCareerGame())
            {


                if (Range < 5000)
                {
                    LaunchSiteManager.getSiteOpenCloseState(Base, out sClosed, out fOpenCost);
                    fOpenCost /= 2f;

                    if (FlightGlobals.ActiveVessel.Landed && sClosed == "Closed")
                    {
                        GUILayout.Space(2);
                        GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                        GUILayout.Space(2);
                        if (GUILayout.Button("Open Base for " + fOpenCost + " funds", GUILayout.Height(23)))
                        {
                            double currentfunds = Funding.Instance.Funds;

                            if (fOpenCost > currentfunds)
                            {
                                MiscUtils.HUDMessage("Insufficient funds to open this site!", 10, 0);
                            }
                            else
                            {
                                LaunchSiteManager.OpenLaunchSite(LaunchSiteManager.GetLaunchSiteByName(Base));
                                if (MiscUtils.isCareerGame())
                                {
                                    Funding.Instance.AddFunds(-LaunchSiteManager.GetLaunchSiteByName(Base).OpenCost, TransactionReasons.Cheating);
                                }
                                smessage = Base + " opened";
                                MiscUtils.HUDMessage(smessage, 10, 2);
                            }
                        }
                    }

                    if (FlightGlobals.ActiveVessel.Landed && sClosed == "Open")
                    {
                        GUI.enabled = false;
                        GUILayout.Button("Base is Open", GUILayout.Height(23));
                        GUI.enabled = true;
                    }

                    if (FlightGlobals.ActiveVessel.Landed && (sClosed == "OpenLocked" || sClosed == "ClosedLocked"))
                    {
                        GUI.enabled = false;
                        GUILayout.Button("Base cannot be opened or closed", GUILayout.Height(23));
                        GUI.enabled = true;
                    }

                    GUILayout.Space(2);
                    GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                    GUILayout.Space(2);
                }
                else
                {
                    GUILayout.Label("Bases can only be opened or closed at the base when within 5km of the base.", LabelInfo);
                }

                //if (Range > 100000)
                //{
                //    if (bLanded)
                //    {
                //        GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                //        GUILayout.Space(2);
                //        GUILayout.Label("This feature is WIP.", LabelInfo);
                //        GUI.enabled = false;
                //        if (GUILayout.Button("Found a New Base", GUILayout.Height(23)))
                //        {
                //            foundingBase = true;
                //        }
                //        GUI.enabled = true;
                //        GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
                //        GUILayout.Space(2);
                //    }
                //}
                //else
                //{
                //    GUILayout.Label("This feature is WIP.", LabelInfo);
                //}
            }

            GUILayout.Space(2);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(2);
            GUILayout.Box("Operational Facilities", BoxNoBorder);


            if (FlightGlobals.ActiveVessel.Landed)
            {
                if (GUILayout.Button("Show/Hide", GUILayout.Height(23)))
                {
                    if (bShowFacilities)
                    {
                        bShowFacilities = false;
                    }
                    else
                    {
                        CacheFacilities();
                        bShowFacilities = true;
                    }
                }
                if (bShowFacilities && allFacilities.Count == 0)
                {
                    GUILayout.Label("No facilities within 5000m", LabelInfo);
                }


                if (bShowFacilities && allFacilities.Count > 0)
                {
                    scrollPos = GUILayout.BeginScrollView(scrollPos);
                    {
                        for (int i = 0; i < allFacilities.Count; i++)
                        {

                            GUILayout.BeginHorizontal();
                            {
                                bIsOpen = allFacilities[i].myFacilities[0].isOpen;

                                if (!bIsOpen)
                                {
                                    iFundsOpen2 = allFacilities[i].myFacilities[0].OpenCost;
                                    if (iFundsOpen2 == 0)
                                    {
                                        bIsOpen = true;
                                    }
                                }

                                if (GUILayout.Button(allFacilities[i].model.title, GUILayout.Height(23)))
                                {
                                    selectedObject = allFacilities[i];
                                    KerbalKonstructs.SelectInstance(allFacilities[i], false);
                                    FacilityManager.selectedInstance = allFacilities[i];
                                    FacilityManager.instance.Open();
                                }

                                if (bIsOpen)
                                {
                                    GUILayout.Label(tIconOpen, GUILayout.Height(23), GUILayout.Width(23));
                                }

                                if (!bIsOpen)
                                {
                                    GUILayout.Label(tIconClosed, GUILayout.Height(23), GUILayout.Width(23));
                                }
                            }
                            GUILayout.EndHorizontal();

                        }
                    }
                    GUILayout.EndScrollView();
                }
                else
                {
                    GUILayout.Label("Click the button above to display a list of nearby operational facilities.", LabelInfo);
                }
            }
            else
            {
                GUILayout.Label("Nearby facilities can only be shown when landed.", LabelInfo);
                bShowFacilities = false;
            }

            GUILayout.FlexibleSpace();
            GUILayout.Space(2);
            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(2);
            GUILayout.Box("Other Features", BoxNoBorder);
            if (GUILayout.Button("Start Air Racing!", GUILayout.Height(23)))
            {
                AirRacing.instance.Open();
                AirRacing.runningRace = true;
                NavGuidanceSystem.instance.Close();
                FacilityManager.instance.Close();
            }
            if (GUILayout.Button("Basic Orbital Data", GUILayout.Height(23)))
            {
                AirRacing.instance.Open();
                AirRacing.runningRace = false;
                AirRacing.basicorbitalhud = true;
                NavGuidanceSystem.instance.Close();
                FacilityManager.instance.Close();
            }
            GUILayout.Space(5);

            GUILayout.Box(tHorizontalSep, BoxNoBorder, GUILayout.Height(4));
            GUILayout.Space(2);

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        private void InitializeLayout()
        {

            BoxNoBorder = new GUIStyle(GUI.skin.box);
            BoxNoBorder.normal.background = null;
            BoxNoBorder.normal.textColor = Color.white;

            LabelInfo = new GUIStyle(GUI.skin.label);
            LabelInfo.normal.background = null;
            LabelInfo.normal.textColor = Color.white;
            LabelInfo.fontSize = 13;
            LabelInfo.fontStyle = FontStyle.Bold;
            LabelInfo.padding.left = 3;
            LabelInfo.padding.top = 0;
            LabelInfo.padding.bottom = 0;

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

            isInitialized = true;
        }


        /// <summary>
        /// Caches the facilities on button open
        /// </summary>
        private void CacheFacilities()
        {

            StaticInstance[] allStatics = StaticDatabase.allStaticInstances;
            allFacilities = new List<StaticInstance>();

            for (int i = 0; i < allStatics.Length; i++)
            {
                // No facility assigned
                if (!allStatics[i].hasFacilities)
                {
                    continue;
                }
                //not anywhere here
                if (!allStatics[i].isActive)
                {
                    continue;
                }
                if (allStatics[i].myFacilities.Count == 0)
                {
                    continue;
                }
                // Facility is more than 5000m away
                if (Vector3.Distance(FlightGlobals.ActiveVessel.GetTransform().position, allStatics[i].position) > 5000f)
                {
                    continue;
                }
                // is not a facility
                if (string.Equals(allStatics[i].FacilityType, "None", StringComparison.CurrentCultureIgnoreCase))
                {
                    continue;
                }

                allFacilities.Add(allStatics[i]);
            }
        }


    }
}
