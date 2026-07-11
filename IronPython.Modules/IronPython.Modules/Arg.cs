using System;
using System.Reflection.Emit;

namespace IronPython.Modules;

internal class Arg : LocalOrArg
{
	private readonly int _index;

	private readonly Type _type;

	public override Type Type => _type;

	public Arg(int index, Type type)
	{
		_index = index;
		_type = type;
	}

	public override void Emit(ILGenerator ilgen)
	{
		ilgen.Emit(OpCodes.Ldarg, _index);
	}
}
