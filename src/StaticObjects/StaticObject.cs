using System;
using System.Linq;
using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;
using System.Reflection;

namespace KerbalKonstructs.StaticObjects
{
	public class StaticObject
	{
		[PersistentKey]
		public Vector3 RadialPosition;

		[PersistentField]
		public CelestialBody CelestialBody;
		[PersistentField]
		public float StaffCurrent;

		public GameObject gameObject;
		public PQSCity pqsCity;
		public StaticModel model;

		[KSPField]
		public Dictionary<string, object> settings = new Dictionary<string, object>();

		public Boolean editing;
		public Boolean preview;

		private List<Renderer> _rendererComponents; 

		public void update()
		{
			if (pqsCity != null)
			{
				pqsCity.repositionRadial = (Vector3) settings["RadialPosition"];
				pqsCity.repositionRadiusOffset = (float) settings["RadiusOffset"];
				pqsCity.reorientInitialUp = (Vector3) settings["Orientation"];
				pqsCity.reorientFinalAngle = (float) settings["RotationAngle"];
				pqsCity.Orientate();
			}
		}

		public object getSetting(string setting)
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
				if (KerbalKonstructs.instance.DebugMode) Debug.Log("KK: Setting " + setting + " not found in instance API. BUG BUG BUG.");
				
				return null;
			}
		}

		public void setSetting(string setting, object value)
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

		public void HighlightObject(Color highlightColor)
		{
			Renderer[] rendererList = gameObject.GetComponentsInChildren<Renderer>();
			_rendererComponents = new List<Renderer>(rendererList);

			foreach (Renderer renderer in _rendererComponents)
			{
				renderer.material.SetFloat("_RimFalloff", 1.8f);
				renderer.material.SetColor("_RimColor", highlightColor);
			}
		}

		public void ToggleAllColliders(bool enable)
		{
			Transform[] gameObjectList = gameObject.GetComponentsInChildren<Transform>();
			
			List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.collider != null select t.gameObject).ToList();

			foreach (GameObject gocollider in colliderList)
			{
				gocollider.collider.enabled = enable;
			}
		}

		public float GetDistanceToObject(Vector3 vPosition)
		{
			float fDistance = 0f;
			fDistance = Vector3.Distance(gameObject.transform.position, vPosition);
			return fDistance;
		}

		public void spawnObject(Boolean editing, Boolean bPreview)
		{
			// Objects spawned at runtime should be active, ones spawned at loading not
			SetActiveRecursively(gameObject, editing);
			
			Transform[] gameObjectList = gameObject.GetComponentsInChildren<Transform>();
			List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.renderer != null select t.gameObject).ToList();

			setLayerRecursively(gameObject, 15);

			if (bPreview) this.ToggleAllColliders(false);

			this.preview = bPreview;

			if (editing) KerbalKonstructs.instance.selectObject(this, true, true, bPreview);

			float objvisibleRange = (float)getSetting("VisibilityRange");
			
			if (objvisibleRange < 1) objvisibleRange = 25000f;

			PQSCity.LODRange range = new PQSCity.LODRange
			{
				renderers = rendererList.ToArray(),
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
			gameObject.transform.parent = ((CelestialBody)getSetting("CelestialBody")).pqsController.transform;
			pqsCity.sphere = ((CelestialBody)getSetting("CelestialBody")).pqsController;
			pqsCity.order = 100;
			pqsCity.modEnabled = true;
			pqsCity.OnSetup();
			pqsCity.Orientate();

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
				gorenderer.renderer.enabled = true;
			}
		}

		public void setLayerRecursively(GameObject sGameObject, int newLayerNumber)
		{
			if ((sGameObject.collider != null && sGameObject.collider.enabled && !sGameObject.collider.isTrigger) || sGameObject.collider == null)
			{
				sGameObject.layer = newLayerNumber;
			}

			foreach (Transform child in sGameObject.transform)
			{
				setLayerRecursively(child.gameObject, newLayerNumber);
			}
		}

		public void SetActiveRecursively(GameObject rootObject, bool active)
		{
			rootObject.SetActive(active);

			foreach (Transform childTransform in rootObject.transform)
			{
				SetActiveRecursively(childTransform.gameObject, active);
			}
		}
	}
}