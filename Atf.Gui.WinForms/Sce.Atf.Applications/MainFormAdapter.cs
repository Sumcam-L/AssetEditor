using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[Export(typeof(IMainWindow))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class MainFormAdapter : IMainWindow
{
	private readonly Form m_mainForm;

	public string Text
	{
		get
		{
			return m_mainForm.Text;
		}
		set
		{
			m_mainForm.Text = value;
		}
	}

	public IWin32Window DialogOwner => m_mainForm;

	public event EventHandler Loading;

	public event EventHandler Loaded;

	public event CancelEventHandler Closing;

	public event EventHandler Closed;

	[ImportingConstructor]
	public MainFormAdapter(Form mainForm)
	{
		m_mainForm = mainForm;
		m_mainForm.Load += mainForm_Load;
		m_mainForm.Shown += mainForm_Shown;
		m_mainForm.FormClosing += mainForm_FormClosing;
		m_mainForm.FormClosed += mainForm_FormClosed;
	}

	public void Close()
	{
		m_mainForm.Close();
	}

	private void mainForm_Load(object sender, EventArgs e)
	{
		this.Loading.Raise(this, e);
	}

	private void mainForm_Shown(object sender, EventArgs e)
	{
		this.Loaded.Raise(this, e);
	}

	private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
	{
		e.Cancel = this.Closing.RaiseCancellable(this, e);
	}

	private void mainForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		this.Closed.Raise(this, e);
	}
}
