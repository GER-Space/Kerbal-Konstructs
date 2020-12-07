using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class MapDecalItemView : LayoutPanel
	{
		MapDecalItem decal;

		public class MapDecalItemViewEvent : UnityEvent<MapDecalItem> { }
		MapDecalItemViewEvent onSelect;

		Toggle toggle;

		UIText category;
		UIText title;
		UIText mesh;

		public override void CreateUI()
		{
			base.CreateUI ();

			onSelect = new MapDecalItemViewEvent ();

			toggle = gameObject.AddComponent<Toggle> ();
			toggle.targetGraphic = BackGround;
			toggle.onValueChanged.AddListener (onValueChanged);

			this.Horizontal ()
				.ChildAlignment(TextAnchor.MiddleCenter)
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Add<UIText>(out category)
					.Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				.Add<UIText>(out title)
					.Finish()
				.Add<FlexibleSpace>() .Finish()
				.Add<UIText>(out mesh)
					.Finish()
				.Finish();
		}

		void onValueChanged (bool on)
		{
			if (on) {
				Debug.Log($"[MapDecalItemView] onValueChanged {decal.name}");
				onSelect.Invoke (decal);
			}
		}

		public MapDecalItemView OnSelect (UnityAction<MapDecalItem> action)
		{
			if (action != null) {
				onSelect.AddListener (action);
			}
			return this;
		}

		public MapDecalItemView Group (ToggleGroup group)
		{
			toggle.group = group;
			return this;
		}

		public MapDecalItemView Select ()
		{
			toggle.isOn = true;
			return this;
		}

		public MapDecalItemView MapDecal (MapDecalItem decal)
		{
			this.decal = decal;
			title.Text(decal.name);
			return this;
		}
	}
}
