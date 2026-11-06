using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Barracks : KKFacility
    {
        [CFGSetting]
        public float StaffMax;
        [CareerSetting]
        public float StaffCurrent = 0;
        [CareerSetting]
        public float ProductionRateCurrent;


    }
}
