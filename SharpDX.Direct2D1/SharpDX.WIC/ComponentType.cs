using System;

namespace SharpDX.WIC;

[Flags]
public enum ComponentType
{
	Decoder = 1,
	Encoder = 2,
	PixelFormatConverter = 4,
	MetadataReader = 8,
	MetadataWriter = 0x10,
	PixelFormat = 0x20,
	AllComponents = 0x3F
}
