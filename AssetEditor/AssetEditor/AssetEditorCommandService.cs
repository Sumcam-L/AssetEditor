using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Applications;

namespace AssetEditor;

[Export(typeof(ICommandService))]
[Export(typeof(IInitializable))]
[Export(typeof(CommandService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AssetEditorCommandService : CommandService
{
	private IProjectSelectionService ProjectSelector;

	private MenuInfo SelectProject;

	[ImportingConstructor]
	public AssetEditorCommandService(Form mainForm, IProjectSelectionService pss)
		: base(mainForm)
	{
		ProjectSelector = pss;
		ProjectSelector.ProjectChanged += ProjectChanged;
		SelectProject = new MenuInfo(ProjectSelectionService.SelectProjectTag, "Project: " + pss.ActiveProject, "Change the currently active project");
		RegisterMenuInfo(SelectProject);
	}

	private void ProjectChanged(object sender, EventArgs e)
	{
		GetMenuToolStripItem(SelectProject).Text = "Project: " + ProjectSelector.ActiveProject;
	}
}
