using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AppHost;
using AssetPreviewer;
using Platform;
using Reflection;
using std;
using String;
using ToolHost;

namespace Firaxis.CivTech.AssetPreviewer;

public class ManagedNativeExceptionHandler : INativeExceptionHandler
{
	private IToolHostInterface m_toolHost;

	private unsafe shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E* m_nativeExceptionHandler;

	private bool m_collectionEnabled;

	public unsafe virtual ulong SessionHash
	{
		get
		{
			//IL_0017: Expected I, but got I8
			global::AssetPreviewer.INativeExceptionHandler* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002E_002D_003E(m_nativeExceptionHandler);
			return ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ulong>)(*(ulong*)(*(long*)intPtr + 16)))((nint)intPtr);
		}
		set
		{
			//IL_001a: Expected I, but got I8
			global::AssetPreviewer.INativeExceptionHandler* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002E_002D_003E(m_nativeExceptionHandler);
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ulong, void>)(*(ulong*)(*(long*)ptr + 24)))((nint)ptr, value);
		}
	}

	public unsafe ManagedNativeExceptionHandler(IToolHostInterface toolHost, string logPath, AssertionConfiguration assCfg)
	{
		//IL_000f: Expected I, but got I8
		//IL_00c1: Expected I, but got I8
		//IL_004b: Expected I, but got I8
		//IL_0131: Expected I, but got I8
		//IL_02e2: Expected I, but got I8
		//IL_02e4: Expected I8, but got I
		//IL_02f1: Expected I, but got I8
		m_toolHost = toolHost;
		m_nativeExceptionHandler = null;
		m_collectionEnabled = true;
		base._002Ector();
		ToolHostInterface toolHostInterface = (ToolHostInterface)m_toolHost;
		if (!global::_003CModule_003E._003FA0x68f142af_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ManagedNativeExceptionHandler_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAUIToolHostInterface_004034_0040PE_0024AAVString_0040System_0040_0040W4AssertionConfiguration_004034_0040_0040Z_00404_NA && toolHostInterface == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040JPOMFLOK_0040toolHostInterface_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040DHLMOOLE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 70u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x68f142af_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ManagedNativeExceptionHandler_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAUIToolHostInterface_004034_0040PE_0024AAVString_0040System_0040_0040W4AssertionConfiguration_004034_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!File.Exists(toolHostInterface.DllPath))
		{
			ulong num = 1uL;
			ulong num2 = 1uL;
			global::_003CModule_003E.Platform_002EGetLoggingOptions(&num, &num2);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D2);
			uint num3 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CF_0040GDINFLLF_0040Unable_003F5to_003F5find_003F5ToolHost_003F5DLL_003F5on_003F5d_0040), __arglist());
			global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CO_0040EHHIHFHK_0040N_003F_0024AAa_003F_0024AAt_003F_0024AAi_003F_0024AAv_003F_0024AAe_003F_0024AAE_003F_0024AAx_003F_0024AAc_003F_0024AAe_003F_0024AAp_003F_0024AAt_003F_0024AAi_003F_0024AAo_003F_0024AAn_003F_0024AAH_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D2), num3);
			throw new Exception($"ToolHost .DLL located at path:\n\t{toolHostInterface.DllPath}\n\n does not exist on disk.  Consider reinstalling the tools to correct this issue.");
		}
		IToolHostModuleMgrClient* toolHostModuleMgr = toolHostInterface.GetToolHostModuleMgr();
		NativeExceptionHandlerInterface* ptr = (NativeExceptionHandlerInterface*)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::Reflection.TypeInfo*, int, void*>)(*(ulong*)(*(long*)toolHostModuleMgr + 16)))((nint)toolHostModuleMgr, global::_003CModule_003E.ToolHost_002ENativeExceptionHandlerInterface_002EGetTypeInfo(), 5);
		if (ptr == null)
		{
			ulong num4 = 1uL;
			ulong num5 = 1uL;
			global::_003CModule_003E.Platform_002EGetLoggingOptions(&num4, &num5);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BBH_0040D _0024ArrayType_0024_0024_0024BY0BBH_0040D3);
			uint num6 = (uint)global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D3), 279uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EI_0040MAKENID_0040Unable_003F5to_003F5find_003F5appropriate_003F5excep_0040), __arglist());
			global::_003CModule_003E.Platform_002ELogEvent((char*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CO_0040EHHIHFHK_0040N_003F_0024AAa_003F_0024AAt_003F_0024AAi_003F_0024AAv_003F_0024AAe_003F_0024AAE_003F_0024AAx_003F_0024AAc_003F_0024AAe_003F_0024AAp_003F_0024AAt_003F_0024AAi_003F_0024AAo_003F_0024AAn_003F_0024AAH_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0BBH_0040D3), num6);
			throw new Exception($"ToolHost .DLL located at path:\n\t{toolHostInterface.DllPath}\n\n does not contain version {5} of the native exception handler.  Do you have use local toolhost specified?");
		}
		shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E* ptr2 = (shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E*)global::_003CModule_003E.@new(16uL);
		m_nativeExceptionHandler = ((ptr2 == null) ? null : global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002E_007Bctor_007D(ptr2));
		sbyte* ptr3 = (sbyte*)Marshal.StringToHGlobalAnsi(Environment.GetFolderPath(Environment.SpecialFolder.Personal)).ToPointer();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E);
		global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E, ptr3);
		try
		{
			IntPtr hglobal = new IntPtr(ptr3);
			Marshal.FreeHGlobal(hglobal);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			try
			{
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002ETranslateFrom_003C0_002Cclass_0020Platform_003A_003AStaticHeapAllocator_003C5_002C0_003E_0020_003E(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E);
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002EAppendLiteral_003C44_003E(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E, (_0024ArrayType_0024_0024_0024BY0CM_0040_0024_0024CB_S*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FI_0040BIAJDCHO_0040_003F2_003F_0024AAM_003F_0024AAy_003F_0024AA_003F5_003F_0024AAG_003F_0024AAa_003F_0024AAm_003F_0024AAe_003F_0024AAs_003F_0024AA_003F2_003F_0024AAS_003F_0024AAi_003F_0024AAd_003F_0024AA_003F5_003F_0024AAM_003F_0024AAe_003F_0024AA_0040));
				Assembly entryAssembly = Assembly.GetEntryAssembly();
				AssemblyName name = entryAssembly.GetName();
				System.Runtime.CompilerServices.Unsafe.SkipInit(out ModuleVersionInfo moduleVersionInfo);
				global::_003CModule_003E.AppHost_002EVersionNumber_002E_007Bctor_007D((VersionNumber*)(&moduleVersionInfo));
				*(short*)(&moduleVersionInfo) = (short)name.Version.Major;
				System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, short>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 2)) = (short)name.Version.Minor;
				System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, short>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 6)) = (short)name.Version.Build;
				System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, short>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 4)) = (short)name.Version.Revision;
				object[] customAttributes = entryAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), inherit: false);
				if ((nint)customAttributes.LongLength > 0)
				{
					sbyte* ptr4 = (sbyte*)Marshal.StringToHGlobalAnsi(((AssemblyInformationalVersionAttribute)customAttributes[0]).InformationalVersion).ToPointer();
					System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 8)) = global::_003CModule_003E.atoi(ptr4);
					IntPtr hglobal2 = new IntPtr(ptr4);
					Marshal.FreeHGlobal(hglobal2);
				}
				else
				{
					System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 8)) = 0;
				}
				sbyte* ptr5 = (sbyte*)Marshal.StringToHGlobalAnsi(name.Name).ToPointer();
				System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2);
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2, ptr5);
				try
				{
					IntPtr hglobal3 = new IntPtr(ptr5);
					Marshal.FreeHGlobal(hglobal3);
					global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_002B_003D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_04JLMDILM_0040_003F4exe_003F_0024AA_0040));
					sbyte* ptr6 = (sbyte*)Marshal.StringToHGlobalAnsi(logPath).ToPointer();
					System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E3);
					global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E3, ptr6);
					try
					{
						IntPtr hglobal4 = new IntPtr(ptr6);
						Marshal.FreeHGlobal(hglobal4);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E2);
						global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E2);
						try
						{
							global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002ETranslateFrom_003C0_002Cclass_0020Platform_003A_003AStaticHeapAllocator_003C5_002C0_003E_0020_003E(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E2, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E3);
							NativeExceptionHandlerInterface.AssertionConfiguration assertionConfiguration = ((assCfg != AssertionConfiguration.eOff) ? ((assCfg == AssertionConfiguration.eDialog) ? ((NativeExceptionHandlerInterface.AssertionConfiguration)1) : ((assCfg != AssertionConfiguration.eOutput) ? ((NativeExceptionHandlerInterface.AssertionConfiguration)1) : ((NativeExceptionHandlerInterface.AssertionConfiguration)2))) : ((NativeExceptionHandlerInterface.AssertionConfiguration)0));
							System.Runtime.CompilerServices.Unsafe.SkipInit(out shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E2);
							long num7 = (nint)((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E*, BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E*, ModuleVersionInfo*, NativeExceptionHandlerInterface.AssertionConfiguration, shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E*>)(*(ulong*)(*(long*)ptr + 32)))((nint)ptr, &shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E2, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E2, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2, &moduleVersionInfo, assertionConfiguration);
							try
							{
								global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002E_003D(m_nativeExceptionHandler, (shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E*)num7);
							}
							catch
							{
								//try-fault
								global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002E_007Bdtor_007D), &shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E2);
								throw;
							}
							global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002E_007Bdtor_007D(&shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E2);
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E2);
							throw;
						}
						global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E2);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E3);
						throw;
					}
					global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E3);
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2);
					throw;
				}
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				throw;
			}
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E);
			throw;
		}
		global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E);
	}

	public ManagedNativeExceptionHandler(IToolHostInterface toolHost, string logPath)
		: this(toolHost, logPath, AssertionConfiguration.eOff)
	{
	}

	private void _007EManagedNativeExceptionHandler()
	{
		_0021ManagedNativeExceptionHandler();
	}

	private unsafe void _0021ManagedNativeExceptionHandler()
	{
		shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E* nativeExceptionHandler = m_nativeExceptionHandler;
		if (nativeExceptionHandler != null)
		{
			global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002Ereset(nativeExceptionHandler);
			shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E* nativeExceptionHandler2 = m_nativeExceptionHandler;
			if (nativeExceptionHandler2 != null)
			{
				global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002E_007Bdtor_007D(nativeExceptionHandler2);
				global::_003CModule_003E.delete(nativeExceptionHandler2, 16uL);
			}
		}
	}

	public unsafe virtual void EnableCollection([MarshalAs(UnmanagedType.U1)] bool bEnabled)
	{
		//IL_0028: Expected I, but got I8
		shared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E* nativeExceptionHandler = m_nativeExceptionHandler;
		if (nativeExceptionHandler != null && global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002E_002E_N(nativeExceptionHandler))
		{
			global::AssetPreviewer.INativeExceptionHandler* ptr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002E_002D_003E(m_nativeExceptionHandler);
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, byte, void>)(*(ulong*)(*(ulong*)ptr)))((nint)ptr, bEnabled ? ((byte)1) : ((byte)0));
		}
	}

	public unsafe virtual void CrashCLI()
	{
		//IL_001a: Expected I, but got I8
		IToolHostModuleMgrClient* toolHostModuleMgr = ((ToolHostInterface)null).GetToolHostModuleMgr();
		((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::Reflection.TypeInfo*, int, void*>)(*(ulong*)(*(long*)toolHostModuleMgr + 16)))((nint)toolHostModuleMgr, global::_003CModule_003E.ToolHost_002ENativeExceptionHandlerInterface_002EGetTypeInfo(), 5);
	}

	public unsafe virtual void CrashNative()
	{
		//IL_0016: Expected I, but got I8
		global::AssetPreviewer.INativeExceptionHandler* intPtr = global::_003CModule_003E.std_002Eshared_ptr_003CAssetPreviewer_003A_003AINativeExceptionHandler_003E_002E_002D_003E(m_nativeExceptionHandler);
		((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)intPtr + 8)))((nint)intPtr);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021ManagedNativeExceptionHandler();
			return;
		}
		try
		{
			_0021ManagedNativeExceptionHandler();
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

	~ManagedNativeExceptionHandler()
	{
		Dispose(A_0: false);
	}
}
