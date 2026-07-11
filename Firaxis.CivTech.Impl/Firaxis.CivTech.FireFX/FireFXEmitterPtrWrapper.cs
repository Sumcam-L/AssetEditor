using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using AssetObjects.FireFX;
using Firaxis.CivTech.AssetObjects;
using Platform;
using std;

namespace Firaxis.CivTech.FireFX;

public class FireFXEmitterPtrWrapper : IFireFXEmitter, IDisposable
{
	private unsafe shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E* m_emitterPtr;

	private IList<IMaterialClass> m_validMaterialClasses;

	private IList<IGeometryClass> m_validGeometeryClasses;

	public virtual IEnumerable<IGeometryClass> ValidGeometries => m_validGeometeryClasses;

	public virtual IEnumerable<IMaterialClass> ValidMaterials => m_validMaterialClasses;

	public unsafe virtual BlendMode BlendMode
	{
		get
		{
			//IL_0017: Expected I, but got I8
			global::AssetObjects.IFireFXEmitter* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002E_002D_003E(m_emitterPtr);
			return (BlendMode)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.FireFX.BlendMode>)(*(ulong*)(*(long*)intPtr + 48)))((nint)intPtr);
		}
	}

	public unsafe virtual EmitterFlags Flags
	{
		get
		{
			//IL_0017: Expected I, but got I8
			global::AssetObjects.IFireFXEmitter* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002E_002D_003E(m_emitterPtr);
			return (EmitterFlags)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint>)(*(ulong*)(*(long*)intPtr + 24)))((nint)intPtr);
		}
	}

	public unsafe virtual string Name
	{
		get
		{
			//IL_0017: Expected I, but got I8
			global::AssetObjects.IFireFXEmitter* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002E_002D_003E(m_emitterPtr);
			return new string(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)intPtr + 16)))((nint)intPtr));
		}
	}

	public unsafe virtual string FullName
	{
		get
		{
			//IL_0016: Expected I, but got I8
			global::AssetObjects.IFireFXEmitter* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002E_002D_003E(m_emitterPtr);
			return new string(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)intPtr + 8)))((nint)intPtr));
		}
	}

	public unsafe FireFXEmitterPtrWrapper(shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E* emitterPtr)
	{
		//IL_0019: Expected I, but got I8
		//IL_004e: Expected I, but got I8
		//IL_007e: Expected I, but got I8
		shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E* ptr = (shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E*)global::_003CModule_003E.@new(16uL);
		m_emitterPtr = ((ptr == null) ? null : global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002E_007Bctor_007D(ptr, emitterPtr));
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x7be354fa_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0FireFXEmitterPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXEmitter_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA && m_emitterPtr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0N_0040GMALNGCL_0040m_emitterPtr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040BEIHCEEJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 14u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7be354fa_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0FireFXEmitterPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXEmitter_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x7be354fa_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0FireFXEmitterPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXEmitter_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA && !global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002E_002E_N(m_emitterPtr) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040KNJHELCN_0040_003F_0024CKm_emitterPtr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GM_0040BEIHCEEJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 15u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x7be354fa_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0FireFXEmitterPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXEmitter_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_validMaterialClasses = new List<IMaterialClass>();
		m_validGeometeryClasses = new List<IGeometryClass>();
	}

	private void _007EFireFXEmitterPtrWrapper()
	{
		_0021FireFXEmitterPtrWrapper();
	}

	private unsafe void _0021FireFXEmitterPtrWrapper()
	{
		shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E* emitterPtr = m_emitterPtr;
		if (emitterPtr != null)
		{
			global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002Ereset(emitterPtr);
			shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E* emitterPtr2 = m_emitterPtr;
			if (emitterPtr2 != null)
			{
				global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002E_007Bdtor_007D(emitterPtr2);
				global::_003CModule_003E.delete(emitterPtr2, 16uL);
			}
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EFireFXEmitterPtrWrapper();
			return;
		}
		try
		{
			_0021FireFXEmitterPtrWrapper();
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~FireFXEmitterPtrWrapper()
	{
		Dispose(A_0: false);
	}
}
