using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KSP.UI.Screens;

using KodeUI;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.UI {

	public class StaffItem
	{
		public bool isAssigned { get; private set; }

		public StaffItem (bool isAssigned)
		{
			this.isAssigned = isAssigned;
		}

		public class List : List<StaffItem>, UIKit.IListObject
		{
			ToggleGroup group;
			public UnityAction<StaffItem> onSelected { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<StaffItemView> ()
						.Group (group)
						.OnSelected (onSelected)
						.Staff (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<StaffItemView> ();
				view.Staff (this[index]);
			}

			public void Update(StaffItem facility)
			{
				int index = IndexOf (facility);
				if (index >= 0) {
					var child = Content.rectTransform.GetChild (index);
					var view = child.GetComponent<StaffItemView> ();
					view.Staff(facility);
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
					var view = child.GetComponent<StaffItemView> ();
					view.Select ();
				}
			}
		}
	}
}
