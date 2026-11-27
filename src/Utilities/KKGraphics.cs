using KerbalKonstructs.Core;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using System;
using System.IO;

namespace KerbalKonstructs
{
    class KKGraphics
    {
        private static Dictionary<string, Shader> assetBundleShaders = new Dictionary<string, Shader>();
        private static bool loadedMaterials = false;

        private static Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Material> cachedMaterials = new Dictionary<string, Material>();

        private static List<string> imageExtentions = new List<string> { ".png", ".tga", ".jpg" , ".dds" };

        private static Dictionary<string, Texture> builtinTextures = new Dictionary<string, Texture>();
        private static bool texturesAreCached = false;

        private static Dictionary<string, Texture2D> normalMaps = new Dictionary<string, Texture2D>();

        /// <summary>
        /// Return shaders loaded from KSP asset bundles (with autoload on), then fallback to Shader.Find
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal static Shader GetShader(string name)
        {
            Shader shader = null;

            if (assetBundleShaders.ContainsKey(name))
            {
                shader = assetBundleShaders[name];
            }
            else
            {
                shader = Shader.Find(name);
            }

            if (shader == null)
            {
                Log.UserError("AdvTexture: Shader not found: " + name);
                Log.Trace();
                // return the error Shader, if we have one
                if (assetBundleShaders.ContainsKey("Hidden/InternalErrorShader"))
                {
                    //Log.UserWarning("Cannot load shader: " + name);
                    return assetBundleShaders["Hidden/InternalErrorShader"];
                }
            }

            return shader;
        }


        internal static void LoadShaderBundles ()
        {

            string bundleFileName ;

            switch (Application.platform)
            {
                case RuntimePlatform.OSXPlayer:
                    bundleFileName = "kkshaders.osx";
                    break;
                case RuntimePlatform.LinuxPlayer:
                    bundleFileName = "kkshaders.linux";
                    break;
                default:
                    bundleFileName = "kkshaders.windows";
                    break;
            }
            string bundlePath = KSPUtil.ApplicationRootPath + "GameData/KerbalKonstructs/Shaders/" + bundleFileName;

            AssetBundle shadersBundle = null;
            try
            {
                shadersBundle = AssetBundle.LoadFromFile(bundlePath);
                if (shadersBundle == null)
                {
                    Log.Normal("Failed to load shader asset file: "+ bundlePath);
                    return;
                }
                foreach (string shadername in shadersBundle.GetAllAssetNames())
                {
                    LoadAndRegisterShader(shadersBundle, shadername);
                }
            }
            catch (System.Exception exeption)
            {
               Log.Error("Error loading Shader assetbundle "+ exeption);
            }
            finally
            {
                if (shadersBundle != null)
                {
                    shadersBundle.Unload(false);
                }
            }

        }

        private static void LoadAndRegisterShader(AssetBundle bundle , string shaderName)
        {
            Shader newShader = bundle.LoadAsset<Shader>(shaderName);
            if (newShader == null) { return; } // This file is not a shader; ignore it.

            GameObject.DontDestroyOnLoad(newShader);
            if (!newShader.isSupported)
            {
                Log.Error("could not load shader: " + shaderName + " from: " + bundle.name);
            }
            else
            {
                GameDatabase.Instance.databaseShaders.AddUnique(newShader);
                assetBundleShaders.Add(newShader.name, newShader);
                Log.Normal("Loaded Shader: " + newShader.name + " from file: " + shaderName);
            }

        }

        /// <summary>
        /// Get a cached or create a Normal Map from a Texture
        /// </summary>
        /// <param name="texture"></param>
        /// <returns></returns>
        internal static Texture2D GetNormalMap(Texture2D texture)
        {
            if (texture == null || string.IsNullOrEmpty(texture.name))
            {
                Log.Error("Could not get NormalTexture for empty name or Texture");
                return null;
            }

            System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
            md5Hash.Initialize();
            byte[] crypto = md5Hash.ComputeHash(System.Text.Encoding.ASCII.GetBytes(texture.name));
            string normalHash = string.Empty;
            foreach (byte theByte in crypto)
            {
                normalHash += theByte.ToString("x2");
            }

            string storagePath = Path.Combine(KSPUtil.ApplicationRootPath, "GameData/KerbalKonstructs/Normals/");

            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }

            string filename = storagePath + normalHash + ".png";

            Texture2D normalMap;

            // first check if we loaded the map before:
            if (normalMaps.ContainsKey(normalHash))
            {
                //Log.Normal("returning Cached normalMap");
                return normalMaps[normalHash];
            }
            else
            {
                if (!File.Exists(filename))
                {
                    CreateNormalFromTex(texture, filename);
                }
                if (File.Exists(filename))
                {
                    normalMap = LoadNormalFromFile(filename, texture.width, texture.height);
                    normalMaps.Add(normalHash, normalMap);
                    //Log.Normal("returning normalMap from File");
                    return normalMap;
                }

                
            }
            // here you should never end
            Log.Error("Something went wrong for: " + texture.name);
            Log.Error("Should be cached here: " + filename);

            return null;
        }


        /// <summary>
        /// Create and Cache a Normal Map from a Texture
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="filename"></param>
        private static void CreateNormalFromTex (Texture2D texture, string filename)
        {
            if (texture == null)
            {
                //Log.Error("Called with no Texture");
                return;
            }
            //Log.Normal("Normal Map CacheName: " + filename);

            Material converter = new Material(GetShader("KK/Calc/NormalFromTexture"));
            converter.mainTexture = texture;
            converter.SetFloat("_Strengh", 0.35f );
            converter.SetFloat("_Offset", 0.8f);

            RenderTexture renderTarget;
                renderTarget = RenderTexture.GetTemporary(
               texture.width,
               texture.height,
               0,
               RenderTextureFormat.ARGB32,
               RenderTextureReadWrite.Linear);
            // Blit the pixels on texture to the RenderTexture
            Graphics.Blit(texture, renderTarget, converter);

            renderTarget.ToTexture2D().WritePNG(filename);
            //Log.Normal("Finished creation of normalMap");
        }


        /// <summary>
        /// Load a Normal Map from the Disk
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        internal static Texture2D LoadNormalFromFile(string fileName, int width, int height )
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Log.Error("Called with no Texture");
                return null;
            }


            Texture2D loadedTexture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            loadedTexture.LoadImage﻿(System.IO.File.ReadAllBytes(fileName), false);
            loadedTexture.Apply(false, false);

            Texture2D normalTexture = new Texture2D(width, height, TextureFormat.ARGB32, false, true);
            
            normalTexture = new Texture2D(loadedTexture.width, loadedTexture.height, TextureFormat.ARGB32, true, true);
            Color32[] colours = loadedTexture.GetPixels32();
            for (int i = 0; i < colours.Length; i++)
            {
                Color32 c = colours[i];
                c.a = c.r;
                c.r = c.b = c.g;
                colours[i] = c;
            }
            normalTexture.SetPixels32(colours);
            normalTexture.Apply(true, false);

            return normalTexture;
        }


        /// <summary>
        /// Remove a Cached Texture, so it is loaded from Disk during next access
        /// </summary>
        /// <param name="textureName"></param>
        internal static void RemoveCache(string textureName)
        {
            string textureKey;
            int index = 0;
            textureName.Replace("\\", "/");
            if (textureName.StartsWith("BUILTIN:"))
            {
                {
                    Log.Normal("Builtin Textures don't change");
                    return;
                }

            }
            else
            {
                textureKey = Regex.Replace(textureName, "/", "_") + index.ToString();
            }

            if (cachedTextures.ContainsKey(textureKey))
            {
                cachedTextures.Remove(textureKey);
            }
        }



        /// <summary>
        /// return a buildin or GameDatabase Texture
        /// </summary>
        /// <param name="textureName"></param>
        /// <returns></returns>
        internal static Texture2D GetTexture(string textureName, bool asNormal = false, int index = 0, bool createMibMaps = false )
        {
            if (string.IsNullOrEmpty(textureName))
            {
                //Log.UserWarning("Empty texture Name");
                return null;
            }
            string textureKey;
            textureName.Replace("\\", "/");
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
            Texture2D foundTexture = null;

            if (textureName.StartsWith("BUILTIN:"))
            {
                foundTexture = GetBuiltinTexture(textureName, index);
            }
            else
            {

                Texture2D tmpTexture = null;
                string fileExtension = GetImageExtention(textureName);

                //// Otherwise search the game database for one loaded from GameData/
                if (GameDatabase.Instance.ExistsTexture(textureName) && (fileExtension != null))
                {
                    // Get the texture URL
                    tmpTexture = GameDatabase.Instance.GetTexture(textureName, asNormal);

                    foundTexture = new Texture2D(tmpTexture.width, tmpTexture.height, TextureFormat.ARGB32, createMibMaps);
                    if (fileExtension == ".dds")
                    {
                        foundTexture = LoadTextureDXT(File.ReadAllBytes(KSPUtil.ApplicationRootPath + "GameData/" + textureName + GetImageExtention(textureName)), createMibMaps);
                    }
                    else
                    {
                        foundTexture.LoadImage﻿(File.ReadAllBytes(KSPUtil.ApplicationRootPath + "GameData/" + textureName + GetImageExtention(textureName)), false);
                    }
                    foundTexture.Apply(createMibMaps, false);
                }
                else
                {
                    Log.UserWarning("Failed: TextureLoader faild. Fallback to GameDatabase");
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


        public static Texture2D LoadTextureDXT(byte[] ddsBytes, bool createMibMaps)
        {
            byte[] exampleByteArray = new byte[] { ddsBytes[84], ddsBytes[85], ddsBytes[86], ddsBytes[87] };
            string textureFormat = System.Text.Encoding.ASCII.GetString(exampleByteArray);



            if (textureFormat != "DXT1" && textureFormat != "DXT5")
            {
                Log.Error("Invalid TextureFormat. Only DXT1 and DXT5 formats are supported by this method.");
                return null;
            }
            TextureFormat format = (TextureFormat)Enum.Parse(typeof(TextureFormat), textureFormat, createMibMaps);
            //Log.Normal("Found DXT Texture Format: " + format.ToString());

            byte ddsSizeCheck = ddsBytes[4];
            if (ddsSizeCheck != 124)
            {
                Log.Error("Invalid DDS DXTn texture. Unable to read");  //this header byte should be 124 for DDS image files
                return null;
            }

            int height = ddsBytes[13] * 256 + ddsBytes[12];
            int width = ddsBytes[17] * 256 + ddsBytes[16];

            int DDS_HEADER_SIZE = 128;
            byte[] dxtBytes = new byte[ddsBytes.Length - DDS_HEADER_SIZE];
            Buffer.BlockCopy(ddsBytes, DDS_HEADER_SIZE, dxtBytes, 0, ddsBytes.Length - DDS_HEADER_SIZE);

            Texture2D texture = new Texture2D(width, height, format, true);
            texture.LoadRawTextureData(dxtBytes);

            return (texture);
        }


        internal static Texture2D GetBuiltinTexture(string textureName, int index)
        {
            Texture2D foundTexture = null;
            string textureNameShort = Regex.Replace(textureName, "BUILTIN:/", "");

            string texkey = textureNameShort + "_" + index;


            if (!texturesAreCached)
            {
                Texture[] foundTextures = Resources.FindObjectsOfTypeAll<Texture>();
                int counter = 0;
                foreach (Texture texture in foundTextures)
                {
                    counter = 0;
                    while (builtinTextures.ContainsKey(texture.name + "_" + counter))
                    {
                        counter++;
                    }
                    builtinTextures.Add(texture.name + "_" + counter, texture);
                    // Debug Code
                    //Log.Normal("BuiltinTex:  " +  texture.name + " | " + counter);
                }
                texturesAreCached = true;
            }

            if (builtinTextures.ContainsKey(texkey))
            {
                foundTexture = builtinTextures[texkey] as Texture2D;
            }
            else
            {
                Log.UserError("AdvTexture: Could not find built-in texture " + textureNameShort + " index: " + index);
                foundTexture = null;
            }

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
                        //Log.Normal("Material added: " + material.name);
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

            string fullPath = KSPUtil.ApplicationRootPath + "GameData/" + imageName;

            int pathIndex = (fullPath).LastIndexOf('/');
            string path = (fullPath).Substring(0, pathIndex + 1);
            string imageShortName = (fullPath).Substring(pathIndex + 1);

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
