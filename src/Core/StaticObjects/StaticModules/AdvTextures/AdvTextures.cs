using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace KerbalKonstructs
{
    public class AdvancedTextures : StaticModule
    {

        public string newShader = null;

        public string transforms = "Any";

        public string newMaterial = "";
        public string BuiltinIndex = "0";

        public string tileTextureWithScale = "false";
        public string forceTiling = "1, 1";

        public string _MainTex = null;          // texture
        public string _BumpMap = null;          // normal map
        public string _ParallaxMap = null;      // height map
        public string _Emissive = null;         // legacy shader  U4 name for emissive map
        public string _EmissionMap = null;      // U5 std shader name for emissive map
        public string _MetallicGlossMap = null; // U5 metallic (standard shader)
        public string _OcclusionMap = null;     // ambient occlusion
        public string _SpecGlossMap = null;     // U5 metallic (standard shader - spec gloss setup)

        public string _Color = "";      // MainColor

        private int textureIndex = 0;
        private List<string> targetTransforms = new List<string>();

        private readonly string[] seperators = new string[] { ",", ";" };
        private static Dictionary<string, Material> cachedMaterials = new Dictionary<string, Material>();

        private Color color = Color.white;
        private bool setColor = false;

        private bool doTileing = false;

        private Vector2 iTileing = Vector2.zero;
        TileTextures tileing;
        private bool done = false;


        public void Start()
        {
            if (done)
            {
                return;
            }
            done = true;

            //Log.Normal("AdvTexture called on " + staticInstance.model.name);

            if (!int.TryParse(BuiltinIndex, out textureIndex))
            {
                Log.UserError("AdvancedTexture: could not parse BuiltinIndex " + BuiltinIndex);
            }

            if (!bool.TryParse(tileTextureWithScale, out doTileing))
            {
                Log.UserError("AdvancedTexture: could not parse TileTexture " + tileTextureWithScale);
            }

            //                Log.UserError("AdvancedTexture: could not parse TileTexture " + tileTextureWithScale);


            if (!string.IsNullOrEmpty(_Color))
            {
                color = ConfigNode.ParseColor(_Color);
                setColor = true;
            }


            if (doTileing)
            {
                try
                {
                    iTileing = ConfigNode.ParseVector2(forceTiling);
                    Log.Normal("found tiling: " + iTileing.ToString());
                }
                catch
                {
                    Log.UserError("Cannot parse: \"forceTiling\" : " + forceTiling);
                    iTileing = Vector2.zero;
                }


                tileing = staticInstance.mesh.AddComponent<TileTextures>();
                tileing.initialTileing = iTileing;
                tileing.staticInstance = staticInstance;
                tileing.textureTransformNames = transforms;
                tileing.Start();
                tileing.enabled = true;
            }


            List<string> tmpList = transforms.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();

            foreach (string value in tmpList)
            {
                targetTransforms.Add(value.Trim());
            }


            foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!transforms.Equals("Any", StringComparison.CurrentCultureIgnoreCase) && !targetTransforms.Contains(renderer.transform.name))
                {
                    continue;
                }

                // Log.Normal("Processing Transform: " + renderer.transform.name);

                if (!string.IsNullOrEmpty(newMaterial) )
                {
                    ReplaceMaterial(renderer, newMaterial);
                    continue;
                }

                if (!string.IsNullOrEmpty(newShader))
                {
                    ReplaceShader(renderer, newShader);
                }

                if (setColor) 
                {
                    renderer.material.SetColor("_Color", color);
                }
                SetTexture(renderer, _MainTex, "_MainTex");
                SetTexture(renderer, _ParallaxMap, "_ParallaxMap");
                SetTexture(renderer, _Emissive, "_Emissive");
                SetTexture(renderer, _EmissionMap, "_EmissionMap");
                SetTexture(renderer, _MetallicGlossMap, "_MetallicGlossMap");
                SetTexture(renderer, _OcclusionMap, "_OcclusionMap");
                SetTexture(renderer, _SpecGlossMap, "_SpecGlossMap");
                SetTexture(renderer, _BumpMap, "_BumpMap", true);

                //     KKGraphics.ReplaceShader(renderer);
                CheckForExistingMaterial(renderer);

            }


        }


        internal static void CheckForExistingMaterial(Renderer renderer)
        {

            if (renderer.material.mainTexture == null)
            {
                return;
            }
            string textureName = renderer.material.mainTexture.GetHashCode().ToString();
            string ColorValue = "";
            string tiling = renderer.material.GetTextureScale("_MainTex").ToString();
            string shader = renderer.material.shader.name;


            if (renderer.material.HasProperty("_Color"))
            {
                ColorValue = (renderer.material.color.r.ToString() + "_" + renderer.material.color.g.ToString() + "_" + renderer.material.color.b.ToString() + "_" + renderer.material.color.a.ToString());
                //Log.Normal("ColorValueOrig : " + ColorValue);
                //Log.Normal("ColorValue: " + renderer.material.color.ToString());
            }

            string key = (shader + "_" + textureName + "_" + tiling + "_" + ColorValue);

            if (!cachedMaterials.ContainsKey(key))
            {
                //     Log.Normal("creating new: " + key);
                cachedMaterials.Add(key, renderer.material);
            }
            else
            {
                //     Log.Normal("setting to: " + key);
                renderer.sharedMaterial = cachedMaterials[key];
            }
        }


        private void SetTexture(MeshRenderer renderer, string texturename, string targetname, bool isNormal = false)
        {
            if (!String.IsNullOrEmpty(texturename))
            {
                Texture2D newTexture = KKGraphics.GetTexture(texturename, isNormal, textureIndex);
                if (newTexture != null)
                {

                    renderer.material.SetTexture(targetname, newTexture);
                    if (doTileing)
                    {
                        tileing.StaticObjectUpdate();
                    }
                }
                else
                {
                    Log.UserWarning("cannot set Texture: " + texturename + " as " + targetname + " on: " + staticInstance.model.name);
                }
            }
        }


        private void ReplaceMaterial(MeshRenderer renderer, string materialName)
        {
            //Log.Normal("Material replaceder called");
            Material foundMaterial = KKGraphics.GetMaterial(materialName);
            if (foundMaterial != null)
            {
                // Log.Normal("Material replaced: " + foundMaterial.name);
                renderer.material = Instantiate(foundMaterial);
                if (doTileing)
                {
                    tileing.StaticObjectUpdate();
                }
            }

        }


        internal void ReplaceShader(MeshRenderer renderer, string newShaderName)
        {
            Shader newShader = KKGraphics.GetShader(newShaderName);

            if (newShader == null)
            {
                Log.UserError("No Shader like this found: " + newShaderName);
                return;
            }

            renderer.material.shader = newShader;
            //Log.Normal("Applied Shader: " + newShader.name);

        }

    }
}