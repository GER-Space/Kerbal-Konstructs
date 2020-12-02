using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
	public interface IBarracks
	{
		int StaffMax { get; set; }
		int StaffCurrent { get; set; }
		int StaffAvailable { get; set; }

		bool HireStaff();
		bool FireStaff();

		bool DrawStaff();
		bool ReturnStaff();
	}
}

