using System;

namespace SharpDX.DXGI;

[Flags]
public enum Usage
{
	ShaderInput = 0x10,
	RenderTargetOutput = 0x20,
	BackBuffer = 0x40,
	Shared = 0x80,
	ReadOnly = 0x100,
	DiscardOnPresent = 0x200,
	UnorderedAccess = 0x400
}
