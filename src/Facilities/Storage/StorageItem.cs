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

	public class StorageItem
	{
		public Storage storage { get; private set; }
		public StoredResource storedResource { get; private set; }
		public RMResourceInfo vesselResource { get; private set; }

		public string name
		{
			get { return storedResource.resource.name; }
		}

		public double storedVolume
		{
			get { return storedResource.resource.volume * storedAmount; }
		}

		public double maxVolume
		{
			get { return storage.maxVolume; }
		}

		public double storedAmount
		{
			get { return storedResource.amount; }
			set { storedResource.amount = value; }
		}

		public double maxAmount
		{
			get { return maxVolume / storedResource.resource.volume; }
		}

		public StorageItem (Storage storage, StoredResource storedResource, RMResourceInfo vesselResource)
		{
			this.storage = storage;
			this.storedResource = storedResource;
			this.vesselResource = vesselResource;
		}

		public class List : List<StorageItem>, UIKit.IListObject
		{
			public UnityAction<StorageItem> onIncrement { get; set; }
			public UnityAction<StorageItem> onDecrement { get; set; }
			public Layout Content { get; set; }
			public Storage Storage { get; set; }
			public Vessel Vessel { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<StorageItemView> ("StorageItem")
						.OnIncrement (onIncrement)
						.OnDecrement (onDecrement)
						.Storage (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<StorageItemView> ();
				view.Storage (this[index]);
			}

			public void Update(StorageItem facility)
			{
				int index = IndexOf (facility);
				if (index >= 0) {
					var child = Content.rectTransform.GetChild (index);
					var view = child.GetComponent<StorageItemView> ();
					view.Storage(facility);
				}
			}

			public List ()
			{
			}
		}
	}
}
