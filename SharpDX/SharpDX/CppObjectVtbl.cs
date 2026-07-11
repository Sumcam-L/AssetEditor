using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SharpDX;

internal class CppObjectVtbl
{
	private readonly List<Delegate> methods;

	private readonly IntPtr vtbl;

	public IntPtr Pointer => vtbl;

	public CppObjectVtbl(int numberOfCallbackMethods)
	{
		vtbl = Marshal.AllocHGlobal(IntPtr.Size * numberOfCallbackMethods);
		methods = new List<Delegate>();
	}

	public unsafe void AddMethod(Delegate method)
	{
		int count = methods.Count;
		methods.Add(method);
		((IntPtr*)(void*)vtbl)[count] = Marshal.GetFunctionPointerForDelegate(method);
	}
}
