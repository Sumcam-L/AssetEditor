using System;

namespace SharpDX.WIC;

[Flags]
public enum BitmapDecoderCapabilities
{
	SameEncoder = 1,
	CanDecodeAllImages = 2,
	CanDecodeSomeImages = 4,
	CanEnumerateMetadata = 8,
	CanDecodeThumbnail = 0x10,
	None = 0
}
