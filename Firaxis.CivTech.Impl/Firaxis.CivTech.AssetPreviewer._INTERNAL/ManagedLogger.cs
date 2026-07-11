using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetCooker;

namespace Firaxis.CivTech.AssetPreviewer._INTERNAL;

internal class ManagedLogger : IDisposable
{
	private LogEventHandler m_EventHandler;

	public ManagedLogger(LogEventHandler evt)
	{
		m_EventHandler = evt;
		base._002Ector();
	}

	private void _0021ManagedLogger()
	{
		m_EventHandler = null;
	}

	private void _007EManagedLogger()
	{
		m_EventHandler = null;
	}

	public unsafe void Post(sbyte* pContext, AssetCooker.LogLevel logLevel, sbyte* pText)
	{
		IntPtr ptr = new IntPtr(pContext);
		string context = Marshal.PtrToStringAnsi(ptr);
		IntPtr ptr2 = new IntPtr(pText);
		string text = Marshal.PtrToStringAnsi(ptr2);
		AsyncCallback callback = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002E_003FA0x1c71d42f_002EEndAsyncEvent;
		m_EventHandler.BeginInvoke(context, (LogLevel)logLevel, text, callback, null);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EManagedLogger();
			return;
		}
		try
		{
			_0021ManagedLogger();
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

	~ManagedLogger()
	{
		Dispose(A_0: false);
	}
}
