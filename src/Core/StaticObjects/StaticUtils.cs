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




    }
}


