using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Barracks : KKFacility
    {
        [CFGSetting]
        public float StaffMax;
        [CareerSetting]
        public float StaffCurrent;
        [CareerSetting]
        public float ProductionRateCurrent;

    }
}
