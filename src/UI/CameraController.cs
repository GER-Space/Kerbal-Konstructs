using System;
using UnityEngine;

namespace KerbalKonstructs.UI
{
	public class CameraController
	{

		public FlightCamera cam;
		public Transform oldTarget;
		public Boolean active = false;

		private float x = 0; 
		private float y = 0;
		private float zoom = 10;

		public void enable(GameObject targ)
		{
			cam = FlightCamera.fetch;
			if (cam)
			{
				cam.DeactivateUpdate();
				oldTarget = cam.transform.parent;
				cam.transform.parent = targ.transform;
				active = true;
			}
			else
			{
				Debug.LogError("FlightCamera doesn't exist!");
			}
		}

		public void disable()
		{
			cam.ActivateUpdate();
			cam.transform.parent = oldTarget;
			active = false;
		}

		//TODO: Make this less shaky
		public void updateCamera()
		{
			if (Input.GetMouseButton(1))
			{
				x += Input.GetAxis("Mouse X") * cam.orbitSensitivity * 50.0f;
				y -= Input.GetAxis("Mouse Y") * cam.orbitSensitivity * 50.0f;
			}

			if (Input.GetAxis("Mouse ScrollWheel") != 0)
			{
				// ASH 08112014 Make zoom faster
				zoom = Mathf.Clamp(zoom - Input.GetAxis("Mouse ScrollWheel") * 200.0f, cam.minDistance, cam.maxDistance);
			}

			cam.transform.localRotation = Quaternion.Euler(y, x, 0);
			cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, Quaternion.Euler(y, x, 0) * new Vector3(0.0f, 0.0f, -zoom), Time.deltaTime * cam.sharpness);
		}
	}
}
