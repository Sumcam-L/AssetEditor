using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

public class GridViewEditor : UserControl, IATFEditor, IDisposable
{
	private GridControl m_gridControl;

	public const string EditorName = "GridView";

	private IGridViewEditingContext GridViewContext { get; set; }

	public GridViewEditor()
	{
		m_gridControl = new GridControl(PropertyGridMode.PropertySorting | PropertyGridMode.ShowHideProperties, PropertyCategorySettings.Disabled);
		m_gridControl.Dock = DockStyle.Fill;
		m_gridControl.GridView.MultiSelectionEnabled = true;
		m_gridControl.GridView.AutoScaleColumns = true;
		base.Controls.Add(m_gridControl);
	}

	public void Bind(IATFEditingContext context)
	{
		if (GridViewContext != null)
		{
			m_gridControl.GridView.SelectedRowsChanged -= GridView_SelectedRowsChanged;
		}
		GridViewContext = context as IGridViewEditingContext;
		if (GridViewContext != null)
		{
			m_gridControl.GridView.SelectedRowsChanged += GridView_SelectedRowsChanged;
		}
		m_gridControl.Bind(GridViewContext);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && m_gridControl != null && m_gridControl != null)
		{
			base.Controls.Remove(m_gridControl);
			m_gridControl.GridView.SelectedRowsChanged -= GridView_SelectedRowsChanged;
			m_gridControl.Dispose();
			m_gridControl = null;
		}
		base.Dispose(disposing);
	}

	private IEnumerable<int> ConvertToIndices(IEnumerable<object> selectedObjs, IEnumerable<object> allObjs)
	{
		foreach (object selectedObj in selectedObjs)
		{
			yield return allObjs.IndexOf(selectedObj);
		}
	}

	private IEnumerable<object> ConvertToObjects(IEnumerable<int> selectedObjs, IEnumerable<object> allObjs)
	{
		foreach (int selectedObj in selectedObjs)
		{
			yield return allObjs.ElementAt(selectedObj);
		}
	}

	private void GridView_SelectedRowsChanged(object sender, EventArgs e)
	{
		IPropertyEditingControlOwner gridView = m_gridControl.GridView;
		if (gridView != null)
		{
			GridViewContext.SelectedIndices = ConvertToIndices(gridView.SelectedObjects, GridViewContext.Items);
		}
		else
		{
			GridViewContext.SelectedIndices = m_gridControl.GridView.SelectedIndices;
		}
	}
}
