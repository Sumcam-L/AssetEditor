using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906c2-12e2-11dc-9fed-001143a055f9")]
public class Mesh : Resource
{
	public Mesh(IntPtr nativePtr)
		: base(nativePtr)
	{
	}

	public static explicit operator Mesh(IntPtr nativePointer)
	{
		if (!(nativePointer == IntPtr.Zero))
		{
			return new Mesh(nativePointer);
		}
		return null;
	}

	internal unsafe void Open_(out TessellationSink tessellationSink)
	{
		IntPtr zero = IntPtr.Zero;
		Result result = ((delegate* unmanaged[Stdcall]<void*, void*, int>)(*(IntPtr*)((nint)(*(IntPtr*)_nativePointer) + (nint)4 * (nint)sizeof(void*))))(_nativePointer, &zero);
		tessellationSink = ((zero == IntPtr.Zero) ? null : new TessellationSinkNative(zero));
		result.CheckError();
	}

	public Mesh(RenderTarget renderTarget)
		: base(IntPtr.Zero)
	{
		renderTarget.CreateMesh(this);
	}

	public Mesh(RenderTarget renderTarget, Triangle[] triangles)
		: this(renderTarget)
	{
		TessellationSink tessellationSink = Open();
		tessellationSink.AddTriangles(triangles);
		tessellationSink.Close();
	}

	public TessellationSink Open()
	{
		Open_(out var tessellationSink);
		return tessellationSink;
	}
}
