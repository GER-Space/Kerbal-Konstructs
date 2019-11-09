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

        private string legacyTextureName = "BUILTIN:/terrain_grass00_new";
        private string replacementTextureName = "KerbalKonstructs/legacyGrass";

        private bool usePQS = true;
        private bool useNormalMap = false;

        private bool isInitialized = false;
        private List<Material> grasMaterials = new List<Material>();

        private Color grasColor = Color.red;
        private Texture2D grasTexture = null;
        private string grasTextureName = "";

        private static Color defaultColor = new Color(0.640f, 0.728f, 0.171f, 0.729f);

        //private string defaultGrasTextureName = "BUILTIN:/terrain_grass00_new";

        public void Awake()
        {
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

            if (staticInstance.GrasColor == Color.clear && (StaticsEditorGUI.instance != null) && !(StaticsEditorGUI.instance.IsOpen() && (EditorGUI.instance != null) && EditorGUI.instance.grasColorModeIsAuto))
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

            //Log.Normal("Texture Setting is: " + staticInstance.GrasTexture);

            //Log.Normal("Texture Setting2 is: " + grasTextureName);
            //if (grasTextureName == legacyTextureName)
            //{
            //    grasTextureName = replacementTextureName;
            //}

            grasTexture = KKGraphics.GetTexture(grasTextureName, false, 0 , true);

            foreach (Material material in grasMaterials)
            {

                material.SetColor("_Color", grasColor);
                if (grasTexture != null)
                {
                    //Log.Normal("GC: Setting Texture to: " + grasTextureName);
                    material.mainTexture = grasTexture;
                }
            }
        }


        internal void Initialize()
        {
           
           findModelGrasMaterials();
           


            isInitialized = true;
        }

        internal Color GetColor()
        {
            Color underGroundColor = defaultColor;
            if (StaticsEditorGUI.instance != null && EditorGUI.instance != null && StaticsEditorGUI.instance.IsOpen() && EditorGUI.instance.grasColorModeIsAuto)
            {
                if (usePQS)
                {
                    underGroundColor = GetSurfaceColorPQS(staticInstance.CelestialBody, staticInstance.RefLatitude, staticInstance.RefLongitude);
                }
                else
                {
                    underGroundColor = GrasColorCam.instance.GetCameraColor(staticInstance);
                }
                underGroundColor.a = underGroundColor.b - underGroundColor.g;
                staticInstance.GrasColor = underGroundColor;
            }
            else
            {
                underGroundColor = staticInstance.GrasColor;
            }
            //Log.Normal("underGroundColor: " + underGroundColor.ToString());
            if (underGroundColor.a < 1f)
            {
                underGroundColor = GrassColor2.ManualCalcNewColor(underGroundColor, grasTextureName, grasTextureName);
            }
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


        public void findModelGrasMaterials()
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
                if (useNormalMap)
                {
                    //grasRenderer.material.shader = Shader.Find("Legacy Shaders/Bumped Diffuse");
                    //grasRenderer.material.shader = Shader.Find("KSP/Scenery/Diffuse Multiply");
                    if ((String.IsNullOrEmpty(GrasTextureNormalMap) == false) && grasRenderer.material.HasProperty("_BumpMap"))
                    {
                        grasRenderer.material.SetTexture("_BumpMap", KKGraphics.GetTexture(GrasTextureNormalMap, true, 0 , true));
                    }
                }
            }
        }

        public void findSquadGrasMaterial()
        {
            foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                foreach (Material material in renderer.materials.Where(mat => mat.name.StartsWith("ksc_exterior_terrain_grass_02")))
                {
                    //                    defaultColor = material.GetColor("_Color");
                    //Log.Normal("Added material:" + material.name + " : " + material.mainTexture.name);
                    //material.mainTexture = KKGraphics.GetTexture(GrasTextureImage);
                    grasMaterials.Add(material);
                }
            }
        }


    }

}