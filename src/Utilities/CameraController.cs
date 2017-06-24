using System;
using UnityEngine;
using KerbalKonstructs.Core;
using KerbalKonstructs;


namespace KerbalKonstructs.UI
{
    public class CameraController
    {
        public FlightCamera cam;
        public bool active = false;

        private Transform oldTarget;

        private float x = 0;
        private float y = 0;
        private float zoom = 10;

        public void enable(GameObject target)
        {

            cam = FlightCamera.fetch;
            if (cam)
            {
                active = true;
                if (KerbalKonstructs.useLegacyCamera)
                {
                    
                    InputLockManager.SetControlLock(ControlTypes.CAMERACONTROLS, "KKCamControls");

                    cam.DeactivateUpdate();
                    oldTarget = cam.transform.parent;
                    //cam.updateActive = false;
                    cam.transform.parent = target.transform;
                    cam.transform.position = target.transform.position;
                }
                else
                {
                    // new camera code
                    cam.SetTargetTransform(target.transform);
                }
            }
            else
            {
                Log.UserError("FlightCamera doesn't exist!");
            }
        }

        public void disable()
        {
            if (KerbalKonstructs.useLegacyCamera)
            {
                if (oldTarget != null)
                    cam.transform.parent = oldTarget;
                cam.ActivateUpdate();

                //for legacy control
                InputLockManager.RemoveControlLock("KKCamControls");
            }
            else
            {
                cam.SetTargetVessel(FlightGlobals.ActiveVessel);
            }
            active = false;
        }


        public void updateCamera()
        {
            bool needUpdate = false; 
            if (Input.GetMouseButton(1))
            {
                x += Input.GetAxis("Mouse X") * cam.orbitSensitivity * 50.0f;
                y -= Input.GetAxis("Mouse Y") * cam.orbitSensitivity * 50.0f;
                needUpdate = true;
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                zoom = Mathf.Clamp(zoom - Input.GetAxis("Mouse ScrollWheel") * 100.0f, cam.minDistance, cam.maxDistance);
                needUpdate = true;
            }
            if (needUpdate)
            {
                cam.transform.localRotation = Quaternion.Euler(y, x, 0);

                cam.transform.localPosition = Vector3.Slerp(cam.transform.localPosition, Quaternion.Euler(y, x, 0) * new Vector3(0.0f, 0.0f, -zoom), Time.deltaTime * cam.sharpness);
                //cam.transform.localPosition = cam.transform.localPosition + Quaternion.Euler(y, x, 0) * new Vector3(0, 0, -zoom) * Time.deltaTime * cam.sharpness;
            }
        }
    }
}
