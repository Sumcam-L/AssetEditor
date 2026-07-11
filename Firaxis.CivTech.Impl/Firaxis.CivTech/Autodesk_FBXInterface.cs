using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using _003CCppImplementationDetails_003E;
using fbxsdk;
using Firaxis.CivTech._003FA0x5a0f875a;
using Firaxis.Error;
using Platform;

namespace Firaxis.CivTech;

public class Autodesk_FBXInterface : IFBXInterface
{
	private unsafe FbxManager* m_FBXMgr = null;

	public unsafe Autodesk_FBXInterface()
	{
		//IL_0008: Expected I, but got I8
		m_FBXMgr = global::_003CModule_003E.fbxsdk_002EFbxManager_002ECreate();
	}

	private void _007EAutodesk_FBXInterface()
	{
		_0021Autodesk_FBXInterface();
	}

	private unsafe void _0021Autodesk_FBXInterface()
	{
		//IL_0013: Expected I, but got I8
		FbxManager* fBXMgr = m_FBXMgr;
		if (fBXMgr != null)
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(ulong*)fBXMgr)))((nint)fBXMgr);
		}
	}

	public unsafe virtual IEnumerable<string> GetAnimations(string fbxFilePath)
	{
		//Discarded unreachable code: IL_01a7
		//IL_0042: Expected I8, but got I
		//IL_0028: Expected I, but got I8
		//IL_0077: Expected I, but got I8
		//IL_008d: Expected I8, but got I
		//IL_00a7: Expected I, but got I8
		//IL_00a7: Expected I, but got I8
		//IL_00a7: Expected I, but got I8
		//IL_00a7: Expected I, but got I8
		//IL_00e7: Expected I, but got I8
		//IL_00b7: Expected I, but got I8
		//IL_0105: Expected I, but got I8
		//IL_0112: Expected I, but got I8
		//IL_0146: Expected I, but got I8
		//IL_015d: Expected I, but got I8
		//IL_0139: Expected I, but got I8
		//IL_0180: Expected I, but got I8
		//IL_01a5: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetAnimations_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA && m_FBXMgr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08GMBLEFNH_0040m_FBXMgr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040LCGDFHHA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 118u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetAnimations_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		FbxManager* fBXMgr = m_FBXMgr;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
		*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) = (nint)global::_003CModule_003E.fbxsdk_002EFbxImporter_002ECreate(fBXMgr, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CF_0040FFOKMGLN_0040Autodesk_FBXInterface_003F3_003F3GetAnimat_0040));
		IEnumerable<string> result;
		try
		{
			if ((byte)((*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0) ? 1 : 0) == 0)
			{
				result = Enumerable.Empty<string>();
				goto IL_0069;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
			throw;
		}
		IList<string> list;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedString scopedString);
			*(long*)(&scopedString) = (nint)Marshal.StringToHGlobalAnsi(fbxFilePath).ToPointer();
			try
			{
				if (((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, int, FbxIOSettings*, byte>)(*(ulong*)(*(long*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)) + 184)))((nint)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), (sbyte*)(*(ulong*)(&scopedString)), -1, null) == 0)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxString fbxString);
					global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bctor_007D(&fbxString, global::_003CModule_003E.fbxsdk_002EFbxStatus_002EGetErrorString((FbxStatus*)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) + 120)));
					try
					{
						throw new Exception(new string(global::_003CModule_003E.fbxsdk_002EFbxString_002EBuffer(&fbxString)));
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxString*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bdtor_007D), &fbxString);
						throw;
					}
				}
				if (!global::_003CModule_003E.fbxsdk_002EFbxImporter_002EIsFBX((FbxImporter*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E))))
				{
					throw new Exception("Not an FBX file!");
				}
				list = new List<string>();
				int num = 0;
				if (0 < global::_003CModule_003E.fbxsdk_002EFbxImporter_002EGetAnimStackCount((FbxImporter*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E))))
				{
					do
					{
						FbxTakeInfo* ptr = global::_003CModule_003E.fbxsdk_002EFbxImporter_002EGetTakeInfo((FbxImporter*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), num);
						if (!global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003FBF_0040_003F_003FGetAnimations_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09BFPMDPCM_0040lTakeInfo_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040LCGDFHHA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 142u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003FBF_0040_003F_003FGetAnimations_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA), (ErrorType)0))
						{
							/*OpCode not supported: DebugBreak*/;
						}
						list.Add(new string(global::_003CModule_003E.fbxsdk_002EFbxString_002EBuffer((FbxString*)((ulong)(nint)ptr + 8uL))));
						num++;
					}
					while (num < global::_003CModule_003E.fbxsdk_002EFbxImporter_002EGetAnimStackCount((FbxImporter*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E))));
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedString*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedString_002E_007Bdtor_007D), &scopedString);
				throw;
			}
			if (*(long*)(&scopedString) != 0L)
			{
				IntPtr hglobal = new IntPtr((void*)(*(ulong*)(&scopedString)));
				Marshal.FreeHGlobal(hglobal);
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
			throw;
		}
		if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0L)
		{
			global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), false);
		}
		return list;
		IL_0069:
		if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0L)
		{
			global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), false);
		}
		return result;
	}

	public unsafe virtual IEnumerable<string> GetMeshes(string fbxFilePath)
	{
		//Discarded unreachable code: IL_0259
		//IL_0045: Expected I8, but got I
		//IL_002b: Expected I, but got I8
		//IL_007a: Expected I, but got I8
		//IL_0090: Expected I8, but got I
		//IL_00aa: Expected I, but got I8
		//IL_00aa: Expected I, but got I8
		//IL_00aa: Expected I, but got I8
		//IL_00aa: Expected I, but got I8
		//IL_00ea: Expected I, but got I8
		//IL_00ba: Expected I, but got I8
		//IL_010e: Expected I8, but got I
		//IL_011a: Expected I, but got I8
		//IL_011a: Expected I, but got I8
		//IL_0163: Expected I, but got I8
		//IL_012a: Expected I, but got I8
		//IL_0173: Expected I, but got I8
		//IL_01ec: Expected I, but got I8
		//IL_019a: Expected I, but got I8
		//IL_0211: Expected I, but got I8
		//IL_01cb: Expected I, but got I8
		//IL_0232: Expected I, but got I8
		//IL_0257: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetMeshes_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA && m_FBXMgr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08GMBLEFNH_0040m_FBXMgr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040LCGDFHHA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 152u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetMeshes_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		FbxManager* fBXMgr = m_FBXMgr;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
		*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) = (nint)global::_003CModule_003E.fbxsdk_002EFbxImporter_002ECreate(fBXMgr, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040JHDNHELG_0040GetMeshes_003F3_003F3FbxImporter_003F_0024AA_0040));
		IEnumerable<string> result;
		try
		{
			if ((byte)((*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0) ? 1 : 0) == 0)
			{
				result = Enumerable.Empty<string>();
				goto IL_006c;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
			throw;
		}
		IList<string> list;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedString scopedString);
			*(long*)(&scopedString) = (nint)Marshal.StringToHGlobalAnsi(fbxFilePath).ToPointer();
			try
			{
				if (((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, int, FbxIOSettings*, byte>)(*(ulong*)(*(long*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)) + 184)))((nint)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), (sbyte*)(*(ulong*)(&scopedString)), -1, null) == 0)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxString fbxString);
					global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bctor_007D(&fbxString, global::_003CModule_003E.fbxsdk_002EFbxStatus_002EGetErrorString((FbxStatus*)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) + 120)));
					try
					{
						throw new Exception(new string(global::_003CModule_003E.fbxsdk_002EFbxString_002EBuffer(&fbxString)));
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxString*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bdtor_007D), &fbxString);
						throw;
					}
				}
				if (!global::_003CModule_003E.fbxsdk_002EFbxImporter_002EIsFBX((FbxImporter*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E))))
				{
					throw new Exception("Not an FBX file!");
				}
				FbxManager* fBXMgr2 = m_FBXMgr;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E);
				*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E) = (nint)global::_003CModule_003E.fbxsdk_002EFbxScene_002ECreate(fBXMgr2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040HLPAJI_0040GetMeshes_003F3_003F3FbxScene_003F_0024AA_0040));
				try
				{
					if (!global::_003CModule_003E.fbxsdk_002EFbxImporter_002EImport((FbxImporter*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), (FbxDocument*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)), false))
					{
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxString fbxString2);
						global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bctor_007D(&fbxString2, global::_003CModule_003E.fbxsdk_002EFbxStatus_002EGetErrorString((FbxStatus*)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) + 120)));
						try
						{
							throw new Exception(new string(global::_003CModule_003E.fbxsdk_002EFbxString_002EBuffer(&fbxString2)));
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxString*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bdtor_007D), &fbxString2);
							throw;
						}
					}
					list = new List<string>();
					int num = 0;
					if (0 < global::_003CModule_003E.fbxsdk_002EFbxScene_002EGetGeometryCount((FbxScene*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E))))
					{
						do
						{
							FbxGeometry* ptr = global::_003CModule_003E.fbxsdk_002EFbxScene_002EGetGeometry((FbxScene*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)), num);
							if (!global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FGetMeshes_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09CAMFCCJJ_0040lGeometry_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040LCGDFHHA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 184u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FGetMeshes_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA), (ErrorType)0))
							{
								/*OpCode not supported: DebugBreak*/;
							}
							FbxNode* ptr2 = global::_003CModule_003E.fbxsdk_002EFbxNodeAttribute_002EGetNode((FbxNodeAttribute*)ptr, 0);
							if (!global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003FBP_0040_003F_003FGetMeshes_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA && ptr2 == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05OCNJGMBO_0040lNode_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040LCGDFHHA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 187u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003FBP_0040_003F_003FGetMeshes_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA), (ErrorType)0))
							{
								/*OpCode not supported: DebugBreak*/;
							}
							list.Add(new string(global::_003CModule_003E.fbxsdk_002EFbxObject_002EGetName((FbxObject*)ptr2)));
							num++;
						}
						while (num < global::_003CModule_003E.fbxsdk_002EFbxScene_002EGetGeometryCount((FbxScene*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E))));
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E);
					throw;
				}
				if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E) != 0L)
				{
					global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)), false);
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedString*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedString_002E_007Bdtor_007D), &scopedString);
				throw;
			}
			if (*(long*)(&scopedString) != 0L)
			{
				IntPtr hglobal = new IntPtr((void*)(*(ulong*)(&scopedString)));
				Marshal.FreeHGlobal(hglobal);
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
			throw;
		}
		if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0L)
		{
			global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), false);
		}
		return list;
		IL_006c:
		if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0L)
		{
			global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), false);
		}
		return result;
	}

	public unsafe virtual IEnumerable<string> GetRootModels(string fbxFilePath)
	{
		//Discarded unreachable code: IL_0233
		//IL_0045: Expected I8, but got I
		//IL_002b: Expected I, but got I8
		//IL_007a: Expected I, but got I8
		//IL_0090: Expected I8, but got I
		//IL_00aa: Expected I, but got I8
		//IL_00aa: Expected I, but got I8
		//IL_00aa: Expected I, but got I8
		//IL_00aa: Expected I, but got I8
		//IL_00ea: Expected I, but got I8
		//IL_00ba: Expected I, but got I8
		//IL_010e: Expected I8, but got I
		//IL_011a: Expected I, but got I8
		//IL_011a: Expected I, but got I8
		//IL_0163: Expected I, but got I8
		//IL_012a: Expected I, but got I8
		//IL_0175: Expected I, but got I8
		//IL_01c3: Expected I, but got I8
		//IL_01a2: Expected I, but got I8
		//IL_01eb: Expected I, but got I8
		//IL_020c: Expected I, but got I8
		//IL_0231: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetRootModels_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA && m_FBXMgr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08GMBLEFNH_0040m_FBXMgr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040LCGDFHHA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 197u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003F2_003F_003FGetRootModels_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		FbxManager* fBXMgr = m_FBXMgr;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
		*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) = (nint)global::_003CModule_003E.fbxsdk_002EFbxImporter_002ECreate(fBXMgr, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BL_0040MJCALJME_0040GetRootModels_003F3_003F3FbxImporter_003F_0024AA_0040));
		IEnumerable<string> result;
		try
		{
			if ((byte)((*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0) ? 1 : 0) == 0)
			{
				result = Enumerable.Empty<string>();
				goto IL_006c;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
			throw;
		}
		IList<string> list;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedString scopedString);
			*(long*)(&scopedString) = (nint)Marshal.StringToHGlobalAnsi(fbxFilePath).ToPointer();
			try
			{
				if (((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, int, FbxIOSettings*, byte>)(*(ulong*)(*(long*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)) + 184)))((nint)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), (sbyte*)(*(ulong*)(&scopedString)), -1, null) == 0)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxString fbxString);
					global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bctor_007D(&fbxString, global::_003CModule_003E.fbxsdk_002EFbxStatus_002EGetErrorString((FbxStatus*)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) + 120)));
					try
					{
						throw new Exception(new string(global::_003CModule_003E.fbxsdk_002EFbxString_002EBuffer(&fbxString)));
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxString*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bdtor_007D), &fbxString);
						throw;
					}
				}
				if (!global::_003CModule_003E.fbxsdk_002EFbxImporter_002EIsFBX((FbxImporter*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E))))
				{
					throw new Exception("Not an FBX file!");
				}
				FbxManager* fBXMgr2 = m_FBXMgr;
				System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E);
				*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E) = (nint)global::_003CModule_003E.fbxsdk_002EFbxScene_002ECreate(fBXMgr2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BI_0040CKGAMJDK_0040GetRootModels_003F3_003F3FbxScene_003F_0024AA_0040));
				try
				{
					if (!global::_003CModule_003E.fbxsdk_002EFbxImporter_002EImport((FbxImporter*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), (FbxDocument*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)), false))
					{
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxString fbxString2);
						global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bctor_007D(&fbxString2, global::_003CModule_003E.fbxsdk_002EFbxStatus_002EGetErrorString((FbxStatus*)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) + 120)));
						try
						{
							throw new Exception(new string(global::_003CModule_003E.fbxsdk_002EFbxString_002EBuffer(&fbxString2)));
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxString*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bdtor_007D), &fbxString2);
							throw;
						}
					}
					list = new List<string>();
					int num = 0;
					if (0 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(global::_003CModule_003E.fbxsdk_002EFbxScene_002EGetRootNode((FbxScene*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E))), false))
					{
						do
						{
							FbxNode* ptr = global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChild(global::_003CModule_003E.fbxsdk_002EFbxScene_002EGetRootNode((FbxScene*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E))), num);
							if (!global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FGetRootModels_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05OCNJGMBO_0040lNode_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FK_0040LCGDFHHA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 229u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x5a0f875a_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FGetRootModels_0040Autodesk_FBXInterface_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAU_003F_0024IEnumerable_0040PE_0024AAVString_0040System_0040_0040_0040Generic_0040Collections_0040System_0040_0040PE_0024AAVString_00408_0040_0040Z_00404_NA), (ErrorType)0))
							{
								/*OpCode not supported: DebugBreak*/;
							}
							list.Add(new string(global::_003CModule_003E.fbxsdk_002EFbxObject_002EGetName((FbxObject*)ptr)));
							num++;
						}
						while (num < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(global::_003CModule_003E.fbxsdk_002EFbxScene_002EGetRootNode((FbxScene*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E))), false));
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E);
					throw;
				}
				if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E) != 0L)
				{
					global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)), false);
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedString*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedString_002E_007Bdtor_007D), &scopedString);
				throw;
			}
			if (*(long*)(&scopedString) != 0L)
			{
				IntPtr hglobal = new IntPtr((void*)(*(ulong*)(&scopedString)));
				Marshal.FreeHGlobal(hglobal);
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
			throw;
		}
		if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0L)
		{
			global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), false);
		}
		return list;
		IL_006c:
		if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0L)
		{
			global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), false);
		}
		return result;
	}

	public virtual Firaxis.Error.ResultCode ExportAnimation(string fbxFilePath, string outputFolder, string animName, string fgxFileName)
	{
		Form form = FindFirstVisibleForm();
		string text = Path.Combine(outputFolder, fgxFileName + ".fgx");
		IntPtr handle = form.Handle;
		if (!global::_003CModule_003E.ExportFBXScene(fbxFilePath, animName, text, ShowInterface: true, ShowProgress: true, handle))
		{
			return new Firaxis.Error.ResultCode("Failed to export granny file: " + text);
		}
		return Firaxis.Error.ResultCode.Success;
	}

	public virtual Firaxis.Error.ResultCode ExportGeometry(string fbxFilePath, string outputFolder, string nodeName, string fgxFileName)
	{
		Form form = FindFirstVisibleForm();
		string text = Path.Combine(outputFolder, fgxFileName + ".fgx");
		IntPtr handle = form.Handle;
		if (!global::_003CModule_003E.ExportFBXScene(fbxFilePath, nodeName, text, ShowInterface: true, ShowProgress: true, handle))
		{
			return new Firaxis.Error.ResultCode("Failed to export granny file: " + text);
		}
		return Firaxis.Error.ResultCode.Success;
	}

	public unsafe virtual Firaxis.Error.ResultCode ExportWIG(string fbxFilePath, string outputFolder, string nodeName, string wigFileName)
	{
		//Discarded unreachable code: IL_0780
		//IL_002b: Expected I8, but got I
		//IL_006b: Expected I, but got I8
		//IL_0081: Expected I8, but got I
		//IL_009b: Expected I, but got I8
		//IL_009b: Expected I, but got I8
		//IL_009b: Expected I, but got I8
		//IL_009b: Expected I, but got I8
		//IL_00db: Expected I, but got I8
		//IL_00ab: Expected I, but got I8
		//IL_00ff: Expected I8, but got I
		//IL_010b: Expected I, but got I8
		//IL_010b: Expected I, but got I8
		//IL_0183: Expected I, but got I8
		//IL_011b: Expected I, but got I8
		//IL_0208: Expected I, but got I8
		//IL_0229: Expected I, but got I8
		//IL_026b: Expected I, but got I8
		//IL_0249: Expected I, but got I8
		//IL_029a: Expected I, but got I8
		//IL_02c9: Expected I, but got I8
		//IL_0347: Expected I, but got I8
		//IL_0376: Expected I, but got I8
		//IL_073c: Expected I, but got I8
		//IL_03a5: Expected I, but got I8
		//IL_075d: Expected I, but got I8
		//IL_077d: Expected I, but got I8
		//IL_03ff: Expected I, but got I8
		//IL_0432: Expected I, but got I8
		//IL_06c7: Expected I, but got I8
		//IL_0461: Expected I, but got I8
		//IL_06e8: Expected I, but got I8
		//IL_0490: Expected I, but got I8
		//IL_0708: Expected I, but got I8
		//IL_04bc: Expected I, but got I8
		//IL_04c8: Expected I, but got I8
		//IL_057a: Expected I, but got I8
		//IL_0590: Expected I, but got I8
		//IL_0590: Expected I, but got I8
		//IL_0522: Expected I, but got I8
		//IL_04ee: Expected I8, but got I
		//IL_05b3: Expected I, but got I8
		//IL_04ff: Expected I, but got I8
		//IL_064b: Expected I, but got I8
		string path = Path.Combine(outputFolder, wigFileName + ".wig");
		FbxManager* fBXMgr = m_FBXMgr;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
		*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) = (nint)global::_003CModule_003E.fbxsdk_002EFbxImporter_002ECreate(fBXMgr, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040FCLJFAJC_0040ExportWIG_003F3_003F3FbxImporter_003F_0024AA_0040));
		Firaxis.Error.ResultCode result;
		try
		{
			if ((byte)((*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0) ? 1 : 0) == 0)
			{
				result = new Firaxis.Error.ResultCode("Failed to import FBX file: " + fbxFilePath);
				goto IL_005d;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
			throw;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedString scopedString);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E);
		FileStream fileStream;
		int iNumToWrite;
		FbxNode* ptr2;
		Firaxis.Error.ResultCode result2;
		try
		{
			*(long*)(&scopedString) = (nint)Marshal.StringToHGlobalAnsi(fbxFilePath).ToPointer();
			try
			{
				if (((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, int, FbxIOSettings*, byte>)(*(ulong*)(*(long*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)) + 184)))((nint)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), (sbyte*)(*(ulong*)(&scopedString)), -1, null) == 0)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxString fbxString);
					global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bctor_007D(&fbxString, global::_003CModule_003E.fbxsdk_002EFbxStatus_002EGetErrorString((FbxStatus*)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) + 120)));
					try
					{
						throw new Exception(new string(global::_003CModule_003E.fbxsdk_002EFbxString_002EBuffer(&fbxString)));
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxString*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bdtor_007D), &fbxString);
						throw;
					}
				}
				if (!global::_003CModule_003E.fbxsdk_002EFbxImporter_002EIsFBX((FbxImporter*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E))))
				{
					throw new Exception("Not an FBX file!");
				}
				FbxManager* fBXMgr2 = m_FBXMgr;
				*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E) = (nint)global::_003CModule_003E.fbxsdk_002EFbxScene_002ECreate(fBXMgr2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040HLPAJI_0040GetMeshes_003F3_003F3FbxScene_003F_0024AA_0040));
				try
				{
					if (!global::_003CModule_003E.fbxsdk_002EFbxImporter_002EImport((FbxImporter*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), (FbxDocument*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)), false))
					{
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxString fbxString2);
						global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bctor_007D(&fbxString2, global::_003CModule_003E.fbxsdk_002EFbxStatus_002EGetErrorString((FbxStatus*)(*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) + 120)));
						try
						{
							throw new Exception(new string(global::_003CModule_003E.fbxsdk_002EFbxString_002EBuffer(&fbxString2)));
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxString*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxString_002E_007Bdtor_007D), &fbxString2);
							throw;
						}
					}
					fileStream = File.OpenWrite(path);
					WriteStringNoLen(fileStream, "WIG");
					WriteU32(fileStream, 65536);
					WriteStringU32Len(fileStream, "Asset Editor");
					WriteStringU32Len(fileStream, "FBXInterface.cpp");
					FbxNode* ptr = global::_003CModule_003E.fbxsdk_002EFbxScene_002EGetRootNode((FbxScene*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)));
					iNumToWrite = CountHairSystems(ptr);
					int num = 0;
					if (0 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(ptr, false))
					{
						while (true)
						{
							ptr2 = global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChild(ptr, num);
							if (!(new string(global::_003CModule_003E.fbxsdk_002EFbxObject_002EGetName((FbxObject*)ptr2)) == nodeName))
							{
								num++;
								if (num >= global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(ptr, false))
								{
									break;
								}
								continue;
							}
							if (ptr2 == null)
							{
								break;
							}
							goto end_IL_00ff;
						}
					}
					result2 = new Firaxis.Error.ResultCode("Failed to find wig root node: " + nodeName);
					goto IL_01f8;
					end_IL_00ff:;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E);
					throw;
				}
				goto end_IL_0081;
				IL_01f8:
				if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E) != 0L)
				{
					global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)), false);
				}
				goto IL_0218;
				end_IL_0081:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedString*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedString_002E_007Bdtor_007D), &scopedString);
				throw;
			}
			goto end_IL_006f;
			IL_0218:
			if (*(long*)(&scopedString) != 0L)
			{
				IntPtr hglobal = new IntPtr((void*)(*(ulong*)(&scopedString)));
				Marshal.FreeHGlobal(hglobal);
			}
			goto IL_0240;
			end_IL_006f:;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
			throw;
		}
		Firaxis.Error.ResultCode result3;
		try
		{
			try
			{
				try
				{
					WriteStringU32Len(fileStream, new string(global::_003CModule_003E.fbxsdk_002EFbxObject_002EGetName((FbxObject*)ptr2)));
					System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate3_003Cdouble_003E fbxVectorTemplate3_003Cdouble_003E);
					FbxVectorTemplate3_003Cdouble_003E* ptr3 = global::_003CModule_003E.fbxsdk_002EFbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E_002EGet((FbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E*)((ulong)(nint)ptr2 + 120uL), &fbxVectorTemplate3_003Cdouble_003E);
					try
					{
						WriteFBXVec3(fileStream, *ptr3);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fbxVectorTemplate3_003Cdouble_003E);
						throw;
					}
					System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate3_003Cdouble_003E fbxVectorTemplate3_003Cdouble_003E2);
					FbxVectorTemplate3_003Cdouble_003E* ptr4 = global::_003CModule_003E.fbxsdk_002EFbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E_002EGet((FbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E*)((ulong)(nint)ptr2 + 136uL), &fbxVectorTemplate3_003Cdouble_003E2);
					try
					{
						WriteFBXVec3(fileStream, *ptr4);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fbxVectorTemplate3_003Cdouble_003E2);
						throw;
					}
					System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate3_003Cdouble_003E fbxVectorTemplate3_003Cdouble_003E3);
					FbxVectorTemplate3_003Cdouble_003E* ptr5 = global::_003CModule_003E.fbxsdk_002EFbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E_002EGet((FbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E*)((ulong)(nint)ptr2 + 152uL), &fbxVectorTemplate3_003Cdouble_003E3);
					try
					{
						WriteFBXVec3(fileStream, *ptr5);
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fbxVectorTemplate3_003Cdouble_003E3);
						throw;
					}
					WriteU32(fileStream, iNumToWrite);
					int num2 = 0;
					if (0 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(ptr2, false))
					{
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate3_003Cdouble_003E fbxVectorTemplate3_003Cdouble_003E4);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate3_003Cdouble_003E fbxVectorTemplate3_003Cdouble_003E5);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate3_003Cdouble_003E fbxVectorTemplate3_003Cdouble_003E6);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate3_003Cdouble_003E fbxVectorTemplate3_003Cdouble_003E7);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate3_003Cdouble_003E fbxVectorTemplate3_003Cdouble_003E8);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate3_003Cdouble_003E fbxVectorTemplate3_003Cdouble_003E9);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate2_003Cdouble_003E fMtx);
						System.Runtime.CompilerServices.Unsafe.SkipInit(out FbxVectorTemplate3_003Cdouble_003E fMtx2);
						while (true)
						{
							FbxNode* ptr6 = global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChild(ptr2, num2);
							if (IsHairSystem(ptr6))
							{
								int num3 = CountHairStrands(ptr6);
								if (num3 != 0)
								{
									WriteStringU32Len(fileStream, new string(global::_003CModule_003E.fbxsdk_002EFbxObject_002EGetName((FbxObject*)ptr6)));
									FbxVectorTemplate3_003Cdouble_003E* ptr7 = global::_003CModule_003E.fbxsdk_002EFbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E_002EGet((FbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E*)((ulong)(nint)ptr6 + 120uL), &fbxVectorTemplate3_003Cdouble_003E4);
									try
									{
										WriteFBXVec3(fileStream, *ptr7);
									}
									catch
									{
										//try-fault
										global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fbxVectorTemplate3_003Cdouble_003E4);
										throw;
									}
									FbxVectorTemplate3_003Cdouble_003E* ptr8 = global::_003CModule_003E.fbxsdk_002EFbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E_002EGet((FbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E*)((ulong)(nint)ptr6 + 136uL), &fbxVectorTemplate3_003Cdouble_003E5);
									try
									{
										WriteFBXVec3(fileStream, *ptr8);
									}
									catch
									{
										//try-fault
										global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fbxVectorTemplate3_003Cdouble_003E5);
										throw;
									}
									FbxVectorTemplate3_003Cdouble_003E* ptr9 = global::_003CModule_003E.fbxsdk_002EFbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E_002EGet((FbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E*)((ulong)(nint)ptr6 + 152uL), &fbxVectorTemplate3_003Cdouble_003E6);
									try
									{
										WriteFBXVec3(fileStream, *ptr9);
									}
									catch
									{
										//try-fault
										global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fbxVectorTemplate3_003Cdouble_003E6);
										throw;
									}
									WriteU32(fileStream, num3);
									int num4 = 0;
									int num5 = 0;
									if (0 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(ptr6, false))
									{
										do
										{
											FbxNode* ptr10 = global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChild(ptr6, num5);
											FbxNodeAttribute* intPtr = global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetNodeAttribute(ptr10);
											if (((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, FbxNodeAttribute.EType>)(*(ulong*)(*(long*)intPtr + 184)))((nint)intPtr) == (FbxNodeAttribute.EType)13)
											{
												FbxNurbsCurve* ptr11 = (FbxNurbsCurve*)global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetNodeAttribute(ptr10);
												num4++;
												WriteStringU32Len(fileStream, new string(global::_003CModule_003E.fbxsdk_002EFbxObject_002EGetName((FbxObject*)ptr10)));
												FbxVectorTemplate3_003Cdouble_003E* ptr12 = global::_003CModule_003E.fbxsdk_002EFbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E_002EGet((FbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E*)((ulong)(nint)ptr10 + 120uL), &fbxVectorTemplate3_003Cdouble_003E7);
												try
												{
													WriteFBXVec3(fileStream, *ptr12);
												}
												catch
												{
													//try-fault
													global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fbxVectorTemplate3_003Cdouble_003E7);
													throw;
												}
												FbxVectorTemplate3_003Cdouble_003E* ptr13 = global::_003CModule_003E.fbxsdk_002EFbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E_002EGet((FbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E*)((ulong)(nint)ptr10 + 136uL), &fbxVectorTemplate3_003Cdouble_003E8);
												try
												{
													WriteFBXVec3(fileStream, *ptr13);
												}
												catch
												{
													//try-fault
													global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fbxVectorTemplate3_003Cdouble_003E8);
													throw;
												}
												FbxVectorTemplate3_003Cdouble_003E* ptr14 = global::_003CModule_003E.fbxsdk_002EFbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E_002EGet((FbxPropertyT_003Cfbxsdk_003A_003AFbxVectorTemplate3_003Cdouble_003E_0020_003E*)((ulong)(nint)ptr10 + 152uL), &fbxVectorTemplate3_003Cdouble_003E9);
												try
												{
													WriteFBXVec3(fileStream, *ptr14);
												}
												catch
												{
													//try-fault
													global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fbxVectorTemplate3_003Cdouble_003E9);
													throw;
												}
												int num6 = 0;
												if (0 < global::_003CModule_003E.fbxsdk_002EFbxScene_002EGetGeometryCount((FbxScene*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E))))
												{
													do
													{
														FbxGeometry* intPtr2 = global::_003CModule_003E.fbxsdk_002EFbxScene_002EGetGeometry((FbxScene*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)), num6);
														sbyte* ptr15 = global::_003CModule_003E.fbxsdk_002EFbxObject_002EGetName((FbxObject*)ptr10);
														sbyte* ptr16 = global::_003CModule_003E.fbxsdk_002EFbxObject_002EGetName((FbxObject*)intPtr2);
														sbyte b = *ptr16;
														sbyte b2 = *ptr15;
														if (b >= b2)
														{
															long num7 = (nint)(ptr16 - (nuint)ptr15);
															while (b <= b2)
															{
																if (b == 0)
																{
																	goto end_IL_04be;
																}
																ptr15 = (sbyte*)((ulong)(nint)ptr15 + 1uL);
																b = *(sbyte*)((nint)ptr15 + num7);
																b2 = *ptr15;
																if (b < b2)
																{
																	break;
																}
															}
														}
														num6++;
														continue;
														end_IL_04be:
														break;
													}
													while (num6 < global::_003CModule_003E.fbxsdk_002EFbxScene_002EGetGeometryCount((FbxScene*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E))));
												}
												*(double*)(&fMtx) = 0.0;
												System.Runtime.CompilerServices.Unsafe.As<FbxVectorTemplate2_003Cdouble_003E, double>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fMtx, 8)) = 0.0;
												try
												{
													*(double*)(&fMtx) = 0.0;
													System.Runtime.CompilerServices.Unsafe.As<FbxVectorTemplate2_003Cdouble_003E, double>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fMtx, 8)) = 0.0;
													WriteFBXVec2(fileStream, fMtx);
													WriteU32(fileStream, 0);
													int num8 = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, int>)(*(ulong*)(*(long*)ptr11 + 232)))((nint)ptr11);
													FbxVector4* ptr17 = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, FbxStatus*, FbxVector4*>)(*(ulong*)(*(long*)ptr11 + 240)))((nint)ptr11, null);
													WriteU32(fileStream, num8);
													long num9 = num8;
													if (0 < num9)
													{
														FbxVector4* ptr18 = (FbxVector4*)((ulong)(nint)ptr17 + 16uL);
														ulong num10 = (ulong)num9;
														do
														{
															*(double*)(&fMtx2) = 0.0;
															System.Runtime.CompilerServices.Unsafe.As<FbxVectorTemplate3_003Cdouble_003E, double>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fMtx2, 8)) = 0.0;
															System.Runtime.CompilerServices.Unsafe.As<FbxVectorTemplate3_003Cdouble_003E, double>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fMtx2, 16)) = 0.0;
															try
															{
																*(double*)(&fMtx2) = *(double*)((ulong)(nint)ptr18 - 16uL);
																System.Runtime.CompilerServices.Unsafe.As<FbxVectorTemplate3_003Cdouble_003E, double>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fMtx2, 8)) = *(double*)((ulong)(nint)ptr18 - 8uL);
																System.Runtime.CompilerServices.Unsafe.As<FbxVectorTemplate3_003Cdouble_003E, double>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fMtx2, 16)) = *(double*)ptr18;
																WriteFBXVec3(fileStream, fMtx2);
																uint num11 = 8u;
																do
																{
																	WriteU32(fileStream, 0);
																	num11 += uint.MaxValue;
																}
																while (num11 != 0);
																uint num12 = 8u;
																do
																{
																	WriteU32(fileStream, 0);
																	num12 += uint.MaxValue;
																}
																while (num12 != 0);
															}
															catch
															{
																//try-fault
																global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fMtx2);
																throw;
															}
															ptr18 = (FbxVector4*)((ulong)(nint)ptr18 + 32uL);
															num10--;
														}
														while (num10 != 0);
													}
												}
												catch
												{
													//try-fault
													global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate2_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate2_003Cdouble_003E_002E_007Bdtor_007D), &fMtx);
													throw;
												}
											}
											num5++;
										}
										while (num5 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(ptr6, false));
									}
									if (num4 != num3)
									{
										result3 = new Firaxis.Error.ResultCode("Strands count mismatch!");
										break;
									}
								}
							}
							num2++;
							if (num2 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(ptr2, false))
							{
								continue;
							}
							goto end_IL_024d;
						}
						goto IL_06b7;
					}
					end_IL_024d:;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E);
					throw;
				}
				goto end_IL_024d_2;
				IL_06b7:
				if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E) != 0L)
				{
					global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)), false);
				}
				goto IL_06d7;
				end_IL_024d_2:;
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedString*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedString_002E_007Bdtor_007D), &scopedString);
				throw;
			}
			goto end_IL_024d_3;
			IL_06d7:
			if (*(long*)(&scopedString) != 0L)
			{
				IntPtr hglobal2 = new IntPtr((void*)(*(ulong*)(&scopedString)));
				Marshal.FreeHGlobal(hglobal2);
			}
			goto IL_06ff;
			end_IL_024d_3:;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
			throw;
		}
		Firaxis.Error.ResultCode success;
		try
		{
			try
			{
				try
				{
					((IDisposable)fileStream)?.Dispose();
					success = Firaxis.Error.ResultCode.Success;
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E);
					throw;
				}
				if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E) != 0L)
				{
					global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxScene_003E)), false);
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedString*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedString_002E_007Bdtor_007D), &scopedString);
				throw;
			}
			if (*(long*)(&scopedString) != 0L)
			{
				IntPtr hglobal3 = new IntPtr((void*)(*(ulong*)(&scopedString)));
				Marshal.FreeHGlobal(hglobal3);
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002E_003FA0x5a0f875a_002EScopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E_002E_007Bdtor_007D), &scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E);
			throw;
		}
		global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), false);
		return success;
		IL_005d:
		if (*(long*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E) != 0L)
		{
			global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), false);
		}
		return result;
		IL_0240:
		global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), false);
		return result2;
		IL_06ff:
		global::_003CModule_003E.fbxsdk_002EFbxObject_002EDestroy((FbxObject*)(*(ulong*)(&scopedSDKObject_003Cfbxsdk_003A_003AFbxImporter_003E)), false);
		return result3;
	}

	private Form FindFirstVisibleForm()
	{
		foreach (Form openForm in Application.OpenForms)
		{
			if (openForm != null && openForm.Visible)
			{
				return openForm;
			}
		}
		return null;
	}

	private unsafe int CountHairSystems(FbxNode* lRootNode)
	{
		int num = 0;
		int num2 = 0;
		if (0 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(lRootNode, false))
		{
			do
			{
				FbxNode* lNode = global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChild(lRootNode, num2);
				if (IsHairSystem(lNode))
				{
					num++;
				}
				num2++;
			}
			while (num2 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(lRootNode, false));
		}
		return num;
	}

	private unsafe int CountHairStrands(FbxNode* lHairSystem)
	{
		//IL_0029: Expected I, but got I8
		int num = 0;
		int num2 = 0;
		if (0 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(lHairSystem, false))
		{
			do
			{
				FbxNodeAttribute* intPtr = global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetNodeAttribute(global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChild(lHairSystem, num2));
				if (((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, FbxNodeAttribute.EType>)(*(ulong*)(*(long*)intPtr + 184)))((nint)intPtr) == (FbxNodeAttribute.EType)13)
				{
					num++;
				}
				num2++;
			}
			while (num2 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(lHairSystem, false));
		}
		return num;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe bool IsHairSystem(FbxNode* lNode)
	{
		//IL_0015: Expected I, but got I8
		FbxNodeAttribute* intPtr = global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetNodeAttribute(lNode);
		if (((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, FbxNodeAttribute.EType>)(*(ulong*)(*(long*)intPtr + 184)))((nint)intPtr) == (FbxNodeAttribute.EType)13)
		{
			return true;
		}
		int num = 0;
		if (0 < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(lNode, false))
		{
			do
			{
				if (!IsHairSystem(global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChild(lNode, num)))
				{
					num++;
					continue;
				}
				return true;
			}
			while (num < global::_003CModule_003E.fbxsdk_002EFbxNode_002EGetChildCount(lNode, false));
		}
		return false;
	}

	private unsafe void WriteFBXVec2(FileStream fStr, FbxVectorTemplate2_003Cdouble_003E fMtx)
	{
		try
		{
			byte[] array = new byte[16];
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY01M _0024ArrayType_0024_0024_0024BY01M2);
			*(float*)(&_0024ArrayType_0024_0024_0024BY01M2) = (float)(*(double*)(&fMtx));
			System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY01M, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY01M2, 4)) = (float)System.Runtime.CompilerServices.Unsafe.As<FbxVectorTemplate2_003Cdouble_003E, double>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fMtx, 8));
			IntPtr source = new IntPtr(&_0024ArrayType_0024_0024_0024BY01M2);
			Marshal.Copy(source, array, 0, 8);
			fStr.Write(array, 0, 8);
			return;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate2_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate2_003Cdouble_003E_002E_007Bdtor_007D), &fMtx);
			throw;
		}
	}

	private unsafe void WriteFBXVec3(FileStream fStr, FbxVectorTemplate3_003Cdouble_003E fMtx)
	{
		try
		{
			byte[] array = new byte[16];
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY02M _0024ArrayType_0024_0024_0024BY02M2);
			*(float*)(&_0024ArrayType_0024_0024_0024BY02M2) = (float)(*(double*)(&fMtx));
			System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY02M, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY02M2, 4)) = (float)System.Runtime.CompilerServices.Unsafe.As<FbxVectorTemplate3_003Cdouble_003E, double>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fMtx, 8));
			System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY02M, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY02M2, 8)) = (float)System.Runtime.CompilerServices.Unsafe.As<FbxVectorTemplate3_003Cdouble_003E, double>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fMtx, 16));
			IntPtr source = new IntPtr(&_0024ArrayType_0024_0024_0024BY02M2);
			Marshal.Copy(source, array, 0, 12);
			fStr.Write(array, 0, 12);
			return;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FbxVectorTemplate3_003Cdouble_003E*, void>)(&global::_003CModule_003E.fbxsdk_002EFbxVectorTemplate3_003Cdouble_003E_002E_007Bdtor_007D), &fMtx);
			throw;
		}
	}

	private void WriteU32(FileStream fStr, int iNumToWrite)
	{
		byte[] array = new byte[4]
		{
			(byte)iNumToWrite,
			(byte)(iNumToWrite / 256),
			0,
			0
		};
		byte b = (byte)(iNumToWrite / 65536);
		array[2] = b;
		array[3] = b;
		fStr.Write(array, 0, 4);
	}

	private void WriteStringNoLen(FileStream fStr, string strToWrite)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		int byteCount = uTF8Encoding.GetByteCount(strToWrite);
		fStr.Write(uTF8Encoding.GetBytes(strToWrite), 0, byteCount);
	}

	private void WriteStringU32Len(FileStream fStr, string strToWrite)
	{
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		int byteCount = uTF8Encoding.GetByteCount(strToWrite);
		WriteU32(fStr, byteCount);
		fStr.Write(uTF8Encoding.GetBytes(strToWrite), 0, byteCount);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021Autodesk_FBXInterface();
			return;
		}
		try
		{
			_0021Autodesk_FBXInterface();
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

	~Autodesk_FBXInterface()
	{
		Dispose(A_0: false);
	}
}
