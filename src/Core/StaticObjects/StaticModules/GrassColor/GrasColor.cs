using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;

namespace KerbalKonstructs
{

    //Legacy module that is just a wrapper to the new version
    class GrasColor : StaticModule
    {

        public string GrasMeshName;
        public string GrasTextureImage = "BUILTIN:/terrain_grass00_new";
        public string UsePQSColor = "False";



        private bool isInitialized = false;

        public override void StaticObjectUpdate()
        {
            if (!isInitialized)
            {
                isInitialized = true;
                ApplyModule();
            }
        }


        internal void ApplyModule()
        {
            GrassColor2 grass2 = staticInstance.mesh.AddComponent<GrassColor2>();
            grass2.enabled = false;
            grass2.staticInstance = staticInstance;
            grass2.GrassMeshName = GrasMeshName;
            grass2.nearGrassTexture = string.IsNullOrEmpty(staticInstance.GrasTexture) ? KKGraphics.GetTexture(GrasTextureImage) : KKGraphics.GetTexture(staticInstance.GrasTexture);
            grass2.UsePQSColor = UsePQSColor;


            Log.Normal("Color String: " + staticInstance.GrasColor.ToString());
            switch (staticInstance.GrasColor.ToString())
            {
                case "RGBA(0.640, 0.728, 0.171, 0.729)":
                    grass2.grassColor = GrassColor2.defaultColor;
                    Log.Normal("setting new Color");
                    Log.Normal("setting new Tiling");
                    grass2.nearGrassTiling = 0.2f;
                    grass2.farGrassTiling = 0.01f;
                    grass2.DefaultNearGrassTiling = "0.2";
                    grass2.DefaultFarGrassTiling = "0.01";
                    break;
                default:
                    grass2.grassColor = staticInstance.GrasColor;
                    grass2.nearGrassTiling = 0.2f;
                    grass2.farGrassTiling = 0.01f;
                    grass2.DefaultNearGrassTiling = "0.2";
                    grass2.DefaultFarGrassTiling = "0.01";
                    break;
            }

            grass2.enabled = true;
            grass2.Initialize();
            grass2.ApplySettings();
        }
    }
}
