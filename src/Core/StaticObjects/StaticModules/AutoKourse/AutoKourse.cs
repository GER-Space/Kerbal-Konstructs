using System;
using System.IO;
using UnityEngine;
using KerbalKonstructs.Core;

namespace BWStatics
{
	public class AutoKourseModule: StaticModule
	{
		public AutoKourseModule ()
		{
		}

		public void Awake() {
			
			try {
				setTextures();
			}
			catch {
				Debug.Log ("AutoKourseModule: Awake failed in setTextures()");
			}	
		}

		public override void StaticObjectUpdate() {
			setTextures ();
		}

		private void setTextures() {

			int heading = getHeading ();
			if (heading % 10 > 5)
				heading += 10 - heading % 10;
				
			Debug.Log ("AutoKourseModule: heading = " + heading);
			int dg0 = heading / 10 % 10;
			int dg1 = heading / 100 % 10;
			Transform digit0 = gameObject.transform.GetChild(0).FindChild("digit0_obj");
			Transform digit1 = gameObject.transform.GetChild(0).FindChild("digit1_obj");

			Renderer dg0renderer = digit0.GetComponent<Renderer>();
			Renderer dg1renderer = digit1.GetComponent<Renderer>();

			Debug.Log ("AutoKourseModule: setting course " + dg1 + dg0);
			dg0renderer.material.SetTextureOffset("_MainTex",  new Vector2(dg0 * 0.1f,0));
			dg1renderer.material.SetTextureOffset("_MainTex",  new Vector2(dg1 * 0.1f,0));
		}

		private int getHeading() {
			
			CelestialBody body = FlightGlobals.ActiveVessel.mainBody;
			Vector3 upVector = body.GetSurfaceNVector(
				FlightGlobals.ActiveVessel.latitude, FlightGlobals.ActiveVessel.longitude).normalized;
			Vector3 north = Vector3.ProjectOnPlane(body.transform.up, upVector).normalized;
			Vector3 east = Vector3.Cross(upVector, north).normalized;
			Vector3 forward = Vector3.ProjectOnPlane(gameObject.transform.forward, upVector);
			float heading = Vector3.Angle (forward, north);
			if (Vector3.Dot (forward, east) < 0)
				heading = 360 - heading;
			return (int)heading;
		}
	}
}

