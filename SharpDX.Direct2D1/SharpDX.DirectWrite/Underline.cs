using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.DirectWrite;

public struct Underline
{
	internal struct __Native
	{
		public float Width;

		public float Thickness;

		public float Offset;

		public float RunHeight;

		public ReadingDirection ReadingDirection;

		public FlowDirection FlowDirection;

		public IntPtr LocaleName;

		public MeasuringMode MeasuringMode;

		internal void __MarshalFree()
		{
			if (LocaleName != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(LocaleName);
			}
		}
	}

	public float Width;

	public float Thickness;

	public float Offset;

	public float RunHeight;

	public ReadingDirection ReadingDirection;

	public FlowDirection FlowDirection;

	public string LocaleName;

	public MeasuringMode MeasuringMode;

	internal void __MarshalFree(ref __Native @ref)
	{
		@ref.__MarshalFree();
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		Width = @ref.Width;
		Thickness = @ref.Thickness;
		Offset = @ref.Offset;
		RunHeight = @ref.RunHeight;
		ReadingDirection = @ref.ReadingDirection;
		FlowDirection = @ref.FlowDirection;
		LocaleName = ((@ref.LocaleName == IntPtr.Zero) ? null : Marshal.PtrToStringUni(@ref.LocaleName));
		MeasuringMode = @ref.MeasuringMode;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.Width = Width;
		@ref.Thickness = Thickness;
		@ref.Offset = Offset;
		@ref.RunHeight = RunHeight;
		@ref.ReadingDirection = ReadingDirection;
		@ref.FlowDirection = FlowDirection;
		@ref.LocaleName = ((LocaleName == null) ? IntPtr.Zero : Utilities.StringToHGlobalUni(LocaleName));
		@ref.MeasuringMode = MeasuringMode;
	}
}
