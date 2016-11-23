using System;
using UnityEngine;


namespace KerbalKonstructs.UI
{
    public class CameraController
    {
        public FlightCamera cam;
        public bool active = false;


        public void enable(GameObject target)
        {
            cam = FlightCamera.fetch;
            if (cam)
            {
                cam.SetTargetTransform(target.transform);
                active = true;
            }
            else
            {
                Debug.LogError("KK: FlightCamera doesn't exist!");
            }
        }

        public void disable()
        {
            cam.SetTargetVessel(FlightGlobals.ActiveVessel);
            active = false;
        }
    }
}
