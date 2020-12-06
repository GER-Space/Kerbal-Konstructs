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

	public class GroupItem
	{
		public GroupCenter group { get; private set; }

		public GroupItem (GroupCenter group)
		{
			this.group = group;
		}
		public string name { get { return group.Group; } }

		public class List : List<GroupItem>, UIKit.IListObject
		{
			ToggleGroup group;
			public UnityAction<GroupItem> onSelect { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<GroupItemView> ("GroupItem")
						.Group (group)
						.OnSelect (onSelect)
						.Group (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<GroupItemView> ();
				view.Group (this[index]);
			}

			public void Update(GroupItem item)
			{
				int index = IndexOf (item);
				if (index >= 0) {
					var child = Content.rectTransform.GetChild (index);
					var view = child.GetComponent<GroupItemView> ();
					view.Group(item);
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
					var view = child.GetComponent<GroupItemView> ();
					view.Select ();
				}
			}
		}
	}
}
