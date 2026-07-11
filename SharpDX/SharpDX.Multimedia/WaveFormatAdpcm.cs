using System;
using System.Runtime.InteropServices;

namespace SharpDX.Multimedia;

public class WaveFormatAdpcm : WaveFormat
{
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	internal new struct __Native
	{
		public WaveFormat.__Native waveFormat;

		public ushort samplesPerBlock;

		public ushort numberOfCoefficients;

		public short coefficients;

		internal void __MarshalFree()
		{
			waveFormat.__MarshalFree();
		}
	}

	public ushort SamplesPerBlock { get; private set; }

	public short[] Coefficients1 { get; set; }

	public short[] Coefficients2 { get; set; }

	internal WaveFormatAdpcm()
	{
	}

	public WaveFormatAdpcm(int rate, int channels, int blockAlign = 0)
		: base(rate, 4, channels)
	{
		waveFormatTag = WaveFormatEncoding.Adpcm;
		base.blockAlign = (short)blockAlign;
		if (blockAlign == 0)
		{
			blockAlign = ((rate <= 11025) ? 256 : ((rate > 22050) ? 1024 : 512));
		}
		SamplesPerBlock = (ushort)(blockAlign * 2 / channels - 12);
		averageBytesPerSecond = base.SampleRate * blockAlign / SamplesPerBlock;
		Coefficients1 = new short[7] { 256, 512, 0, 192, 240, 460, 392 };
		Coefficients2 = new short[7] { 0, -256, 0, 64, 0, -208, -232 };
		extraSize = 32;
	}

	protected unsafe override IntPtr MarshalToPtr()
	{
		IntPtr intPtr = Marshal.AllocHGlobal(Utilities.SizeOf<WaveFormat.__Native>() + 4 + 4 * Coefficients1.Length);
		__MarshalTo(ref *(__Native*)(void*)intPtr);
		return intPtr;
	}

	internal unsafe void __MarshalFrom(ref __Native @ref)
	{
		__MarshalFrom(ref @ref.waveFormat);
		SamplesPerBlock = @ref.samplesPerBlock;
		Coefficients1 = new short[@ref.numberOfCoefficients];
		Coefficients2 = new short[@ref.numberOfCoefficients];
		if (@ref.numberOfCoefficients > 7)
		{
			throw new InvalidOperationException("Unable to read Adpcm format. Too may coefficients (max 7)");
		}
		fixed (short* coefficients = &@ref.coefficients)
		{
			for (int i = 0; i < @ref.numberOfCoefficients; i++)
			{
				Coefficients1[i] = coefficients[i * 2];
				Coefficients2[i] = coefficients[i * 2 + 1];
			}
		}
		extraSize = (short)(4 + 4 * @ref.numberOfCoefficients);
	}

	private unsafe void __MarshalTo(ref __Native @ref)
	{
		if (Coefficients1.Length > 7)
		{
			throw new InvalidOperationException("Unable to encode Adpcm format. Too may coefficients (max 7)");
		}
		extraSize = (short)(4 + 4 * Coefficients1.Length);
		__MarshalTo(ref @ref.waveFormat);
		@ref.samplesPerBlock = SamplesPerBlock;
		@ref.numberOfCoefficients = (ushort)Coefficients1.Length;
		fixed (short* coefficients = &@ref.coefficients)
		{
			for (int i = 0; i < @ref.numberOfCoefficients; i++)
			{
				coefficients[i * 2] = Coefficients1[i];
				coefficients[i * 2 + 1] = Coefficients2[i];
			}
		}
	}
}
