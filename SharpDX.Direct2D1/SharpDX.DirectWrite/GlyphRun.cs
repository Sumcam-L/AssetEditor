using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

public class GlyphRun : IDisposable
{
	internal struct __Native
	{
		public IntPtr FontFace;

		public float FontEmSize;

		public int GlyphCount;

		public IntPtr GlyphIndices;

		public IntPtr GlyphAdvances;

		public IntPtr GlyphOffsets;

		public Bool IsSideways;

		public int BidiLevel;

		internal void __MarshalFree()
		{
			if (GlyphIndices != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(GlyphIndices);
			}
			if (GlyphAdvances != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(GlyphAdvances);
			}
			if (GlyphOffsets != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(GlyphOffsets);
			}
		}
	}

	internal IntPtr FontFacePointer;

	public float FontSize;

	internal int GlyphCount;

	internal IntPtr GlyphIndicesPointer;

	internal IntPtr GlyphAdvancesPointer;

	internal IntPtr GlyphOffsetsPointer;

	public Bool IsSideways;

	public int BidiLevel;

	public FontFace FontFace { get; set; }

	public short[] Indices { get; set; }

	public float[] Advances { get; set; }

	public GlyphOffset[] Offsets { get; set; }

	internal void __MarshalFree(ref __Native @ref)
	{
		@ref.__MarshalFree();
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		FontFace = ((@ref.FontFace == IntPtr.Zero) ? null : new FontFace(@ref.FontFace));
		if (FontFace != null)
		{
			((IUnknown)FontFace).AddReference();
		}
		FontSize = @ref.FontEmSize;
		GlyphCount = @ref.GlyphCount;
		if (@ref.GlyphIndices != IntPtr.Zero)
		{
			Indices = new short[GlyphCount];
			if (GlyphCount > 0)
			{
				Utilities.Read(@ref.GlyphIndices, Indices, 0, GlyphCount);
			}
		}
		if (@ref.GlyphAdvances != IntPtr.Zero)
		{
			Advances = new float[GlyphCount];
			if (GlyphCount > 0)
			{
				Utilities.Read(@ref.GlyphAdvances, Advances, 0, GlyphCount);
			}
		}
		if (@ref.GlyphOffsets != IntPtr.Zero)
		{
			Offsets = new GlyphOffset[GlyphCount];
			if (GlyphCount > 0)
			{
				Utilities.Read(@ref.GlyphOffsets, Offsets, 0, GlyphCount);
			}
		}
		IsSideways = @ref.IsSideways;
		BidiLevel = @ref.BidiLevel;
	}

	internal unsafe void __MarshalTo(ref __Native @ref)
	{
		@ref.FontFace = ((FontFace == null) ? IntPtr.Zero : FontFace.NativePointer);
		@ref.FontEmSize = FontSize;
		@ref.GlyphCount = -1;
		@ref.GlyphIndices = IntPtr.Zero;
		@ref.GlyphAdvances = IntPtr.Zero;
		@ref.GlyphOffsets = IntPtr.Zero;
		if (Indices != null)
		{
			@ref.GlyphCount = Indices.Length;
			@ref.GlyphIndices = Marshal.AllocHGlobal(Indices.Length * 2);
			if (Indices.Length > 0)
			{
				Utilities.Write(@ref.GlyphIndices, Indices, 0, Indices.Length);
			}
		}
		if (Advances != null)
		{
			if (@ref.GlyphCount >= 0 && @ref.GlyphCount != Advances.Length)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid length for array Advances [{0}] and Indices [{1}]. Indices, Advances and Offsets array must have same size - or may be null", new object[2] { Advances.Length, @ref.GlyphCount }));
			}
			@ref.GlyphCount = Advances.Length;
			@ref.GlyphAdvances = Marshal.AllocHGlobal(Advances.Length * 4);
			if (Advances.Length > 0)
			{
				Utilities.Write(@ref.GlyphAdvances, Advances, 0, Advances.Length);
			}
		}
		if (Offsets != null)
		{
			if (@ref.GlyphCount >= 0 && @ref.GlyphCount != Offsets.Length)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Invalid length for array Offsets [{0}]. Indices, Advances and Offsets array must have same size (Current is [{1}]- or may be null", new object[2] { Offsets.Length, @ref.GlyphCount }));
			}
			@ref.GlyphCount = Offsets.Length;
			@ref.GlyphOffsets = Marshal.AllocHGlobal(Offsets.Length * sizeof(GlyphOffset));
			if (Offsets.Length > 0)
			{
				Utilities.Write(@ref.GlyphOffsets, Offsets, 0, Offsets.Length);
			}
		}
		if (@ref.GlyphCount < 0)
		{
			@ref.GlyphCount = 0;
		}
		GlyphCount = @ref.GlyphCount;
		@ref.IsSideways = IsSideways;
		@ref.BidiLevel = BidiLevel;
	}

	public void Dispose()
	{
		if (FontFace != null)
		{
			FontFace.Dispose();
			FontFace = null;
		}
	}
}
