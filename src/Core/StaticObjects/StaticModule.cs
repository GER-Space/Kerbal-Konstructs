using System;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalKonstructs.Core
{
    public class StaticModule : MonoBehaviour
    {
        public string moduleNamespace;
        public string moduleClassname;
        public Dictionary<String, String> moduleFields = new Dictionary<string, string>();
        public StaticInstance staticInstance = null;

        public virtual void StaticObjectUpdate() { }
    }
}
