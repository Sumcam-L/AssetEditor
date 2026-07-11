using System;
using System.Runtime.InteropServices;

namespace SharpDX.Diagnostics;

public sealed class VSGraphicsDebugger : DisposeBase
{
	public struct CaptureToken : IDisposable
	{
		private readonly VSGraphicsDebugger debugger;

		internal CaptureToken(VSGraphicsDebugger debugger)
		{
			this = default(CaptureToken);
			this.debugger = debugger;
		}

		public void Dispose()
		{
			debugger.EndCapture();
		}
	}

	public VSGraphicsDebugger(string logFileName)
	{
		VsgDbgInit(logFileName);
	}

	public void CopyLogFile(string destinationFileName)
	{
		VsgDbgCopy(destinationFileName);
	}

	public void ToggleHUD()
	{
		VsgDbgToggleHUD();
	}

	public void AddHUDMessage(string message)
	{
		VsgDbgAddHUDMessage(message);
	}

	public void CaptureCurrentFrame()
	{
		VsgDbgCaptureCurrentFrame();
	}

	public CaptureToken BeginCapture()
	{
		VsgDbgBeginCapture();
		return new CaptureToken(this);
	}

	public void EndCapture()
	{
		VsgDbgEndCapture();
	}

	protected override void Dispose(bool disposing)
	{
		VsgDbgUnInit();
	}

	[DllImport("VsGraphicsHelper.dll", CharSet = CharSet.Unicode)]
	private static extern void VsgDbgInit(string logName);

	[DllImport("VsGraphicsHelper.dll", CharSet = CharSet.Unicode)]
	private static extern void VsgDbgUnInit();

	[DllImport("VsGraphicsHelper.dll", CharSet = CharSet.Unicode)]
	private static extern void VsgDbgToggleHUD();

	[DllImport("VsGraphicsHelper.dll", CharSet = CharSet.Unicode)]
	private static extern void VsgDbgCaptureCurrentFrame();

	[DllImport("VsGraphicsHelper.dll", CharSet = CharSet.Unicode)]
	private static extern void VsgDbgBeginCapture();

	[DllImport("VsGraphicsHelper.dll", CharSet = CharSet.Unicode)]
	private static extern void VsgDbgEndCapture();

	[DllImport("VsGraphicsHelper.dll", CharSet = CharSet.Unicode)]
	private static extern void VsgDbgCopy(string newLogName);

	[DllImport("VsGraphicsHelper.dll", CharSet = CharSet.Unicode)]
	private static extern void VsgDbgAddHUDMessage(string message);
}
