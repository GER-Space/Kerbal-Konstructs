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



        /// <summary>
        /// Libits the Channels of a Color to this maxValue
        /// </summary>
        /// <param name="color"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public static Color LimitTo(this Color color, float maxValue)
        {
            Color finalColor = new Color(Math.Min(color.r, maxValue) ,
                                         Math.Min(color.g, maxValue),
                                         Math.Min(color.b, maxValue),
                                         Math.Min(color.a, maxValue)
                                        );
            return finalColor;
        }

        /// <summary>
        /// Tries to devide the first color by the second;
        /// </summary>
        /// <param name="first"></param>
        /// <param name="divisor"></param>
        /// <returns></returns>
        public static Color DivideWith(this Color first, Color divisor)
        {
            Color finalColor = new Color(first.r / Math.Max(divisor.r, 0.00001f), 
                                         first.g / Math.Max(divisor.g, 0.00001f),
                                         first.b / Math.Max(divisor.b, 0.00001f),
                                         first.a / Math.Max(divisor.a, 0.00001f)
                                        );
            return finalColor;
        }

        /// <summary>
        /// Converts a Texture to a Texture2D
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        public static Texture2D ToTexture2D(this Texture texture, int textureSize = -1)
        {
            RenderTexture renderTarget;
            if (textureSize == -1)
            {
                renderTarget = RenderTexture.GetTemporary(
               texture.width,
               texture.height,
               0,
               RenderTextureFormat.ARGB32,
               RenderTextureReadWrite.Linear);
            }
            else
            {
                renderTarget = RenderTexture.GetTemporary(
               textureSize,
               textureSize,
               0,
               RenderTextureFormat.ARGB32,
               RenderTextureReadWrite.Linear);

            }
            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, renderTarget);
            return renderTarget.ToTexture2D();
        }

        /// <summary>
        /// Converts a RenderTexture to a Texture2D
        /// </summary>
        /// <param name="rTex"></param>
        /// <returns></returns>
        public static Texture2D ToTexture2D(this RenderTexture rTex)
        {
            Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.ARGB32, false);
            RenderTexture oldActive = RenderTexture.active;
            RenderTexture.active = rTex;
            tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            tex.Apply(false, false);
            RenderTexture.active = oldActive;
            return tex;
        }

        /// <summary>
        /// Creates a fully readable copy of the source Texture with the size
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="textureSize"></param>
        /// <returns></returns>
        public static Texture2D BlitTexture(this Texture2D texture, int textureSize = -1)
        {
            RenderTexture renderTarget;
            if (textureSize == -1)
            {
                renderTarget = RenderTexture.GetTemporary(
               texture.width,
               texture.height,
               0,
               RenderTextureFormat.ARGB32,
               RenderTextureReadWrite.Linear);
            } else
            {
                renderTarget = RenderTexture.GetTemporary(
               textureSize,
               textureSize,
               0,
               RenderTextureFormat.ARGB32,
               RenderTextureReadWrite.Linear);

            }
            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, renderTarget);
            return renderTarget.ToTexture2D();
        }

        /// <summary>
        /// Writes a Texture2D to a file
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="filePath"></param>
        public static void WritePNG(this Texture2D texture, string filePath)
        {
            var mainTextureblob = texture.EncodeToPNG();
            // For testing purposes, also write to a file in the project folder
            System.IO.File.WriteAllBytes(KSPUtil.ApplicationRootPath + "GameData/" +filePath+".png", mainTextureblob);
        }

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
