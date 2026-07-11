using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.CivTech.FireFX._003FA0xce210b22;
using Firaxis.Error;
using FireFX;
using Platform;
using std;

namespace Firaxis.CivTech.FireFX;

public class FireFXCompilerService : IFireFXCompilerService
{
	private IVirtualPantry m_pmVirtualPantry;

	private unsafe shared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E* m_pFireFXCompiterPtr = null;

	public unsafe virtual IEnumerable<string> IntrinsicFunctionNames
	{
		get
		{
			//IL_005a: Expected I, but got I8
			//IL_007a: Expected I, but got I8
			//IL_0038: Expected I, but got I8
			if (!global::_003CModule_003E._003FA0xce210b22_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040IntrinsicFunctionNames_0040FireFXCompilerService_0040FireFX_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040XZ_00404_NA)
			{
				shared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E* pFireFXCompiterPtr = m_pFireFXCompiterPtr;
				if ((pFireFXCompiterPtr == null || !global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002E_N(pFireFXCompiterPtr)) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CO_0040JOGPBJHL_0040m_pFireFXCompiterPtr_003F5_003F_0024CG_003F_0024CG_003F5_003F_0024CKm_pFire_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GK_0040CMJHNKEF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 155u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xce210b22_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040IntrinsicFunctionNames_0040FireFXCompilerService_0040FireFX_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040XZ_00404_NA), (ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			IList<string> list = new List<string>();
			int num = 0;
			IFireFXCompiler* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002D_003E(m_pFireFXCompiterPtr);
			int num2 = (int)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint>)(*(ulong*)(*(long*)intPtr + 40)))((nint)intPtr);
			if (0 < num2)
			{
				do
				{
					IFireFXCompiler* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002D_003E(m_pFireFXCompiterPtr);
					list.Add(new string(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, uint, sbyte*>)(*(ulong*)(*(long*)ptr + 48)))((nint)ptr, (uint)num)));
					num++;
				}
				while (num < num2);
			}
			return list;
		}
	}

	public unsafe FireFXCompilerService()
	{
		//IL_0008: Expected I, but got I8
		//IL_0035: Expected I, but got I8
		shared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E* ptr = (shared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E*)global::_003CModule_003E.@new(16uL, (int)global::_003CModule_003E.Platform_002EGetMemBlockType(), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GK_0040CMJHNKEF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 55, 23, 0);
		m_pFireFXCompiterPtr = ((ptr == null) ? null : global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_007Bctor_007D(ptr));
	}

	private unsafe void _007EFireFXCompilerService()
	{
		shared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E* pFireFXCompiterPtr = m_pFireFXCompiterPtr;
		if (pFireFXCompiterPtr != null)
		{
			global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_007Bdtor_007D(pFireFXCompiterPtr);
			global::_003CModule_003E.delete(pFireFXCompiterPtr, 16uL);
		}
	}

	public unsafe virtual Firaxis.Error.ResultCode Startup(IVirtualPantry pantry)
	{
		//IL_002c: Expected I, but got I8
		m_pmVirtualPantry = pantry;
		VirtualPantry virtualPantry = (VirtualPantry)pantry;
		FireFXCompiler* ptr = (FireFXCompiler*)global::_003CModule_003E.@new(72uL);
		FireFXCompiler* ptr2;
		try
		{
			ptr2 = ((ptr == null) ? null : global::_003CModule_003E.AssetObjects_002EFireFXCompiler_002E_007Bctor_007D(ptr, virtualPantry.GetNativePantry()));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, 72uL);
			throw;
		}
		global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002Ereset_003Cclass_0020AssetObjects_003A_003AFireFXCompiler_003E(m_pFireFXCompiterPtr, ptr2);
		return Firaxis.Error.ResultCode.Success;
	}

	public unsafe virtual void Shutdown()
	{
		global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002Ereset(m_pFireFXCompiterPtr);
	}

	public unsafe virtual Firaxis.Error.ResultCode Compile(string scriptPath, string byteCodePath, IList<CompileIssue> issues)
	{
		//IL_0036: Expected I, but got I8
		//IL_0070: Expected I8, but got I
		//IL_009a: Expected I, but got I8
		IOStringWrapper iOStringWrapper = null;
		IOStringWrapper iOStringWrapper2 = null;
		if (!global::_003CModule_003E._003FA0xce210b22_002E_003FbIgnoreAlways_0040_003F2_003F_003FCompile_0040FireFXCompilerService_0040FireFX_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVResultCode_0040Error_00405_0040PE_0024AAVString_0040System_0040_00400PE_0024AAU_003F_0024IList_0040PE_0024AAVCompileIssue_0040FireFX_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_00409_0040_0040Z_00404_NA)
		{
			shared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E* pFireFXCompiterPtr = m_pFireFXCompiterPtr;
			if ((pFireFXCompiterPtr == null || !global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002E_N(pFireFXCompiterPtr)) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CO_0040JOGPBJHL_0040m_pFireFXCompiterPtr_003F5_003F_0024CG_003F_0024CG_003F5_003F_0024CKm_pFire_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GK_0040CMJHNKEF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 98u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xce210b22_002E_003FbIgnoreAlways_0040_003F2_003F_003FCompile_0040FireFXCompilerService_0040FireFX_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVResultCode_0040Error_00405_0040PE_0024AAVString_0040System_0040_00400PE_0024AAU_003F_0024IList_0040PE_0024AAVCompileIssue_0040FireFX_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_00409_0040_0040Z_00404_NA), (ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		IOStringWrapper iOStringWrapper3 = new IOStringWrapper(scriptPath);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out CompilerErrorListener compilerErrorListener);
		Firaxis.Error.ResultCode result;
		try
		{
			iOStringWrapper = iOStringWrapper3;
			IOStringWrapper iOStringWrapper4 = new IOStringWrapper(byteCodePath);
			try
			{
				iOStringWrapper2 = iOStringWrapper4;
				*(long*)(&compilerErrorListener) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7CompilerErrorListener_0040_003FA0xce210b22_0040FireFX_0040CivTech_0040Firaxis_0040_00406B_0040);
				System.Runtime.CompilerServices.Unsafe.As<CompilerErrorListener, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)) = (nint)((IntPtr)GCHandle.Alloc(issues)).ToPointer();
				try
				{
					IFireFXCompiler* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002D_003E(m_pFireFXCompiterPtr);
					System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
					System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode2);
					global::_003CModule_003E.Platform_002EResultCode_002E_007Bctor_007D(&resultCode, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, Platform.ResultCode*, char*, char*, IErrorListener*, Platform.ResultCode*>)(*(ulong*)(*(long*)ptr + 8)))((nint)ptr, &resultCode2, iOStringWrapper.Value, iOStringWrapper2.Value, (IErrorListener*)(&compilerErrorListener)));
					if (!global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
					{
						result = new Firaxis.Error.ResultCode(new string(global::_003CModule_003E.Platform_002EResultCode_002EGetMessage(&resultCode)));
						goto IL_00ce;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<CompilerErrorListener*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EFireFX_002E_003FA0xce210b22_002ECompilerErrorListener_002E_007Bdtor_007D), &compilerErrorListener);
					throw;
				}
				goto end_IL_004c;
				IL_00ce:
				global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)));
				goto IL_00e0;
				end_IL_004c:;
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper2).Dispose();
				throw;
			}
			goto end_IL_0041;
			IL_00e0:
			((IDisposable)iOStringWrapper2).Dispose();
			goto IL_00ef;
			end_IL_0041:;
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		Firaxis.Error.ResultCode success;
		try
		{
			try
			{
				try
				{
					success = Firaxis.Error.ResultCode.Success;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<CompilerErrorListener*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EFireFX_002E_003FA0xce210b22_002ECompilerErrorListener_002E_007Bdtor_007D), &compilerErrorListener);
					throw;
				}
				global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)));
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper2).Dispose();
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return success;
		IL_00ef:
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual Firaxis.Error.ResultCode Compile(string scriptPath, ref IFireFXEffect effect, IList<CompileIssue> issues)
	{
		//IL_0034: Expected I, but got I8
		//IL_006b: Expected I8, but got I
		//IL_0092: Expected I, but got I8
		IOStringWrapper iOStringWrapper = null;
		if (!global::_003CModule_003E._003FA0xce210b22_002E_003FbIgnoreAlways_0040_003F2_003F_003FCompile_0040FireFXCompilerService_0040FireFX_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVResultCode_0040Error_00405_0040PE_0024AAVString_0040System_0040_0040AE_0024CAPE_0024AAUIFireFXEffect_0040345_0040PE_0024AAU_003F_0024IList_0040PE_0024AAVCompileIssue_0040FireFX_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_00409_0040_0040Z_00404_NA)
		{
			shared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E* pFireFXCompiterPtr = m_pFireFXCompiterPtr;
			if ((pFireFXCompiterPtr == null || !global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002E_N(pFireFXCompiterPtr)) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CO_0040JOGPBJHL_0040m_pFireFXCompiterPtr_003F5_003F_0024CG_003F_0024CG_003F5_003F_0024CKm_pFire_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GK_0040CMJHNKEF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 79u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xce210b22_002E_003FbIgnoreAlways_0040_003F2_003F_003FCompile_0040FireFXCompilerService_0040FireFX_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVResultCode_0040Error_00405_0040PE_0024AAVString_0040System_0040_0040AE_0024CAPE_0024AAUIFireFXEffect_0040345_0040PE_0024AAU_003F_0024IList_0040PE_0024AAVCompileIssue_0040FireFX_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_00409_0040_0040Z_00404_NA), (ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
		global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bctor_007D(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out CompilerErrorListener compilerErrorListener);
		Firaxis.Error.ResultCode result;
		try
		{
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(scriptPath);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				*(long*)(&compilerErrorListener) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7CompilerErrorListener_0040_003FA0xce210b22_0040FireFX_0040CivTech_0040Firaxis_0040_00406B_0040);
				System.Runtime.CompilerServices.Unsafe.As<CompilerErrorListener, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)) = (nint)((IntPtr)GCHandle.Alloc(issues)).ToPointer();
				try
				{
					IFireFXCompiler* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002D_003E(m_pFireFXCompiterPtr);
					System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
					System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode2);
					global::_003CModule_003E.Platform_002EResultCode_002E_007Bctor_007D(&resultCode, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, Platform.ResultCode*, char*, shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E*, IErrorListener*, Platform.ResultCode*>)(*(ulong*)(*(long*)ptr + 16)))((nint)ptr, &resultCode2, iOStringWrapper.Value, &shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2, (IErrorListener*)(&compilerErrorListener)));
					if (!global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
					{
						result = new Firaxis.Error.ResultCode(new string(global::_003CModule_003E.Platform_002EResultCode_002EGetMessage(&resultCode)));
						goto IL_00c6;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<CompilerErrorListener*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EFireFX_002E_003FA0xce210b22_002ECompilerErrorListener_002E_007Bdtor_007D), &compilerErrorListener);
					throw;
				}
				goto end_IL_0047;
				IL_00c6:
				global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)));
				goto IL_00d8;
				end_IL_0047:;
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			goto end_IL_003f;
			IL_00d8:
			((IDisposable)iOStringWrapper).Dispose();
			goto IL_00ee;
			end_IL_003f:;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D), &shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
			throw;
		}
		Firaxis.Error.ResultCode success;
		try
		{
			try
			{
				try
				{
					effect = new FireFXEffectPtrWrapper(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
					success = Firaxis.Error.ResultCode.Success;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<CompilerErrorListener*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EFireFX_002E_003FA0xce210b22_002ECompilerErrorListener_002E_007Bdtor_007D), &compilerErrorListener);
					throw;
				}
				global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)));
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D), &shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
			throw;
		}
		global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
		return success;
		IL_00ee:
		global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
		return result;
	}

	public unsafe virtual Firaxis.Error.ResultCode CompileText(string scriptName, string scriptText, string byteCodePath, IList<CompileIssue> issues)
	{
		//IL_003e: Expected I, but got I8
		//IL_0084: Expected I8, but got I
		//IL_00b5: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		IOStringWrapper iOStringWrapper = null;
		if (!global::_003CModule_003E._003FA0xce210b22_002E_003FbIgnoreAlways_0040_003F2_003F_003FCompileText_0040FireFXCompilerService_0040FireFX_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVResultCode_0040Error_00405_0040PE_0024AAVString_0040System_0040_004000PE_0024AAU_003F_0024IList_0040PE_0024AAVCompileIssue_0040FireFX_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_00409_0040_0040Z_00404_NA)
		{
			shared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E* pFireFXCompiterPtr = m_pFireFXCompiterPtr;
			if ((pFireFXCompiterPtr == null || !global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002E_N(pFireFXCompiterPtr)) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CO_0040JOGPBJHL_0040m_pFireFXCompiterPtr_003F5_003F_0024CG_003F_0024CG_003F5_003F_0024CKm_pFire_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GK_0040CMJHNKEF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 136u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xce210b22_002E_003FbIgnoreAlways_0040_003F2_003F_003FCompileText_0040FireFXCompilerService_0040FireFX_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVResultCode_0040Error_00405_0040PE_0024AAVString_0040System_0040_004000PE_0024AAU_003F_0024IList_0040PE_0024AAVCompileIssue_0040FireFX_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_00409_0040_0040Z_00404_NA), (ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(scriptText);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out CompilerErrorListener compilerErrorListener);
		Firaxis.Error.ResultCode result;
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(scriptName);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				IOStringWrapper iOStringWrapper2 = new IOStringWrapper(byteCodePath);
				try
				{
					iOStringWrapper = iOStringWrapper2;
					*(long*)(&compilerErrorListener) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7CompilerErrorListener_0040_003FA0xce210b22_0040FireFX_0040CivTech_0040Firaxis_0040_00406B_0040);
					System.Runtime.CompilerServices.Unsafe.As<CompilerErrorListener, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)) = (nint)((IntPtr)GCHandle.Alloc(issues)).ToPointer();
					try
					{
						IFireFXCompiler* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002D_003E(m_pFireFXCompiterPtr);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode2);
						global::_003CModule_003E.Platform_002EResultCode_002E_007Bctor_007D(&resultCode, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, Platform.ResultCode*, sbyte*, sbyte*, char*, IErrorListener*, Platform.ResultCode*>)(*(ulong*)(*(long*)ptr + 24)))((nint)ptr, &resultCode2, standardStringWrapper2.Value, standardStringWrapper.Value, iOStringWrapper.Value, (IErrorListener*)(&compilerErrorListener)));
						if (!global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
						{
							result = new Firaxis.Error.ResultCode(new string(global::_003CModule_003E.Platform_002EResultCode_002EGetMessage(&resultCode)));
							goto IL_00e9;
						}
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<CompilerErrorListener*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EFireFX_002E_003FA0xce210b22_002ECompilerErrorListener_002E_007Bdtor_007D), &compilerErrorListener);
						throw;
					}
					goto end_IL_005f;
					IL_00e9:
					global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)));
					goto IL_00fb;
					end_IL_005f:;
				}
				catch
				{
					//try-fault
					((IDisposable)iOStringWrapper).Dispose();
					throw;
				}
				goto end_IL_0054;
				IL_00fb:
				((IDisposable)iOStringWrapper).Dispose();
				goto IL_010a;
				end_IL_0054:;
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			goto end_IL_0049;
			IL_010a:
			((IDisposable)standardStringWrapper2).Dispose();
			goto IL_0119;
			end_IL_0049:;
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		Firaxis.Error.ResultCode success;
		try
		{
			try
			{
				try
				{
					try
					{
						success = Firaxis.Error.ResultCode.Success;
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<CompilerErrorListener*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EFireFX_002E_003FA0xce210b22_002ECompilerErrorListener_002E_007Bdtor_007D), &compilerErrorListener);
						throw;
					}
					global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)));
				}
				catch
				{
					//try-fault
					((IDisposable)iOStringWrapper).Dispose();
					throw;
				}
				((IDisposable)iOStringWrapper).Dispose();
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return success;
		IL_0119:
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual Firaxis.Error.ResultCode CompileText(string scriptName, string scriptText, ref IFireFXEffect effect, IList<CompileIssue> issues)
	{
		//IL_0036: Expected I, but got I8
		//IL_0079: Expected I8, but got I
		//IL_00a6: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		if (!global::_003CModule_003E._003FA0xce210b22_002E_003FbIgnoreAlways_0040_003F2_003F_003FCompileText_0040FireFXCompilerService_0040FireFX_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVResultCode_0040Error_00405_0040PE_0024AAVString_0040System_0040_00400AE_0024CAPE_0024AAUIFireFXEffect_0040345_0040PE_0024AAU_003F_0024IList_0040PE_0024AAVCompileIssue_0040FireFX_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_00409_0040_0040Z_00404_NA)
		{
			shared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E* pFireFXCompiterPtr = m_pFireFXCompiterPtr;
			if ((pFireFXCompiterPtr == null || !global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002E_N(pFireFXCompiterPtr)) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CO_0040JOGPBJHL_0040m_pFireFXCompiterPtr_003F5_003F_0024CG_003F_0024CG_003F5_003F_0024CKm_pFire_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GK_0040CMJHNKEF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 116u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xce210b22_002E_003FbIgnoreAlways_0040_003F2_003F_003FCompileText_0040FireFXCompilerService_0040FireFX_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVResultCode_0040Error_00405_0040PE_0024AAVString_0040System_0040_00400AE_0024CAPE_0024AAUIFireFXEffect_0040345_0040PE_0024AAU_003F_0024IList_0040PE_0024AAVCompileIssue_0040FireFX_0040CivTech_0040Firaxis_0040_0040_0040Generic_0040Collections_00409_0040_0040Z_00404_NA), (ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
		global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bctor_007D(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out CompilerErrorListener compilerErrorListener);
		Firaxis.Error.ResultCode result;
		try
		{
			StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(scriptText);
			try
			{
				standardStringWrapper = standardStringWrapper3;
				StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(scriptName);
				try
				{
					standardStringWrapper2 = standardStringWrapper4;
					*(long*)(&compilerErrorListener) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7CompilerErrorListener_0040_003FA0xce210b22_0040FireFX_0040CivTech_0040Firaxis_0040_00406B_0040);
					System.Runtime.CompilerServices.Unsafe.As<CompilerErrorListener, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)) = (nint)((IntPtr)GCHandle.Alloc(issues)).ToPointer();
					try
					{
						IFireFXCompiler* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXCompiler_003E_002E_002D_003E(m_pFireFXCompiterPtr);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode2);
						global::_003CModule_003E.Platform_002EResultCode_002E_007Bctor_007D(&resultCode, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, Platform.ResultCode*, sbyte*, sbyte*, shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E*, IErrorListener*, Platform.ResultCode*>)(*(ulong*)(*(long*)ptr + 32)))((nint)ptr, &resultCode2, standardStringWrapper2.Value, standardStringWrapper.Value, &shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2, (IErrorListener*)(&compilerErrorListener)));
						if (!global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
						{
							result = new Firaxis.Error.ResultCode(new string(global::_003CModule_003E.Platform_002EResultCode_002EGetMessage(&resultCode)));
							goto IL_00da;
						}
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<CompilerErrorListener*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EFireFX_002E_003FA0xce210b22_002ECompilerErrorListener_002E_007Bdtor_007D), &compilerErrorListener);
						throw;
					}
					goto end_IL_0054;
					IL_00da:
					global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)));
					goto IL_00ec;
					end_IL_0054:;
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper2).Dispose();
					throw;
				}
				goto end_IL_0049;
				IL_00ec:
				((IDisposable)standardStringWrapper2).Dispose();
				goto IL_00fb;
				end_IL_0049:;
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			goto end_IL_0041;
			IL_00fb:
			((IDisposable)standardStringWrapper).Dispose();
			goto IL_0111;
			end_IL_0041:;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D), &shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
			throw;
		}
		Firaxis.Error.ResultCode success;
		try
		{
			try
			{
				try
				{
					try
					{
						effect = new FireFXEffectPtrWrapper(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
						success = Firaxis.Error.ResultCode.Success;
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<CompilerErrorListener*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EFireFX_002E_003FA0xce210b22_002ECompilerErrorListener_002E_007Bdtor_007D), &compilerErrorListener);
						throw;
					}
					global::_003CModule_003E.gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003AIList_003CFiraxis_003A_003ACivTech_003A_003AFireFX_003A_003ACompileIssue_0020_005E_003E_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)));
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper2).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper2).Dispose();
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper).Dispose();
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D), &shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
			throw;
		}
		global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
		return success;
		IL_0111:
		global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
		return result;
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EFireFXCompilerService();
		}
		else
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}
}
