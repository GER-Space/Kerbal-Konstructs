using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;


namespace KerbalKonstructs
{
    public class AdvancedTextures : StaticModule
    {

        public string newShader = null;

        public string transforms = "Any";

        public string newMaterial = "";

        public string _MainTex = null;          // texture
        public string BuiltinIndex = "0";
        public string _BumpMap = null;          // normal map
        public string _ParallaxMap = null;      // height map
        public string _Emissive = null;         // legacy shader  U4 name for emissive map
        public string _EmissionMap = null;      // U5 std shader name for emissive map
        public string _MetallicGlossMap = null; // U5 metallic (standard shader)
        public string _OcclusionMap = null;     // ambient occlusion
        public string _SpecGlossMap = null;     // U5 metallic (standard shader - spec gloss setup)

        private int textureIndex = 0;
        private List<string> targetTransforms = new List<string>();

        private string[] seperators = new string[] { " ", ",", ";" };



        public void Start()
        {

            //Log.Normal("AdvTexture called on " + staticInstance.model.name);

            if (!int.TryParse(BuiltinIndex, out textureIndex))
            {
                Log.UserError("AdvancedTexture: could not parse BuiltinIndex " + BuiltinIndex);
            }

            targetTransforms = transforms.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();


            foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!transforms.Equals("Any", StringComparison.CurrentCultureIgnoreCase) && !targetTransforms.Contains(renderer.transform.name))
                {
                    continue;
                }

                // Log.Normal("Processing Transform: " + renderer.transform.name);

                if (newMaterial != "")
                {
                    ReplaceMaterial(renderer, newMaterial);
                    continue;
                }

                if (!string.IsNullOrEmpty(newShader))
                {
                    ReplaceShader(renderer, newShader);
                }

            }

            SetTexture(_MainTex, "_MainTex");
            SetTexture(_ParallaxMap, "_ParallaxMap");
            SetTexture(_Emissive, "_Emissive");
            SetTexture(_EmissionMap, "_EmissionMap");
            SetTexture(_MetallicGlossMap, "_MetallicGlossMap");
            SetTexture(_OcclusionMap, "_OcclusionMap");
            SetTexture(_SpecGlossMap, "_SpecGlossMap");
            SetTexture(_BumpMap, "_BumpMap", true);
        }


        private void SetTexture(string texturename, string targetname, bool isNormal = false)
        {
            if (!String.IsNullOrEmpty(texturename))
            {
                Texture2D newTexture = KKGraphics.GetTexture(texturename, isNormal, textureIndex);
                if (newTexture != null)
                {
                    foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
                    {
                        if (!transforms.Equals("Any", StringComparison.CurrentCultureIgnoreCase) && !targetTransforms.Contains(renderer.transform.name))
                        {
                            continue;
                        }
                        renderer.material.SetTexture(targetname, newTexture);
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
                Log.Normal("Material replaced: " + foundMaterial.name);
                renderer.material = Instantiate(foundMaterial);
            }

        }


        private void ReplaceShader(MeshRenderer renderer, string newShaderName)
        {
            if (!KKGraphics.HasShader(newShaderName))
            {
                Log.UserError("No Shader like this found: " + newShaderName);
                return;
            }

            Shader newShader = KKGraphics.GetShader(newShaderName);
            renderer.material.shader = newShader;
            //Log.Normal("Applied Shader: " + newShader.name);

        }

    }
}