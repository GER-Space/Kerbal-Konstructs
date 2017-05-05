using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;
using KSP.UI.Screens;
using System.Reflection;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.Core
{
	public class StaticObject
	{
		public Vector3 RadialPosition;

		internal CelestialBody body;
		public float StaffCurrent;

		public GameObject gameObject;
		public PQSCity pqsCity;
		internal StaticModel model;

        public UrlDir.UrlConfig configUrl;
        public String configPath;

		[KSPField]
		public Dictionary<string, object> settings = new Dictionary<string, object>();

		public Boolean editing;
		public Boolean preview;

        private Vector3 origScale;
        public bool isActive ;

		private List<Renderer> _rendererComponents; 


        /// <summary>
        /// Updates the static instance with new settings
        /// </summary>
		internal void update()
		{
			if (pqsCity != null)
			{
				pqsCity.repositionRadial = (Vector3) settings["RadialPosition"];
				pqsCity.repositionRadiusOffset = (float) settings["RadiusOffset"];
				pqsCity.reorientInitialUp = (Vector3) settings["Orientation"];
				pqsCity.reorientFinalAngle = (float) settings["RotationAngle"];
                pqsCity.transform.localScale = origScale * (float)settings["ModelScale"];
                pqsCity.Orientate();
			}
			// Notify modules about update
			foreach (StaticModule module in gameObject.GetComponents<StaticModule>())
			    module.StaticObjectUpdate();
		}

        internal object getSetting(string setting)
		{
			if (settings.ContainsKey(setting))
				return settings[setting];
			// Debug.Log("KK: Setting " + setting + " not found in instance of model " + model.config);
			object defaultValue = KKAPI.getInstanceSettings()[setting].getDefaultValue();

			if (defaultValue != null)
			{
				settings.Add(setting, defaultValue);
				return defaultValue;
			}
			else
			{
				Log.Normal("Setting " + setting + " not found in instance API. BUG BUG BUG.");
				return null;
			}
		}

        internal void setSetting(string setting, object value)
		{
			if (settings.ContainsKey(setting))
			{
				settings[setting] = value;
			}
			else
			{
				settings.Add(setting, value);
			}
		}

        internal void HighlightObject(Color highlightColor)
		{
			Renderer[] rendererList = gameObject.GetComponentsInChildren<Renderer>();
			_rendererComponents = new List<Renderer>(rendererList);

			foreach (Renderer renderer in _rendererComponents)
			{
				renderer.material.SetFloat("_RimFalloff", 1.8f);
				renderer.material.SetColor("_RimColor", highlightColor);
			}
		}

        internal void ToggleAllColliders(bool enable)
		{
			Transform[] gameObjectList = gameObject.GetComponentsInChildren<Transform>();
			
			List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.GetComponent<Collider>() != null select t.gameObject).ToList();

			foreach (GameObject gocollider in colliderList)
			{
				gocollider.GetComponent<Collider>().enabled = enable;
			}
		}

        internal float GetDistanceToObject(Vector3 vPosition)
		{
			float fDistance = 0f;
			fDistance = Vector3.Distance(gameObject.transform.position, vPosition);
			return fDistance;
		}

        internal void spawnObject(Boolean editing, Boolean bPreview)
		{
            // mangle Squads statics
            if (model.isSquad)
            {
                InstanceUtil.MangleSquadStatic(gameObject);
            }

            // Objects spawned at runtime should be active, ones spawned at loading not
            InstanceUtil.SetActiveRecursively(this,editing);
			
			Transform[] gameObjectList = gameObject.GetComponentsInChildren<Transform>();
			List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.GetComponent<Renderer>() != null select t.gameObject).ToList();

			setLayerRecursively(gameObject, 15);

			if (bPreview) this.ToggleAllColliders(false);

			this.preview = bPreview;

			if (editing) KerbalKonstructs.instance.selectObject(this, true, true, bPreview);

			float objvisibleRange = (float)getSetting("VisibilityRange");
			
			if (objvisibleRange < 1) objvisibleRange = 25000f;

            body = (CelestialBody)getSetting("CelestialBody");

            PQSCity.LODRange range = new PQSCity.LODRange
			{
				renderers = new GameObject[0],
				objects = new GameObject[0],
				visibleRange = objvisibleRange
			};

			pqsCity = gameObject.AddComponent<PQSCity>();
			pqsCity.lod = new[] { range };
			pqsCity.frameDelta = 1; //Unknown
			pqsCity.repositionToSphere = true; //enable repositioning
			pqsCity.repositionToSphereSurface = false; //Snap to surface?
			pqsCity.repositionRadial = (Vector3)getSetting("RadialPosition"); //position
			pqsCity.repositionRadiusOffset = (float)getSetting("RadiusOffset"); //height
			pqsCity.reorientInitialUp = (Vector3)getSetting("Orientation"); //orientation
			pqsCity.reorientFinalAngle = (float)getSetting("RotationAngle"); //rotation x axis
			pqsCity.reorientToSphere = true; //adjust rotations to match the direction of gravity
			gameObject.transform.parent = body.pqsController.transform;
			pqsCity.sphere = body.pqsController;
            origScale = pqsCity.transform.localScale;             // save the original scale for later use
            pqsCity.transform.localScale *= (float)getSetting("ModelScale");
            pqsCity.order = 100;
			pqsCity.modEnabled = true;
            pqsCity.OnSetup();
			pqsCity.Orientate();


            // Add them to the bodys objectlist, so they show up as anomalies
            if (bool.Parse((string)getSetting("isScanable")))
            {
                Log.Normal("Added " + gameObject.name + " to scanable Objects");
                var pqsObjectList = body.pqsSurfaceObjects.ToList();
                pqsObjectList.Add(pqsCity as PQSSurfaceObject);
                body.pqsSurfaceObjects = pqsObjectList.ToArray();
            }

            foreach (StaticModule module in model.modules)
			{
				Type moduleType = AssemblyLoader.loadedAssemblies.SelectMany(asm => asm.assembly.GetTypes()).FirstOrDefault(t => t.Namespace == module.moduleNamespace && t.Name == module.moduleClassname);
				MonoBehaviour mod = gameObject.AddComponent(moduleType) as MonoBehaviour;

				if (mod != null)
				{
					foreach (string fieldName in module.moduleFields.Keys)
					{
						FieldInfo field = mod.GetType().GetField(fieldName);
						if (field != null)
						{
							field.SetValue(mod, Convert.ChangeType(module.moduleFields[fieldName], field.FieldType));
						}
						else
						{
							Debug.Log("KK: WARNING: Field " + fieldName + " does not exist in " + module.moduleClassname);
						}
					}
				}
				else
				{
					Debug.Log("KK: WARNING: Module " + module.moduleClassname + " could not be loaded in " + gameObject.name);
				}
			}

			foreach (GameObject gorenderer in rendererList)
			{
				gorenderer.GetComponent<Renderer>().enabled = true;
			}
		}

        internal void setLayerRecursively(GameObject sGameObject, int newLayerNumber)
		{

            var transforms = gameObject.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].gameObject.GetComponent<Collider>() == null) transforms[i].gameObject.layer = newLayerNumber;
                else
                if (!transforms[i].gameObject.GetComponent<Collider>().isTrigger) transforms[i].gameObject.layer = newLayerNumber;
            }
		}


        internal void deselectObject(Boolean enableColliders)
		{
			this.editing = false;
			if (enableColliders) this.ToggleAllColliders(true);

			Color highlightColor = new Color(0, 0, 0, 0);
			this.HighlightObject(highlightColor);
		}
	}
}