using System;

namespace SharpDX.DXGI;

public struct SwapChainDescription
{
	public ModeDescription ModeDescription;

	public SampleDescription SampleDescription;

	public Usage Usage;

	public int BufferCount;

	public IntPtr OutputHandle;

	public Bool IsWindowed;

	public SwapEffect SwapEffect;

	public SwapChainFlags Flags;
}
