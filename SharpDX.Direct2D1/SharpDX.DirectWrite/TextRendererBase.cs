using System;
using SharpDX.Direct2D1;

namespace SharpDX.DirectWrite;

public abstract class TextRendererBase : CallbackBase, TextRenderer, PixelSnapping, ICallbackable, IDisposable
{
	public virtual bool IsPixelSnappingDisabled(object clientDrawingContext)
	{
		return false;
	}

	public virtual Matrix3x2 GetCurrentTransform(object clientDrawingContext)
	{
		return new Matrix3x2
		{
			M11 = 1f,
			M22 = 1f
		};
	}

	public virtual float GetPixelsPerDip(object clientDrawingContext)
	{
		return 1f;
	}

	public virtual Result DrawGlyphRun(object clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, ComObject clientDrawingEffect)
	{
		return Result.NotImplemented;
	}

	public virtual Result DrawUnderline(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Underline underline, ComObject clientDrawingEffect)
	{
		return Result.NotImplemented;
	}

	public virtual Result DrawStrikethrough(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Strikethrough strikethrough, ComObject clientDrawingEffect)
	{
		return Result.NotImplemented;
	}

	public virtual Result DrawInlineObject(object clientDrawingContext, float originX, float originY, InlineObject inlineObject, bool isSideways, bool isRightToLeft, ComObject clientDrawingEffect)
	{
		return Result.NotImplemented;
	}
}
