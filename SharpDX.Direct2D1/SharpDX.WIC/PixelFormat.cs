using System;
using System.Collections.Generic;
using System.Globalization;

namespace SharpDX.WIC;

public sealed class PixelFormat
{
	public static readonly Guid Format32bppBGR101010;

	public static readonly Guid Format72bpp8ChannelsAlpha;

	public static readonly Guid Format2bppIndexed;

	public static readonly Guid Format32bppGrayFixedPoint;

	public static readonly Guid Format1bppIndexed;

	public static readonly Guid Format64bpp8Channels;

	public static readonly Guid Format16bppBGR555;

	public static readonly Guid Format16bppBGR565;

	public static readonly Guid Format32bppRGBA1010102XR;

	public static readonly Guid Format112bpp7Channels;

	public static readonly Guid Format128bppRGBFloat;

	public static readonly Guid Format48bppRGBFixedPoint;

	public static readonly Guid FormatDontCare;

	public static readonly Guid Format128bpp7ChannelsAlpha;

	public static readonly Guid Format48bppBGRFixedPoint;

	public static readonly Guid Format24bppRGB;

	public static readonly Guid Format24bpp3Channels;

	public static readonly Guid Format32bppBGR;

	public static readonly Guid Format48bppRGBHalf;

	public static readonly Guid Format64bpp3ChannelsAlpha;

	public static readonly Guid Format64bppBGRA;

	public static readonly Guid Format96bpp6Channels;

	public static readonly Guid FormatBlackWhite;

	public static readonly Guid Format32bppPBGRA;

	public static readonly Guid Format96bpp5ChannelsAlpha;

	public static readonly Guid Format80bppCMYKAlpha;

	public static readonly Guid Format128bpp8Channels;

	public static readonly Guid Format64bppRGBAFixedPoint;

	public static readonly Guid Format144bpp8ChannelsAlpha;

	public static readonly Guid Format112bpp6ChannelsAlpha;

	public static readonly Guid Format16bppGrayHalf;

	public static readonly Guid Format48bpp6Channels;

	public static readonly Guid Format64bpp7ChannelsAlpha;

	public static readonly Guid Format128bppRGBAFixedPoint;

	public static readonly Guid Format8bppIndexed;

	public static readonly Guid Format16bppGrayFixedPoint;

	public static readonly Guid Format48bppRGB;

	public static readonly Guid Format32bpp4Channels;

	public static readonly Guid Format32bpp3ChannelsAlpha;

	public static readonly Guid Format64bppCMYK;

	public static readonly Guid Format4bppIndexed;

	public static readonly Guid Format40bpp4ChannelsAlpha;

	public static readonly Guid Format64bppRGBFixedPoint;

	public static readonly Guid Format64bppPBGRA;

	public static readonly Guid Format16bppGray;

	public static readonly Guid Format40bppCMYKAlpha;

	public static readonly Guid Format32bppBGRA;

	public static readonly Guid Format80bpp4ChannelsAlpha;

	public static readonly Guid Format64bppRGBA;

	public static readonly Guid Format16bppBGRA5551;

	public static readonly Guid Format64bppBGRAFixedPoint;

	public static readonly Guid Format32bppPRGBA;

	public static readonly Guid Format96bppRGBFixedPoint;

	public static readonly Guid Format32bppGrayFloat;

	public static readonly Guid Format48bpp3Channels;

	public static readonly Guid Format80bpp5Channels;

	public static readonly Guid Format56bpp7Channels;

	public static readonly Guid Format128bppRGBFixedPoint;

	public static readonly Guid Format64bpp4Channels;

	public static readonly Guid Format40bpp5Channels;

	public static readonly Guid Format8bppGray;

	public static readonly Guid Format32bppCMYK;

	public static readonly Guid Format24bppBGR;

	public static readonly Guid Format32bppRGBA1010102;

	public static readonly Guid Format4bppGray;

	public static readonly Guid Format64bppPRGBA;

	public static readonly Guid Format2bppGray;

	public static readonly Guid Format64bppRGBAHalf;

	public static readonly Guid Format128bppRGBAFloat;

	public static readonly Guid Format48bpp5ChannelsAlpha;

	public static readonly Guid Format32bppRGBA;

	public static readonly Guid Format32bppRGBE;

	public static readonly Guid Format56bpp6ChannelsAlpha;

	public static readonly Guid Format128bppPRGBAFloat;

	public static readonly Guid Format48bppBGR;

	public static readonly Guid Format64bppRGBHalf;

	public static readonly Guid Format8bppAlpha;

	private static Dictionary<Guid, int> mapGuidToSize;

	public static int GetBitsPerPixel(Guid guid)
	{
		mapGuidToSize.TryGetValue(guid, out var value);
		return value;
	}

	public static int GetStride(Guid guid, int width)
	{
		int bitsPerPixel = GetBitsPerPixel(guid);
		if (bitsPerPixel == 0)
		{
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid PixelFormat guid [{0}]. Unable to calculate stride", new object[1] { guid }));
		}
		return (bitsPerPixel * width + 7) / 8;
	}

	static PixelFormat()
	{
		Format32bppBGR101010 = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc914");
		Format72bpp8ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc933");
		Format2bppIndexed = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc902");
		Format32bppGrayFixedPoint = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc93f");
		Format1bppIndexed = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc901");
		Format64bpp8Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc925");
		Format16bppBGR555 = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc909");
		Format16bppBGR565 = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90a");
		Format32bppRGBA1010102XR = new Guid("00de6b9a-c101-434b-b502-d0165ee1122c");
		Format112bpp7Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc92a");
		Format128bppRGBFloat = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91b");
		Format48bppRGBFixedPoint = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc912");
		FormatDontCare = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc900");
		Format128bpp7ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc938");
		Format48bppBGRFixedPoint = new Guid("49ca140e-cab6-493b-9ddf-60187c37532a");
		Format24bppRGB = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90d");
		Format24bpp3Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc920");
		Format32bppBGR = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90e");
		Format48bppRGBHalf = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc93b");
		Format64bpp3ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc934");
		Format64bppBGRA = new Guid("1562ff7c-d352-46f9-979e-42976b792246");
		Format96bpp6Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc929");
		FormatBlackWhite = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc905");
		Format32bppPBGRA = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc910");
		Format96bpp5ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc936");
		Format80bppCMYKAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc92d");
		Format128bpp8Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc92b");
		Format64bppRGBAFixedPoint = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91d");
		Format144bpp8ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc939");
		Format112bpp6ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc937");
		Format16bppGrayHalf = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc93e");
		Format48bpp6Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc923");
		Format64bpp7ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc932");
		Format128bppRGBAFixedPoint = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91e");
		Format8bppIndexed = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc904");
		Format16bppGrayFixedPoint = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc913");
		Format48bppRGB = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc915");
		Format32bpp4Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc921");
		Format32bpp3ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc92e");
		Format64bppCMYK = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91f");
		Format4bppIndexed = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc903");
		Format40bpp4ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc92f");
		Format64bppRGBFixedPoint = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc940");
		Format64bppPBGRA = new Guid("8c518e8e-a4ec-468b-ae70-c9a35a9c5530");
		Format16bppGray = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90b");
		Format40bppCMYKAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc92c");
		Format32bppBGRA = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90f");
		Format80bpp4ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc935");
		Format64bppRGBA = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc916");
		Format16bppBGRA5551 = new Guid("05ec7c2b-f1e6-4961-ad46-e1cc810a87d2");
		Format64bppBGRAFixedPoint = new Guid("356de33c-54d2-4a23-bb04-9b7bf9b1d42d");
		Format32bppPRGBA = new Guid("3cc4a650-a527-4d37-a916-3142c7ebedba");
		Format96bppRGBFixedPoint = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc918");
		Format32bppGrayFloat = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc911");
		Format48bpp3Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc926");
		Format80bpp5Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc928");
		Format56bpp7Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc924");
		Format128bppRGBFixedPoint = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc941");
		Format64bpp4Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc927");
		Format40bpp5Channels = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc922");
		Format8bppGray = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc908");
		Format32bppCMYK = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91c");
		Format24bppBGR = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc90c");
		Format32bppRGBA1010102 = new Guid("25238d72-fcf9-4522-b514-5578e5ad55e0");
		Format4bppGray = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc907");
		Format64bppPRGBA = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc917");
		Format2bppGray = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc906");
		Format64bppRGBAHalf = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc93a");
		Format128bppRGBAFloat = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc919");
		Format48bpp5ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc930");
		Format32bppRGBA = new Guid("f5c7ad2d-6a8d-43dd-a7a8-a29935261ae9");
		Format32bppRGBE = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc93d");
		Format56bpp6ChannelsAlpha = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc931");
		Format128bppPRGBAFloat = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc91a");
		Format48bppBGR = new Guid("e605a384-b468-46ce-bb2e-36f180e64313");
		Format64bppRGBHalf = new Guid("6fddc324-4e03-4bfe-b185-3d77768dc942");
		Format8bppAlpha = new Guid("e6cd0116-eeba-4161-aa85-27dd9fb3a895");
		mapGuidToSize = new Dictionary<Guid, int>
		{
			{ Format72bpp8ChannelsAlpha, 72 },
			{ Format48bppBGRFixedPoint, 48 },
			{ Format64bppCMYK, 64 },
			{ Format40bpp5Channels, 40 },
			{ Format4bppIndexed, 4 },
			{ Format128bppRGBAFixedPoint, 128 },
			{ Format40bpp4ChannelsAlpha, 40 },
			{ Format32bppGrayFixedPoint, 32 },
			{ Format48bpp6Channels, 48 },
			{ Format16bppGray, 16 },
			{ Format1bppIndexed, 1 },
			{ Format32bpp3ChannelsAlpha, 32 },
			{ Format64bppRGBAFixedPoint, 64 },
			{ Format80bpp4ChannelsAlpha, 80 },
			{ Format64bpp8Channels, 64 },
			{ Format16bppBGR555, 16 },
			{ Format16bppBGR565, 16 },
			{ Format32bppRGBA1010102XR, 32 },
			{ Format24bppRGB, 24 },
			{ Format128bppRGBFloat, 128 },
			{ Format32bppBGR, 32 },
			{ Format64bppRGBA, 64 },
			{ FormatDontCare, 0 },
			{ Format40bppCMYKAlpha, 40 },
			{ Format32bppPRGBA, 32 },
			{ Format24bpp3Channels, 24 },
			{ Format32bppRGBE, 32 },
			{ Format24bppBGR, 24 },
			{ Format64bppRGBFixedPoint, 64 },
			{ Format96bppRGBFixedPoint, 96 },
			{ Format128bppRGBFixedPoint, 128 },
			{ Format144bpp8ChannelsAlpha, 144 },
			{ Format64bppBGRAFixedPoint, 64 },
			{ Format32bppCMYK, 32 },
			{ Format32bppGrayFloat, 32 },
			{ Format48bpp3Channels, 48 },
			{ Format32bppBGR101010, 32 },
			{ Format2bppGray, 2 },
			{ Format56bpp7Channels, 56 },
			{ Format16bppBGRA5551, 16 },
			{ Format48bppRGBFixedPoint, 48 },
			{ Format32bppRGBA1010102, 32 },
			{ Format64bppPBGRA, 64 },
			{ Format96bpp6Channels, 96 },
			{ Format32bppPBGRA, 32 },
			{ Format64bpp4Channels, 64 },
			{ Format96bpp5ChannelsAlpha, 48 },
			{ Format48bppRGBHalf, 96 },
			{ Format48bpp5ChannelsAlpha, 48 },
			{ Format8bppGray, 8 },
			{ Format128bpp7ChannelsAlpha, 128 },
			{ Format32bppRGBA, 32 },
			{ Format80bpp5Channels, 80 },
			{ Format64bppPRGBA, 64 },
			{ Format16bppGrayFixedPoint, 16 },
			{ Format112bpp7Channels, 112 },
			{ Format128bpp8Channels, 128 },
			{ Format80bppCMYKAlpha, 80 },
			{ Format8bppAlpha, 8 },
			{ Format4bppGray, 4 },
			{ FormatBlackWhite, 0 },
			{ Format32bppBGRA, 32 },
			{ Format128bppRGBAFloat, 128 },
			{ Format112bpp6ChannelsAlpha, 112 },
			{ Format64bppRGBAHalf, 64 },
			{ Format16bppGrayHalf, 16 },
			{ Format2bppIndexed, 2 },
			{ Format64bppBGRA, 64 },
			{ Format8bppIndexed, 8 },
			{ Format56bpp6ChannelsAlpha, 56 },
			{ Format48bppBGR, 48 },
			{ Format128bppPRGBAFloat, 128 },
			{ Format64bpp7ChannelsAlpha, 64 },
			{ Format64bpp3ChannelsAlpha, 64 },
			{ Format64bppRGBHalf, 64 },
			{ Format48bppRGB, 48 },
			{ Format32bpp4Channels, 32 }
		};
	}
}
