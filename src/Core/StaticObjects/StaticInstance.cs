using KerbalKonstructs.Modules;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    public enum HeightReference
    {
        Unset = 0,
        Sphere = 1,
        Terrain = 2
    }

    public class StaticInstance
    {
        // UUID for later identification
        [CFGSetting]
        public string UUID;

        // Position
        [CFGSetting]
        public CelestialBody CelestialBody = null;
        [CFGSetting]
        public Vector3 RadialPosition = Vector3.zero;
        [CFGSetting]
        public Vector3 RelativePosition = Vector3.zero;
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

        internal float ModelScaleSave = -1f;

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
        public float VisibilityRange = KerbalKonstructs.localGroupRange;
        [CFGSetting]
        public string Group = "Ungrouped";
        [CFGSetting]
        public int IsRelativeToTerrain = (int)HeightReference.Unset;

        // Special Effects
        [CFGSetting]
        public Color GrasColor = Color.clear;
        [CFGSetting]
        public string GrasTexture;
        [CFGSetting]
        public string VariantName;

        public GameObject gameObject;
        private GameObject _mesh = null;

        internal GameObject mesh
        {
            get
            {
                if (_mesh == null)
                {
                    Activate();
                    //_mesh = GameObject.(model.prefab);
                    //GameObject.DontDestroyOnLoad(_mesh);
                    //_mesh.name = "Mesh";
                    //_mesh.transform.parent = transform;
                    //_mesh.transform.position = transform.position;
                    //_mesh.transform.rotation = transform.rotation;
                    //foreach (MonoBehaviour behaviour in _mesh.GetComponentsInChildren<MonoBehaviour>(true))
                    //{
                    //    behaviour.enabled = false;
                    //}
                    ////gameObject.SetActive(false);
                    //isActive = false;
                    //isSpawned = true;
                }
                return _mesh;
            }
            set
            {
                _mesh = value;
                _mesh.name = "Mesh";

                if (_mesh != gameObject)
                {
                    _mesh.transform.parent = gameObject.transform;
                    _mesh.transform.localPosition = Vector3.zero;
                    _mesh.transform.localRotation = Quaternion.identity;
                }
            }
        }
        internal GameObject wreck;
        internal DestructibleBuilding destructible;


        internal Transform transform => gameObject.transform;
        public Vector3 position => gameObject.transform.position;

        internal StaticModel model;

        public UrlDir.UrlConfig configUrl;
        public String configPath;
        internal List<ConfigNode> grassColor2Configs = new List<ConfigNode>();


        internal GroupCenter groupCenter = null;

        public bool isInSavegame = false;

        internal bool isDestroyed = false;

        private static Dictionary<string, Type> staticModules = new Dictionary<string, Type>();
        private static string moduleKey;

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
        public KKLaunchSite launchSite;

        public KKFacilityType facilityType = KKFacilityType.None;
        public List<KKFacility> myFacilities = new List<KKFacility>();


        // used for non KKFacility objects like AirRace
        public string legacyfacilityID;

        private Vector3 origScale;
        internal bool isActive = false;
        internal bool isPreview = false;
        internal bool isSpawned = false;

        internal int indexInGroup = 0;

        private List<Renderer> _rendererComponents;
        //internal List<StaticModule> myStaticModules = new List<StaticModule>();


        internal StaticInstance()
        {
            gameObject = new GameObject("KKBuilding");
            GameObject.DontDestroyOnLoad(gameObject);
            wreck = new GameObject("KKWreck");
            GameObject.DontDestroyOnLoad(wreck);
            wreck.transform.parent = gameObject.transform;
        }



        /// <summary>
        /// Updates the static instance with new settings
        /// </summary>
        public void Update()
        {
            RefLatitude = (float)CelestialBody.GetLatitudeAndLongitude(gameObject.transform.position).x;
            RefLongitude = (float)(CelestialBody.GetLatitudeAndLongitude(gameObject.transform.position).y);
            RadialPosition = radialPosition;

            gameObject.transform.localScale = origScale != Vector3.zero ? origScale * ModelScale : Vector3.one * ModelScale;

            RelativePosition = gameObject.transform.localPosition;
            Orientation = gameObject.transform.localEulerAngles;
            RadiusOffset = (float)((surfaceHeight - groupCenter.surfaceHeight) + RelativePosition.y);


            // Notify modules about update
            gameObject.BroadcastMessage("StaticObjectUpdate");
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

        public void ToggleAllColliders(bool enable)
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
            return Vector3.Distance(gameObject.transform.position, vPosition);
        }


        internal void TrySpawn()
        {
            if (!isSpawned)
            {
                Spawn();

                if (isSpawned)
                {
                    API.OnBuildingSpawned.Invoke(gameObject);
                }

            }
        }

        private string PrintChildren(GameObject o, string prefix)
        {
            string retstr = prefix + ">" + o.name;
            for (int i = 0; i < o.transform.childCount; ++i)
            {
                retstr += "\n" + PrintChildren(o.transform.GetChild(i).gameObject, prefix + "--");
            }
            return retstr;
        }


        private void Spawn()
        {
            CelestialBody.CBUpdate();
            isSpawned = true;

            ModelScaleSave = -1;

            mesh = ModelVariant.SpawnVariant(this);
            {
                if (_mesh == null)
                {
                    Log.UserError("Cannot spawn 3dModel of Instance: " + model.name);
                    Destroy();
                    return;
                }
            }

            if (model.isSquad)
            {
                //Debug.Log("@@@ Spawning Squad static " + model.name);
                //Debug.Log("Tree: \n" + PrintChildren(mesh, ""));

                InstanceUtil.MangleSquadStatic(this);
            }
            InstanceUtil.SetLayerRecursively(this, 15);

            mesh.SetActive(true);

            //Scaling
            origScale = gameObject.transform.localScale;             // save the original scale for later use
            gameObject.transform.localScale *= ModelScale;

            foreach (StaticModule module in model.modules)
            {
                moduleKey = (module.moduleNamespace + "_" + module.moduleClassname);
                Type moduleType;
                if (staticModules.ContainsKey(moduleKey))
                {
                    moduleType = staticModules[moduleKey];
                }
                else
                {
                    moduleType = AssemblyLoader.loadedAssemblies.SelectMany(asm => asm.assembly.GetTypes()).FirstOrDefault(t => t.Namespace == module.moduleNamespace && t.Name == module.moduleClassname);
                    staticModules.Add(moduleKey, moduleType);
                }

                StaticModule mod = mesh.AddComponent(moduleType) as StaticModule;

                if (mod != null)
                {
                    mod.enabled = false;
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
                    //myStaticModules.Add(mod);
                }
                else
                {
                    Log.UserError("Module " + module.moduleClassname + " could not be loaded in " + gameObject.name);
                }
            }


            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = true;
                AdvancedTextures.CheckForExistingMaterial(renderer);
            }

            ModelVariant.ApplyVariant(this);

            //Make LaunchSites more sturdy
            if (!model.isSquad)
            {
                Destructable.MakeDestructable(this);
                if (hasLauchSites)
                {
                    destructible.impactMomentumThreshold = Math.Max(destructible.impactMomentumThreshold, 3000f);
                    launchSite.AttachSelector();
                }
            }

            foreach (var facility in myFacilities)
            {
                facility.AttachSelector();
            }

        }


        internal void Despawn()
        {
            //if (hasLauchSites && (KerbalKonstructs.selectedInstance != this))
            //{
            //    return;
            //}

            // save the Color configs when we despawn, because they could be written to disk later
            GrassColor2[] grassArray = _mesh.GetComponents<GrassColor2>();
            if (grassArray.Length > 0)
            {
                grassColor2Configs.Clear();
                foreach (GrassColor2 grassColor in grassArray)
                {
                    grassColor2Configs.Add(grassColor.GiveConfig());
                }
            }

            foreach (StaticModule module in gameObject.GetComponentsInChildren<StaticModule>())
            {
                GameObject.DestroyImmediate(module);
            }

            foreach (DestructibleBuilding building in gameObject.GetComponentsInChildren<DestructibleBuilding>())
            {
                GameObject.DestroyImmediate(building);
            }

            destructible = null;

            _mesh.transform.parent = null;
            GameObject.DestroyImmediate(_mesh);

            wreck.transform.parent = null;
            GameObject.DestroyImmediate(wreck);

            wreck = new GameObject("wreck");
            GameObject.DontDestroyOnLoad(wreck);

            wreck.transform.position = transform.position;
            wreck.transform.rotation = transform.rotation;

            wreck.transform.parent = transform;

            isSpawned = false;

        }

        /// <summary>
        /// Spawns a new Instance in the Gameworld and registers itself to the Static Database 
        /// </summary>
        /// <param name="editing"></param>
        /// <param name="bPreview"></param>
        internal void Orientate()
        {
            // mangle Squads statics
            CelestialBody.CBUpdate();

            InstanceUtil.CreateGroupCenterIfMissing(this);

            if (!StaticDatabase.HasGroupCenter(groupCenterName))
            {
                Log.UserWarning("cannot load " + configPath);
                return;
            }
            groupCenter = StaticDatabase.GetGroupCenter(groupCenterName);

            if (RelativePosition.Equals(Vector3.zero))
            {
                Log.Normal("LegacySpawnInstance called for " + groupCenterName + "_" + model.name);
                LegacySpawnInstance();
                gameObject.transform.parent = groupCenter.gameObject.transform;
                RelativePosition = gameObject.transform.localPosition;
                Orientation = gameObject.transform.localEulerAngles;

            }
            else
            {
                gameObject.transform.position = groupCenter.gameObject.transform.position + RelativePosition; ;
                gameObject.transform.parent = groupCenter.gameObject.transform;
                gameObject.transform.localEulerAngles = Orientation;
                gameObject.transform.localPosition = RelativePosition;

            }


            RefLatitude = (float)CelestialBody.GetLatitudeAndLongitude(gameObject.transform.position).x;
            RefLongitude = (float)(CelestialBody.GetLatitudeAndLongitude(gameObject.transform.position).y);
            RadialPosition = radialPosition;

            StaticDatabase.AddStatic(this);

            // Add them to the bodys objectlist, so they show up as anomalies
            // After we got a new Name from StaticDatabase.AddStatic()
            if (isScanable)
            {
                var pqsObjectList = CelestialBody.pqsSurfaceObjects.ToList();
                if (!pqsObjectList.Contains((PQSSurfaceObject)groupCenter.pqsCity))
                {
                    Log.Normal("Added " + groupCenter.Group + " to scanable Objects");
                    pqsObjectList.Add(groupCenter.pqsCity as PQSSurfaceObject);
                }
                CelestialBody.pqsSurfaceObjects = pqsObjectList.ToArray();
            }
        }



        private void LegacySpawnInstance()
        {

            RefLatitude = KKMath.GetLatitudeInDeg(RadialPosition);
            RefLongitude = KKMath.GetLongitudeInDeg(RadialPosition);

            double alt = RadiusOffset;

            if (heighReference == HeightReference.Terrain)
            {
                alt += surfaceHeight;
            }
            gameObject.transform.parent = CelestialBody.gameObject.transform;

            gameObject.transform.localPosition = RadialPosition.normalized * (float)(CelestialBody.Radius + alt);
            gameObject.transform.localRotation = Quaternion.FromToRotation(Orientation, gameObject.transform.localPosition.normalized) * Quaternion.AngleAxis(RotationAngle, Vector3.up);

            KerbalKonstructs.convertLegacyConfigs = true;
        }



        /// <summary>
        /// Geturns a Facility by its type
        /// </summary>
        /// <param name="facilityType"></param>
        /// <returns></returns>
        internal KKFacility GetFacility(KKFacilityType facilityType)
        {
            foreach (KKFacility facility in myFacilities)
            {
                if (facility.facType == facilityType)
                {
                    return facility;
                }
            }
            return null;
        }


        /// <summary>
        /// resets the object highlightColor to 0 and resets the editing flag.
        /// </summary>
        /// <param name="enableColliders"></param>
        internal void DeselectObject(Boolean enableColliders)
        {

            if (enableColliders)
            {
                this.ToggleAllColliders(true);
            }

            //Color highlightColor = new Color(0, 0, 0, 0);
            this.HighlightObject(Color.black);
        }

        public void SaveConfig()
        {
            ConfigParser.SaveInstanceByCfg(configPath);
        }

        /// <summary>
        /// Returns the evelation of the surface under the object
        /// </summary>
        internal double surfaceHeight
        {
            get
            {
                //return CelestialBody.pqsController.GetSurfaceHeight(CelestialBody.pqsController.GetRelativePosition(gameObject.transform.position));
                return (CelestialBody.pqsController.GetSurfaceHeight(CelestialBody.GetRelSurfaceNVector(RefLatitude, RefLongitude).normalized * CelestialBody.Radius) - CelestialBody.pqsController.radius);
            }
        }

        internal Vector3 radialPosition
        {
            get
            {
                return (CelestialBody.GetRelSurfaceNVector(RefLatitude, RefLongitude).normalized * CelestialBody.Radius);
            }
        }


        internal string groupCenterName
        {
            get
            {
                return (CelestialBody.name + "_" + Group);
            }
        }


        internal void Activate()
        {
            if (isActive)
            {
                return;
            }

            TrySpawn();

            foreach (Collider colloder in gameObject.GetComponentsInChildren<Collider>(true).Where(col => col.isTrigger == false))
            {
                if (colloder.gameObject.GetComponent<KKMouseUtility>() == null)
                {
                    KKMouseUtility selector = colloder.gameObject.AddComponent<KKMouseUtility>();
                    selector.staticInstance = this;
                    selector.enabled = true;
                }
            }

            isActive = true;
            gameObject.SetActive(true);

            //if (!model.isSquad && !isDestroyed)
            //if (!model.isSquad && !isDestroyed)
            //{
            //    if (mesh != null)
            //    {
            //        foreach (Transform transform in mesh.GetComponentsInChildren<Transform>(true))
            //        {
            //            transform.gameObject.SetActive(true);
            //        }
            //    }
            //}

            foreach (MonoBehaviour module in gameObject.GetComponentsInChildren<MonoBehaviour>(true))
            {
                module.enabled = true;
            }


            gameObject.BroadcastMessage("StaticObjectUpdate");
        }


        internal void Deactivate()
        {
            if (!isActive)
            {
                return;
            }

            //Log.Normal("Deactivate: " + gameObject.name);

            if (isSpawned && destructible != null)
            {
                isDestroyed = destructible.IsDestroyed;
            }

            isActive = false;

            foreach (MonoBehaviour module in gameObject.GetComponentsInChildren<MonoBehaviour>(true))
            {
                module.enabled = false;
            }

            gameObject.SetActive(false);

            // LaunchSites have to stay on the surface.
            if (isSpawned && !hasLauchSites)
            {
                //Despawn();
            }

        }



        internal void Destroy()
        {
            if (StaticsEditorGUI.selectedObject == this) StaticsEditorGUI.selectedObject = null;

            if (launchSite != null)
            {
                LaunchSiteManager.DeleteLaunchSite(launchSite);
            }

            if (groupCenter != null)
            {
                groupCenter.RemoveInstance(this);
            }
            if (StaticDatabase.HasInstance(this))
            {

                StaticDatabase.DeleteStaticFromDB(this);
            }
            if (_mesh != null)
            {
                GameObject.DestroyImmediate(_mesh);
            }
            if (wreck != null)
            {
                wreck.transform.parent = null;
                GameObject.DestroyImmediate(wreck);
            }
            GameObject.DestroyImmediate(gameObject);
        }
    }
}