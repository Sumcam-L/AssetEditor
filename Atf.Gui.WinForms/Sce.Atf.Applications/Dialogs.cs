using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(Dialogs))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class Dialogs : IPartImportsSatisfiedNotification, IInitializable
{
	[Import]
	private IDialogService m_dialogService;

	private static IDialogService s_dialogService;

	public void Initialize()
	{
	}

	void IPartImportsSatisfiedNotification.OnImportsSatisfied()
	{
		s_dialogService = m_dialogService;
	}

	public static DialogResult ShowParentedDialog(Form form)
	{
		return s_dialogService.ShowParentedDialog(form);
	}

	public static void Configure(IDialogService dialogService)
	{
		s_dialogService = dialogService;
	}
}
