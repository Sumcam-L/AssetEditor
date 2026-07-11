using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using std;

namespace Firaxis.CivTech.FireFX;

public class FireFXEffectPtrWrapper : IFireFXEffect, IDisposable
{
	private unsafe shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E* m_scriptPtr;

	private IList<IFireFXEmitter> m_emitterWrappers;

	public virtual IEnumerable<IFireFXEmitter> Emitters => m_emitterWrappers;

	public unsafe virtual string Name
	{
		get
		{
			//IL_0016: Expected I, but got I8
			IFireFXScript* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_002D_003E(m_scriptPtr);
			return new string(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)intPtr + 8)))((nint)intPtr));
		}
	}

	public unsafe FireFXEffectPtrWrapper(shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E* sciprtPtr)
	{
		//IL_001e: Expected I, but got I8
		//IL_007d: Expected I, but got I8
		//IL_0054: Expected I, but got I8
		//IL_00a3: Expected I, but got I8
		//IL_02c8: Expected I, but got I8
		//IL_00ff: Expected I, but got I8
		//IL_022a: Expected I, but got I8
		//IL_019f: Expected I, but got I8
		shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E* ptr = (shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E*)global::_003CModule_003E.@new(16uL);
		m_scriptPtr = ((ptr == null) ? null : global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bctor_007D(ptr, sciprtPtr));
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x943cd082_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0FireFXEffectPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXScript_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA && m_scriptPtr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040NDNGPOCB_0040m_scriptPtr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040OMDAKAKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 19u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x943cd082_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0FireFXEffectPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXScript_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_emitterWrappers = new List<IFireFXEmitter>();
		IFireFXScript* ptr2 = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002Eget(m_scriptPtr);
		int num = 0;
		int num2 = (int)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint>)(*(ulong*)(*(long*)ptr2 + 16)))((nint)ptr2);
		if (0 >= num2)
		{
			return;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ResultCode resultCode);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ResultCode resultCode2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.DateTime dateTime);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BAA_0040D _0024ArrayType_0024_0024_0024BY0BAA_0040D2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D4);
		do
		{
			global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002E_007Bctor_007D(&shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E2);
			try
			{
				global::_003CModule_003E.Platform_002EResultCode_002E_007Bctor_007D(&resultCode, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ResultCode*, uint, shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E*, ResultCode*>)(*(ulong*)(*(long*)ptr2 + 24)))((nint)ptr2, &resultCode2, (uint)num, &shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E2));
				if (global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
				{
					m_emitterWrappers.Add(new FireFXEmitterPtrWrapper(&shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E2));
				}
				else
				{
					ulong num3 = 1uL;
					ulong num4 = 1uL;
					global::_003CModule_003E.Platform_002EGetLoggingOptions(&num3, &num4);
					if (num3 != 0L)
					{
						global::_003CModule_003E.Platform_002ELocalTimeFromEpoch(&dateTime, global::_003CModule_003E.Platform_002EGetSecondsSinceEpoch());
						uint num5 = (uint)global::_003CModule_003E.Platform_002Evscprintf_dargs((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CI_0040KBLGBKBP_0040Failed_003F5to_003F5get_003F5emitter_003F5_003F_0024CFd_003F5from_003F5sc_0040), __arglist(num, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)ptr2 + 8)))((nint)ptr2)));
						uint num6 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), 23uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CB_0040BJCDHDJ_0040_003F_0024FL_003F_0024CF04d_003F9_003F_0024CF02d_003F9_003F_0024CF02d_003F5_003F_0024CF02d_003F3_003F_0024CF02d_003F3_003F_0024CF02d_003F_0024FN_003F7_0040), __arglist(*(uint*)(&dateTime), System.Runtime.CompilerServices.Unsafe.As<Platform.DateTime, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dateTime, 4)), System.Runtime.CompilerServices.Unsafe.As<Platform.DateTime, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dateTime, 8)), System.Runtime.CompilerServices.Unsafe.As<Platform.DateTime, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dateTime, 12)), System.Runtime.CompilerServices.Unsafe.As<Platform.DateTime, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dateTime, 16)), System.Runtime.CompilerServices.Unsafe.As<Platform.DateTime, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dateTime, 20))));
						uint num7 = num6 + num5;
						if (num7 > 279)
						{
							ulong num8 = num7 + 1;
							sbyte* ptr3 = (sbyte*)global::_003CModule_003E.Platform_002EMalloc(num8, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040OMDAKAKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 35, 3, 0);
							sbyte* ptr4 = (sbyte*)global::_003CModule_003E.Platform_002EMalloc(num8, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040OMDAKAKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 35, 3, 0);
							global::_003CModule_003E.Platform_002Ememset(ptr3, 0, num8);
							global::_003CModule_003E.Platform_002Ememset(ptr4, 0, num8);
							ulong num9 = num7;
							global::_003CModule_003E.Platform_002Esnprintf(ptr3, num9, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CI_0040KBLGBKBP_0040Failed_003F5to_003F5get_003F5emitter_003F5_003F_0024CFd_003F5from_003F5sc_0040), __arglist(num, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)ptr2 + 8)))((nint)ptr2)));
							global::_003CModule_003E.Platform_002Estrncat(ptr4, (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), num9);
							global::_003CModule_003E.Platform_002Estrncat(ptr4, ptr3, num9);
							global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040PICHLLLM_0040A_003F_0024AAs_003F_0024AAs_003F_0024AAe_003F_0024AAt_003F_0024AAP_003F_0024AAr_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAe_003F_0024AAw_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_003F_0024AA_0040), ptr4, num7);
							if (num4 != 0L && !global::_003CModule_003E._003FA0x943cd082_002E_003FbIgnoreAlways_0040_003FBN_0040_003F_003F_003F0FireFXEffectPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXScript_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA)
							{
								global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, ptr3, __arglist());
								if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05FBJJDIO_0040_003F_0024CBtrue_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040OMDAKAKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 35u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x943cd082_002E_003FbIgnoreAlways_0040_003FBN_0040_003F_003F_003F0FireFXEffectPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXScript_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA), (ErrorType)0))
								{
									/*OpCode not supported: DebugBreak*/;
								}
							}
							global::_003CModule_003E.Platform_002EFree(ptr3);
							global::_003CModule_003E.Platform_002EFree(ptr4);
						}
						else
						{
							global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BAA_0040D2), 256uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CI_0040KBLGBKBP_0040Failed_003F5to_003F5get_003F5emitter_003F5_003F_0024CFd_003F5from_003F5sc_0040), __arglist(num, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)ptr2 + 8)))((nint)ptr2)));
							global::_003CModule_003E.Platform_002Estrncat((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BAA_0040D2), 279 - num6);
							uint num10 = ((num7 < 279) ? num7 : 279u);
							global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040PICHLLLM_0040A_003F_0024AAs_003F_0024AAs_003F_0024AAe_003F_0024AAt_003F_0024AAP_003F_0024AAr_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAe_003F_0024AAw_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), num10);
							if (num4 != 0L && !global::_003CModule_003E._003FA0x943cd082_002E_003FbIgnoreAlways_0040_003FCI_0040_003F_003F_003F0FireFXEffectPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXScript_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA)
							{
								global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BAA_0040D2), __arglist());
								if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05FBJJDIO_0040_003F_0024CBtrue_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040OMDAKAKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 35u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x943cd082_002E_003FbIgnoreAlways_0040_003FCI_0040_003F_003F_003F0FireFXEffectPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXScript_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA), (ErrorType)0))
								{
									/*OpCode not supported: DebugBreak*/;
								}
							}
						}
					}
					else
					{
						uint num11 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CI_0040KBLGBKBP_0040Failed_003F5to_003F5get_003F5emitter_003F5_003F_0024CFd_003F5from_003F5sc_0040), __arglist(num, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)ptr2 + 8)))((nint)ptr2)));
						global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BO_0040PICHLLLM_0040A_003F_0024AAs_003F_0024AAs_003F_0024AAe_003F_0024AAt_003F_0024AAP_003F_0024AAr_003F_0024AAe_003F_0024AAv_003F_0024AAi_003F_0024AAe_003F_0024AAw_003F_0024AAe_003F_0024AAr_003F_0024AA_003F_0024AA_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), num11);
						if (num4 != 0L && !global::_003CModule_003E._003FA0x943cd082_002E_003FbIgnoreAlways_0040_003FDD_0040_003F_003F_003F0FireFXEffectPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXScript_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA)
						{
							global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D4), 2048uL, (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), __arglist());
							if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05FBJJDIO_0040_003F_0024CBtrue_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D4), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040OMDAKAKK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 35u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x943cd082_002E_003FbIgnoreAlways_0040_003FDD_0040_003F_003F_003F0FireFXEffectPtrWrapper_0040FireFX_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040AEBV_003F_0024shared_ptr_0040VIFireFXScript_0040AssetObjects_0040_0040_0040std_0040_0040_0040Z_00404_NA), (ErrorType)0))
							{
								/*OpCode not supported: DebugBreak*/;
							}
						}
					}
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002E_007Bdtor_007D), &shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E2);
				throw;
			}
			global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E_002E_007Bdtor_007D(&shared_ptr_003CAssetObjects_003A_003AIFireFXEmitter_003E2);
			num++;
		}
		while (num < num2);
	}

	private void _007EFireFXEffectPtrWrapper()
	{
		_0021FireFXEffectPtrWrapper();
	}

	private unsafe void _0021FireFXEffectPtrWrapper()
	{
		shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E* scriptPtr = m_scriptPtr;
		if (scriptPtr != null)
		{
			global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002Ereset(scriptPtr);
			shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E* scriptPtr2 = m_scriptPtr;
			if (scriptPtr2 != null)
			{
				global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D(scriptPtr2);
				global::_003CModule_003E.delete(scriptPtr2, 16uL);
			}
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EFireFXEffectPtrWrapper();
			return;
		}
		try
		{
			_0021FireFXEffectPtrWrapper();
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

	~FireFXEffectPtrWrapper()
	{
		Dispose(A_0: false);
	}
}
