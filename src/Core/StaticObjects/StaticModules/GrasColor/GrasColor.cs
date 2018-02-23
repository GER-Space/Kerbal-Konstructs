using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;
using KerbalKonstructs.UI;


namespace KerbalKonstructs
{
    public class GrasColor: StaticModule
    {

        public string GrasMeshName;
        public string GrasTextureImage;
        public string UsePQSColor = "True";
        public string UseNormalMap = "False";
        public string GrasTextureNormalMap = "";



        private bool usePQS = true;
        private bool useNormalMap = true;

        public void Awake()
        {
            //Log.Normal("FlagDeclal: Awake called");
            //setTexture();
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
            setTexture();
        }

        internal Color GetColor()
        {

            
            Color underGroundColor = Color.clear;
            if (staticInstance.GrasColor == Color.clear || StaticsEditorGUI.instance.IsOpen())
            {
                if (usePQS)
                {

                    underGroundColor = GetSurfaceColor(staticInstance.CelestialBody, staticInstance.RefLatitude, staticInstance.RefLongitude);
                } else
                {
                    underGroundColor = GrasColorCam.instance.getCameraColor(staticInstance);
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
        /// Sets the texture in all transforms of the right name
        /// </summary>
        internal void setTexture()
        {
            //Log.Normal("FlagDeclal: setTexture called");

            Transform[] allTransforms = gameObject.transform.GetComponentsInChildren<Transform>(true).Where(x => x.name == GrasMeshName).ToArray();


            foreach (var transform in allTransforms)
            {
                Renderer flagRenderer = transform.GetComponent<Renderer>();
                if (useNormalMap)
                {
                    flagRenderer.material.shader = Shader.Find("Legacy Shaders/Bumped Diffuse");
                    flagRenderer.material.SetTexture("", GameDatabase.Instance.GetTexture(GrasTextureNormalMap, true));
                }
                else
                {
                    flagRenderer.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                }
                flagRenderer.material.shader = Shader.Find("Legacy Shaders/Diffuse");
                flagRenderer.material.mainTexture = GameDatabase.Instance.GetTexture(GrasTextureImage, false);

                flagRenderer.material.SetColor("_Color", GetColor());
            }
        }


        /// <summary>
        /// Uses the PQS System to query the color of the undergound
        /// </summary>
        /// <param name="body"></param>
        /// <param name="lat"></param>
        /// <param name="lon"></param>
        /// <returns></returns>
        public static Color GetSurfaceColor(CelestialBody body, Double lat, Double lon)
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
            PQSMod[] mods = body.GetComponentsInChildren<PQSMod>().Where(m => m.modEnabled && m.sphere == body.pqsController).ToArray();

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
    }



}