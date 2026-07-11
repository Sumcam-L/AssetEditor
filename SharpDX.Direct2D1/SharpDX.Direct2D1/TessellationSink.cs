using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd906c1-12e2-11dc-9fed-001143a055f9")]
[Shadow(typeof(TessellationSinkShadow))]
public interface TessellationSink : ICallbackable, IDisposable
{
	void AddTriangles(Triangle[] triangles);

	void Close();
}
