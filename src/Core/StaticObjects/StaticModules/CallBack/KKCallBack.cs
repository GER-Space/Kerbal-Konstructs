using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;
using UnityEngine;

namespace KerbalKonstructs
{
    class KKCallBack : StaticModule
    {
        public string ColliderNames = "";
        private List<Collider> triggerColliders = new List<Collider>();

        private List<string> colNames = new List<string>();
        private string[] seperators = new string[] { " ", ",", ";" };

        private List<KKCallBackWorker> allCallBacks = new List<KKCallBackWorker>();

        internal bool isSetup = false;

        internal void Start()
        {
            if (!isSetup)
            {
                colNames = ColliderNames.Split(seperators, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (colNames.Count == 0)
                {
                    Log.UserError("No trigger colliders found");
                    return;
                }

                AddColliderworkers();
                isSetup = true;
            }
        }


        private void AddColliderworkers()
        {
            foreach (Collider col in gameObject.GetComponentsInChildren<Collider>(true))
            {
                if (colNames.Contains(col.name))
                {
                    KKCallBackWorker callBack = col.gameObject.AddComponent<KKCallBackWorker>();
                    allCallBacks.Add(callBack);
                }
            }
        }


        internal void RegisterEnterFunc(Action<Part> function)
        {
            foreach (KKCallBackWorker cb in allCallBacks)
            {
                cb.onEnterAction += function;
            }
        }


        internal void RegisterStayFunc(Action<Part> function)
        {
            foreach (KKCallBackWorker cb in allCallBacks)
            {
                cb.onStayAction += function;
            }
        }

        internal void RegisterExitFunc(Action<Part> function)
        {
            foreach (KKCallBackWorker cb in allCallBacks)
            {
                cb.onExitAction += function;
            }
        }
    }
}
