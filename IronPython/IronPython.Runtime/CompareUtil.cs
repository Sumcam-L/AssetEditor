using System;
using System.Collections.Generic;
using IronPython.Runtime.Operations;

namespace IronPython.Runtime;

internal class CompareUtil
{
	[ThreadStatic]
	private static Stack<object> CmpStack;

	internal static bool Check(object o)
	{
		if (CmpStack == null)
		{
			return false;
		}
		return CmpStack.Contains(o);
	}

	internal static void Push(object o)
	{
		Stack<object> infiniteCmp = GetInfiniteCmp();
		if (infiniteCmp.Contains(o))
		{
			throw PythonOps.RuntimeError("maximum recursion depth exceeded in cmp");
		}
		CmpStack.Push(o);
	}

	internal static void Push(object o1, object o2)
	{
		Stack<object> infiniteCmp = GetInfiniteCmp();
		TwoObjects item = new TwoObjects(o1, o2);
		if (infiniteCmp.Contains(item))
		{
			throw PythonOps.RuntimeError("maximum recursion depth exceeded in cmp");
		}
		CmpStack.Push(item);
	}

	internal static void Pop(object o)
	{
		CmpStack.Pop();
	}

	internal static void Pop(object o1, object o2)
	{
		CmpStack.Pop();
	}

	private static Stack<object> GetInfiniteCmp()
	{
		if (CmpStack == null)
		{
			CmpStack = new Stack<object>();
		}
		return CmpStack;
	}
}
