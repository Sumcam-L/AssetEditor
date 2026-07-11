using System;
using System.Runtime.InteropServices;

namespace SharpDX.Direct2D1;

[Guid("2cd9069f-12e2-11dc-9fed-001143a055f9")]
[Shadow(typeof(GeometrySinkShadow))]
public interface GeometrySink : SimplifiedGeometrySink, ICallbackable, IDisposable
{
	void AddLine(Vector2 point);

	void AddBezier(BezierSegment bezier);

	void AddQuadraticBezier(QuadraticBezierSegment bezier);

	void AddQuadraticBeziers(QuadraticBezierSegment[] beziers);

	void AddArc(ArcSegment arc);
}
