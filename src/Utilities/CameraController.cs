using System;
using UnityEngine;
using System.Linq;
using KerbalKonstructs.Core;
using KerbalKonstructs;
using System.Reflection;


namespace KerbalKonstructs.Core
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

        /// <summary>
        /// Set the SpaceCenterCam to the location os the current LaunchSite
        /// </summary>
        /// <param name="currentSite"></param>
        internal static void SetSpaceCenterCam(KKLaunchSite currentSite)
        {
            if (KerbalKonstructs.focusLastLaunchSite && (currentSite.body.name == ConfigUtil.GetCelestialBody("HomeWorld").name))
            {
                foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                {
                    Log.Normal("Restting to: " + currentSite.LaunchSiteName);
                    scCam.transform.parent = currentSite.lsGameObject.transform;
                    scCam.transform.position = currentSite.lsGameObject.transform.position;
                    scCam.initialPositionTransformName = currentSite.lsGameObject.transform.name;
                    scCam.pqsName = currentSite.body.name;
                    scCam.rotationInitial = currentSite.InitialCameraRotation;
                    scCam.ResetCamera();
                    KerbalKonstructs.scCamWasAltered = true;
                }
            }
            else
            {
                foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                {
                    Log.Normal("Restting to KSC");
                    Upgradeables.UpgradeableObject kscRnD = Resources.FindObjectsOfTypeAll<Upgradeables.UpgradeableObject>().Where(x => x.name == "ResearchAndDevelopment").First();
                    scCam.transform.parent = kscRnD.gameObject.transform;
                    scCam.transform.position = kscRnD.gameObject.transform.transform.position;
                    scCam.initialPositionTransformName = kscRnD.gameObject.transform.name;
                    scCam.pqsName = ConfigUtil.GetCelestialBody("HomeWorld").name;
                    scCam.rotationInitial = -60;
                    scCam.ResetCamera();
                    KerbalKonstructs.scCamWasAltered = false;
                }
            }

            if (currentSite.LaunchSiteName == "Runway" || currentSite.LaunchSiteName == "LaunchPad" || currentSite.body.name != ConfigUtil.GetCelestialBody("HomeWorld").name)
            {
                foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll(typeof(SpaceCenterCamera2)))
                {
                    cam.altitudeInitial = 45f;
                    cam.ResetCamera();
                    
                }
            } else {

                    PQSCity sitePQS = currentSite.staticInstance.pqsCity;

                    foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll(typeof(SpaceCenterCamera2)))
                    {
                        if (sitePQS.repositionToSphere || sitePQS.repositionToSphereSurface)
                        {

                            double nomHeight = currentSite.body.pqsController.GetSurfaceHeight((Vector3d)sitePQS.repositionRadial.normalized) - currentSite.body.Radius;
                            if (sitePQS.repositionToSphereSurface)
                            {
                                nomHeight += sitePQS.repositionRadiusOffset;
                            }
                            cam.altitudeInitial = 0f - (float)nomHeight;
                        }
                        else
                        {
                            cam.altitudeInitial = 0f - (float)sitePQS.repositionRadiusOffset;
                        }
                        cam.ResetCamera();
                        Log.Normal("fixed the Space Center camera.");

                    }
                }
            SetNextMorningPoint(currentSite);
        }


        internal static void SetSpaceCenterCam2(KKLaunchSite currentSite)
        {
            if (KerbalKonstructs.focusLastLaunchSite)
            {
                foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                {
                    scCam.transform.parent = currentSite.lsGameObject.transform;
                    scCam.transform.position = currentSite.lsGameObject.transform.position + Vector3.up * 100;
                    scCam.initialPositionTransformName = currentSite.lsGameObject.transform.name;
                    FieldInfo pqsField = scCam.GetType().GetField("pqs", BindingFlags.Instance | BindingFlags.NonPublic);
                    pqsField.SetValue(scCam, currentSite.body.pqsController);
                    scCam.pqsName = currentSite.body.name;
                    scCam.ResetCamera();
                }

            }
            else
            {
                foreach (SpaceCenterCamera2 scCam in Resources.FindObjectsOfTypeAll<SpaceCenterCamera2>())
                {
                    scCam.transform.parent = SpaceCenter.Instance.transform;
                    scCam.transform.position = SpaceCenter.Instance.transform.position;
                    scCam.initialPositionTransformName = "KSC/SpaceCenter/SpaceCenterCameraPosition";
                    scCam.pqsName = "Kerbin";
                    scCam.ResetCamera();
                }
            }
            if (currentSite.LaunchSiteName == "Runway" || currentSite.LaunchSiteName == "LaunchPad")
            {
                foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll(typeof(SpaceCenterCamera2)))
                {
                    cam.altitudeInitial = 45f;
                    cam.ResetCamera();

                }
            }
            else
            {
                PQSCity sitePQS = currentSite.staticInstance.pqsCity;

                foreach (SpaceCenterCamera2 cam in Resources.FindObjectsOfTypeAll(typeof(SpaceCenterCamera2)))
                {
                    if (sitePQS.repositionToSphere || sitePQS.repositionToSphereSurface)
                    {
                        double nomHeight = currentSite.body.pqsController.GetSurfaceHeight((Vector3d)sitePQS.repositionRadial.normalized) - currentSite.body.Radius;
                        if (sitePQS.repositionToSphereSurface)
                        {
                            nomHeight += sitePQS.repositionRadiusOffset;
                        }
                        cam.altitudeInitial = 0f - (float)nomHeight;
                    }
                    else
                    {
                        cam.altitudeInitial = 0f - (float)sitePQS.repositionRadiusOffset;
                    }
                    cam.ResetCamera();
                    Log.Normal("fixed the Space Center camera.");
                }
            }

            SetNextMorningPoint(currentSite);
        }

        static void SetNextMorningPoint(KKLaunchSite launchSite)
        {
            double timeOfDawn = (( 0.95* (0.25 - launchSite.body.initialRotation / 360) - ((launchSite.refLon) / 360) + 1) % 1);

            KSP.UI.UIWarpToNextMorning.timeOfDawn = (timeOfDawn + 0.05);
            
            Log.Normal("Fixed the \"warp to next morning\" button: " + KSP.UI.UIWarpToNextMorning.timeOfDawn.ToString());
        }

    }
}
