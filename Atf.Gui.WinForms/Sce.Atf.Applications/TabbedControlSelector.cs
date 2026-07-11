using System.ComponentModel.Composition;
using System.Windows.Forms;
using Sce.Atf.Controls;

namespace Sce.Atf.Applications;

[Export(typeof(TabbedControlSelector))]
[Export(typeof(IInitializable))]
public class TabbedControlSelector : IInitializable
{
	private readonly Form m_mainForm;

	private readonly IControlHostService m_controlHostService;

	[ImportingConstructor]
	public TabbedControlSelector(Form mainForm, IControlHostService controlHostService)
	{
		m_mainForm = mainForm;
		m_controlHostService = controlHostService;
	}

	void IInitializable.Initialize()
	{
		m_mainForm.KeyDown += OnKeyDown;
		m_mainForm.KeyPreview = true;
	}

	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Tab && e.Control)
		{
			TabbedControlSelectorDialog tabbedControlSelectorDialog = new TabbedControlSelectorDialog(m_controlHostService, !e.Shift);
			tabbedControlSelectorDialog.ShowDialog(m_mainForm);
		}
	}
}
