using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using KerbalKonstructs;

namespace KerbalKonstructs.Core
{
    class GroupCenter
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
        internal double RefLatitude = 361d;
        [CFGSetting]
        internal double RefLongitude = 361d;


        public UrlDir.UrlConfig configUrl;
        public String configPath = null;

        internal GameObject gameObject;
        internal PQSCity pqsCity;
        private Vector3 origScale = new Vector3(1, 1, 1);

        internal List<StaticInstance> childInstances = new List<StaticInstance>();
        internal bool isActive = true;

        internal List<KKLaunchSite> launchsites = new List<KKLaunchSite>();

        internal bool hidden = false;
        internal bool isBuiltIn = false;
        internal bool isInSavegame = false;





        internal bool isHidden
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

            if (StaticDatabase.HasGroupCenter(dbKey))
            {
                string oldName = Group;
                int index = 0;
                while (StaticDatabase.HasGroupCenter(dbKey))
                {
                    Group = oldName + "_" + index.ToString();
                    index++;
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

                Vector3 forward = CelestialBody.GetRelSurfacePosition(RefLatitude, RefLongitude + CelestialBody.directRotAngle, RadiusOffset);
                Quaternion rotForward = Quaternion.LookRotation(forward);
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

        internal void RenameGroup(string newName)
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

        internal void AddInstance(StaticInstance instance)
        {
            if (!childInstances.Contains(instance))
            {
                childInstances.Add(instance);
            }

        }


        internal void RemoveInstance(StaticInstance instance)
        {
            if (childInstances.Contains(instance))
            {
                childInstances.Remove(instance);
            }
        }



        internal void SetInstancesEnabled(bool isInRange)
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
        internal bool CheckIfInRange(float distance, float maxDistance)
        {
            bool isKnown = !isHidden;
            if (distance < 5000  && isHidden)
            {
                foreach (KKLaunchSite site in launchsites)
                {
                    site.WasSeen = true;
                }
                MiscUtils.HUDMessage("You discovered the bases at: " + Group);
                isKnown = true;
            }
            bool isInRange = (distance < maxDistance);
            SetInstancesEnabled(isInRange);
            return isKnown;
        }

        internal static void ActivateInstance(StaticInstance instance)
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

        internal static void DeActivateInstance(StaticInstance instance)
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


        internal void Update()
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
        internal void Save()
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


        internal void DeleteGroupCenter()
        {
            foreach (StaticInstance child in childInstances.ToArray())
            {
                KerbalKonstructs.instance.DeleteInstance(child);
            }

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

        internal double surfaceHeight
        {
            get
            {
                return (CelestialBody.pqsController.GetSurfaceHeight(CelestialBody.GetRelSurfaceNVector(RefLatitude, RefLongitude).normalized * CelestialBody.Radius) - CelestialBody.pqsController.radius);
            }
        }

        internal string dbKey
        {
            get
            {
                return (CelestialBody.name + "_" + Group);
            }
        }

        internal float heading
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
