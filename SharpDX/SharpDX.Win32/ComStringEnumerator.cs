using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace SharpDX.Win32;

internal class ComStringEnumerator : IEnumerator<string>, IDisposable, IEnumerator, IEnumerable<string>, IEnumerable
{
	private readonly IEnumString enumString;

	private string current;

	public string Current => current;

	object IEnumerator.Current => Current;

	public ComStringEnumerator(IntPtr ptrToIEnumString)
	{
		enumString = (IEnumString)Marshal.GetTypedObjectForIUnknown(ptrToIEnumString, typeof(IEnumString));
	}

	public void Dispose()
	{
	}

	public unsafe bool MoveNext()
	{
		string[] array = new string[1];
		int num = 0;
		bool flag = enumString.Next(1, array, new IntPtr(&num)) == Result.Ok.Code;
		current = (flag ? array[0] : null);
		return flag;
	}

	public void Reset()
	{
		enumString.Reset();
	}

	public IEnumerator<string> GetEnumerator()
	{
		return this;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
