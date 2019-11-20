using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;



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

        internal static string NearGrassTexture;
        internal static string FarGrassTexture;
        internal static string TamarcTexture;
        internal static string BlendMaskTexture;

        internal static string GrassColor = "1,1,1,1";
        internal static string TarmacColor = "1,1,1,1";
        internal static string ThirdColor = "1,1,1,0";
        internal static string FourthcColor = "1,1,1,0";

        internal static string NearGrassTiling;
        internal static string FarGrassTiling;
        internal static string BlendDistance;

        internal static string ThirdTexture;
        internal static string ThirdTextureTiling;

        internal static string FourthTexture;
        internal static string FourthTextureTiling;

        internal static StaticInstance staticInstance = null;
        internal static GrassColor2 selectedMod = null;
        internal static int offset = 0;



        internal static void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("Grass Color Editor", HighLogic.UISkin.label),
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("select preset", delegate { GrassColorPresetUI2.callBack = selectedMod.UpdateCallBack; GrassColorPresetUI2.instance.Open(); }, 50, 25, false),
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("Pick Surface Color", delegate {
                    selectedMod.grassColor = GrassColorUtils.GetUnderGroundColor(selectedMod);
                    selectedMod.ApplySettings();
                }, 80, 25, false)
                ));


            content.Add(VaiantList);

            content.Add(new DialogGUIVerticalLayout(
                new DialogGUIHorizontalLayout(
                    new DialogGUILabel("NearGrassTexture", KKStyle.whiteLabel),
                    new DialogGUITextInput(NearGrassTexture, false, 40, SetNearGrassTexture, delegate { return GetTextureName("nearGrassTextureName"); }, TMPro.TMP_InputField.ContentType.Standard, 25),
                    new DialogGUIButton("  S", delegate { OpenTextureSelector("nearGrassTextureName"); }, 21f, 21.0f, false, HighLogic.UISkin.label)),
                new DialogGUIHorizontalLayout(
                    new DialogGUILabel("FarGrassTexture", KKStyle.whiteLabel),
                    new DialogGUITextInput(FarGrassTexture, false, 40, SetFarGrassTexture, delegate { return GetTextureName("farGrassTextureName"); }, TMPro.TMP_InputField.ContentType.Standard, 25),
                    new DialogGUIButton("  S", delegate { OpenTextureSelector("farGrassTextureName"); }, 21f, 21.0f, false, HighLogic.UISkin.label)),
                new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Grass Color : ", KKStyle.whiteLabel),
                    new DialogGUITextInput("", false, 40, delegate (string colorString) { selectedMod.grassColor = ConfigNode.ParseColor(colorString); return colorString; }, delegate { return ConfigNode.WriteColor(selectedMod.grassColor); }, TMPro.TMP_InputField.ContentType.Standard, 25),
                    new DialogGUIButton("Edit", delegate { ColorSelector.callBack = SetGrassColor; ; ColorSelector.selectedColor = selectedMod.grassColor; ColorSelector.Open(); }, 40f, 21.0f, false, KKStyle.whiteLabel)),
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel("NearGrassTiling", KKStyle.whiteLabel),
                    new DialogGUITextInput(NearGrassTiling, NearGrassTiling, false, 10, SetNearTile, 25)),
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel("FarGrassTiling", KKStyle.whiteLabel),
                    new DialogGUITextInput(FarGrassTiling, FarGrassTiling, false, 10, SetFarTile, 25)),
                new DialogGUIHorizontalLayout(
                    new DialogGUILabel("FarBlendDistance", KKStyle.whiteLabel),
                    new DialogGUITextInput(BlendDistance, BlendDistance, false, 10, SetBlendDistance, 25)),
                new DialogGUIHorizontalLayout(
                    new DialogGUILabel("TamarcTexture", KKStyle.whiteLabel),
                    new DialogGUITextInput(TamarcTexture, false, 40, SetTarmacTexture, delegate { return GetTextureName("tarmacTextureName"); }, TMPro.TMP_InputField.ContentType.Standard, 25),
                    new DialogGUIButton("  S", delegate { OpenTextureSelector("tarmacTextureName"); }, 21f, 21.0f, false, HighLogic.UISkin.label)),
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Tarmac Color : ", KKStyle.whiteLabel),
                    new DialogGUITextInput("", false, 40, delegate (string colorString) { selectedMod.tarmacColor = ConfigNode.ParseColor(colorString); return colorString; }, delegate { return ConfigNode.WriteColor(selectedMod.tarmacColor); }, TMPro.TMP_InputField.ContentType.Standard, 25),
                    new DialogGUIButton("Edit", delegate { ColorSelector.callBack = SetTarmacColor; ; ColorSelector.selectedColor = selectedMod.tarmacColor; ColorSelector.Open(); }, 40f, 21.0f, false, KKStyle.whiteLabel)),
               new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Tarmac Tiling: U: ", KKStyle.whiteLabel),
                    new DialogGUITextInput("0", false, 10, SetTarmacUStr, GetTarmacUStr, TMPro.TMP_InputField.ContentType.DecimalNumber, 25),
                    new DialogGUILabel("V: ", KKStyle.whiteLabel),
                    new DialogGUITextInput("0", false, 10, SetTarmacVStr, GetTarmacVStr, TMPro.TMP_InputField.ContentType.DecimalNumber, 25)),
               new DialogGUIHorizontalLayout(
                   new DialogGUIToggle(selectedMod.tarmacTileRandom,"Tarmac Texture Random Tiling", delegate(bool state) { selectedMod.tarmacTileRandom = state; selectedMod.ApplySettings(); }, 120,21)),
                new DialogGUIHorizontalLayout(
                    new DialogGUIButton("  R", ReloadBlendMask, 21f, 21.0f, false, HighLogic.UISkin.label),
                    new DialogGUILabel("BlendMask", KKStyle.whiteLabel),
                    new DialogGUITextInput(BlendMaskTexture, false, 40, SetBlendMaskTexture, delegate { return GetTextureName("blendMaskTextureName"); }, TMPro.TMP_InputField.ContentType.Standard, 25),
                    new DialogGUIButton("  S", delegate { OpenTextureSelector("blendMaskTextureName", TextureUsage.BlendMask); }, 21f, 21.0f, false, HighLogic.UISkin.label)),
                  new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Third Texture", KKStyle.whiteLabel),
                    new DialogGUITextInput(TamarcTexture, false, 40, SetTarmacTexture, delegate { return GetTextureName("thirdTextureName"); }, TMPro.TMP_InputField.ContentType.Standard, 25),
                    new DialogGUIButton("  S", delegate { OpenTextureSelector("thirdTextureName"); }, 21f, 21.0f, false, HighLogic.UISkin.label)),
                  new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Third Texture Tiling: ", KKStyle.whiteLabel),
                    new DialogGUITextInput(ThirdTextureTiling, ThirdTextureTiling, false, 10, SetThirdTile, 25),
                    new DialogGUIToggle(selectedMod.thirdTextureTileRandom, "Random Tiling", delegate (bool state) { selectedMod.thirdTextureTileRandom = state; selectedMod.ApplySettings(); }, 120, 21)),
                new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Third Color : ", KKStyle.whiteLabel),
                    new DialogGUITextInput("", false, 40, delegate(string colorString) { selectedMod.thirdTextureColor = ConfigNode.ParseColor(colorString); return colorString; }, delegate { return ConfigNode.WriteColor(selectedMod.thirdTextureColor); }, TMPro.TMP_InputField.ContentType.Standard, 25),
                    new DialogGUIButton("Edit", delegate { ColorSelector.callBack = SetThirdColor; ; ColorSelector.selectedColor = selectedMod.thirdTextureColor; ColorSelector.Open(); }, 40f, 21.0f, false, KKStyle.whiteLabel)),
                new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Fourth Texture", KKStyle.whiteLabel),
                    new DialogGUITextInput(TamarcTexture, false, 40, SetTarmacTexture, delegate { return GetTextureName("thirdTextureName"); }, TMPro.TMP_InputField.ContentType.Standard, 25),
                    new DialogGUIButton("  S", delegate { OpenTextureSelector("thirdTextureName"); }, 21f, 21.0f, false, HighLogic.UISkin.label)),
                  new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Fourth Texture Tiling: ", KKStyle.whiteLabel),
                    new DialogGUITextInput(FourthTextureTiling, FourthTextureTiling, false, 10, SetFourthTile, 25),
                    new DialogGUIToggle(selectedMod.fourthTextureTileRandom, "Random Tiling", delegate (bool state) { selectedMod.fourthTextureTileRandom = state; selectedMod.ApplySettings(); }, 120, 21)),
                new DialogGUIHorizontalLayout(
                    new DialogGUILabel("Fourth Color : ", KKStyle.whiteLabel),
                    new DialogGUITextInput("", false, 40, delegate (string colorString) { selectedMod.fourthTextureColor = ConfigNode.ParseColor(colorString); return colorString; }, delegate { return ConfigNode.WriteColor(selectedMod.fourthTextureColor); }, TMPro.TMP_InputField.ContentType.Standard, 25),
                    new DialogGUIButton("Edit", delegate { ColorSelector.callBack = SetFourthColor; ; ColorSelector.selectedColor = selectedMod.fourthTextureColor; ColorSelector.Open(); }, 40f, 21.0f, false, KKStyle.whiteLabel))

                //new DialogGUIHorizontalLayout(
                //    new DialogGUILabel("Overall Tiling", KKStyle.whiteLabel),
                //    new DialogGUISlider(GetOffsetTiling, 0, 200f, true, 140, 25, SetOffsetTiling),
                //    new DialogGUITextInput("0", false, 10, SetOffsetTilingStr, GetOffsetTilingStr, TMPro.TMP_InputField.ContentType.DecimalNumber, 25))

                )); ;
        }



        internal static void SetGrassColor (Color color)
        {
            selectedMod.grassColor = color;
            selectedMod.ApplySettings();
        }

        internal static void SetTarmacColor(Color color)
        {
            selectedMod.tarmacColor = color;
            selectedMod.ApplySettings();
        }

        internal static void SetThirdColor(Color color)
        {
            selectedMod.thirdTextureColor = color;
            selectedMod.ApplySettings();
        }

        internal static void SetFourthColor(Color color)
        {
            selectedMod.fourthTextureColor = color;
            selectedMod.ApplySettings();
        }



        internal static string SetThirdTile(string newTile)
        {
            selectedMod.thirdTextureTiling = float.Parse(newTile);
            selectedMod.ApplySettings();
            return newTile;
        }

        internal static string SetFourthTile(string newTile)
        {
            selectedMod.fourthTextureTiling = float.Parse(newTile);
            selectedMod.ApplySettings();
            return newTile;
        }


        internal static void ReloadBlendMask()
        {
            KKGraphics.RemoveCache(selectedMod.blendMaskTextureName);
            selectedMod.ApplySettings();
        }


        internal static string SetTarmacUStr(string newUvalue)
        {
            float uValue = float.Parse(newUvalue);
            selectedMod.tarmacTiling = new Vector2(uValue, selectedMod.tarmacTiling.y);
            selectedMod.ApplySettings();
            return selectedMod.tarmacTiling.x.ToString();
        }

        internal static string SetTarmacVStr(string newVvalue)
        {
            float vValue = float.Parse(newVvalue);
            selectedMod.tarmacTiling = new Vector2(selectedMod.tarmacTiling.x, vValue);
            selectedMod.ApplySettings();
            return selectedMod.tarmacTiling.y.ToString();
        }

        internal static string GetTarmacUStr()
        {
            return selectedMod.tarmacTiling.x.ToString();
        }

        internal static string GetTarmacVStr()
        {
            return selectedMod.tarmacTiling.y.ToString();
        }



        internal static string GetTextureName(string fieldName)
        {
            return typeof(GrassColor2).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(selectedMod) as string;
        }


        internal static void OpenTextureSelector(string fieldName, TextureUsage filter = TextureUsage.Texture)
        {
            TextureSelector.fieldName = fieldName;
            TextureSelector.textureFilter = filter;
            TextureSelector.Open();
        }


        internal static string SetNearGrassTexture(string newTexture)
        {
            selectedMod.nearGrassTextureName = newTexture;
            selectedMod.ApplySettings();
            return newTexture;
        }

        internal static string SetFarGrassTexture(string newTexture)
        {
            selectedMod.farGrassTextureName = newTexture;
            selectedMod.ApplySettings();
            return newTexture;
        }

        internal static string SetTarmacTexture(string newTexture)
        {
            selectedMod.tarmacTextureName = newTexture;
            selectedMod.ApplySettings();
            return newTexture;
        }

        internal static string SetBlendMaskTexture(string newTexture)
        {
            selectedMod.blendMaskTextureName = newTexture;
            selectedMod.ApplySettings();
            return newTexture;
        }


        internal static string SetNearTile(string newTile)
        {
            selectedMod.nearGrassTiling = float.Parse(newTile);
            selectedMod.ApplySettings();
            return newTile;
        }

        internal static string SetFarTile(string newTile)
        {
            selectedMod.farGrassTiling = float.Parse(newTile);
            selectedMod.ApplySettings();
            return newTile;
        }
        internal static string SetBlendDistance(string newTile)
        {
            selectedMod.farGrassBlendDistance = float.Parse(newTile);
            selectedMod.ApplySettings();
            return newTile;
        }


        


        internal static DialogGUIScrollList VaiantList
        {
            get
            {
                List<DialogGUIBase> list = new List<DialogGUIBase>();
                list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
                list.Add(new DialogGUIFlexibleSpace());
                //list.Add(new DialogGUIButton("Default", delegate { SetVariant(null);}, 140.0f, 30.0f, true));

                foreach (var grassMod in EditorGUI.selectedInstance.mesh.GetComponents<GrassColor2>())
                {

                        list.Add(new DialogGUIButton(grassMod.GrassMeshName, delegate { SelectGrass(grassMod);  }, delegate { return (selectedMod != grassMod) ; }, 210.0f, 21.0f, false));

                }
                list.Add(new DialogGUIFlexibleSpace());
                var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleLeft, list.ToArray());
                var scroll = new DialogGUIScrollList(new Vector2(210, 60), new Vector2(200, 25f * (list.Count-3)), false, true, layout);
                return scroll;
            }
        }


        internal static void KKTitle()
        {
            if (!showKKTitle)
            {
                return;
            }


            var button = new DialogGUIButton("X", delegate { Close(); }, 21f, 21.0f, true, KKStyle.DeadButtonRed)
            {
                image = null,
                tint = new Color(0, 0, 0, 0),
                useColor = false
            };

            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("-KK-", KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),

                new DialogGUILabel(windowTitle, KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                //new DialogGUIButton("X", delegate { Close(); }, 21f, 21.0f, true, KKStyle.DeadButtonRed)
                button
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
                if (staticInstance != EditorGUI.selectedInstance)
                {
                    Close();
                }
            }
        }




        internal static void Open()
        {
            KKStyle.Init();
            staticInstance = EditorGUI.selectedInstance;
            staticInstance.HighlightObject(Color.clear);
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
            if (EditorGUI.instance.IsOpen() && (EditorGUI.selectedInstance == staticInstance))
            {
                staticInstance.HighlightObject(XKCDColors.Green_Yellow);
            }

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
            BlendDistance = mod.farGrassBlendDistance.ToString();

            ThirdTexture = mod.thirdTexture != null ? mod.thirdTextureName : "no texture";
            ThirdTextureTiling = mod.thirdTextureTiling.ToString();

            FourthTexture = mod.fourthTexture != null ? mod.fourthTextureName : "no texture";
            FourthTextureTiling = mod.fourthTextureTiling.ToString();

        }


        internal static void SelectGrass(GrassColor2 grass)
        {
            selectedMod = grass;
            //selectedMod.matOffset = offset;

            ReadMod(selectedMod);
        }

    }
}
