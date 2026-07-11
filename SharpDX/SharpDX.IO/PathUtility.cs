using System;
using System.Collections.Generic;

namespace SharpDX.IO;

public class PathUtility
{
	public static string GetNormalizedPath(string path)
	{
		if (path == null)
		{
			throw new ArgumentNullException("path");
		}
		path = path.Replace('/', '\\');
		string[] array = path.Split(new char[1] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
		Stack<string> stack = new Stack<string>();
		string[] array2 = array;
		foreach (string text in array2)
		{
			if (text == ".")
			{
				continue;
			}
			if (text == "..")
			{
				if (stack.Count == 0)
				{
					return null;
				}
				stack.Pop();
			}
			else
			{
				stack.Push(text);
			}
		}
		string[] array3 = stack.ToArray();
		Array.Reverse(array3);
		return Utilities.Join("\\", array3);
	}
}
