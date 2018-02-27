using System;
using System.Collections.Generic;
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
                Log.UserError("Shader not found: " + name);
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
        internal static Texture2D GetTexture(string textureName, bool asNormal = false)
        {
            Texture2D foundTexture = null;
            if (textureName.StartsWith("BUILTIN:"))
            {
                string textureNameShort = Regex.Replace(textureName, "BUILTIN:/", "");
                foundTexture = Resources.FindObjectsOfTypeAll<Texture>().FirstOrDefault(tex => tex.name == textureNameShort) as Texture2D;
                if (foundTexture == null)
                {
                    Log.UserError("Could not find built-in texture " + textureNameShort);
                }
            }
            else
            {
                // Otherwise search the game database for one loaded from GameData/
                if (GameDatabase.Instance.ExistsTexture(textureName))
                {
                    // Get the texture URL
                    foundTexture = GameDatabase.Instance.GetTexture(textureName, asNormal);
                }
            }
            return foundTexture;
        }



    }
}
