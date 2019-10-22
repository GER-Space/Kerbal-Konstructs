using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using KerbalKonstructs.Core;
using KerbalKonstructs.UI;


namespace KerbalKonstructs.UI2
{
    class GrassEditor
    {
        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "GrassEditor";
        internal static string windowMessage = null;
        internal static string windowTitle = "Grass Editor";

        internal static Rect windowRect;

        //internal static float windowWidth = Screen.width * 0.9f;
        internal static float windowWidth = 300f;
        internal static float windowHeight = 300f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = true;


        internal static bool placeToParent = false;
        internal static bool checkForParent = true;

        internal static Func<bool> parentWindow = EditorGUI.instance.IsOpen;

        internal static GrassColor2 selectedMod = null;

        internal static string NearGrassTexture;
        internal static string FarGrassTexture;
        internal static string TamarcTexture;
        internal static string BlendMaskTexture;

        internal static string NearGrassTiling;
        internal static string FarGrassTiling;

        internal static StaticInstance staticInstance = null;


        internal static void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("All Models", HighLogic.UISkin.label),
                new DialogGUIFlexibleSpace()
                ));


            content.Add(VaiantList);
            content.Add(new DialogGUIVerticalLayout(
                new DialogGUILabel("NearGrassTexture", HighLogic.UISkin.label),
                new DialogGUITextInput(NearGrassTexture, NearGrassTexture, false, 40, SetString, 30),
                new DialogGUILabel("FarGrassTexture", HighLogic.UISkin.label),
                new DialogGUITextInput(FarGrassTexture, FarGrassTexture, false, 40, SetString, 30),
                new DialogGUILabel("TamarcTexture", HighLogic.UISkin.label),
                new DialogGUITextInput(TamarcTexture, TamarcTexture, false, 40, SetString, 30),
                new DialogGUILabel("BlendMaskTexture", HighLogic.UISkin.label),
                new DialogGUITextInput(BlendMaskTexture, BlendMaskTexture, false, 40, SetString, 30),
                new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Color: R ", HighLogic.UISkin.label),
                    new DialogGUISlider(GetFloatR, 0, 1, false, 140, 25, SetFloatR),
                    new DialogGUILabel(GetFloatRStr)) ,
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Color: G ", HighLogic.UISkin.label),
                    new DialogGUISlider(GetFloatG, 0, 1, false, 140, 25, SetFloatG),
                    new DialogGUILabel(GetFloatGStr)),
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Color: B ", HighLogic.UISkin.label),
                    new DialogGUISlider(GetFloatB, 0, 1, false, 140, 25, SetFloatB),
                    new DialogGUILabel(GetFloatBStr)),
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel("NearGrassTiling", HighLogic.UISkin.label),
                    new DialogGUITextInput(NearGrassTiling, NearGrassTiling, false, 40, SetNearTile, 30)),
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel("FarGrassTiling", HighLogic.UISkin.label),
                    new DialogGUITextInput(FarGrassTiling, FarGrassTiling, false, 40, SetFarTile, 30))



                )); ;
        }



        internal static string SetString(string teststring)
        {
            Log.Normal("SetString: " + teststring);
            return teststring;
        }

        internal static string SetNearTile(string newTile)
        {
            selectedMod.nearGrassTiling = float.Parse(newTile);
            selectedMod.SetColor();
            return newTile;
        }

        internal static string SetFarTile(string newTile)
        {
            selectedMod.farGrassTiling = float.Parse(newTile);
            selectedMod.SetColor();
            return newTile;

        }



        internal static float GetFloatR()
        {
            return selectedMod.grassColor.r;
        }
        internal static string GetFloatRStr()
        {
            return selectedMod.grassColor.r.ToString();
        }
        internal static void SetFloatR(float newFloat)
        {
            Log.Normal("SetFloat: " + newFloat);
            selectedMod.grassColor.r = newFloat;
            selectedMod.SetColor();
        }

        internal static float GetFloatG()
        {
            return selectedMod.grassColor.g;
        }
        internal static string GetFloatGStr()
        {
            return selectedMod.grassColor.g.ToString();
        }

        internal static void SetFloatG(float newFloat)
        {
            Log.Normal("SetFloat: " + newFloat);
            selectedMod.grassColor.g = newFloat;
            selectedMod.SetColor();
        }

        internal static float GetFloatB()
        {
            return selectedMod.grassColor.b;
        }
        internal static string GetFloatBStr()
        {
            return selectedMod.grassColor.b.ToString();
        }
        internal static void SetFloatB(float newFloat)
        {
            Log.Normal("SetFloat: " + newFloat);
            selectedMod.grassColor.b = newFloat;
            selectedMod.SetColor();
        }



        internal static DialogGUIScrollList VaiantList
        {
            get
            {
                List<DialogGUIBase> list = new List<DialogGUIBase>();
                //list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
                //list.Add(new DialogGUIFlexibleSpace());
                //list.Add(new DialogGUIButton("Default", delegate { SetVariant(null);}, 140.0f, 30.0f, true));

                foreach (var grass in EditorGUI.selectedInstance.mesh.GetComponents<GrassColor2>())
                {
                    list.Add(new DialogGUIButton(grass.GrassMeshName, delegate { SelectGrass(grass); }, delegate { return (grass != selectedMod); }, 140.0f, 25.0f, false));
                }
                list.Add(new DialogGUIFlexibleSpace());
                var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleCenter, list.ToArray());
                var scroll = new DialogGUIScrollList(new Vector2(200, 50), new Vector2(200, 25f * list.Count), false, false, layout);
                return scroll;
            }
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
            windowRect = new Rect(WindowManager.GetPosition(windowName), new Vector2(windowWidth, windowHeight));
            optionDialog = new MultiOptionDialog(windowName, windowMessage, windowTitle, null, windowRect, content.ToArray());

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
            if (placeToParent)
            {
                dialog.dialogToDisplay.OnUpdate += PlaceToParent;
            }

        }

        internal static void PlaceToParent()
        {

        }


        internal static void CheckForParent()
        {
            if (checkForParent)
            {
                if (parentWindow != null && !parentWindow.Invoke())
                {
                    Close();
                }
                if (staticInstance != EditorGUI.selectedInstance )
                {
                    Close();
                }
            }
        }




        internal static void Open()
        {
            KKStyle.Init();
            staticInstance = EditorGUI.selectedInstance;
            selectedMod = EditorGUI.selectedInstance.mesh.GetComponents<GrassColor2>()[0];
            ReadMod(selectedMod);
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

        internal static void ReadMod(GrassColor2 mod)
        {
            if (mod == null)
            {
                Log.UserError("No Grass Mod Found");
                return; 
            }
            NearGrassTexture = mod.nearGrassTexture != null ? mod.nearGrassTextureName : "no texture";
            FarGrassTexture = mod.farGrassTexture != null ? mod.farGrassTextureName : "no texture";
            TamarcTexture = mod.tarmacTexture != null ? mod.tarmacTextureName : "no texture";
            BlendMaskTexture = mod.blendMaskTexture != null ? mod.blendMaskTextureName : "no texture";
            NearGrassTiling = mod.nearGrassTiling.ToString();
            FarGrassTiling = mod.farGrassTiling.ToString();
        }


        internal static void SelectGrass(GrassColor2 grass)
        {
            selectedMod = grass;
            ReadMod(selectedMod);
        }

    }
}
