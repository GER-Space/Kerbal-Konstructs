using System;
using System.IO;
using UnityEngine;
using KerbalKonstructs.Core;

namespace BWStatics
{
	public class AutoKourseModule: StaticModule
	{
        public string digit0Name = "digit0_obj";
        public string digit1Name = "digit1_obj";
        public string headingAdjustment = "0";

        private Transform digit0Transform ;
        private Transform digit1Transform ;

        private int heading = 360;
        private int dg0;
        private int dg1 ;

        private Renderer dg0renderer;
        private Renderer dg1renderer;

        private CelestialBody body;
        private Vector3 upVector;
        private Vector3 north;
        private Vector3 east;
        private Vector3 forward;

        private int headingAdj;

        private bool isInitialized = false;


        public void Start() {

            if (!isInitialized)
            {
                Initialize();
            }
			try {
				setTextures();
			}
			catch {
				Debug.Log ("AutoKourseModule: Start failed in setTextures()");
			}	
		}

		public override void StaticObjectUpdate() {
            if (!isInitialized)
            {
                Initialize();
            }
            setTextures ();
		}

        private void Initialize()
        {
            digit0Transform = GetTransformRecursive(digit0Name, gameObject);
            digit1Transform = GetTransformRecursive(digit1Name, gameObject);
            dg0renderer = digit0Transform.GetComponent<Renderer>();
            dg1renderer = digit1Transform.GetComponent<Renderer>();
            headingAdj = int.Parse(headingAdjustment);
        }



        private void setTextures() {

            heading = GetHeading ();
           // Log.Normal ("AutoKourseModule: heading = " + heading + " " + staticInstance.gameObject.name);

			dg0 = heading / 10 % 10;
			dg1 = heading / 100 % 10;

            //Log.Normal ("AutoKourseModule: setting course " + dg1 + dg0);
			dg0renderer.material.SetTextureOffset("_MainTex",  new Vector2(dg0 * 0.1f,0));
			dg1renderer.material.SetTextureOffset("_MainTex",  new Vector2(dg1 * 0.1f,0));
		}


		private int GetHeading() {
			body = staticInstance.CelestialBody;
			upVector = body.GetSurfaceNVector(staticInstance.RefLatitude, staticInstance.RefLongitude).normalized;
			north = Vector3.ProjectOnPlane(body.transform.up, upVector).normalized;
			east = Vector3.Cross(upVector, north).normalized;
			forward = Vector3.ProjectOnPlane(gameObject.transform.forward, upVector);

			float heading = Vector3.Angle (forward, north);

            if (Vector3.Dot (forward, east) < 0)
            {
                heading = 360 - heading;
            }

            heading = (heading + headingAdj + 360 ) % 360;
            
            if (heading % 10 > 5)
            {
                heading += 10 - heading % 10;
            }
            if (heading < 6)
            {
                heading = 360; // There are no 00 runways, they all are 36!
            }

            return (int)heading;
		}

        private Transform GetTransformRecursive(string name, GameObject gameObject)
        {
            foreach (Transform child in gameObject.transform)
            {
                if (child.Find(name) != null)
                {
                    return child.Find(name);
                }
            }
            Log.Error("Could not find Transform: " + name);
            return null;
        }

	}
}

