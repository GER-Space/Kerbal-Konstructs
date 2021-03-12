using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class ToggleText : Layout, IPointerClickHandler
	{
		MiniToggle toggle;
		UIText label;

		public bool interactable
		{
			get { return toggle.interactable; }
			set { toggle.interactable = value; }
		}

		public override void CreateUI()
		{
			var toggleMin = new Vector2 (0, 0.25f);
			var toggleMax = new Vector2 (1, 0.75f);
			var textMargins = new Vector4 (5, 5, 10, 10);
			this.Horizontal ()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.SizeDelta(0, 0)
				.Add<MiniToggle> (out toggle)
					.SizeDelta (0, 0)
					.Finish ()
				.Add<UIText> (out label)
					.Alignment (TextAlignmentOptions.Left)
					.Margin (textMargins)
					.SizeDelta(0, 0)
					.Finish ()
				;
		}

		public ToggleText Group (ToggleGroup group)
		{
			toggle.Group (group);
			return this;
		}

		public ToggleText Text (string text)
		{
			label.Text (text);
			return this;
		}

		public ToggleText OnValueChanged (UnityAction<bool> action)
		{
			toggle.OnValueChanged (action);
			return this;
		}

		public ToggleText SetIsOnWithoutNotify (bool on)
		{
			toggle.SetIsOnWithoutNotify (on);
			return this;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			toggle.OnPointerClick (eventData);
		}
	}
}
