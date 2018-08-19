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
        private static List<Type> behavioursToRemove = new List<Type> { typeof(DestructibleBuilding), typeof(CrashObjectName), typeof(CommNet.CommNetHome), typeof(PQSCity2) };


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


            var transforms = gameObject.transform.GetComponentsInChildren<Transform>(true);
            foreach (var transform in transforms)
            {

                if (transform.name.Equals("wreck", StringComparison.InvariantCultureIgnoreCase))
                {
                    transform.parent = null;
                    GameObject.Destroy(transform.gameObject);
                }

                //if (transform.name.Equals("commnetnode", StringComparison.InvariantCultureIgnoreCase))
                //{
                //    transform.parent = null;
                //    GameObject.Destroy(transform.gameObject);
                //}
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
                controller.maxSpeed = 2 / instance.ModelScale;
                controller.maxElevation = 90f;
                controller.minElevation = 5f;

                DishController.Dish dish = new DishController.Dish();
                dish.rotationTransform = gameObject.transform.FindRecursive("Lower Assembly");
                dish.elevationTransform = gameObject.transform.FindRecursive("Satellite Dish");
                controller.dishes = new DishController.Dish[] { dish };
                controller.enabled = true;
            }


            //if (instance.model.name == "KSC_Runway_level_2")
            //{
            //    SquadStatics.PimpLv2Runway(instance.gameObject);
            //}

        }

        internal static void SetActiveRecursively(StaticInstance instance, bool active)
        {
            
            if (instance.isActive != active)
            {
                instance.isActive = active;

                foreach (StaticModule module in instance.gameObject.GetComponents<StaticModule>())
                {
                    module.StaticObjectUpdate();
                }

                instance.gameObject.SetActive(active);
                
                var transforms = instance.gameObject.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < transforms.Length; i++)
                {
                    transforms[i].gameObject.SetActive(active);
                }
            }
        }

        /// <summary>
        /// Sets tje Layer of the Colliders
        /// </summary>
        /// <param name="sGameObject"></param>
        /// <param name="newLayerNumber"></param>
        internal static void SetLayerRecursively(StaticInstance instance, int newLayerNumber)
        {

            var transforms = instance.gameObject.GetComponentsInChildren<Transform>(true);
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
    }
}


