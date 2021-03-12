using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using System;
using UnityEngine;

using KodeUI;

namespace KerbalKonstructs.UI
{
    internal class TrackingStationGUI : VerticalLayout
    {
		InfoLine rangeInfo;
		InfoLine groupInfo;

		public override void CreateUI()
		{
			base.CreateUI();

			this.ChildForceExpand(true, false)
				.Add<FixedSpace>() .Size(2) .Finish()
				.Add<InfoLine>(out rangeInfo)
					.Label(KKLocalization.Range)
					.Finish()
				.Add<FixedSpace>() .Size(2) .Finish()
				.Add<InfoLine>(out groupInfo)
					.Label(KKLocalization.MemberOfGroup)
					.Finish()
				.Add<FixedSpace>() .Size(5) .Finish()
				;
		}

        public void UpdateUI(StaticInstance instance)
        {

            GroundStation station = instance.myFacilities[0] as GroundStation;
			rangeInfo.Info($"{station.TrackingShort:F0} Mm");
			groupInfo.Info(instance.Group);
        }
    }
}
