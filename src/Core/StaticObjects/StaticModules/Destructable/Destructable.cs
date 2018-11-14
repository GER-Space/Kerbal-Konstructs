using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KerbalKonstructs.Core;

namespace KerbalKonstructs
{
    class Destructable
    {
        private static bool isInitialized = false;
        private static VFXSequencer demolitionPrefab;

        private static void Initialize()
        {
            DestructibleBuilding building = StaticDatabase.GetModelByName("KSC_VehicleAssemblyBuilding_level_3").prefab.GetComponentInChildren<DestructibleBuilding>();
            demolitionPrefab = building.DemolitionFXPrefab;

            isInitialized = true;
        }



        internal static void MakeDestructable(StaticInstance instance)
        {

            if (!isInitialized)
            {
                Initialize();        
            }



            GameObject oldGO = instance.gameObject;
            GameObject newBaseObject = new GameObject(oldGO.name);
            if (newBaseObject == null)
            {
                Log.Normal("Cannot create GO");
                return;
            }
            oldGO.name = "Static";
            newBaseObject.SetActive(false);
            GameObject.DontDestroyOnLoad(newBaseObject);
            
            DestructibleBuilding building = newBaseObject.AddComponent<DestructibleBuilding>();
            building.enabled = false;
            newBaseObject.transform.position = instance.gameObject.transform.position;
            newBaseObject.transform.rotation = instance.gameObject.transform.rotation;

            newBaseObject.transform.parent = instance.gameObject.transform.parent;

            instance.gameObject.transform.parent = newBaseObject.transform;

            instance.gameObject = newBaseObject;

      //      oldGO.SetActive(true);
            Bounds staticBounds = oldGO.GetRendererBounds();
       //     oldGO.SetActive(false);

            float min = Math.Min(staticBounds.size.x, staticBounds.size.z);
            float max = Math.Max(staticBounds.size.x, staticBounds.size.z);

            float scale = min / 70;
            float times = max / min;

            Vector3 center = new Vector3(staticBounds.center.x, 0, staticBounds.center.y);
            List<DestructibleBuilding.CollapsibleObject> allCollapsibles = new List<DestructibleBuilding.CollapsibleObject>();

            int counter = 0;
            while (times > 0.3f)
            {
                float extrascale = Math.Min(times, 1);
                GameObject replacementObject = GameObject.Instantiate(StaticDatabase.GetModelByName("KSC_LaunchPad_level_2_wreck_1").prefab);
                replacementObject.transform.position = instance.gameObject.transform.position;
                replacementObject.transform.rotation = instance.gameObject.transform.rotation;
                replacementObject.transform.parent = instance.gameObject.transform;
                Vector3 localScale = replacementObject.transform.localScale;
                replacementObject.transform.localScale = new Vector3(localScale.x * scale* extrascale, 0.75f * Math.Min(1f, scale * extrascale), localScale.z * scale * extrascale);
                replacementObject.SetActive(false);
                float offset = max / 2 - ((counter - 0.5f) + ((1+ extrascale)/2)) * (70 * scale );
                Vector3 pos;
                if (staticBounds.size.x == max)
                {
                    pos =  new Vector3(offset, 0, 0);
                }
                else
                {
                    pos =  new Vector3(0, 0, offset);
                }

                replacementObject.transform.localPosition = pos;

                DestructibleBuilding.CollapsibleObject collapsible = new DestructibleBuilding.CollapsibleObject();

                collapsible.collapseBehaviour = DestructibleBuilding.CollapsibleObject.Behaviour.Collapse;
                collapsible.collapseDuration = 1f;
                collapsible.collapseObject = oldGO;
                collapsible.repairDuration = 0;
                collapsible.replaceDelay = 0.8f;
                collapsible.replacementObject = replacementObject;
               // collapsible.SecondaryFXPrefab = scondaryPrefab;
                collapsible.Init();

         //       Log.Normal("Added collapsible: " + instance.model.name + " : " + pos.ToString()  + " : " + counter);

                collapsible.sharedWith = building;
                allCollapsibles.Add(collapsible);

                times -= 1f;
                counter++;
            }

            building.CollapsibleObjects = allCollapsibles.ToArray();
            building.CollapseReputationHit = 0;
            building.id = instance.UUID;
            building.preCompiledId = true;
            building.DemolitionFXPrefab = demolitionPrefab;
            building.FxTarget = instance.gameObject.transform;

            building.enabled = true;
        }

    }
}
