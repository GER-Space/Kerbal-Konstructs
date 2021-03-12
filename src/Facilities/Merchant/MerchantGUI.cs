using KerbalKonstructs.Core;
using KerbalKonstructs.ResourceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using UnityEngine;

using KodeUI;

namespace KerbalKonstructs.UI
{
    internal class MerchantGUI : VerticalLayout
    {
		StackSize stackSize;
		ListView merchantList;
		MerchantItem.List merchantItems;
		InfoLine thanksForTrading;

		public override void CreateUI()
		{
			base.CreateUI();

			this.ChildForceExpand(true, false)
				.Add<FixedSpace>() .Size(4) .Finish()
				.Add<StackSize>(out stackSize) .Finish()
				.Add<UIText>()
					.Text(KKLocalization.BuySellResources)
					.Finish()
				.Add<ListView>(out merchantList)
					.PreferredHeight(120)
					.Finish()
				.Add<InfoLine>(out thanksForTrading)
					.Label(KKLocalization.ThanksForTrading)
					.Finish()
				;

			merchantItems = new MerchantItem.List();
			merchantItems.Content = merchantList.Content;
			merchantItems.onFromVessel = OnFromVessel;
			merchantItems.onToVessel = OnToVessel;
		}

		void OnFromVessel(MerchantItem item)
		{
			double increment = stackSize.Increment;
			increment += item.vesselResource.Transfer(-increment);
			BookCredits(increment * item.sellPrice);
			merchantItems.Update(item);
		}

		void OnToVessel(MerchantItem item)
		{
			double increment = stackSize.Increment;
			increment -= item.vesselResource.Transfer(increment);
			BookCredits(-increment * item.buyPrice);
			merchantItems.Update(item);
		}

		public void UpdateUI(StaticInstance instance)
		{
			var merchant = instance.myFacilities[0] as Merchant;
			if (merchantItems.Merchant != merchant || merchantItems.Vessel != FlightGlobals.ActiveVessel) {
				merchantItems.Merchant = merchant;
				merchantItems.Vessel = FlightGlobals.ActiveVessel;
				BuildMerchantItems();
			}
		}

		void BuildMerchantItems()
		{
			merchantItems.Clear();

			var merchant = merchantItems.Merchant;

			Part rootPart = merchantItems.Vessel.parts[0].localRoot;
			var manager = new RMResourceManager(merchantItems.Vessel.parts, rootPart);
			var vesselResources = manager.masterSet.resources;

			var tradedResources = merchant.Resources;
			for (int i = tradedResources.Count; i-- > 0; ) {
				var resource = tradedResources[i];
				RMResourceInfo vesselResource;
				vesselResources.TryGetValue(resource.resource.name, out vesselResource);
				merchantItems.Add(new MerchantItem(merchant, resource, vesselResource));
			}
			UIKit.UpdateListContent(merchantItems);
		}

        private static void BookCredits(double amount)
        {
            if (HighLogic.CurrentGame.Mode == Game.Modes.CAREER) {
                Funding.Instance.AddFunds(amount,TransactionReasons.Vessels);
               // Log.Normal("Transaction for: " + (amount).ToString());
            }
        }
    }
}
