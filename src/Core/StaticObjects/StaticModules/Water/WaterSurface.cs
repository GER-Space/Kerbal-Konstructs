using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs
{
    class WaterSurface : StaticModule
    {
        public string WaterSurfaceQuadNames = "";
        public string ColliderNames = "";

        private List<Collider> waterColliders = new List<Collider>();

        private List<string> colNames = new List<string>();
        private List<string> waterNames = new List<string>();

        private string[] seperators = new string[] { " ", ",", ";" };

        private GameObject waterSurface;
        private double waterLevel = 0d;


        internal void Start()
        {

            waterNames = WaterSurfaceQuadNames.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (waterNames.Count > 0)
            {
                Transform waterTransform = gameObject.transform.FindRecursive(waterNames[0]);
                if (waterTransform == null)
                {
                    Log.UserError("No WaterSurface found");
                    return;
                }
                waterSurface = waterTransform.gameObject;
            }
            else
            {
                Log.UserError("No WaterSurfaceNames found");
                return;
            }

            colNames = ColliderNames.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (colNames.Count == 0)
            {
                Log.UserError("No WaterCollidernames found");
                return;
            }

            AddColliderTriggers();
            SetupWaterEffect();

        }


        internal void AddColliderTriggers()
        {
            waterLevel = staticInstance.CelestialBody.GetAltitude(waterSurface.transform.position);

            //KKCallBack kkCall = gameObject.GetComponentInChildren<KKCallBack>(true);
            Log.Normal("WaterSurface: Adding KKCallBack to GameObject");
            KKCallBack kkCall = gameObject.AddComponent<KKCallBack>();
            kkCall.ColliderNames = ColliderNames;

            if (!kkCall.isSetup)
            {
                kkCall.Start();
            }

            // only register on the workers of our own KKCallBack, so handlers of other modules are not overwritten
            kkCall.RegisterEnterFunc(MakePartFloat);
            kkCall.RegisterExitFunc(RemoveCustomWaterLevel);
        }


        // called by OnTriggerEnter
        internal void MakePartFloat(Part myPart)
        {
            if (myPart == null)
            {
                Log.Normal("null part");
                return;
            }
            myPart.partBuoyancy.waterLevel = waterLevel;

        }

        internal void RemoveCustomWaterLevel(Part myPart)
        {
            if (myPart == null)
            {
                Log.Normal("null part");
                return;
            }
            myPart.partBuoyancy.waterLevel = 0;
        }


        internal void SetupWaterEffect()
        {
            foreach (string name in waterNames)
            {
                foreach (Transform trans in gameObject.transform.FindAllRecursive(name))
                {
                    Log.Normal("Added water effect to: " + name);
                }
            }
        }
    }
}
