using System;
using System.Reflection.Emit;

namespace IronPython.Modules;

internal class Local : LocalOrArg
{
	private readonly LocalBuilder _local;

	public override Type Type => _local.LocalType;

	public Local(LocalBuilder local)
	{
		_local = local;
	}

	public override void Emit(ILGenerator ilgen)
	{
		ilgen.Emit(OpCodes.Ldloc, _local);
	}
}
