using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Shadow(typeof(FontCollectionLoaderShadow))]
[Guid("cca920e4-52f0-492b-bfa8-29c72ee0a468")]
public interface FontCollectionLoader : ICallbackable, IDisposable
{
	FontFileEnumerator CreateEnumeratorFromKey(Factory factory, DataPointer collectionKey);
}
