using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEngine;
using KSP.UI.Screens;

namespace KerbalKonstructs.Core
{
    public class StaticUtils
    {
        public static StaticObject getStaticFromGameObject(GameObject gameObject)
        {
            List<StaticObject> objList = (from obj in KerbalKonstructs.instance.staticDB.GetAllStatics() where obj.gameObject == gameObject select obj).ToList();

            if (objList.Count >= 1)
            {
                if (objList.Count > 1)
                    Debug.Log("KK: WARNING: More than one StaticObject references to GameObject " + gameObject.name);

                return objList[0];
            }

            Debug.Log("KK: WARNING: StaticObject doesn't exist for " + gameObject.name);
            return null;
        }
    }
}


