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
    internal class AvailableCrewWindow
    {

        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "AvailableCrewWindow";
        internal static string windowMessage = null;
        internal static string windowTitle = "Available Crew for assignment";

        internal static Rect windowRect;

        internal static float windowWidth = 300f;
        internal static float windowHeight = 200f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;


        internal static bool setToParent = false;

        internal static bool checkForParent = false;
        internal static Func<bool> parentWindow  ;




        internal static void CreateContent()
        {
            content.Add(new DialogGUILabel("Available Crew: "));
            content.Add(variantList);

            content.Add(new DialogGUIButton("Cancel", delegate { Close(); }, false));
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

                //foreach (ProtoCrewMember protoKerbal in HighLogic.CurrentGame.CrewRoster.Crew)
                //{
                //    Log.Normal(protoKerbal.name + " : " + protoKerbal.trait);

                //}

                foreach (ProtoCrewMember protoKerbal in HighLogic.CurrentGame.CrewRoster.Crew.Where(x => x.rosterStatus == ProtoCrewMember.RosterStatus.Available))
                {
                    list.Add(new DialogGUIHorizontalLayout(
                        new DialogGUILabel(delegate { return GetCurrentCrewTrait(protoKerbal); }, KKStyle.whiteLabel),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUILabel(delegate { return GetCurrentCrewExperience(protoKerbal); }, KKStyle.whiteLabel),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIButton(delegate { return GetCurrentCrewName(protoKerbal); }, delegate { CrewAssignment.SeatKerbal(protoKerbal); }, 140f, 25, true))
                    );
                }


                list.Add(new DialogGUIFlexibleSpace());
                var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleCenter, list.ToArray());
                var scroll = new DialogGUIScrollList(new Vector2(350, 200), new Vector2(330, 23f * list.Count - 2), false, true, layout);
                return scroll;

            }
        }



        internal static string GetCurrentCrewTrait(ProtoCrewMember crewMember)
        {

           

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



        internal static string GetCurrentCrewName(ProtoCrewMember crewMember)
        {
            
            if (crewMember == null)
            {
                return "none";
            }
            else
            {
                return crewMember.name;
            }

        }


        internal static void SelectCrew(ProtoPartSnapshot crewPart, int seatNumber)
        {



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



    }
}
