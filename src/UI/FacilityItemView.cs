using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class FacilityItemView : LayoutPanel, IPointerEnterHandler, IPointerExitHandler
	{
		FacilityItem facility;

		public class FacilityItemViewEvent : UnityEvent<FacilityItem> { }
		FacilityItemViewEvent onSelected;

		Toggle toggle;

		IconToggle openStatus;
		new UIText name;

		public override void CreateUI()
		{
			base.CreateUI ();

			onSelected = new FacilityItemViewEvent ();

			toggle = gameObject.AddComponent<Toggle> ();
			toggle.targetGraphic = BackGround;
			toggle.onValueChanged.AddListener (onValueChanged);

			this.Horizontal ()
				.ChildAlignment(TextAnchor.MiddleCenter)
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Add<IconToggle> (out openStatus)
					.OnSprite(UIMain.tIconOpen)
					.OffSprite(UIMain.tIconClosed)
					.PreferredSize(23, 23)
					.Finish()
				.Add<UIText> (out name)
					.Finish()
				.Finish();

			openStatus.interactable = false;
		}

		void onValueChanged (bool on)
		{
			if (on) {
				onSelected.Invoke (facility);
			}
		}

		public FacilityItemView Group (ToggleGroup group)
		{
			toggle.group = group;
			return this;
		}

		public FacilityItemView OnSelected (UnityAction<FacilityItem> action)
		{
			onSelected.AddListener (action);
			return this;
		}

		public FacilityItemView Select ()
		{
			toggle.isOn = true;
			return this;
		}

		public FacilityItemView Facility (FacilityItem facility)
		{
			this.facility = facility;
			openStatus.SetIsOnWithoutNotify (facility.isOpen);
			name.Text (facility.Name);
			return this;
		}
#region OnPointerEnter/Exit
		public void OnPointerEnter (PointerEventData eventData)
		{
		}

		public void OnPointerExit (PointerEventData eventData)
		{
		}
#endregion
	}
}
