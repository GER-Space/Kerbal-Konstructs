using UnityEngine;

using KerbalKonstructs.UI;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.Core
{
    public class CustomSpaceCenter : IMapIcon
    {
        public string SpaceCenterName;

        private SpaceCenter _spaceCenter;
        internal StaticInstance staticInstance;
        internal GameObject gameObject;

        internal bool isFromFacility = false;

        public static void CreateFromLaunchsite(KKLaunchSite site)
        {
            StaticInstance parentinstance = site.staticInstance;
            if (parentinstance != null)
            {
                var csc = new CustomSpaceCenter();
                csc.SpaceCenterName = site.LaunchSiteName;
                csc.staticInstance = parentinstance;
                csc.gameObject = site.staticInstance.gameObject;
                SpaceCenterManager.AddSpaceCenter(csc);
            }
            else
            {
                Log.Normal("CreateFromLaunchsite failed because staticObject is null.");
            }
        }

        public SpaceCenter spaceCenter
        {
            get
            {
                return GetSpaceCenter();
            }
        }


        public SpaceCenter GetSpaceCenter()
        {
            if (_spaceCenter == null)
            {
                _spaceCenter = gameObject.AddComponent<SpaceCenter>();
                gameObject.SetActive(true);
                _spaceCenter.cb = staticInstance.CelestialBody;
                _spaceCenter.name = SpaceCenterName;
                _spaceCenter.AreaRadius = 3000;
                _spaceCenter.spaceCenterAreaTrigger = SpaceCenterManager.KSC.spaceCenterAreaTrigger;
                _spaceCenter.enabled = true;
                _spaceCenter.Start();
            }
            return _spaceCenter;
        }

        public bool isOpen
        {
            get
            {
                if (isFromFacility)
                {
                    return staticInstance.GetFacility(Modules.KKFacilityType.RecoveryBase).isOpen;
                }
                else
                {
                    return staticInstance.launchSite.isOpen;
                }
            }
        }

		public Vector3d Position
		{
			get {
				return gameObject.transform.position;
			}
		}

		public string Tooltip
		{
			get {
				string name;
				if (isFromFacility) {
					name = staticInstance.GetFacility(KKFacilityType.RecoveryBase).FacilityName;
				} else {
					name = staticInstance.launchSite.LaunchSiteName;
				}
				var pos = staticInstance.CelestialBody.transform.InverseTransformPoint(Position);
				var lat = KKMath.GetLatitudeInDeg(pos);
				var lon = KKMath.GetLongitudeInDeg(pos);
				return $"{name}\n(Lat. {lat:F2}/ Lon. {lon:F2})";
			}
		}

		public Sprite Icon
		{
			get {
				return UIMain.iconRecoveryBase;
			}
		}

		public bool IsOccluded
		{
			get {
				return MapIcon.IsOccluded(Position, staticInstance.CelestialBody);
			}
		}

		public bool IsHidden
		{
			get {
				bool hidden = false;
				hidden |= !KerbalKonstructs.instance.mapShowRecovery;
				hidden |= (staticInstance.groupCenter?.isHidden is bool h) && h && !isOpen;
				if (MiscUtils.isCareerGame()) {
					hidden |= (!isOpen && !KerbalKonstructs.instance.mapShowClosed);
					hidden |= (isOpen && !KerbalKonstructs.instance.mapShowOpen);
				}
				return hidden;
			}
		}

		public MapIcon MapIcon { get; set; }

		public void OnClick()
		{
			MapIconSelector.Close();
			if (isFromFacility) {
				MapIconSelector.useLaunchSite = false;
				MapIconSelector.staticInstance = staticInstance;
			} else {
				MapIconSelector.selectedSite = staticInstance.launchSite;
				MapIconSelector.useLaunchSite = true;
			}
			MapIconSelector.Open();
		}
    }
}
