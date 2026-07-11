using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using _003CCppImplementationDetails_003E;
using AssetPreviewer;
using Firaxis.CivTech.AssetObjects;
using Platform;
using std;

namespace Firaxis.CivTech.AssetPreviewer._INTERNAL;

internal class WidgetManager : IDisposable
{
	private Control m_pmOwningControl;

	private unsafe IPreviewer* m_pPreviewer;

	private unsafe WidgetDB* m_pWidgetDB;

	public unsafe WidgetManager(Control ownerControl)
	{
		//IL_000f: Expected I, but got I8
		//IL_0027: Expected I, but got I8
		m_pmOwningControl = ownerControl;
		m_pPreviewer = null;
		WidgetDB* ptr = (WidgetDB*)global::_003CModule_003E.@new(112uL);
		WidgetDB* pWidgetDB;
		try
		{
			pWidgetDB = ((ptr == null) ? null : global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EWidgetDB_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, 112uL);
			throw;
		}
		m_pWidgetDB = pWidgetDB;
		base._002Ector();
	}

	private void _007EWidgetManager()
	{
		_0021WidgetManager();
	}

	private unsafe void _0021WidgetManager()
	{
		//IL_0025: Expected I, but got I8
		//IL_0014: Expected I, but got I8
		//IL_002b: Expected I, but got I8
		WidgetDB* pWidgetDB = m_pWidgetDB;
		if (pWidgetDB != null)
		{
			try
			{
				global::_003CModule_003E.std_002Evector_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_0020_002A_002Cstd_003A_003Aallocator_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_0020_002A_003E_0020_003E_002E_007Bdtor_007D((vector_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_0020_002A_002Cstd_003A_003Aallocator_003CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_0020_002A_003E_0020_003E*)((ulong)(nint)pWidgetDB + 72uL));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<unordered_map_003Cenum_0020AssetPreviewer_003A_003AWidgetID_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_002Cstd_003A_003Ahash_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_002Cstd_003A_003Aequal_to_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_002Cstd_003A_003Aallocator_003Cstd_003A_003Apair_003Cenum_0020AssetPreviewer_003A_003AWidgetID_0020const_0020_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_003E_0020_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Eunordered_map_003Cenum_0020AssetPreviewer_003A_003AWidgetID_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_002Cstd_003A_003Ahash_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_002Cstd_003A_003Aequal_to_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_002Cstd_003A_003Aallocator_003Cstd_003A_003Apair_003Cenum_0020AssetPreviewer_003A_003AWidgetID_0020const_0020_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_003E_0020_003E_0020_003E_002E_007Bdtor_007D), (void*)((ulong)(nint)pWidgetDB + 8uL));
				throw;
			}
			unordered_map_003Cenum_0020AssetPreviewer_003A_003AWidgetID_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_002Cstd_003A_003Ahash_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_002Cstd_003A_003Aequal_to_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_002Cstd_003A_003Aallocator_003Cstd_003A_003Apair_003Cenum_0020AssetPreviewer_003A_003AWidgetID_0020const_0020_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_003E_0020_003E_0020_003E* ptr = (unordered_map_003Cenum_0020AssetPreviewer_003A_003AWidgetID_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_002Cstd_003A_003Ahash_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_002Cstd_003A_003Aequal_to_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_002Cstd_003A_003Aallocator_003Cstd_003A_003Apair_003Cenum_0020AssetPreviewer_003A_003AWidgetID_0020const_0020_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_003E_0020_003E_0020_003E*)((ulong)(nint)pWidgetDB + 8uL);
			global::_003CModule_003E.std_002E_Hash_003Cstd_003A_003A_Umap_traits_003Cenum_0020AssetPreviewer_003A_003AWidgetID_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_002Cstd_003A_003A_Uhash_compare_003Cenum_0020AssetPreviewer_003A_003AWidgetID_002Cstd_003A_003Ahash_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_002Cstd_003A_003Aequal_to_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_0020_003E_002Cstd_003A_003Aallocator_003Cstd_003A_003Apair_003Cenum_0020AssetPreviewer_003A_003AWidgetID_0020const_0020_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_003E_0020_003E_002C0_003E_0020_003E_002E_007Bdtor_007D((_Hash_003Cstd_003A_003A_Umap_traits_003Cenum_0020AssetPreviewer_003A_003AWidgetID_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_002Cstd_003A_003A_Uhash_compare_003Cenum_0020AssetPreviewer_003A_003AWidgetID_002Cstd_003A_003Ahash_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_002Cstd_003A_003Aequal_to_003Cenum_0020AssetPreviewer_003A_003AWidgetID_003E_0020_003E_002Cstd_003A_003Aallocator_003Cstd_003A_003Apair_003Cenum_0020AssetPreviewer_003A_003AWidgetID_0020const_0020_002CFiraxis_003A_003ACivTech_003A_003AAssetPreviewer_003A_003A_INTERNAL_003A_003AWidgetDB_003A_003AWidgetInfo_003E_0020_003E_002C0_003E_0020_003E*)ptr);
			global::_003CModule_003E.delete(pWidgetDB, 112uL);
		}
	}

	public unsafe Widget CreateWidget(WindowID nWindowID, string WidgetType, IValueSet arguments, object BoundObject)
	{
		//IL_003d: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = new StandardStringWrapper(arguments.SerializeIntoXML());
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(WidgetType);
		sbyte* value = standardStringWrapper.Value;
		sbyte* value2 = standardStringWrapper2.Value;
		IPreviewer* pPreviewer = m_pPreviewer;
		WidgetID widgetID = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, sbyte*, sbyte*, WidgetID>)(*(ulong*)(*(long*)pPreviewer + 232)))((nint)pPreviewer, nWindowID, value2, value);
		if (widgetID == (WidgetID)0u)
		{
			return null;
		}
		Widget widget = new Widget(this, widgetID, BoundObject, WidgetType);
		global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EWidgetDB_002EStoreWidget(m_pWidgetDB, widget, widgetID, nWindowID);
		return widget;
	}

	public unsafe void DestroyWindowWidgets(WindowID nWindowID)
	{
		global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EWidgetDB_002EDestroyWindowWidgets(m_pWidgetDB, nWindowID);
	}

	public unsafe void DestroyWidget(WidgetID nWidgetID)
	{
		global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EWidgetDB_002EDestroyWidget(m_pWidgetDB, nWidgetID);
	}

	public unsafe void SetWidgetVisibility(WidgetID nWidgetID, [MarshalAs(UnmanagedType.U1)] bool b)
	{
		//IL_0019: Expected I, but got I8
		IPreviewer* pPreviewer = m_pPreviewer;
		((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WidgetID, byte, void>)(*(ulong*)(*(long*)pPreviewer + 248)))((nint)pPreviewer, nWidgetID, b ? ((byte)1) : ((byte)0));
	}

	public unsafe void AlterWidget(WidgetID nWidgetID, IValueSet arguments)
	{
		//IL_002a: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = new StandardStringWrapper(arguments.SerializeIntoXML());
		IPreviewer* pPreviewer = m_pPreviewer;
		((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WidgetID, sbyte*, void>)(*(ulong*)(*(long*)pPreviewer + 256)))((nint)pPreviewer, nWidgetID, standardStringWrapper.Value);
	}

	public unsafe void SendNotificationPacketToUIThread(IWidgetNotificationPacket* pPacket)
	{
		//IL_00dc: Expected I, but got I8
		//IL_001d: Expected I, but got I8
		if (pPacket == null)
		{
			return;
		}
		WidgetDB* pWidgetDB = m_pWidgetDB;
		if (pWidgetDB != null)
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, IWidgetNotificationProcessor*, void>)(*(ulong*)(*(ulong*)pPacket)))((nint)pPacket, (IWidgetNotificationProcessor*)pWidgetDB);
			if (!global::_003CModule_003E._003FA0xdd2c60f2_002E_003FbIgnoreAlways_0040_003F8_003F_003FSendNotificationPacketToUIThread_0040WidgetManager_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVIWidgetNotificationPacket_00404_0040_0040Z_00404_NA && !m_pmOwningControl.IsHandleCreated)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DO_0040NLLDFCIH_0040Sending_003F5widget_003F5packet_003F5to_003F5handle_003F9_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CD_0040LAKDIMGG_0040m_pmOwningControl_003F9_003F_0024DOIsHandleCreat_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GK_0040PDIDAFAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 285u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xdd2c60f2_002E_003FbIgnoreAlways_0040_003F8_003F_003FSendNotificationPacketToUIThread_0040WidgetManager_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVIWidgetNotificationPacket_00404_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			if (!global::_003CModule_003E._003FA0xdd2c60f2_002E_003FbIgnoreAlways_0040_003FBA_0040_003F_003FSendNotificationPacketToUIThread_0040WidgetManager_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVIWidgetNotificationPacket_00404_0040_0040Z_00404_NA && m_pmOwningControl.IsDisposed)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DN_0040DBGPAGIO_0040Sending_003F5widget_003F5packet_003F5to_003F5disposi_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BP_0040GACCOOMC_0040_003F_0024CBm_pmOwningControl_003F9_003F_0024DOIsDisposed_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GK_0040PDIDAFAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 287u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xdd2c60f2_002E_003FbIgnoreAlways_0040_003FBA_0040_003F_003FSendNotificationPacketToUIThread_0040WidgetManager_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVIWidgetNotificationPacket_00404_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
			}
			if (m_pmOwningControl.IsHandleCreated)
			{
				Delegate method = new WidgetNotificationHandler(ProcessNotificationPacket);
				m_pmOwningControl.BeginInvoke(method);
			}
		}
		((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, void>)(*(ulong*)(*(long*)pPacket + 8)))((nint)pPacket);
	}

	public unsafe void ProcessNotificationPacket()
	{
		//IL_002d: Expected I, but got I8
		Exception ex = null;
		if (!global::_003CModule_003E._003FA0xdd2c60f2_002E_003FbIgnoreAlways_0040_003F2_003F_003FProcessNotificationPacket_0040WidgetManager_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXXZ_00404_NA && m_pWidgetDB == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040JMBEPPFK_0040m_pWidgetDB_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GK_0040PDIDAFAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 303u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xdd2c60f2_002E_003FbIgnoreAlways_0040_003F2_003F_003FProcessNotificationPacket_0040WidgetManager_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (m_pWidgetDB != null)
		{
			try
			{
				global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EWidgetDB_002EDispatchUpdateEvents(m_pWidgetDB);
			}
			catch (Exception ex2)
			{
				throw ex2;
			}
		}
	}

	public unsafe void SetPreviewer(IPreviewer* pPreviewer)
	{
		//IL_001f: Expected I, but got I8
		//IL_0027: Expected I8, but got I
		//IL_0041: Expected I, but got I8
		//IL_0041: Expected I, but got I8
		m_pPreviewer = pPreviewer;
		WidgetDB* pWidgetDB = m_pWidgetDB;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ScopedLock scopedLock);
		global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EScopedLock_002E_007Bctor_007D(&scopedLock, (OwnableLock*)((ulong)(nint)pWidgetDB + 104uL), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EE_0040JAFAKHKA_0040Firaxis_003F3_003F3CivTech_003F3_003F3AssetPreviewer_0040));
		try
		{
			*(long*)((ulong)(nint)pWidgetDB + 96uL) = (nint)pPreviewer;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ScopedLock*, void>)(&global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EScopedLock_002E_007Bdtor_007D), &scopedLock);
			throw;
		}
		global::_003CModule_003E.std_002Eatomic_003Cchar_0020const_0020_002A_003E_002E_003D((atomic_003Cchar_0020const_0020_002A_003E*)(*(ulong*)(&scopedLock)), null);
	}

	public unsafe Widget FindWidget(WidgetID nID)
	{
		return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EWidgetDB_002EFindWidget(m_pWidgetDB, nID);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021WidgetManager();
			return;
		}
		try
		{
			_0021WidgetManager();
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

	~WidgetManager()
	{
		Dispose(A_0: false);
	}
}
