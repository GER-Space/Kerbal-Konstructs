using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
	public interface IProduction
	{
		int StaffMax { get; set; }
		int StaffCurrent { get; set; }
		string Produces { get; } // what the facility produces
		double ProductionRate { get; set; }// how much per staff-day
		double ProductionMax { get; set; }// how much can be stored
		double CurrentRate { get; }// how much per day (staff * rate)
		double UpdateProduction ();
		void TransmitProduction();

		StaticInstance StaticInstance { get; }
		bool AssignStaff();
		bool UnassignStaff();
	}
}
