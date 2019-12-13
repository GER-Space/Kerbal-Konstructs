using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KerbalKonstructs.Core;
using UnityEngine;

namespace KerbalKonstructs
{
    class OnSunRiseMaster : MonoBehaviour
    {

        internal GroupCenter center;

        bool active = false;


        public void Start()
        {
            if (center == null)
            {
                return;
            }
        }


        public void LateUpdate()
        {
            if (center == null)
            {
                return;
            }
            // Don't do anything when we are not in range
            if (!center.isActive)
            {
                return;
            }

            // Vectors show in the same dirction --> Sun should be ahead of us
            if (Vector3.Dot(upVector, sunVector) > 0)
            {
                if (active)
                {
                    return;
                }
                active = true;
                Log.Normal("Activating ");
                foreach (var instance in center.childInstances)
                {
                    instance.mesh.SendMessage("OnSunriseActivate", SendMessageOptions.DontRequireReceiver);
                }
            }
            else
            {
                if (!active)
                {
                    return;
                }
                active = false;
                Log.Normal("Deactivating");
                foreach (var instance in center.childInstances)
                {
                    instance.mesh.SendMessage("OnSunriseDeActivate", SendMessageOptions.DontRequireReceiver);
                }
            }

        }


        Vector3 sunVector
        {
            get
            {
                return (Planetarium.fetch.Sun.gameObject.transform.position - center.gameObject.transform.position);
            }
        }

        Vector3 upVector
        {
            get
            {
                return (Vector3)(center.CelestialBody.GetSurfaceNVector(center.RefLatitude, center.RefLongitude));
            }
        }

    }
}
