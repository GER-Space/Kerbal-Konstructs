using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using System;
using System.Reflection;
using UnityEngine;
using KSP.Localization;

using KodeUI;

namespace KerbalKonstructs.UI
{
    public class ProductionGUI : VerticalLayout
    {
		InfoLine produces;
		InfoLine current;
		UIButton transfer;
		InfoLine productionRate;

		IProduction production;

		public override void CreateUI()
		{
			base.CreateUI();
			this.ChildForceExpand(true, false)
				.Add<InfoLine>(out produces)
					.Label(KKLocalization.ProductionProduces)
					.Finish()
				.Add<InfoLine>(out current)
					.Label(KKLocalization.ProductionCurrent)
					.Finish()
				.Add<UIButton>(out transfer)
					.OnClick(TransmitProduction)
					.Finish()
				.Add<InfoLine>(out productionRate)
					.Label(KKLocalization.ProductionRate)
					.Finish()
				;
		}

		void TransmitProduction()
		{
			production.TransmitProduction();
		}

		public void UpdateUI(StaticInstance selectedFacility)
		{
			production = selectedFacility.myFacilities[0] as IProduction;

			produces.Info(production.Produces);
			current.Info($"{production.UpdateProduction():F0}");
			transfer.Text(Localizer.Format(KKLocalization.TransferToKSC, production.Produces));
			productionRate.Info($"{production.CurrentRate:F2}/d");
		}
    }
}
