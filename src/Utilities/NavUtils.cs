using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using KerbalKonstructs.API;
using UnityEngine;

namespace KerbalKonstructs.Utilities
{
	public class NavUtils
	{
        /// <summary>
        /// Returns the Longitude of a Position (in Radians) 
        /// </summary>
        /// <param name="radialPosition"></param>
        /// <returns></returns>
		public static double GetLongitude(Vector3d radialPosition)
		{
			Vector3d norm = radialPosition.normalized;
			double longitude = Math.Atan2(norm.z, norm.x);
			return (!double.IsNaN(longitude) ? longitude : 0.0);
		}

        /// <summary>
        /// Returns the Lattitude of a Position (in Radians) 
        /// </summary>
        /// <param name="radialPosition"></param>
        /// <returns></returns>
        public static double GetLatitude(Vector3d radialPosition)
		{
			double latitude = Math.Asin(radialPosition.normalized.y);
			return (!double.IsNaN(latitude) ? latitude : 0.0);
		}

		public static StaticObject GetNearestFacility(Vector3 vPosition, string sFacilityType, string sGroup = "None")
		{
			StaticObject soFacility = null;

			float fLastDist = 100000000f;
			float fDistance = 0f;
			float fNearest = 0f;

			foreach (StaticObject instance in StaticDatabase.allStaticInstances)
			{
				if (sGroup != "None")
				{
					if (instance.Group != sGroup) continue;
				}

				if (instance.FacilityType == sFacilityType)
				{
					fDistance = Vector3.Distance(instance.gameObject.transform.position, vPosition);

					if (fDistance < fLastDist)
					{
						fNearest = fDistance;
						soFacility = instance;
					}

					fLastDist = fDistance;
				}
				else continue;
			}

			return soFacility;
		}

		public static Material lineMaterial1;
		public static Material lineMaterial2;
		public static Material lineMaterial3;
		public static Material lineMaterial4;
		public static Material lineMaterial5;

		public static void CreateLineMaterial(int iMat = 1)
		{
			Material mMat = lineMaterial1;
			if (iMat == 2) mMat = lineMaterial2;
			if (iMat == 3) mMat = lineMaterial3;
			if (iMat == 4) mMat = lineMaterial4;
			if (iMat == 5) mMat = lineMaterial5;

			if (mMat == null)
			{
				var shader = Shader.Find("Hidden/Internal-Colored");
				mMat = new Material(shader);
				mMat.hideFlags = HideFlags.HideAndDontSave;
				// Turn on alpha blending
				mMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				mMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
				// Turn backface culling off
				mMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
				// Turn off depth writes
				mMat.SetInt("_ZWrite", 0);

				if (iMat == 1) lineMaterial1 = mMat;
				if (iMat == 2) lineMaterial2 = mMat;
				if (iMat == 3) lineMaterial3 = mMat;
				if (iMat == 4) lineMaterial4 = mMat;
				if (iMat == 5) lineMaterial5 = mMat;
			}
		}
	}
}
