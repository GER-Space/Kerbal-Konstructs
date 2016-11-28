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
            lat = (lat-90) * deg2rad;
            lon *= deg2rad;
            double n = body.Radius;
            double x, y, z;
            x = (n + alt) * -1.0 * Math.Sin(lat) * Math.Cos(lon);
            y = (n + alt) * Math.Cos(lat); // for now, it's still a sphere, so no eccentricity
            z = (n + alt) * -1.0 * Math.Sin(lat) * Math.Sin(lon);
            return new Vector3d((float)x, (float)y, (float)z);
        }
    }
}
