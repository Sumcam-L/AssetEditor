using System;
using Sce.Atf;

namespace Firaxis.ATF;

public struct OutputMessageData
{
	public DateTime Time { get; private set; }

	public OutputMessageType Type { get; private set; }

	public OutputMessageVerbosity Verbosity { get; private set; }

	public string Category { get; private set; }

	public string Message { get; private set; }

	public OutputMessageData(DateTime time, string cat, OutputMessageType messageType, OutputMessageVerbosity messageVerbosity, string messageText)
	{
		Time = time;
		Type = messageType;
		Verbosity = messageVerbosity;
		Category = cat;
		Message = messageText;
	}
}
