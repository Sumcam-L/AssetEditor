using System;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class LightTagDirectionRemoved : EntityChangedEvent, ILightTagDirectionRemoved
{
	private string m_lightTagDirectionName = string.Empty;

	public virtual string LightTagDirectionName
	{
		get
		{
			return m_lightTagDirectionName;
		}
		set
		{
			m_lightTagDirectionName = value;
		}
	}

	public LightTagDirectionRemoved()
	{
		SetChangeType(EntityChangeType.ECT_LIGHT_DIRECTION_TAG_REMOVED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		if (string.IsNullOrEmpty(m_lightTagDirectionName))
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HB_0040MJNOPGND_0040Attempted_003F5to_003F5add_003F5light_003F5tag_003F5direc_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IH_0040EFCJNJGA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 21u);
			return;
		}
		LightDirectionTagRemoved* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003ALightDirectionTagRemoved_003E(changeList);
		if (ptr != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_lightTagDirectionName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002ELightDirectionTagRemoved_002ESetLightTagName(ptr, standardStringWrapper.Value);
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
	}
}
