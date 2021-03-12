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

	public class InstanceItem
	{
		public StaticInstance instance { get; private set; }

		public LaunchSiteCategory category
		{
			get {
				if (instance.hasLauchSites) {
					return instance.launchSite.sitecategory;
				}
				return LaunchSiteCategory.None;
			}
		}

		public string name
		{
			get { return $"{instance.model.title} ({instance.indexInGroup})"; }
		}

		public InstanceItem (StaticInstance instance)
		{
			this.instance = instance;
		}

		public class List : List<InstanceItem>, UIKit.IListObject
		{
			ToggleGroup group;
			public UnityAction<InstanceItem> onSelect { get; set; }
			public UnityAction<InstanceItem> onSetAsSnap { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<InstanceItemView> ("InstanceItem")
						.Group (group)
						.OnSelect (onSelect)
						.OnSetAsSnap (onSetAsSnap)
						.Instance (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<InstanceItemView> ();
				view.Instance (this[index]);
			}

			public void Update(InstanceItem item)
			{
				int index = IndexOf (item);
				if (index >= 0) {
					var child = Content.rectTransform.GetChild (index);
					var view = child.GetComponent<InstanceItemView> ();
					view.Instance(item);
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
					var view = child.GetComponent<InstanceItemView> ();
					view.Select ();
				}
			}
		}
	}
}
