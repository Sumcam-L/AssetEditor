using System;
using System.ComponentModel.Composition;

namespace Sce.Atf;

[Export(typeof(IOutputWriter))]
[Export(typeof(ConsoleOutputWriter))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ConsoleOutputWriter : IOutputWriter
{
	public void Write(OutputMessageType type, OutputMessageVerbosity verbosity, string message)
	{
		switch (type)
		{
		case OutputMessageType.Error:
			Console.Error.Write("{0}: {1}", "Error".Localize(), message);
			break;
		case OutputMessageType.Warning:
			Console.Error.Write("{0}: {1}", "Warning".Localize(), message);
			break;
		case OutputMessageType.Diagnostic:
			Console.Error.Write("{0}: {1}", "Diagnostic".Localize(), message);
			break;
		default:
			Console.Write(message);
			break;
		}
	}

	public void Clear()
	{
	}
}
