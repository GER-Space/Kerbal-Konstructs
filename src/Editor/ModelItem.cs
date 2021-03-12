using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KSP.UI.Screens;

using KodeUI;
using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using KerbalKonstructs.ResourceManager;

namespace KerbalKonstructs.UI {

	public class ModelItem
	{
		public StaticModel model { get; private set; }
		public string category { get { return model.category; } }
		public string title { get { return model.title; } }
		public string mesh { get { return model.mesh; } }

		public ModelItem (StaticModel model)
		{
			this.model = model;
		}

		public class List : List<ModelItem>, UIKit.IListObject
		{
			ToggleGroup group;
			public UnityAction<ModelItem> onSelect { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<ModelItemView> ("ModelItem")
						.Group (group)
						.OnSelect (onSelect)
						.Model (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<ModelItemView> ();
				view.Model (this[index]);
			}

			public void Update(ModelItem item)
			{
				int index = IndexOf (item);
				if (index >= 0) {
					var child = Content.rectTransform.GetChild (index);
					var view = child.GetComponent<ModelItemView> ();
					view.Model(item);
				}
			}

			public List (ToggleGroup group)
			{
				this.group = group;
			}

			public void Select (int index)
			{
				if (index >= 0 && index < Count) {
					group.SetAllTogglesOff (false);
					var child = Content.rectTransform.GetChild (index);
					var view = child.GetComponent<ModelItemView> ();
					view.Select ();
				}
			}
		}
	}
}
