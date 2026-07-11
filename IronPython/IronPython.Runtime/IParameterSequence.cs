namespace IronPython.Runtime;

public interface IParameterSequence
{
	object this[int index] { get; }

	int Count { get; }

	object[] Expand(object initial);
}
