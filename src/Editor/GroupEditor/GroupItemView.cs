using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class GroupItemView : LayoutPanel
	{
		GroupItem group;

		public class GroupItemViewEvent : UnityEvent<GroupItem> { }
		GroupItemViewEvent onSelect;

		Toggle toggle;

		UIText groupName;

		public override void CreateUI()
		{
			base.CreateUI ();

			onSelect = new GroupItemViewEvent ();

			toggle = gameObject.AddComponent<Toggle> ();
			toggle.targetGraphic = BackGround;
			toggle.onValueChanged.AddListener (onValueChanged);

			this.Horizontal ()
				.ChildAlignment(TextAnchor.MiddleCenter)
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)

				.Add<UIText>(out groupName)
					.Finish()

				.Finish();
		}

		void onValueChanged (bool on)
		{
			if (on) {
				onSelect.Invoke (group);
			}
		}

		public GroupItemView OnSelect (UnityAction<GroupItem> action)
		{
			if (action != null) {
				onSelect.AddListener (action);
			}
			return this;
		}

		public GroupItemView Group (ToggleGroup group)
		{
			toggle.group = group;
			return this;
		}

		public GroupItemView Select ()
		{
			toggle.isOn = true;
			return this;
		}

		public GroupItemView Group (GroupItem group)
		{
			this.group = group;
			groupName.Text(group.name);
			return this;
		}
	}
}
