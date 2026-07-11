using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd9069b-12e2-11dc-9fed-001143a055f9")]
public class Layer : Resource
{
	public Size2F Size => GetSize();

	public Layer(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Layer(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Layer(nativePointer);
		}
		return null;
	}

	internal unsafe Size2F GetSize()
	{
		Size2F result = default(Size2F);
		((delegate* unmanaged[Stdcall]<void*, void*, void*>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &result);
		return result;
	}

	public Layer(RenderTarget renderTarget)
		: this(renderTarget, null)
	{
	}

	public Layer(RenderTarget renderTarget, Size2F? size)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateLayer(size, this);
	}
}
