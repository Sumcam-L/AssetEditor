using System;

namespace SharpDX.DirectWrite;

public class EllipsisTrimming : InlineObjectNative
{
	protected EllipsisTrimming(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public EllipsisTrimming(Factory factory, TextFormat textFormat)
		: this(IntPtr.Zero)
	{
		factory.CreateEllipsisTrimmingSign(textFormat, this);
	}
}
