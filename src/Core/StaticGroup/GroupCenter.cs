using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using KerbalKonstructs.Modules;
using KerbalKonstructs.Utilities;
using KerbalKonstructs.Career;
using UnityEngine;
using System.IO;
using KerbalKonstructs.UI;

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
        public float ModelScale = 1f;

        internal double RefLatitude = 0d;
        internal double RefLongitude = 0d;


        public UrlDir.UrlConfig configUrl;
        public String configPath = null;

        internal GameObject gameObject;
        internal PQSCity pqsCity;
        private Vector3 origScale = new Vector3(1, 1, 1);

        internal List<StaticInstance> childInstances = new List<StaticInstance>();
        internal bool isActive = true;


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
            gameObject.name = Group + "_PQS";

            pqsCity = gameObject.AddComponent<PQSCity>();

            PQSCity.LODRange range = new PQSCity.LODRange
            {
                renderers = new GameObject[0],
                objects = new GameObject[0],
                visibleRange = 25000
            };
            pqsCity.lod = new[] { range };
            pqsCity.frameDelta = 10000; //update interval for its own visiblility range checking. unused by KK, so set this to a high value
            pqsCity.repositionRadial = RadialPosition; //position
            pqsCity.repositionRadiusOffset = RadiusOffset; //height
            pqsCity.reorientInitialUp = Orientation; //orientation
            pqsCity.reorientFinalAngle = RotationAngle; //rotation x axis
            pqsCity.reorientToSphere = true; //adjust rotations to match the direction of gravity
            pqsCity.sphere = CelestialBody.pqsController;
            pqsCity.order = 100;
            pqsCity.modEnabled = true;
            pqsCity.transform.parent = CelestialBody.pqsController.transform;

            pqsCity.repositionToSphereSurface = true; //Snap to surface?
            pqsCity.repositionToSphereSurfaceAddHeight = true;
            pqsCity.repositionToSphere = false;

            pqsCity.OnSetup();
            pqsCity.Orientate();

            RefLatitude = KKMath.GetLatitudeInDeg(RadialPosition);
            RefLongitude = KKMath.GetLongitudeInDeg(RadialPosition);

            StaticDatabase.AddGroupCenter(this);

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

        internal void SetInstancesEnabled(bool newState)
        {
            if (newState == isActive)
            {
                return;
            }
            isActive = newState;

            Log.Normal("Setting Group " + Group + " to: " + newState);

            foreach (StaticInstance instance in childInstances)
            {
                if (isActive)
                {
                    ActivateInstance(instance);
                }
                else
                {
                    DeActivateInstance(instance);
                }
                InstanceUtil.SetActiveRecursively(instance, newState);
            }
        }

        internal static void ActivateInstance(StaticInstance instance)
        {
            switch (instance.FacilityType)
            {
                case "Hangar":
                    HangarGUI.CacheHangaredCraft(instance);
                    break;
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
                case "Hangar":
                    HangarGUI.CacheHangaredCraft(instance);
                    break;
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
                pqsCity.repositionToSphereSurface = true; //Snap to surface?
                pqsCity.repositionToSphereSurfaceAddHeight = true;
                pqsCity.repositionToSphere = false;

                pqsCity.Orientate();
            }
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
        private void WriteConfig(ConfigNode cfgNode)
        {
            foreach (var groupField in ConfigUtil.groupCenterFields.Values)
            {
                if (groupField.GetValue(this) == null)
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
                instance.mesh = UnityEngine.Object.Instantiate(sourceInstance.model.prefab);
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

                instance.SpawnObject();
                InstanceUtil.SetActiveRecursively(instance, true);

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


    }
}
