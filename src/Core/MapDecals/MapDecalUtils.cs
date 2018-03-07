using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace KerbalKonstructs.Core
{
    class MapDecalUtils
    {
        internal static void GetSquadMaps ()
        {
            MapDecalsMap noHeighmap = ScriptableObject.CreateInstance<MapDecalsMap>();
            noHeighmap.map = null;
            noHeighmap.isHeightMap = true;
            noHeighmap.Name = "None";

            MapDecalsMap noColormap = ScriptableObject.CreateInstance<MapDecalsMap>();
            noColormap.map = null;
            noColormap.isHeightMap = false;
            noColormap.Name = "None";

            DecalsDatabase.RegisterMap(noHeighmap);
            DecalsDatabase.RegisterMap(noColormap);


            PQSMod_MapDecal[] allMapDecals =  Resources.FindObjectsOfTypeAll<PQSMod_MapDecal>();

            foreach (var mapDecal in allMapDecals) 
            {
                //Log.Normal("");
            //    Log.Normal("Stats for: " + mapDecal.name);
                if (mapDecal.heightMap != null)
                {
                    MapDecalsMap heightMap = ScriptableObject.CreateInstance<MapDecalsMap>();
                    heightMap.isHeightMap = true;
                    heightMap.map = mapDecal.heightMap;
                    heightMap.Name = "height_" + mapDecal.name;
                    DecalsDatabase.RegisterMap(heightMap);

                   // Log.Normal("MapDecal: heightmap: " + mapDecal.heightMap.name);

                }

                if (mapDecal.colorMap != null)
                {
                    MapDecalsMap colorMap = ScriptableObject.CreateInstance<MapDecalsMap>();
                    colorMap.isHeightMap = false;
                    colorMap.map = mapDecal.colorMap;
                    colorMap.Name = "color_" + mapDecal.name;
                    DecalsDatabase.RegisterMap(colorMap);
              //      Log.Normal("MapDecal: colormap: " + mapDecal.colorMap.name);
                }

                
                //Log.Normal("MapDecal: Radius: " + mapDecal.radius.ToString());
                //Log.Normal("MapDecal: Vector: " + mapDecal.position.ToString());
                //Log.Normal("MapDecal: Offset: " + mapDecal.absoluteOffset);
                //Log.Normal("MapDecal: UseABS: " + mapDecal.absolute.ToString());

                //Log.Normal("MapDecal: Heightmapdeformity: " + mapDecal.heightMapDeformity.ToString());

                //Log.Normal("MapDecal: modisenabled " + mapDecal.modEnabled.ToString());
                //Log.Normal("MapDecal: order: " + mapDecal.order.ToString());
                //Log.Normal("MapDecal: requirement: " + mapDecal.requirements.ToString());
                //Log.Normal("MapDecal: smoothcolor: " + mapDecal.smoothColor.ToString());
                //Log.Normal("MapDecal: smoothhheight: " + mapDecal.smoothHeight.ToString());
                //if (mapDecal.sphere != null )
                //    Log.Normal("MapDecal: sphere: " + mapDecal.sphere.name.ToString());
                //Log.Normal("MapDecal: Alphaheight: " + mapDecal.useAlphaHeightSmoothing.ToString());
                //Log.Normal("MapDecal: Cullblack: " + mapDecal.cullBlack.ToString());


            }

            PQSMod_MapDecalTangent[] allMapDecalTangents = Resources.FindObjectsOfTypeAll<PQSMod_MapDecalTangent>();

            foreach (var mapDecal in allMapDecalTangents)
            {
                //Log.Normal("");
                //Log.Normal("Stats for: " + mapDecal.name);

                if (mapDecal.heightMap != null)
                {
                    MapDecalsMap heightMap = ScriptableObject.CreateInstance<MapDecalsMap>();
                    heightMap.isHeightMap = true;
                    heightMap.map = mapDecal.heightMap;
                    heightMap.Name = "height_" + mapDecal.name;
                    DecalsDatabase.RegisterMap(heightMap);
                    //Log.Normal("MapDecalTGT: heightmap: " + mapDecal.heightMap.name);
                }

                if (mapDecal.colorMap != null)
                {
                    MapDecalsMap colorMap = ScriptableObject.CreateInstance<MapDecalsMap>();
                    colorMap.isHeightMap = false;
                    colorMap.map = mapDecal.colorMap;
                    colorMap.Name = "color_" + mapDecal.name;
                    DecalsDatabase.RegisterMap(colorMap);
                    //Log.Normal("MapDecalTGT: colormap: " + mapDecal.colorMap.name);
                }

                //Log.Normal("MapDecalTGT: Radius: " + mapDecal.radius.ToString());
                //Log.Normal("MapDecalTGT: Vector:  " + mapDecal.position.ToString());
                //Log.Normal("MapDecalTGT: Offset: " + mapDecal.absoluteOffset);
                //Log.Normal("MapDecalTGT: UseABS: " + mapDecal.absolute.ToString());

                //Log.Normal("MapDecalTGT: Heightmapdeformity: " + mapDecal.heightMapDeformity.ToString());

                //Log.Normal("MapDecalTGT: modisenabled: " + mapDecal.modEnabled.ToString());
                //Log.Normal("MapDecalTGT: order: " + mapDecal.order.ToString());
                //Log.Normal("MapDecalTGT: requirement: " + mapDecal.requirements.ToString());
                //Log.Normal("MapDecalTGT: smoothcolor: " + mapDecal.smoothColor.ToString());
                //Log.Normal("MapDecalTGT: smoothhheight: " + mapDecal.smoothHeight.ToString());
                //if (mapDecal.sphere != null)
                //    Log.Normal("MapDecalTGT: sphere: " + mapDecal.sphere.name.ToString());
                //Log.Normal("MapDecalTGT: Alphaheight: " + mapDecal.useAlphaHeightSmoothing.ToString());
                //Log.Normal("MapDecalTGT: Cullblack: " + mapDecal.cullBlack.ToString());


            }

            Log.Normal("Imported " + (DecalsDatabase.allHeightMaps.Count + DecalsDatabase.allColorMaps.Count) + " Maps from around the universe" );
            //foreach (var map in DecalsDatabase.allDecalMaps)
            //{
            //    Log.Normal("DecalMap: " + map.name);
            //}
        }

        internal static MapDecalInstance SpawnNewDecalInstance()
        {
            MapDecalInstance newMapDecal = new MapDecalInstance();
            newMapDecal.CelestialBody = FlightGlobals.currentMainBody;
            newMapDecal.mapDecal.position = FlightGlobals.currentMainBody.transform.InverseTransformPoint(FlightGlobals.ActiveVessel.transform.position);

            newMapDecal.mapDecal.transform.position = FlightGlobals.ActiveVessel.transform.position;

            double lat, lon, alt;


            FlightGlobals.currentMainBody.GetLatLonAlt(FlightGlobals.ActiveVessel.transform.position, out lat, out lon, out alt);

            newMapDecal.Latitude = lat;
            newMapDecal.Longitude = lon;
            newMapDecal.AbsolutOffset = (float)alt;
            newMapDecal.mapDecal.transform.up = FlightGlobals.currentMainBody.GetSurfaceNVector(lat, lon);

            Log.Normal("New MapDecalInstance created");
            return newMapDecal;

        }

    }
}
