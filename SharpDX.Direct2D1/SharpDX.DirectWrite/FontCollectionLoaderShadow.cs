using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

internal class FontCollectionLoaderShadow : ComObjectShadow
{
	private class FontCollectionLoaderVtbl : ComObjectVtbl
	{
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate int CreateEnumeratorFromKeyDelegate(IntPtr thisPtr, IntPtr factory, IntPtr collectionKey, int collectionKeySize, out IntPtr fontFileEnumerator);

		public FontCollectionLoaderVtbl()
			: base(1)
		{
			AddMethod(new CreateEnumeratorFromKeyDelegate(CreateEnumeratorFromKeyImpl));
		}

		private static int CreateEnumeratorFromKeyImpl(IntPtr thisPtr, IntPtr factory, IntPtr collectionKey, int collectionKeySize, out IntPtr fontFileEnumerator)
		{
			fontFileEnumerator = IntPtr.Zero;
			try
			{
				FontCollectionLoaderShadow fontCollectionLoaderShadow = CppObjectShadow.ToShadow<FontCollectionLoaderShadow>(thisPtr);
				FontCollectionLoader fontCollectionLoader = (FontCollectionLoader)fontCollectionLoaderShadow.Callback;
				FontFileEnumerator callback = fontCollectionLoader.CreateEnumeratorFromKey(fontCollectionLoaderShadow._factory, new DataPointer(collectionKey, collectionKeySize));
				fontFileEnumerator = FontFileEnumeratorShadow.ToIntPtr(callback);
			}
			catch (Exception ex)
			{
				return (int)Result.GetResultFromException(ex);
			}
			return Result.Ok.Code;
		}
	}

	private static readonly FontCollectionLoaderVtbl Vtbl = new FontCollectionLoaderVtbl();

	private Factory _factory;

	protected override CppObjectVtbl GetVtbl => Vtbl;

	public static IntPtr ToIntPtr(FontCollectionLoader fontFileEnumerator)
	{
		return CppObject.ToCallbackPtr<FontCollectionLoader>(fontFileEnumerator);
	}

	public static IntPtr ToIntPtr(Factory factory, FontCollectionLoader fontFileEnumerator)
	{
		IntPtr intPtr = ToIntPtr(fontFileEnumerator);
		FontCollectionLoaderShadow fontCollectionLoaderShadow = CppObjectShadow.ToShadow<FontCollectionLoaderShadow>(intPtr);
		fontCollectionLoaderShadow._factory = factory;
		return intPtr;
	}
}
