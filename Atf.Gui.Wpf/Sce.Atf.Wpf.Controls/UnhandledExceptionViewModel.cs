using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls;

public class UnhandledExceptionViewModel : DialogViewModelBase
{
	public string Message { get; private set; }

	public UnhandledExceptionViewModel(string message)
	{
		base.Title = "An error has occurred".Localize();
		Message = message;
	}
}
