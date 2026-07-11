using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetPreviewer;
using Platform;
using String;

namespace Firaxis.CivTech.AssetPreviewer._INTERNAL;

internal class DisplayWrapper : IPreviewDisplay
{
	private unsafe IScreenShotRequest* m_screenshotRequest;

	private unsafe IPreviewer* m_pPreviewer;

	private DisplayID m_DisplayID;

	public unsafe DisplayWrapper(DisplayID displayID, IPreviewer* pPreviewer)
	{
		m_pPreviewer = pPreviewer;
		m_DisplayID = displayID;
		base._002Ector();
	}

	private void _007EDisplayWrapper()
	{
	}

	private void _0021DisplayWrapper()
	{
	}

	public unsafe void Destroy()
	{
		//IL_0020: Expected I, but got I8
		IPreviewer* pPreviewer = m_pPreviewer;
		if (pPreviewer != null)
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, DisplayID, void>)(*(ulong*)(*(long*)pPreviewer + 144)))((nint)pPreviewer, m_DisplayID);
			m_DisplayID = (DisplayID)0u;
		}
	}

	public unsafe virtual void MakeActiveDisplay()
	{
		//IL_0020: Expected I, but got I8
		IPreviewer* pPreviewer = m_pPreviewer;
		if (pPreviewer != null)
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, DisplayID, void>)(*(ulong*)(*(long*)pPreviewer + 176)))((nint)pPreviewer, m_DisplayID);
		}
	}

	public unsafe virtual void CaptureScreenshot(string A_0)
	{
		//IL_008a: Expected I, but got I8
		//IL_002a: Expected I, but got I8
		//IL_0092: Expected I, but got I8
		//IL_005e: Expected I, but got I8
		//IL_00bd: Expected I, but got I8
		//IL_0079: Expected I8, but got I
		IOStringWrapper iOStringWrapper = null;
		if (!global::_003CModule_003E._003FA0xee50d08f_002E_003FbIgnoreAlways_0040_003F2_003F_003FCaptureScreenshot_0040DisplayWrapper_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && m_screenshotRequest != null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BP_0040FJGFPFDD_0040m_screenshotRequest_003F5_003F_0024DN_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040HIJOHNAA_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 101u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xee50d08f_002E_003FbIgnoreAlways_0040_003F2_003F_003FCaptureScreenshot_0040DisplayWrapper_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(A_0);
		try
		{
			iOStringWrapper = iOStringWrapper2;
			ScreenShotRequest* ptr = (ScreenShotRequest*)global::_003CModule_003E.@new(24uL);
			ScreenShotRequest* ptr2;
			try
			{
				if (ptr != null)
				{
					char* value = iOStringWrapper.Value;
					*(long*)ptr = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7ScreenShotRequest_0040_INTERNAL_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_00406B_0040);
					global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D((BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*)((ulong)(nint)ptr + 8uL), value);
					try
					{
						*(long*)((ulong)(nint)ptr + 16uL) = (nint)((IntPtr)GCHandle.Alloc(this)).ToPointer();
					}
					catch
					{
						//try-fault
						global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), (void*)((ulong)(nint)ptr + 8uL));
						throw;
					}
					ptr2 = ptr;
				}
				else
				{
					ptr2 = null;
				}
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr, 24uL);
				throw;
			}
			m_screenshotRequest = (IScreenShotRequest*)ptr2;
			IPreviewer* pPreviewer = m_pPreviewer;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, IScreenShotRequest*, void>)(*(ulong*)(*(long*)pPreviewer + 264)))((nint)pPreviewer, (IScreenShotRequest*)ptr2);
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
	}

	public unsafe virtual void BindWindow(IPreviewWindow pmWindow)
	{
		//IL_0032: Expected I, but got I8
		if (m_pPreviewer != null)
		{
			WindowWrapper windowWrapper = (WindowWrapper)pmWindow;
			IPreviewer* pPreviewer = m_pPreviewer;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, WindowID, DisplayID, void>)(*(ulong*)(*(long*)pPreviewer + 160)))((nint)pPreviewer, windowWrapper.GetWindowID(), m_DisplayID);
		}
	}

	public unsafe virtual void UnbindWindow()
	{
		//IL_0020: Expected I, but got I8
		IPreviewer* pPreviewer = m_pPreviewer;
		if (pPreviewer != null)
		{
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, DisplayID, void>)(*(ulong*)(*(long*)pPreviewer + 168)))((nint)pPreviewer, m_DisplayID);
		}
	}

	public unsafe virtual void OnResized(int width, int height)
	{
		//IL_0028: Expected I, but got I8
		DisplayID displayID = m_DisplayID;
		if (displayID != 0)
		{
			IPreviewer* pPreviewer = m_pPreviewer;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, DisplayID, uint, uint, void>)(*(ulong*)(*(long*)pPreviewer + 152)))((nint)pPreviewer, displayID, (uint)width, (uint)height);
		}
	}

	internal unsafe void ResetScreenshotRequest()
	{
		//IL_0015: Expected I, but got I8
		global::_003CModule_003E.delete(m_screenshotRequest, 8uL);
		m_screenshotRequest = null;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EDisplayWrapper();
			return;
		}
		try
		{
			_0021DisplayWrapper();
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

	~DisplayWrapper()
	{
		Dispose(A_0: false);
	}
}
