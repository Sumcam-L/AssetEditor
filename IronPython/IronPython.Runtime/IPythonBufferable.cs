using System;

namespace IronPython.Runtime;

public interface IPythonBufferable
{
	IntPtr UnsafeAddress { get; }

	int Size { get; }

	byte[] GetBytes(int offset, int length);
}
