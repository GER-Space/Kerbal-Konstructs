using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using KerbalKonstructs;

namespace KerbalKonstructs.Core
{
    public class GroupCenter
    {
        [CFGSetting]
        public string Group;
        //[CFGSetting]
        //public string Name;
        //[CFGSetting]
        //public string UUID;

        //Positioning
        [CFGSetting]
        public CelestialBody CelestialBody = null;
        [CFGSetting]
        public Vector3 RadialPosition = Vector3.zero;
        [CFGSetting]
        public Vector3 Orientation = Vector3.up;
        [CFGSetting]
        public float RadiusOffset = 0f;
        [CFGSetting]
        public float RotationAngle = 0f;
        [CFGSetting]
        public float Heading = 361f;
        [CFGSetting]
        public float ModelScale = 1f;
        [CFGSetting]
        public bool SeaLevelAsReference = false;
        [CFGSetting]
        public double RefLatitude = 361d;
        [CFGSetting]
        public double RefLongitude = 361d;


        public UrlDir.UrlConfig configUrl;
        public String configPath = null;

        public GameObject gameObject;
        internal PQSCity pqsCity;
        private Vector3 origScale = new Vector3(1, 1, 1);

        public List<StaticInstance> childInstances = new List<StaticInstance>();
        public bool isActive = true;

        public List<KKLaunchSite> launchsites = new List<KKLaunchSite>();

        public bool hidden = false;
        public bool isBuiltIn = false;
        public bool isInSavegame = false;





        public bool isHidden
        {
            get
            {
                if (!hidden)
                {
                    return false;
                }

                bool active = false;
                foreach (KKLaunchSite site in launchsites)
                {
                    active = (active || ! site.LaunchSiteIsHidden);
                }
                return (!active);
            }
        }


        internal void Spawn()
        {

            var key = dbKey;
            if (string.IsNullOrEmpty(key))
                return;
            
            if (StaticDatabase.HasGroupCenter(key))
            {
                string oldName = Group;
                int index = 0;
                while (StaticDatabase.HasGroupCenter(key))
                {
                    Group = oldName + "_" + index.ToString();
                    index++;
                    key = dbKey;
                }
            }

            gameObject = new GameObject();
            GameObject.DontDestroyOnLoad(gameObject);

            CelestialBody.CBUpdate();


            gameObject.name = Group;
            //gameObject.name = "SpaceCenter";

            pqsCity = gameObject.AddComponent<PQSCity>();

            PQSCity.LODRange range = new PQSCity.LODRange
            {
                renderers = new GameObject[0],
                objects = new GameObject[0],
                visibleRange = 25000
            };
            pqsCity.lod = new[] { range };
            pqsCity.frameDelta = 10000; //update interval for its own visiblility range checking. unused by KK, so set this to a high value

            if (RadialPosition == Vector3.zero)
            {
                if ((RefLatitude != 361d) && (RefLongitude != 361d))
                {
                    RadialPosition = KKMath.GetRadiadFromLatLng(CelestialBody, RefLatitude, RefLongitude);
                }
                else
                {
                    Log.UserError("No Valid Position found for Group: " + Group);
                }
            }
            else
            if ((RefLatitude == 361d) || (RefLongitude == 361d))
            {
                {
                    RefLatitude = KKMath.GetLatitudeInDeg(RadialPosition);
                    RefLongitude = KKMath.GetLongitudeInDeg(RadialPosition);
                }
            }

            pqsCity.reorientFinalAngle = 0;
            pqsCity.repositionRadial = RadialPosition; //position

            pqsCity.repositionRadiusOffset = RadiusOffset; //height
            pqsCity.reorientInitialUp = Orientation; //orientation
            pqsCity.reorientToSphere = true; //adjust rotations to match the direction of gravity
            pqsCity.sphere = CelestialBody.pqsController;
            pqsCity.order = 100;
            pqsCity.modEnabled = true;
            pqsCity.transform.parent = CelestialBody.pqsController.transform;

            pqsCity.repositionToSphereSurfaceAddHeight = false;
            pqsCity.repositionToSphereSurface = false;
            SetReference();

            pqsCity.OnSetup();
            pqsCity.Orientate();

            UpdateRotation2Heading();

            StaticDatabase.AddGroupCenter(this);

        }

        internal void UpdateRotation2Heading()
        {
            if (Heading >= 361f)
            {
                // legacy configs
                pqsCity.reorientFinalAngle = RotationAngle; //rotation x axis
            }
            else
            {
                // we are alligned to the 0 rotation, now we rotate until we got the real heading, then we messure the angle and then we update the internal Roation value
                Vector3 oldForeward = gameObject.transform.forward;
                Vector3 oldRight = gameObject.transform.right;
                Quaternion oldRotation = gameObject.transform.rotation;

                Vector3 forward = CelestialBody.GetWorldSurfacePosition(RefLatitude, RefLongitude, RadiusOffset) - CelestialBody.position;

                // Use the body's polar axis as "up" so heading 0 points toward the pole,
                // matching the heading getter. Without this, tilted bodies would align
                // to celestial north instead of the body's own north.
                Quaternion rotForward = Quaternion.LookRotation(forward, CelestialBody.transform.up);
                Quaternion rotHeading = Quaternion.Euler(0f, 0f, Heading);
                Quaternion halveInvert = Quaternion.Euler(-90f, -90f, -90f);

                Quaternion newRotation = rotForward * rotHeading * halveInvert;

                gameObject.transform.rotation = newRotation;

                float newfinalAngle = Vector3.Angle(oldForeward, gameObject.transform.forward);

                if (Vector3.Dot(gameObject.transform.forward, oldRight) < 0)
                {
                    newfinalAngle = (360 - newfinalAngle) % 360;
                }
                RotationAngle = newfinalAngle;
                pqsCity.reorientFinalAngle = newfinalAngle;
            }
            pqsCity.Orientate();

        }

        public void RenameGroup(string newName)
        {
            StaticDatabase.RemoveGroupCenter(this);
            Group = newName;
            StaticDatabase.AddGroupCenter(this);

            StaticInstance[] staticInstances = childInstances.ToArray();

            foreach (StaticInstance instance in staticInstances)
            {
                StaticDatabase.ChangeGroup(instance, this);
            }

        }

        public void AddInstance(StaticInstance instance)
        {
            if (!childInstances.Contains(instance))
            {
                childInstances.Add(instance);
            }

        }


        public void RemoveInstance(StaticInstance instance)
        {
            if (childInstances.Contains(instance))
            {
                childInstances.Remove(instance);
            }
        }



        public void SetInstancesEnabled(bool isInRange)
        {
            if (isInRange == isActive)
            {
                return;
            }
            Log.Normal("Setting Group " + Group + ": active state form: " + isActive + " to: " + isInRange);
            isActive = isInRange;

            foreach (StaticInstance instance in childInstances)
            {
                if (isActive)
                {
                    ActivateInstance(instance);
                    instance.Activate();
                }
                else
                {
                    DeActivateInstance(instance);
                    instance.Deactivate();
                }
            }
        }


        /// <summary>
        /// Checks if the Group is in Range, or nearby so the bases are seen
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="maxDistance"></param>
        public void CheckIfInRange(float distance, float maxDistance)
        {

            if (distance < 5000  && isHidden)
            {
                foreach (KKLaunchSite site in launchsites)
                {
                    site.WasSeen = true;
                    MiscUtils.HUDMessage("You discovered the bases at: " + Group);
                }
            }
            bool isInRange = (distance < maxDistance);
            SetInstancesEnabled(isInRange);
        }

        public static void ActivateInstance(StaticInstance instance)
        {
            switch (instance.FacilityType)
            {
                //case "Hangar":
                //    HangarGUI.CacheHangaredCraft(instance);
                //    break;
                case "LandingGuide":
                    LandingGuideUI.instance.drawLandingGuide(instance);
                    break;
                case "TouchdownGuideL":
                    LandingGuideUI.instance.drawTouchDownGuideL(instance);
                    break;
                case "TouchdownGuideR":
                    LandingGuideUI.instance.drawTouchDownGuideR(instance);
                    break;
            }
        }

        public static void DeActivateInstance(StaticInstance instance)
        {
            switch (instance.FacilityType)
            {
                //case "Hangar":
                //    HangarGUI.CacheHangaredCraft(instance);
                //    break;
                case "LandingGuide":
                    LandingGuideUI.instance.drawLandingGuide(null);
                    break;
                case "TouchdownGuideL":
                    LandingGuideUI.instance.drawTouchDownGuideL(null);
                    break;
                case "TouchdownGuideR":
                    LandingGuideUI.instance.drawTouchDownGuideR(null);
                    break;
            }
        }


        public void Update()
        {
            if (pqsCity != null)
            {
                pqsCity.repositionRadial = RadialPosition;
                pqsCity.repositionRadiusOffset = RadiusOffset;
                pqsCity.reorientInitialUp = Orientation;

                pqsCity.reorientFinalAngle = RotationAngle;


                pqsCity.transform.localScale = origScale * ModelScale;

                RefLatitude = KKMath.GetLatitudeInDeg(RadialPosition);
                RefLongitude = KKMath.GetLongitudeInDeg(RadialPosition);

                SetReference();

                pqsCity.Orientate();
            }
            Heading = heading;
            // Notify modules about update
            foreach (StaticModule module in gameObject.GetComponentsInChildren<StaticModule>((true)))
            {
                module.StaticObjectUpdate();
            }
        }

        /// <summary>
        /// Parser for MapDecalInstance Objects
        /// </summary>
        /// <param name="cfgNode"></param>
        internal void ParseCFGNode(ConfigNode cfgNode)
        {
            if (!ConfigUtil.initialized)
            {
                ConfigUtil.InitTypes();
            }

            foreach (var field in ConfigUtil.groupCenterFields.Values)
            {
                ConfigUtil.ReadCFGNode(this, field, cfgNode);
            }
        }

        /// <summary>
        /// Writer for MapDecalObjects
        /// </summary>
        /// <param name="mapDecalInstance"></param>
        /// <param name="cfgNode"></param>
        internal void WriteConfig(ConfigNode cfgNode)
        {
            foreach (var groupField in ConfigUtil.groupCenterFields.Values)
            {
                if ((groupField.GetValue(this) == null) || (groupField.Name == "RadialPosition") || (groupField.Name == "RotationAngle"))
                {
                    continue;
                }
                ConfigUtil.Write2CfgNode(this, groupField, cfgNode);
            }
        }

        /// <summary>
        /// Writes out the GroupCenter config
        /// </summary>
        /// <param name="instance"></param>
        public void Save()
        {
            if (isBuiltIn)
            {
                return;
            }
            if (isInSavegame)
            {
                return;
            }

            if (configPath == null)
            {
                configPath = KerbalKonstructs.newInstancePath + "/KK_GroupCenter_" + dbKey + ".cfg";
                if (System.IO.File.Exists(KSPUtil.ApplicationRootPath + "GameData/" + configPath))
                {
                    configPath = KerbalKonstructs.newInstancePath + "/KK_GroupCenter_" + dbKey + "_" + Guid.NewGuid().ToString() + ".cfg";
                }
            }

            ConfigNode masterNode = new ConfigNode("");
            ConfigNode childNode = new ConfigNode("KK_GroupCenter");

            WriteConfig(childNode);
            masterNode.AddNode(childNode);

            masterNode.Save(KSPUtil.ApplicationRootPath + "GameData/" + configPath, "Generated by Kerbal Konstructs");

            API.OnGroupSaved.Invoke(this);
        }


        internal void CopyGroup(GroupCenter sourceGroup)
        {
            foreach (StaticInstance sourceInstance in sourceGroup.childInstances)
            {
                StaticInstance instance = new StaticInstance();
                //instance.mesh = UnityEngine.Object.Instantiate(sourceInstance.model.prefab);
                instance.RelativePosition = sourceInstance.RelativePosition;
                instance.Orientation = sourceInstance.Orientation;
                instance.CelestialBody = CelestialBody;
                instance.ModelScale = sourceInstance.ModelScale;
                instance.VariantName = sourceInstance.VariantName;

                instance.Group = Group;
                instance.groupCenter = this;

                instance.model = sourceInstance.model;
                if (!Directory.Exists(KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath))
                {
                    Directory.CreateDirectory(KSPUtil.ApplicationRootPath + "GameData/" + KerbalKonstructs.newInstancePath);
                }
                instance.configPath = KerbalKonstructs.newInstancePath + "/" + sourceInstance.model.name + "-instances.cfg";
                instance.configUrl = null;

                instance.Orientate();
                instance.Activate();

            }
        }


        public void DeleteGroupCenter()
        {
            foreach (StaticInstance child in childInstances.ToArray())
            {
                KerbalKonstructs.instance.DeleteInstance(child);
            }

            if (StaticsEditorGUI.GetActiveGroup() == this) StaticsEditorGUI.SetActiveGroup(null);

            StaticDatabase.RemoveGroupCenter(this);
            // check later when saving if this file is empty

            GameObject.Destroy(gameObject);

            KerbalKonstructs.deletedGroups.Add(this);

            Log.Normal("deleted GroupCenter: " + dbKey);
        }


        internal void SetReference()
        {
            pqsCity.repositionToSphere = SeaLevelAsReference;
            pqsCity.repositionToSphereSurface = !SeaLevelAsReference; //Snap to surface?
            pqsCity.repositionToSphereSurfaceAddHeight = !SeaLevelAsReference;
        }

        public double surfaceHeight
        {
            get
            {
                return (CelestialBody.pqsController.GetSurfaceHeight(CelestialBody.GetRelSurfaceNVector(RefLatitude, RefLongitude).normalized * CelestialBody.Radius) - CelestialBody.pqsController.radius);
            }
        }

        public string dbKey
        {
            get
            {
                var cb = CelestialBody;
                if (cb == null)
                    return string.Empty;
                return (CelestialBody.name + "_" + Group);
            }
        }

        public float heading
        {
            get
            {
                Vector3 myForward = Vector3.ProjectOnPlane(gameObject.transform.forward, upVector);
                float myHeading;

                if (Vector3.Dot(myForward, eastVector) >= 0)
                {
                    myHeading = Vector3.Angle(myForward, northVector);
                }
                else
                {
                    myHeading = (360 - Vector3.Angle(myForward, northVector) % 360);
                }
                return myHeading % 360;
            }
        }


        /// <summary>
        /// gives a vector to the east
        /// </summary>
        private Vector3 eastVector
        {
            get
            {
                return Vector3.Cross(upVector, northVector).normalized;
            }
        }

        /// <summary>
        /// vector to north
        /// </summary>
        private Vector3 northVector
        {
            get
            {
                return Vector3.ProjectOnPlane(CelestialBody.transform.up, upVector).normalized;
            }
        }

        private Vector3 upVector
        {
            get
            {
                return CelestialBody.GetSurfaceNVector(RefLatitude, RefLongitude).normalized;
            }
        }


    }
}
