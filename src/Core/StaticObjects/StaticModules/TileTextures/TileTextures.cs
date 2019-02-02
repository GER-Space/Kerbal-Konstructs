using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;


namespace KerbalKonstructs
{
    public class TileTextures: StaticModule
    {

        public string textureTransformNames;

        private List<string> targetTransforms = new List<string>();
        private string[] seperators = new string[] { " ", ",", ";" };

        private Dictionary<string,Vector2> origsSale = new Dictionary<string, Vector2>();
       // private List<String> textNameList = new List<string>{ "_MainTex", "_ParallaxMap", "_Emissive" , "_EmissionMap", "_MetallicGlossMap", "_OcclusionMap", "_SpecGlossMap", "_BumpMap"};
        private List<String> textNameList = new List<string>{ "_MainTex" , "_BumpMap" };
        private bool isInit = false;

        internal Vector2 initialTileing = Vector2.zero;

        public void Start()
        {
            Log.Normal("Start called");
            //setTexture();
            if (String.IsNullOrEmpty(textureTransformNames))
            {
                DestroyImmediate(this);
                return;
            }
            targetTransforms = textureTransformNames.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();
            GetScales();
        }

        public override void StaticObjectUpdate()
        {
            //Log.Normal("StaticObjectUpdate Called");
            TileTexture();
        }

        internal void GetScales()
        {
            if (isInit)
            {
                return;
            }
            isInit = true;

            Log.Normal("GetScales: called");

            int index = 0;
            foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!textureTransformNames.Equals("Any", StringComparison.CurrentCultureIgnoreCase) && !targetTransforms.Contains(renderer.transform.name))
                {
                    continue;
                }
                if (initialTileing == Vector2.zero)
                {
                    origsSale.Add(renderer.transform.name + "_" + index.ToString(), renderer.material.GetTextureScale("_MainTex"));
                }
                else
                {
                    origsSale.Add(renderer.transform.name + "_" + index.ToString(), initialTileing);
                }
                index++;

            }

        }



        /// <summary>
        /// Sets the texture in all transforms of the right name
        /// </summary>
        internal void TileTexture()
        {
            Log.Normal("TileTexture: called");

            int index = 0;
            foreach (MeshRenderer renderer in gameObject.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!textureTransformNames.Equals("Any", StringComparison.CurrentCultureIgnoreCase) && !targetTransforms.Contains(renderer.transform.name))
                {
                    continue;
                }
                Vector2 origscale = origsSale[renderer.transform.name + "_" + index.ToString()];
                foreach (string texName in textNameList)
                {
                    if (renderer.material.GetTexture(texName))
                    {
                        renderer.material.SetTextureScale(texName, origscale * staticInstance.ModelScale);
                        Log.Normal("Setting: " + texName + " to: " + origscale * staticInstance.ModelScale + "  on: " + renderer.transform.name);
                    }
                }
                index++;
            }

        }
    }
}