using System.ComponentModel.Composition;
using System.Windows.Forms;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(HelpAboutCommand))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class HelpAboutCommandWinForms : HelpAboutCommand
{
	protected override void ShowHelpAbout()
	{
		RichTextBox richTextBox = new RichTextBox();
		richTextBox.BorderStyle = BorderStyle.None;
		richTextBox.ReadOnly = true;
		richTextBox.Text = "An application built using the Authoring Tools Framework".Localize();
		string url = "https://github.com/SonyWWS/ATF/wiki";
		AboutDialog aboutDialog = new AboutDialog("About this Application".Localize(), url, richTextBox, null, null, addAtfInfo: true);
		aboutDialog.ShowDialog();
	}
}
