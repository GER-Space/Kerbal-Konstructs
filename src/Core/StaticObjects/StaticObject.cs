using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;
using KSP.UI.Screens;
using System.Reflection;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.Core
{
	public class StaticObject
	{

        // Position
        [CFGSetting]
        internal CelestialBody CelestialBody;
        [CFGSetting]
        internal Vector3 RadialPosition = Vector3.zero;
        [CFGSetting]
        internal Vector3 Orientation;
        [CFGSetting]
        internal float RadiusOffset;
        [CFGSetting]
        internal float RotationAngle;
        [CFGSetting]
        internal bool isScanable = false;
        [CFGSetting]
        internal float ModelScale = 1f;

        // Legacy Faclility Setting
        [CFGSetting]
        internal string FacilityType = "None";

        // Calculated References
        [CFGSetting]
        internal double RefLatitude = 361f;
        [CFGSetting]
        internal double RefLongitude = 361f;

        // Visibility and Grouping
        [CFGSetting]
        internal float VisibilityRange = 25000f;
        [CFGSetting]
        internal string Group  = "Ungrouped";
        [CFGSetting]
        internal string GroupCenter = "false";




        internal GameObject gameObject;
        internal PQSCity pqsCity;
        internal StaticModel model;

        internal UrlDir.UrlConfig configUrl;
        internal String configPath;

        //internal Dictionary<string, object> settings = new Dictionary<string, object>();

        internal bool hasFacilities = false;
        internal bool hasLauchSites = false;
        internal LaunchSite launchSite;

        internal KKFacilityType facilityType = KKFacilityType.None; 
        internal List<KKFacility> myFacilities = new List<KKFacility>();


        // used for non KKFacility objects like AirRace
        internal string legacyfacilityID;


		internal Boolean editing;
        internal Boolean preview;

        private Vector3 origScale;
        internal bool isActive ;

        internal int indexInGroup = 0;

		private List<Renderer> _rendererComponents; 


        /// <summary>
        /// Updates the static instance with new settings
        /// </summary>
		internal void update()
		{
			if (pqsCity != null)
			{
				pqsCity.repositionRadial = RadialPosition;
				pqsCity.repositionRadiusOffset = RadiusOffset;
				pqsCity.reorientInitialUp = Orientation;
				pqsCity.reorientFinalAngle = RotationAngle;
                pqsCity.transform.localScale = origScale * ModelScale;
                pqsCity.Orientate();
			}
			// Notify modules about update
			foreach (StaticModule module in gameObject.GetComponents<StaticModule>())
			    module.StaticObjectUpdate();
		}

  //      internal object getSetting(string setting)
		//{
		//	if (settings.ContainsKey(setting))
		//		return settings[setting];
		//	// Debug.Log("KK: Setting " + setting + " not found in instance of model " + model.config);
		//	object defaultValue = KKAPI.getInstanceSettings()[setting].getDefaultValue();

		//	if (defaultValue != null)
		//	{
		//		settings.Add(setting, defaultValue);
		//		return defaultValue;
		//	}
		//	else
		//	{
		//		Log.Normal("Setting " + setting + " not found in instance API. BUG BUG BUG.");
		//		return null;
		//	}
		//}

  //      internal void setSetting(string setting, object value)
		//{
		//	if (settings.ContainsKey(setting))
		//	{
		//		settings[setting] = value;
		//	}
		//	else
		//	{
		//		settings.Add(setting, value);
		//	}
		//}

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


        /// <summary>
        /// Spawns a new Instance in the Gameworld and registers itself to the Static Database 
        /// </summary>
        /// <param name="editing"></param>
        /// <param name="bPreview"></param>
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

			float objvisibleRange = VisibilityRange;
			
			if (objvisibleRange < 1) objvisibleRange = 25000f;

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
			pqsCity.repositionRadial = RadialPosition; //position
			pqsCity.repositionRadiusOffset = RadiusOffset; //height
			pqsCity.reorientInitialUp = Orientation; //orientation
			pqsCity.reorientFinalAngle = RotationAngle; //rotation x axis
			pqsCity.reorientToSphere = true; //adjust rotations to match the direction of gravity
			gameObject.transform.parent = CelestialBody.pqsController.transform;
			pqsCity.sphere = CelestialBody.pqsController;
            origScale = pqsCity.transform.localScale;             // save the original scale for later use
            pqsCity.transform.localScale *= ModelScale;
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
				gorenderer.GetComponent<Renderer>().enabled = true;
			}

            StaticDatabase.AddStatic(this);

            // Add them to the bodys objectlist, so they show up as anomalies
            // After we got a new Name from StaticDatabase.AddStatic()
            if (isScanable)
            {
                Log.Normal("Added " + gameObject.name + " to scanable Objects");
                var pqsObjectList = CelestialBody.pqsSurfaceObjects.ToList();
                pqsObjectList.Add(pqsCity as PQSSurfaceObject);
                CelestialBody.pqsSurfaceObjects = pqsObjectList.ToArray();
            }


        }


        /// <summary>
        /// Sets tje Layer of the Colliders
        /// </summary>
        /// <param name="sGameObject"></param>
        /// <param name="newLayerNumber"></param>
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

        /// <summary>
        /// resets the object highlightColor to 0 and resets the editing flag.
        /// </summary>
        /// <param name="enableColliders"></param>
        internal void deselectObject(Boolean enableColliders)
		{
			this.editing = false;
			if (enableColliders) this.ToggleAllColliders(true);

			Color highlightColor = new Color(0, 0, 0, 0);
			this.HighlightObject(highlightColor);
		}
	}
}