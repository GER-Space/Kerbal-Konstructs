using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using KerbalKonstructs.API;
using UnityEngine;
using KSP.UI.Screens;
using System.Reflection;
using KerbalKonstructs;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.LaunchSites;
using KerbalKonstructs.API.Config;

namespace KerbalKonstructs.StaticObjects
{
	public class StaticModelInstance
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


/*
        // new configuration system
        // Static opjects don't get inheritanced
        private static bool isInitialized = false;
        private static Dictionary<string, Type> configTypes = null;

        // Position
        internal Vector3 Orientation = Vector3.zero;
        internal float RadiusOffset = 0f;
        internal float RotationAngle = 0f;
        // Calculated References - do not set, it will not work
        internal float RefLatitude = 0f;
        internal float RefLongitude = 0f;

        // Visibility and Grouping
        internal float VisibilityRange = 25000f;
        internal string Group = "Ungrouped";
        internal string GroupCenter = "false";
        internal Vector3 RefCenter = Vector3.zero;

        // Launchsite
        internal string LaunchSiteName = "";
        internal string LaunchPadTransform = "";
        internal string LaunchSiteAuthor = "";
        internal string LaunchSiteDescription = "No description available.";
        internal string LaunchSiteLogo = "";
        internal string LaunchSiteIcon = "";
        internal SiteType LaunchSiteType = SiteType.Any;
        internal string Category = "Other";
        internal float LaunchSiteLength = 0f;
        internal float LaunchSiteWidth = 0f;
        internal string LaunchSiteNation = "";

        // Career Mode Strategy Instances
        internal float OpenCost = 0f;
        internal float CloseValue = 0f;
        internal string OpenCloseState = "Closed";
        internal string FavouriteSite = "No";
        internal float MissionCount = 0f;
        internal string MissionLog = "No missions logged";

        // Facility Types
        internal string FacilityType = "None";
        internal float FacilityLengthUsed = 0f;
        internal float FacilityWidthUsed = 0f;
        internal float FacilityHeightUsed = 0f;
        internal float FacilityMassUsed = 0f;
        internal string InStorage = "";


        

        void test()
        {

            // Facility Ratings

            // Tracking station max short range in m
            KKAPI.addInstanceSetting("TrackingShort", new ConfigFloat());
            // Max tracking angle
            KKAPI.addInstanceSetting("TrackingAngle", new ConfigFloat());

            // Target Type and ID
            KKAPI.addInstanceSetting("TargetType", new ConfigGenericString());
            KKAPI.addInstanceSetting("TargetID", new ConfigGenericString());

            //XP
            KKAPI.addInstanceSetting("FacilityXP", new ConfigFloat());

            // Staff
            KKAPI.addInstanceSetting("StaffMax", new ConfigFloat());
            KKAPI.addInstanceSetting("StaffCurrent", new ConfigFloat());

            // Fueling
            KKAPI.addInstanceSetting("LqFCurrent", new ConfigFloat());
            KKAPI.addInstanceSetting("OxFCurrent", new ConfigFloat());
            KKAPI.addInstanceSetting("MoFCurrent", new ConfigFloat());

            KKAPI.addInstanceSetting("LqFAlt", new ConfigGenericString());
            KKAPI.addInstanceSetting("OxFAlt", new ConfigGenericString());
            KKAPI.addInstanceSetting("MoFAlt", new ConfigGenericString());

            KKAPI.addInstanceSetting("ECCurrent", new ConfigFloat());

            // Industry
            KKAPI.addInstanceSetting("ProductionRateMax", new ConfigFloat());
            KKAPI.addInstanceSetting("ProductionRateCurrent", new ConfigFloat());
            KKAPI.addInstanceSetting("Producing", new ConfigGenericString());

            KKAPI.addInstanceSetting("OreCurrent", new ConfigFloat());
            KKAPI.addInstanceSetting("PrOreCurrent", new ConfigFloat());

            // Science Rep Funds generation
            KKAPI.addInstanceSetting("ScienceOMax", new ConfigFloat());
            KKAPI.addInstanceSetting("ScienceOCurrent", new ConfigFloat());
            KKAPI.addInstanceSetting("RepOMax", new ConfigFloat());
            KKAPI.addInstanceSetting("RepOCurrent", new ConfigFloat());
            KKAPI.addInstanceSetting("FundsOMax", new ConfigFloat());
            KKAPI.addInstanceSetting("FundsOCurrent", new ConfigFloat());

            // Local to a specific save - constructed in a specific save-game
            // WIP for founding
            ConfigGenericString LocalToSave = new ConfigGenericString();
            LocalToSave.setDefaultValue("False");
            KKAPI.addInstanceSetting("LocalToSave", LocalToSave);

            // Custom instances - added or modified by player with the editor
            ConfigGenericString CustomInstance = new ConfigGenericString();
            CustomInstance.setDefaultValue("False");
            KKAPI.addInstanceSetting("CustomInstance", CustomInstance);

            // Launch and Recovery
            ConfigFloat flaunchrefund = new ConfigFloat();
            flaunchrefund.setDefaultValue(0f);
            KKAPI.addInstanceSetting("LaunchRefund", flaunchrefund);
            ConfigFloat frecoveryfactor = new ConfigFloat();
          //  frecoveryfactor.setDefaultValue((float)KerbalKonstructs.instance.defaultRecoveryFactor);
            KKAPI.addInstanceSetting("RecoveryFactor", frecoveryfactor);
            ConfigFloat frecoveryrange = new ConfigFloat();
            //  frecoveryrange.setDefaultValue((float)KerbalKonstructs.instance.defaultEffectiveRange);
            KKAPI.addInstanceSetting("RecoveryRange", frecoveryrange);

            // Activity logging
            KKAPI.addInstanceSetting("LastCheck", new ConfigFloat());

        }

        // end new config system

        */


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
				Log.Debug("Setting " + setting + " not found in instance API. BUG BUG BUG.");
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
			
			List<GameObject> colliderList = (from t in gameObjectList where t.gameObject.GetComponent<Collider>() != null select t.gameObject).ToList();

			foreach (GameObject gocollider in colliderList)
			{
				gocollider.GetComponent<Collider>().enabled = enable;
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
			List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.GetComponent<Renderer>() != null select t.gameObject).ToList();

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
				gorenderer.GetComponent<Renderer>().enabled = true;
			}
		}

		public void setLayerRecursively(GameObject sGameObject, int newLayerNumber)
		{
			if (sGameObject.GetComponent<Collider>() == null) sGameObject.layer = newLayerNumber;
			else
				if (!sGameObject.GetComponent<Collider>().isTrigger) sGameObject.layer = newLayerNumber;


			/* if ((sGameObject.GetComponent<Collider>() != null && sGameObject.GetComponent<Collider>().enabled && !sGameObject.GetComponent<Collider>().isTrigger) || sGameObject.GetComponent<Collider>() == null)
			{
				sGameObject.layer = newLayerNumber;
			} */

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

		public void deselectObject(Boolean enableColliders)
		{
			this.editing = false;
			if (enableColliders) this.ToggleAllColliders(true);

			Color highlightColor = new Color(0, 0, 0, 0);
			this.HighlightObject(highlightColor);
		}
	}
}