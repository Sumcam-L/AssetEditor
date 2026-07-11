using System.Windows.Forms;

namespace Sce.Atf;

public class CustomOpenFileDialog : CustomFileDialog
{
	public bool ReadOnlyChecked
	{
		get
		{
			return GetFlag(1);
		}
		set
		{
			SetFlag(1, value);
		}
	}

	public bool ShowReadOnly
	{
		get
		{
			return !GetFlag(4);
		}
		set
		{
			SetFlag(4, !value);
		}
	}

	public bool Multiselect
	{
		get
		{
			return GetFlag(512);
		}
		set
		{
			SetFlag(512, value);
		}
	}

	public CustomOpenFileDialog()
	{
		base.CheckFileExists = true;
	}

	protected internal override DialogResult ShowNonCustomDialog(IWin32Window owner)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.ReadOnlyChecked = ReadOnlyChecked;
		openFileDialog.ShowReadOnly = ShowReadOnly;
		openFileDialog.Multiselect = Multiselect;
		return ShowNonCustomDialogInternal(openFileDialog, owner);
	}
}
