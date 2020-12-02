using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Business : KKFacility, IProduction
    {
		public string Produces { get { return KKLocalization.Funds; } }
		public double CurrentRate
		{
			get { return StaffCurrent * ProductionRate; }
		}
        [CFGSetting]
        public double ProductionRate { get; set; }
		public double ProductionMax
		{
			get { return FundsOMax; }
			set { FundsOMax = value; }
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
        //public float CurrentRate =0f;
        [CareerSetting]
        public double LastCheck { get; set; }

        [CFGSetting]
        public double FundsOMax;
        [CareerSetting]
        public double FundsOCurrent;

		public double UpdateProduction()
		{
			if (ProductionRate < 10) {
				ProductionRate = 10;
			}
			if (FundsOMax < 1) {
				FundsOMax = 10000;
			}

			double currentTime = Planetarium.GetUniversalTime();

			if (LastCheck > currentTime) {
				LastCheck = currentTime;
			}
			var dtFmt = KSPUtil.dateTimeFormatter;
			double daysPast = (currentTime - LastCheck) / dtFmt.Day;
			double produced = daysPast * CurrentRate;
			if (produced > FundsOMax) {
				produced = FundsOMax;
			}
			return produced;
		}

		public void TransmitProduction()
		{
			double produced = UpdateProduction();
			LastCheck = Planetarium.GetUniversalTime();
			//FIXME not cheating!!!
			Funding.Instance.AddFunds(produced, TransactionReasons.Cheating);
			FundsOCurrent = 0;
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
