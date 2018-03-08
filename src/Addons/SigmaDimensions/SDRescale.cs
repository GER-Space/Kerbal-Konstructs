using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using KerbalKonstructs.Core;
using KerbalKonstructs.Modules;




namespace KerbalKonstructs.Core
{
    class SDRescale
    {
        internal static double planetRescale = 1;
        internal static double terrainRescale = 1;
        internal static double globalTerrainRescale = 1;

        private static bool rescalesSet = false;


        internal static void SetTerrainRescales()
        {
            CelestialBody homeWorld = ConfigUtil.GetCelestialBody("HomeWorld");

            if (homeWorld.bodyName == "Kerbin")
            {
                planetRescale = Math.Round(homeWorld.pqsController.radius, 0) / 600000;

                foreach (var pqs in homeWorld.GetComponentsInChildren<PQSCity>(true))
                {
                    if (pqs.SurfaceObjectName == "KSC")
                    {
                        var myheight = Math.Round(homeWorld.pqsController.GetSurfaceHeight(pqs.repositionRadial) - homeWorld.pqsController.radius, 1);
                        Log.Normal("Found KSC at: " + Math.Round(myheight, 1) + "m above sealevel");
                        Log.Normal("body radius: " + Math.Round(homeWorld.pqsController.radius, 1) + "m");

                        terrainRescale = myheight / 64.8d ;
                    }
                }
                globalTerrainRescale = planetRescale * terrainRescale;
            }
            else
            {
                Log.Warning("our homeworld is " + homeWorld.name +  ", nothing to do");
            }


        }


        internal static float GetSurfaceRefereceHeight(StaticInstance instance)
        {
            if (!rescalesSet)
            {
                SetTerrainRescales();
                rescalesSet = true;
            }
            double currentTerrainHeight = (instance.CelestialBody.pqsController.GetSurfaceHeight(instance.RadialPosition) - instance.CelestialBody.pqsController.radius);
            double origTerrainHeight = currentTerrainHeight / globalTerrainRescale;

            return (float)(instance.RadiusOffset - origTerrainHeight);
        }


    }
}
