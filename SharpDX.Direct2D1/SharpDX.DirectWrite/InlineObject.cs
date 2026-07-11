using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Shadow(typeof(InlineObjectShadow))]
[Guid("8339FDE3-106F-47ab-8373-1C6295EB10B3")]
public interface InlineObject : ICallbackable, IDisposable
{
	InlineObjectMetrics Metrics { get; }

	OverhangMetrics OverhangMetrics { get; }

	void Draw(object clientDrawingContext, TextRenderer renderer, float originX, float originY, bool isSideways, bool isRightToLeft, ComObject clientDrawingEffect);

	void GetBreakConditions(out BreakCondition breakConditionBefore, out BreakCondition breakConditionAfter);
}
