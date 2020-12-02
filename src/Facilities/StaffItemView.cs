using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class StaffItemView : LayoutPanel, IPointerEnterHandler, IPointerExitHandler
	{
		StaffItem staff;

		public class StaffItemViewEvent : UnityEvent<StaffItem> { }
		StaffItemViewEvent onSelected;

		Toggle toggle;

		IconToggle assignedStatus;
		new UIText name;

		public override void CreateUI()
		{
			base.CreateUI ();

			onSelected = new StaffItemViewEvent ();

			toggle = gameObject.AddComponent<Toggle> ();
			toggle.targetGraphic = BackGround;
			toggle.onValueChanged.AddListener (onValueChanged);

			this.Horizontal ()
				.ChildAlignment(TextAnchor.MiddleCenter)
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Add<IconToggle> (out assignedStatus)
					.OnSprite(UIMain.tKerbal)
					.OffSprite(UIMain.tNoKerbal)
					.PreferredSize(23, 23)
					.Finish()
				.Add<UIText> (out name)
					.Finish()
				.Finish();

			assignedStatus.interactable = false;
		}

		void onValueChanged (bool on)
		{
			if (on) {
				onSelected.Invoke (staff);
			}
		}

		public StaffItemView Group (ToggleGroup group)
		{
			toggle.group = group;
			return this;
		}

		public StaffItemView OnSelected (UnityAction<StaffItem> action)
		{
			if (action != null) {
				onSelected.AddListener (action);
			}
			return this;
		}

		public StaffItemView Select ()
		{
			toggle.isOn = true;
			return this;
		}

		public StaffItemView Staff (StaffItem staff)
		{
			this.staff = staff;
			assignedStatus.SetIsOnWithoutNotify (staff.isAssigned);
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
