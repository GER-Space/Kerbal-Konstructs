using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    class KKCallBackWorker : MonoBehaviour
    {

        private bool isInitialized = false;

        private Dictionary<Collider,Part> includedParts = new Dictionary<Collider,Part>();

        internal Action<Part> onEnterAction = delegate { };
        internal Action<Part> onStayAction = delegate { };
        internal Action<Part> onExitAction = delegate { };

        internal void Start()
        {
            if (!isInitialized)
            {
                isInitialized = true;
            }
        }

        internal void RemovePart(Part part)
        {
            if (includedParts.ContainsKey(part.collider))
            {
                includedParts.Remove(part.collider);
            }
        }


        internal void OnTriggerEnter(Collider partCollider)
        {
            if (includedParts.ContainsKey(partCollider))
            {
                onEnterAction.Invoke(includedParts[partCollider]);
            }
            else
            {
                Part mypart = GetPartForCollider(partCollider);
                includedParts.Add(partCollider, mypart);
                onEnterAction.Invoke(mypart);
            }
        }


        internal void OnTriggerStay(Collider partCollider)
        {
            if (includedParts.ContainsKey(partCollider))
            {
                onStayAction.Invoke(includedParts[partCollider]);
            }
            else
            {
                Part mypart = GetPartForCollider(partCollider);
                includedParts.Add(partCollider, mypart);
            }
        }

        internal void OnTriggerExit(Collider partCollider)
        {
            if (includedParts.ContainsKey(partCollider))
            {
                onExitAction.Invoke(includedParts[partCollider]);
                includedParts.Remove(partCollider);
            }
            else
            {
                Part mypart = GetPartForCollider(partCollider);
                onExitAction.Invoke(mypart);               
            }
        }


        internal static Part GetPartForCollider(Collider collider)
        {

            foreach (Part part in FlightGlobals.PersistentLoadedPartIds.Values)
            {
                if (part.collider == collider)
                {
                    Log.Normal("Found part for collider: " + part.name);
                    return part;
                }
            }
            return null;
        }
    }
}
