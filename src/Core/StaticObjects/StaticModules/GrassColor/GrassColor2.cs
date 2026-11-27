using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KerbalKonstructs.UI2;


namespace KerbalKonstructs
{
    public class GrassColor2 : StaticModule
    {

        public string GrassMeshName = "Nix";

        public string DefaultNearGrassTexture = "BUILTIN:/terrain_grass00_new";
        public string DefaultNearGrassTiling = "0.03";
        public string DefaultNearGrassTextureGray = "False";

        public string DefaultFarGrassTexture = "BUILTIN:/terrain_grass00_new_detail";
        public string DefaultFarGrassTiling = "0.01";
        public string DefaultFarGrassTextureGray = "False";
        public string DefaultFarGrassBlendDistance = "100";

        public string DefaultBlendMaskTexture = "BUILTIN:/blackSquare";
        
        public string DefaultTarmacTexture = "BUILTIN:/ksc_exterior_terrain_asphalt";
        public string DefaultTarmacTiling = "10, 10";
        public string DefaultTarmacTileRandom = "False";
        public string DefaultTarmacColor = "1,1,1,1";
        public string DefaultTarmacTextureGray = "False";

        public string DefaultThirdTexture = "";
        public string DefaultThirdTextureColor = "0,0,0,0";
        public string DefaultThirdTextureTiling = "1";
        public string DefaultThirdTextureTileRandom = "False"; 
        public string DefaultThirdTextureGray = "False";


        public string DefaultFourthTexture = "";
        public string DefaultFourthTextureColor = "0,0,0,0";
        public string DefaultFourthTextureTiling = "1";
        public string DefaultFourthTextureTileRandom = "False";
        public string DefaultFourthTextureGray = "False";

        public string MaterialOffset = "0";

        // set by LoadModels() when this module replaces a legacy GrasColor module: that module's GrasTextureImage
        public string LegacyGrasTextureImage = "";

        // internal variables below


        internal Color grassColor = new Color(0.576471f, 0.611765f, 0.392157f, 1.00000f);
        internal string nearGrassTextureName = null;
        internal string farGrassTextureName = null;
        internal Texture2D nearGrassTexture = null;
        internal Texture2D farGrassTexture = null;
        
        internal float nearGrassTiling = 0.2f;
        internal float farGrassTiling = 0.01f;
        internal float farGrassBlendDistance = 100;
        internal bool nearGrassTextureGray = false;
        internal bool farGrassTextureGray = false;


        internal string tarmacTextureName = null;
        internal Texture2D tarmacTexture = null;
        internal Vector2 tarmacTiling = new Vector2(10, 10);
        internal Color tarmacColor = Color.white;
        internal bool tarmacTileRandom = false;
        internal bool tarmacTextureGray = false;

        internal Texture2D blendMaskTexture = null;
        internal string blendMaskTextureName = null;

        internal string thirdTextureName = null;
        internal Texture2D thirdTexture = null;
        internal Color thirdTextureColor = Color.clear;
        internal float thirdTextureTiling = 1;
        internal bool thirdTextureTileRandom = false;
        internal bool thirdTextureGray = false;

        internal string fourthTextureName = null;
        internal Texture2D fourthTexture = null;
        internal Color fourthTextureColor = Color.clear;
        internal float fourthTextureTiling = 1;
        internal bool fourthTextureTileRandom = false;
        internal bool fourthTextureGray = false;


        internal static Color defaultColor = new Color(0.576471f, 0.611765f, 0.392157f, 1.00000f);

        internal bool useLegacy = false;



        // private ones
        private bool isInitialized = false;
        private List<Renderer> grassRenderers = new List<Renderer>();
        internal List<Material> grassMaterials = new List<Material>();


        private static Texture2D _flatNormalTexture = null;

        /// <summary>
        /// Workaround for KK/Ground_Multi_NoUV until the shader is fixed: it samples its normal textures as raw colors
        /// and adds (color * 2 - 1) to the world space normal. The normal textures are never set (and have no "bump"
        /// default), so the lighting normal was bent away from the surface and the grass was too dark in sunlight.
        /// A texture of exactly 0.5 gray gives (0, 0, 0): the shader then uses the mesh normal.
        /// </summary>
        internal static Texture2D flatNormalTexture
        {
            get
            {
                if (_flatNormalTexture == null)
                {
                    // linear: the 0.5 must not be changed by a color space conversion
                    _flatNormalTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
                    _flatNormalTexture.name = "KKFlatNormalWorkaround";
                    _flatNormalTexture.SetPixels32(new Color32[] { new Color32(128, 128, 128, 128) });
                    _flatNormalTexture.Apply(false, true);
                }
                return _flatNormalTexture;
            }
        }


        /// <summary>
        /// Sets all normal textures of the ground shader to the flat workaround texture (see flatNormalTexture)
        /// </summary>
        internal static void SetFlatNormals(Material material)
        {
            // the fixed shader (marked by _KKNormalFix) unpacks its normal maps and defaults to flat ones:
            // the workaround texture would be read as a tilted normal there, so leave its normal textures alone
            if (material == null || material.HasProperty("_KKNormalFix"))
            {
                return;
            }
            Texture2D flat = flatNormalTexture;
            material.SetTexture("_FarGrassNormalTexture", flat);
            material.SetTexture("_TarmacNormalTexture", flat);
            material.SetTexture("_ThirdNormalTexture", flat);
            material.SetTexture("_FourthNormalTexture", flat);
        }


        public void Awake()
        {
            if (staticInstance != null)
            {
                ApplySettings();
            }
        }



        public override void StaticObjectUpdate()
        {
            ApplySettings();
        }


        /// <summary>
        /// Sets the texture in all transforms of the right name
        /// </summary>
        internal virtual void ApplySettings()
        {

            if (!isInitialized)
            {
                Initialize();
            }

            if (grassColor == Color.clear)
            {
                return;
            }

            nearGrassTexture = KKGraphics.GetTexture(nearGrassTextureName);
            farGrassTexture = KKGraphics.GetTexture(farGrassTextureName);
            tarmacTexture = KKGraphics.GetTexture(tarmacTextureName);
            blendMaskTexture = KKGraphics.GetTexture(blendMaskTextureName);

            if (!string.IsNullOrEmpty(thirdTextureName))
            {
                thirdTexture = KKGraphics.GetTexture(thirdTextureName);
            }
            if (!string.IsNullOrEmpty(fourthTextureName))
            {
                fourthTexture = KKGraphics.GetTexture(fourthTextureName);
            }
  

            //Material material = grassMaterials.ElementAt(matOffset);

            for (int i = 0; i < grassMaterials.Count; i++)
            {
                Material material = grassMaterials[i];
                SetFlatNormals(material);

                //if (useLegacy)
                //{
                //    renderer.material.SetFloat("_TilingOffset", tilingOffset);
                //}

                material.SetColor("_GrassColor", grassColor);
                material.SetColor("_TarmacColor", tarmacColor);
                //if (!staticInstance.model.isSquad)
                //{
                material.SetFloat("_NearGrassTiling", nearGrassTiling);
                material.SetFloat("_FarGrassTiling", farGrassTiling);

                material.SetFloat("_FarGrassBlendDistance", farGrassBlendDistance);


                //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                material.SetTexture("_NearGrassTexture", nearGrassTexture);
                //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                material.SetTexture("_FarGrassTexture", farGrassTexture);
                //if (farGrassTexture != null)
                //{
                //    material.SetTexture("_FarGrassNormalTexture", KKGraphics.GetNormalMap(farGrassTexture));
                //}
                //else
                //{
                //    material.SetTexture("_FarGrassNormalTexture", null);
                //}

                //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                material.SetTexture("_TarmacTexture", tarmacTexture);
                if (tarmacTexture != null)
                {
                    //material.SetTexture("_TarmacNormalTexture", KKGraphics.GetNormalMap(tarmacTexture));
                    material.SetTextureScale("_TarmacTexture", tarmacTiling);
                    material.SetFloat("_TarmacTileRandom", tarmacTileRandom ? 1f : 0);
                }
                else
                {
                    //material.SetTexture("_TarmacNormalTexture", null);
                    material.SetFloat("_TarmacTileRandom", 0);
                }

                material.SetColor("_TarmacColor", tarmacColor);


                //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                material.SetTexture("_BlendMaskTexture", blendMaskTexture);


                // third Texture (green)
                material.SetTexture("_ThirdTexture", thirdTexture);
                if (thirdTexture != null)
                {
                    //material.SetTexture("_ThirdNormalTexture", KKGraphics.GetNormalMap(thirdTexture));
                    material.SetFloat("_ThirdTextureTileRandom", thirdTextureTileRandom ? 1f : 0);
                }
                else
                {
                    //material.SetTexture("_ThirdNormalTexture", null);
                    material.SetFloat("_ThirdTextureTileRandom", 0);
                }

                material.SetColor("_ThirdTextureColor", thirdTextureColor);
                material.SetFloat("_ThirdTextureTiling", thirdTextureTiling);
                

                // Fourth Texture (blue)
                material.SetTexture("_FourthTexture", fourthTexture);
                if (fourthTexture != null)
                {
                    material.SetFloat("_FourthTextureTileRandom", fourthTextureTileRandom ? 1f : 0f);
                    //material.SetTexture("_FourthNormalTexture", KKGraphics.GetNormalMap(fourthTexture));
                }
                else
                {
                    material.SetFloat("_FourthTextureTileRandom", 0);
                    //material.SetTexture("_FourthNormalTexture", null);
                }

                material.SetColor("_FourthTextureColor", fourthTextureColor);
                material.SetFloat("_FourthTextureTiling", fourthTextureTiling);


                material.SetFloat("_NearGrassGrayScale", nearGrassTextureGray ? 1f : 0);
                material.SetFloat("_FarGrassGrayScale", farGrassTextureGray ? 1f : 0);
                material.SetFloat("_TarmacGrayScale", tarmacTextureGray ? 1f : 0);
                material.SetFloat("_ThirdTextureGrayScale", thirdTextureGray ? 1f : 0);
                material.SetFloat("_FourthTextureGrayScale", fourthTextureGray ? 1f : 0);
            }
        }


        internal void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            ReadConfig();

            if (staticInstance.model.isSquad)
            {
                FindSquadGrasMaterial();
            }
            else
            {
                FindModelGrasMaterials();
            }


            isInitialized = true;
        }



        /// <summary>
        /// The shader of the grass meshes
        /// </summary>
        protected virtual Shader GroundShader => KKTerrainShader.OldGroundShader;

        /// <summary>
        /// The renderers of the grass meshes, grassRenderers[i] holds grassMaterials[i]
        /// </summary>
        internal List<Renderer> GrassRenderers => grassRenderers;

        /// <summary>
        /// The instance's GrassColor2 config node that ReadConfig() used (null when there was none)
        /// </summary>
        protected ConfigNode instanceConfig = null;

        /// <summary>
        /// Called when the user sets the grass color in the editor or by a preset
        /// </summary>
        internal virtual void OnUserColorChange()
        {
        }

        /// <summary>
        /// True: an imported legacy instance without a color gets the ground color underneath as grass color
        /// </summary>
        protected virtual bool ImportsGroundColor => true;

        public void FindModelGrasMaterials()
        {
            Transform[] allTransforms = gameObject.transform.GetComponentsInChildren<Transform>(true).Where(x => x.name == GrassMeshName).ToArray();
            foreach (var transform in allTransforms)
            {
                Renderer grassRenderer = transform.GetComponent<Renderer>();

                grassRenderers.Add(grassRenderer);
                grassMaterials.Add(grassRenderer.material);
                grassRenderer.material.shader = GroundShader;
                

            }
        }

        public void FindSquadGrasMaterial()
        {
            Transform[] allTransforms = gameObject.transform.GetComponentsInChildren<Transform>(true).Where(x => x.name == GrassMeshName).ToArray();
            foreach (var transform in allTransforms)
            {
                var renderers = transform.GetComponents<MeshRenderer>();

                foreach (var renderer in renderers)
                {
                    foreach (Material material in renderer.materials.Where(mat => mat.shader.name.StartsWith("KSP/Scenery/Diffuse Ground KSC")))
                    {
                        material.shader = GroundShader;

                        grassRenderers.Add(renderer);
                        grassMaterials.Add(material);

                        tarmacTiling = renderer.material.GetTextureScale("_TarmacTexture");
                        GrassMeshName = renderer.transform.name;
                        break;

                    }
                }
            }
        }


        /// <summary>
        /// Converts the instance's legacy GrasColor / GrasTexture values with LegacyGrassColorConverter, and stores the
        /// result as this instance's GrassColor2 config (so it is converted only once and saved with the instance).
        /// Converted legacy modules (LegacyGrasTextureImage set): the legacy texture is the near and far grass texture,
        /// tiling 1, blend distance 200; an instance without a legacy color gets the color of the ground underneath.
        /// </summary>
        private void ImportLegacyValues()
        {
            bool isConvertedModule = !string.IsNullOrEmpty(LegacyGrasTextureImage);

            if (staticInstance.GrasColor == Color.clear && !isConvertedModule)
            {
                // nothing to import: model defaults
                return;
            }

            string legacyTexture = staticInstance.GrasTexture;
            string grasTextureName;
            if (isConvertedModule)
            {
                grasTextureName = LegacyGrassColorConverter.ShownLegacyTexture(legacyTexture, LegacyGrasTextureImage);
                nearGrassTiling = LegacyGrassColorConverter.convertedTiling;
                farGrassTiling = LegacyGrassColorConverter.convertedTiling;
                farGrassBlendDistance = LegacyGrassColorConverter.convertedBlendDistance;
            }
            else
            {
                grasTextureName = LegacyGrassColorConverter.GrassColor2Texture(legacyTexture);
            }
            nearGrassTextureName = grasTextureName;
            farGrassTextureName = grasTextureName;

            if (staticInstance.GrasColor == Color.clear && !ImportsGroundColor)
            {
                grassColor = defaultColor;
            }
            else if (staticInstance.GrasColor == Color.clear)
            {
                Color ground;
                try
                {
                    ground = KerbalKonstructs.useCam ? GrasColorCam.instance.GetCameraColor(staticInstance)
                        : GrassColorUtils.GetSurfaceColorPQS(staticInstance.CelestialBody, staticInstance.RefLatitude, staticInstance.RefLongitude);
                }
                catch (Exception e)
                {
                    Log.UserWarning("GrassColor2: could not get the ground color for " + staticInstance.gameObject.name + ": " + e.Message);
                    ground = Color.clear;
                }
                grassColor = (ground == Color.clear) ? defaultColor
                    : LegacyGrassColorConverter.FromGroundColor(ground, nearGrassTextureName, nearGrassTextureGray, farGrassTextureName, farGrassTextureGray);
                Log.Normal("Loading Legacy Grass Config " + staticInstance.gameObject.name + ": no legacy color, ground color " +
                    ConfigNode.WriteColor(grassColor) + " on " + grasTextureName);
            }
            else
            {
                grassColor = LegacyGrassColorConverter.ToGrassColor2(staticInstance.GrasColor, legacyTexture,
                    nearGrassTextureName, nearGrassTextureGray, farGrassTextureName, farGrassTextureGray, LegacyGrasTextureImage);
                Log.Normal("Loading Legacy Grass Config " + staticInstance.gameObject.name + ": " +
                    LegacyGrassColorConverter.Describe(staticInstance.GrasColor, legacyTexture, grassColor, grasTextureName));
            }

            staticInstance.grassColor2Configs.Add(GiveConfig());
        }


        public void ReadConfig()
        {
            instanceConfig = null;

            if (staticInstance.grassColor2Configs.Count == 0)
            {
                
                nearGrassTextureName = DefaultNearGrassTexture;
                farGrassTextureName = DefaultFarGrassTexture;
                tarmacTextureName = DefaultTarmacTexture;
                blendMaskTextureName = DefaultBlendMaskTexture;
                nearGrassTiling = float.Parse(DefaultNearGrassTiling);
                farGrassTiling = float.Parse(DefaultFarGrassTiling);
                farGrassBlendDistance = float.Parse(DefaultFarGrassBlendDistance);
                tarmacTiling = ConfigNode.ParseVector2(DefaultTarmacTiling);
                tarmacTileRandom = bool.Parse(DefaultTarmacTileRandom);
                grassColor = defaultColor;
                tarmacColor = Color.white;

                nearGrassTextureGray = bool.Parse(DefaultNearGrassTextureGray);
                farGrassTextureGray = bool.Parse(DefaultFarGrassTextureGray);
                tarmacTextureGray = bool.Parse(DefaultTarmacTextureGray);
                thirdTextureGray = bool.Parse(DefaultThirdTextureGray);
                fourthTextureGray = bool.Parse(DefaultFourthTextureGray);

                //Log.Normal("MaterialOffset:" + MaterialOffset);


                thirdTextureName = DefaultThirdTexture;
                thirdTextureTiling = float.Parse(DefaultThirdTextureTiling);
                thirdTextureColor =  ConfigNode.ParseColor(DefaultThirdTextureColor);
                thirdTextureTileRandom = bool.Parse(DefaultThirdTextureTileRandom);

                fourthTextureName = DefaultFourthTexture;
                fourthTextureTiling = float.Parse(DefaultFourthTextureTiling);
                fourthTextureColor = ConfigNode.ParseColor(DefaultFourthTextureColor);
                fourthTextureTileRandom = bool.Parse(DefaultFourthTextureTileRandom);


                // try to import legacy values
                ImportLegacyValues();

                return;
            }

            foreach (ConfigNode grassConfig in staticInstance.grassColor2Configs)
            {
                //Log.Normal("found instance cfgnode");
                if (grassConfig.GetValue("GrassMeshName") == GrassMeshName)
                {
                    if (grassConfig.HasValue("MaterialOffset") && (grassConfig.GetValue("MaterialOffset") != MaterialOffset))
                    {
                        continue;
                    }

                    //Log.Normal("found instance grassnode");
                    instanceConfig = grassConfig;
                        grassColor = (grassConfig.HasValue("GrassColor")) ? ConfigNode.ParseColor(grassConfig.GetValue("GrassColor")) : defaultColor;

                    nearGrassTextureName = (grassConfig.HasValue("NearGrassTexture")) ? grassConfig.GetValue("NearGrassTexture") : DefaultNearGrassTexture;
                    farGrassTextureName = (grassConfig.HasValue("FarGrassTexture")) ? grassConfig.GetValue("FarGrassTexture") : DefaultFarGrassTexture;
                    
                    nearGrassTiling = (grassConfig.HasValue("NearGrassTiling")) ? float.Parse(grassConfig.GetValue("NearGrassTiling")) : 1f;
                    farGrassTiling = (grassConfig.HasValue("FarGrassTiling")) ? float.Parse(grassConfig.GetValue("FarGrassTiling")) : 1f;
                    farGrassBlendDistance = (grassConfig.HasValue("FarGrassBlendDistance")) ? float.Parse(grassConfig.GetValue("FarGrassBlendDistance")) : 50f;

                    blendMaskTextureName = (grassConfig.HasValue("BlendMaskTexture")) ? grassConfig.GetValue("BlendMaskTexture") : DefaultBlendMaskTexture;

                    tarmacTextureName = (grassConfig.HasValue("TarmacTexture")) ? grassConfig.GetValue("TarmacTexture") : DefaultTarmacTexture;
                    tarmacColor = (grassConfig.HasValue("TarmacColor")) ? ConfigNode.ParseColor(grassConfig.GetValue("TarmacColor")) : Color.white;
                    tarmacTiling = (grassConfig.HasValue("TarmacTiling")) ? ConfigNode.ParseVector2(grassConfig.GetValue("TarmacTiling")) : new Vector2(10,10);
                    tarmacTileRandom = (grassConfig.HasValue("TarmacTileRandom")) ? Boolean.Parse(grassConfig.GetValue("TarmacTileRandom")) : false;

                    thirdTextureName = (grassConfig.HasValue("ThirdTexture")) ? grassConfig.GetValue("ThirdTexture") : "";
                    thirdTextureTiling = (grassConfig.HasValue("ThirdTextureTiling")) ? float.Parse(grassConfig.GetValue("ThirdTextureTiling")) : 1f;
                    thirdTextureColor = (grassConfig.HasValue("ThirdTextureColor")) ? ConfigNode.ParseColor(grassConfig.GetValue("ThirdTextureColor")) : new Color(1,1,1,1);
                    thirdTextureTileRandom = (grassConfig.HasValue("ThirdTextureTileRandom")) ? Boolean.Parse(grassConfig.GetValue("ThirdTextureTileRandom")) : false;

                    fourthTextureName = (grassConfig.HasValue("FourthTexture")) ? grassConfig.GetValue("FourthTexture") : "";
                    fourthTextureTiling = (grassConfig.HasValue("FourthTextureTiling")) ? float.Parse(grassConfig.GetValue("FourthTextureTiling")) : 1f;
                    fourthTextureColor = (grassConfig.HasValue("FourthTextureColor")) ? ConfigNode.ParseColor(grassConfig.GetValue("FourthTextureColor")) : new Color(1, 1, 1, 1);
                    fourthTextureTileRandom = (grassConfig.HasValue("FourthTextureTileRandom")) ? Boolean.Parse(grassConfig.GetValue("FourthTextureTileRandom")) : false;


                    nearGrassTextureGray = grassConfig.HasValue("NearGrassGrayScale") ? bool.Parse(grassConfig.GetValue("NearGrassGrayScale")): false;
                    farGrassTextureGray = grassConfig.HasValue("FarGrassGrayScale") ? bool.Parse(grassConfig.GetValue("FarGrassGrayScale")) : false;
                    tarmacTextureGray = grassConfig.HasValue("TarmacGrayScale") ? bool.Parse(grassConfig.GetValue("TarmacGrayScale")) : false;
                    thirdTextureGray = grassConfig.HasValue("ThirdTextureGrayScale") ? bool.Parse(grassConfig.GetValue("ThirdTextureGrayScale")) : false;
                    fourthTextureGray = grassConfig.HasValue("FourthTextureGrayScale") ? bool.Parse(grassConfig.GetValue("FourthTextureGrayScale")) : false;

                    break;
                }



            }
        }

          

        public virtual ConfigNode GiveConfig()
        {
            ConfigNode grassNode = new ConfigNode("GrassColor2");

            grassNode.AddValue("GrassMeshName", GrassMeshName);
            grassNode.AddValue("MaterialOffset", MaterialOffset);

            grassNode.AddValue("GrassColor", grassColor);

            if (nearGrassTexture != null)
            {
                grassNode.AddValue("NearGrassTexture", nearGrassTextureName);
                grassNode.AddValue("NearGrassTiling", nearGrassTiling);
                grassNode.AddValue("NearGrassGrayScale", nearGrassTextureGray);
            }
            if (farGrassTexture != null)
            {
                grassNode.AddValue("FarGrassTexture", farGrassTextureName);
                grassNode.AddValue("FarGrassTiling", farGrassTiling);
                grassNode.AddValue("FarGrassBlendDistance", farGrassBlendDistance);
                grassNode.AddValue("FarGrassGrayScale", farGrassTextureGray);
            }
            if (tarmacTexture != null)
            {
                grassNode.AddValue("TarmacTexture", tarmacTextureName);
                grassNode.AddValue("TarmacColor", tarmacColor);
                grassNode.AddValue("TarmacTiling", tarmacTiling);
                grassNode.AddValue("TarmacTileRandom", tarmacTileRandom);
                grassNode.AddValue("TarmacGrayScale", tarmacTextureGray);
            }

            if (blendMaskTexture != null)
            {
                grassNode.AddValue("BlendMaskTexture", blendMaskTextureName);
            }

            if (thirdTexture != null) {
                grassNode.AddValue("ThirdTexture",thirdTextureName);
                grassNode.AddValue("ThirdTextureTiling",thirdTextureTiling);
                grassNode.AddValue("ThirdTextureColor",thirdTextureColor);
                grassNode.AddValue("ThirdTextureTileRandom",thirdTextureTileRandom);
                grassNode.AddValue("ThirdTextureGrayScale", thirdTextureGray);
            }
            if (fourthTexture != null) {
                grassNode.AddValue("FourthTexture",fourthTextureName);
                grassNode.AddValue("FourthTextureTiling",fourthTextureTiling);
                grassNode.AddValue("FourthTextureColor",fourthTextureColor);
                grassNode.AddValue("FourthTextureTileRandom",fourthTextureTileRandom);
                grassNode.AddValue("FourthTextureGrayScale", fourthTextureGray);
            }

            return grassNode;
        }


        internal void UpdateCallBack(GrassColorPresetUI2.ColorPreset2 preset)
        {

            if (KKGraphics.GetTexture(preset.nearGrassTexture) != null)
            {
                //Log.Normal("Updating Texture to: " + newTexture);
                nearGrassTextureName = preset.nearGrassTexture;
            }

            if (KKGraphics.GetTexture(preset.farGrassTexture) != null)
            {
                //Log.Normal("Updating Texture to: " + newTexture);
                farGrassTextureName = preset.farGrassTexture;
            }

            if (KKGraphics.GetTexture(preset.tarmacTexture) != null)
            {
                //Log.Normal("Updating Texture to: " + newTexture);
                tarmacTextureName = preset.tarmacTexture;
            }


            grassColor = preset.grassColor;
            tarmacColor = preset.tarmacColor;

            ApplySettings();

        }

    }
}