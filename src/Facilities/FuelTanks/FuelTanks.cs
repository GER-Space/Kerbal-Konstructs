using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    internal class FuelTanks : KKFacility
    {
        [CFGSetting]
        public float LqFMax;
        [CFGSetting]
        public float OxFMax;
        [CFGSetting]
        public float MoFMax;

        [CFGSetting]
        public string LqFAlt = "";
        [CFGSetting]
        public string OxFAlt = "";
        [CFGSetting]
        public string MoFAlt = "";

        [CareerSetting]
        public float LqFCurrent;
        [CareerSetting]
        public float OxFCurrent;
        [CareerSetting]
        public float MoFCurrent;
    }
}
