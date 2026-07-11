using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

public class GlyphRunDescription
{
	internal struct __Native
	{
		public IntPtr LocaleName;

		public IntPtr Text;

		public int TextLength;

		public IntPtr ClusterMap;

		public int TextPosition;

		internal void __MarshalFree()
		{
			if (LocaleName != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(LocaleName);
			}
			if (Text != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(Text);
			}
		}
	}

	public string LocaleName;

	public string Text;

	internal int TextLength;

	public IntPtr ClusterMap;

	public int TextPosition;

	internal void __MarshalFrom(ref __Native @ref)
	{
		LocaleName = ((@ref.LocaleName == IntPtr.Zero) ? null : Marshal.PtrToStringUni(@ref.LocaleName));
		Text = ((@ref.Text == IntPtr.Zero) ? null : Marshal.PtrToStringUni(@ref.Text, @ref.TextLength));
		TextLength = @ref.TextLength;
		ClusterMap = @ref.ClusterMap;
		TextPosition = @ref.TextPosition;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		@ref.LocaleName = ((LocaleName == null) ? IntPtr.Zero : Marshal.StringToHGlobalUni(LocaleName));
		@ref.Text = ((Text == null) ? IntPtr.Zero : Marshal.StringToHGlobalUni(Text));
		@ref.TextLength = ((Text != null) ? Text.Length : 0);
		@ref.ClusterMap = ClusterMap;
		@ref.TextPosition = TextPosition;
	}

	internal void __MarshalFree(ref __Native @ref)
	{
		@ref.__MarshalFree();
	}
}
