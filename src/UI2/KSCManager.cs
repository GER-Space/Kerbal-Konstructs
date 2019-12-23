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
    internal class KSCManager
    {

        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "KSCManager";
        internal static string windowMessage = null;
        internal static string windowTitle = "Kerbal Konstructs Base Manager";

        internal static Rect windowRect;

        internal static float windowWidth = 250;
        internal static float windowHeight = 200f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;


        internal static bool setToParent = false;

        internal static bool checkForParent = false;
        internal static Func<bool> parentWindow = null;


        internal static void CreateContent()
        {
            content.Add(new DialogGUILabel("Action from the KSC", HighLogic.UISkin.label));
            content.Add(new DialogGUIButton("Show Stored Vessels", delegate { HangarKSCGUI.Open(); } , HangarKSCGUI.IsClosed, false));
            content.Add(new DialogGUIButton("Repair All Buildings", RepairAllBuildings, 140.0f, 30.0f, true));
            content.Add(new DialogGUIButton("Close", delegate { Close(); }, 140.0f, 30.0f, false));
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



        internal static void CreateMultiOptionDialog()
        {
            windowRect = new Rect(WindowManager2.GetPosition(windowName), new Vector2(windowWidth, windowHeight));
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
            InputLockManager.SetControlLock("KK_KSC");

            //GameObject prefab = null;

            //foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            //{
            //    if (go.name == "CrewManagementArea")
            //    {
            //        prefab = GameObject.Instantiate(go, new Vector3(3,41,0), Quaternion.identity) as GameObject;
            //    }
            //}

            //foreach (MonoBehaviour comp in prefab.GetComponentsInChildren<MonoBehaviour>(true))
            //{
            //    Log.Normal(comp.GetType().FullName.ToString());
            //}

            //prefab.transform.SetParent(KSP.UI.UIMasterController.Instance.appCanvas.transform);
            //prefab.SetActive(true);

        }


        internal static void Close()
        {
            if (dialog != null)
            {
                WindowManager2.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;
            optionDialog = null;
            InputLockManager.RemoveControlLock("KK_KSC");
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

        internal static void RepairAllBuildings()
        {
            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                foreach (DestructibleBuilding building in instance.transform.gameObject.GetComponentsInChildren<DestructibleBuilding>(true))
                {
                    if (building.IsDestroyed)
                    {
                        //building.RepairCost = 0;
                        //building.Repair();
                        building.Reset();
                        // reactivate the Building Spawn if the building was active before
                        if (instance.isActive)
                        {
                            instance.Activate();
                        }
                        instance.isDestroyed = false;
                    }
                }
            }
            InputLockManager.RemoveControlLock("KK_KSC");
        }


    }
}
