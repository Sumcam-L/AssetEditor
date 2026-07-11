using System;

namespace Sce.Atf.Wpf.Models;

public class OutputItemVm : NotifyPropertyChangedBase
{
	public DateTime Time { get; private set; }

	public OutputMessageType MessageType { get; private set; }

	public string Message { get; private set; }

	public OutputItemVm(DateTime time, OutputMessageType messageType, string message)
	{
		Time = time;
		MessageType = messageType;
		Message = message.Replace(Environment.NewLine, string.Empty);
	}

	public override string ToString()
	{
		return $"{Time}: {Message}";
	}
}
