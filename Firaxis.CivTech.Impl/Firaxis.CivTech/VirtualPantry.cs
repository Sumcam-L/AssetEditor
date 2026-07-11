using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Firaxis.CivTech.AssetObjects;
using Platform;
using std;
using String;

namespace Firaxis.CivTech;

internal class VirtualPantry : IVirtualPantry
{
	private unsafe global::AssetObjects.VirtualPantry* m_pkPantry = null;

	public unsafe virtual IEnumerable<string> PantryRoots
	{
		get
		{
			IList<string> list = new List<string>();
			int num = 0;
			if (0 < global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetNumPantryRoots(m_pkPantry))
			{
				do
				{
					char* value = global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetPantryRoot(m_pkPantry, num);
					list.Add(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(value));
					num++;
				}
				while (num < global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetNumPantryRoots(m_pkPantry));
			}
			return list;
		}
	}

	public VirtualPantry(IEnumerable<string> pantries)
		: this()
	{
		try
		{
			foreach (string pantry in pantries)
			{
				AddPantryRoot(pantry);
			}
			return;
		}
		catch
		{
			//try-fault
			((IDisposable)this).Dispose();
			throw;
		}
	}

	public unsafe VirtualPantry()
	{
		//IL_0008: Expected I, but got I8
		//IL_002f: Expected I, but got I8
		//IL_0021: Expected I4, but got I8
		global::AssetObjects.VirtualPantry* ptr = (global::AssetObjects.VirtualPantry*)global::_003CModule_003E.@new(24uL);
		global::AssetObjects.VirtualPantry* pkPantry;
		try
		{
			if (ptr != null)
			{
				// IL initblk instruction
				System.Runtime.CompilerServices.Unsafe.InitBlock(ptr, 0, 24);
				global::_003CModule_003E.std_002Evector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002Cstd_003A_003Aallocator_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_0020_003E_0020_003E_002E_007Bctor_007D((vector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002Cstd_003A_003Aallocator_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_0020_003E_0020_003E*)ptr);
				pkPantry = ptr;
			}
			else
			{
				pkPantry = null;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, 24uL);
			throw;
		}
		m_pkPantry = pkPantry;
	}

	private void _007EVirtualPantry()
	{
		_0021VirtualPantry();
	}

	private unsafe void _0021VirtualPantry()
	{
		global::AssetObjects.VirtualPantry* pkPantry = m_pkPantry;
		if (pkPantry != null)
		{
			global::_003CModule_003E.std_002Evector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002Cstd_003A_003Aallocator_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_0020_003E_0020_003E_002E_007Bdtor_007D((vector_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002Cstd_003A_003Aallocator_003CString_003A_003ABasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_0020_003E_0020_003E*)pkPantry);
			global::_003CModule_003E.delete(pkPantry, 24uL);
		}
	}

	public unsafe virtual void AddPantryRoot(string pantryRoot)
	{
		//IL_002a: Expected I, but got I8
		IOStringWrapper iOStringWrapper = null;
		if (!global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddPantryRoot_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && m_pkPantry == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040LBKPICPP_0040m_pkPantry_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040MAPKAPDK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 85u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddPantryRoot_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pantryRoot);
		try
		{
			iOStringWrapper = iOStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EAddPantryRoot(m_pkPantry, iOStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
	}

	public unsafe virtual string GetPrimaryPantryPath(string pmEntityName, Firaxis.CivTech.AssetObjects.InstanceType entityType)
	{
		//IL_002a: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetPrimaryPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040W4InstanceType_0040AssetObjects_004034_0040_0040Z_00404_NA && m_pkPantry == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040LBKPICPP_0040m_pkPantry_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040MAPKAPDK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 94u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetPrimaryPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040W4InstanceType_0040AssetObjects_004034_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmEntityName);
		string result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			try
			{
				global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetPrimaryPantryPath(m_pkPantry, standardStringWrapper.Value, (global::AssetObjects.InstanceType)entityType, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				result = global::_003CModule_003E._003FA0x42705325_002EGetValidPathOrEmptyString(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E))));
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
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual string GetPantryPath(string entityName, Firaxis.CivTech.AssetObjects.InstanceType entityType, ProjectEnvironment project)
	{
		//IL_002c: Expected I, but got I8
		//IL_0055: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		IOStringWrapper iOStringWrapper = null;
		if (!global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040W4InstanceType_0040AssetObjects_004034_0040PE_0024AAVProjectEnvironment_004034_0040_0040Z_00404_NA && m_pkPantry == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040LBKPICPP_0040m_pkPantry_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040MAPKAPDK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 127u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040W4InstanceType_0040AssetObjects_004034_0040PE_0024AAVProjectEnvironment_004034_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F9_003F_003FGetPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040W4InstanceType_0040AssetObjects_004034_0040PE_0024AAVProjectEnvironment_004034_0040_0040Z_00404_NA && project == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_07GKGAGMNN_0040project_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040MAPKAPDK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 128u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F9_003F_003FGetPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040W4InstanceType_0040AssetObjects_004034_0040PE_0024AAVProjectEnvironment_004034_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(entityName);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
		string result;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			IOStringWrapper iOStringWrapper2 = new IOStringWrapper(project.Paths.GamePantry);
			try
			{
				iOStringWrapper = iOStringWrapper2;
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				try
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out ResultCode resultCode);
					global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EGetXMLPath(&resultCode, iOStringWrapper.Value, standardStringWrapper.Value, (global::AssetObjects.InstanceType)entityType, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
					if (global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode) && !global::_003CModule_003E.String_002EBase_003C2_003E_002EIsEmpty((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E)))
					{
						result = global::_003CModule_003E._003FA0x42705325_002EGetValidPathOrEmptyString(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E))));
						goto IL_00cf;
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
					throw;
				}
				goto end_IL_0075;
				IL_00cf:
				global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				goto IL_00df;
				end_IL_0075:;
			}
			catch
			{
				//try-fault
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			goto end_IL_0060;
			IL_00df:
			((IDisposable)iOStringWrapper).Dispose();
			goto IL_00ee;
			end_IL_0060:;
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		string empty;
		try
		{
			try
			{
				try
				{
					empty = string.Empty;
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
				((IDisposable)iOStringWrapper).Dispose();
				throw;
			}
			((IDisposable)iOStringWrapper).Dispose();
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return empty;
		IL_00ee:
		((IDisposable)standardStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual string GetPantryPath(string entityName, Firaxis.CivTech.AssetObjects.InstanceType entityType)
	{
		//IL_002a: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040W4InstanceType_0040AssetObjects_004034_0040_0040Z_00404_NA && m_pkPantry == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040LBKPICPP_0040m_pkPantry_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040MAPKAPDK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 108u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040W4InstanceType_0040AssetObjects_004034_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(entityName);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
		string empty;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			try
			{
				global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetPantryPath(m_pkPantry, standardStringWrapper.Value, (global::AssetObjects.InstanceType)entityType, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				if (global::_003CModule_003E.String_002EBase_003C2_003E_002EIsEmpty((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E)))
				{
					empty = string.Empty;
					goto IL_0073;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				throw;
			}
			goto end_IL_0034;
			IL_0073:
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			goto IL_0083;
			end_IL_0034:;
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		string result;
		try
		{
			try
			{
				result = global::_003CModule_003E._003FA0x42705325_002EGetValidPathOrEmptyString(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E))));
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
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
		IL_0083:
		((IDisposable)standardStringWrapper).Dispose();
		return empty;
	}

	public unsafe virtual string GetXLPPantryPath(string relativePath)
	{
		//IL_002d: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetXLPPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040_0040Z_00404_NA && m_pkPantry == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040LBKPICPP_0040m_pkPantry_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040MAPKAPDK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 148u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetXLPPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(relativePath);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
		string empty;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			try
			{
				global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetXLPPantryPath(m_pkPantry, standardStringWrapper.Value, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				if (global::_003CModule_003E.String_002EBase_003C2_003E_002EIsEmpty((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E)))
				{
					empty = string.Empty;
					goto IL_0075;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				throw;
			}
			goto end_IL_0037;
			IL_0075:
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			goto IL_0085;
			end_IL_0037:;
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		string result;
		try
		{
			try
			{
				result = global::_003CModule_003E._003FA0x42705325_002EGetValidPathOrEmptyString(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E))));
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
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
		IL_0085:
		((IDisposable)standardStringWrapper).Dispose();
		return empty;
	}

	public unsafe virtual string GetArtDefPantryPath(string relativePath)
	{
		//IL_002d: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetArtDefPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040_0040Z_00404_NA && m_pkPantry == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040LBKPICPP_0040m_pkPantry_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FL_0040MAPKAPDK_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 166u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x42705325_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetArtDefPantryPath_0040VirtualPantry_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAV56_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(relativePath);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
		string empty;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			try
			{
				global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetArtDefPantryPath(m_pkPantry, standardStringWrapper.Value, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				if (global::_003CModule_003E.String_002EBase_003C2_003E_002EIsEmpty((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E)))
				{
					empty = string.Empty;
					goto IL_0075;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
				throw;
			}
			goto end_IL_0037;
			IL_0075:
			global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			goto IL_0085;
			end_IL_0037:;
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		string result;
		try
		{
			try
			{
				result = global::_003CModule_003E._003FA0x42705325_002EGetValidPathOrEmptyString(global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E))));
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
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return result;
		IL_0085:
		((IDisposable)standardStringWrapper).Dispose();
		return empty;
	}

	internal unsafe global::AssetObjects.VirtualPantry* GetNativePantry()
	{
		return m_pkPantry;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021VirtualPantry();
			return;
		}
		try
		{
			_0021VirtualPantry();
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

	~VirtualPantry()
	{
		Dispose(A_0: false);
	}
}
