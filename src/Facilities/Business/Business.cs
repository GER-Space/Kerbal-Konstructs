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
        [CareerSetting]
        public int StaffCurrent = 0;

        [CFGSetting]
        public float FundsOMax;
        [CFGSetting]
        public float ProductionRateMax;

        [CareerSetting]
        public float FundsOCurrent;
        [CareerSetting]
        public float LastCheck = 0;
        [CareerSetting]
        public float ProductionRateCurrent = 0;


    }
}
