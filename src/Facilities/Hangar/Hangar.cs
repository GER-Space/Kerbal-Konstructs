using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Hangar : KKFacility
    {
        [CareerSetting]
        public string InStorage1 = "";
        [CareerSetting]
        public string InStorage2 = "";
        [CareerSetting]
        public string InStorage3 = "";


        [CFGSetting]
        public float FacilityMassCapacity;
        [CFGSetting]
        public int FacilityCraftCapacity;


    }
}
