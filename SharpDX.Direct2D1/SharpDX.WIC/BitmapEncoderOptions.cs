using System;
using SharpDX.Win32;

namespace SharpDX.WIC;

public class BitmapEncoderOptions : PropertyBag
{
	private static readonly PropertyBagKey<float, float> ImageQualityKey = new PropertyBagKey<float, float>("ImageQuality");

	private static readonly PropertyBagKey<float, float> CompressionQualityKey = new PropertyBagKey<float, float>("CompressionQuality");

	private static readonly PropertyBagKey<bool, bool> LosslessKey = new PropertyBagKey<bool, bool>("Lossless");

	private static readonly PropertyBagKey<BitmapTransformOptions, byte> BitmapTransformKey = new PropertyBagKey<BitmapTransformOptions, byte>("BitmapTransform");

	private static readonly PropertyBagKey<bool, bool> InterlaceOptionKey = new PropertyBagKey<bool, bool>("InterlaceOption");

	private static readonly PropertyBagKey<PngFilterOption, byte> FilterOptionKey = new PropertyBagKey<PngFilterOption, byte>("FilterOption");

	private static readonly PropertyBagKey<TiffCompressionOption, bool> TiffCompressionMethodKey = new PropertyBagKey<TiffCompressionOption, bool>("TiffCompressionMethod");

	private static readonly PropertyBagKey<uint[], uint[]> LuminanceKey = new PropertyBagKey<uint[], uint[]>("Luminance");

	private static readonly PropertyBagKey<uint[], uint[]> ChrominanceKey = new PropertyBagKey<uint[], uint[]>("Chrominance");

	private static readonly PropertyBagKey<JpegYCrCbSubsamplingOption, byte> JpegYCrCbSubsamplingKey = new PropertyBagKey<JpegYCrCbSubsamplingOption, byte>("JpegYCrCbSubsampling");

	private static readonly PropertyBagKey<bool, bool> SuppressApp0Key = new PropertyBagKey<bool, bool>("SuppressApp0");

	public float ImageQuality
	{
		get
		{
			return Get(ImageQualityKey);
		}
		set
		{
			Set(ImageQualityKey, value);
		}
	}

	public float CompressionQuality
	{
		get
		{
			return Get(CompressionQualityKey);
		}
		set
		{
			Set(CompressionQualityKey, value);
		}
	}

	public bool LossLess
	{
		get
		{
			return Get(LosslessKey);
		}
		set
		{
			Set(LosslessKey, value);
		}
	}

	public BitmapTransformOptions BitmapTransform
	{
		get
		{
			return Get(BitmapTransformKey);
		}
		set
		{
			Set(BitmapTransformKey, value);
		}
	}

	public bool InterlaceOption
	{
		get
		{
			return Get(InterlaceOptionKey);
		}
		set
		{
			Set(InterlaceOptionKey, value);
		}
	}

	public PngFilterOption FilterOption
	{
		get
		{
			return Get(FilterOptionKey);
		}
		set
		{
			Set(FilterOptionKey, value);
		}
	}

	public TiffCompressionOption TiffCompressionMethod
	{
		get
		{
			return Get(TiffCompressionMethodKey);
		}
		set
		{
			Set(TiffCompressionMethodKey, value);
		}
	}

	public uint[] Luminance
	{
		get
		{
			return Get(LuminanceKey);
		}
		set
		{
			Set(LuminanceKey, value);
		}
	}

	public uint[] Chrominance
	{
		get
		{
			return Get(ChrominanceKey);
		}
		set
		{
			Set(ChrominanceKey, value);
		}
	}

	public JpegYCrCbSubsamplingOption JpegYCrCbSubsampling
	{
		get
		{
			return Get(JpegYCrCbSubsamplingKey);
		}
		set
		{
			Set(JpegYCrCbSubsamplingKey, value);
		}
	}

	public bool SuppressApp0
	{
		get
		{
			return Get(SuppressApp0Key);
		}
		set
		{
			Set(SuppressApp0Key, value);
		}
	}

	public BitmapEncoderOptions(IntPtr propertyBagPointer)
		: base(propertyBagPointer)
	{
	}
}
