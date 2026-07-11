using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.ATF;
using Sce.Atf.Applications;

namespace AssetEditor;

[Export(typeof(Form))]
[Export(typeof(IMainWindow))]
[Export(typeof(IMainForm))]
[Export(typeof(MainForm))]
[Export(typeof(IWin32Window))]
[Export(typeof(ISynchronizeInvoke))]
[Export(typeof(AssetEditorForm))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AssetEditorForm : MainForm, IMainForm, IMainWindow
{
	private const int WS_EX_APPWINDOW = 0x00040000;

	private const int WS_EX_TOOLWINDOW = 0x00000080;

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle |= WS_EX_APPWINDOW;
			createParams.ExStyle &= ~WS_EX_TOOLWINDOW;
			return createParams;
		}
	}

	public ISynchronizeInvoke Invoker
	{
		get
		{
			if (!base.IsDisposed)
			{
				return this;
			}
			return null;
		}
	}

	public AssetEditorForm(ToolStripContainer toolStripContainer)
		: base(toolStripContainer)
	{
	}
}
