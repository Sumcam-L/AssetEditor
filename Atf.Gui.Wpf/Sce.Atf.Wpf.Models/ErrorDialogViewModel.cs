namespace Sce.Atf.Wpf.Models;

internal class ErrorDialogViewModel : DialogViewModelBase
{
	public string Message { get; set; }

	public bool SuppressMessage { get; set; }

	public ErrorDialogViewModel()
	{
		base.Title = "Error".Localize();
	}
}
