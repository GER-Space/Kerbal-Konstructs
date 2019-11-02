using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    static class KKClassExtentions
    {
        internal static Dictionary<int, string> gameObjectTags = new Dictionary<int, string>();
        //internal static AsmUtils.Detour getGetTagDetour;
        //internal static AsmUtils.Detour getSetTagDetour;



        public static Transform FindRecursive(this Transform transform, string name)
        {
            foreach (Transform child in transform.gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child;
                }
            }
            Log.Warning("Transform not found: " + name);
            return null;

            //var result = aParent.Find(name);
            //if (result != null)
            //    return result;
            //foreach (Transform child in aParent)
            //{
            //    result = child.FindDeepChild(name);
            //    if (result != null)
            //        return result;
            //}
            //return null;
        }

        public static Transform FindRecursiveContains(this Transform transform, string name)
        {
            foreach (Transform child in transform.gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.Contains(name))
                {
                    return child;
                }
            }
            Log.Warning("Transform not found: " + name);
            return null;

            //var result = aParent.Find(name);
            //if (result != null)
            //    return result;
            //foreach (Transform child in aParent)
            //{
            //    result = child.FindDeepChild(name);
            //    if (result != null)
            //        return result;
            //}
            //return null;
        }

        public static Transform[] FindAllRecursive(this Transform transform, string name)
        {
            List<Transform> transforms = new List<Transform>();

            foreach (Transform child in transform.gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    transforms.Add(child);
                }
            }

            return transforms.ToArray();
        }

        public static Transform[] FindAllRecursiveContains(this Transform transform, string name)
        {
            List<Transform> transforms = new List<Transform>();

            foreach (Transform child in transform.gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (child.name.Contains(name))
                {
                    transforms.Add(child);
                }
            }

            return transforms.ToArray();
        }




        internal static void FakeGameObjectTags()
        {

            PropertyInfo origProperty = typeof(GameObject).GetProperty("tag", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo newGetMethod = typeof(KKClassExtentions).GetMethod("GetTag", BindingFlags.Static | BindingFlags.Public);
            MethodInfo newSetMethod = typeof(KKClassExtentions).GetMethod("SetTag", BindingFlags.Static | BindingFlags.Public);

            foreach (var param in newSetMethod.GetParameters())
            {
                Log.Normal("Set Method Params: " + param.Name + " " + param.ParameterType.ToString());
            }


            //getGetTagDetour = new AsmUtils.Detour(origProperty.GetGetMethod(), newGetMethod);
            //getGetTagDetour.Install();

            //getSetTagDetour = new AsmUtils.Detour(origProperty.GetSetMethod(), newSetMethod);
            //getSetTagDetour.Install();
        }

        public static string GetTag(GameObject gameObject)
        {
            int instanceID = gameObject.GetInstanceID();
            if (gameObjectTags.ContainsKey(instanceID))
            {
                Log.Normal("Returning tag (cached): " + gameObjectTags[instanceID] + " from " + instanceID);
                return gameObjectTags[instanceID];
            }
            else
            {
                //getGetTagDetour.Uninstall();
                string retval = gameObject.tag;
                //getGetTagDetour.Install();
                if (String.IsNullOrEmpty(retval))
                {
                    retval = "";
                }
                Log.Normal("Returning tag (uncached): " + retval + " from " + instanceID);
                gameObjectTags.Add(instanceID, retval);
                return retval;
            }
        }

        public static void SetTag(GameObject gameObject, string value)
        {
            int instanceID = gameObject.GetInstanceID();
            Log.Normal("Setting " + instanceID + " to: " + value);
            if (gameObjectTags.ContainsKey(instanceID))
            {
                gameObjectTags[instanceID] = value;
            }
            else
            {
                gameObjectTags.Add(instanceID, value);
            }
        }


        public static Bounds GetAllRendererBounds(this GameObject gameObject)
        {
            Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

    }
}
