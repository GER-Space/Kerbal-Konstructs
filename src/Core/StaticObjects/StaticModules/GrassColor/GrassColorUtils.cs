using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    class GrassColorUtils
    {


        internal static Color ManualCalcNewColor(Color oldColor, string oldTextrueName, string newTextureName)
        {

            if (String.IsNullOrEmpty(oldTextrueName) || oldTextrueName == "BUILTIN:/terrain_grass00_new")
            {
                oldTextrueName = "KerbalKonstructs/Assets/Colors/legacyGrassColors";
            }

            if (String.IsNullOrEmpty(newTextureName))
            {
                newTextureName = "BUILTIN:/terrain_grass00_new_detail";
            }


            Texture2D oldTexture = KKGraphics.GetTexture(oldTextrueName).BlitTexture(64);
            Texture2D newTexture = KKGraphics.GetTexture(newTextureName).BlitTexture(64);

            Color oldAvgTexColor = AverageColor(oldTexture.GetPixels());
            Color newAvgTexColor = AverageColor(newTexture.GetPixels());

            //Log.Normal("oldAvgTexColor: " + oldAvgTexColor.ToString());
            //Log.Normal("newAvgTexColor: " + newAvgTexColor.ToString());

            Color legacyColor = Color.Lerp(oldColor, oldAvgTexColor, oldColor.a);
            legacyColor.a = 1;

            //Color firstNewColor = newAvgTexColor * legacyColor;
            //Log.Normal("firstNewColor : " + firstNewColor.ToString());


            Color finalColor = legacyColor.DivideWith(newAvgTexColor).LimitTo(6f);
            finalColor.a = 1;
            //Log.Normal("final Color: " + finalColor.ToString());

            return finalColor;
        }

        internal static Color GetAverageColor(string textureName)
        {
            Texture2D texture = KKGraphics.GetTexture(textureName).BlitTexture(64);
            if (texture == null)
            {
                return Color.grey;
            }
            return AverageColor(texture.GetPixels());
        }


        internal static Color AverageColor(Color[] array)
        {
            Color sum = Color.clear;
            for (var i = 0; i < array.Length; i++)
            {
                sum += array[i];
            }
            return (sum / array.Length);

        }



        internal static Color GetUnderGroundColor(StaticInstance staticInstance)
        {
            Color underGroundColor;

            if (KerbalKonstructs.useCam)
            {
                underGroundColor = GrasColorCam.instance.GetCameraColor(staticInstance);
            }
            else
            {
                underGroundColor = GetSurfaceColorPQS(staticInstance.CelestialBody, staticInstance.RefLatitude, staticInstance.RefLongitude);
                //    Texture2D groundTex = GrasColorCam.instance.GetCameraTexture(staticInstance);
                //    if (groundTex != null)
                //    {
                //        Color textureColor = AverageColor(groundTex.GetPixels());
                //        Log.Normal("TextureColor: " + textureColor.ToString());
                //        underGroundColor +=  textureColor;
                //    }
                //    else
                //    {
                //        Log.Normal("No Texture received");
                //    }
            }

            Color avColor = GetAverageColor(staticInstance.GrasTexture);

            underGroundColor = underGroundColor.DivideWith(avColor);
            underGroundColor.a = 1;
            return underGroundColor;
        }



        internal static Color GetUnderGroundColor(GrassColor2 selectedMod)
        {
            Color underGroundColor;

            StaticInstance staticInstance = selectedMod.staticInstance;

            if (KerbalKonstructs.useCam)
            {
                underGroundColor = GrasColorCam.instance.GetCameraColor(staticInstance);
            }
            else
            {
                underGroundColor = GetSurfaceColorPQS(staticInstance.CelestialBody, staticInstance.RefLatitude, staticInstance.RefLongitude);
                //Texture2D groundTex = GrasColorCam.instance.GetCameraTexture(staticInstance);
                //if (groundTex != null)
                //{
                //    Color textureColor = AverageColor(groundTex.GetPixels());
                //    Log.Normal("TextureColor: " + textureColor.ToString());
                //    underGroundColor += textureColor;
                //}
                //else
                //{
                //    Log.Normal("No Texture received");
                //}
            }

            Color avColor = GetAverageColor(selectedMod.farGrassTextureName);

            underGroundColor = underGroundColor.DivideWith(avColor);
            underGroundColor.a = 1;

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
                //vertHeight = body.pqsController.radius
                vertHeight = body.pqsController.GetSurfaceHeight(body.GetRelSurfaceNVector(lat, lon).normalized * body.Radius)
            };

            // Fetch all enabled Mods
            //PQSMod[] mods = body.GetComponentsInChildren<PQSMod>(true).Where(m => m.modEnabled && m.sphere == body.pqsController).ToArray();
            List<PQSMod> modsList = body.GetComponentsInChildren<PQSMod>(true).Where(m => m.modEnabled && m.sphere == body.pqsController).ToList();

            modsList.Sort(delegate (PQSMod first, PQSMod second)
            {
                return first.order.CompareTo(second.order);
            });
            PQSMod[] mods = modsList.ToArray();


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

            Color vertColor = data.vertColor;

            vertColor += new Color(0.01f, 0.01f, 0.02f);
            vertColor.a = 1;
            // The terrain color is now stored in data.vertColor. 
            // For getting the height at this point you can use data.vertHeight
            return vertColor;
        }






    }
}
