using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KerbalKonstructs.UI2
{
    /// <summary>
    /// Extra controls for UI2 (PopupDialog) windows:
    /// - Switch: an on/off switch instead of a toggle / radio button, always showing the current value
    /// - RepeatButton: a button that acts every frame while it is held down (like GUILayout.RepeatButton)
    /// Create them while building the content, then call ApplyControls(content) after the dialog was spawned.
    /// </summary>
    internal static class KKControls
    {
        private class SwitchInfo
        {
            internal Func<bool> getValue;
            internal Action<bool> setValue;
        }

        private static Dictionary<DialogGUIBase, SwitchInfo> pendingSwitches = new Dictionary<DialogGUIBase, SwitchInfo>();
        private static Dictionary<DialogGUIBase, Callback> pendingRepeats = new Dictionary<DialogGUIBase, Callback>();

        private static Sprite switchOnSprite = null;
        private static Sprite switchOffSprite = null;

        internal const float switchWidth = 48f;
        internal const float switchHeight = 24f;


        /// <summary>
        /// An on/off switch. It always shows getValue(), also when the value is changed elsewhere.
        /// Clicking it calls setValue(!getValue()).
        /// </summary>
        internal static DialogGUIButton Switch(Func<bool> getValue, Action<bool> setValue)
        {
            DialogGUIButton button = new DialogGUIButton("", delegate { setValue(!getValue()); }, switchWidth, switchHeight, false);
            pendingSwitches[button] = new SwitchInfo { getValue = getValue, setValue = setValue };
            return button;
        }


        /// <summary>
        /// A button that calls onHold every frame while it is held down.
        /// </summary>
        internal static DialogGUIButton RepeatButton(string text, Callback onHold, float width, float height)
        {
            DialogGUIButton button = new DialogGUIButton(text, delegate { }, width, height, false);
            pendingRepeats[button] = onHold;
            return button;
        }


        /// <summary>
        /// Call after the dialog was spawned: sets up the switches and repeat buttons in the content.
        /// </summary>
        internal static void ApplyControls(IEnumerable<DialogGUIBase> items)
        {
            if (items == null)
            {
                return;
            }
            foreach (DialogGUIBase item in items)
            {
                ApplyControl(item);
            }
        }


        private static void ApplyControl(DialogGUIBase item)
        {
            if (item == null)
            {
                return;
            }

            SwitchInfo switchInfo;
            if (pendingSwitches.TryGetValue(item, out switchInfo))
            {
                pendingSwitches.Remove(item);
                SetupSwitch(item, switchInfo);
            }

            Callback holdAction;
            if (pendingRepeats.TryGetValue(item, out holdAction))
            {
                pendingRepeats.Remove(item);
                if (item.uiItem != null)
                {
                    KKRepeatButton repeat = item.uiItem.AddComponent<KKRepeatButton>();
                    repeat.onHold = holdAction;
                }
            }

            if (item.children != null)
            {
                foreach (DialogGUIBase child in item.children)
                {
                    ApplyControl(child);
                }
            }
        }


        private static void SetupSwitch(DialogGUIBase item, SwitchInfo switchInfo)
        {
            Sprite onSprite = GetSwitchSprite(true);
            Sprite offSprite = GetSwitchSprite(false);
            if (item.uiItem == null || onSprite == null || offSprite == null)
            {
                return;
            }
            UnityEngine.UI.Button button = item.uiItem.GetComponent<UnityEngine.UI.Button>();
            UnityEngine.UI.Image image = (button != null && button.image != null) ? button.image : item.uiItem.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.type = UnityEngine.UI.Image.Type.Simple;
                image.preserveAspect = true;
                image.color = Color.white;
                KKSwitchView view = item.uiItem.AddComponent<KKSwitchView>();
                view.image = image;
                view.onSprite = onSprite;
                view.offSprite = offSprite;
                view.getValue = switchInfo.getValue;
                view.Refresh();
            }
            if (button != null)
            {
                // keep the switch picture, tint it on hover / press
                button.transition = UnityEngine.UI.Selectable.Transition.ColorTint;
                UnityEngine.UI.ColorBlock colors = button.colors;
                colors.normalColor = Color.white;
                colors.highlightedColor = new Color(0.85f, 1f, 0.75f, 1f);
                colors.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f);
                colors.selectedColor = Color.white;
                colors.colorMultiplier = 1f;
                button.colors = colors;
            }
            TMPro.TextMeshProUGUI text = item.uiItem.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            if (text != null)
            {
                text.text = "";
            }
        }


        private static Sprite GetSwitchSprite(bool on)
        {
            if (on)
            {
                if (switchOnSprite == null)
                {
                    switchOnSprite = LoadSprite("KerbalKonstructs/Assets/switchOn");
                }
                return switchOnSprite;
            }
            if (switchOffSprite == null)
            {
                switchOffSprite = LoadSprite("KerbalKonstructs/Assets/switchOff");
            }
            return switchOffSprite;
        }


        private static Sprite LoadSprite(string textureName)
        {
            Texture2D texture = GameDatabase.Instance.GetTexture(textureName, false);
            if (texture == null)
            {
                Log.UserWarning("Icon not found: " + textureName);
                return null;
            }
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }


    /// <summary>
    /// Shows the on or off picture of a switch, following its value every frame.
    /// </summary>
    internal class KKSwitchView : MonoBehaviour
    {
        internal UnityEngine.UI.Image image;
        internal Sprite onSprite;
        internal Sprite offSprite;
        internal Func<bool> getValue;

        internal void Refresh()
        {
            if (image == null || getValue == null)
            {
                return;
            }
            Sprite wanted = getValue() ? onSprite : offSprite;
            if (image.sprite != wanted)
            {
                image.sprite = wanted;
            }
        }

        private void Update()
        {
            Refresh();
        }
    }


    /// <summary>
    /// Calls onHold every frame while the UI element is held down with the left mouse button.
    /// </summary>
    internal class KKRepeatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        internal Callback onHold = null;
        private bool held = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            held = (eventData.button == PointerEventData.InputButton.Left);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            held = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            held = false;
        }

        private void OnDisable()
        {
            held = false;
        }

        private void Update()
        {
            if (held && onHold != null)
            {
                onHold();
            }
        }
    }
}
