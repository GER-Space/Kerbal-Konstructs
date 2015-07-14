using System;
using UnityEngine;

namespace KerbalKonstructs
{
	class Util
	{
		public static CelestialBody getCelestialBody(String name)
		{
			CelestialBody[] bodies = GameObject.FindObjectsOfType(typeof(CelestialBody)) as CelestialBody[];
			foreach (CelestialBody body in bodies)
			{
				if (body.bodyName == name)
					return body;
			}
			Debug.Log("Couldn't find body \"" + name + "\"");
			return null;
		}
	}
}
