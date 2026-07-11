using System.ComponentModel.Composition;

namespace Sce.Atf.Wpf.Controls;

[InheritedExport(typeof(IInitializable))]
public class DialogFactory : IInitializable
{
	private static DialogFactory s_instance = new DialogFactory();

	public void Initialize()
	{
		s_instance = this;
	}

	public static IDialogContentHost Create(IDialogContent content)
	{
		return s_instance.CreateDialog(content);
	}

	public virtual IDialogContentHost CreateDialog(IDialogContent content)
	{
		CommonDialogHost commonDialogHost = new CommonDialogHost();
		commonDialogHost.Content = content;
		content.Host = commonDialogHost;
		return commonDialogHost;
	}
}
