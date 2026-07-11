using System.Collections.Generic;
using System.ComponentModel.Composition;
using Sce.Atf.Applications;

namespace Sce.Atf.WinForms.Applications;

[Export(typeof(IMessageBoxService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class LoggedMessageBoxService : IMessageBoxService
{
	private readonly Dictionary<MessageBoxButton, MessageBoxResult> m_automatedResultMap = new Dictionary<MessageBoxButton, MessageBoxResult>
	{
		{
			MessageBoxButton.OK,
			MessageBoxResult.OK
		},
		{
			MessageBoxButton.OKCancel,
			MessageBoxResult.Cancel
		},
		{
			MessageBoxButton.YesNo,
			MessageBoxResult.No
		},
		{
			MessageBoxButton.YesNoCancel,
			MessageBoxResult.Cancel
		}
	};

	public MessageBoxResult Show(string message, string title, MessageBoxButton button, MessageBoxImage image)
	{
		MessageBoxResult result = m_automatedResultMap[button];
		OutputMessageType type = OutputMessageType.Error;
		switch (image)
		{
		case MessageBoxImage.Information:
			type = OutputMessageType.Info;
			break;
		case MessageBoxImage.Exclamation:
			type = OutputMessageType.Warning;
			break;
		}
		Outputs.WriteLine(type, $"[{title}] {message}");
		return result;
	}
}
