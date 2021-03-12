using KerbalKonstructs.Core;

namespace KerbalKonstructs.Modules
{
	public interface IBarracks
	{
		int StaffMax { get; set; }
		int StaffCurrent { get; }
		int StaffAvailable { get; }

		bool HireStaff();
		bool FireStaff();

		bool DrawStaff();
		bool ReturnStaff();
	}
}

