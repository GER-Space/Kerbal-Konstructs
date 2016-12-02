using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    class KKMath
    {

        internal static double rad2deg { get { return  180/Math.PI ;  } }
        internal static double deg2rad { get { return  Math.PI/180 ; } }

        /// <summary>
        /// Returns a Vector3d from the body center to surface with the magnitude (body.radius + altitude)
        /// This is done by converting the spheric coorinates to cubic and multiplying with the radius.
        /// </summary>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <param name="body"></param>
        /// <param name="alt"></param>
        /// <returns></returns>
        internal static Vector3d Spheric2Cubic(double lat, double lon, CelestialBody body, double alt=0d)
        {
            double n = body.Radius;
            double rlon = Mathf.Deg2Rad * lon;
            double rlat = Mathf.Deg2Rad * lat;

            Vector3d rad = new Vector3d(Math.Cos(rlat) * Math.Cos(rlon), Math.Sin(rlat), Math.Cos(rlat) * Math.Sin(rlon));
            return rad* n;
        }

        /// <summary>
        /// simple vector rotation. Best used with 90° seperated vectors
        /// </summary>
        /// <param name="vec_from"></param>
        /// <param name="vec_to"></param>
        /// <param name="deg"></param>
        /// <returns></returns>
        internal static Vector3d RotateVector(Vector3d vec_from, Vector3d vec_to, double deg)
        {
            double deginrad = deg2rad * deg;
            return ((Math.Cos(deginrad) * vec_from) + (Math.Sin(deginrad) * vec_to));
        }

    }
}
