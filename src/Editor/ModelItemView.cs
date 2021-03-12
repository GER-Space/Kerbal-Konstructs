using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class ModelItemView : LayoutPanel
	{
		ModelItem model;

		public class ModelItemViewEvent : UnityEvent<ModelItem> { }
		ModelItemViewEvent onSelect;

		Toggle toggle;

		UIText category;
		UIText title;
		UIText mesh;

		public override void CreateUI()
		{
			base.CreateUI ();

			onSelect = new ModelItemViewEvent ();

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
				Debug.Log($"[ModelItemView] onValueChanged {model.title}");
				onSelect.Invoke (model);
			}
		}

		public ModelItemView OnSelect (UnityAction<ModelItem> action)
		{
			if (action != null) {
				onSelect.AddListener (action);
			}
			return this;
		}

		public ModelItemView Group (ToggleGroup group)
		{
			toggle.group = group;
			return this;
		}

		public ModelItemView Select ()
		{
			toggle.isOn = true;
			return this;
		}

		public ModelItemView Model (ModelItem model)
		{
			this.model = model;
			category.Text(model.category);
			title.Text(model.title);
			mesh.Text(model.mesh);
			return this;
		}
	}
}
