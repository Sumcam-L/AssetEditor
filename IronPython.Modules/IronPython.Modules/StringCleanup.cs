using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace IronPython.Modules;

internal class StringCleanup : MarshalCleanup
{
	private readonly LocalBuilder _local;

	public StringCleanup(LocalBuilder local)
	{
		_local = local;
	}

	public override void Cleanup(ILGenerator generator)
	{
		generator.Emit(OpCodes.Ldloc, _local);
		generator.Emit(OpCodes.Call, typeof(Marshal).GetMethod("FreeHGlobal"));
	}
}
