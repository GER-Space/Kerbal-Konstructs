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
    internal class CrewAssignment
    {

        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "CrewAssignment";
        internal static string windowMessage = null;
        internal static string windowTitle = "All stored Vessels";

        internal static Rect windowRect;

        internal static float windowWidth = 350f;
        internal static float windowHeight = 200f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;


        internal static bool setToParent = false;

        internal static bool checkForParent = false;
        internal static Func<bool> parentWindow = HangarKSCGUI.IsOpen;


        internal static Vessel vesselToLaunch;

        private static ProtoPartSnapshot selectedPart;
        private static int selectedSeat = 0;


        internal static void CreateContent()
        {
            content.Add(new DialogGUILabel("Selected Vessel: " + vesselToLaunch.GetDisplayName(), KKStyle.whiteLabel));
            content.Add(variantList);

            content.Add(new DialogGUIButton("Continue with launch", delegate { Close(); HangarKSCGUI.LaunchVessel(); }, false));
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



        internal static DialogGUIScrollList variantList
        {
            get
            {
                List<DialogGUIBase> list = new List<DialogGUIBase>();
                list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
                list.Add(new DialogGUIFlexibleSpace());
                //list.Add(new DialogGUIButton("Default", delegate { SetVariant(null);}, 140.0f, 30.0f, true));

                //vesselToLaunch.protoVessel.LoadObjects();

                foreach (ProtoPartSnapshot protoPart0 in vesselToLaunch.protoVessel.protoPartSnapshots)
                {
                    ProtoPartSnapshot protoPart = protoPart0;
                    if (protoPart.partPrefab == null)
                    {
                        continue;
                    }
                    
                    Part myPart = protoPart.partPrefab;
                    int seatCount = myPart.CrewCapacity;
                    Log.Normal("Part: " + myPart.name + " seats: " + seatCount);


                    if (seatCount > 0)
                    {
                        list.Add(new DialogGUILabel(myPart.name, KKStyle.whiteLabel));
                        for (int i = 0; i < myPart.CrewCapacity; i++)
                        {
                            int seatNumber = i;
                            list.Add(new DialogGUIHorizontalLayout(
                                new DialogGUILabel(delegate { return GetCurrentCrewTrait(protoPart, seatNumber); }, KKStyle.whiteLabel),
                                new DialogGUIFlexibleSpace(),
                                new DialogGUILabel(delegate { return GetCurrentCrewExperience(protoPart, seatNumber); }, KKStyle.whiteLabel),
                                new DialogGUIFlexibleSpace(),
                                new DialogGUIButton(delegate { return GetCurrentCrewName(protoPart, seatNumber); }, delegate { OpenCrewSelector(protoPart, seatNumber); }, 140f, 25, false))
                            ); 
                        }

                    }
                }
                

                list.Add(new DialogGUIFlexibleSpace());
                var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleCenter, list.ToArray());
                var scroll = new DialogGUIScrollList(new Vector2(350, 200), new Vector2(330, 23f * list.Count - 2), false, true, layout);
                return scroll;

            }
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



        internal static string GetCurrentCrewName (ProtoPartSnapshot crewPart, int seatNumber)
        {

            ProtoCrewMember crewMember = crewPart.protoModuleCrew.Where(member => member.seatIdx == seatNumber).FirstOrDefault();
            
            if (crewMember == null)
            {
                return "none";
            } else
            {
                return crewMember.name;
            }

        }


        internal static void OpenCrewSelector(ProtoPartSnapshot crewPart, int seatNumber)
        {
            selectedPart = crewPart;
            selectedSeat = seatNumber;
            AvailableCrewWindow.Open();

        }


        internal static void SeatKerbal (ProtoCrewMember crewMember)
        {
            crewMember.seatIdx = selectedSeat;
            //crewMember.seatIdx = -1;
            selectedPart.protoModuleCrew.Add(crewMember);
            selectedPart.protoCrewNames.Add(crewMember.name);
            vesselToLaunch.protoVessel.AddCrew(crewMember);
            crewMember.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
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



        internal static void Open()
        {
            KKStyle.Init();
            //windowRect = new Rect(CreateBesidesMainwindow(), new Vector2(windowWidth, windowHeight));
            content = new List<DialogGUIBase>();
            KKTitle();
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
            vesselToLaunch = null;
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
