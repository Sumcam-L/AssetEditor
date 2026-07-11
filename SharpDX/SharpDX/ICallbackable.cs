using System;

namespace SharpDX;

public interface ICallbackable : IDisposable
{
	IDisposable Shadow { get; set; }
}
