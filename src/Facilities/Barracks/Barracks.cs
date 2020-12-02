using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
    class Barracks : KKFacility, IBarracks
    {
        [CFGSetting]
        public int StaffMax { get; set; }
        [CareerSetting]
        public int StaffCurrent { get; set; }
        [CareerSetting]
        public int StaffAvailable { get; set; }

		public const float hireFundCost = 5000;
		public const float fireRefund = 2500;
		public const float fireRepCost = 1;

		public bool HireStaff()
		{
			if (StaffCurrent < StaffMax) {
				StaffCurrent += 1;
				StaffAvailable += 1;
				//FIXME cheating???
				Funding.Instance.AddFunds(-hireFundCost, TransactionReasons.Cheating);
				return true;
			}
			return false;
		}

		public bool FireStaff()
		{
			if (StaffCurrent > 0 && StaffAvailable > 0) {
				StaffCurrent -= 1;
				StaffAvailable -= 1;
				Funding.Instance.AddFunds(fireRefund, TransactionReasons.Cheating);
				Reputation.Instance.AddReputation(-fireRepCost, TransactionReasons.Cheating);
				return true;
			}
			return false;
		}

		public bool DrawStaff()
		{
			if (StaffAvailable > 0) {
				StaffAvailable -= 1;
				return true;
			}
			return false;
		}

		public bool ReturnStaff()
		{
			if (StaffAvailable < StaffCurrent) {
				StaffAvailable -= 1;
				return true;
			}
			return false;
		}
    }
}
