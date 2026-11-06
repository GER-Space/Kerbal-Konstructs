using KerbalKonstructs.Modules;
using KerbalKonstructs.UI;
using KSP.UI;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    internal class KKMouseUtility : MonoBehaviour
    {
        internal bool initialized = false;
        internal StaticInstance staticInstance = null;

        internal void Initialize()
        {
            if (this.gameObject == null)
            {
                Destroy(this);
            }
            if (staticInstance == null)
            {
                staticInstance = InstanceUtil.GetStaticInstanceForGameObject(this.gameObject);
            }
            if (staticInstance == null)
            {
                Log.UserInfo("Cound not determin instance for mouse selector");
                Destroy(this);
            }
            initialized = true;
        }

        void OnMouseDown()
        {
            if (!initialized)
            {
                Initialize();
            }

            API.OnStaticClicked.Invoke(staticInstance);
        }

        void OnMouseEnter()
        {
            if (!initialized)
            {
                Initialize();
            }

            API.OnStaticMouseEnter.Invoke(staticInstance);
        }

        void OnMouseExit()
        {
            if (!initialized)
            {
                Initialize();
            }

            API.OnStaticMouseExit.Invoke(staticInstance);
        }
    }
}
