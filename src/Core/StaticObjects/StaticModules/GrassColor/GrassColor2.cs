using KerbalKonstructs.Core;
using KerbalKonstructs.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace KerbalKonstructs
{
    public class GrassColor2 : StaticModule
    {

        public string GrassMeshName = "Nix";

        public string DefaultNearGrassTexture = "BUILTIN:/terrain_grass00_new";
        public string DefaultNearGrassTiling = "4";

        public string DefaultFarGrassTexture = "BUILTIN:/terrain_grass00_new_detail";
        public string DefaultFarGrassTiling = "10";
        public string DefaultFarGrassBlendDistance = "100";

        public string DefaultBlendMaskTexture = "BUILTIN:/blackSquare";
        
        public string DefaultTarmacTexture = "BUILTIN:/ksc_exterior_terrain_asphalt";
        public string DefaultTarmacTiling = "10, 10";
        public string DefaultTarmacTileRandom = "False";
        public string DefaultTarmacColor = "1,1,1,1";

        public string DefaultThirdTexture = "";
        public string DefaultThirdTextureColor = "0,0,0,0";
        public string DefaultThirdTextureTiling = "1";
        public string DefaultThirdTextureTileRandom = "False";


        public string DefaultFourthTexture = "";
        public string DefaultFourthTextureColor = "0,0,0,0";
        public string DefaultFourthTextureTiling = "1";
        public string DefaultFourthTextureTileRandom = "False";

        public string MaterialOffset = "0";

        // internal variables below


        internal Color grassColor = new Color(0.576471f, 0.611765f, 0.392157f, 1.00000f);
        internal string nearGrassTextureName = null;
        internal string farGrassTextureName = null;
        internal Texture2D nearGrassTexture = null;
        internal Texture2D farGrassTexture = null;

        internal float nearGrassTiling = 0.2f;
        internal float farGrassTiling = 0.01f;
        internal float farGrassBlendDistance = 100;
        

        internal string tarmacTextureName = null;
        internal Texture2D tarmacTexture = null;
        internal Vector2 tarmacTiling = new Vector2(10, 10);
        internal Color tarmacColor = Color.white;
        internal bool tarmacTileRandom = false;

        internal Texture2D blendMaskTexture = null;
        internal string blendMaskTextureName = null;

        internal string thirdTextureName = null;
        internal Texture2D thirdTexture = null;
        internal Color thirdTextureColor = Color.clear;
        internal float thirdTextureTiling = 1;
        internal bool thirdTextureTileRandom = false;

        internal string fourthTextureName = null;
        internal Texture2D fourthTexture = null;
        internal Color fourthTextureColor = Color.clear;
        internal float fourthTextureTiling = 1;
        internal bool fourthTextureTileRandom = false;


        internal static Color defaultColor = new Color(0.576471f, 0.611765f, 0.392157f, 1.00000f);

        internal bool useLegacy = false;



        // private ones
        private bool isInitialized = false;
        private List<Renderer> grassRenderers = new List<Renderer>();
        internal List<Material> grassMaterials = new List<Material>();


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
        internal void ApplySettings()
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

            foreach (Material material in grassMaterials)
            {

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

                //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                material.SetTexture("_TarmacTexture", tarmacTexture);
                if (tarmacTileRandom)
                {
                    material.SetFloat("_TarmacTileRandom", 1f);
                }
                else
                {
                    material.SetFloat("_TarmacTileRandom", 0f);
                }
                material.SetColor("_TarmacColor", tarmacColor);
                if (tarmacTexture != null)
                {
                    material.SetTextureScale("_TarmacTexture", tarmacTiling);
                }

                //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                material.SetTexture("_BlendMaskTexture", blendMaskTexture);


                // third Texture (green)
                material.SetTexture("_ThirdTexture", thirdTexture);
                material.SetColor("_ThirdTextureColor", thirdTextureColor);
                material.SetFloat("_ThirdTextureTiling", thirdTextureTiling);
                if (thirdTextureTileRandom)
                {
                    material.SetFloat("_ThirdTextureTileRandom", 1f);
                }
                else
                {
                    material.SetFloat("_ThirdTextureTileRandom", 0);
                }

                // Fourth Texture (blue)
                material.SetTexture("_FourthTexture", fourthTexture);
                material.SetColor("_FourthTextureColor", fourthTextureColor);
                material.SetFloat("_FourthTextureTiling", fourthTextureTiling);
                if (fourthTextureTileRandom)
                {
                    material.SetFloat("_FourthTextureTileRandom", 1f);
                }
                else
                {
                    material.SetFloat("_FourthTextureTileRandom", 0);
                }

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



        public void FindModelGrasMaterials()
        {
            Transform[] allTransforms = gameObject.transform.GetComponentsInChildren<Transform>(true).Where(x => x.name == GrassMeshName).ToArray();
            foreach (var transform in allTransforms)
            {
                Renderer grassRenderer = transform.GetComponent<Renderer>();

                grassRenderers.Add(grassRenderer);
                grassMaterials.Add(grassRenderer.material);
                grassRenderer.material.shader = KKGraphics.GetShader("KK/Ground_Multi_NoUV");
                

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
                        material.shader = KKGraphics.GetShader("KK/Ground_Multi_NoUV");

                        grassRenderers.Add(renderer);
                        grassMaterials.Add(material);

                        tarmacTiling = renderer.material.GetTextureScale("_TarmacTexture");
                        GrassMeshName = renderer.transform.name;
                        break;

                    }
                }
            }
        }


        public void ReadConfig()
        {

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
                if (staticInstance.GrasColor != Color.clear)
                {

                    Log.Normal("Loading Legacy Grass Config" + staticInstance.gameObject.name);
                    //useLegacy = true;
                    grassColor = staticInstance.GrasColor;
                    string oldGrassTexture = staticInstance.GrasTexture;
                    if (String.IsNullOrEmpty(staticInstance.GrasTexture) ||staticInstance.GrasTexture == "BUILTIN:/terrain_grass00_new")
                    {
                        oldGrassTexture = "KerbalKonstructs/Assets/Colors/legacyGrassColors";
                    }
                    if (String.IsNullOrEmpty(staticInstance.GrasTexture))
                    {
                        staticInstance.GrasTexture = "BUILTIN:/terrain_grass00_new_detail";
                    }

                    //farGrassTiling = 10;
                    //nearGrassTiling = 1;
                    nearGrassTextureName = staticInstance.GrasTexture;
                    farGrassTextureName = staticInstance.GrasTexture;

                    
                    grassColor = GrassColorUtils.ManualCalcNewColor(staticInstance.GrasColor, oldGrassTexture, staticInstance.GrasTexture);

                    staticInstance.grassColor2Configs.Add(GiveConfig());

                }

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


                    break;
                }



            }
        }

          

        public ConfigNode GiveConfig()
        {
            ConfigNode grassNode = new ConfigNode("GrassColor2");

            grassNode.AddValue("GrassMeshName", GrassMeshName);
            grassNode.AddValue("MaterialOffset", MaterialOffset);

            grassNode.AddValue("GrassColor", grassColor);

            if (nearGrassTexture != null)
            {
                grassNode.AddValue("NearGrassTexture", nearGrassTextureName);
                grassNode.AddValue("NearGrassTiling", nearGrassTiling);
            }
            if (farGrassTexture != null)
            {
                grassNode.AddValue("FarGrassTexture", farGrassTextureName);
                grassNode.AddValue("FarGrassTiling", farGrassTiling);
                grassNode.AddValue("FarGrassBlendDistance", farGrassBlendDistance);
            }
            if (tarmacTexture != null)
            {
                grassNode.AddValue("TarmacTexture", tarmacTextureName);
                grassNode.AddValue("TarmacColor", tarmacColor);
                grassNode.AddValue("TarmacTiling", tarmacTiling);
                grassNode.AddValue("TarmacTileRandom", tarmacTileRandom);
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
            }
            if (fourthTexture != null) {
                grassNode.AddValue("FourthTexture",fourthTextureName);
                grassNode.AddValue("FourthTextureTiling",fourthTextureTiling);
                grassNode.AddValue("FourthTextureColor",fourthTextureColor);
                grassNode.AddValue("FourthTextureTileRandom",fourthTextureTileRandom);
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