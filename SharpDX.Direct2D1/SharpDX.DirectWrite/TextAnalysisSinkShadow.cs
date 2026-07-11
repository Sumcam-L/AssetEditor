using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal class TextAnalysisSinkShadow : ComObjectShadow
{
	protected class TextAnalysisSinkVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private unsafe delegate int SetScriptAnalysisDelegate(IntPtr thisPtr, int textPosition, int textLength, ScriptAnalysis* scriptAnalysis);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetLineBreakpointsDelegate(IntPtr thisPtr, int textPosition, int textLength, IntPtr pLineBreakpoints);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetBidiLevelDelegate(IntPtr thisPtr, int textPosition, int textLength, byte explicitLevel, byte resolvedLevel);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int SetNumberSubstitutionDelegate(IntPtr thisPtr, int textPosition, int textLength, IntPtr numberSubstitution);

		public unsafe TextAnalysisSinkVtbl(int methodCount = 0)
			: base(4 + methodCount)
		{
			AddMethod(new SetScriptAnalysisDelegate(SetScriptAnalysisImpl));
			AddMethod(new SetLineBreakpointsDelegate(SetLineBreakpointsImpl));
			AddMethod(new SetBidiLevelDelegate(SetBidiLevelImpl));
			AddMethod(new SetNumberSubstitutionDelegate(SetNumberSubstitutionImpl));
		}

		private unsafe static int SetScriptAnalysisImpl(IntPtr thisPtr, int textPosition, int textLength, ScriptAnalysis* scriptAnalysis)
		{
			try
			{
				TextAnalysisSinkShadow textAnalysisSinkShadow = CppObjectShadow.ToShadow<TextAnalysisSinkShadow>(thisPtr);
				TextAnalysisSink textAnalysisSink = (TextAnalysisSink)textAnalysisSinkShadow.Callback;
				textAnalysisSink.SetScriptAnalysis(textPosition, textLength, *scriptAnalysis);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetLineBreakpointsImpl(IntPtr thisPtr, int textPosition, int textLength, IntPtr pLineBreakpoints)
		{
			try
			{
				TextAnalysisSinkShadow textAnalysisSinkShadow = CppObjectShadow.ToShadow<TextAnalysisSinkShadow>(thisPtr);
				TextAnalysisSink textAnalysisSink = (TextAnalysisSink)textAnalysisSinkShadow.Callback;
				LineBreakpoint[] array = new LineBreakpoint[textLength];
				Utilities.Read(pLineBreakpoints, array, 0, textLength);
				textAnalysisSink.SetLineBreakpoints(textPosition, textLength, array);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetBidiLevelImpl(IntPtr thisPtr, int textPosition, int textLength, byte explicitLevel, byte resolvedLevel)
		{
			try
			{
				TextAnalysisSinkShadow textAnalysisSinkShadow = CppObjectShadow.ToShadow<TextAnalysisSinkShadow>(thisPtr);
				TextAnalysisSink textAnalysisSink = (TextAnalysisSink)textAnalysisSinkShadow.Callback;
				textAnalysisSink.SetBidiLevel(textPosition, textLength, explicitLevel, resolvedLevel);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int SetNumberSubstitutionImpl(IntPtr thisPtr, int textPosition, int textLength, IntPtr numberSubstitution)
		{
			try
			{
				TextAnalysisSinkShadow textAnalysisSinkShadow = CppObjectShadow.ToShadow<TextAnalysisSinkShadow>(thisPtr);
				TextAnalysisSink textAnalysisSink = (TextAnalysisSink)textAnalysisSinkShadow.Callback;
				textAnalysisSink.SetNumberSubstitution(textPosition, textLength, new NumberSubstitution(numberSubstitution));
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly TextAnalysisSinkVtbl Vtbl = new TextAnalysisSinkVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(TextAnalysisSink callback)
	{
		return CppObject.ToCallbackPtr<TextAnalysisSink>(callback);
	}
}
