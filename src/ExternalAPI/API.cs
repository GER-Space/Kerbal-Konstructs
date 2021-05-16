using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;
using System;
using UnityEngine;

namespace KerbalKonstructs
{
    public static class API
    {

        public static Action<GameObject> OnBuildingSpawned = delegate { };


        public static string SpawnObject(string modelName)
        {
            StaticModel model = StaticDatabase.GetModelByName(modelName);
            if (model != null)
            {
                return CareerEditor.instance.SpawnInstance(model, 3f, KerbalKonstructs.instance.GetCurrentBody().transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position));
            }
            else
            {
                Log.UserError("API:SpawnObject: Could not find selected KK-Model named: " + modelName);
                return null;
            }
        }

        public static void RemoveStatic(string uuid)
        {
            if (StaticDatabase.instancedByUUID.ContainsKey(uuid))
            {
                StaticDatabase.instancedByUUID[uuid].Destroy();
            }
            else
            {
                Log.UserWarning("API:RemoveObject: Can´t find a static with the UUID: " + uuid);
            }
        }

        public static void HighLightStatic(string uuid, Color color)
        {
            if (StaticDatabase.instancedByUUID.ContainsKey(uuid))
            {
                StaticDatabase.instancedByUUID[uuid].HighlightObject(color);
            }
            else
            {
                Log.UserWarning("API:Highlight: Can´t find a static with the UUID: " + uuid);
            }
        }

        public static string GetModelTitel(string uuid)
        {
            if (StaticDatabase.instancedByUUID.ContainsKey(uuid))
            {
                return StaticDatabase.instancedByUUID[uuid].model.title;
            }
            else
            {
                Log.UserWarning("API:GetModelTitel: Can´t find a static with the UUID: " + uuid);
                return null;
            }
        }

        public static void SetEditorRange(float newRange)
        {
            CareerEditor.maxEditorRange = newRange;
        }


        public static GameObject GetGameObject(string uuid)
        {
            if (StaticDatabase.instancedByUUID.ContainsKey(uuid))
            {
                return (StaticDatabase.instancedByUUID[uuid]).gameObject;
            }
            else
            {
                Log.UserWarning("API:GetGameObject: Can´t find a static with the UUID: " + uuid);
                return null;
            }
        }

        public static string PlaceStatic(string modelName, string bodyName, double lat, double lng, float alt, float rotation, bool isScanable = false)
        {
            StaticModel model = StaticDatabase.GetModelByName(modelName);
            if (model != null)
            {
                StaticInstance instance = new StaticInstance();
                instance.isInSavegame = true;

                instance.heighReference = HeightReference.Terrain;

                //instance.mesh = UnityEngine.Object.Instantiate(model.prefab);
                instance.RadiusOffset = alt;
                instance.CelestialBody = ConfigUtil.GetCelestialBody(bodyName);
                instance.Group = "SaveGame";
                instance.RadialPosition = KKMath.GetRadiadFromLatLng(instance.CelestialBody, lat, lng);
                instance.RotationAngle = rotation;
                instance.Orientation = Vector3.up;
                instance.VisibilityRange = (PhysicsGlobals.Instance.VesselRangesDefault.flying.unload + 3000);
                instance.RefLatitude = lat;
                instance.RefLongitude = lng;

                instance.model = model;
                instance.configPath = null;
                instance.configUrl = null;

                instance.isScanable = isScanable;

                instance.Orientate();

                return instance.UUID;
            }

            Log.UserError("API:PlaceStatic: StaticModel not found in Database: " + modelName);
            return null;
        }



        public static bool AddEnterTriggerCallback(string uuid, Action<Part> myFunction)
        {
            if (StaticDatabase.instancedByUUID.ContainsKey(uuid))
            {
                //do stuff
                StaticInstance instance = StaticDatabase.instancedByUUID[uuid];

                KKCallBack controller = instance.gameObject.GetComponent<KKCallBack>();
                if (controller == null)
                {
                    Log.UserWarning("API:AddEnterTriggerCallback: Can´t find a CallBack controller");
                    return false;
                }

                controller.RegisterEnterFunc(myFunction);
                return true;
            }
            else
            {
                Log.UserWarning("API:AddEnterTriggerCallback: Can´t find a static with the UUID: " + uuid);
                return false;
            }
        }

        public static bool AddStayTriggerCallback(string uuid, Action<Part> myFunction)
        {
            if (StaticDatabase.instancedByUUID.ContainsKey(uuid))
            {
                //do stuff
                StaticInstance instance = StaticDatabase.instancedByUUID[uuid];

                KKCallBack controller = instance.gameObject.GetComponent<KKCallBack>();
                if (controller == null)
                {
                    Log.UserWarning("API:AddStayTriggerCallback: Can´t find a CallBack controller");
                    return false;
                }

                controller.RegisterStayFunc(myFunction);
                return true;
            }
            else
            {
                Log.UserWarning("API:AddStayTriggerCallback: Can´t find a static with the UUID: " + uuid);
                return false;
            }
        }

        public static bool AddExitTriggerCallback(string uuid, Action<Part> myFunction)
        {
            if (StaticDatabase.instancedByUUID.ContainsKey(uuid))
            {
                //do stuff
                StaticInstance instance = StaticDatabase.instancedByUUID[uuid];

                KKCallBack controller = instance.gameObject.GetComponent<KKCallBack>();
                if (controller == null)
                {
                    Log.UserWarning("API:AddExitTriggerCallback: Can´t find a CallBack controller");
                    return false;
                }

                controller.RegisterExitFunc(myFunction);
                return true;
            }
            else
            {
                Log.UserWarning("API:AddExitTriggerCallback: Can´t find a static with the UUID: " + uuid);
                return false;
            }
        }

        public static void RegisterOnBuildingSpawned(Action<GameObject> action)
        {
            OnBuildingSpawned += action;
        }

        public static void UnregisterOnBuildingSpawned(Action<GameObject> action)
        {
            OnBuildingSpawned -= action;
        }


    }
}
