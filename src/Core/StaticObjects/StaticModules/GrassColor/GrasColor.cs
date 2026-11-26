using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;
using KerbalKonstructs.UI;


namespace KerbalKonstructs
{
    public class GrasColor : StaticModule
    {

        public string GrasMeshName;
        public string GrasTextureImage = "BUILTIN:/terrain_grass00_new";
        public string UsePQSColor = "False";
        public string UseNormalMap = "False";
        public string GrasTextureNormalMap = null;

        internal bool usePQS = true;
        private bool useNormalMap = false;

        private bool isInitialized = false;
        private List<Material> grasMaterials = new List<Material>();

        private Color grasColor = Color.red;
        private Texture2D grasTexture = null;
        private string grasTextureName = "";

        private static Color defaultColor = new Color(0.640f, 0.728f, 0.171f, 0.729f);

        //private string defaultGrasTextureName = "BUILTIN:/terrain_grass00_new";

        private bool settingsParsed = false;

        public void Start()
        {
            ParseSettings();
        }

        /// <summary>
        /// Parses the cfg fields. Done in Start / lazily, so the fields are set regardless of Awake ordering.
        /// </summary>
        private void ParseSettings()
        {
            if (settingsParsed)
            {
                return;
            }
            settingsParsed = true;

            if (!bool.TryParse(UsePQSColor, out usePQS))
            {
                Log.UserWarning("GrasColor Module: could not parse UsePQSColor to bool: " + UsePQSColor);
            }
            if (!bool.TryParse(UseNormalMap, out useNormalMap))
            {
                Log.UserWarning("GrasColor Module: could not parse UseNormalMap to bool: " + UseNormalMap);
            }
        }

        public override void StaticObjectUpdate()
        {
            SetTexture();
        }


        /// <summary>
        /// Sets the texture in all transforms of the right name
        /// </summary>
        internal void SetTexture()
        {

            if (staticInstance.GrasColor == Color.clear)
            {
                return;
            }

            //Log.Normal("FlagDeclal: setTexture called");
            if (!isInitialized)
            {
                Initialize();
            }

            grasTextureName = staticInstance.GrasTexture;

            if (string.IsNullOrEmpty(grasTextureName))
            {
                //Log.Normal("String was emtpy");
                grasTextureName = GrasTextureImage;
            }

            grasColor = GetColor();

            grasTexture = KKGraphics.GetTexture(grasTextureName, false, 0 , true);

            foreach (Material material in grasMaterials)
            {

                material.SetColor("_Color", grasColor);
                if (grasTexture != null)
                {
                    //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                    material.mainTexture = grasTexture;

                    if (!string.IsNullOrEmpty(GrasTextureNormalMap))
                    {
                        material.SetTexture("_BumpMap", KKGraphics.GetTexture(GrasTextureNormalMap, true, 0, true));
                    }
                    //else
                    //{
                    //    Texture normalMap = KKGraphics.GetNormalMap(grasTexture);
                    //    material.SetTexture("_BumpMap", normalMap);
                    //}

                }
            }
        }


        internal void Initialize()
        {
            ParseSettings();
           
           FindModelGrasMaterials();
           


            isInitialized = true;
        }

        /// <summary>
        /// The color for the shader. An old blend color (alpha between 0 and 1) is converted to a multiplier
        /// (see LegacyGrassColorConverter). The instance value is not changed any more, so the GrassColor2 import
        /// reads the same value whichever module runs first.
        /// </summary>
        internal Color GetColor()
        {
            return LegacyGrassColorConverter.ToLegacyMultiplier(staticInstance.GrasColor, staticInstance.GrasTexture, GrasTextureImage);
        }

        /// <summary>
        /// Uses the PQS System to query the color of the undergound
        /// </summary>
        /// <param name="body"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>


        public void FindModelGrasMaterials()
        {
            Transform[] allTransforms = gameObject.transform.GetComponentsInChildren<Transform>(true).Where(x => x.name == GrasMeshName).ToArray();
            foreach (var transform in allTransforms)
            {
                transform.name = "KKGrass";
                Renderer grasRenderer = transform.GetComponent<Renderer>();
                grasMaterials.Add(grasRenderer.material);
                //grasRenderer.material.mainTexture = KKGraphics.GetTexture(GrasTextureImage);
                grasRenderer.material.mainTexture = KKGraphics.GetTexture(GrasTextureImage, false, 0, true);
                //grasRenderer.material.shader = Shader.Find("KSP/Scenery/Diffuse Multiply");
                grasRenderer.material.shader = KKGraphics.GetShader("KK/Diffuse_Multiply_Random");
            }
        }

    }

}