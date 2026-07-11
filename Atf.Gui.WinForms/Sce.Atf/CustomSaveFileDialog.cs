using System.Windows.Forms;

namespace Sce.Atf;

public class CustomSaveFileDialog : CustomFileDialog
{
	public bool CreatePrompt
	{
		get
		{
			return GetFlag(8192);
		}
		set
		{
			SetFlag(8192, value);
		}
	}

	public bool OverwritePrompt
	{
		get
		{
			return GetFlag(2);
		}
		set
		{
			SetFlag(2, value);
		}
	}

	public CustomSaveFileDialog()
	{
		CreatePrompt = false;
		OverwritePrompt = true;
	}

	protected internal override DialogResult ShowNonCustomDialog(IWin32Window owner)
	{
		SaveFileDialog saveFileDialog = new SaveFileDialog();
		saveFileDialog.CreatePrompt = CreatePrompt;
		saveFileDialog.OverwritePrompt = OverwritePrompt;
		return ShowNonCustomDialogInternal(saveFileDialog, owner);
	}
}
