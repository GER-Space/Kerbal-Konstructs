using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    class BuiltinCenters
    {
        internal static void AddAllBuiltinCenters()
        {
            AddPQSCenter("KSC");
            AddPQSCenter("KSC2");
            AddPQSCenter("Pyramids");
            AddPQSCenter("IslandAirfield");

            if (Expansions.ExpansionsLoader.IsExpansionInstalled("MakingHistory"))
            {
                AddPQSCenter2("Desert_Airfield");
                AddPQSCenter2("Woomerang_Launch_Site");
            }
        }


        internal static void AddPQSCenter2(string tgtName)
        {
            CelestialBody body = ConfigUtil.GetCelestialBody("HomeWorld");
            var mods = body.pqsController.GetComponentsInChildren<PQSCity2>(true);
            PQSCity2 tgtPQS = null;

            foreach (var m in mods)
            {
                //Log.Normal("PQS2Name: " + m.name);
                if (m.name == tgtName)
                {
                    tgtPQS = m;
                    continue;
                }
            }
            if (tgtPQS == null)
            {
                Log.Normal("No BasePQS found: " + tgtName);
                return;
            }

            GroupCenter newGroup = new GroupCenter();
            newGroup.isBuiltIn = true;
            newGroup.Group = tgtName + "_Builtin";
            newGroup.CelestialBody = body;

            newGroup.RotationAngle = (float)tgtPQS.rotation;
            newGroup.RadialPosition = KKMath.GetRadiadFromLatLng(body , tgtPQS.lat, tgtPQS.lon);

            //newGroup.RadiusOffset = (float)0;
            newGroup.RadiusOffset = (float)tgtPQS.alt;
            newGroup.SeaLevelAsReference = true;
            //newGroup.repositionToSphere = tgtPQS.repositionToSphere;
            newGroup.Spawn();

        }

            internal static void AddPQSCenter(string tgtName)
        {
            CelestialBody body = ConfigUtil.GetCelestialBody("HomeWorld");
            var mods = body.pqsController.GetComponentsInChildren<PQSCity>(true);
            PQSCity tgtPQS = null;

            foreach (var m in mods)
            {
                //Log.Normal("PQSName: " + m.name);
                if (m.name == tgtName)
                {
                    tgtPQS = m;
                    continue;
                }
            }
            if (tgtPQS == null)
            {
                Log.Normal("No BasePQS found: " + tgtName);
                return;
            }
            
            GroupCenter newGroup = new GroupCenter();
            newGroup.isBuiltIn = true;
            newGroup.Group = tgtName + "_Builtin";
            newGroup.CelestialBody = body;

            newGroup.RotationAngle = tgtPQS.reorientFinalAngle;
            newGroup.RadialPosition = tgtPQS.repositionRadial;

            //newGroup.RadiusOffset = (float)0;
            newGroup.RadiusOffset = (float)tgtPQS.repositionRadiusOffset;
            //newGroup.repositionToSphere = false;
            newGroup.SeaLevelAsReference = tgtPQS.repositionToSphere;
            newGroup.Spawn();




            //newGroup.Update();
            //Log.Normal("Added GroupCenter:" + newGroup.Group);
        }

           

    }
}
