using System;
using System.Drawing;

namespace Sce.Atf;

public static class StandardIconUtil
{
	public enum IconType
	{
		Application = 32512,
		Error = 32513,
		Question = 32514,
		Exclamation = 32515,
		Warning = 32515,
		Information = 32516
	}

	public unsafe static Icon GetStandardIcon(IconType type)
	{
		IntPtr intPtr = User32.LoadIcon((IntPtr)(void*)null, (IntPtr)(int)type);
		Icon result = (Icon)Icon.FromHandle(intPtr).Clone();
		User32.DestroyIcon(intPtr);
		return result;
	}
}
