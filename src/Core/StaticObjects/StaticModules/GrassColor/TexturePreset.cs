using KerbalKonstructs.UI;
using KerbalKonstructs.UI2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.Core
{

    enum TextureUsage
    {
        Unused,
        Texture,
        BlendMask
    }

    class TexturePreset
    {
        //internal string name = "unused";
        internal string texturePath = "";
        internal TextureUsage usage = TextureUsage.Unused;
    }


    class TextureSelector
    {


        internal static PopupDialog dialog;
        internal static MultiOptionDialog optionDialog;
        internal static List<DialogGUIBase> content;

        internal static string windowName = "TexturePresets";
        internal static string windowMessage = null;
        internal static string windowTitle = "Texture Presets";

        internal static Rect windowRect;

        //internal static float windowWidth = Screen.width * 0.9f;
        internal static float windowWidth = 350f;
        internal static float windowHeight = 300f;

        internal static bool showTitle = false;
        internal static bool showKKTitle = true;
        internal static bool isModal = false;


        internal static bool placeToParent = false;
        internal static bool checkForParent = true;

        internal static Func<bool> parentWindow = EditorGUI.instance.IsOpen;




        private static bool isInitialized = false;
        private static List<TexturePreset> textureList = new List<TexturePreset>();

        internal static List<TexturePreset> additionalTextures = new List<TexturePreset>();

        private static StaticInstance staticInstance = null;
        private static GrassColor2 selectedMod = null;

        //internal static Callback<TexturePreset> callBack = null;
        internal static string fieldName = "";
        internal static TextureUsage textureFilter = TextureUsage.Texture;

        internal static string lastTexture = "";


        private static void Initialize()
        {

            if (isInitialized)
            {
                return;
            }
            isInitialized = true;
            textureList.Clear();
            textureList = additionalTextures;
            foreach (ConfigNode colorNode in GameDatabase.Instance.GetConfigNodes("KK_TexturePreset"))
            {

                if (colorNode.HasValue("TextureFile") && colorNode.HasValue("Usage"))
                {
                    TexturePreset preset = new TexturePreset();
                    preset.texturePath = colorNode.GetValue("TextureFile");

                    if (!Enum.TryParse(colorNode.GetValue("Usage"), true, out preset.usage))
                    {
                        preset.usage = TextureUsage.Unused;
                    }
                    //Log.Normal("Adding Texture to List" + preset.texturePath + " : " + preset.usage.ToString());

                    if (textureList.Where(x => x.texturePath == preset.texturePath).Where(x => x.usage == preset.usage).Count() > 0  )
                    {
                        continue;
                    } 

                    textureList.Add(preset);

                }
            }
            textureList.Sort(delegate (TexturePreset first, TexturePreset second)
            {
                return first.texturePath.CompareTo(second.texturePath);
            });

        }


        private static void SetTexture(TexturePreset preset)
        {
            typeof(GrassColor2).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(selectedMod, preset.texturePath);
            selectedMod.ApplySettings();
        }



        internal static void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(
                new DialogGUILabel("select a Texture", HighLogic.UISkin.label),
                new DialogGUIFlexibleSpace()
                ));


            content.Add(VaiantList);
            content.Add(new DialogGUIVerticalLayout(
                new DialogGUILabel("NearGrassTexture", HighLogic.UISkin.label)


                )); ;
        }




        internal static DialogGUIScrollList VaiantList
        {
            get
            {
                List<DialogGUIBase> list = new List<DialogGUIBase>();
                list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
                list.Add(new DialogGUIFlexibleSpace());

                foreach (var textureSet in textureList.Where(set => set.usage == textureFilter))
                {
                    list.Add(new DialogGUIButton(textureSet.texturePath, delegate { SetTexture(textureSet); }, delegate { return (textureSet.texturePath != lastTexture); }, 250, 20.0f, true, KKStyle.whiteLabel));
                }
                list.Add(new DialogGUIButton("Cancel", null, 140.0f, 25f, true));
                list.Add(new DialogGUIFlexibleSpace());
                var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(6, 24, 10, 10), TextAnchor.MiddleCenter, list.ToArray());
                var scroll = new DialogGUIScrollList(new Vector2(300, 250), new Vector2(200, 25f * list.Count), false, true, layout);
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
            windowRect = new Rect(UI2.WindowManager.GetPosition(windowName), new Vector2(windowWidth, windowHeight));
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
            selectedMod = GrassEditor.selectedMod;
            Initialize();
            lastTexture = typeof(GrassColor2).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(selectedMod) as string;

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
                UI2.WindowManager.SavePosition(dialog);
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








    }

}
