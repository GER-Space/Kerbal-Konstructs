using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class LaunchsiteItemView : LayoutPanel, IPointerEnterHandler, IPointerExitHandler
	{
		LaunchsiteItem launchsite;

		public class LaunchsiteItemViewEvent : UnityEvent<LaunchsiteItem> { }
		LaunchsiteItemViewEvent onSelected;

		Toggle toggle;

		IconToggle openStatus;
		UIImage category;
		new UIText name;

		public override void CreateUI()
		{
			base.CreateUI ();

			onSelected = new LaunchsiteItemViewEvent ();

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
					.PreferredSize(30, 30)
					.Finish()
				.Add<UIImage> (out category)
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
				onSelected.Invoke (launchsite);
			}
		}

		public LaunchsiteItemView Group (ToggleGroup group)
		{
			toggle.group = group;
			return this;
		}

		public LaunchsiteItemView OnSelected (UnityAction<LaunchsiteItem> action)
		{
			onSelected.AddListener (action);
			return this;
		}

		public LaunchsiteItemView Select ()
		{
			toggle.isOn = true;
			return this;
		}

		public LaunchsiteItemView Launchsite (LaunchsiteItem launchsite)
		{
			this.launchsite = launchsite;
			openStatus.SetIsOnWithoutNotify (launchsite.isOpen);
			category.Image (launchsite.Icon);
			name.Text (launchsite.Name);
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
