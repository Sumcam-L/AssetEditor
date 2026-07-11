using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AppHost;
using Crypto;
using Platform;
using std;
using String;

namespace Firaxis.CivTech;

public class CivTechAssert : ICivTechAssert
{
	private static CivTechAssert s_this = null;

	public unsafe CivTechAssert(string logPath, AssertionConfiguration assCfg)
	{
		//IL_002d: Expected I, but got I8
		//IL_01ce: Expected I, but got I8
		//IL_01e9: Expected I, but got I8
		//IL_02ac: Expected I, but got I8
		//IL_0325: Expected I8, but got I
		//IL_0345: Expected I8, but got I
		//IL_032d: Expected I, but got I8
		//IL_034d: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x44d35d96_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CivTechAssert_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_0040W4AssertionConfiguration_004023_0040_0040Z_00404_NA && s_this != null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040NPCJFNJ_0040s_this_003F5_003F_0024DN_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040EOJMDPBH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 65u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x44d35d96_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CivTechAssert_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_0040W4AssertionConfiguration_004023_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		s_this = this;
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(Environment.GetFolderPath(Environment.SpecialFolder.Personal)).ToPointer();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E);
		global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E, ptr);
		try
		{
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			try
			{
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002ETranslateFrom_003C0_002Cclass_0020Platform_003A_003AStaticHeapAllocator_003C5_002C0_003E_0020_003E(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E);
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002EAppendLiteral_003C44_003E(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E, (_0024ArrayType_0024_0024_0024BY0CM_0040_0024_0024CB_S*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FI_0040BIAJDCHO_0040_003F2_003F_0024AAM_003F_0024AAy_003F_0024AA_003F5_003F_0024AAG_003F_0024AAa_003F_0024AAm_003F_0024AAe_003F_0024AAs_003F_0024AA_003F2_003F_0024AAS_003F_0024AAi_003F_0024AAd_003F_0024AA_003F5_003F_0024AAM_003F_0024AAe_003F_0024AA_0040));
				Assembly assembly = Assembly.GetEntryAssembly();
				if (assembly == null)
				{
					assembly = Assembly.GetExecutingAssembly();
				}
				AssemblyName name = assembly.GetName();
				System.Runtime.CompilerServices.Unsafe.SkipInit(out ModuleVersionInfo moduleVersionInfo);
				global::_003CModule_003E.AppHost_002EVersionNumber_002E_007Bctor_007D((VersionNumber*)(&moduleVersionInfo));
				*(short*)(&moduleVersionInfo) = (short)name.Version.Major;
				System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, short>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 2)) = (short)name.Version.Minor;
				System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, short>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 6)) = (short)name.Version.Build;
				System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, short>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 4)) = (short)name.Version.Revision;
				object[] customAttributes = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), inherit: false);
				if ((nint)customAttributes.LongLength > 0)
				{
					sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(((AssemblyInformationalVersionAttribute)customAttributes[0]).InformationalVersion).ToPointer();
					System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 8)) = global::_003CModule_003E.atoi(ptr2);
					IntPtr hglobal2 = new IntPtr(ptr2);
					Marshal.FreeHGlobal(hglobal2);
				}
				else
				{
					System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 8)) = 0;
				}
				sbyte* ptr3 = (sbyte*)Marshal.StringToHGlobalAnsi(name.Name).ToPointer();
				System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2);
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2, ptr3);
				try
				{
					IntPtr hglobal3 = new IntPtr(ptr3);
					Marshal.FreeHGlobal(hglobal3);
					global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_002B_003D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_04JLMDILM_0040_003F4exe_003F_0024AA_0040));
					sbyte* ptr4 = (sbyte*)Marshal.StringToHGlobalAnsi(logPath).ToPointer();
					System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E3);
					global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E3, ptr4);
					try
					{
						IntPtr hglobal4 = new IntPtr(ptr4);
						Marshal.FreeHGlobal(hglobal4);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E2);
						global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E2);
						try
						{
							global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002ETranslateFrom_003C0_002Cclass_0020Platform_003A_003AStaticHeapAllocator_003C5_002C0_003E_0020_003E(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E2, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E3);
							if (!global::_003CModule_003E._003FA0x44d35d96_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003F_003F0CivTechAssert_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_0040W4AssertionConfiguration_004023_0040_0040Z_00404_NA && !global::_003CModule_003E.std_002Eoperator_003D_003D_003Cclass_0020AppHost_003A_003ABugSubmissionPackager_003E((shared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.CivTechImpl_002E_INTERNAL_002E_003FA0x44d35d96_002Es_bugPackagerPtr), null) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DE_0040KOGDMKPL_0040CivTechImpl_003F3_003F3_INTERNAL_003F3_003F3s_bugPac_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040EOJMDPBH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 118u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x44d35d96_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003F_003F0CivTechAssert_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVString_0040System_0040_0040W4AssertionConfiguration_004023_0040_0040Z_00404_NA), (ErrorType)0))
							{
								/*OpCode not supported: DebugBreak*/;
							}
							System.Runtime.CompilerServices.Unsafe.SkipInit(out Hash64 hash);
							global::_003CModule_003E.Crypto_002EHash64_002E_007Bctor_007D(&hash, global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2)), false);
							ulong num;
							try
							{
								System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E4);
								global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E4);
								try
								{
									global::_003CModule_003E.AppHost_002EFillHostName(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E4);
									global::_003CModule_003E.Crypto_002EHash64_002EHashString(&hash, global::_003CModule_003E.String_002EBase_003C0_003E_002Ec_str((Base_003C0_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E4)));
									global::_003CModule_003E.Crypto_002EHash64_002EHashInt(&hash, System.Runtime.CompilerServices.Unsafe.As<ModuleVersionInfo, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref moduleVersionInfo, 8)));
									global::_003CModule_003E.Crypto_002EHash64_002EHashInt(&hash, (ulong)global::_003CModule_003E.Platform_002EGetTime(false, true));
									num = global::_003CModule_003E.Crypto_002EHash64_002EValue(&hash);
								}
								catch
								{
									//try-fault
									global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E4);
									throw;
								}
								global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E4);
							}
							catch
							{
								//try-fault
								global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Hash64*, void>)(&global::_003CModule_003E.Crypto_002EHash64_002E_007Bdtor_007D), &hash);
								throw;
							}
							global::_003CModule_003E.Crypto_002EHash64_002E_007Bdtor_007D(&hash);
							int num2 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
							BugSubmissionPackager* ptr5 = (BugSubmissionPackager*)global::_003CModule_003E.@new(144uL, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040EOJMDPBH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 134, 23, 0);
							BugSubmissionPackager* ptr6;
							try
							{
								ptr6 = ((ptr5 == null) ? null : global::_003CModule_003E.AppHost_002EBugSubmissionPackager_002E_007Bctor_007D(ptr5, num, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C0_003E2, &moduleVersionInfo));
							}
							catch
							{
								//try-fault
								global::_003CModule_003E.delete(ptr5, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040EOJMDPBH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 134, 23, 0);
								throw;
							}
							System.Runtime.CompilerServices.Unsafe.SkipInit(out shared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E shared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E2);
							shared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E* ptr7 = global::_003CModule_003E.std_002Eshared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E_002E_007Bctor_007D_003Cclass_0020AppHost_003A_003ABugSubmissionPackager_003E(&shared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E2, ptr6);
							try
							{
								global::_003CModule_003E.std_002Eshared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E_002E_003D((shared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.CivTechImpl_002E_INTERNAL_002E_003FA0x44d35d96_002Es_bugPackagerPtr), ptr7);
							}
							catch
							{
								//try-fault
								global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<shared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E*, void>)(&global::_003CModule_003E.std_002Eshared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E_002E_007Bdtor_007D), &shared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E2);
								throw;
							}
							global::_003CModule_003E.std_002Eshared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E_002E_007Bdtor_007D(&shared_ptr_003CAppHost_003A_003ABugSubmissionPackager_003E2);
							global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_003D((BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E.CivTechImpl_002E_INTERNAL_002E_003FA0x44d35d96_002Es_logPath), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E2);
							global::_003CModule_003E.Platform_002EBugCollectionEnable(true, (delegate* unmanaged[Cdecl, Cdecl]<sbyte*, char*, uint, sbyte*, sbyte*, ulong*, int, char*, char*, char**, int, byte, void>)global::_003CModule_003E.__unep_0040_003FCollectBugSubmission_0040_INTERNAL_0040CivTechImpl_0040_0040_0024_0024FYAXPEBDPEB_SI00PEB_KH11PEAPEB_SH_N_0040Z);
							global::_003CModule_003E.Platform_002EAssertEnable(true);
							global::_003CModule_003E.Platform_002EAssertCollectionEnable(true);
							switch (assCfg)
							{
							case AssertionConfiguration.eOutput:
							{
								uint num5 = 0u;
								long num6 = (nint)global::_003CModule_003E.__unep_0040_003FHandleTeamCityOutput_0040_INTERNAL_0040CivTechImpl_0040_0040_0024_0024FYA_003FAW4AssertResult_0040Platform_0040_0040PEBDI00_0040Z;
								do
								{
									global::_003CModule_003E.Platform_002ESetAssertProc((ErrorType)num5, (delegate* unmanaged[Cdecl, Cdecl]<sbyte*, uint, sbyte*, sbyte*, AssertResult>)num6);
									num5++;
								}
								while (num5 < 2);
								break;
							}
							case AssertionConfiguration.eOff:
							{
								uint num3 = 0u;
								long num4 = (nint)global::_003CModule_003E.__unep_0040_003FHandleLogOutput_0040_INTERNAL_0040CivTechImpl_0040_0040_0024_0024FYA_003FAW4AssertResult_0040Platform_0040_0040PEBDI00_0040Z;
								do
								{
									global::_003CModule_003E.Platform_002ESetAssertProc((ErrorType)num3, (delegate* unmanaged[Cdecl, Cdecl]<sbyte*, uint, sbyte*, sbyte*, AssertResult>)num4);
									num3++;
								}
								while (num3 < 2);
								break;
							}
							}
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

	private void _007ECivTechAssert()
	{
		s_this = null;
	}

	private void _0021CivTechAssert()
	{
		s_this = null;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			s_this = null;
			return;
		}
		try
		{
			s_this = null;
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

	~CivTechAssert()
	{
		Dispose(A_0: false);
	}
}
