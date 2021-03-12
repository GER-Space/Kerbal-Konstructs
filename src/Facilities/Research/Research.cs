using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Research : KKFacility, IProduction
    {
		public string Produces { get { return KKLocalization.Science; } }
        [CFGSetting]
        public double ProductionRate { get; set; }
		public double CurrentRate
		{
			get { return StaffCurrent * ProductionRate; }
		}
		public double ProductionMax
		{
			get { return ScienceOMax; }
			set { ScienceOMax = value; }
		}

		[CFGSetting]
		public int StaffMax { get; set; }
		[CareerSetting]
		public int StaffCurrent { get; set; }

        //[CFGSetting]
        //public int StaffMax;

        //[CareerSetting]
        //public float StaffCurrent = 0;
        //[CareerSetting]
        //public float ProductionRateCurrent = 0;
        [CareerSetting]
        public double LastCheck { get; set; }

        [CFGSetting]
        public double ScienceOMax;

        [CareerSetting]
        public double ScienceOCurrent;

		public double UpdateProduction()
		{
			if (ProductionRate < 0.1f) {
				ProductionRate = 0.1f;
			}
			if (ScienceOMax < 1) {
				ScienceOMax = ProductionRate;
			}
			if (ScienceOMax < 1) {
				ScienceOMax = 10;
			}

			double currentTime = Planetarium.GetUniversalTime();

			if (LastCheck > currentTime) {
				LastCheck = currentTime;
			}
			var dtFmt = KSPUtil.dateTimeFormatter;
			double daysPast = (currentTime - LastCheck) / dtFmt.Day;
			double produced = daysPast * CurrentRate;
			if (produced > ScienceOMax) {
				produced = ScienceOMax;
			}
			return produced;
		}

		public void TransmitProduction()
		{
			float produced = (float)UpdateProduction();
			LastCheck = Planetarium.GetUniversalTime();
			//FIXME not cheating!!!
			ResearchAndDevelopment.Instance.AddScience(produced, TransactionReasons.Cheating);
			ScienceOCurrent = 0;
		}

		public StaticInstance StaticInstance { get { return staticInstance; } }

		public bool AssignStaff()
		{
			if (StaffCurrent < StaffMax) {
				StaffCurrent += 1;
				return true;
			}
			return false;
		}

		public bool UnassignStaff()
		{
			if (StaffCurrent > 0) {
				StaffCurrent -= 1;
				return true;
			}
			return false;
		}
    }
}
