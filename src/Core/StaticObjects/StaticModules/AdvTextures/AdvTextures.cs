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

        private string[] seperators = new string[] {" " , "," , ";" };

        public void Start()
        {

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

                ReplaceShader(renderer,newShader);

                var myFields = this.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                foreach (var texturemap in myFields)
                {
                    if (texturemap.Name.Contains("_") && (texturemap.GetValue(this) != null))
                    {
                        Log.Normal(" Advanced texture" + texturemap.Name + " : " + (string)texturemap.GetValue(this));
                        Texture2D newTexture = KKGraphics.GetTexture((string)texturemap.GetValue(this), (texturemap.Name.Equals("_BumpMap", StringComparison.CurrentCultureIgnoreCase)), textureIndex);
                        renderer.material.SetTexture(texturemap.Name, newTexture);
                    }
                }
            }



        }


        private void ReplaceShader(MeshRenderer renderer, string newShaderName)
        {
            if (string.IsNullOrEmpty(newShaderName) || !KKGraphics.HasShader(newShaderName))
            {
                return;
            }

            Shader newShader = KKGraphics.GetShader(newShaderName);
            renderer.material.shader = newShader;
        }

    }
}