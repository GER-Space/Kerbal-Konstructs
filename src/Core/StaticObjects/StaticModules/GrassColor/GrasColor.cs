using System;
using System.Collections.Generic;
using System.Linq;
using KerbalKonstructs.Core;
using UnityEngine;


namespace KerbalKonstructs
{
    /// <summary>
    /// Legacy grass module (one color and one texture per instance: StaticInstance.GrasColor / GrasTexture).
    /// It now renders with the GrassColor2 shader (KK/Ground_Multi_NoUV): the legacy values are converted by
    /// LegacyGrassColorConverter, so the grass keeps its average color. An instance without a color gets the color
    /// of the ground underneath. The saved values stay in the legacy format and are converted the same way on every load.
    /// </summary>
    public class GrasColor : StaticModule
    {

        public string GrasMeshName;
        public string GrasTextureImage = "BUILTIN:/terrain_grass00_new";
        public string UsePQSColor = "False";
        public string UseNormalMap = "False";
        public string GrasTextureNormalMap = null;

        // GrassColor2 shader settings (optional in the model config). The shader places the texture by position,
        // not by the mesh UVs.
        public string NearGrassTiling = "1.0";
        public string FarGrassTiling = "1.0";
        public string FarGrassBlendDistance = "100";

        internal const string shaderName = "KK/Ground_Multi_NoUV";
        internal const string blendMaskTexture = "BUILTIN:/blackSquare";

        internal bool usePQS = true;
        private bool useNormalMap = false;

        private bool isInitialized = false;
        private List<Material> grasMaterials = new List<Material>();

        private Color grasColor = Color.red;
        private Texture2D grasTexture = null;
        private string grasTextureName = "";

        private float nearTiling = 1f;
        private float farTiling = 1f;
        private float farBlendDistance = 100f;

        // the ground color is only sampled again when the static moved
        private const float groundResampleDistance = 5f;
        private bool hasGroundColor = false;
        private Vector3 groundColorPosition;
        private string groundColorTexture = null;
        private Color groundColor;

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
            nearTiling = ParseFloat(NearGrassTiling, 1f, "NearGrassTiling");
            farTiling = ParseFloat(FarGrassTiling, 1f, "FarGrassTiling");
            farBlendDistance = ParseFloat(FarGrassBlendDistance, 100f, "FarGrassBlendDistance");
        }

        private static float ParseFloat(string value, float fallback, string fieldName)
        {
            float result;
            if (MiscUtils.TryParseFloat(value, out result))
            {
                return result;
            }
            Log.UserWarning("GrasColor Module: could not parse " + fieldName + ": " + value);
            return fallback;
        }

        public override void StaticObjectUpdate()
        {
            SetTexture();
        }


        /// <summary>
        /// Sets the GrassColor2 shader values in all grass materials
        /// </summary>
        internal void SetTexture()
        {
            if (!isInitialized)
            {
                Initialize();
            }

            grasTextureName = LegacyGrassColorConverter.ShownLegacyTexture(staticInstance.GrasTexture, GrasTextureImage);
            grasTexture = KKGraphics.GetTexture(grasTextureName, false, 0, true);
            if (grasTexture == null)
            {
                Log.UserWarning("GrasColor Module: texture not found: " + grasTextureName + " on " + staticInstance.gameObject.name);
            }

            grasColor = GetColor();

            Texture2D blendMask = KKGraphics.GetTexture(blendMaskTexture);

            foreach (Material material in grasMaterials)
            {
                material.SetColor("_GrassColor", grasColor);

                // the legacy texture is the near and the far grass texture
                if (grasTexture != null)
                {
                    material.SetTexture("_NearGrassTexture", grasTexture);
                    material.SetTexture("_FarGrassTexture", grasTexture);
                }
                material.SetFloat("_NearGrassTiling", nearTiling);
                material.SetFloat("_FarGrassTiling", farTiling);
                material.SetFloat("_FarGrassBlendDistance", farBlendDistance);
                material.SetFloat("_NearGrassGrayScale", 0f);
                material.SetFloat("_FarGrassGrayScale", 0f);

                // grass only: no tarmac, third or fourth texture
                material.SetTexture("_BlendMaskTexture", blendMask);
                material.SetColor("_ThirdTextureColor", Color.clear);
                material.SetColor("_FourthTextureColor", Color.clear);
            }
        }


        internal void Initialize()
        {
            ParseSettings();

            FindModelGrasMaterials();

            if (!string.IsNullOrEmpty(GrasTextureNormalMap))
            {
                Log.Normal("GrasColor Module: the normal map " + GrasTextureNormalMap + " is not used by the GrassColor2 shader");
            }

            isInitialized = true;
        }


        /// <summary>
        /// The _GrassColor for the shader: the converted legacy color, or (no color set) the ground color underneath.
        /// Does not change the instance's values.
        /// </summary>
        internal Color GetColor()
        {
            if (staticInstance.GrasColor == Color.clear)
            {
                return GetGroundColor();
            }
            return LegacyGrassColorConverter.ToGrassColor2(staticInstance.GrasColor, staticInstance.GrasTexture,
                grasTextureName, false, grasTextureName, false, GrasTextureImage);
        }


        /// <summary>
        /// The color of the ground underneath as a multiplier for the grass texture (the grass then has the ground's
        /// average color). Sampled again only when the static moved or the texture changed.
        /// </summary>
        private Color GetGroundColor()
        {
            Vector3 position = staticInstance.gameObject.transform.position;
            if (hasGroundColor && groundColorTexture == grasTextureName && (position - groundColorPosition).sqrMagnitude < groundResampleDistance * groundResampleDistance)
            {
                return groundColor;
            }

            Color ground;
            try
            {
                if (KerbalKonstructs.useCam)
                {
                    ground = GrasColorCam.instance.GetCameraColor(staticInstance);
                }
                else
                {
                    ground = GrassColorUtils.GetSurfaceColorPQS(staticInstance.CelestialBody, staticInstance.RefLatitude, staticInstance.RefLongitude);
                }
            }
            catch (Exception e)
            {
                Log.UserWarning("GrasColor Module: could not get the ground color for " + staticInstance.gameObject.name + ": " + e.Message);
                return GrassColor2.defaultColor;
            }

            groundColor = LegacyGrassColorConverter.FromGroundColor(ground, grasTextureName, false, grasTextureName, false);
            groundColorPosition = position;
            groundColorTexture = grasTextureName;
            hasGroundColor = true;
            return groundColor;
        }


        // Compatibility note: Parallax patches this method by name (and the KK shader names) to replace the ground shader.
        public void FindModelGrasMaterials()
        {
            Shader shader = KKGraphics.GetShader(shaderName);
            Transform[] allTransforms = gameObject.transform.GetComponentsInChildren<Transform>(true).Where(x => x.name == GrasMeshName).ToArray();
            foreach (var transform in allTransforms)
            {
                transform.name = "KKGrass";
                Renderer grasRenderer = transform.GetComponent<Renderer>();
                if (grasRenderer == null)
                {
                    continue;
                }
                grasMaterials.Add(grasRenderer.material);
                grasRenderer.material.shader = shader;
            }
        }

    }

}
