using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;
using KSP;

namespace KerbalKonstructs.Core
{
    internal static class InstanceUtil
    {

//        private static List<Type> behaviosToRemove = new List<Type> { typeof(DestructibleBuilding), typeof(CollisionEventsHandler), typeof(CrashObjectName), typeof(CommNet.CommNetHome), typeof(PQSCity2) };

        //private static List<Type> behavioursToRemove = new List<Type> { typeof(DestructibleBuilding), typeof(CrashObjectName), typeof(CommNet.CommNetHome), typeof(PQSCity2) };
        private static List<Type> behavioursToRemove = new List<Type> { typeof(CrashObjectName), typeof(CommNet.CommNetHome), typeof(PQSCity2) };

        internal static List<TimeOfDayAnimation.MaterialProperty> dayNightEmissives = null;
        internal static Color dotColor;
        internal static string dotPoperty;
        internal static AnimationCurve dotAnimationCurve;
        internal static List<string> materialPropertyNames = new List<string>();




        /// <summary>
        /// Returns a StaticObject object for a gives GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
		internal static StaticInstance GetStaticInstanceForGameObject(GameObject gameObject)
        {
            List<StaticInstance> objList = (from obj in StaticDatabase.allStaticInstances where obj.gameObject == gameObject select obj).ToList();

            if (objList.Count >= 1)
            {
                if (objList.Count > 1)
                {
                    Log.UserError("More than one StaticObject references to GameObject " + gameObject.name);
                }
                return objList[0];
            }

            Log.UserWarning("StaticObject doesn't exist for " + gameObject.name);
            return null;
        }

        /// <summary>
        /// Removes the wreck model from an KSC Object.
        /// </summary>
        internal static void MangleSquadStatic(StaticInstance instance)
        {
            GameObject gameObject = instance.gameObject;

            gameObject.transform.parent = null;
            foreach (var component in gameObject.GetComponentsInChildren<MonoBehaviour>(true))
            {

                if (behavioursToRemove.Contains(component.GetType()))
                {
               //     Log.Normal("Removed: " + bla.GetType().ToString());
                    UnityEngine.Object.Destroy(component);
                }               
            }
            //instance.gameObject.tag = String.Empty;

            //foreach (Transform transform in gameObject.transform.GetComponentsInChildren<Transform>(true))
            //{
            //    //transform.gameObject.tag = String.Empty;
            //    if (transform.name.Equals("wreck", StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        transform.parent = null;
            //        GameObject.Destroy(transform.gameObject);
            //    }

            //    //if (transform.name.Equals("commnetnode", StringComparison.InvariantCultureIgnoreCase))
            //    //{
            //    //    transform.parent = null;
            //    //    GameObject.Destroy(transform.gameObject);
            //    //}
            //}

            foreach (var component in gameObject.GetComponentsInChildren<DestructibleBuilding>(true))
            {
                component.id = instance.UUID;
                component.preCompiledId = true;
                instance.destructible = component;
            }


            TimeOfDayAnimation dotAnim = gameObject.AddComponent<TimeOfDayAnimation>();
            dotAnim.emissives = new List<TimeOfDayAnimation.MaterialProperty>();
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material mat in renderer.materials)
                {
                   // Log.Normal("found Material: " + gameObject.name +" " +  mat.name);
                    foreach (string matname in materialPropertyNames)
                    {
                        if (mat.name.Contains(matname) && mat.HasProperty(dotPoperty))
                        {
                            //Log.Normal("added Materialproperty to: " + gameObject.name + " "+ mat.name);

                            TimeOfDayAnimation.MaterialProperty mp = new TimeOfDayAnimation.MaterialProperty();
                            mp.material = mat;
                            mp.propertyName = dotPoperty;
                            mp.isDirty = true;
                            dotAnim.emissives.Add(mp);
                            break;
                        }
                    }
                }
            }
            dotAnim.emissiveTgtColor = dotColor;
            dotAnim.emissiveColorProperty = dotPoperty;
            dotAnim.emissivesCurve = dotAnimationCurve;
            dotAnim.enabled = true;

            // Lv3 Tracking Dish Animation
            if (instance.model.name == "SQUAD_LV3_Tracking_Dish")
            {
                DishController controller = gameObject.AddComponent<DishController>();

                controller.fakeTimeWarp = 1f;
                controller.maxSpeed = 2/instance.ModelScale;
                controller.maxElevation = 90f;
                controller.minElevation = 5f;

                DishController.Dish dish = new DishController.Dish();
                dish.rotationTransform = gameObject.transform.FindRecursive("Lower Assembly");
                dish.elevationTransform = gameObject.transform.FindRecursive("Satellite Dish");
                controller.dishes = new DishController.Dish[] { dish };
                controller.enabled = true;
            }

            if (instance.model.name == "KSC_FuelTank")
            {
                //Log.Normal("Fixing KSC Fuel Tank");
                GameObject oldGameObject = instance.mesh;
                GameObject newBaseObject = new GameObject(instance.model.name);
                oldGameObject.transform.parent = newBaseObject.transform;
                oldGameObject.transform.localEulerAngles = new Vector3(270, 0, 0);
                instance.mesh = newBaseObject;
            }

        }

        //internal static void SetActiveRecursively(StaticInstance instance, bool active)
        //{

        //    if (instance == null || instance.gameObject == null)
        //    {
        //        Log.UserError("No instance or GameObject found: " + instance.model.name + " " +instance.Group);
        //        return;
        //    }

        //    if (active && !instance.isActive)
        //    {
        //        SetActive(instance);
        //    }
        //    else
        //    {
        //        SetInActive(instance);
        //    }


        //}

        //internal static void SetActive(StaticInstance instance)
        //{

        //    instance.isActive = true;
        //    instance.gameObject.SetActive(true);

        //    //DestructibleBuilding destructible = instance.gameObject.GetComponentInChildren<DestructibleBuilding>();

        //    //if (instance.model.isSquad)
        //    //{
        //    //    foreach (Transform transform in instance.mesh.GetComponentsInChildren<Transform>(true))
        //    //    {
        //    //        if (destructible == null || destructible.IsDestroyed == transform.name.Contains("wreck"))
        //    //        {
        //    //            transform.gameObject.SetActive(true);
        //    //        }
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    if (destructible != null && destructible.IsDestroyed)
        //    //    {
        //    //        foreach (GameObject wreck in instance.wrecks)
        //    //        foreach (Transform transform in wreck.GetComponentsInChildren<Transform>(true))
        //    //        {
        //    //            transform.gameObject.SetActive(true);
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        foreach (Transform transform in instance.mesh.GetComponentsInChildren<Transform>(true))
        //    //        {
        //    //            transform.gameObject.SetActive(true);
        //    //        }
        //    //    }
        //    //}

        //    foreach (MonoBehaviour module in instance.gameObject.GetComponentsInChildren<MonoBehaviour>())
        //    {
        //        module.enabled = true;
        //    }
        //    instance.gameObject.BroadcastMessage("StaticObjectUpdate");
        //}



        //internal static void SetInActive(StaticInstance instance)
        //{
        //    instance.isActive = false;
        //    instance.gameObject.SetActive(false);

        //    //foreach (Transform transform in instance.gameObject.GetComponentsInChildren<Transform>(true))
        //    //{
        //    //    transform.gameObject.SetActive(false);
        //    //}


        //    foreach (MonoBehaviour module in instance.gameObject.GetComponentsInChildren<MonoBehaviour>())
        //    {
        //        module.enabled = false;
        //    }
        //}


        /// <summary>
        /// Sets tje Layer of the Colliders
        /// </summary>
        /// <param name="sGameObject"></param>
        /// <param name="newLayerNumber"></param>
        internal static void SetLayerRecursively(StaticInstance instance, int newLayerNumber)
        {

            var transforms = instance.transform.gameObject.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                // don't set trigger collider 
                if ((transforms[i].gameObject.GetComponent<Collider>() != null) && (transforms[i].gameObject.GetComponent<Collider>().isTrigger))
                {
                    continue;
                }
                transforms[i].gameObject.layer = newLayerNumber;
            }
        }

        /// <summary>
        /// Creates a GroupCenter if needed
        /// </summary>
        /// <param name="instance"></param>
        internal static void CreateGroupCenterIfMissing(StaticInstance instance)
        {
            if (!StaticDatabase.HasGroupCenter(instance.groupCenterName))
            {
                if (instance.RadialPosition.Equals(Vector3.zero))
                {
                    Log.UserError("No Group Found and no position found to create a Group: " + instance.configPath);
                    return;
                }

                Log.Normal("Creating a new Group Center: " + instance.groupCenterName);

                GroupCenter center = new GroupCenter();
                center.Group = instance.Group;
                center.RadialPosition = instance.RadialPosition;
                center.CelestialBody = instance.CelestialBody;
                center.Spawn();
            }
            else
            {
                // we have a GroupCenter and a legacy Object we might regroup now
                if ((instance.RelativePosition.Equals(Vector3.zero)) && (!instance.RadialPosition.Equals(Vector3.zero)))
                {
                    Vector3 groupPostion = StaticDatabase.GetGroupCenter(instance.groupCenterName).gameObject.transform.position;
                    Vector3 instancePosition = instance.CelestialBody.GetWorldSurfacePosition(KKMath.GetLatitudeInDeg(instance.RadialPosition), KKMath.GetLongitudeInDeg(instance.RadialPosition), (instance.CelestialBody.pqsController.GetSurfaceHeight(instance.RadialPosition) - instance.CelestialBody.Radius));

                    if (Vector3.Distance(groupPostion, instancePosition) > KerbalKonstructs.localGroupRange)
                    {
                        // Check if we have a similar named GC somewhere
                        GroupCenter closestCenter = CheckForClosesCenter(instance);
                        if (closestCenter != null)
                        {
                            instance.Group = closestCenter.Group;
                        }
                        else
                        {
                            Log.Normal("Creating a new local GroupCenter on: " + instance.CelestialBody.name + " for " + instance.Group);

                            GroupCenter center = new GroupCenter();
                            // we use index keys for new Group Names
                            center.Group = instance.Group;
                            center.RadialPosition = instance.RadialPosition;
                            center.CelestialBody = instance.CelestialBody;
                            center.Spawn();
                            instance.Group = center.Group;
                            Log.Normal("New GroupCenter Created: " + instance.groupCenterName);
                        }
                    }
                }
            }
        }

        internal static GroupCenter CheckForClosesCenter(StaticInstance instance)
        {

            GroupCenter closestCenter = null;

            Vector3 groupPostion = StaticDatabase.GetGroupCenter(instance.groupCenterName).gameObject.transform.position;
            Vector3 instancePosition = instance.CelestialBody.GetWorldSurfacePosition(KKMath.GetLatitudeInDeg(instance.RadialPosition), KKMath.GetLongitudeInDeg(instance.RadialPosition), (instance.CelestialBody.pqsController.GetSurfaceHeight(instance.RadialPosition) - instance.CelestialBody.Radius));
            float distance = Vector3.Distance(groupPostion, instancePosition);
            float oldDistance = KerbalKonstructs.localGroupRange * 2;


            if (distance > KerbalKonstructs.localGroupRange)
            {
                // Check if we have a similar named GC somewhere
                int index = 0;
                while (StaticDatabase.HasGroupCenter(instance.CelestialBody.name + "_" + instance.Group + "_" + index.ToString()))
                {                   
                    groupPostion = StaticDatabase.GetGroupCenter(instance.CelestialBody.name + "_" + instance.Group + "_" + index.ToString()).gameObject.transform.position;                  
                    distance = Vector3.Distance(groupPostion, instancePosition);

                    if (distance < KerbalKonstructs.localGroupRange)
                    {
                        if (closestCenter == null || distance < oldDistance)
                        {
                            oldDistance = distance;
                            closestCenter = StaticDatabase.GetGroupCenter(instance.CelestialBody.name + "_" + instance.Group + "_" + index.ToString());
                        }
                    }
                    index++;
                }
            }
            else
            {
                closestCenter = StaticDatabase.GetGroupCenter(instance.groupCenterName);
            }
            return closestCenter;
        }
    }
}


