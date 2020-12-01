using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using KSP.Localization;
using TMPro;

using KodeUI;

namespace KerbalKonstructs.UI {

	public class MerchantItemView : LayoutPanel
	{
		MerchantItem item;

		public class MerchantItemViewEvent : UnityEvent<MerchantItem> { }
		MerchantItemViewEvent onFromVesselEvent;
		MerchantItemViewEvent onToVesselEvent;

		UIText resourceName;
		InfoLine sell;
		InfoLine buy;
		InfoLine vessel;
		TransferButtons transferButtons;

		public override void CreateUI()
		{
			base.CreateUI ();

			onFromVesselEvent = new MerchantItemViewEvent ();
			onToVesselEvent = new MerchantItemViewEvent ();

			var sellMin = new Vector2(0, 0);
			var sellMax = new Vector2(0.48f, 1);
			var buyMin = new Vector2(0.52f, 0);
			var buyMax = new Vector2(1, 1);

			this.Vertical ()
				.ChildAlignment(TextAnchor.MiddleCenter)
				.ControlChildSize (true, true)
				.ChildForceExpand (false, false)
				.Add<UIText>(out resourceName)
					.Finish()
				.Add<InfoLine>(out vessel)
					.Label(KKLocalization.MerchantHeld)
					.Finish()
				.Add<LayoutAnchor>()
					.DoPreferredWidth (true)
					.DoPreferredHeight (true)
					.FlexibleLayout(true, false)
					.Add<InfoLine>(out sell)
						.Label(KKLocalization.MerchantSellFor)
						.Anchor(sellMin, sellMax)
						.Finish()
					.Add<InfoLine>(out buy)
						.Label(KKLocalization.MerchantBuyFor)
						.Anchor(buyMin, buyMax)
						.Finish()
					.Finish()
				.Add<TransferButtons>(out transferButtons)
					.OnFromVessel(onFromVessel)
					.OnToVessel(onToVessel)
					.Finish()
				.Finish();
		}

		void onFromVessel ()
		{
			Debug.Log($"[MerchantItemView] onFromVessel {item.name}");
			onFromVesselEvent.Invoke (item);
		}

		void onToVessel ()
		{
			Debug.Log($"[MerchantItemView] onToVessel {item.name}");
			onToVesselEvent.Invoke (item);
		}

		public MerchantItemView OnFromVessel (UnityAction<MerchantItem> action)
		{
			onFromVesselEvent.AddListener (action);
			return this;
		}

		public MerchantItemView OnToVessel (UnityAction<MerchantItem> action)
		{
			onToVesselEvent.AddListener (action);
			return this;
		}

		public MerchantItemView Item (MerchantItem item)
		{
			this.item = item;
			resourceName.Text(item.name);
			sell.Info($"{item.sellPrice:F2}");
			buy.Info($"{item.buyPrice:F2}");
			transferButtons.fromVesselInteractable = item.canSell;
			transferButtons.toVesselInteractable = item.canBuy;
			if (item.vesselResource != null) {
				transferButtons.SetActive(true);
				vessel.SetActive(true);
				vessel.Info($"{item.vesselResource.amount:F2} / {item.vesselResource.maxAmount:F2}");
			} else {
				transferButtons.SetActive(false);
				vessel.SetActive(false);
			}
			return this;
		}
	}
}
