using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KerbalKonstructs.Core;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    internal class HangarKSCGUI
    {

        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "KSCHangar";
        internal static string windowMessage = null;
        internal static string windowTitle = "All stored Vessels";

        internal static Rect windowRect;

        internal static float windowWidth = 720;
        internal static float windowHeight = 200f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;


        internal static bool setToParent = false;

        internal static bool checkForParent = false;
        internal static Func<bool> parentWindow = KSCManager.IsOpen;


        internal static bool useFromFlight = false;

        private static ProtoVessel selectedVessel = null;

        private static int selectedSeat = -1;
        private static ProtoPartSnapshot selectedPart = null;

        private static DialogGUIScrollList seatList;
        private static DialogGUIScrollList availableCrewList;

        private static Modules.Hangar.StoredVessel selectedStoredVessel;
        private static Modules.Hangar selectedHangar;
        internal static StaticInstance selectedFacility;


        internal static void CreateContent()
        {
            content.Add(new DialogGUILabel("SpaceCenter,   VesselName", KKStyle.whiteLabel));
            content.Add(vesselList);
            content.Add(new DialogGUILabel(delegate { string text = (selectedVessel == null) ? "none " : "Selected Vessel: " + selectedVessel.vesselName ;  return text; }, KKStyle.whiteLabel)); ;
            content.Add( new DialogGUIHorizontalLayout( seatList, availableCrewList));
            content.Add(
                new DialogGUIHorizontalLayout(
                new DialogGUIButton("Close", delegate { Close(); }, 80, 25, false),
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("Launch Vessel", delegate { Close(); LaunchVessel(); }, 120, 25, false)
                )) ;
        }


        internal static void KKTitle()
        {
            if (!showKKTitle)
            {
                return;
            }
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("-KK-", KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),

                new DialogGUILabel(windowTitle, KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("X", delegate { Close(); }, 21f, 21.0f, true, KKStyle.DeadButtonRed)

                ));
        }



        internal static DialogGUIScrollList vesselList
        {
            get
            {
                List<DialogGUIBase> list = new List<DialogGUIBase>();
                list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
                list.Add(new DialogGUIFlexibleSpace());
                //list.Add(new DialogGUIButton("Default", delegate { SetVariant(null);}, 140.0f, 30.0f, true));

                StaticInstance[] allInstances = StaticDatabase.allStaticInstances;

                if (useFromFlight && selectedFacility != null)
                {
                    allInstances = new StaticInstance[] { selectedFacility };
                }


                foreach (var staticInstance in allInstances )
                {
                    if (!staticInstance.hasFacilities || staticInstance.facilityType != Modules.KKFacilityType.Hangar)
                    {
                        continue;
                    }

                    Modules.Hangar hangar = staticInstance.GetFacility(Modules.KKFacilityType.Hangar) as Modules.Hangar;
                    foreach (Modules.Hangar.StoredVessel storedVessel in hangar.storedVessels)
                    {
                        Modules.Hangar.StoredVessel vessel = storedVessel;
                        list.Add(new DialogGUIHorizontalLayout(
                            new DialogGUILabel(staticInstance.groupCenterName, KKStyle.whiteLabel) ,
                            new DialogGUIFlexibleSpace(),
                            new DialogGUILabel(vessel.vesselName),
                            new DialogGUIFlexibleSpace(),
                            new DialogGUIButton("Select", delegate { SelectVessel(vessel, hangar); }, delegate { if (selectedVessel == null) { return true; } return selectedVessel.vesselID != vessel.uuid; }  , 60.0f, 23.0f, false))
                            );
                    }
                }
                list.Add(new DialogGUIFlexibleSpace());
                var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleLeft, list.ToArray());
                var scroll = new DialogGUIScrollList(new Vector2(350, 200), new Vector2(330, 23f * list.Count-2), false, true, layout);

                // rreset that for next time
                useFromFlight = false;

                return scroll;

            }
        }


        internal static void CreateSeatList()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
            list.Add(new DialogGUIFlexibleSpace());
            //list.Add(new DialogGUIButton("Default", delegate { SetVariant(null);}, 140.0f, 30.0f, true));

            if (selectedVessel != null)
            {
                foreach (ProtoPartSnapshot protoPart0 in selectedVessel.protoPartSnapshots)
                {
                    ProtoPartSnapshot protoPart = protoPart0;
                    if (protoPart.partPrefab == null)
                    {
                        continue;
                    }

                    Part myPart = protoPart.partPrefab;
                    int seatCount = myPart.CrewCapacity;

                    if (seatCount > 0)
                    {
                        list.Add(new DialogGUIHorizontalLayout(
                            new DialogGUILabel(myPart.name, KKStyle.defaultLabel),
                            new DialogGUIFlexibleSpace()
                            ));
                        for (int i = 0; i < myPart.CrewCapacity; i++)
                        {
                            int seatNumber = i;                           
                            list.Add(new DialogGUIHorizontalLayout(
                                new DialogGUIFlexibleSpace(),
                                new DialogGUILabel(delegate { return GetCurrentCrewTrait(protoPart, seatNumber); }, KKStyle.whiteLabel),
                                new DialogGUIFlexibleSpace(),
                                new DialogGUILabel(delegate { return GetCurrentCrewExperience(protoPart, seatNumber); }, KKStyle.goldLabel),
                                new DialogGUIFlexibleSpace(),
                                new DialogGUIButton(delegate { return GetCurrentCrewName(protoPart, seatNumber); }, delegate { SelectSeat(protoPart, seatNumber); }, delegate { return ((protoPart != selectedPart) || selectedSeat != seatNumber); }, 140f, 25, false),
                                new DialogGUIButton(" X ", delegate { EmptySeat(protoPart, seatNumber); }, 25, 25, false, KKStyle.DeadButtonRed)
                            ));
                        }

                    }
                }
            }

            list.Add(new DialogGUIFlexibleSpace());
            var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleCenter, list.ToArray());
            var scroll = new DialogGUIScrollList(new Vector2(350, 200), new Vector2(330, 23f * list.Count - 2), false, true, layout);

            seatList = scroll;
        }

        internal static void CreateCrewList()
        {
            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
            list.Add(new DialogGUIFlexibleSpace());
            //list.Add(new DialogGUIButton("Default", delegate { SetVariant(null);}, 140.0f, 30.0f, true));

            //vesselToLaunch.protoVessel.LoadObjects();

            //foreach (ProtoCrewMember protoKerbal in HighLogic.CurrentGame.CrewRoster.Crew)
            //{
            //    Log.Normal(protoKerbal.name + " : " + protoKerbal.trait);

            //}

            foreach (ProtoCrewMember protoKerbal in HighLogic.CurrentGame.CrewRoster.Crew.Where(x => x.rosterStatus == ProtoCrewMember.RosterStatus.Available))
            {
                list.Add(new DialogGUIHorizontalLayout(
                    new DialogGUILabel(delegate { return protoKerbal.trait; }, KKStyle.whiteLabel),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUILabel(delegate { return GetCurrentCrewExperience(protoKerbal); }, KKStyle.goldLabel),
                    new DialogGUIFlexibleSpace(),
                    new DialogGUIButton(delegate { return protoKerbal.name; }, delegate { SeatKerbal(protoKerbal); }, delegate { return (selectedPart != null) && protoKerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available; }, 140f, 25, false))
                );
            }


            list.Add(new DialogGUIFlexibleSpace());
            var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleCenter, list.ToArray());
            var scroll = new DialogGUIScrollList(new Vector2(350, 200), new Vector2(330, 23f * list.Count - 2), false, true, layout);
            availableCrewList = scroll;
        }


        internal static void EmptySeat(ProtoPartSnapshot crewPart, int seatNumber)
        {
            foreach (ProtoCrewMember oldCrew in crewPart.protoModuleCrew.Where(x => x.seatIdx == seatNumber).ToArray())
            {
                crewPart.protoModuleCrew.Remove(oldCrew);
                oldCrew.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                oldCrew.seatIdx = -1;
                crewPart.protoCrewNames.Remove(oldCrew.name);
                selectedVessel.RemoveCrew(oldCrew);
            }
        }


        internal static void SeatKerbal(ProtoCrewMember crewMember)
        {
            foreach (ProtoCrewMember oldCrew in selectedPart.protoModuleCrew.Where(x => x.seatIdx == selectedSeat).ToArray())
            {
                selectedPart.protoModuleCrew.Remove(oldCrew);
                oldCrew.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                oldCrew.seatIdx = -1;
                selectedPart.protoCrewNames.Remove(oldCrew.name);
                selectedVessel.RemoveCrew(oldCrew);
            }

            crewMember.seatIdx = selectedSeat;
            selectedPart.protoModuleCrew.Add(crewMember);
            selectedPart.protoCrewNames.Add(crewMember.name);
            selectedVessel.AddCrew(crewMember);
            crewMember.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
        }




        internal static string GetCurrentCrewTrait(ProtoPartSnapshot crewPart, int seatNumber)
        {

            ProtoCrewMember crewMember = crewPart.protoModuleCrew.Where(member => member.seatIdx == seatNumber).FirstOrDefault();

            if (crewMember == null)
            {
                return "none";
            }
            else
            {
                return crewMember.trait;
            }

        }


        internal static string GetCurrentCrewExperience(ProtoCrewMember crewMember)
        {
            if (crewMember == null)
            {
                return "";
            }
            else
            {
                string experienceStars = "";
                for (int i = 1; i <= crewMember.experienceLevel; i++)
                {
                    experienceStars += "* ";
                }

                return experienceStars;
            }
        }

        internal static string GetCurrentCrewExperience(ProtoPartSnapshot crewPart, int seatNumber)
        {
            ProtoCrewMember crewMember = crewPart.protoModuleCrew.Where(member => member.seatIdx == seatNumber).FirstOrDefault();
            if (crewMember == null)
            {
                return "";
            }
            else
            {
                string experienceStars = "";
                for (int i = 1; i <= crewMember.experienceLevel; i++)
                {
                    experienceStars += "* ";
                }

                return experienceStars;
            }
        }


        internal static void SelectSeat(ProtoPartSnapshot crewPart, int seatNumber)
        {
            selectedSeat = seatNumber;
            selectedPart = crewPart;
        }


        internal static string GetCurrentCrewName(ProtoPartSnapshot crewPart, int seatNumber)
        {

            ProtoCrewMember crewMember = crewPart.protoModuleCrew.Where(member => member.seatIdx == seatNumber).FirstOrDefault();

            if (crewMember == null)
            {
                return "select seat";
            }
            else
            {
                return crewMember.name;
            }

        }


        /// <summary>
        /// Rollout the Vessel and store it in the placeHolder variable, then open the Crew assignment dialog
        /// </summary>
        /// <param name="storedVessel"></param>
        /// <param name="hangar"></param>
        internal static void SelectVessel(Modules.Hangar.StoredVessel storedVessel, Modules.Hangar hangar)
        {


            if (selectedVessel != null)
            {
                foreach (ProtoCrewMember crew in selectedVessel.GetVesselCrew())
                {
                    // unseat all crew
                    crew.rosterStatus = ProtoCrewMember.RosterStatus.Available;
                    crew.seatIdx = -1;
                }
            }

            selectedVessel = null;

            if (!hangar.storedVessels.Contains(storedVessel))
            {
                Log.Error("no stored vessel found:" + storedVessel.vesselName);
                return;
            }

            selectedHangar = hangar;
            selectedStoredVessel = storedVessel;


            ProtoVessel protoVessel = new ProtoVessel(storedVessel.vesselNode, HighLogic.CurrentGame);
            protoVessel.Load(HighLogic.CurrentGame.flightState);

            selectedVessel = protoVessel;


            if (selectedVessel == null)
            {
                Log.UserError("Could not receive vessel from storage.");
                return;
            }

            Reload();

           }





        internal static void LaunchVessel()
        {

            if (selectedVessel == null)
            {
                Log.UserError("This was not ment to be called like this");
                return;
            }

            ProtoVessel vesselToLaunch = selectedVessel;
            Log.Normal(selectedStoredVessel.vesselName);

            selectedHangar.storedVessels.Remove(selectedStoredVessel);
            selectedVessel = null;
            selectedHangar = null;
            selectedPart = null;

            // remove the control lock. which was created by the KSC Manager window
            InputLockManager.RemoveControlLock("KK_KSC");



            if (KSCManager.isOpen)
            {
                WindowManager.SavePosition(KSCManager.dialog);
            }

            GamePersistence.SaveGame("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);

            selectedFacility = null;

            if (!useFromFlight)
            {
                useFromFlight = false;
                FlightDriver.StartAndFocusVessel("persistent", FlightGlobals.Vessels.IndexOf(vesselToLaunch.vesselRef));
            } else
            {
                useFromFlight = false;
                vesselToLaunch.vesselRef.Load();
                vesselToLaunch.vesselRef.MakeActive();
            }

        }



        internal static void CreateMultiOptionDialog()
        {
            windowRect = new Rect(WindowManager.GetPosition(windowName), new Vector2(windowWidth, windowHeight));
            optionDialog = new MultiOptionDialog(windowName, windowMessage, windowTitle, null, windowRect, content.ToArray());
            optionDialog.OnFixedUpdate += PlaceToParent;
        }


        internal static void CreatePopUp()
        {
            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f),
                   new Vector2(0.5f, 0.5f), optionDialog,
                   false,
                   null, isModal);
            if (!showTitle)
            {
                dialog.gameObject.GetChild("Title").SetActive(false);
            }
            if (checkForParent)
            {
                dialog.dialogToDisplay.OnUpdate += CheckForParent;
            }
        }

        internal static void PlaceToParent()
        {
            Log.Normal(" " + dialog.dialogToDisplay.position.ToString());
        }

        internal static void CheckForParent()
        {
            if (checkForParent)
            {
                if (parentWindow != null && !parentWindow.Invoke())
                {
                    Close();
                }
            }
        }


        internal static void Reload()
        {
            if (dialog != null)
            {
                WindowManager.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;
            optionDialog = null;
            Open();
        }


        internal static void Open()
        {
            KKStyle.Init();
            //windowRect = new Rect(CreateBesidesMainwindow(), new Vector2(windowWidth, windowHeight));
            content = new List<DialogGUIBase>();
            KKTitle();

            CreateSeatList();
            CreateCrewList();

            CreateContent();
            CreateMultiOptionDialog();
            CreatePopUp();

            

        }


        internal static void Close()
        {
            if (dialog != null)
            {
                WindowManager.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;
            optionDialog = null;
            useFromFlight = false;
        }


        internal static bool isOpen
        {
            get
            {
                return (dialog != null);
            }
        }

        internal static bool IsOpen()
        {
            return (dialog != null);
        }

        internal static bool IsClosed()
        {
            return (dialog == null);
        }

        internal static void Toggle()
        {
            if (isOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }


        internal static void SetVariant(string variantName)
        {
            Log.Normal("Base Selected: " + variantName);
        }


    }
}
