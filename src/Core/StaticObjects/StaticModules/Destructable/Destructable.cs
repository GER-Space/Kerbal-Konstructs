using KerbalKonstructs.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs
{
    class Destructable
    {
        private static bool isInitialized = false;
        private static VFXSequencer demolitionPrefab, secondaryPrefab;

        private static void Initialize()
        {
            foreach (DestructibleBuilding building in StaticDatabase.GetModelByName("KSC_VehicleAssemblyBuilding_level_3").prefab.GetComponentsInChildren<DestructibleBuilding>(true))
            {
                if (building.name == "mainBuilding")
                {
                    demolitionPrefab = building.DemolitionFXPrefab;

                }

            }
            foreach (DestructibleBuilding building in Resources.FindObjectsOfTypeAll<DestructibleBuilding>())
            {
                if (building.name == "CornerLab")
                {
                    foreach (var bla in building.CollapsibleObjects)
                    {
                        if (bla.SecondaryFXPrefab != null)
                        {
                            secondaryPrefab = bla.SecondaryFXPrefab;
                        }
                    }
                }
            }



            isInitialized = true;
        }



        internal static void MakeDestructable(StaticInstance instance)
        {

            if (!isInitialized)
            {
                Initialize();
            }

            bool wasActive = instance.gameObject.activeSelf;
            instance.gameObject.SetActive(false);
            instance.destructible = instance.gameObject.AddComponent<DestructibleBuilding>();
            List<DestructibleBuilding.CollapsibleObject> allCollapsibles = new List<DestructibleBuilding.CollapsibleObject>();
            instance.destructible.enabled = false;

            CreateCollapsables(instance, instance.mesh.transform, allCollapsibles, "KSC_LaunchPad_level_2_wreck_1");

            instance.destructible.CollapsibleObjects = allCollapsibles.ToArray();
            instance.destructible.CollapseReputationHit = 0;
            instance.destructible.FacilityDamageFraction = 100;
            instance.destructible.id = instance.UUID;
            instance.destructible.preCompiledId = true;
            instance.destructible.DemolitionFXPrefab = demolitionPrefab;
            instance.destructible.FxTarget = instance.gameObject.transform;
            instance.destructible.impactMomentumThreshold = instance.model.impactThreshold;

            instance.wreck.SetActive(true);
            instance.destructible.enabled = true;

            instance.gameObject.SetActive(wasActive);
        }


        private static void CreateCollapsables(StaticInstance instance, Transform target, List<DestructibleBuilding.CollapsibleObject> allCollapsibles, string wreckName)
        {

            Bounds staticBounds = target.gameObject.GetAllRendererBounds();
            GameObject wreckObj = GameObject.Instantiate(StaticDatabase.GetModelByName(wreckName).prefab);


            //wreckObj.SetActive(true);
            //Bounds wreckBounds = wreckObj.GetAllRendererBounds();
            //wreckObj.SetActive(false);
            //GameObject.DestroyImmediate(wreckObj);
            //Log.Normal("Wreck Bounds: " + wreckBounds.size.ToString());
            //Log.Normal("Bounds: " + staticBounds.size.ToString());

            float wrecksize = 65f;

            float min = 1;
            if (staticBounds.size.x != 0 && staticBounds.size.z != 0)
            {
                min = Math.Min(staticBounds.size.x, staticBounds.size.z);
            }
            else if (staticBounds.size.x == 0 && staticBounds.size.z != 0)
            {
                min = staticBounds.size.z;
            }
            else if (staticBounds.size.x != 0 && staticBounds.size.z == 0)
            {
                min = staticBounds.size.x;
            }
            else
            {
                min = 1;
            }

            float max = Math.Max(staticBounds.size.x, staticBounds.size.z);

            float scale = min / wrecksize;
            float times = max / min;

            int counter = 0;
            while (times > 0.3f)
            {
                float extrascale = Math.Min(times, 1);
                GameObject replacementObject = GameObject.Instantiate(StaticDatabase.GetModelByName(wreckName).prefab);
                replacementObject.name = "wreck_" + counter;
                replacementObject.transform.position = target.position;
                replacementObject.transform.rotation = target.rotation;

                replacementObject.transform.parent = instance.wreck.transform;
                Vector3 localScale = replacementObject.transform.localScale;
                replacementObject.transform.localScale = new Vector3(localScale.x * scale * extrascale, 0.75f * Math.Min(1f, scale * extrascale), localScale.z * scale * extrascale);
                replacementObject.SetActive(false);
                float offset = max / 2 - ((counter - 0.5f) + ((1 + extrascale) / 2)) * (wrecksize * scale);
                Vector3 pos;
                if (staticBounds.size.x == max)
                {
                    pos = new Vector3(offset, 0, 0);
                }
                else
                {
                    pos = new Vector3(0, 0, offset);
                }

                replacementObject.transform.localPosition += pos;

                DestructibleBuilding.CollapsibleObject collapsible = new DestructibleBuilding.CollapsibleObject();

                collapsible.collapseBehaviour = DestructibleBuilding.CollapsibleObject.Behaviour.Collapse;
                collapsible.collapseDuration = 5f;
                collapsible.collapseObject = target.gameObject;
                collapsible.repairDuration = 0;
                collapsible.replaceDelay = 0.8f;
                collapsible.replacementObject = replacementObject;

                collapsible.collapseTiltMax = new Vector3(5, 0, 5);
                collapsible.collapseOffset = new Vector3(0, -staticBounds.size.y / 4, 0);

                collapsible.SecondaryFXPrefab = secondaryPrefab;
                collapsible.Init();

                //       Log.Normal("Added collapsible: " + instance.model.name + " : " + pos.ToString()  + " : " + counter);

                collapsible.sharedWith = instance.destructible;
                allCollapsibles.Add(collapsible);

                times -= 1f;
                counter++;
            }

        }


    }
}
