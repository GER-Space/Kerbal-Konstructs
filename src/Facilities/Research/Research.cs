using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Research : KKFacility
    {
        [CFGSetting]
        public int StaffMax;
        [CareerSetting]
        public int StaffCurrent;

        [CFGSetting]
        public float ScienceOMax;
        [CFGSetting]
        public float ProductionRateMax;

        [CareerSetting]
        public float ScienceOCurrent;
        [CareerSetting]
        public float LastCheck;
        [CareerSetting]
        public float ProductionRateCurrent;


    }
}
