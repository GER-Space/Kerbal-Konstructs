using KerbalKonstructs.Core;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    public class CustomSpaceCenter
    {
        public string SpaceCenterName;

        internal SpaceCenter spaceCenter;
        internal StaticObject staticObject;
        internal GameObject gameObject;

        public static void CreateFromLaunchsite(string name, GameObject go)
        {
            StaticObject staticObject = InstanceUtil.GetStaticInstanceForGameObject(go);
            if (staticObject != null)
            {
                var csc = new CustomSpaceCenter();
                csc.SpaceCenterName = name;
                csc.staticObject = staticObject;
                csc.gameObject = go;
                SpaceCenterManager.addSpaceCenter(csc);
            }
            else
            {
                Log.Normal("CreateFromLaunchsite failed because staticObject is null.");
            }
        }

        public SpaceCenter getSpaceCenter()
        {
            if (spaceCenter == null)
            {
                spaceCenter = gameObject.AddComponent<SpaceCenter>();
                spaceCenter.cb = staticObject.CelestialBody;
                spaceCenter.name = SpaceCenterName;

                // Debug.Log("KK: getSpaceCenter set spaceCenter.name to " + SpaceCenterName);

                FieldInfo Latitude = spaceCenter.GetType().GetField("latitude", BindingFlags.NonPublic | BindingFlags.Instance);
                Latitude.SetValue(spaceCenter, spaceCenter.cb.GetLatitude(gameObject.transform.position));
                FieldInfo Longitude = spaceCenter.GetType().GetField("longitude", BindingFlags.NonPublic | BindingFlags.Instance);
                Longitude.SetValue(spaceCenter, spaceCenter.cb.GetLongitude(gameObject.transform.position));
                FieldInfo SrfNVector = spaceCenter.GetType().GetField("srfNVector", BindingFlags.NonPublic | BindingFlags.Instance);
                SrfNVector.SetValue(spaceCenter, spaceCenter.cb.GetRelSurfaceNVector(spaceCenter.Latitude, spaceCenter.Longitude));

            }
            else
            {
                // Debug.Log("KK: getSpaceCenter was not null.");
            }

            return spaceCenter;
        }

    }
}