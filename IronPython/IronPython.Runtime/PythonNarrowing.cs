using Microsoft.Scripting.Actions.Calls;

namespace IronPython.Runtime;

internal static class PythonNarrowing
{
	public const NarrowingLevel None = NarrowingLevel.None;

	public const NarrowingLevel BinaryOperator = NarrowingLevel.Two;

	public const NarrowingLevel IndexOperator = NarrowingLevel.Three;

	public const NarrowingLevel All = NarrowingLevel.All;
}
