using System;

namespace SharpDX.DXGI;

public struct OutputDescription
{
	internal struct __Native
	{
		public char DeviceName;

		private char __DeviceName1;

		private char __DeviceName2;

		private char __DeviceName3;

		private char __DeviceName4;

		private char __DeviceName5;

		private char __DeviceName6;

		private char __DeviceName7;

		private char __DeviceName8;

		private char __DeviceName9;

		private char __DeviceName10;

		private char __DeviceName11;

		private char __DeviceName12;

		private char __DeviceName13;

		private char __DeviceName14;

		private char __DeviceName15;

		private char __DeviceName16;

		private char __DeviceName17;

		private char __DeviceName18;

		private char __DeviceName19;

		private char __DeviceName20;

		private char __DeviceName21;

		private char __DeviceName22;

		private char __DeviceName23;

		private char __DeviceName24;

		private char __DeviceName25;

		private char __DeviceName26;

		private char __DeviceName27;

		private char __DeviceName28;

		private char __DeviceName29;

		private char __DeviceName30;

		private char __DeviceName31;

		public Rectangle DesktopBounds;

		public Bool IsAttachedToDesktop;

		public DisplayModeRotation Rotation;

		public IntPtr MonitorHandle;

		internal void __MarshalFree()
		{
		}
	}

	public string DeviceName;

	public Rectangle DesktopBounds;

	public Bool IsAttachedToDesktop;

	public DisplayModeRotation Rotation;

	public IntPtr MonitorHandle;

	internal void __MarshalFree(ref __Native @ref)
	{
		@ref.__MarshalFree();
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		fixed (char* deviceName = &@ref.DeviceName)
		{
			DeviceName = Utilities.PtrToStringUni((IntPtr)deviceName, 32);
		}
		DesktopBounds = @ref.DesktopBounds;
		IsAttachedToDesktop = @ref.IsAttachedToDesktop;
		Rotation = @ref.Rotation;
		MonitorHandle = @ref.MonitorHandle;
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		fixed (char* deviceName = DeviceName)
		{
			fixed (char* deviceName2 = &@ref.DeviceName)
			{
				Utilities.CopyMemory((IntPtr)deviceName2, (IntPtr)deviceName, DeviceName.Length * 2);
			}
		}
		@ref.DesktopBounds = DesktopBounds;
		@ref.IsAttachedToDesktop = IsAttachedToDesktop;
		@ref.Rotation = Rotation;
		@ref.MonitorHandle = MonitorHandle;
	}
}
