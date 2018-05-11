using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
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
                waterSurface = gameObject.transform.FindRecursive(waterNames[0]).gameObject;
                if (waterSurface == null)
                {
                    Log.UserError("No WaterSurface found");
                    return;
                }
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

            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>(true))
            {
                if (colNames.Contains(col.name))
                {
                    KKWaterFloating floatingTrigger = col.gameObject.AddComponent<KKWaterFloating>();
                    floatingTrigger.waterLevel = waterLevel;
                }
            }

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
