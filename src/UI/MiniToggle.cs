using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class MiniToggle : Layout, IPointerClickHandler
	{
		UIToggle toggle;

		public bool isOn
		{
			get { return toggle.isOn; }
			set { toggle.isOn = value; }
		}

		public bool interactable
		{
			get { return toggle.interactable; }
			set { toggle.interactable = value; }
		}

		public override void CreateUI()
		{
			var toggleMin = new Vector2 (0, 0.25f);
			var toggleMax = new Vector2 (1, 0.75f);
			this.Horizontal ()
				.ControlChildSize (true, true)
				.ChildForceExpand (false,false)
				.ChildAlignment (TextAnchor.UpperCenter)
				.SizeDelta (0, 0)
				.MinSize (20, -1)
				.Add<LayoutAnchor> ()
					.DoPreferredWidth (true)
					.FlexibleLayout (false, true)
					.SizeDelta (0, 0)
					.Add<UIToggle> (out toggle)
						.Anchor(toggleMin, toggleMax)
						.AspectRatioSizeFitter (AspectRatioFitter.AspectMode.HeightControlsWidth, 1)
						.SizeDelta (0, 0)
						.Finish ()
					.Finish ()
				;
		}

		public MiniToggle Group (ToggleGroup group)
		{
			toggle.Group (group);
			return this;
		}

		public MiniToggle OnValueChanged (UnityAction<bool> action)
		{
			toggle.OnValueChanged (action);
			return this;
		}

		public MiniToggle SetIsOnWithoutNotify (bool on)
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
