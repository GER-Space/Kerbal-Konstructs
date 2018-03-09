using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using System.Reflection;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Modules;

namespace KerbalKonstructs.Core
{
    public enum HeightReference
    {
        Unset = 0,
        Sphere = 1 ,
        Terrain = 2
    }


    public class StaticInstance
    {

        // Position
        [CFGSetting]
        public CelestialBody CelestialBody = null;
        [CFGSetting]
        public Vector3 RadialPosition = Vector3.zero;
        [CFGSetting]
        public Vector3 Orientation;
        [CFGSetting]
        public float RadiusOffset;
        [CFGSetting]
        public float RotationAngle;
        [CFGSetting]
        public bool isScanable = false;
        [CFGSetting]
        public float ModelScale = 1f;

        // Legacy Faclility Setting
        [CFGSetting]
        public string FacilityType = "None";

        // Calculated References
        [CFGSetting]
        public double RefLatitude = 361f;
        [CFGSetting]
        public double RefLongitude = 361f;

        // Visibility and Grouping
        [CFGSetting]
        public float VisibilityRange = 25000f;
        [CFGSetting]
        public string Group = "Ungrouped";
        [CFGSetting]
        public string GroupCenter = "false";
        [CFGSetting]
        public int IsRelativeToTerrain = (int)HeightReference.Unset;

        // Special Effects
        [CFGSetting]
        public Color GrasColor = Color.clear;


        public GameObject gameObject;
        public PQSCity pqsCity;
        //public PQSCity2 pqsCity2;
        internal StaticModel model;

        public UrlDir.UrlConfig configUrl;
        public String configPath;

        public bool hasFacilities
        {
            get
            {
                return (myFacilities.Count > 0);
            }
        }

        internal HeightReference heighReference
        {
            get
            {
                return (HeightReference)IsRelativeToTerrain;
            }
            set
            {
                IsRelativeToTerrain = (int)value;
            }
        }


        public bool hasLauchSites = false;
        public LaunchSite launchSite;

        public KKFacilityType facilityType = KKFacilityType.None;
        public List<KKFacility> myFacilities = new List<KKFacility>();


        // used for non KKFacility objects like AirRace
        public string legacyfacilityID;


        internal Boolean editing;
        internal Boolean preview;

        private Vector3 origScale;
        internal bool isActive;

        internal int indexInGroup = 0;

        private List<Renderer> _rendererComponents;


        /// <summary>
        /// Updates the static instance with new settings
        /// </summary>
        public void Update()
        {
            if (pqsCity != null)
            {
                pqsCity.repositionRadial = RadialPosition;
                pqsCity.repositionRadiusOffset = RadiusOffset;
                pqsCity.reorientInitialUp = Orientation;
                pqsCity.reorientFinalAngle = RotationAngle;
                pqsCity.transform.localScale = origScale * ModelScale;

                switch (heighReference)
                {
                    case HeightReference.Sphere:
                        pqsCity.repositionToSphereSurface = false; //Snap to surface?
                        pqsCity.repositionToSphereSurfaceAddHeight = false;
                        pqsCity.repositionToSphere = true;
                        break;
                    case HeightReference.Terrain:
                        pqsCity.repositionToSphereSurface = true; //Snap to surface?
                        pqsCity.repositionToSphereSurfaceAddHeight = true;
                        pqsCity.repositionToSphere = false;
                        break;
                }

                pqsCity.Orientate();
            }
            // Notify modules about update
            foreach (StaticModule module in gameObject.GetComponents<StaticModule>())
            {
                module.StaticObjectUpdate();
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
            InstanceUtil.SetActiveRecursively(this, editing);

            Transform[] gameObjectList = gameObject.GetComponentsInChildren<Transform>();
            List<GameObject> rendererList = (from t in gameObjectList where t.gameObject.GetComponent<Renderer>() != null select t.gameObject).ToList();

            InstanceUtil.SetLayerRecursively(this, 15);

            if (bPreview)
                this.ToggleAllColliders(false);

            this.preview = bPreview;

            if (editing)
                KerbalKonstructs.instance.selectObject(this, true, true, bPreview);

            float objvisibleRange = VisibilityRange;

            if (objvisibleRange < 1)
                objvisibleRange = 25000f;

            PQSCity.LODRange range = new PQSCity.LODRange
            {
                renderers = new GameObject[0],
                objects = new GameObject[0],
                visibleRange = objvisibleRange
            };

            pqsCity = gameObject.AddComponent<PQSCity>();
            pqsCity.lod = new[] { range };
            pqsCity.frameDelta = 10000; //update interval for its own visiblility range checking. unused by KK, so set this to a high value
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
            pqsCity.repositionToSphere = true; //enable repositioning


            switch (heighReference)
            {
                case HeightReference.Sphere:
                    pqsCity.repositionToSphereSurface = false; //Snap to surface?

                    break;
                case HeightReference.Terrain:

                    pqsCity.repositionToSphereSurface = true; //Snap to surface?
                    pqsCity.repositionToSphereSurfaceAddHeight = true;
                    pqsCity.repositionToSphere = false;
                    break;
                default:
                    // we try to descide which one is the best to take
                    string biome = ScienceUtil.GetExperimentBiome(CelestialBody, RefLatitude, RefLongitude);
                    float heightAboveTerrain = SDRescale.GetSurfaceRefereceHeight(this);

                    if ((biome == "Water" || biome == "Shores") && ((Math.Abs(RadiusOffset) < 5) && heightAboveTerrain > 5)) // most likely at ocean surface 
                    {
                        Log.Normal("Found a swimming object: " + this.gameObject.name);
                        pqsCity.repositionToSphereSurface = false; //Snap to surface?
                        heighReference = HeightReference.Sphere;
                    }
                    else
                    {
                        {
                            //    Log.Normal("found new Radiusffset: " + heightAboveTerrain);
                            RadiusOffset = heightAboveTerrain;
                            pqsCity.repositionToSphereSurface = true; //Snap to surface?#
                            pqsCity.repositionToSphereSurfaceAddHeight = true;
                            pqsCity.repositionRadiusOffset = heightAboveTerrain;
                            pqsCity.repositionToSphere = false;

                            heighReference = HeightReference.Terrain;
                        }
                    }
                    break;
            }


            pqsCity.OnSetup();
            pqsCity.Orientate();


            //PQSCity2.LodObject lodObject = new PQSCity2.LodObject();
            //lodObject.visibleRange = VisibilityRange;
            //lodObject.objects = new GameObject[] { };
            //pqsCity2 = gameObject.AddComponent<PQSCity2>();
            //pqsCity2.objects = new [] { lodObject } ;
            //pqsCity2.objectName = ""; 
            //pqsCity2.lat = RefLatitude;
            //pqsCity2.lon = RefLongitude;
            //pqsCity2.alt = RadiusOffset;
            //pqsCity2.up = Orientation;
            //pqsCity2.rotation = RotationAngle;
            //pqsCity2.sphere = CelestialBody.pqsController;


            //pqsCity2.OnSetup();
            //pqsCity2.Orientate();


            foreach (StaticModule module in model.modules)
            {
                Type moduleType = AssemblyLoader.loadedAssemblies.SelectMany(asm => asm.assembly.GetTypes()).FirstOrDefault(t => t.Namespace == module.moduleNamespace && t.Name == module.moduleClassname);
                StaticModule mod = gameObject.AddComponent(moduleType) as StaticModule;

                if (mod != null)
                {
                    mod.staticInstance = this;
                    foreach (string fieldName in module.moduleFields.Keys)
                    {
                        FieldInfo field = mod.GetType().GetField(fieldName);
                        if (field != null)
                        {
                            field.SetValue(mod, Convert.ChangeType(module.moduleFields[fieldName], field.FieldType));
                        }
                        else
                        {
                            Log.UserWarning("Field " + fieldName + " does not exist in " + module.moduleClassname);
                        }
                    }
                }
                else
                {
                    Log.UserError("Module " + module.moduleClassname + " could not be loaded in " + gameObject.name);
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

        internal void SaveConfig()
        {
            ConfigParser.SaveInstanceByCfg(configPath);
        }

	}
}