using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal class FontFileEnumeratorShadow : ComObjectShadow
{
	private class FontFileEnumeratorVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int MoveNextDelegate(IntPtr thisPtr, out int hasCurrentFile);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int GetCurrentFontFileDelegate(IntPtr thisPtr, out IntPtr fontFile);

		public FontFileEnumeratorVtbl()
			: base(2)
		{
			AddMethod(new MoveNextDelegate(MoveNextImpl));
			AddMethod(new GetCurrentFontFileDelegate(GetCurrentFontFileImpl));
		}

		private static int MoveNextImpl(IntPtr thisPtr, out int hasCurrentFile)
		{
			hasCurrentFile = 0;
			try
			{
				FontFileEnumeratorShadow fontFileEnumeratorShadow = CppObjectShadow.ToShadow<FontFileEnumeratorShadow>(thisPtr);
				FontFileEnumerator fontFileEnumerator = (FontFileEnumerator)fontFileEnumeratorShadow.Callback;
				hasCurrentFile = (fontFileEnumerator.MoveNext() ? 1 : 0);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}

		private static int GetCurrentFontFileImpl(IntPtr thisPtr, out IntPtr fontFile)
		{
			fontFile = IntPtr.Zero;
			try
			{
				FontFileEnumeratorShadow fontFileEnumeratorShadow = CppObjectShadow.ToShadow<FontFileEnumeratorShadow>(thisPtr);
				FontFileEnumerator fontFileEnumerator = (FontFileEnumerator)fontFileEnumeratorShadow.Callback;
				fontFile = fontFileEnumerator.CurrentFontFile.NativePointer;
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly FontFileEnumeratorVtbl Vtbl = new FontFileEnumeratorVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(FontFileEnumerator callback)
	{
		return CppObject.ToCallbackPtr<FontFileEnumerator>(callback);
	}
}
