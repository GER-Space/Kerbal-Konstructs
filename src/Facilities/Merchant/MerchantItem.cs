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

	public class MerchantItem
	{
		public Merchant merchant { get; private set; }
		public TradedResource tradedResource { get; private set; }
		public RMResourceInfo vesselResource { get; private set; }

		public string name
		{
			get { return tradedResource.resource.name; }
		}

		public double basePrice
		{
			get { return tradedResource.resource.unitCost; }
		}

		public double buyPrice
		{
			get { return tradedResource.multiplierBuy * basePrice; }
		}

		public double sellPrice
		{
			get { return tradedResource.multiplierSell * basePrice; }
		}

		public bool canSell { get { return tradedResource.canBeSold; } }
		public bool canBuy { get { return tradedResource.canBeBought; } }

		public MerchantItem (Merchant merchant, TradedResource tradedResource, RMResourceInfo vesselResource)
		{
			this.merchant = merchant;
			this.tradedResource = tradedResource;
			this.vesselResource = vesselResource;
		}

		public class List : List<MerchantItem>, UIKit.IListObject
		{
			public UnityAction<MerchantItem> onFromVessel { get; set; }
			public UnityAction<MerchantItem> onToVessel { get; set; }
			public Layout Content { get; set; }
			public Merchant Merchant { get; set; }
			public Vessel Vessel { get; set; }
			public RectTransform RectTransform
			{
				get { return Content.rectTransform; }
			}

			public void Create (int index)
			{
				Content
					.Add<MerchantItemView> ("MerchantItem")
						.OnFromVessel (onFromVessel)
						.OnToVessel (onToVessel)
						.Item (this[index])
						.Finish ()
					;
			}

			public void Update (GameObject obj, int index)
			{
				var view = obj.GetComponent<MerchantItemView> ();
				view.Item (this[index]);
			}

			public void Update(MerchantItem item)
			{
				int index = IndexOf (item);
				if (index >= 0) {
					var child = Content.rectTransform.GetChild (index);
					var view = child.GetComponent<MerchantItemView> ();
					view.Item(item);
				}
			}

			public List ()
			{
			}
		}
	}
}
