using System;

namespace Sce.Atf.Rendering.Dom;

public class ManipulatorEventArgs : EventArgs
{
	public readonly IManipulator Manipulator;

	public ManipulatorEventArgs(IManipulator manipulator)
	{
		Manipulator = manipulator;
	}
}
