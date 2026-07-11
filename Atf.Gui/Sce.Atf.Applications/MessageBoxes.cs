using System;
using System.ComponentModel.Composition;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(MessageBoxes))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class MessageBoxes : IInitializable, IPartImportsSatisfiedNotification
{
	[Import(AllowRecomposition = true)]
	private Lazy<IMessageBoxService> m_messageBoxService = null;

	private static IMessageBoxService ms_messageBoxService = null;

	void IInitializable.Initialize()
	{
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		ms_messageBoxService = m_messageBoxService.Value;
	}

	public static MessageBoxResult Show(string message, string title, MessageBoxButton buttons, MessageBoxImage image)
	{
		return (ms_messageBoxService == null) ? MessageBoxResult.Cancel : ms_messageBoxService.Show(message, title, buttons, image);
	}

	public static MessageBoxResult Show(string message)
	{
		return Show(message, "Error".Localize(), MessageBoxButton.OK, MessageBoxImage.Error);
	}
}
