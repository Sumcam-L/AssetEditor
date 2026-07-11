using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Shadow(typeof(FontFileEnumeratorShadow))]
[Guid("72755049-5ff7-435d-8348-4be97cfa6c7c")]
public interface FontFileEnumerator : ICallbackable, IDisposable
{
	FontFile CurrentFontFile { get; }

	bool MoveNext();
}
