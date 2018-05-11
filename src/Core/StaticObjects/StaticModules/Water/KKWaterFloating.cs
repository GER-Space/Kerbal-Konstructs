using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    class KKWaterFloating : MonoBehaviour
    {

        private bool isInitialized = false;
        internal double waterLevel = 0f;

        private Dictionary<Collider,Part> submergedParts = new Dictionary<Collider,Part>();

        

        internal void Start()
        {
            if (!isInitialized)
            {

                //GameEvents.onPartDestroyed.Add(RemovePart);
                isInitialized = true;
            }

        }

        internal void RemovePart(Part part)
        {
            if (submergedParts.ContainsKey(part.collider))
            {
                submergedParts.Remove(part.collider);
            }
        }




        internal void OnTriggerEnter(Collider partCollider)
        {
            if (submergedParts.ContainsKey(partCollider))
            {
                submergedParts[partCollider].partBuoyancy.waterLevel = waterLevel;
            }
            else
            {
                Part mypart = GetPartForCollider(partCollider);
                submergedParts.Add(partCollider, mypart);
                mypart.partBuoyancy.waterLevel = waterLevel;
            }


        }


        internal void OnTriggerStay(Collider partCollider)
        {



        }

        internal void OnTriggerExit(Collider partCollider)
        {
            submergedParts[partCollider].partBuoyancy.waterLevel = 0;
        }


        internal static Part GetPartForCollider(Collider collider)
        {

            foreach (Part part in FlightGlobals.PersistentLoadedPartIds.Values)
            {
                if (part.collider == collider)
                {
                    Log.Normal("Found Floating Part");
                    return part;
                }

            }

            return null;

        }



    }
}
