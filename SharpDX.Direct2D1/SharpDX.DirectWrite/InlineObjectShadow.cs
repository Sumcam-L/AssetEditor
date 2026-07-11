using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal class InlineObjectShadow : ComObjectShadow
{
	private class InlineObjectVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int DrawDelegate(IntPtr thisPtr, IntPtr clientDrawingContextPtr, IntPtr renderer, float originX, float originY, int isSideways, int isRightToLeft, IntPtr clientDrawingEffectPtr);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private unsafe delegate int GetMetricsDelegate(IntPtr thisPtr, InlineObjectMetrics* pMetrics);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private unsafe delegate int GetOverhangMetricsDelegate(IntPtr thisPtr, OverhangMetrics* pOverhangs);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetBreakConditionsDelegate(IntPtr thisPtr, out BreakCondition breakConditionBefore, out BreakCondition breakConditionAfter);

		public unsafe InlineObjectVtbl()
			: base(4)
		{
			AddMethod(new DrawDelegate(DrawImpl));
			AddMethod(new GetMetricsDelegate(GetMetricsImpl));
			AddMethod(new GetOverhangMetricsDelegate(GetOverhangMetricsImpl));
			AddMethod(new GetBreakConditionsDelegate(GetBreakConditionsImpl));
		}

		private static int DrawImpl(IntPtr thisPtr, IntPtr clientDrawingContextPtr, IntPtr renderer, float originX, float originY, int isSideways, int isRightToLeft, IntPtr clientDrawingEffectPtr)
		{
			try
			{
				InlineObjectShadow inlineObjectShadow = CppObjectShadow.ToShadow<InlineObjectShadow>(thisPtr);
				InlineObject inlineObject = (InlineObject)inlineObjectShadow.Callback;
				TextRendererShadow textRendererShadow = CppObjectShadow.ToShadow<TextRendererShadow>(renderer);
				inlineObject.Draw(GCHandle.FromIntPtr(clientDrawingContextPtr).Target, (TextRenderer)textRendererShadow.Callback, originX, originY, isSideways != 0, isRightToLeft != 0, (ComObject)Utilities.GetObjectForIUnknown(clientDrawingEffectPtr));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int GetMetricsImpl(IntPtr thisPtr, InlineObjectMetrics* pMetrics)
		{
			try
			{
				InlineObjectShadow inlineObjectShadow = CppObjectShadow.ToShadow<InlineObjectShadow>(thisPtr);
				InlineObject inlineObject = (InlineObject)inlineObjectShadow.Callback;
				*pMetrics = inlineObject.Metrics;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private unsafe static int GetOverhangMetricsImpl(IntPtr thisPtr, OverhangMetrics* pOverhangs)
		{
			try
			{
				InlineObjectShadow inlineObjectShadow = CppObjectShadow.ToShadow<InlineObjectShadow>(thisPtr);
				InlineObject inlineObject = (InlineObject)inlineObjectShadow.Callback;
				*pOverhangs = inlineObject.OverhangMetrics;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int GetBreakConditionsImpl(IntPtr thisPtr, out BreakCondition breakConditionBefore, out BreakCondition breakConditionAfter)
		{
			breakConditionBefore = BreakCondition.Neutral;
			breakConditionAfter = BreakCondition.Neutral;
			try
			{
				InlineObjectShadow inlineObjectShadow = CppObjectShadow.ToShadow<InlineObjectShadow>(thisPtr);
				InlineObject inlineObject = (InlineObject)inlineObjectShadow.Callback;
				inlineObject.GetBreakConditions(out breakConditionBefore, out breakConditionAfter);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly InlineObjectVtbl Vtbl = new InlineObjectVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(InlineObject callback)
	{
		return CppObject.ToCallbackPtr<InlineObject>(callback);
	}
}
