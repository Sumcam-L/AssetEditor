using System;
using System.Runtime.InteropServices;
using SharpDX.Direct2D1;

namespace SharpDX.DirectWrite;

internal class TextRendererShadow : PixelSnappingShadow
{
	private class TextRendererVtbl : PixelSnappingVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawGlyphRunDelegate(IntPtr thisObject, IntPtr clientDrawingContext, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, IntPtr glyphRunPtr, IntPtr glyphRunDescription, IntPtr clientDrawingEffect);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private unsafe delegate int DrawUnderlineDelegate(IntPtr thisObject, IntPtr clientDrawingContext, float baselineOriginX, float baselineOriginY, Underline.__Native* underline, IntPtr clientDrawingEffect);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private unsafe delegate int DrawStrikethroughDelegate(IntPtr thisObject, IntPtr clientDrawingContext, float baselineOriginX, float baselineOriginY, Strikethrough.__Native* strikethrough, IntPtr clientDrawingEffect);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawInlineObjectDelegate(IntPtr thisObject, IntPtr clientDrawingContext, float originX, float originY, IntPtr inlineObject, int isSideways, int isRightToLeft, IntPtr clientDrawingEffect);

		public unsafe TextRendererVtbl()
			: base(4)
		{
			AddMethod(new DrawGlyphRunDelegate(DrawGlyphRunImpl));
			AddMethod(new DrawUnderlineDelegate(DrawUnderlineImpl));
			AddMethod(new DrawStrikethroughDelegate(DrawStrikethroughImpl));
			AddMethod(new DrawInlineObjectDelegate(DrawInlineObjectImpl));
		}

		private static int DrawGlyphRunImpl(IntPtr thisObject, IntPtr clientDrawingContextPtr, float baselineOriginX, float baselineOriginY, MeasuringMode measuringMode, IntPtr glyphRunPtr, IntPtr glyphRunDescriptionPtr, IntPtr clientDrawingEffectPtr)
		{
			TextRendererShadow textRendererShadow = CppObjectShadow.ToShadow<TextRendererShadow>(thisObject);
			TextRenderer textRenderer = (TextRenderer)textRendererShadow.Callback;
			GlyphRun.__Native data = default(GlyphRun.__Native);
			Utilities.Read(glyphRunPtr, ref data);
			using GlyphRun glyphRun = new GlyphRun();
			glyphRun.__MarshalFrom(ref data);
			GlyphRunDescription.__Native data2 = default(GlyphRunDescription.__Native);
			Utilities.Read(glyphRunDescriptionPtr, ref data2);
			GlyphRunDescription glyphRunDescription = new GlyphRunDescription();
			glyphRunDescription.__MarshalFrom(ref data2);
			return textRenderer.DrawGlyphRun(GCHandle.FromIntPtr(clientDrawingContextPtr).Target, baselineOriginX, baselineOriginY, measuringMode, glyphRun, glyphRunDescription, (ComObject)Utilities.GetObjectForIUnknown(clientDrawingEffectPtr)).Code;
		}

		private unsafe static int DrawUnderlineImpl(IntPtr thisObject, IntPtr clientDrawingContextPtr, float baselineOriginX, float baselineOriginY, Underline.__Native* underline, IntPtr clientDrawingEffectPtr)
		{
			TextRendererShadow textRendererShadow = CppObjectShadow.ToShadow<TextRendererShadow>(thisObject);
			TextRenderer textRenderer = (TextRenderer)textRendererShadow.Callback;
			Underline underline2 = default(Underline);
			underline2.__MarshalFrom(ref *underline);
			return textRenderer.DrawUnderline(GCHandle.FromIntPtr(clientDrawingContextPtr).Target, baselineOriginX, baselineOriginY, ref underline2, (ComObject)Utilities.GetObjectForIUnknown(clientDrawingEffectPtr)).Code;
		}

		private unsafe static int DrawStrikethroughImpl(IntPtr thisObject, IntPtr clientDrawingContextPtr, float baselineOriginX, float baselineOriginY, Strikethrough.__Native* strikethrough, IntPtr clientDrawingEffectPtr)
		{
			TextRendererShadow textRendererShadow = CppObjectShadow.ToShadow<TextRendererShadow>(thisObject);
			TextRenderer textRenderer = (TextRenderer)textRendererShadow.Callback;
			Strikethrough strikethrough2 = default(Strikethrough);
			strikethrough2.__MarshalFrom(ref *strikethrough);
			return textRenderer.DrawStrikethrough(GCHandle.FromIntPtr(clientDrawingContextPtr).Target, baselineOriginX, baselineOriginY, ref strikethrough2, (ComObject)Utilities.GetObjectForIUnknown(clientDrawingEffectPtr)).Code;
		}

		private static int DrawInlineObjectImpl(IntPtr thisObject, IntPtr clientDrawingContextPtr, float originX, float originY, IntPtr inlineObject, int isSideways, int isRightToLeft, IntPtr clientDrawingEffectPtr)
		{
			TextRendererShadow textRendererShadow = CppObjectShadow.ToShadow<TextRendererShadow>(thisObject);
			TextRenderer textRenderer = (TextRenderer)textRendererShadow.Callback;
			return textRenderer.DrawInlineObject(GCHandle.FromIntPtr(clientDrawingContextPtr).Target, originX, originY, new InlineObjectNative(inlineObject), isSideways != 0, isRightToLeft != 0, (ComObject)Utilities.GetObjectForIUnknown(clientDrawingEffectPtr)).Code;
		}
	}

	private static readonly TextRendererVtbl Vtbl = new TextRendererVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(TextRenderer callback)
	{
		return CppObject.ToCallbackPtr<TextRenderer>(callback);
	}
}
