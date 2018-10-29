using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KerbalKonstructs.Core;

namespace KerbalKonstructs
{
    class KKGraphics
    {
        private static Dictionary<string, Shader> allShaders = new Dictionary<string, Shader>();
        private static bool loadedShaders = false;
        private static bool loadedMaterials = false;

        private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Material> cachedMaterials = new Dictionary<string, Material>();

        private static List<string> imageExtentions = new List<string> { ".png", ".tga" , ".jpg"};




        /// <summary>
        /// Load all shaders into the system and fill our shader database.
        /// </summary>
        internal static void LoadShaders()
        {
            foreach (var shader in Resources.FindObjectsOfTypeAll<Shader>())
            {
                if (!allShaders.ContainsKey(shader.name))
                {
                    allShaders.Add(shader.name, shader);
                    //Util.log("Loaded shader: " + shader.name);
                }
            }
            loadedShaders = true;
        }

        internal static bool HasShader (string name)
        {
            if (!loadedShaders)
            {
                LoadShaders();
            }
            return allShaders.ContainsKey(name);
        }


        /// <summary>
        /// Replacement for Shader.Find() function, as we return also shaders, that are through KSP asset bundles (with autoload on)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static Shader GetShader(string name)
        {
            if (!loadedShaders)
            {
                LoadShaders();
            }

            if (allShaders.ContainsKey(name))
            {
                return allShaders[name];
            }
            else
            {
                Log.UserError("AdvTexture: Shader not found: " + name);
                // return the error Shader, if we have one
                if (allShaders.ContainsKey("Hidden/InternalErrorShader"))
                {
                    return allShaders["Hidden/InternalErrorShader"];
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// return a buildin or GameDatabase Texture
        /// </summary>
        /// <param name="textureName"></param>
        /// <returns></returns>
        internal static Texture2D GetTexture(string textureName, bool asNormal = false, int index = 0)
        {
            string textureKey;

            if (textureName.StartsWith("BUILTIN:"))
            {
                {
                    textureKey = Regex.Replace(textureName, "BUILTIN:/", "") + index.ToString();
                }

            }
            else
            {
                textureKey = Regex.Replace(textureName, "/", "_") + index.ToString();
            }

            if (cachedTextures.ContainsKey(textureKey))
            {
                return cachedTextures[textureKey];
            }

            List<Texture> foundTextures = null;
            Texture2D foundTexture = null;
            ;
            if (textureName.StartsWith("BUILTIN:"))
            {
                string textureNameShort = Regex.Replace(textureName, "BUILTIN:/", "");
                foundTextures = Resources.FindObjectsOfTypeAll<Texture>().Where(tex => tex.name == textureNameShort).ToList() as List<Texture>;

                if (foundTextures.Count == 0)
                {
                    Log.UserError("AdvTexture: Could not find built-in texture " + textureNameShort);
                    return null;
                }
                if (foundTextures.Count < index+1)
                {
                    Log.UserError("AdvTexture: index out of range" + textureNameShort + " : " + index );
                    return null;
                }
                return foundTextures[index] as Texture2D;

            }
            else
            {

                Texture2D tmpTexture = null;

                //// Otherwise search the game database for one loaded from GameData/
                if (GameDatabase.Instance.ExistsTexture(textureName) && (GetImageExtention(textureName) != null))
                {
                    // Get the texture URL
                    tmpTexture = GameDatabase.Instance.GetTexture(textureName, asNormal);


                    foundTexture = new Texture2D(tmpTexture.width, tmpTexture.height, TextureFormat.ARGB32, false);
                    foundTexture.LoadImage﻿(System.IO.File.ReadAllBytes("GameData/" + textureName + GetImageExtention(textureName)), false);
                }
                else
                {
                    Log.UserWarning("AdvTexture: TextureLoader faild. Fallback to GameDatabase");
                    foundTexture = GameDatabase.Instance.GetTexture(textureName, asNormal);
                }

                //// Otherwise search the game database for one loaded from GameData/
                //if (GameDatabase.Instance.ExistsTexture(textureName))
                //{
                //    // Get the texture URL
                //    foundTexture = GameDatabase.Instance.GetTexture(textureName, asNormal);
                //}
            }
            cachedTextures.Add(textureKey, foundTexture);
            return foundTexture;
        }

        internal static Material GetMaterial(string materialName)
        {
            if (!loadedMaterials)
            {
                foreach (Material material in Resources.FindObjectsOfTypeAll<Material>())
                {
                    if (!cachedMaterials.ContainsKey(material.name))
                    {
                        cachedMaterials.Add(material.name, material);
                    }
                }

                loadedMaterials = true;
            }

            if (cachedMaterials.ContainsKey(materialName))
            {
                return cachedMaterials[materialName];
            }
            if (cachedMaterials.ContainsKey(materialName + " (Instance)"))
            {
                return cachedMaterials[materialName + " (Instance)"];
            }

            Log.UserError("AdvTexture: No Material found: " + materialName);
            return null;
        }



        private static string GetImageExtention(string imageName)
        {
            int pathIndex = (KSPUtil.ApplicationRootPath + "GameData/" + imageName).LastIndexOf('/');
            string path = (KSPUtil.ApplicationRootPath + "GameData/" + imageName).Substring(0, pathIndex +1);
            string imageShortName = (KSPUtil.ApplicationRootPath + "GameData/" + imageName).Substring(pathIndex + 1);

            //Log.Normal("path: " + path);
            //Log.Normal("imageShortName: " + imageShortName);

            foreach (var filename in System.IO.Directory.GetFiles(path, (imageShortName + ".*")))
            {
                //Log.Normal("Found Filename: " + filename);
                foreach (string pattern in imageExtentions)
                {
                    //Log.Normal("pattern:" + pattern);
                    if (filename.Contains(pattern))
                    {
                        return pattern;
                    }
                }
            }

            Log.UserError("AdvTexture: Could not find an image with the name: " + imageName);
            return null;
        }


    }
}
