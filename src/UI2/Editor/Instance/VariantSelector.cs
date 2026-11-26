using KerbalKonstructs.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Lists the variants of the static's model; clicking one switches the static to it.
    /// (moved from Editor/InstanceEditor and given the KK editor look)
    /// </summary>
    internal class VariantSelector : KKEditorWindow
    {
        private static VariantSelector _instance = null;
        private static VariantSelector instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VariantSelector();
                }
                return _instance;
            }
        }

        internal static StaticInstance staticInstance;

        protected override string WindowName => "VariantSelector";
        protected override string WindowTitle => "Model Variant";
        protected override float WindowWidth => 260f;
        protected override Vector2 DefaultPosition => new Vector2(0.6f, 0.5f);


        internal static void Open()
        {
            if (staticInstance == null || staticInstance.model == null || !staticInstance.model.hasVariants)
            {
                return;
            }
            instance.DismissDialog();
            instance.Build();
        }


        internal static void CloseWindow()
        {
            if (_instance != null)
            {
                _instance.DismissDialog();
            }
            staticInstance = null;
        }


        public override void Close()
        {
            DismissDialog();
            staticInstance = null;
        }


        protected override void OnUpdate()
        {
            // close with the Instance editor, or when another static is selected there
            if (staticInstance == null || !InstanceEditor.instance.IsOpen() || InstanceEditor.selectedInstance != staticInstance)
            {
                Close();
            }
        }


        protected override void CreateContent()
        {
            content.Add(new DialogGUIHorizontalLayout(new DialogGUIFlexibleSpace(), GoldLabel(delegate { return (staticInstance == null) ? "" : staticInstance.model.title; }), new DialogGUIFlexibleSpace()));

            List<DialogGUIBase> list = new List<DialogGUIBase>();
            list.Add(new DialogGUIContentSizer(ContentSizeFitter.FitMode.Unconstrained, ContentSizeFitter.FitMode.PreferredSize, true));
            foreach (var variant in staticInstance.model.variants)
            {
                string variantName = variant.Value.name;
                string label = variant.Key;
                list.Add(new DialogGUIButton(label, delegate { SetVariant(variantName); }, delegate { return staticInstance != null && staticInstance.VariantName != variantName; }, 200f, itemHeight, false));
            }
            var layout = new DialogGUIVerticalLayout(10, 100, 4, new RectOffset(4, 20, 4, 4), TextAnchor.UpperLeft, list.ToArray());
            content.Add(new DialogGUIScrollList(new Vector2(WindowWidth - 20f, Mathf.Min(250f, 30f * staticInstance.model.variants.Count + 10f)), false, true, layout));
        }


        internal static void SetVariant(string variantName)
        {
            if (staticInstance == null)
            {
                return;
            }
            staticInstance.VariantName = variantName;
            staticInstance.Despawn();
            staticInstance.TrySpawn();
        }
    }
}
