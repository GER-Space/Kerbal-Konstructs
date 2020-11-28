using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using KSP.UI.Screens;

using KodeUI;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.UI {

	public class FacilityItem
	{
		public StaticInstance facility { get; private set; }
		public string Name { get { return facility.model.title; } }
		public float Cost { get { return facility.myFacilities[0].OpenCost; } }
		public bool isOpen {
			get {
				bool isOpen = facility.myFacilities[0].isOpen;
				return Cost == 0 || isOpen;
			}
		}

		public FacilityItem (StaticInstance facility)
		{
			this.facility = facility;
		}

		public class List : List<FacilityItem>, UIKit.IListObject
		{
			ToggleGroup group;
			public UnityAction<FacilityItem> onSelected { get; set; }
			public Layout Content { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<FacilityItemView> ()
						.Group (group)
						.OnSelected (onSelected)
						.Facility (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<FacilityItemView> ();
				view.Facility (this[index]);
			}

			public void Update(FacilityItem facility)
			{
				int index = IndexOf (facility);
				if (index >= 0) {
					var child = Content.rectTransform.GetChild (index);
					var view = child.GetComponent<FacilityItemView> ();
					view.Facility(facility);
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
					var view = child.GetComponent<FacilityItemView> ();
					view.Select ();
				}
			}
		}
	}
}
