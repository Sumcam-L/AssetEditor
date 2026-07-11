using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("727cad4e-d6af-4c9e-8a08-d695b11caa49")]
[Shadow(typeof(FontFileLoaderShadow))]
public interface FontFileLoader : ICallbackable, IDisposable
{
	FontFileStream CreateStreamFromKey(DataPointer fontFileReferenceKey);
}
