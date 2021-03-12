using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class InstanceItemView : LayoutPanel
	{
		InstanceItem instance;

		public class InstanceItemViewEvent : UnityEvent<InstanceItem> { }
		InstanceItemViewEvent onSelect;
		InstanceItemViewEvent onSetAsSnap;

		Toggle toggle;

		LaunchSiteCategoryIcon category;
		UIText instanceName;

		public override void CreateUI()
		{
			base.CreateUI ();

			onSelect = new InstanceItemViewEvent ();
			onSetAsSnap = new InstanceItemViewEvent ();

			toggle = gameObject.AddComponent<Toggle> ();
			toggle.targetGraphic = BackGround;
			toggle.onValueChanged.AddListener (onValueChanged);

			this.Horizontal ()
				.ChildAlignment(TextAnchor.MiddleCenter)
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Add<LaunchSiteCategoryIcon>(out category)
					.PreferredSize(23, 23)
					.Finish()
				.Add<UIText>(out instanceName)
					.FlexibleLayout (true, false)
					.Finish()
				.Add<UIButton>()
					.Image(UIMain.tFocus)
					.OnClick(OnSetAsSnap)
					//.Tooltip(KKLocalization.SetAsSnapTarget)
					.Finish()
				.Finish();
		}

		void onValueChanged (bool on)
		{
			if (on) {
				Debug.Log($"[InstanceItemView] onValueChanged {instance.name}");
				onSelect.Invoke (instance);
			}
		}

		void OnSetAsSnap ()
		{
			Debug.Log($"[InstanceItemView] OnSetAsSnap {instance.name}");
			onSetAsSnap.Invoke (instance);
		}

		public InstanceItemView OnSelect (UnityAction<InstanceItem> action)
		{
			onSelect.AddListener (action);
			return this;
		}

		public InstanceItemView OnSetAsSnap (UnityAction<InstanceItem> action)
		{
			onSetAsSnap.AddListener (action);
			return this;
		}

		public InstanceItemView Group (ToggleGroup group)
		{
			toggle.group = group;
			return this;
		}

		public InstanceItemView Select ()
		{
			toggle.isOn = true;
			return this;
		}

		public InstanceItemView Instance (InstanceItem instance)
		{
			this.instance = instance;
			instanceName.Text(instance.name);
			category.Category(instance.category);
			return this;
		}
	}
}
