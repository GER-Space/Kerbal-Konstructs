using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    static class KKClassExtentions
    {
        public static Transform FindRecursive(this Transform transform, string name)
        {
            foreach (Transform child in transform.gameObject.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == name)
                {
                    return child;
                }
            }

            return null;

            //var result = aParent.Find(name);
            //if (result != null)
            //    return result;
            //foreach (Transform child in aParent)
            //{
            //    result = child.FindDeepChild(name);
            //    if (result != null)
            //        return result;
            //}
            //return null;
        }

    }
}
