using System;

namespace SharpDX.Multimedia;

[Flags]
public enum Speakers
{
	FrontLeft = 1,
	FrontRight = 2,
	FrontCenter = 4,
	LowFrequency = 8,
	BackLeft = 0x10,
	BackRight = 0x20,
	FrontLeftOfCenter = 0x40,
	FrontRightOfCenter = 0x80,
	BackCenter = 0x100,
	SideLeft = 0x200,
	SideRight = 0x400,
	TopCenter = 0x800,
	TopFrontLeft = 0x1000,
	TopFrontCenter = 0x2000,
	TopFrontRight = 0x4000,
	TopBackLeft = 0x8000,
	TopBackCenter = 0x10000,
	TopBackRight = 0x20000,
	Reserved = 0x7FFC0000,
	All = int.MinValue,
	Mono = 4,
	Stereo = 3,
	TwoPointOne = 0xB,
	Surround = 0x107,
	Quad = 0x33,
	FourPointOne = 0x3B,
	FivePointOne = 0x3F,
	SevenPointOne = 0xFF,
	FivePointOneSurround = 0x60F,
	SevenPointOneSurround = 0x63F,
	None = 0
}
