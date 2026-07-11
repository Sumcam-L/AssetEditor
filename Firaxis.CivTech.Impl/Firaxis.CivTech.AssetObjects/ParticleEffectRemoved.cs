using System;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class ParticleEffectRemoved : EntityChangedEvent, IParticleEffectRemoved
{
	private string m_particleName = string.Empty;

	public virtual string ParticleEffectName
	{
		get
		{
			return m_particleName;
		}
		set
		{
			m_particleName = value;
		}
	}

	public ParticleEffectRemoved()
	{
		SetChangeType(EntityChangeType.ECT_PARTICLE_EFFECT_REMOVED);
	}

	public unsafe override void AddToChangeList(global::AssetObjects.EntityChangeList* changeList)
	{
		StandardStringWrapper standardStringWrapper = null;
		if (string.IsNullOrEmpty(m_particleName))
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HD_0040PCDFPFCF_0040Attempted_003F5to_003F5add_003F5a_003F5particle_003F5effe_0040), __arglist());
			global::_003CModule_003E.Platform_002EAssertSilently((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IE_0040FLHKHBGO_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 21u);
			return;
		}
		global::AssetObjects.ParticleEffectRemoved* ptr = AddToChangeList_Checked_003CAssetObjects_003A_003AParticleEffectRemoved_003E(changeList);
		if (ptr != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_particleName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EParticleEffectRemoved_002ESetParticleEffectName(ptr, standardStringWrapper.Value);
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
