using KerbalKonstructs.UI;
using KerbalKonstructs.Utilities;
using System;
using UnityEngine;
using UnityEngine.UI;

using KodeUI;

namespace KerbalKonstructs.Modules
{
    class MapIconManager : Layout
    {
        private static MapIconManager _instance = null;
        internal static MapIconManager instance
        {
            get {
                if (_instance == null) {
					_instance = UIKit.CreateUI<MapIconManager> (UIMain.appCanvasRect, "KKMapIconManager");

                }
                return _instance;
            }
        }


        Rect mapManagerRect = new Rect(250, 40, 455, 75);

        public float iFundsOpen = 0;
        public float iFundsClose = 0;
        public Boolean isOpen = false;

        public float iFundsOpen2 = 0;
        public float iFundsClose2 = 0;
        public Boolean isOpen2 = false;
        public Boolean bChangeTargetType = false;
        public Boolean bChangeTarget = false;
        public Boolean showBaseManager = false;
        public Boolean bHideOccluded = false;
        public Boolean bHideOccluded2 = false;

		StateButton openedBases;
		StateButton closedBases;
		StateButton trackingStations;
		StateButton rocketbases;
		StateButton helipads;
		StateButton runways;
		StateButton waterLaunch;
		StateButton other;
		StateButton recovery;
		UIButton closeButton;

        Vector3 ObjectPos = new Vector3(0, 0, 0);

		public override void CreateUI()
		{
			base.CreateUI();

			gameObject.AddComponent<Draggable>();
			
			this.Horizontal()
				.ControlChildSize(true, true)
				.ChildForceExpand(false,false)
				.PreferredSizeFitter(true, true)
				.Anchor(AnchorPresets.TopLeft)
				.Pivot(PivotPresets.TopLeft)
				.X(250).Y(-40)
				.SetSkin("KK.Default")

				.Add<StateButton>(out openedBases, "mapOpenBases")
					.Tooltip(KKLocalization.Opened)
					.OnSprite(UIMain.tOpenBasesOn)
					.OffSprite(UIMain.tOpenBasesOff)
					.StateVar(KerbalKonstructs.instance.mapShowOpen)
					.PreferredSize(32, 32)
					.Finish()
				.Add<StateButton>(out closedBases, "mapClosedBases")
					.Tooltip(KKLocalization.Opened)
					.OnSprite(UIMain.tClosedBasesOn)
					.OffSprite(UIMain.tClosedBasesOff)
					.StateVar(KerbalKonstructs.instance.mapShowClosed)
					.PreferredSize(32, 32)
					.Finish()
				.Add<UIEmpty>()
					.PreferredSize(8, 32)
					.Finish()
				.Add<StateButton>(out trackingStations, "mapTracking")
					.Tooltip(KKLocalization.TrackingStations)
					.OnSprite(UIMain.tTrackingOn)
					.OffSprite(UIMain.tTrackingOff)
					.StateVar(KerbalKonstructs.instance.mapShowGroundStation)
					.PreferredSize(32, 32)
					.Finish()
				.Add<UIEmpty>()
					.PreferredSize(8, 32)
					.Finish()
				.Add<StateButton>(out rocketbases, "mapLaunchpads")
					.Tooltip(KKLocalization.Rocketpads)
					.OnSprite(UIMain.tLaunchpadsOn)
					.OffSprite(UIMain.tLaunchpadsOff)
					.StateVar(KerbalKonstructs.instance.mapShowRocketbases)
					.PreferredSize(32, 32)
					.Finish()
				.Add<StateButton>(out helipads, "mapHelipads")
					.Tooltip(KKLocalization.Helipads)
					.OnSprite(UIMain.tHelipadsOn)
					.OffSprite(UIMain.tHelipadsOff)
					.StateVar(KerbalKonstructs.instance.mapShowHelipads)
					.PreferredSize(32, 32)
					.Finish()
				.Add<StateButton>(out runways, "mapRunways")
					.Tooltip(KKLocalization.Runways)
					.OnSprite(UIMain.tRunwaysOn)
					.OffSprite(UIMain.tRunwaysOff)
					.StateVar(KerbalKonstructs.instance.mapShowRunways)
					.PreferredSize(32, 32)
					.Finish()
				.Add<StateButton>(out waterLaunch, "mapWater")
					.Tooltip(KKLocalization.WaterLaunch)
					.OnSprite(UIMain.tWaterOn)
					.OffSprite(UIMain.tWaterOff)
					.StateVar(KerbalKonstructs.instance.mapShowWaterLaunch)
					.PreferredSize(32, 32)
					.Finish()
				.Add<StateButton>(out other, "mapOther")
					.Tooltip(KKLocalization.Other)
					.OnSprite(UIMain.tOtherOn)
					.OffSprite(UIMain.tOtherOff)
					.StateVar(KerbalKonstructs.instance.mapShowOther)
					.PreferredSize(32, 32)
					.Finish()
				.Add<StateButton>(out recovery, "mapRecovery")
					.Tooltip(KKLocalization.Other)
					.StateVar(KerbalKonstructs.instance.mapShowRecovery)
					.Text("$")
					.PreferredSize(32, 32)
					.Finish()
				.Add<UIEmpty>()
					.PreferredSize(8, 32)
					.Finish()
				.Add<StateButton>(out other, "mapHide")
					.Tooltip(KKLocalization.Occlude)
					.OnSprite(UIMain.tHideOn)
					.OffSprite(UIMain.tHideOff)
					.StateVar(MapIconDraw.mapHideIconsBehindBody)
					.PreferredSize(32, 32)
					.Finish()
				.Add<UIButton>(out closeButton, "mapCloseButton")
					.OnClick(Close)
					.Text("X")
					.PreferredSize(20, 20)
					.Finish()
				.Finish()
				;
			openedBases.SetActive(MiscUtils.isCareerGame());
			closedBases.SetActive(MiscUtils.isCareerGame());

			MapView.OnExitMapView += OnExitMapView;
		}

		protected override void OnDestroy()
		{
			MapView.OnExitMapView -= OnExitMapView;
		}

		void OnExitMapView()
		{
			Close();
		}

		public void Close()
		{
			gameObject.SetActive(false);
		}

		public void Open()
		{
			openedBases.SetActive(MiscUtils.isCareerGame());
			closedBases.SetActive(MiscUtils.isCareerGame());
			gameObject.SetActive(true);
		}

        /*public override void Draw()
        {
            if (MapView.MapIsEnabled)
            {
                DrawManager();
            }
            else
            {
                this.Close();
            }
        }*/
    }
}
