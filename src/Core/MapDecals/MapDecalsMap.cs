using System.Collections.Generic;
using UnityEngine;
using KerbalKonstructs.Utilities;

namespace KerbalKonstructs.Core
{
	public class MapDecalsMap : MapSO
	{
        internal MapSO map = null;
        internal bool isHeightMap = false;
        internal Texture2D mapTexture = null;
        internal string path = null;

        [CFGSetting]
        public string Name = "";
        [CFGSetting]
        public string Image = "";
        [CFGSetting]
        public bool UseAsHeighMap = false;
    }
}
