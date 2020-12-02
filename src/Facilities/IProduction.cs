using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
	public interface IProduction
	{
		int StaffMax { get; set; }
		int StaffCurrent { get; }
		string Produces { get; } // what the facility produces
		double ProductionRate { get; set; }// how much per staff-day
		double ProductionMax { get; set; }// how much can be stored
		double CurrentRate { get; }// how much per day (staff * rate)
		double UpdateProduction ();
		void TransmitProduction();

		StaticInstance StaticInstance { get; } //FIXME should not need
		bool AssignStaff();
		bool UnassignStaff();
	}
}
