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
        public string UsePQSColor = "True";

        public string DefaultNearGrassTexture = "BUILTIN:/terrain_grass00_new";
        public string DefaultFarGrassTexture = "BUILTIN:/terrain_grass00_new_detail";
        public string DefaultTarmacTexture = "BUILTIN:/ksc_exterior_terrain_asphalt";
        public string DefaultBlendMaskTexture = "BUILTIN:/blackSquare";

        public string DefaultNearGrassTiling = "4";
        public string DefaultFarGrassTiling = "10";
        public string DefaultFarGrassBlendDistance = "100";
        public string DefaultTarmacTiling = "10, 10";



        internal bool usePQS = false;
        private bool isInitialized = false;

        private List<Renderer> grassRenderers = new List<Renderer>();

        internal Color grassColor = Color.clear;
        internal Color tarmacColor = Color.white;

        internal Texture2D nearGrassTexture = null;
        internal Texture2D farGrassTexture = null;
        internal Texture2D blendMaskTexture = null;
        internal Texture2D tarmacTexture = null;

        internal string nearGrassTextureName = null;
        internal string farGrassTextureName = null;
        internal string blendMaskTextureName = null;
        internal string tarmacTextureName = null;

        internal Vector2 tarmacTiling = new Vector2(10, 10);

        internal float nearGrassTiling = 0.2f;
        internal float farGrassTiling = 0.01f;
        internal float farGrassBlendDistance = 100;

        internal static Color defaultColor = new Color(0.576471f, 0.611765f, 0.392157f, 1.00000f);
        internal static Color defaultTarmacColor = Color.white;

        internal bool useLegacy = false;

        internal int tilingOffset = 10;


        public void Awake()
        {
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

            //grassColor = GetColor();

            foreach (Renderer renderer in grassRenderers)
            {

                //if (useLegacy && renderer.material.shader.name.StartsWith("KSP/Scenery/Diffuse Ground KSC"))
                //{
                //    Material grassMaterial = new Material(KKGraphics.GetShader("KK/legacy_Ground_KSC"));
                //    grassMaterial.CopyPropertiesFromMaterial(renderer.material);
                //    renderer.SetPropertyBlock(null);

                //    Log.Normal("found localscale: " + renderer.transform.localScale);


                //    Vector2 blendmaskTiling = renderer.material.GetTextureScale("_BlendMaskTexture");
                //    tarmacTiling = renderer.material.GetTextureScale("_TarmacTexture");

                //renderer.material.shader = KKGraphics.GetShader("KK/Ground_KSC_NoUV");
                //    renderer.material = grassMaterial;

                //    renderer.material.SetTextureScale("_TarmacTexture", tarmacTiling);
                //    renderer.material.SetTextureScale("_BlendMaskTexture", new Vector2(1,1));
                //    nearGrassTiling = 20;
                //    farGrassTiling = 80;
                //    //if (staticInstance.model.name == "KSC_SpaceplaneHangar_level_3")
                //    //{
                //    //    nearGrassTiling = 5;
                //    //    farGrassTiling = 10;
                //    //    renderer.material.SetTextureScale("_BlendMaskTexture", (new Vector2(1, 1)*(1f/16f)));
                //    //    renderer.material.SetTextureOffset("_BlendMaskTexture", (new Vector2(0.5f, 0.5f)));
                //    //}
                //    //renderer.material.SetFloat("_TilingOffset", 10f);


                //}

                //if (!useLegacy && !renderer.material.shader.name.StartsWith("KSP/Scenery/Diffuse Ground KSC")) 
                //{
                //    Vector2 blendmaskTiling = renderer.material.GetTextureScale("_BlendMaskTexture");
                //    tarmacTiling = renderer.material.GetTextureScale("_TarmacTexture");
                //    renderer.material.shader = KKGraphics.GetShader("KSP/Scenery/Diffuse Ground KSC");
                //    renderer.material.SetTextureScale("_TarmacTexture", tarmacTiling);
                //}


                //if (useLegacy)
                //{
                //    renderer.material.SetFloat("_TilingOffset", tilingOffset);
                //}

                renderer.material.SetColor("_GrassColor", grassColor);
                renderer.material.SetColor("_TarmacColor", tarmacColor);
                //if (!staticInstance.model.isSquad)
                //{
                renderer.material.SetFloat("_NearGrassTiling", nearGrassTiling);
                renderer.material.SetFloat("_FarGrassTiling", farGrassTiling);

                //renderer.material.SetTextureScale("_TarmacTexture", tarmacTiling);
                //}
                //Log.Normal("tiling near: " + nearGrassTiling + " isSquad: " + staticInstance.model.isSquad.ToString());
                //Log.Normal("tiling far: " + farGrassTiling + " isSquad: " + staticInstance.model.isSquad.ToString());

                renderer.material.SetFloat("_FarGrassBlendDistance", farGrassBlendDistance);

                if (nearGrassTexture != null)
                {
                    //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                    renderer.material.SetTexture("_NearGrassTexture", nearGrassTexture);
                }
                if (farGrassTexture != null)
                {
                    //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                    renderer.material.SetTexture("_FarGrassTexture", farGrassTexture);
                }
                if (tarmacTexture != null)
                {
                    //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                    renderer.material.SetTexture("_TarmacTexture", tarmacTexture);
                    renderer.material.SetTextureScale("_TarmacTexture", tarmacTiling);
                }
                if (blendMaskTexture != null)
                {
                    //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                    renderer.material.SetTexture("_BlendMaskTexture", blendMaskTexture);
                }

            }
        }


        internal void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            if (staticInstance.model.isSquad)
            {
                FindSquadGrasMaterial();
            }
            else
            {
                FindModelGrasMaterials();
            }
            ReadConfig();

            isInitialized = true;
        }



        public void FindModelGrasMaterials()
        {
            Transform[] allTransforms = gameObject.transform.GetComponentsInChildren<Transform>(true).Where(x => x.name == GrassMeshName).ToArray();
            foreach (var transform in allTransforms)
            {
                Renderer grassRenderer = transform.GetComponent<Renderer>();

                grassRenderers.Add(grassRenderer);


                //grassRenderer.material.shader = Shader.Find("KSP/Scenery/Diffuse");
                //grassRenderer.material.SetTexture("_MainTex", KKGraphics.GetTexture(GrasTextureImage,false,0));

                grassRenderer.material.shader = KKGraphics.GetShader("KK/Ground_KSC_NoUV");
                //grassRenderer.material.SetTexture("_NearGrassTexture", KKGraphics.GetTexture(DefaultNearGrassTexture));
                //grassRenderer.material.SetTexture("_FarGrassTexture", KKGraphics.GetTexture(DefaultFarGrassTexture));
                //grassRenderer.material.SetTexture("_TarmacTexture", KKGraphics.GetTexture(DefaultTarmacTexture));
                //grassRenderer.material.SetTexture("_BlendMaskTexture", KKGraphics.GetTexture(DefaultBlendMaskTexture));

                //grassRenderer.material.SetFloat("_FarGrassBlendDistance", 100f);

                //Log.Normal("Scale: " + grassRenderer.material.GetTextureScale("_NearGrassTexture"));

            }
        }

        public void FindSquadGrasMaterial()
        {
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material material in renderer.materials.Where(mat => mat.shader.name.StartsWith("KSP/Scenery/Diffuse Ground KSC")))
                {
                    grassRenderers.Add(renderer);
                    //DefaultBlendMaskTexture = "BUILTIN:/"+material.GetTexture("_BlendMaskTexture").name;
                    //DefaultTarmacTexture = "BUILTIN:/" + material.GetTexture("_TarmacTexture").name;
                    //nearGrassTextureName = "BUILTIN:/" + material.GetTexture("_NearGrassTexture").name;
                    //farGrassTextureName = "BUILTIN:/" + material.GetTexture("_FarGrassTexture").name;
                    material.shader = KKGraphics.GetShader("KK/Ground_KSC_NoUV");

                    //nearGrassTiling = renderer.material.GetFloat("_NearGrassTiling");
                    //farGrassTiling = renderer.material.GetFloat("_FarGrassTiling");
                    tarmacTiling = renderer.material.GetTextureScale("_TarmacTexture");
                    GrassMeshName = renderer.transform.name;
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
                grassColor = defaultColor;
                tarmacColor = Color.white;

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

                    farGrassTiling = 10;
                    nearGrassTiling = 1;
                    nearGrassTextureName = staticInstance.GrasTexture;
                    farGrassTextureName = staticInstance.GrasTexture;

                    grassColor = GrassColorUtils.ManualCalcNewColor(staticInstance.GrasColor, oldGrassTexture, staticInstance.GrasTexture);


                }

                return;
            }

            foreach (ConfigNode grassConfig in staticInstance.grassColor2Configs)
            {
                Log.Normal("found instance cfgnode");
                if (grassConfig.GetValue("GrassMeshName") == GrassMeshName)
                {
                    Log.Normal("found instance grassnode");
                    nearGrassTextureName = (grassConfig.HasValue("NearGrassTexture")) ? grassConfig.GetValue("NearGrassTexture") : DefaultNearGrassTexture;
                    farGrassTextureName = (grassConfig.HasValue("FarGrassTexture")) ? grassConfig.GetValue("FarGrassTexture") : DefaultFarGrassTexture;
                    tarmacTextureName = (grassConfig.HasValue("TarmacTexture")) ? grassConfig.GetValue("TarmacTexture") : DefaultTarmacTexture;
                    blendMaskTextureName = (grassConfig.HasValue("BlendMaskTexture")) ? grassConfig.GetValue("BlendMaskTexture") : DefaultBlendMaskTexture;
                    nearGrassTiling = (grassConfig.HasValue("NearGrassTiling")) ? float.Parse(grassConfig.GetValue("NearGrassTiling")) : 1f;
                    farGrassTiling = (grassConfig.HasValue("FarGrassTiling")) ? float.Parse(grassConfig.GetValue("FarGrassTiling")) : 1f;
                    farGrassBlendDistance = (grassConfig.HasValue("FarGrassBlendDistance")) ? float.Parse(grassConfig.GetValue("FarGrassBlendDistance")) : 50f;

                    grassColor = (grassConfig.HasValue("GrassColor")) ? ConfigNode.ParseColor(grassConfig.GetValue("GrassColor")) : defaultColor;
                    tarmacColor = (grassConfig.HasValue("TarmacColor")) ? ConfigNode.ParseColor(grassConfig.GetValue("TarmacColor")) : Color.white;
                    tarmacTiling = (grassConfig.HasValue("TarmacTiling")) ? ConfigNode.ParseVector2(grassConfig.GetValue("TarmacTiling")) : new Vector2(10,10);
                    useLegacy = (grassConfig.HasValue("UseLegacyColor")) ? Boolean.Parse(grassConfig.GetValue("UseLegacyColor")) : false;
                    break;
                }



            }
        }

      


    

        public ConfigNode GiveConfig()
        {
            ConfigNode grassNode = new ConfigNode("GrassColor2");

            grassNode.AddValue("GrassMeshName", GrassMeshName);
            if (nearGrassTexture != null)
            {
                grassNode.AddValue("NearGrassTexture", nearGrassTextureName);
            }
            if (farGrassTexture != null)
            {
                grassNode.AddValue("FarGrassTexture", farGrassTextureName);
            }
            if (tarmacTexture != null)
            {
                grassNode.AddValue("TarmacTexture", tarmacTextureName);
            }

            if (blendMaskTexture != null)
            {
                grassNode.AddValue("BlendMaskTexture", blendMaskTextureName);
            }
            grassNode.AddValue("NearGrassTiling", nearGrassTiling);
            grassNode.AddValue("FarGrassTiling", farGrassTiling);
            grassNode.AddValue("FarGrassBlendDistance", farGrassBlendDistance);
            grassNode.AddValue("GrassColor", grassColor);
            grassNode.AddValue("TarmacColor", tarmacColor);
            grassNode.AddValue("TarmacTiling", tarmacTiling);
            grassNode.AddValue("UseLegacyColor", useLegacy);

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