using System;

namespace SharpDX.DXGI;

[Flags]
public enum SwapChainFlags
{
	Nonprerotated = 1,
	AllowModeSwitch = 2,
	GdiCompatible = 4,
	None = 0
}
