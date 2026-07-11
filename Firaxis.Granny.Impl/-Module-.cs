using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using _003CCppImplementationDetails_003E;
using _003CCrtImplementationDetails_003E;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Granny;
using granny;
using Math.Mesh;
using msclr.interop.details;
using Platform;
using std;
using Types;

internal class _003CModule_003E
{
	internal static _0024ArrayType_0024_0024_0024BY00_0024_0024CBD _003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040/* Not supported: data(00) */;

	internal static _0024ArrayType_0024_0024_0024BY0FH_0040_0024_0024CBD _003F_003F_C_0040_0FH_0040IKCIJNIH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040/* Not supported: data(43 3A 5C 42 75 69 6C 64 41 67 65 6E 74 5C 77 6F 72 6B 5C 61 63 66 33 34 32 33 66 62 32 65 35 39 65 37 5C 43 69 76 54 65 63 68 5C 54 6F 6F 6C 4C 69 62 73 5C 46 69 72 61 78 69 73 2E 47 72 61 6E 6E 79 2E 49 6D 70 6C 5C 47 72 61 6E 6E 79 46 69 6C 65 2E 63 70 70 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0BP_0040_0024_0024CBD _003F_003F_C_0040_0BP_0040HCKGLFHN_0040GrannyCurveIsKeyframed_003F_0024CI_003F_0024CGCurve_003F_0024CJ_003F_0024AA_0040/* Not supported: data(47 72 61 6E 6E 79 43 75 72 76 65 49 73 4B 65 79 66 72 61 6D 65 64 28 26 43 75 72 76 65 29 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0BB_0040_0024_0024CBD _003F_003F_C_0040_0BB_0040GKOPDPME_0040GrannyTrackMasks_003F_0024AA_0040/* Not supported: data(47 72 61 6E 6E 79 54 72 61 63 6B 4D 61 73 6B 73 00) */;

	internal static _0024ArrayType_0024_0024_0024BY09_0024_0024CBD _003F_003F_C_0040_09MNIJHJJO_0040ShaderSet_003F_0024AA_0040/* Not supported: data(53 68 61 64 65 72 53 65 74 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0M_0040_0024_0024CBD _003F_003F_C_0040_0M_0040JHOBELMO_0040FallbackMtl_003F_0024AA_0040/* Not supported: data(46 61 6C 6C 62 61 63 6B 4D 74 6C 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0P_0040_0024_0024CBD _003F_003F_C_0040_0P_0040DCLCLCM_0040UseFallbackMtl_003F_0024AA_0040/* Not supported: data(55 73 65 46 61 6C 6C 62 61 63 6B 4D 74 6C 00) */;

	internal static _0024ArrayType_0024_0024_0024BY09_0024_0024CBD _003F_003F_C_0040_09JEIJEBPJ_0040AlphaMode_003F_0024AA_0040/* Not supported: data(41 6C 70 68 61 4D 6F 64 65 00) */;

	internal static _0024ArrayType_0024_0024_0024BY05_0024_0024CBD _003F_003F_C_0040_05HHFKPCHO_0040ZMode_003F_0024AA_0040/* Not supported: data(5A 4D 6F 64 65 00) */;

	internal static _0024ArrayType_0024_0024_0024BY08_0024_0024CBD _003F_003F_C_0040_08OHHJEIMD_0040AlphaRef_003F_0024AA_0040/* Not supported: data(41 6C 70 68 61 52 65 66 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0O_0040_0024_0024CBD _003F_003F_C_0040_0O_0040OJGCDMCP_0040SkinBoneCount_003F_0024AA_0040/* Not supported: data(53 6B 69 6E 42 6F 6E 65 43 6F 75 6E 74 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0BD_0040_0024_0024CBD _003F_003F_C_0040_0BD_0040KPGOALGA_0040FireGrafixMaterial_003F_0024AA_0040/* Not supported: data(46 69 72 65 47 72 61 66 69 78 4D 61 74 65 72 69 61 6C 00) */;

	internal static _0024ArrayType_0024_0024_0024BY08_0024_0024CBD _003F_003F_C_0040_08IPCLAMCO_0040typeName_003F_0024AA_0040/* Not supported: data(74 79 70 65 4E 61 6D 65 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0BC_0040_0024_0024CBD _003F_003F_C_0040_0BC_0040ILKEHOOB_0040ParameterSetCount_003F_0024AA_0040/* Not supported: data(50 61 72 61 6D 65 74 65 72 53 65 74 43 6F 75 6E 74 00) */;

	internal static _0024ArrayType_0024_0024_0024BY03_0024_0024CBD _003F_003F_C_0040_03FCCPIJGJ_0040Pos_003F_0024AA_0040/* Not supported: data(50 6F 73 00) */;

	internal static _0024ArrayType_0024_0024_0024BY03_0024_0024CBD _003F_003F_C_0040_03POIGAEFI_0040Ori_003F_0024AA_0040/* Not supported: data(4F 72 69 00) */;

	internal static _0024ArrayType_0024_0024_0024BY03_0024_0024CBD _003F_003F_C_0040_03NMAEPLEB_0040ScS_003F_0024AA_0040/* Not supported: data(53 63 53 00) */;

	internal static _0024ArrayType_0024_0024_0024BY06_0024_0024CBD _003F_003F_C_0040_06OINDDGEP_0040Prefix_003F_0024AA_0040/* Not supported: data(50 72 65 66 69 78 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0BD_0040_0024_0024CBD _003F_003F_C_0040_0BD_0040OLBABOEK_0040vector_003F_0024DMT_003F_0024DO_003F5too_003F5long_003F_0024AA_0040/* Not supported: data(76 65 63 74 6F 72 3C 54 3E 20 74 6F 6F 20 6C 6F 6E 67 00) */;

	internal static bool _003FA0x2cc7db78_002E_003FbIgnoreAlways_0040_003F2_003F_003FRecompressCurve_0040GrannyFile_0040Granny_0040Firaxis_0040_0040AE_0024AAM_NPEBDAEAUgranny_curve2_0040_0040M_NHAEBUgranny_compress_curve_parameters_0040_0040_0040Z_00404_NA/* Not supported: data(00) */;

	internal static _0024ArrayType_0024_0024_0024BY0BA_0040_0024_0024CBD _003F_003F_C_0040_0BA_0040JFNIOLAK_0040string_003F5too_003F5long_003F_0024AA_0040/* Not supported: data(73 74 72 69 6E 67 20 74 6F 6F 20 6C 6F 6E 67 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0BI_0040_0024_0024CBD _003F_003F_C_0040_0BI_0040CFPLBAOH_0040invalid_003F5string_003F5position_003F_0024AA_0040/* Not supported: data(69 6E 76 61 6C 69 64 20 73 74 72 69 6E 67 20 70 6F 73 69 74 69 6F 6E 00) */;

	internal static _0024_TypeDescriptor_0024_extraBytes_23 _003F_003F_R0_003FAVexception_0040stdext_0040_0040_00408/* Not supported: data(D0 0C 05 80 01 00 00 00 00 00 00 00 00 00 00 00 2E 3F 41 56 65 78 63 65 70 74 69 6F 6E 40 73 74 64 65 78 74 40 40 00) */;

	public unsafe static delegate*<void*, uint, sbyte*> __m2mep_0040_003FGrannyFileLoaderRebaseToStringHash_0040_003FA0x7673ad18_0040Granny_0040Firaxis_0040_0040_0024_0024FYAPEADPEAXI_0040Z/* Not supported: data(04 00 00 06 00 00 00 00) */;

	public unsafe static delegate*<_String_alloc_003Cstd_003A_003A_String_base_types_003Cchar_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_0020_003E*, void> __m2mep_0040_003F_003F1_003F_0024_String_alloc_0040U_003F_0024_String_base_types_0040DV_003F_0024allocator_0040D_0040std_0040_0040_0040std_0040_0040_0040std_0040_0040_0024_0024FQEAA_0040XZ/* Not supported: data(0B 00 00 06 00 00 00 00) */;

	public unsafe static delegate*<_Compressed_pair_003Cstd_003A_003A_Wrap_alloc_003Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002Cstd_003A_003A_String_val_003Cstd_003A_003A_Simple_types_003Cchar_003E_0020_003E_002C1_003E*, void> __m2mep_0040_003F_003F1_003F_0024_Compressed_pair_0040U_003F_0024_Wrap_alloc_0040V_003F_0024allocator_0040D_0040std_0040_0040_0040std_0040_0040V_003F_0024_String_val_0040U_003F_0024_Simple_types_0040D_0040std_0040_0040_00402_0040_002400_0040std_0040_0040_0024_0024FQEAA_0040XZ/* Not supported: data(0C 00 00 06 00 00 00 00) */;

	public unsafe static delegate*<_String_val_003Cstd_003A_003A_Simple_types_003Cchar_003E_0020_003E*, void> __m2mep_0040_003F_003F1_003F_0024_String_val_0040U_003F_0024_Simple_types_0040D_0040std_0040_0040_0040std_0040_0040_0024_0024FQEAA_0040XZ/* Not supported: data(0D 00 00 06 00 00 00 00) */;

	public unsafe static int** __unep_0040_003FGrannyFileLoaderRebaseToStringHash_0040_003FA0x7673ad18_0040Granny_0040Firaxis_0040_0040_0024_0024FYAPEADPEAXI_0040Z/* Not supported: data(10 90 04 80 01 00 00 00) */;

	internal static _0024ArrayType_0024_0024_0024BY04_0024_0024CBD _003F_003F_C_0040_04HNBCGJMD_0040Tex0_003F_0024AA_0040/* Not supported: data(54 65 78 30 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0EF_0040_0024_0024CBD _003F_003F_C_0040_0EF_0040MEKODELL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040/* Not supported: data(43 3A 5C 42 75 69 6C 64 41 67 65 6E 74 5C 77 6F 72 6B 5C 61 63 66 33 34 32 33 66 62 32 65 35 39 65 37 5C 43 69 76 54 65 63 68 5C 4C 69 62 73 5C 54 79 70 65 73 5C 54 79 70 65 73 5F 56 65 63 74 6F 72 2E 68 00) */;

	internal static _0024ArrayType_0024_0024_0024BY0BI_0040_0024_0024CBD _003F_003F_C_0040_0BI_0040BJCDIEEI_0040i_003F5_003F_0024DM_003F5_003F_0024CIsize_t_003F_0024CJm_nCurrSize_003F_0024AA_0040/* Not supported: data(69 20 3C 20 28 73 69 7A 65 5F 74 29 6D 5F 6E 43 75 72 72 53 69 7A 65 00) */;

	internal static bool _003FbIgnoreAlways_0040_003F2_003F_003F_003FA_003F_0024Vector_0040ULodTarget_0040Mesh_0040Math_0040_0040U_003F_0024Default_0040_002400_00240A_0040_0040LinearAlloc_0040Types_0040_0040_0040Types_0040_0040QEAAAEAULodTarget_0040Mesh_0040Math_0040_0040_K_0040Z_00404_NA/* Not supported: data(00) */;

	internal static int ___0040_0040_PchSym__004000_0040UyfrowztvmgUdlipUzxuDECDuyCvFJvHUnlwzhhvgvwrgliUivovzhvUxregvxsUgllooryhUurizcrhOtizmmbOrnkoUurizcrhOtizmmbOrnkoOwriUivovzhvUhgwzucOlyq_0040595B3306334E7AC8/* Not supported: data(00 00 00 00) */;

	internal static global::__s_GUID _GUID_cb2f6723_ab3a_11d2_9c40_00c04fa30a3e/* Not supported: data(23 67 2F CB 3A AB D2 11 9C 40 00 C0 4F A3 0A 3E) */;

	internal static global::__s_GUID _GUID_cb2f6722_ab3a_11d2_9c40_00c04fa30a3e/* Not supported: data(22 67 2F CB 3A AB D2 11 9C 40 00 C0 4F A3 0A 3E) */;

	[FixedAddressValueType]
	internal static int _003FUninitialized_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2HA;

	internal unsafe static delegate*<void> _003FA0xf62e0d43_002E_003FUninitialized_0024initializer_0024_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2P6MXXZEA/* Not supported: data(22 00 00 06 00 00 00 00) */;

	[FixedAddressValueType]
	internal static _003CCrtImplementationDetails_003E.Progress _003FInitializedNative_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2W4Progress_00402_0040A;

	internal unsafe static delegate*<void> _003FA0xf62e0d43_002E_003FInitializedNative_0024initializer_0024_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2P6MXXZEA/* Not supported: data(25 00 00 06 00 00 00 00) */;

	internal static global::__s_GUID _GUID_90f1a06c_7712_4762_86b5_7a5eba6bdb02/* Not supported: data(6C A0 F1 90 12 77 62 47 86 B5 7A 5E BA 6B DB 02) */;

	internal static global::__s_GUID _GUID_90f1a06e_7712_4762_86b5_7a5eba6bdb02/* Not supported: data(6E A0 F1 90 12 77 62 47 86 B5 7A 5E BA 6B DB 02) */;

	[FixedAddressValueType]
	internal static _003CCrtImplementationDetails_003E.Progress _003FInitializedPerAppDomain_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2W4Progress_00402_0040A;

	internal static bool _003FEntered_0040DefaultDomain_0040_003CCrtImplementationDetails_003E_0040_00402_NA/*Field data (rva=0x90e04) could not be found in any section!*/;

	internal static _003CCrtImplementationDetails_003E.TriBool _003FhasNative_0040DefaultDomain_0040_003CCrtImplementationDetails_003E_0040_00400W4TriBool_00402_0040A/* Not supported: data() */;

	internal static bool _003FInitializedPerProcess_0040DefaultDomain_0040_003CCrtImplementationDetails_003E_0040_00402_NA/*Field data (rva=0x90e07) could not be found in any section!*/;

	internal static int _003FCount_0040AllDomains_0040_003CCrtImplementationDetails_003E_0040_00402HA/*Field data (rva=0x90e00) could not be found in any section!*/;

	[FixedAddressValueType]
	internal static int _003FInitialized_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2HA;

	internal static bool _003FInitializedNativeFromCCTOR_0040DefaultDomain_0040_003CCrtImplementationDetails_003E_0040_00402_NA/*Field data (rva=0x90e06) could not be found in any section!*/;

	[FixedAddressValueType]
	internal static bool _003FIsDefaultDomain_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2_NA;

	[FixedAddressValueType]
	internal static _003CCrtImplementationDetails_003E.Progress _003FInitializedVtables_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2W4Progress_00402_0040A;

	internal static bool _003FInitializedNative_0040DefaultDomain_0040_003CCrtImplementationDetails_003E_0040_00402_NA/*Field data (rva=0x90e05) could not be found in any section!*/;

	[FixedAddressValueType]
	internal static _003CCrtImplementationDetails_003E.Progress _003FInitializedPerProcess_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2W4Progress_00402_0040A;

	internal static _003CCrtImplementationDetails_003E.TriBool _003FhasPerProcess_0040DefaultDomain_0040_003CCrtImplementationDetails_003E_0040_00400W4TriBool_00402_0040A/* Not supported: data() */;

	internal static _0024ArrayType_0024_0024_0024BY00Q6MPEBXXZ __xc_mp_z/* Not supported: data(00 00 00 00 00 00 00 00) */;

	internal static _0024ArrayType_0024_0024_0024BY00Q6MPEBXXZ __xi_vt_z/* Not supported: data(00 00 00 00 00 00 00 00) */;

	internal unsafe static delegate*<void> _003FA0xf62e0d43_002E_003FInitializedPerProcess_0024initializer_0024_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2P6MXXZEA/* Not supported: data(26 00 00 06 00 00 00 00) */;

	internal static _0024ArrayType_0024_0024_0024BY00Q6MPEBXXZ __xc_ma_a/* Not supported: data(00 00 00 00 00 00 00 00) */;

	internal static _0024ArrayType_0024_0024_0024BY00Q6MPEBXXZ __xc_ma_z/* Not supported: data(00 00 00 00 00 00 00 00) */;

	internal unsafe static delegate*<void> _003FA0xf62e0d43_002E_003FInitializedPerAppDomain_0024initializer_0024_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2P6MXXZEA/* Not supported: data(27 00 00 06 00 00 00 00) */;

	internal static _0024ArrayType_0024_0024_0024BY00Q6MPEBXXZ __xi_vt_a/* Not supported: data(00 00 00 00 00 00 00 00) */;

	internal unsafe static delegate*<void> _003FA0xf62e0d43_002E_003FInitialized_0024initializer_0024_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2P6MXXZEA/* Not supported: data(21 00 00 06 00 00 00 00) */;

	internal static _0024ArrayType_0024_0024_0024BY00Q6MPEBXXZ __xc_mp_a/* Not supported: data(00 00 00 00 00 00 00 00) */;

	internal unsafe static delegate*<void> _003FA0xf62e0d43_002E_003FInitializedVtables_0024initializer_0024_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2P6MXXZEA/* Not supported: data(24 00 00 06 00 00 00 00) */;

	internal unsafe static delegate*<void> _003FA0xf62e0d43_002E_003FIsDefaultDomain_0024initializer_0024_0040CurrentDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q2P6MXXZEA/* Not supported: data(23 00 00 06 00 00 00 00) */;

	public unsafe static delegate*<void*, int> __m2mep_0040_003FDoNothing_0040DefaultDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024FCAJPEAX_0040Z/* Not supported: data(1B 00 00 06 00 00 00 00) */;

	public unsafe static delegate*<void*, int> __m2mep_0040_003F_UninitializeDefaultDomain_0040LanguageSupport_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024FCAJPEAX_0040Z/* Not supported: data(30 00 00 06 00 00 00 00) */;

	public unsafe static int** __unep_0040_003FDoNothing_0040DefaultDomain_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024FCAJPEAX_0040Z/* Not supported: data(40 90 04 80 01 00 00 00) */;

	public unsafe static int** __unep_0040_003F_UninitializeDefaultDomain_0040LanguageSupport_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024FCAJPEAX_0040Z/* Not supported: data(50 90 04 80 01 00 00 00) */;

	internal unsafe static delegate*<void>* _003FA0xc2538164_002E__onexitbegin_m/*Field data (rva=0x90f40) could not be found in any section!*/;

	internal static ulong _003FA0xc2538164_002E__exit_list_size/*Field data (rva=0x90f50) could not be found in any section!*/;

	[FixedAddressValueType]
	internal unsafe static delegate*<void>* __onexitend_app_domain;

	[FixedAddressValueType]
	internal unsafe static void* _003F_lock_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0PEAXEA;

	[FixedAddressValueType]
	internal static int _003F_ref_count_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0HA;

	internal unsafe static delegate*<void>* _003FA0xc2538164_002E__onexitend_m/*Field data (rva=0x90f48) could not be found in any section!*/;

	[FixedAddressValueType]
	internal static ulong __exit_list_size_app_domain;

	[FixedAddressValueType]
	internal unsafe static delegate*<void>* __onexitbegin_app_domain;

	internal unsafe static granny_data_type_definition* GrannyModelType/* Not supported: data(60 BF 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyTrackGroupType/* Not supported: data(10 BA 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyAnimationType/* Not supported: data(70 C0 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyTransformType/* Not supported: data(E0 B5 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyBoneType/* Not supported: data(10 9B 06 80 01 00 00 00) */;

	internal unsafe static float* GrannyCurveIdentityScaleShear/* Not supported: data(08 7D 06 80 01 00 00 00) */;

	internal unsafe static float* GrannyCurveIdentityOrientation/* Not supported: data(58 8B 06 80 01 00 00 00) */;

	internal unsafe static float* GrannyCurveIdentityPosition/* Not supported: data(88 D5 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurve2Type/* Not supported: data(30 8C 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyModFileInfoType/* Not supported: data(C0 A6 06 80 01 00 00 00) */;

	internal unsafe static granny_grn_file_magic_value* GrannyGRNFileMV_ThisPlatform/* Not supported: data(E0 BC 04 80 01 00 00 00) */;

	internal unsafe static granny_grn_file_magic_value* GrannyGRNFileMV_32Bit_LittleEndian/* Not supported: data(60 BC 04 80 01 00 00 00) */;

	internal unsafe static float* GrannyCurveIdentityShear/* Not supported: data(98 D5 06 80 01 00 00 00) */;

	internal unsafe static float* GrannyCurveIdentityScale/* Not supported: data(68 8B 06 80 01 00 00 00) */;

	internal unsafe static float* GrannyCurveIdentityNormal/* Not supported: data(48 8B 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD3I1K8uC8uType/* Not supported: data(80 82 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD3I1K16uC16uType/* Not supported: data(70 81 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD3I1K32fC32fType/* Not supported: data(60 80 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD9I3K8uC8uType/* Not supported: data(50 7F 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD9I1K8uC8uType/* Not supported: data(40 7E 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD9I3K16uC16uType/* Not supported: data(30 7D 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD9I1K16uC16uType/* Not supported: data(00 7C 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD4Constant32fType/* Not supported: data(50 7B 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD3Constant32fType/* Not supported: data(A0 7A 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataDaConstant32fType/* Not supported: data(E0 94 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataDaIdentityType/* Not supported: data(50 94 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD4nK8uC7uType/* Not supported: data(70 93 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD4nK16uC15uType/* Not supported: data(90 92 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD3K8uC8uType/* Not supported: data(80 91 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataD3K16uC16uType/* Not supported: data(70 90 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataDaK8uC8uType/* Not supported: data(90 8F 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataDaK16uC16uType/* Not supported: data(B0 8E 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataDaK32fC32fType/* Not supported: data(D0 8D 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyCurveDataDaKeyframes32fType/* Not supported: data(20 8D 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannySkeletonType/* Not supported: data(50 9C 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyMaterialType/* Not supported: data(D0 AA 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyMeshType/* Not supported: data(20 9F 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyFileInfoType/* Not supported: data(90 95 06 80 01 00 00 00) */;

	internal static _0024ArrayType_0024_0024_0024BY01Q6AXXZ _003F_003F_7type_info_0040_00406B_0040/* Not supported: data(E8 54 04 80 01 00 00 00 00 00 00 00 00 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyMaterialMapType/* Not supported: data(40 AA 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyTextureType/* Not supported: data(F0 A1 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyP3VertexType/* Not supported: data(70 69 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyMaterialBindingType/* Not supported: data(10 9E 06 80 01 00 00 00) */;

	internal unsafe static granny_data_type_definition* GrannyTransformTrackType/* Not supported: data(E0 B7 06 80 01 00 00 00) */;

	internal static _003CCppImplementationDetails_003E._0024ArrayType_0024_0024_0024BY0A_0040P6AHXZ __xi_z/* Not supported: data(00) */;

	internal static global::__scrt_native_startup_state __scrt_current_native_startup_state/* Not supported: data() */;

	internal unsafe static void* __scrt_native_startup_lock/*Field data (rva=0x90d90) could not be found in any section!*/;

	internal static _003CCppImplementationDetails_003E._0024ArrayType_0024_0024_0024BY0A_0040P6AXXZ __xc_a/* Not supported: data(00) */;

	internal static _003CCppImplementationDetails_003E._0024ArrayType_0024_0024_0024BY0A_0040P6AHXZ __xi_a/* Not supported: data(00) */;

	internal static uint __scrt_native_dllmain_reason/* Not supported: data(FF FF FF FF) */;

	internal static _003CCppImplementationDetails_003E._0024ArrayType_0024_0024_0024BY0A_0040P6AXXZ __xc_z/* Not supported: data(00) */;

	internal unsafe static ulong msclr_002Einterop_002Edetails_002EGetAnsiStringSize(string _str)
	{
        //IL_0029: Expected I, but got I8
        //IL_0029: Expected I, but got I8
        //IL_0029: Expected I, but got I8
        if (_str == null)
        {
            throw new ArgumentNullException(nameof(_str));
        }
        fixed (char* ptr = _str)
        {
            ulong num = (ulong)WideCharToMultiByte(3u, 1024u, ptr, _str.Length, null, 0, null, null);
            if (num == 0L && _str.Length != 0)
            {
                throw new ArgumentException("Conversion from WideChar to MultiByte failed.  Please check the content of the string and/or locale settings.");
            }
            return num + 1;
        }
    }

	internal unsafe static void msclr_002Einterop_002Edetails_002EWriteAnsiString(sbyte* _buf, ulong _size, string _str)
	{
		// 修复 CS0030: 无法将类型“string”转换为“byte*”
		if (_str == null)
		{
			throw new ArgumentNullException(nameof(_str));
		}
		fixed (char* ptr = _str)
		{
			if (_size > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("Size of string exceeds INT_MAX.");
			}
			ulong num = (ulong)WideCharToMultiByte(3u, 1024u, ptr, _str.Length, _buf, (int)_size, null, null);
			if (num < _size && (num != 0L || _size == 1))
			{
				*(sbyte*)(num + (ulong)(nint)_buf) = 0;
				return;
			}
			throw new ArgumentException("Conversion from WideChar to MultiByte failed.  Please check the content of the string and/or locale settings.");
		}
	}

	internal unsafe static basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* msclr_002Einterop_002Emarshal_as_003Cclass_0020std_003A_003Abasic_string_003Cchar_002Cstruct_0020std_003A_003Achar_traits_003Cchar_003E_002Cclass_0020std_003A_003Aallocator_003Cchar_003E_0020_003E_002Cclass_0020System_003A_003AString_0020_005E_003E(basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* P_0, string* _from_obj)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out uint num);
		try
		{
			num = 0u;
			if (*_from_obj == null)
			{
				throw new ArgumentNullException("NULLPTR is not supported for this conversion.");
			}
			std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bctor_007D(P_0);
			num = 1u;
			ulong num2 = msclr_002Einterop_002Edetails_002EGetAnsiStringSize(*_from_obj);
			if (num2 > 1)
			{
				std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002Eresize(P_0, num2 - 1);
				msclr_002Einterop_002Edetails_002EWriteAnsiString(std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_005B_005D(P_0, 0uL), num2, *_from_obj);
			}
			return P_0;
		}
		catch
		{
			//try-fault
			if ((num & 1) != 0)
			{
				num &= 0xFFFFFFFEu;
				___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E*, void>)(&std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D), P_0);
			}
			throw;
		}
	}
    internal static IArtDefElement Firaxis_Granny_A0xe79110d2_FindFirstElementWithName(string name)
{
        new CivTechRegistry();
        IArtDefRegistry artDefRegistry = CivTechRegistry.ArtDefRegistry;
        if (artDefRegistry != null)
        {
            string text = "LodSettings";
            IArtDefCollection[] suitableCollections = artDefRegistry.GetSuitableCollections(text, text);
            int i = 0;
            for (int num = suitableCollections.Length; i < num; i++)
            {
                foreach (IArtDefElement element in suitableCollections[i].Elements)
                {
                    if (element.Name.Equals(name))
                    {
                        return element;
                    }
                }
            }
        }
        return null;
    }
    internal unsafe static void Firaxis_Granny_A0xe79110d2_GetLodSettings(uint* pWidth, uint* pHeight, float* pZnear, float* pZFar, float* pFovy, float* pDistanceStep, float* pMinArea, float* pMaxDensity, float* pTargetDensity)
{
        *pWidth = 1024u;
        *pHeight = 768u;
        *pZnear = 0.1f;
        *pZFar = 1000f;
        *pFovy = (float)System.Math.PI / 2f;
        *pDistanceStep = 100f;
        *pMinArea = 15f;
        *pMaxDensity = 50f;
        *pTargetDensity = 25f;
        string text = null;
        new CivTechRegistry();
        ICivTechService civTechService = CivTechRegistry.CivTechService;
        if (civTechService != null)
        {
            ProjectEnvironment primaryProject = civTechService.PrimaryProject;
            if (primaryProject != null && primaryProject.Name != null && primaryProject.Name.Length != 0)
            {
                text = primaryProject.Name;
            }
        }
        IArtDefElement artDefElement = Firaxis_Granny_A0xe79110d2_FindFirstElementWithName(text + "_LodSettings");
        if (artDefElement == null)
        {
            artDefElement = Firaxis_Granny_A0xe79110d2_FindFirstElementWithName("Default_LodSettings");
            if (artDefElement == null)
            {
                return;
            }
        }
        IValueSet fields = artDefElement.Fields;
        if (fields != null)
        {
            IIntValue intValue = (IIntValue)fields.FindValue("Width");
            if (intValue != null)
            {
                uint parameterValue = (uint)intValue.ParameterValue;
                uint num = 1024u;
                uint* ptr = Platform_002EMax_003Cunsigned_0020int_003E(&num, &parameterValue);
                *pWidth = *ptr;
            }
            IIntValue intValue2 = (IIntValue)fields.FindValue("Height");
            if (intValue2 != null)
            {
                uint parameterValue2 = (uint)intValue2.ParameterValue;
                uint num2 = 768u;
                uint* ptr2 = Platform_002EMax_003Cunsigned_0020int_003E(&num2, &parameterValue2);
                *pHeight = *ptr2;
            }
            IFloatValue floatValue = (IFloatValue)fields.FindValue("ZNear");
            if (floatValue != null)
            {
                *pZnear = floatValue.ParameterValue;
            }
            IFloatValue floatValue2 = (IFloatValue)fields.FindValue("ZFar");
            if (floatValue2 != null)
            {
                *pZFar = floatValue2.ParameterValue;
            }
            IFloatValue floatValue3 = (IFloatValue)fields.FindValue("FieldOfView");
            if (floatValue3 != null)
            {
                *pFovy = floatValue3.ParameterValue * ((float)System.Math.PI / 180f);
            }
            IFloatValue floatValue4 = (IFloatValue)fields.FindValue("DistanceStep");
            if (floatValue4 != null)
            {
                *pDistanceStep = floatValue4.ParameterValue;
            }
            IFloatValue floatValue5 = (IFloatValue)fields.FindValue("CullThreshold");
            if (floatValue5 != null)
            {
                *pMinArea = floatValue5.ParameterValue;
            }
            IFloatValue floatValue6 = (IFloatValue)fields.FindValue("MaximumDensity");
            if (floatValue6 != null)
            {
                *pMaxDensity = floatValue6.ParameterValue;
            }
            IFloatValue floatValue7 = (IFloatValue)fields.FindValue("TargetDensity");
            if (floatValue7 != null)
            {
                *pTargetDensity = floatValue7.ParameterValue;
            }
        }
    }
    internal unsafe static void Firaxis_002EGranny_002E_003FA0xe79110d2_002EGetLods(GrannyModel model, List<IGrannyLod> lods)
{
        MeshBounds meshBounds = new MeshBounds(new Point3F(float.MaxValue, float.MaxValue, float.MaxValue), new Point3F(1.1754944E-38f, 1.1754944E-38f, 1.1754944E-38f));
        MeshBounds meshBounds2 = meshBounds;
        uint num = 0u;
        int num2 = ((model.MeshBindings != null) ? model.MeshBindings.Count : 0);
        uint num3 = 0u;
        if (0u < (uint)num2)
        {
            do
            {
                IGrannyMesh grannyMesh = model.MeshBindings[(int)num3];
                MeshBounds boundingBox = grannyMesh.BoundingBox;
                meshBounds2.Expand(boundingBox);
                num += (uint)grannyMesh.IndexCount;
                num3++;
            }
            while (num3 < (uint)num2);
        }
        Point3F kMax = meshBounds2.kMax;
        Point3F kMin = meshBounds2.kMin;
        System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXBoundBox fGXBoundBox);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector2);
        FGXBoundBox_002E_007Bctor_007D
(&fGXBoundBox, *FGXVector3_002E_007Bctor_007D(&fGXVector, kMin.x, kMin.y, kMin.z), *FGXVector3_002E_007Bctor_007D(&fGXVector2, kMax.x, kMax.y, kMax.z));
        if (!FGXBoundBox_002Eis_volume(&fGXBoundBox))
        {
            return;
        }
        System.Runtime.CompilerServices.Unsafe.SkipInit(out uint num4);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out uint num5);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out float num6);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out float num7);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out float num8);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out float num9);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out float num10);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out float num11);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out float num12);
        Firaxis_Granny_A0xe79110d2_GetLodSettings(&num4, &num5, &num6, &num7, &num8, &num9, &num10, &num11, &num12);
        System.Runtime.CompilerServices.Unsafe.SkipInit(out ProjectionDesc projectionDesc);
        *(uint*)(&projectionDesc) = num4;
        System.Runtime.CompilerServices.Unsafe.As<ProjectionDesc, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref projectionDesc, 4)) = num5;
        System.Runtime.CompilerServices.Unsafe.As<ProjectionDesc, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref projectionDesc, 8)) = num6;
        System.Runtime.CompilerServices.Unsafe.As<ProjectionDesc, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref projectionDesc, 12)) = num7;
        System.Runtime.CompilerServices.Unsafe.As<ProjectionDesc, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref projectionDesc, 16)) = num8;
        System.Runtime.CompilerServices.Unsafe.SkipInit(out Vector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E obj);
        Types_002EVector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E_002E_007Bctor_007D(&obj);
        try
        {
            uint num13 = Math_002EMesh_002ECreateLodTargets(&projectionDesc, &fGXBoundBox, num, num11, num12, num9, num10, &obj);
            if (0 < num13)
            {
                float num14 = 1f / (float)num;
                ulong num15 = 0uL;
                uint num16 = num13;
                do
                {
                    GrannyLod grannyLod = new GrannyLod();
                    LodTarget* ptr = Types_002EVector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E_002E_005B_005D(&obj, num15);
                    grannyLod.TargetIndexCount = *(int*)ptr;
                    grannyLod.TransitionArea = *(float*)((ulong)(nint)Types_002EVector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E_002E_005B_005D(&obj, num15) + 4uL);
                    LodTarget* ptr2 = Types_002EVector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E_002E_005B_005D(&obj, num15);
                    grannyLod.Reduction = 1f - (float)(*(uint*)ptr2) * num14;
                    model.Lods.Add(grannyLod);
                    num15++;
                    num16 += uint.MaxValue;
                }
                while (num16 != 0);
            }
        }
        catch
        {
            //try-fault
            ___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Vector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E*, void >)(&Types_002EVector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E_002E_007Bdtor_007D), &obj);
            throw;
        }
        Types_002EVector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E_002E_007Bdtor_007D(&obj);
    }

    internal unsafe static sbyte* Firaxis_002EGranny_002E_003FA0x7673ad18_002EGrannyFileLoaderRebaseToStringHash(void* Data, uint Identifier)
	{
		Dictionary<uint, IntPtr> dictionary = gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E_002E_002EPE_0024AAV_003F_0024Dictionary_0040IVIntPtr_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E*)Data);
		IntPtr value = default(IntPtr);
		if (dictionary.TryGetValue(Identifier, out value))
		{
			return (sbyte*)value.ToPointer();
		}
		return (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref _003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040);
	}

	internal unsafe static void Firaxis_002EGranny_002EGFLStringHashWrapper_002E_007Bdtor_007D(GFLStringHashWrapper* P_0)
	{
		gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E_002E_007Bdtor_007D((gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E*)P_0);
	}

	[SecuritySafeCritical]
	internal unsafe static Dictionary<uint, IntPtr> gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E_002E_002EPE_0024AAV_003F_0024Dictionary_0040IVIntPtr_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040(gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E* P_0)
	{
		//IL_0009: Expected I, but got I8
		IntPtr intPtr = new IntPtr((void*)(*(ulong*)P_0));
		return (Dictionary<uint, IntPtr>)((GCHandle)intPtr).Target;
	}

	[DebuggerStepThrough]
	[SecurityCritical]
	internal unsafe static gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E* gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E_002E_003D(gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E* P_0, Dictionary<uint, IntPtr> t)
	{
		//IL_0009: Expected I, but got I8
		IntPtr intPtr = new IntPtr((void*)(*(ulong*)P_0));
		GCHandle gCHandle = (GCHandle)intPtr;
		gCHandle.Target = t;
		return P_0;
	}

	[SecurityCritical]
	[DebuggerStepThrough]
	internal unsafe static void gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E_002E_007Bdtor_007D(gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E* P_0)
	{
		//IL_0009: Expected I, but got I8
		IntPtr intPtr = new IntPtr((void*)(*(ulong*)P_0));
		((GCHandle)intPtr).Free();
		*(long*)P_0 = 0L;
	}

	[SecuritySafeCritical]
	[DebuggerStepThrough]
	internal unsafe static gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E* gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E_002E_007Bctor_007D(gcroot_003CSystem_003A_003ACollections_003A_003AGeneric_003A_003ADictionary_003Cunsigned_0020int_002CSystem_003A_003AIntPtr_003E_0020_005E_003E* P_0)
	{
		//IL_0015: Expected I8, but got I
		*(long*)P_0 = (nint)((IntPtr)GCHandle.Alloc(null)).ToPointer();
		return P_0;
	}

	[HandleProcessCorruptedStateExceptions]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SecurityCritical]
	[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
	internal unsafe static void ___CxxCallUnwindDtor(delegate*<void*, void> pDtor, void* pThis)
	{
		try
		{
			pDtor(pThis);
		}
		catch when (__FrameUnwindFilter((global::_EXCEPTION_POINTERS*)Marshal.GetExceptionPointers()) != 0)
		{
		}
	}

	[SecurityCritical]
	[DebuggerStepThrough]
	internal unsafe static System.ValueType _003CCrtImplementationDetails_003E_002EAtExitLock_002E_handle()
	{
		if (_003F_lock_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0PEAXEA != null)
		{
			IntPtr value = new IntPtr(_003F_lock_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0PEAXEA);
			return GCHandle.FromIntPtr(value);
		}
		return null;
	}

	[SecurityCritical]
	[DebuggerStepThrough]
	internal unsafe static void _003CCrtImplementationDetails_003E_002EAtExitLock_002E_lock_Construct(object value)
	{
		//IL_0007: Expected I, but got I8
		_003F_lock_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0PEAXEA = null;
		_003CCrtImplementationDetails_003E_002EAtExitLock_002E_lock_Set(value);
	}

    //[SecurityCritical]
    //[DebuggerStepThrough]
    //internal unsafe static void _003CCrtImplementationDetails_003E_002EAtExitLock_002E_lock_Set(object value)
    //{
    //	System.ValueType valueType = _003CCrtImplementationDetails_003E_002EAtExitLock_002E_handle();
    //	if (valueType == null)
    //	{
    //		valueType = GCHandle.Alloc(value);
    //		_003F_lock_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0PEAXEA = GCHandle.ToIntPtr((GCHandle)valueType).ToPointer();
    //	}
    //	else
    //	{
    //		((GCHandle)valueType).Target = value;
    //	}
    //}
    [SecurityCritical]
    [DebuggerStepThrough]
    internal unsafe static void _003CCrtImplementationDetails_003E_002EAtExitLock_002E_lock_Set(object value)
    {
        // 直接检查底层指针字段是否为 null (即未初始化)
        if (_003F_lock_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0PEAXEA == null)
        {
            // 分配新的 GCHandle
            GCHandle handle = GCHandle.Alloc(value);
            // 将句柄转换为指针并存储
            _003F_lock_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0PEAXEA = GCHandle.ToIntPtr(handle).ToPointer();
        }
        else
        {
            // 如果已存在，从指针恢复 GCHandle 并更新目标对象
            IntPtr ptr = new IntPtr(_003F_lock_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0PEAXEA);
            GCHandle handle = GCHandle.FromIntPtr(ptr);
            handle.Target = value;
        }
    }

    [DebuggerStepThrough]
	[SecurityCritical]
	internal static object _003CCrtImplementationDetails_003E_002EAtExitLock_002E_lock_Get()
	{
		System.ValueType valueType = _003CCrtImplementationDetails_003E_002EAtExitLock_002E_handle();
		if (valueType != null)
		{
			return ((GCHandle)valueType).Target;
		}
		return null;
	}

	[SecurityCritical]
	[DebuggerStepThrough]
	internal unsafe static void _003CCrtImplementationDetails_003E_002EAtExitLock_002E_lock_Destruct()
	{
		//IL_001b: Expected I, but got I8
		System.ValueType valueType = _003CCrtImplementationDetails_003E_002EAtExitLock_002E_handle();
		if (valueType != null)
		{
			((GCHandle)valueType).Free();
			_003F_lock_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0PEAXEA = null;
		}
	}

	[DebuggerStepThrough]
	[SecurityCritical]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static bool _003CCrtImplementationDetails_003E_002EAtExitLock_002EIsInitialized()
	{
		return _003CCrtImplementationDetails_003E_002EAtExitLock_002E_lock_Get() != null;
	}

	[SecurityCritical]
	[DebuggerStepThrough]
	internal static void _003CCrtImplementationDetails_003E_002EAtExitLock_002EAddRef()
	{
		if (!_003CCrtImplementationDetails_003E_002EAtExitLock_002EIsInitialized())
		{
			_003CCrtImplementationDetails_003E_002EAtExitLock_002E_lock_Construct(new object());
			_003F_ref_count_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0HA = 0;
		}
		_003F_ref_count_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0HA++;
	}

	[DebuggerStepThrough]
	[SecurityCritical]
	internal static void _003CCrtImplementationDetails_003E_002EAtExitLock_002ERemoveRef()
	{
		_003F_ref_count_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0HA += -1;
		if (_003F_ref_count_0040AtExitLock_0040_003CCrtImplementationDetails_003E_0040_0040_0024_0024Q0HA == 0)
		{
			_003CCrtImplementationDetails_003E_002EAtExitLock_002E_lock_Destruct();
		}
	}

	[DebuggerStepThrough]
	[SecurityCritical]
	[return: MarshalAs(UnmanagedType.U1)]
	internal static bool _003FA0xc2538164_002E__alloc_global_lock()
	{
		_003CCrtImplementationDetails_003E_002EAtExitLock_002EAddRef();
		return _003CCrtImplementationDetails_003E_002EAtExitLock_002EIsInitialized();
	}

	[DebuggerStepThrough]
	[SecurityCritical]
	internal static void _003FA0xc2538164_002E__dealloc_global_lock()
	{
		_003CCrtImplementationDetails_003E_002EAtExitLock_002ERemoveRef();
	}

	[SecurityCritical]
	internal unsafe static void _exit_callback()
	{
		//IL_003d: Expected I, but got I8
		//IL_004a: Expected I, but got I8
		//IL_0053: Expected I, but got I8
		//IL_005b: Expected I, but got I8
		//IL_005c: Expected I8, but got I
		if (_003FA0xc2538164_002E__exit_list_size == 0L)
		{
			return;
		}
		delegate*<void>* ptr = (delegate*<void>*)DecodePointer(_003FA0xc2538164_002E__onexitbegin_m);
		delegate*<void>* ptr2 = (delegate*<void>*)DecodePointer(_003FA0xc2538164_002E__onexitend_m);
		if ((nint)ptr != -1L && ptr != null && ptr2 != null)
		{
			delegate*<void>* ptr3 = ptr;
			delegate*<void>* ptr4 = ptr2;
			while (true)
			{
				ptr2 = (delegate*<void>*)((ulong)(nint)ptr2 - 8uL);
				if (ptr2 < ptr)
				{
					break;
				}
				if (*(long*)ptr2 != (nint)EncodePointer(null))
				{
					void* intPtr = DecodePointer((void*)(*(ulong*)ptr2));
					*(long*)ptr2 = (nint)EncodePointer(null);
					((delegate*<void>)intPtr)();
					delegate*<void>* ptr5 = (delegate*<void>*)DecodePointer(_003FA0xc2538164_002E__onexitbegin_m);
					delegate*<void>* ptr6 = (delegate*<void>*)DecodePointer(_003FA0xc2538164_002E__onexitend_m);
					if (ptr3 != ptr5 || ptr4 != ptr6)
					{
						ptr3 = ptr5;
						ptr = ptr5;
						ptr4 = ptr6;
						ptr2 = ptr6;
					}
				}
			}
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
		_003FA0xc2538164_002E__dealloc_global_lock();
	}

	[DebuggerStepThrough]
	[SecurityCritical]
	internal unsafe static int _initatexit_m()
	{
		int result = 0;
		if (_003FA0xc2538164_002E__alloc_global_lock())
		{
			_003FA0xc2538164_002E__onexitbegin_m = (delegate*<void>*)EncodePointer(Marshal.AllocHGlobal(256).ToPointer());
			_003FA0xc2538164_002E__onexitend_m = _003FA0xc2538164_002E__onexitbegin_m;
			_003FA0xc2538164_002E__exit_list_size = 32uL;
			result = 1;
		}
		return result;
	}

	[DebuggerStepThrough]
	[SecurityCritical]
	internal unsafe static int _initatexit_app_domain()
	{
		if (_003FA0xc2538164_002E__alloc_global_lock())
		{
			__onexitbegin_app_domain = (delegate*<void>*)EncodePointer(Marshal.AllocHGlobal(256).ToPointer());
			__onexitend_app_domain = __onexitbegin_app_domain;
			__exit_list_size_app_domain = 32uL;
		}
		return 1;
	}

	[SecurityCritical]
	[HandleProcessCorruptedStateExceptions]
	internal unsafe static void _app_exit_callback()
	{
		//IL_003c: Expected I, but got I8
		//IL_0046: Expected I, but got I8
		//IL_004a: Expected I, but got I8
		//IL_004f: Expected I, but got I8
		//IL_005c: Expected I, but got I8
		//IL_006b: Expected I, but got I8
		//IL_0075: Expected I, but got I8
		//IL_0076: Expected I8, but got I
		if (__exit_list_size_app_domain == 0L)
		{
			return;
		}
		delegate*<void>* ptr = (delegate*<void>*)DecodePointer(__onexitbegin_app_domain);
		delegate*<void>* ptr2 = (delegate*<void>*)DecodePointer(__onexitend_app_domain);
		try
		{
			if ((nint)ptr == -1L || ptr == null || ptr2 == null)
			{
				return;
			}
			delegate*<void> delegate_002A = null;
			delegate*<void>* ptr3 = ptr;
			delegate*<void>* ptr4 = ptr2;
			while (true)
			{
				delegate*<void>* ptr5 = null;
				delegate*<void>* ptr6 = null;
				do
				{
					ptr2 = (delegate*<void>*)((ulong)(nint)ptr2 - 8uL);
				}
				while (ptr2 >= ptr && *(long*)ptr2 == (nint)EncodePointer(null));
				if (ptr2 >= ptr)
				{
					delegate_002A = (delegate*<void>)DecodePointer((void*)(*(ulong*)ptr2));
					*(long*)ptr2 = (nint)EncodePointer(null);
					delegate_002A();
					delegate*<void>* ptr7 = (delegate*<void>*)DecodePointer(__onexitbegin_app_domain);
					delegate*<void>* ptr8 = (delegate*<void>*)DecodePointer(__onexitend_app_domain);
					if (ptr3 != ptr7 || ptr4 != ptr8)
					{
						ptr3 = ptr7;
						ptr = ptr7;
						ptr4 = ptr8;
						ptr2 = ptr8;
					}
					continue;
				}
				break;
			}
		}
		finally
		{
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			_003FA0xc2538164_002E__dealloc_global_lock();
		}
	}

	[DllImport("KERNEL32.dll")]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SuppressUnmanagedCodeSecurity]
	[SecurityCritical]
	public unsafe static extern void* DecodePointer(void* _Ptr);

	[DllImport("KERNEL32.dll")]
	[SuppressUnmanagedCodeSecurity]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SecurityCritical]
	public unsafe static extern void* EncodePointer(void* _Ptr);

	[DebuggerStepThrough]
	[SecurityCritical]
    //internal unsafe static int _initterm_e(delegate* unmanaged[Cdecl, Cdecl]<int>* pfbegin, delegate* unmanaged[Cdecl, Cdecl]<int>* pfend)
    //{
    //	//IL_001c: Expected I, but got I8
    //	//IL_0015: Expected I, but got I8
    //	int num = 0;
    //	if (pfbegin < pfend)
    //	{
    //		while (num == 0)
    //		{
    //			ulong num2 = *(ulong*)pfbegin;
    //			if (num2 != 0L)
    //			{
    //				num = ((delegate* unmanaged[Cdecl, Cdecl]<int>)num2)();
    //			}
    //			pfbegin = (delegate* unmanaged[Cdecl, Cdecl]<int>*)((ulong)(nint)pfbegin + 8uL);
    //			if (pfbegin >= pfend)
    //			{
    //				break;
    //			}
    //		}
    //	}
    //	return num;
    //}

    internal unsafe static void msclr_002Einterop_002Edetails_002Echar_buffer_003Cchar_003E_002E_007Bdtor_007D
    (char_buffer_003Cchar_003E* P_0)
{
        //IL_0007: Expected I, but got I8
        delete_005B_005D((void*)(*(ulong*)P_0));
    }
internal unsafe static int _initterm_e(delegate*<int>* pfbegin, delegate*<int>* pfend)
    {
        int num = 0;
        if (pfbegin < pfend)
        {
            while (num == 0)
            {
                ulong num2 = *(ulong*)pfbegin;
                if (num2 != 0L)
                {
                    num = ((delegate*<int>)num2)();
                }
                pfbegin = (delegate*<int>*)((ulong)(nint)pfbegin + 8uL);
                if (pfbegin >= pfend)
                {
                    break;
                }
            }
        }
        return num;
    }

    [SecurityCritical]
	[DebuggerStepThrough]
	internal unsafe static void _initterm(delegate* <void>* pfbegin, delegate* <void>* pfend)
	{
		//IL_0016: Expected I, but got I8
		//IL_0010: Expected I, but got I8
		if (pfbegin >= pfend)
		{
			return;
		}
		do
		{
			ulong num = *(ulong*)pfbegin;
			if (num != 0L)
			{
				((delegate* <void>)num)();
			}
			pfbegin = (delegate* <void>*)((ulong)(nint)pfbegin + 8uL);
		}
		while (pfbegin < pfend);
	}

	[DebuggerStepThrough]
	internal static ModuleHandle _003CCrtImplementationDetails_003E_002EThisModule_002EHandle()
	{
		return typeof(_003CCrtImplementationDetails_003E.ThisModule).Module.ModuleHandle;
	}

	[DebuggerStepThrough]
	[SecurityCritical]
	[SecurityPermission(SecurityAction.Assert, UnmanagedCode = true)]
	internal unsafe static void _initterm_m(delegate*<void*>* pfbegin, delegate*<void*>* pfend)
	{
		//IL_001c: Expected I, but got I8
		//IL_0010: Expected I, but got I8
		if (pfbegin >= pfend)
		{
			return;
		}
		do
		{
			ulong num = *(ulong*)pfbegin;
			if (num != 0L)
			{
				_003CCrtImplementationDetails_003E_002EThisModule_002EResolveMethod_003Cvoid_0020const_0020_002A_0020__clrcall_0028void_0029_003E((delegate*<void*>)num)();
			}
			pfbegin = (delegate*<void*>*)((ulong)(nint)pfbegin + 8uL);
		}
		while (pfbegin < pfend);
	}

	[SecurityCritical]
	[DebuggerStepThrough]
	internal unsafe static delegate*<void*> _003CCrtImplementationDetails_003E_002EThisModule_002EResolveMethod_003Cvoid_0020const_0020_002A_0020__clrcall_0028void_0029_003E(delegate*<void*> methodToken)
	{
		return (delegate*<void*>)_003CCrtImplementationDetails_003E_002EThisModule_002EHandle().ResolveMethodHandle((int)methodToken).GetFunctionPointer().ToPointer();
	}

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void Firaxis_002EGranny_002ESampleAnimation(granny_model* P_0, granny_animation* P_1, sbyte* P_2, float P_3, float* P_4, float* P_5);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern sbyte* GrannyMemoryArenaPushString(memory_arena* P_0, sbyte* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyFreeMemoryArena(memory_arena* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern int GrannyGetMemberTypeSize(granny_data_type_definition* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern int GrannyGetMemberUnitSize(granny_data_type_definition* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void* GrannyMemoryArenaPush(memory_arena* P_0, ulong P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern memory_arena* GrannyNewMemoryArena();

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyEvaluateCurveAtT(int P_0, [MarshalAs(UnmanagedType.U1)] bool P_1, [MarshalAs(UnmanagedType.U1)] bool P_2, granny_curve2* P_3, [MarshalAs(UnmanagedType.U1)] bool P_4, float P_5, float P_6, float* P_7, float* P_8);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyCurveIsConstantOrIdentity(granny_curve2* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyCurveIsIdentity(granny_curve2* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern int GrannyCurveGetKnotCount(granny_curve2* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern int GrannyCurveGetDegree(granny_curve2* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E_002E_0021_003D(_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E* P_0, _Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern _Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E* std_002E_Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E_002E_002B_002B(_Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern granny_curve2** std_002E_Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E_002E_002A(_Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern float* std_002Evector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E_002E_005B_005D(vector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E* P_0, ulong P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void std_002Evector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E_002E_007Bdtor_007D(vector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern vector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E* std_002Evector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E_002E_007Bctor_007D(vector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E* P_0, ulong P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void std_002Evector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E_002Epush_back(vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* P_0, granny_curve2** P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern _Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E* std_002Evector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E_002Eend(vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* P_0, _Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern _Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E* std_002Evector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E_002Ebegin(vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* P_0, _Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void std_002Evector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E_002E_007Bdtor_007D(vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* std_002Evector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E_002E_007Bctor_007D(vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002Epush_back(vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E* P_0, granny_data_type_definition** P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern granny_data_type_definition** std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_005B_005D(vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E* P_0, ulong P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern ulong std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002Esize(vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bdtor_007D(vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E* std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bctor_007D(vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyEndFile(file_builder* P_0, sbyte* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyPreserveObjectFileSections(file_data_tree_writer* P_0, granny_file* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannySetFileSectionFormat(file_builder* P_0, int P_1, granny_compression_type P_2, int P_3);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern sbyte* GrannyGetTemporaryDirectory();

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern file_builder* GrannyBeginFile(int P_0, uint P_1, granny_grn_file_magic_value* P_2, sbyte* P_3, sbyte* P_4);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern int GrannyGetResultingVariantTypeSize(variant_builder* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern int GrannyGetResultingVariantObjectSize(variant_builder* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyEndVariantInPlace(variant_builder* P_0, void* P_1, granny_data_type_definition** P_2, void* P_3, void** P_4);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyAddIntegerMember(variant_builder* P_0, sbyte* P_1, int P_2);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyAddReferenceMember(variant_builder* P_0, sbyte* P_1, granny_data_type_definition* P_2, void* P_3);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyAddStringMember(variant_builder* P_0, sbyte* P_1, sbyte* P_2);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern variant_builder* GrannyBeginVariant(string_table* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern granny_extract_track_mask_result GrannyExtractTrackMask(track_mask* P_0, int P_1, granny_skeleton* P_2, sbyte* P_3, float P_4, [MarshalAs(UnmanagedType.U1)] bool P_5);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern track_mask* GrannyNewTrackMask(float P_0, int P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyFindMatchingMember(granny_data_type_definition* P_0, void* P_1, sbyte* P_2, granny_variant* P_3);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyFreeMemoryWriterBuffer(byte* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern granny_file_info* GrannyGetFileInfo(granny_file* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern granny_file_info* GrannyGetModFileInfo(granny_file* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyIsMODFile(granny_file* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern granny_file* GrannyReadEntireFileFromMemory(int P_0, void* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyStealMemoryWriterBuffer(granny_file_writer* P_0, byte** P_1, int* P_2);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyEndFileToWriter(file_builder* P_0, granny_file_writer* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern granny_file_writer* GrannyCreateMemoryFileWriter(int P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyEndFileDataTreeWriting(file_data_tree_writer* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyWriteDataTreeToFileBuilder(file_data_tree_writer* P_0, file_builder* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern file_data_tree_writer* GrannyBeginFileDataTreeWriting(granny_data_type_definition* P_0, void* P_1, int P_2, int P_3);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern file_builder* GrannyBeginFileInMemory(int P_0, uint P_1, granny_grn_file_magic_value* P_2, int P_3);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyFreeStringTable(string_table* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyFreeCurve(granny_curve2* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyFreeFile(granny_file* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void delete(void* P_0, ulong P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void* @new(ulong P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern string_table* GrannyNewStringTable();

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyDeallocateBSplineSolver(bspline_solver* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern granny_curve2* GrannyCompressCurve(bspline_solver* P_0, uint P_1, granny_compress_curve_parameters* P_2, float* P_3, int P_4, int P_5, float P_6, bool* P_7);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern bspline_solver* GrannyAllocateBSplineSolver(int P_0, int P_1, int P_2);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyEnsureQuaternionContinuity(int P_0, float* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyCurveExtractKnotValues(granny_curve2* P_0, int P_1, int P_2, float* P_3, float* P_4, float* P_5);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern int GrannyCurveGetDimension(granny_curve2* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool Platform_002EAssertDlg(sbyte* P_0, sbyte* P_1, sbyte* P_2, uint P_3, bool* P_4, Platform.ErrorType P_5);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyCurveIsKeyframed(granny_curve2* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002Eresize(basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* P_0, ulong P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern sbyte* std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_005B_005D(basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* P_0, ulong P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D(basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bctor_007D(basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bctor_007D(basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* P_0, basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void std_002E_String_val_003Cstd_003A_003A_Simple_types_003Cchar_003E_0020_003E_002E_Bxty_002E_007Bdtor_007D(_String_val_003Cstd_003A_003A_Simple_types_003Cchar_003E_0020_003E._Bxty* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void* new_005B_005D(ulong P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool GrannyRebasePointersStringCallback(granny_data_type_definition* P_0, void* P_1, long P_2, delegate* <void*, uint, sbyte*> P_3, void* P_4);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern granny_string_database* GrannyGetStringDatabase(granny_file* P_0);

    [DllImport("vcruntime140.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [MethodImpl(MethodImplOptions.Unmanaged, MethodCodeType = MethodCodeType.Native)]
    [SuppressUnmanagedCodeSecurity]
    internal unsafe static extern void __CxxUnregisterExceptionObject(void* P_0, int P_1);

    [DllImport("vcruntime140.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [MethodImpl(MethodImplOptions.Unmanaged, MethodCodeType = MethodCodeType.Native)]
    [SuppressUnmanagedCodeSecurity]
    internal static extern int __CxxQueryExceptionSize();

    [DllImport("vcruntime140.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [MethodImpl(MethodImplOptions.Unmanaged, MethodCodeType = MethodCodeType.Native)]
    [SuppressUnmanagedCodeSecurity]
    internal unsafe static extern int __CxxDetectRethrow(void* P_0);

    [DllImport("vcruntime140.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [MethodImpl(MethodImplOptions.Unmanaged, MethodCodeType = MethodCodeType.Native)]
    [SuppressUnmanagedCodeSecurity]
    internal unsafe static extern int __CxxRegisterExceptionObject(void* P_0, void* P_1);

    [DllImport("vcruntime140.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [MethodImpl(MethodImplOptions.Unmanaged, MethodCodeType = MethodCodeType.Native)]
    [SuppressUnmanagedCodeSecurity]
    internal unsafe static extern int __CxxExceptionFilter(void* P_0, void* P_1, int P_2, void* P_3);

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [MethodImpl(MethodImplOptions.Unmanaged, MethodCodeType = MethodCodeType.Native)]
    [SuppressUnmanagedCodeSecurity]
    internal unsafe static extern int WideCharToMultiByte(uint P_0, uint P_1, char* P_2, int P_3, sbyte* P_4, int P_5, sbyte* P_6, int* P_7);
    [MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void delete_005B_005D(void* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern sbyte* std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002Ec_str(basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern int GrannyGetMeshIndexCount(granny_mesh* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void FGXBoundBox_002EExpand(FGXBoundBox* P_0, FGXVector3* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void GrannyGetSingleVertex(granny_vertex_data* P_0, int P_1, granny_data_type_definition* P_2, void* P_3);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern FGXVector3* FGXVector3_002E_007Bctor_007D(FGXVector3* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern FGXBoundBox* FGXBoundBox_002EDegenerate(FGXBoundBox* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern LodTarget* Types_002EVector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E_002E_005B_005D(Vector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E* P_0, ulong P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void Types_002EVector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E_002E_007Bdtor_007D(Vector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern Vector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E* Types_002EVector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E_002E_007Bctor_007D(Vector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern uint* Platform_002EMax_003Cunsigned_0020int_003E(uint* P_0, uint* P_1);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern uint Math_002EMesh_002ECreateLodTargets(ProjectionDesc* P_0, FGXBoundBox* P_1, uint P_2, float P_3, float P_4, float P_5, float P_6, Vector_003CMath_003A_003AMesh_003A_003ALodTarget_002CTypes_003A_003ALinearAlloc_003A_003ADefault_003C1_002C0_003E_0020_003E* P_7);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe static extern bool FGXBoundBox_002Eis_volume(FGXBoundBox* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern FGXVector3* FGXVector3_002E_007Bctor_007D(FGXVector3* P_0, float P_1, float P_2, float P_3);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern FGXBoundBox* FGXBoundBox_002E_007Bctor_007D(FGXBoundBox* P_0, FGXVector3 P_1, FGXVector3 P_2);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void Platform_002ETempHeap_002EShutdown(TempHeap* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern TempHeap* Platform_002EGetTempHeap();

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern ulong Platform_002ETempHeap_002EStartup(TempHeap* P_0, ulong P_1, byte* P_2);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void Platform_002ESetLogRoot(char* P_0);

	[MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal unsafe static extern void* _getFiberPtrId();

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [MethodImpl(MethodImplOptions.Unmanaged, MethodCodeType = MethodCodeType.Native)]
    [SuppressUnmanagedCodeSecurity]
    internal static extern void _cexit();

    [DllImport("kernel32.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [MethodImpl(MethodImplOptions.Unmanaged, MethodCodeType = MethodCodeType.Native)]
    [SuppressUnmanagedCodeSecurity]
    internal static extern void Sleep(uint P_0);

    [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [MethodImpl(MethodImplOptions.Unmanaged, MethodCodeType = MethodCodeType.Native)]
    [SuppressUnmanagedCodeSecurity]
    internal static extern void abort();

    [MethodImpl(MethodImplOptions.Unmanaged | MethodImplOptions.PreserveSig, MethodCodeType = MethodCodeType.Native)]
	[SuppressUnmanagedCodeSecurity]
	internal static extern void __security_init_cookie();

    [DllImport("vcruntime140.dll", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    [MethodImpl(MethodImplOptions.Unmanaged, MethodCodeType = MethodCodeType.Native)]
    [SuppressUnmanagedCodeSecurity]
    internal unsafe static extern int __FrameUnwindFilter(global::_EXCEPTION_POINTERS* P_0);
}
