using System;
using System.Reflection.Emit;

namespace IronPython.Modules;

internal abstract class LocalOrArg
{
	public abstract Type Type { get; }

	public abstract void Emit(ILGenerator ilgen);
}
