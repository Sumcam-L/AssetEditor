using System.Runtime.CompilerServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class FireFXConfigDataWrapper : IFireFXConfigData
{
	private unsafe global::AssetObjects.IFireFXInstanceData* m_pkInstData;

	public unsafe FireFXConfigDataWrapper(global::AssetObjects.IFireFXInstanceData* pInstData)
	{
		//IL_0038: Expected I, but got I8
		m_pkInstData = pInstData;
		base._002Ector();
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003F_003F0FireFXConfigDataWrapper_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVIFireFXInstanceData_00402_0040_0040Z_00404_NA && m_pkInstData == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0N_0040NLAKBNGF_0040m_pkInstData_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040IPEDOJCM_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 143u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003F_003F0FireFXConfigDataWrapper_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVIFireFXInstanceData_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}

	private unsafe global::AssetObjects.IFireFXConfigData* GetConfigData()
	{
		return (global::AssetObjects.IFireFXConfigData*)m_pkInstData;
	}
}
