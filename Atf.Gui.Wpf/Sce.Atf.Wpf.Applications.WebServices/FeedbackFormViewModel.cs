using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications.WebServices;

internal class FeedbackFormViewModel : DialogViewModelBase
{
	public FeedbackFormViewModel()
	{
		base.Title = "Send Feedback".Localize();
	}
}
