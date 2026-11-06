using System;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    public class MapDecalInstance
    {

        internal GameObject gameObject;

        public UrlDir.UrlConfig configUrl;
        public String configPath = null;


        internal bool isActivated = false;


        // new code here
        [CFGSetting]
        public string Name;
        [CFGSetting]
        public CelestialBody CelestialBody = null;
        [CFGSetting]
        public double Latitude = 361f;
        [CFGSetting]
        public double Longitude = 361f;

        [CFGSetting]
        public double Radius = 125;

        [CFGSetting]
        public string HeightMapName = "None";
        [CFGSetting]
        public string ColorMapName = "None";

        [CFGSetting]
        public bool RemoveScatter = true;
        [CFGSetting]
        public bool UseAlphaHeightSmoothing = true;
        [CFGSetting]
        public bool UseAbsolut = true;
        [CFGSetting]
        public float AbsolutOffset = 0;
        [CFGSetting]
        public bool CullBlack = false;
        [CFGSetting]
        public double HeightMapDeformity = 0d;
        [CFGSetting]
        public float SmoothColor = 0f;
        [CFGSetting]
        public float SmoothHeight = 0.125f;
        [CFGSetting]
        public float Angle = 0f;
        [CFGSetting]
        public int Order = 100000;
        [CFGSetting]
        public string Group = "Ungrouped";

        public bool isInSavegame = false;


        internal MapDecalsMap heighMap = null;
        internal MapDecalsMap colormapMap = null;

        internal PQSMod_MapDecal mapDecal = null;

        internal Vector3 position = Vector3.zero;



        /// <summary>
        /// constructor
        /// </summary>
        internal MapDecalInstance()
        {
            gameObject = new GameObject();
            mapDecal = gameObject.AddComponent<PQSMod_MapDecal>();
            Name = "KK_MapDecal_" + DecalsDatabase.allMapDecalInstances.Length.ToString();

            mapDecal.radius = 0;

            DecalsDatabase.RegisterMapDecalInstance(this);
        }



        //

        /// <summary>
        /// Updates the static instance with new settings
        /// </summary>
		public void Update(bool doUpdate = true)
        {

            mapDecal.heightMap = DecalsDatabase.GetHeightMapByName(HeightMapName).map;
            mapDecal.colorMap = DecalsDatabase.GetColorMapByName(ColorMapName).map;

            if (mapDecal.heightMap == null && mapDecal.colorMap == null)
            {
                //DecalsDatabase.DeleteMapDecalInstance(this);
                return;
            }

            mapDecal.modEnabled = true;
            mapDecal.requirements = PQS.ModiferRequirements.MeshColorChannel | PQS.ModiferRequirements.MeshCustomNormals;
            mapDecal.order = Order;
            mapDecal.sphere = CelestialBody.pqsController;
            mapDecal.transform.parent = CelestialBody.pqsController.transform;

            mapDecal.position = CelestialBody.GetRelSurfaceNVector(Latitude, Longitude).normalized * CelestialBody.Radius;

            mapDecal.absolute = UseAbsolut;
            mapDecal.absoluteOffset = AbsolutOffset;

            mapDecal.radius = Radius;
            mapDecal.angle = Angle;
            mapDecal.removeScatter = RemoveScatter;


            mapDecal.smoothHeight = SmoothHeight;

            mapDecal.heightMapDeformity = HeightMapDeformity;
            mapDecal.useAlphaHeightSmoothing = UseAlphaHeightSmoothing;
            mapDecal.cullBlack = CullBlack;

            mapDecal.smoothColor = SmoothColor;

            //Log.Normal("MapDecal: heightmap: " + mapDecal.heightMap.name);
            //Log.Normal("MapDecal: Radius: "  + Radius.ToString());
            //Log.Normal("MapDecal: Vector:  " + mapDecal.position.ToString());
            //Log.Normal("MapDecal: Offset: " + AbsolutOffset);
            //Log.Normal("MapDecal: UseABS: " + UseAbsolut.ToString());


            mapDecal.OnSetup();
            // only rebuild the sphere when we use the editor
            if (doUpdate)
            {
                mapDecal.sphere.ForceStart();
            }
        }


    }
}