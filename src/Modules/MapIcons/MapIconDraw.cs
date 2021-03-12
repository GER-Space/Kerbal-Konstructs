using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using KerbalKonstructs.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace KerbalKonstructs.Modules
{
    class MapIconDraw
    {
        private static MapIconDraw _instance = null;
        internal static MapIconDraw instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MapIconDraw();

                }
                return _instance;
            }
        }

        internal static StateButton.State mapHideIconsBehindBody = new StateButton.State(true);

        private static List<StaticInstance> groundStations;
        private static KKLaunchSite[] launchSites;
        private static CustomSpaceCenter[] spaceCenters;

		public void Close()
		{
			KerbalKonstructs.instance.mapShowOpen.onStateChanged.RemoveListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowClosed.onStateChanged.RemoveListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowGroundStation.onStateChanged.RemoveListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowHelipads.onStateChanged.RemoveListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowRunways.onStateChanged.RemoveListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowRocketbases.onStateChanged.RemoveListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowWaterLaunch.onStateChanged.RemoveListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowOther.onStateChanged.RemoveListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowRecovery.onStateChanged.RemoveListener(ButtonStateChanged);
			mapHideIconsBehindBody.onStateChanged.RemoveListener(ButtonStateChanged);
			UpdateIcons();
		}

		void ButtonStateChanged(StateButton.State state)
		{
			Debug.Log("[MapIconDraw] ButtonStateChanged {state.state}");
			UpdateIcons();
		}

        public void Open()
        {
			KerbalKonstructs.instance.mapShowOpen.onStateChanged.AddListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowClosed.onStateChanged.AddListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowGroundStation.onStateChanged.AddListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowHelipads.onStateChanged.AddListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowRunways.onStateChanged.AddListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowRocketbases.onStateChanged.AddListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowWaterLaunch.onStateChanged.AddListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowOther.onStateChanged.AddListener(ButtonStateChanged);
			KerbalKonstructs.instance.mapShowRecovery.onStateChanged.AddListener(ButtonStateChanged);
			mapHideIconsBehindBody.onStateChanged.AddListener(ButtonStateChanged);
            CacheGroundStations();
            CacheObjects();
			UpdateIcons();
        }

        /// <summary>
        /// Generates a list of available groundstations and saves this in the this.groundStations
        /// </summary>
        private void CacheGroundStations()
        {
            groundStations = new List<StaticInstance>(15);

            // Test for later
            //var bla = Resources.FindObjectsOfTypeAll<GroundStation>();

            foreach (StaticInstance instance in StaticDatabase.allStaticInstances)
            {
                if (instance.facilityType != KKFacilityType.GroundStation && instance.facilityType != KKFacilityType.TrackingStation)
                {
                    continue;
                }

                if (instance.Group == "KSCUpgrades")
                    continue;

                if (((GroundStation)instance.myFacilities[0]).TrackingShort == 0f)
                    continue;

                groundStations.Add(instance);
            }
        }

        /// <summary>
        /// Caches the launchsites for later use
        /// </summary>
        private void CacheObjects()
        {
            launchSites = LaunchSiteManager.allLaunchSites;
            spaceCenters = SpaceCenterManager.spaceCenters.ToArray();

        }

		void UpdateIcons()
		{
			//bool isOpen = KerbalKonstructs.instance.toggleIconsWithBB && MapView.MapIsEnabled;
			bool isOpen = MapView.MapIsEnabled;

            for (int i = 0; i < groundStations.Count; i++) {
                var groundStation = groundStations[i];
				if (groundStation.MapIcon == null) {
					groundStation.MapIcon = MapIcon.Create(groundStation);
				}
				groundStation.MapIcon.UpdateActive(isOpen);
            }

            for (int index = 0; index < launchSites.Length; index++) {
                var launchSite = launchSites[index];
				if (launchSite.MapIcon == null) {
					launchSite.MapIcon = MapIcon.Create(launchSite);
				}
				launchSite.MapIcon.UpdateActive(isOpen);
            }

            foreach (CustomSpaceCenter customSpaceCenter in spaceCenters) {
				if (customSpaceCenter.MapIcon == null) {
					customSpaceCenter.MapIcon = MapIcon.Create(customSpaceCenter);
				}
				customSpaceCenter.MapIcon.UpdateActive(isOpen);
            }
        }

        private bool IsOccluded(Vector3d loc, CelestialBody body)
        {
            Vector3d camPos = ScaledSpace.ScaledToLocalSpace(PlanetariumCamera.Camera.transform.position);

            if (Vector3d.Angle(camPos - loc, body.position - loc) > 90)
                return false;

            return true;
        }

        public void DisplayMapIconToolTip(string sitename, Vector3 pos)
        {
            GUI.Label(new Rect((float)(pos.x) + 16, (float)(Screen.height - pos.y) - 8, 210, 25), sitename);
        }
    }

}
