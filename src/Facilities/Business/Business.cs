using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Business : KKFacility
    {
        [CFGSetting]
        public int StaffMax;
        [CFGSetting]
        public float ProductionRateMax;

        [CareerSetting]
        public float StaffCurrent = 0;
        [CareerSetting]
        public float ProductionRateCurrent =0f;
        [CareerSetting]
        public float LastCheck = 0f;

        [CFGSetting]
        public float FundsOMax;
        [CareerSetting]
        public float FundsOCurrent;



    }
}
