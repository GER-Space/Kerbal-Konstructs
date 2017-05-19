using KerbalKonstructs.Core;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    public class CustomSpaceCenter
    {
        public string SpaceCenterName;

        private SpaceCenter _spaceCenter;
        internal StaticInstance staticObject;
        internal GameObject gameObject;

        public static void CreateFromLaunchsite(LaunchSite site)
        {
            StaticInstance staticObject = site.parentInstance;
            if (staticObject != null)
            {
                var csc = new CustomSpaceCenter();
                csc.SpaceCenterName = site.LaunchSiteName;
                csc.staticObject = staticObject;
                csc.gameObject = site.parentInstance.gameObject;
                SpaceCenterManager.addSpaceCenter(csc);
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
                return getSpaceCenter();
            }
        }


        public SpaceCenter getSpaceCenter()
        {
            if (_spaceCenter == null)
            {
                _spaceCenter = gameObject.AddComponent<SpaceCenter>();
                _spaceCenter.cb = staticObject.CelestialBody;
                _spaceCenter.name = SpaceCenterName;

                // Debug.Log("KK: getSpaceCenter set spaceCenter.name to " + SpaceCenterName);

                FieldInfo Latitude = _spaceCenter.GetType().GetField("latitude", BindingFlags.NonPublic | BindingFlags.Instance);
                Latitude.SetValue(_spaceCenter, _spaceCenter.cb.GetLatitude(gameObject.transform.position));
                FieldInfo Longitude = _spaceCenter.GetType().GetField("longitude", BindingFlags.NonPublic | BindingFlags.Instance);
                Longitude.SetValue(_spaceCenter, _spaceCenter.cb.GetLongitude(gameObject.transform.position));
                FieldInfo SrfNVector = _spaceCenter.GetType().GetField("srfNVector", BindingFlags.NonPublic | BindingFlags.Instance);
                SrfNVector.SetValue(_spaceCenter, _spaceCenter.cb.GetRelSurfaceNVector(_spaceCenter.Latitude, _spaceCenter.Longitude));

            }
            else
            {
                // Debug.Log("KK: getSpaceCenter was not null.");
            }

            return _spaceCenter;
        }

    }
}