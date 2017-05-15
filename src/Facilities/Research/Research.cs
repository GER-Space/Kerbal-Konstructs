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
        [CFGSetting]
        public float ProductionRateMax;

        [CareerSetting]
        public float StaffCurrent = 0;
        [CareerSetting]
        public float ProductionRateCurrent = 0;
        [CareerSetting]
        public float LastCheck = 0;

        [CFGSetting]
        public float ScienceOMax;

        [CareerSetting]
        public float ScienceOCurrent;


    }
}
