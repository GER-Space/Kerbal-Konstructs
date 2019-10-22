using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;
using KerbalKonstructs.UI;


namespace KerbalKonstructs
{
    public class GrassColor2 : StaticModule
    {

        public string GrassMeshName = "Nix";
        public string UsePQSColor = "False";

        public string DefaultNearGrassTexture = "BUILTIN:/terrain_grass00_new";
        public string DefaultFarGrassTexture = "BUILTIN:/terrain_grass00_new_detail";
        public string DefaultTarmacTexture = "BUILTIN:/ksc_exterior_terrain_asphalt";
        public string DefaultBlendMaskTexture = "BUILTIN:/blackSquare";

        public string DefaultNearGrassTiling = "1";
        public string DefaultFarGrassTiling = "1";
        public string DefaultFarGrassBlendDistance = "100";



        private bool usePQS = false;
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


        internal float nearGrassTiling = 0.2f; 
        internal float farGrassTiling = 0.01f;  
        internal float farGrassBlendDistance = 100; 

        private ConfigNode cfgNode;

        internal static Color defaultColor = new Color(0.576471f,0.611765f,0.392157f,1.00000f);
        internal static Color defaultTarmacColor = Color.white;






        public void Awake()
        {
        }



        public override void StaticObjectUpdate()
        {
            SetColor();
        }


        /// <summary>
        /// Sets the texture in all transforms of the right name
        /// </summary>
        internal void SetColor()
        {

            if (!isInitialized)
            {
                Initialize();
            }

            if (grassColor == Color.clear && !(StaticsEditorGUI.instance.IsOpen() && EditorGUI.instance.grasColorModeIsAuto))
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

                renderer.material.SetColor("_GrassColor", grassColor);
                renderer.material.SetColor("_TarmacColor", tarmacColor);
                if (!staticInstance.model.isSquad)
                {
                    renderer.material.SetFloat("_NearGrassTiling", nearGrassTiling);
                    renderer.material.SetFloat("_FarGrassTiling", farGrassTiling);
                }
                Log.Normal("tiling near: " + nearGrassTiling + " isSquad: " + staticInstance.model.isSquad.ToString());
                Log.Normal("tiling far: " + farGrassTiling + " isSquad: " + staticInstance.model.isSquad.ToString());

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
            Log.Normal("Hello");
            ReadConfig();
            if (!bool.TryParse(UsePQSColor, out usePQS))
            {
                Log.UserWarning("GrasColor Module: could not parse UsePQSColor to bool: " + UsePQSColor);
            }

            isInitialized = true;
        }



        internal Color GetColor()
        {
            Color underGroundColor = defaultColor;
            if ((StaticsEditorGUI.instance.IsOpen() && EditorGUI.instance.grasColorModeIsAuto)  )
            {
                if (usePQS)
                {
                    underGroundColor = GetSurfaceColorPQS(staticInstance.CelestialBody, staticInstance.RefLatitude, staticInstance.RefLongitude);
                }
                else
                {
                    underGroundColor = GrasColorCam.instance.GetCameraColor(staticInstance);
                }
                staticInstance.GrasColor = underGroundColor;
            }
            else
            {
                underGroundColor = staticInstance.GrasColor;
            }
            //Log.Normal("underGroundColor: " + underGroundColor.ToString());
            return underGroundColor;
        }

        /// <summary>
        /// Uses the PQS System to query the color of the undergound
        /// </summary>
        /// <param name="body"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static Color GetSurfaceColorPQS(CelestialBody body, Double lat, Double lon)
        {
            // Tell the PQS that our actions are not supposed to end up in the terrain
            body.pqsController.isBuildingMaps = true;
            body.pqsController.isFakeBuild = true;

            // Create the vertex information
            PQS.VertexBuildData data = new PQS.VertexBuildData
            {
                directionFromCenter = body.GetRelSurfaceNVector(lat, lon).normalized,
                vertHeight = body.pqsController.radius
            };

            // Fetch all enabled Mods
            PQSMod[] mods = body.GetComponentsInChildren<PQSMod>(true).Where(m => m.modEnabled && m.sphere == body.pqsController).ToArray();

            // Iterate over them and build the height at this point
            // This is neccessary for mods that use the terrain height to 
            // color the terrain (like HeightColorMap)
            foreach (PQSMod mod in mods)
            {
                mod.OnVertexBuildHeight(data);
            }

            // Iterate over the mods again, this time build the color component 
            foreach (PQSMod mod in mods)
            {
                mod.OnVertexBuild(data);
            }

            // Reset the PQS
            body.pqsController.isBuildingMaps = false;
            body.pqsController.isFakeBuild = false;

            // The terrain color is now stored in data.vertColor. 
            // For getting the height at this point you can use data.vertHeight
            return data.vertColor;
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

                grassRenderer.material.shader = Shader.Find("KSP/Scenery/Diffuse Ground KSC");
                grassRenderer.material.SetTexture("_NearGrassTexture", KKGraphics.GetTexture(DefaultNearGrassTexture));
                grassRenderer.material.SetTexture("_FarGrassTexture", KKGraphics.GetTexture(DefaultFarGrassTexture));
                grassRenderer.material.SetTexture("_TarmacTexture", KKGraphics.GetTexture(DefaultTarmacTexture));
                grassRenderer.material.SetTexture("_BlendMaskTexture", KKGraphics.GetTexture(DefaultBlendMaskTexture));

                grassRenderer.material.SetFloat("_FarGrassBlendDistance", 100f);

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

                    nearGrassTiling = renderer.material.GetFloat("_NearGrassTiling");
                    farGrassTiling =  renderer.material.GetFloat("_FarGrassTiling");



                }
            }
        }


        public void ReadConfig()
        {
            Log.Normal("called");

            if (staticInstance.cfgNode == null || staticInstance.cfgNode.GetNodes("GrassColor2").Length == 0)
            {
                nearGrassTextureName =  DefaultNearGrassTexture;
                farGrassTextureName =  DefaultFarGrassTexture;
                tarmacTextureName =  DefaultTarmacTexture;
                blendMaskTextureName = DefaultBlendMaskTexture;
                nearGrassTiling = float.Parse(DefaultNearGrassTiling) ;
                farGrassTiling = float.Parse(DefaultFarGrassTiling);
                farGrassBlendDistance = float.Parse(DefaultFarGrassBlendDistance);
                grassColor = defaultColor;
                tarmacColor = Color.white;
                return;
            } 
                
            foreach (ConfigNode grassConfig in staticInstance.cfgNode.GetNodes("GrassColor2"))
            {
                Log.Normal("found instance cfgnode");
                if (grassConfig.GetValue("GrassMeshName") == GrassMeshName)
                {
                    Log.Normal("found instance grassnode");
                    cfgNode = grassConfig;
                    nearGrassTexture = (grassConfig.GetValue("NearGrassTexture") != null) ? KKGraphics.GetTexture(grassConfig.GetValue("NearGrassTexture")) : KKGraphics.GetTexture(DefaultNearGrassTexture);
                    farGrassTexture = (grassConfig.GetValue("FarGrassTexture") != null) ? KKGraphics.GetTexture(grassConfig.GetValue("FarGrassTexture")) : KKGraphics.GetTexture(DefaultFarGrassTexture);
                    tarmacTexture = (grassConfig.GetValue("TarmacTexture") != null) ? KKGraphics.GetTexture(grassConfig.GetValue("TarmacTexture")) : KKGraphics.GetTexture(DefaultTarmacTexture);
                    blendMaskTexture = (grassConfig.GetValue("BlendMaskTexture") != null) ? KKGraphics.GetTexture(grassConfig.GetValue("BlendMaskTexture")) : KKGraphics.GetTexture(DefaultBlendMaskTexture);                    
                    nearGrassTiling = (grassConfig.GetValue("NearGrassTiling") != null) ? float.Parse(grassConfig.GetValue("NearGrassTiling")) : 1f;
                    farGrassTiling = (grassConfig.GetValue("FarGrassTiling") != null) ? float.Parse(grassConfig.GetValue("FarGrassTiling")) : 1f;
                    farGrassBlendDistance = (grassConfig.GetValue("FarGrassBlendDistance") != null) ? float.Parse(grassConfig.GetValue("FarGrassBlendDistance")) : 50f;

                    grassColor = (grassConfig.GetValue("GrassColor") != null) ? ConfigNode.ParseColor(grassConfig.GetValue("GrassColor")) : defaultColor;
                    tarmacColor = (grassConfig.GetValue("TarmacColor") != null) ? ConfigNode.ParseColor(grassConfig.GetValue("TarmacColor")) : Color.white;
                    break;
                }



            }
        }


        public void WriteCfg(ConfigNode instanceNode)
        {
            ConfigNode grassNode = instanceNode.AddNode("GrassColor2");

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

        }


    }

}