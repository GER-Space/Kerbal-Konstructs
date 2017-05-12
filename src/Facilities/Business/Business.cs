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
        public int StaffCurrent;

        [CFGSetting]
        public float FundsOMax;
        [CFGSetting]

        [CareerSetting]
        public float FundsOCurrent;
        [CareerSetting]
        public float LastCheck;
        [CareerSetting]
        public float ProductionRateCurrent;


    }
}
