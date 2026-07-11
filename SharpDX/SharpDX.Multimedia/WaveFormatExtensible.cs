using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SharpDX.Multimedia;

public class WaveFormatExtensible : WaveFormat
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal new struct __Native
	{
		public WaveFormat.__Native waveFormat;

		public short wValidBitsPerSample;

		public Speakers dwChannelMask;

		public Guid subFormat;

		internal void __MarshalFree()
		{
			waveFormat.__MarshalFree();
		}
	}

	private short wValidBitsPerSample;

	public Guid GuidSubFormat;

	public Speakers ChannelMask;

	internal WaveFormatExtensible()
	{
	}

	public WaveFormatExtensible(int rate, int bits, int channels)
		: base(rate, bits, channels)
	{
		waveFormatTag = WaveFormatEncoding.Extensible;
		extraSize = 22;
		wValidBitsPerSample = (short)bits;
		int num = 0;
		for (int i = 0; i < channels; i++)
		{
			num |= 1 << i;
		}
		ChannelMask = (Speakers)num;
		GuidSubFormat = ((bits == 32) ? new Guid("00000003-0000-0010-8000-00aa00389b71") : new Guid("00000001-0000-0010-8000-00aa00389b71"));
	}

	protected unsafe override IntPtr MarshalToPtr()
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Utilities.SizeOf<__Native>());
		__MarshalTo(ref *(__Native*)(void*)intPtr);
		return intPtr;
	}

	internal void __MarshalFrom(ref __Native @ref)
	{
		__MarshalFrom(ref @ref.waveFormat);
		wValidBitsPerSample = @ref.wValidBitsPerSample;
		ChannelMask = @ref.dwChannelMask;
		GuidSubFormat = @ref.subFormat;
	}

	internal void __MarshalTo(ref __Native @ref)
	{
		__MarshalTo(ref @ref.waveFormat);
		@ref.wValidBitsPerSample = wValidBitsPerSample;
		@ref.dwChannelMask = ChannelMask;
		@ref.subFormat = GuidSubFormat;
	}

	internal static __Native __NewNative()
	{
		return new __Native
		{
			waveFormat = 
			{
				extraSize = 22
			}
		};
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0} wBitsPerSample:{1} ChannelMask:{2} SubFormat:{3} extraSize:{4}", base.ToString(), wValidBitsPerSample, ChannelMask, GuidSubFormat, extraSize);
	}
}
