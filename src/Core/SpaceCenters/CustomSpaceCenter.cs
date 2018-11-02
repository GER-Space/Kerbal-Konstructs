using KerbalKonstructs.Core;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    public class CustomSpaceCenter
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
                csc.gameObject = site.lsGameObject;
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


    }
}