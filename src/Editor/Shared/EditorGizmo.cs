using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalKonstructs.Core;
using KerbalKonstructs;

namespace KerbalKonstructs.UI
{
    class EditorGizmo
    {
        internal static bool camInitialized = false;
        internal static GameObject KKCamObject;
        internal static Camera KKCam;

        internal static EditorGizmos.GizmoOffset moveGizmo;

        internal static GameObject targetObject; 


        internal static void SetupCam()
        {

            if (!camInitialized)
            {
                KKCamObject = new GameObject();
                KKCam = KKCamObject.AddComponent<Camera>();
                if (KKCam == null)
                {
                    Log.UserError("Cam Setup failed");
                    camInitialized = true;
                    return;
                }

                Log.UserError("Cam created");
                KKCam.cullingMask = (1 << 11);
                KKCam.clearFlags = CameraClearFlags.Depth;
                KKCam.depth = 100;
                KKCam.farClipPlane = 250000;

                KKCamObject.transform.position = FlightCamera.fetch.gameObject.transform.position;
                KKCamObject.transform.rotation = FlightCamera.fetch.gameObject.transform.rotation;
                KKCamObject.transform.parent = FlightCamera.fetch.gameObject.transform;

                List<Camera> cams = FlightCamera.fetch.cameras.ToList();
                cams.Add(KKCam);
                FlightCamera.fetch.cameras = cams.ToArray();

                camInitialized = true;
            }
        }

        internal static void SetupMoveGizmo(GameObject target, Quaternion sourceRotation, Callback<Vector3> OnMoveCB, Callback<Vector3> WhenMovedCB)
        {
            if (moveGizmo != null)
            {
                moveGizmo.Detach();
                moveGizmo = null;
            }

            targetObject = target;

            moveGizmo = EditorGizmos.GizmoOffset.Attach(targetObject.transform, sourceRotation, OnMoveCB, WhenMovedCB, FlightCamera.fetch.mainCamera);
            moveGizmo.SetCoordSystem(Space.Self);

            var transforms = moveGizmo.gameObject.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                // don't set trigger collider 
                if ((transforms[i].gameObject.GetComponent<Collider>() != null) && (transforms[i].gameObject.GetComponent<Collider>().isTrigger))
                {
                    continue;
                }
                transforms[i].gameObject.layer = 11;
            }
        }

        internal static void CloseGizmo()
        {
            if (moveGizmo != null)
            {
                moveGizmo.Detach();
                moveGizmo = null;
                targetObject = null;
            }
        }


    }
}
