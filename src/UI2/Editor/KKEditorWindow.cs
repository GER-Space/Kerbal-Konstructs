using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Base class for the KK editor windows (UI2 / PopupDialog). It gives every editor the same look and behaviour:
    /// a "-KK-  title  X" bar, a saved window position, dragging to the screen border, tooltips, switches and
    /// repeat buttons, plus the rows the editors share (number field with &lt;&lt; &lt; &gt; &gt;&gt;, on/off switch,
    /// choice buttons, increment buttons).
    /// Subclasses fill CreateContent() and react to OnUpdate() (called every frame while the window is open).
    /// </summary>
    internal abstract class KKEditorWindow
    {
        protected PopupDialog dialog;
        protected MultiOptionDialog optionDialog;
        protected List<DialogGUIBase> content;

        // shared sizes, so all editors line up the same
        protected const float itemHeight = 23f;
        protected const float smallButton = 30f;
        protected const float numberWidth = 75f;
        protected const float labelWidth = 120f;

        /// <summary>Name for the saved window position</summary>
        protected abstract string WindowName { get; }

        /// <summary>Text in the title bar</summary>
        protected abstract string WindowTitle { get; }

        protected virtual float WindowWidth => 340f;

        /// <summary>Normalized screen position used when no position was saved yet</summary>
        protected virtual Vector2 DefaultPosition => new Vector2(0.75f, 0.5f);

        /// <summary>Adds the rows of the window to content</summary>
        protected abstract void CreateContent();

        /// <summary>Called every frame while the window is open</summary>
        protected virtual void OnUpdate() { }

        /// <summary>What the red X in the title bar does</summary>
        protected virtual void OnCloseButton()
        {
            Close();
        }

        /// <summary>Closes the window. Subclasses that clean up override this and call DismissDialog().</summary>
        public virtual void Close()
        {
            DismissDialog();
        }

        public bool IsOpen()
        {
            return (dialog != null);
        }


        #region Window handling

        protected void Build()
        {
            KKStyle.Init();
            content = new List<DialogGUIBase>();
            content.Add(TitleBar());
            CreateContent();

            Rect windowRect = new Rect(WindowManager2.GetPosition(WindowName, DefaultPosition), new Vector2(WindowWidth, 50f));
            optionDialog = new MultiOptionDialog(WindowName, null, WindowTitle, null, windowRect, content.ToArray());
            dialog = PopupDialog.SpawnPopupDialog(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), optionDialog, false, null, false);
            // allow dragging the window up to the screen border
            KKWindowDrag.Attach(dialog);
            dialog.gameObject.GetChild("Title").SetActive(false);
            dialog.dialogToDisplay.OnUpdate += SafeUpdate;
            KKStyle.ApplyIconButtons(content);
            KKControls.ApplyControls(content);
            KKStyle.AttachTooltips(content);
        }


        private void SafeUpdate()
        {
            if (dialog == null)
            {
                return;
            }
            try
            {
                OnUpdate();
            }
            catch (Exception e)
            {
                Log.UserError(WindowName + ": " + e);
                Close();
            }
        }


        protected void DismissDialog()
        {
            if (dialog != null)
            {
                WindowManager2.SavePosition(dialog);
                dialog.Dismiss();
            }
            dialog = null;
            optionDialog = null;
        }


        /// <summary>
        /// Rebuilds the window, e.g. when rows appear or disappear
        /// </summary>
        protected void Reload()
        {
            if (dialog == null)
            {
                return;
            }
            DismissDialog();
            Build();
        }


        private DialogGUIBase TitleBar()
        {
            return new DialogGUIHorizontalLayout(
                new DialogGUILabel("-KK-", KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                new DialogGUILabel(delegate { return WindowTitle; }, KKStyle.windowTitle),
                new DialogGUIFlexibleSpace(),
                new DialogGUIButton("X", delegate { OnCloseButton(); }, 21f, 21.0f, false, KKStyle.DeadButtonRed)
                );
        }

        #endregion


        #region Row helpers

        protected void Separator()
        {
            content.Add(new DialogGUISpace(4f));
        }


        protected static DialogGUILabel Label(string text)
        {
            return new DialogGUILabel(text, KKStyle.whiteLabel);
        }


        protected static DialogGUILabel Label(Func<string> text)
        {
            return new DialogGUILabel(text, KKStyle.whiteLabel);
        }


        protected static DialogGUILabel GoldLabel(Func<string> text)
        {
            return new DialogGUILabel(text, KKStyle.goldLabel);
        }


        /// <summary>
        /// A text field that shows getValue() and calls setValue() when you press Enter or leave the field
        /// (KSP's DialogGUITextInput reports the text on end of edit; while the field has no focus it shows getValue())
        /// </summary>
        protected static DialogGUIBase TextField(Func<string> getValue, Action<string> setValue, int maxLength, float width)
        {
            DialogGUITextInput input = new DialogGUITextInput(getValue(), false, maxLength, delegate (string s) { setValue(s); return s; }, getValue, TMPro.TMP_InputField.ContentType.Standard, itemHeight);
            return new DialogGUIHorizontalLayout(width, itemHeight, 0f, new RectOffset(), TextAnchor.MiddleLeft, input);
        }


        /// <summary>
        /// A text field for several lines of text
        /// </summary>
        protected static DialogGUIBase TextArea(Func<string> getValue, Action<string> setValue, float width, float height)
        {
            DialogGUITextInput input = new DialogGUITextInput(getValue(), true, 2000, delegate (string s) { setValue(s); return s; }, getValue, TMPro.TMP_InputField.ContentType.Standard, height);
            return new DialogGUIHorizontalLayout(width, height, 0f, new RectOffset(), TextAnchor.UpperLeft, input);
        }


        /// <summary>
        /// label ............ [text field]
        /// </summary>
        protected static DialogGUIBase TextRow(string label, Func<string> getValue, Action<string> setValue, int maxLength, float width, string unit = null)
        {
            List<DialogGUIBase> row = new List<DialogGUIBase>
            {
                Label(label),
                new DialogGUIFlexibleSpace(),
                TextField(getValue, setValue, maxLength, width)
            };
            if (unit != null)
            {
                row.Add(Label(unit));
            }
            return new DialogGUIHorizontalLayout(row.ToArray());
        }


        /// <summary>
        /// label ............ &lt;&lt; (hold)  &lt;  &gt;  &gt;&gt; (hold)
        /// </summary>
        protected static DialogGUIBase MoveRow(string label, Callback decrease, Callback increase)
        {
            return new DialogGUIHorizontalLayout(
                Label(label),
                new DialogGUIFlexibleSpace(),
                KKControls.RepeatButton("<<", decrease, smallButton, itemHeight),
                new DialogGUIButton("<", decrease, smallButton, itemHeight, false),
                new DialogGUIButton(">", increase, smallButton, itemHeight, false),
                KKControls.RepeatButton(">>", increase, smallButton, itemHeight)
                );
        }


        /// <summary>
        /// label ...... [number]  &lt;&lt; (hold)  &lt;  &gt;  &gt;&gt; (hold)
        /// </summary>
        protected static DialogGUIBase NumberRow(string label, Func<string> getValue, Action<string> setValue, Callback decrease, Callback increase)
        {
            return new DialogGUIHorizontalLayout(
                Label(label),
                new DialogGUIFlexibleSpace(),
                TextField(getValue, setValue, 25, numberWidth),
                KKControls.RepeatButton("<<", decrease, smallButton, itemHeight),
                new DialogGUIButton("<", decrease, smallButton, itemHeight, false),
                new DialogGUIButton(">", increase, smallButton, itemHeight, false),
                KKControls.RepeatButton(">>", increase, smallButton, itemHeight)
                );
        }


        /// <summary>
        /// label ............ [on/off switch]
        /// </summary>
        protected static DialogGUIBase SwitchRow(string label, Func<bool> getValue, Action<bool> setValue, string tooltip = null)
        {
            DialogGUIButton toggle = KKControls.Switch(getValue, setValue);
            if (tooltip != null)
            {
                toggle.tooltipText = tooltip;
            }
            return new DialogGUIHorizontalLayout(
                Label(label),
                new DialogGUIFlexibleSpace(),
                toggle
                );
        }


        /// <summary>
        /// label ...... [A] [B] [C]: one button per choice; the current choice is greyed out
        /// (replaces the rows of radio-style buttons)
        /// </summary>
        protected static DialogGUIBase ChoiceRow<T>(string label, IEnumerable<KeyValuePair<string, T>> choices, Func<T> getValue, Action<T> setValue, float buttonWidth)
        {
            List<DialogGUIBase> row = new List<DialogGUIBase> { Label(label), new DialogGUIFlexibleSpace() };
            foreach (KeyValuePair<string, T> choice in choices)
            {
                T value = choice.Value;
                DialogGUIButton button = new DialogGUIButton(choice.Key, delegate { setValue(value); }, delegate { return !EqualityComparer<T>.Default.Equals(getValue(), value); }, buttonWidth, itemHeight, false);
                button.tooltipText = choice.Key;
                row.Add(button);
            }
            return new DialogGUIHorizontalLayout(row.ToArray());
        }


        /// <summary>
        /// The row of increment presets: the current one is greyed out
        /// </summary>
        protected static DialogGUIBase IncrementButtons(Func<float> getIncrement, Action<float> setIncrement, params float[] values)
        {
            List<DialogGUIBase> row = new List<DialogGUIBase>();
            foreach (float value in values)
            {
                float thisValue = value;
                DialogGUIButton button = new DialogGUIButton(thisValue.ToString(CultureInfo.InvariantCulture), delegate { setIncrement(thisValue); }, delegate { return getIncrement() != thisValue; }, 48f, 20f, false);
                button.tooltipText = "Set the increment to " + thisValue.ToString(CultureInfo.InvariantCulture);
                row.Add(button);
            }
            return new DialogGUIHorizontalLayout(row.ToArray());
        }


        /// <summary>
        /// "Increment: [field]" followed by the preset buttons
        /// </summary>
        protected void IncrementRows(Func<float> getIncrement, Action<float> setIncrement, params float[] values)
        {
            content.Add(TextRow("Increment:",
                delegate { return getIncrement().ToString(CultureInfo.InvariantCulture); },
                delegate (string s) { float value; if (MiscUtils.TryParseFloat(s, out value) && value > 0) { setIncrement(value); } },
                10, numberWidth));
            content.Add(IncrementButtons(getIncrement, setIncrement, values));
        }

        #endregion


        #region Parsing

        protected static bool TryParseDouble(string text, out double value)
        {
            value = 0;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }
            if (!double.TryParse(text.Trim().Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value)
                || double.IsNaN(value) || double.IsInfinity(value))
            {
                value = 0;
                return false;
            }
            return true;
        }


        protected static bool TryParseFloat(string text, out float value)
        {
            return MiscUtils.TryParseFloat(text, out value);
        }


        protected static bool TryParseInt(string text, out int value)
        {
            value = 0;
            return !string.IsNullOrEmpty(text) && int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }

        #endregion


        /// <summary>
        /// True while the player types into a text field, so the editor keys (WASD, Q/E, ...) don't move the object
        /// </summary>
        internal static bool IsTyping()
        {
            UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
            if (eventSystem == null || eventSystem.currentSelectedGameObject == null)
            {
                return false;
            }
            TMPro.TMP_InputField input = eventSystem.currentSelectedGameObject.GetComponent<TMPro.TMP_InputField>();
            return (input != null && input.isFocused);
        }
    }
}
