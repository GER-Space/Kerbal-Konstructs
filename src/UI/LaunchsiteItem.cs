using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KSP.UI.Screens;

using KodeUI;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.UI {

	public class LaunchsiteItem
	{
		public KKLaunchSite launchsite { get; private set; }
		public string Name { get { return launchsite.Name; } }
		public Sprite Icon { get { return launchsite.Icon; } }
		public bool isOpen { get { return launchsite.isOpen; } }

		public LaunchsiteItem (KKLaunchSite launchsite)
		{
			this.launchsite = launchsite;
		}

		public class List : List<LaunchsiteItem>, UIKit.IListObject
		{
			ToggleGroup group;
			public UnityAction<LaunchsiteItem> onSelected { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<LaunchsiteItemView> ()
						.Group (group)
						.OnSelected (onSelected)
						.Launchsite (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<LaunchsiteItemView> ();
				view.Launchsite (this[index]);
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
					var view = child.GetComponent<LaunchsiteItemView> ();
					view.Select ();
				}
			}
		}
	}
}
