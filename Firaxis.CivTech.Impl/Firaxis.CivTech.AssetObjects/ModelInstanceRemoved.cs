using System;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class ModelInstanceRemoved : EntityChangedEvent, IModelInstanceRemoved
{
	private string m_modelName = string.Empty;

	public virtual string ModelName
	{
		get
		{
			return m_modelName;
		}
		set
		{
			m_modelName = value;
		}
	}

	public ModelInstanceRemoved()
	{
		SetChangeType(EntityChangeType.ECT_MODEL_INSTANCE_REMOVED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		if (string.IsNullOrEmpty(m_modelName))
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FO_0040JLNMBEDM_0040Attempted_003F5to_003F5add_003F5an_003F5unassigned_003F5m_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0ID_0040POLKGDPF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 21u);
			return;
		}
		global::AssetObjects.ModelInstanceRemoved* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AModelInstanceRemoved_003E(changeList);
		if (ptr != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_modelName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EModelInstanceRemoved_002ESetModelName(ptr, standardStringWrapper.Value);
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
