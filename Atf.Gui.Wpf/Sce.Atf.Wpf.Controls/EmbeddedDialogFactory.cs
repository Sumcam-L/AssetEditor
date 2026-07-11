namespace Sce.Atf.Wpf.Controls;

public class EmbeddedDialogFactory : DialogFactory
{
	public override IDialogContentHost CreateDialog(IDialogContent content)
	{
		return new EmbeddedDialogContentHost(content);
	}
}
