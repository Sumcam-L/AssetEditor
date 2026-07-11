using System.Windows.Forms;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class TriggerListControl : UserControl
{
	private GridControl m_triggerListView;

	public TriggerListControl()
	{
		InitializeControls();
	}

	private void InitializeControls()
	{
		SuspendLayout();
		m_triggerListView = new GridControl(PropertyGridMode.ShowHideProperties | PropertyGridMode.DisableDragDropColumnHeaders);
		m_triggerListView.PropertySorting = PropertySorting.Categorized;
		m_triggerListView.Dock = DockStyle.Fill;
		m_triggerListView.GridView.AutoScaleColumns = true;
		base.Controls.Add(m_triggerListView);
		ResumeLayout(performLayout: false);
	}

	public void Bind(IBehaviorProviderAdapter context)
	{
		TriggerListFilteringContext context2 = null;
		if (context != null)
		{
			context2 = new TriggerListFilteringContext(context);
		}
		m_triggerListView.Bind(context2);
	}
}
