using System.Linq;
using UnityEngine;


namespace KerbalKonstructs.Core
{
    public class CameraController
    {
        private static Camera nearCam = null;

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

        /// <summary>
        /// Set the SpaceCenterCam to the location os the current LaunchSite
        /// </summary>
        /// <param name="currentSite"></param>
        public static void SetSpaceCenterCam(KKLaunchSite currentSite)
        {

            if (KerbalKonstructs.focusLastLaunchSite && (currentSite.body.name == ConfigUtil.GetCelestialBody("HomeWorld").name))
            //if (KerbalKonstructs.focusLastLaunchSite && (currentSite.body == FlightGlobals.currentMainBody))
            {
                float nomHeight = 45f - (float)currentSite.body.GetAltitude(currentSite.staticInstance.transform.position);
                //KerbalKonstructs.instance.FuckUpKSP();

                foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                {
                    Log.Normal("Resetting to: " + currentSite.LaunchSiteName);
                    scCam.transform.parent = currentSite.staticInstance.transform;
                    scCam.transform.position = currentSite.staticInstance.transform.position;
                    scCam.initialPositionTransformName = currentSite.staticInstance.transform.name;
                    scCam.pqsName = currentSite.body.name;
                    scCam.rotationInitial = currentSite.InitialCameraRotation;
                    scCam.altitudeInitial = nomHeight;
                    scCam.ResetCamera();
                    KerbalKonstructs.scCamWasAltered = true;
                }
            }
            else
            {
                foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                {

                    Log.Normal("Resetting to KSC");
                    Upgradeables.UpgradeableObject kscRnD = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>().Where(x => x.name == "FlagPole").First();
                    float nomHeight = 45f - (float)ConfigUtil.GetCelestialBody("HomeWorld").GetAltitude(kscRnD.gameObject.transform.position);
                    scCam.transform.parent = kscRnD.gameObject.transform;
                    scCam.transform.position = kscRnD.gameObject.transform.transform.position;
                    scCam.initialPositionTransformName = kscRnD.gameObject.transform.name;
                    scCam.pqsName = ConfigUtil.GetCelestialBody("HomeWorld").name;
                    scCam.altitudeInitial = nomHeight;
                    scCam.rotationInitial = -60;
                    scCam.ResetCamera();
                    KerbalKonstructs.scCamWasAltered = false;
                }
            }

            SetNextMorningPoint(currentSite);

            TuneSCCam();

        }

        internal static void TuneSCCam()
        {
            if (nearCam != null)
            {
                nearCam.nearClipPlane = 4f;
            }
            else
            {
                foreach (Camera cam in Resources.FindObjectsOfTypeAll<Camera>())
                {
                    if (cam.enabled)
                    {

                        if (cam.nearClipPlane < 1 && cam.farClipPlane > 200 && cam.nearClipPlane > 0.15 && cam.farClipPlane < 900)
                        {
                            nearCam = cam;
                            cam.nearClipPlane = 15f;
                        }
                    }
                }
            }
        }

        internal static void ResetNearCam()
        {
            if (nearCam != null)
            {
                nearCam.nearClipPlane = 0.21f;
            }
        }



        static void SetNextMorningPoint(KKLaunchSite launchSite)
        {
            double timeOfDawn = ((0.95 * (0.25 - launchSite.body.initialRotation / 360) - ((launchSite.refLon) / 360) + 1) % 1);

            KSP.UI.UIWarpToNextMorning.timeOfDawn = (timeOfDawn + 0.05);

            Log.Normal("Fixed the \"warp to next morning\" button: " + KSP.UI.UIWarpToNextMorning.timeOfDawn.ToString());
        }


    }
}
