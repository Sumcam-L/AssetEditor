using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Shadow(typeof(PixelSnappingShadow))]
[Guid("eaf3a2da-ecf4-4d24-b644-b34f6842024b")]
public interface PixelSnapping : ICallbackable, IDisposable
{
	bool IsPixelSnappingDisabled(object clientDrawingContext);

	Matrix3x2 GetCurrentTransform(object clientDrawingContext);

	float GetPixelsPerDip(object clientDrawingContext);
}
