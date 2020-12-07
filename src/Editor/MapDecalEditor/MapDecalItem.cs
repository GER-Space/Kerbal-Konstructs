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

	public class MapDecalItem
	{
		public MapDecalInstance decal { get; private set; }
		public string name { get { return decal.Name; } }

		public MapDecalItem (MapDecalInstance decal)
		{
			this.decal = decal;
		}

		public class List : List<MapDecalItem>, UIKit.IListObject
		{
			ToggleGroup group;
			public UnityAction<MapDecalItem> onSelect { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<MapDecalItemView> ("MapDecalItem")
						.Group (group)
						.OnSelect (onSelect)
						.MapDecal (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<MapDecalItemView> ();
				view.MapDecal (this[index]);
			}

			public void Update(MapDecalItem item)
			{
				int index = IndexOf (item);
				if (index >= 0) {
					var child = Content.rectTransform.GetChild (index);
					var view = child.GetComponent<MapDecalItemView> ();
					view.MapDecal(item);
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
					var view = child.GetComponent<MapDecalItemView> ();
					view.Select ();
				}
			}
		}
	}
}
