using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.Utility;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(ProjectChangeMediator))]
[Export(typeof(ProjectChangeMediatorDialog))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ProjectChangeMediatorDialog : ProjectChangeMediator
{
	[Import(AllowDefault = true)]
	private Lazy<IMainWindow> MainWindow { get; set; }

	private WaitDialog WaitDialog { get; set; }

	[ImportingConstructor]
	public ProjectChangeMediatorDialog(IProjectMapService pms, IProjectSelectionService pss)
		: base(pms, pss)
	{
	}

	protected override void SetMessage(string message)
	{
		WaitDialog?.SetMessage(message);
	}

	protected override void DoProjectChange()
	{
		WaitDialog = new WaitDialog("Changing Projects...", delegate
		{
			base.DoProjectChange();
		});
		base.Invoker = WaitDialog;
		WaitDialog.FormClosed -= WaitDialog_FormClosed;
		WaitDialog.FormClosed += WaitDialog_FormClosed;
		if (MainWindow.Value != null)
		{
			WaitDialog.ShowDialog(MainWindow.Value.DialogOwner);
		}
		else
		{
			WaitDialog.ShowDialog();
		}
	}

	private void WaitDialog_FormClosed(object sender, FormClosedEventArgs e)
	{
		WaitDialog.FormClosed -= WaitDialog_FormClosed;
		base.Invoker = null;
		WaitDialog = null;
	}
}
