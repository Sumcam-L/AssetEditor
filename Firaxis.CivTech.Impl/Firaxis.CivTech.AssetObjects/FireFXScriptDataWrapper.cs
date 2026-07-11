using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.CivTech.AssetObjects._003FA0xd9bf7c8e;
using Firaxis.CivTech.FireFX;
using Firaxis.Utility;
using FireFX;
using Platform;
using std;
using String;

namespace Firaxis.CivTech.AssetObjects;

public class FireFXScriptDataWrapper : IFireFXScriptData
{
	private string m_scriptName;

	private unsafe global::AssetObjects.IFireFXInstanceData* m_pkInstData;

	private unsafe global::AssetObjects.VirtualPantry* m_pkVirtualPantry;

	private IFireFXEffect m_pCompiledScript;

	public virtual IFireFXEffect CompiledScript
	{
		get
		{
			if (m_pCompiledScript == null)
			{
				DoCompile();
			}
			return m_pCompiledScript;
		}
	}

	public unsafe virtual string Script
	{
		get
		{
			//IL_0012: Expected I, but got I8
			global::AssetObjects.IFireFXInstanceData* pkInstData = m_pkInstData;
			return new string(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)pkInstData + 48)))((nint)pkInstData));
		}
		set
		{
			//IL_0025: Expected I, but got I8
			StandardStringWrapper standardStringWrapper = null;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::AssetObjects.IFireFXInstanceData* pkInstData = m_pkInstData;
				((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, void>)(*(ulong*)(*(long*)pkInstData + 40)))((nint)pkInstData, standardStringWrapper.Value);
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

	public unsafe FireFXScriptDataWrapper(string scriptName, global::AssetObjects.IFireFXInstanceData* pInstData, global::AssetObjects.VirtualPantry* pkVirtualPantry)
	{
		//IL_0043: Expected I, but got I8
		//IL_006e: Expected I, but got I8
		m_scriptName = scriptName;
		m_pkInstData = pInstData;
		m_pkVirtualPantry = pkVirtualPantry;
		base._002Ector();
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003F_003F0FireFXScriptDataWrapper_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_0040PEAVIFireFXInstanceData_00402_0040PEAVVirtualPantry_00402_0040_0040Z_00404_NA && m_pkInstData == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0N_0040NLAKBNGF_0040m_pkInstData_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040IPEDOJCM_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 74u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F2_003F_003F_003F0FireFXScriptDataWrapper_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_0040PEAVIFireFXInstanceData_00402_0040PEAVVirtualPantry_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F0FireFXScriptDataWrapper_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_0040PEAVIFireFXInstanceData_00402_0040PEAVVirtualPantry_00402_0040_0040Z_00404_NA && m_pkVirtualPantry == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040MEAOBNDH_0040m_pkVirtualPantry_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040IPEDOJCM_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 75u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FbIgnoreAlways_0040_003F9_003F_003F_003F0FireFXScriptDataWrapper_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_0040PEAVIFireFXInstanceData_00402_0040PEAVVirtualPantry_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		DoCompile();
	}

	private unsafe global::AssetObjects.IFireFXScriptData* GetScriptData()
	{
		return (global::AssetObjects.IFireFXScriptData*)m_pkInstData;
	}

	private unsafe void DoCompile()
	{
		//IL_0018: Expected I, but got I8
		//IL_007c: Expected I8, but got I
		//IL_00a6: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.IFireFXInstanceData* pkInstData = m_pkInstData;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E);
		global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*>)(*(ulong*)(*(long*)pkInstData + 48)))((nint)pkInstData));
		try
		{
			if (global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002EGetLength(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E) != 0)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(m_scriptName);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					System.Runtime.CompilerServices.Unsafe.SkipInit(out shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
					global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bctor_007D(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
					try
					{
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FireFXCompiler fireFXCompiler);
						global::_003CModule_003E.AssetObjects_002EFireFXCompiler_002E_007Bctor_007D(&fireFXCompiler, m_pkVirtualPantry);
						try
						{
							ICivTechLogger civTechLogger = Context.EnsureCreated<CivTechContext>().CivTechLogger;
							System.Runtime.CompilerServices.Unsafe.SkipInit(out CompilerErrorListener compilerErrorListener);
							*(long*)(&compilerErrorListener) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7CompilerErrorListener_0040_003FA0xd9bf7c8e_0040AssetObjects_0040CivTech_0040Firaxis_0040_00406B_0040);
							System.Runtime.CompilerServices.Unsafe.As<CompilerErrorListener, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)) = (nint)((IntPtr)GCHandle.Alloc(civTechLogger)).ToPointer();
							try
							{
								m_pCompiledScript = null;
								System.Runtime.CompilerServices.Unsafe.SkipInit(out ResultCode resultCode);
								if (global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ResultCode*, sbyte*, sbyte*, shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E*, IErrorListener*, ResultCode*>)(*(ulong*)(*(long*)(&fireFXCompiler) + 32)))((nint)(&fireFXCompiler), &resultCode, standardStringWrapper.Value, global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_005B_005D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E, 0), &shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2, (IErrorListener*)(&compilerErrorListener))))
								{
									m_pCompiledScript = new FireFXEffectPtrWrapper(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
								}
							}
							catch
							{
								//try-fault
								global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<CompilerErrorListener*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0xd9bf7c8e_002ECompilerErrorListener_002E_007Bdtor_007D), &compilerErrorListener);
								throw;
							}
							global::_003CModule_003E.gcroot_003CFiraxis_003A_003ACivTech_003A_003AICivTechLogger_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CFiraxis_003A_003ACivTech_003A_003AICivTechLogger_0020_005E_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref compilerErrorListener, 8)));
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FireFXCompiler*, void>)(&global::_003CModule_003E.AssetObjects_002EFireFXCompiler_002E_007Bdtor_007D), &fireFXCompiler);
							throw;
						}
						global::_003CModule_003E.AssetObjects_002EFireFXCompiler_002E_007Bdtor_007D(&fireFXCompiler);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D), &shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
						throw;
					}
					global::_003CModule_003E.std_002Eshared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E_002E_007Bdtor_007D(&shared_ptr_003CAssetObjects_003A_003AIFireFXScript_003E2);
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
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E);
			throw;
		}
		global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E);
	}
}
