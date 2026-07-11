using System.Collections.Generic;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public static class CircuitElementTypes
{
	public static IEnumerable<ICircuitPin> GetAllInputPins(this ICircuitElementType type)
	{
		if (!(type is Group { AllInputPins: var allInputPins }))
		{
			return type.Inputs;
		}
		return allInputPins;
	}

	public static IEnumerable<ICircuitPin> GetAllOutputPins(this ICircuitElementType type)
	{
		if (!(type is Group { AllOutputPins: var allOutputPins }))
		{
			return type.Outputs;
		}
		return allOutputPins;
	}

	public static ICircuitPin GetInputPin(this ICircuitElementType type, int index)
	{
		if (!(type is Group obj))
		{
			return type.Inputs[index];
		}
		return obj.InputPin(index);
	}

	public static ICircuitPin GetOutputPin(this ICircuitElementType type, int index)
	{
		if (!(type is Group obj))
		{
			return type.Outputs[index];
		}
		return obj.OutputPin(index);
	}
}
