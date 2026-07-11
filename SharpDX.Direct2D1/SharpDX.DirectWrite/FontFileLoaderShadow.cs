using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal class FontFileLoaderShadow : ComObjectShadow
{
	private class FontFileLoaderVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int CreateStreamFromKeyDelegate(IntPtr thisPtr, IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, out IntPtr fontFileStream);

		public FontFileLoaderVtbl()
			: base(1)
		{
			AddMethod(new CreateStreamFromKeyDelegate(CreateStreamFromKeyImpl));
		}

		private static int CreateStreamFromKeyImpl(IntPtr thisPtr, IntPtr fontFileReferenceKey, int fontFileReferenceKeySize, out IntPtr fontFileStreamPtr)
		{
			fontFileStreamPtr = IntPtr.Zero;
			try
			{
				FontFileLoaderShadow fontFileLoaderShadow = CppObjectShadow.ToShadow<FontFileLoaderShadow>(thisPtr);
				FontFileLoader fontFileLoader = (FontFileLoader)fontFileLoaderShadow.Callback;
				FontFileStream callback = fontFileLoader.CreateStreamFromKey(new DataPointer(fontFileReferenceKey, fontFileReferenceKeySize));
				fontFileStreamPtr = FontFileStreamShadow.ToIntPtr(callback);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly FontFileLoaderVtbl Vtbl = new FontFileLoaderVtbl();

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(FontFileLoader callback)
	{
		return CppObject.ToCallbackPtr<FontFileLoader>(callback);
	}
}
