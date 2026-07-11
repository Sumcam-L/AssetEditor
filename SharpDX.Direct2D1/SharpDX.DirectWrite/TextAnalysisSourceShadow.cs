using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal class TextAnalysisSourceShadow : ComObjectShadow
{
	protected class TextAnalysisSourceVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetTextAtPositionDelegate(IntPtr thisPtr, int textPosition, out IntPtr textString, out int textLength);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetTextBeforePositionDelegate(IntPtr thisPtr, int textPosition, out IntPtr textString, out int textLength);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate ReadingDirection GetParagraphReadingDirectionDelegate(IntPtr thisPtr);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetLocaleNameDelegate(IntPtr thisPtr, int textPosition, out int textLength, out IntPtr textString);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetNumberSubstitutionDelegate(IntPtr thisPtr, int textPosition, out int textLength, out IntPtr numberSubstitutionPtr);

		public TextAnalysisSourceVtbl(int methodCount = 0)
			: base(5 + methodCount)
		{
			AddMethod(new GetTextAtPositionDelegate(GetTextAtPositionImpl));
			AddMethod(new GetTextBeforePositionDelegate(GetTextBeforePositionImpl));
			AddMethod(new GetParagraphReadingDirectionDelegate(GetParagraphReadingDirectionImpl));
			AddMethod(new GetLocaleNameDelegate(GetLocaleNameImpl));
			AddMethod(new GetNumberSubstitutionDelegate(GetNumberSubstitutionImpl));
		}

		private static int GetTextAtPositionImpl(IntPtr thisPtr, int textPosition, out IntPtr textString, out int textLength)
		{
			textString = IntPtr.Zero;
			textLength = 0;
			try
			{
				TextAnalysisSourceShadow textAnalysisSourceShadow = CppObjectShadow.ToShadow<TextAnalysisSourceShadow>(thisPtr);
				TextAnalysisSource textAnalysisSource = (TextAnalysisSource)textAnalysisSourceShadow.Callback;
				string textAtPosition = textAnalysisSource.GetTextAtPosition(textPosition);
				if (textAtPosition != null)
				{
					textString = Marshal.StringToHGlobalUni(textAtPosition);
					textLength = textAtPosition.Length;
					textAnalysisSourceShadow.allocatedStrings.Add(textString);
				}
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int GetTextBeforePositionImpl(IntPtr thisPtr, int textPosition, out IntPtr textString, out int textLength)
		{
			textString = IntPtr.Zero;
			textLength = 0;
			try
			{
				TextAnalysisSourceShadow textAnalysisSourceShadow = CppObjectShadow.ToShadow<TextAnalysisSourceShadow>(thisPtr);
				TextAnalysisSource textAnalysisSource = (TextAnalysisSource)textAnalysisSourceShadow.Callback;
				string textBeforePosition = textAnalysisSource.GetTextBeforePosition(textPosition);
				if (textBeforePosition != null)
				{
					textString = Marshal.StringToHGlobalUni(textBeforePosition);
					textLength = textBeforePosition.Length;
					textAnalysisSourceShadow.allocatedStrings.Add(textString);
				}
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static ReadingDirection GetParagraphReadingDirectionImpl(IntPtr thisPtr)
		{
			TextAnalysisSourceShadow textAnalysisSourceShadow = CppObjectShadow.ToShadow<TextAnalysisSourceShadow>(thisPtr);
			TextAnalysisSource textAnalysisSource = (TextAnalysisSource)textAnalysisSourceShadow.Callback;
			return textAnalysisSource.ReadingDirection;
		}

		private static int GetLocaleNameImpl(IntPtr thisPtr, int textPosition, out int textLength, out IntPtr textString)
		{
			textString = IntPtr.Zero;
			textLength = 0;
			try
			{
				TextAnalysisSourceShadow textAnalysisSourceShadow = CppObjectShadow.ToShadow<TextAnalysisSourceShadow>(thisPtr);
				TextAnalysisSource textAnalysisSource = (TextAnalysisSource)textAnalysisSourceShadow.Callback;
				string localeName = textAnalysisSource.GetLocaleName(textPosition, out textLength);
				if (localeName != null)
				{
					textString = Marshal.StringToHGlobalUni(localeName);
					textAnalysisSourceShadow.allocatedStrings.Add(textString);
				}
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int GetNumberSubstitutionImpl(IntPtr thisPtr, int textPosition, out int textLength, out IntPtr numberSubstitutionPtr)
		{
			numberSubstitutionPtr = IntPtr.Zero;
			textLength = 0;
			try
			{
				TextAnalysisSourceShadow textAnalysisSourceShadow = CppObjectShadow.ToShadow<TextAnalysisSourceShadow>(thisPtr);
				TextAnalysisSource textAnalysisSource = (TextAnalysisSource)textAnalysisSourceShadow.Callback;
				numberSubstitutionPtr = textAnalysisSource.GetNumberSubstitution(textPosition, out textLength)?.NativePointer ?? IntPtr.Zero;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly TextAnalysisSourceVtbl Vtbl = new TextAnalysisSourceVtbl();

	private List<IntPtr> allocatedStrings = new List<IntPtr>();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	protected override void Dispose(bool disposing)
	{
		if (allocatedStrings != null)
		{
			foreach (IntPtr allocatedString in allocatedStrings)
			{
				Marshal.FreeHGlobal(allocatedString);
			}
			allocatedStrings = null;
		}
		base.Dispose(disposing);
	}

	public static IntPtr ToIntPtr(TextAnalysisSource callback)
	{
		return CppObject.ToCallbackPtr<TextAnalysisSource>(callback);
	}
}
