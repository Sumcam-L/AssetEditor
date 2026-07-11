using System.Reflection.Emit;

namespace IronPython.Modules;

internal abstract class MarshalCleanup
{
	public abstract void Cleanup(ILGenerator generator);
}
