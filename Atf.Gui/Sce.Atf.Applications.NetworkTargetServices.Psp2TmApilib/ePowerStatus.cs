using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Sce.Atf.Applications.NetworkTargetServices.Psp2TmApilib;

[TypeConverter(typeof(PowerStatusEnumConverter))]
[Guid("FBED4567-ABF5-4A96-9D9A-45017FA203A6")]
internal enum ePowerStatus
{
	POWER_STATUS_OFF = 0,
	POWER_STATUS_NO_SUPPLY = 1,
	POWER_STATUS_ON = 0x100,
	POWER_STATUS_SUSPENDED = 0x200
}
