using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;

namespace KerbalKonstructs.Core
{
    internal static class StaticUtils
    {
        /// <summary>
        /// Returns a StaticObject object for a gives GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
		internal static StaticObject getStaticFromGameObject(GameObject gameObject)
        {
            List<StaticObject> objList = (from obj in KerbalKonstructs.instance.staticDB.GetAllStatics() where obj.gameObject == gameObject select obj).ToList();

            if (objList.Count >= 1)
            {
                if (objList.Count > 1)
                    Log.UserError("More than one StaticObject references to GameObject " + gameObject.name);

                return objList[0];
            }

            Log.UserWarning("StaticObject doesn't exist for " + gameObject.name);
            return null;
        }

        /// <summary>
        /// Removes the wreck model from an KSC Object.
        /// </summary>
        internal static void MangleSquadStatic(GameObject gameObject)
        {
            gameObject.transform.parent = null;
            var transforms = gameObject.transform.GetComponentsInChildren<Transform>(true);
            foreach (var transform in transforms)
            {

                if (transform.name.Equals("wreck", StringComparison.InvariantCultureIgnoreCase))
                {
                    transform.parent = null;
                    GameObject.Destroy(transform.gameObject);
                }
            }
        }


    }
}


