using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

public class PropertyEditingListControl : CommandControl
{
	private enum UpdateStatus
	{
		NoUpdateNeeded,
		UpdateNeeded,
		ReloadNeeded
	}

	private UpdateStatus m_updateStatus;

	private int m_updateDepth;

	private IPropertyEditingListContext m_propEditContext;

	private IObservableContext m_observableContext;

	private IValidationContext m_validationContext;

	private ISelectionContext m_selectionContext;

	private GridControl m_gridControl;

	private IContainer components;

	public PropertyEditingListControl()
		: this(PropertyGridMode.DisableDragDropColumnHeaders, PropertyCategorySettings.ShowAll, PropertySorting.None)
	{
	}

	public PropertyEditingListControl(PropertySorting sorting)
		: this(PropertyGridMode.DisableDragDropColumnHeaders, PropertyCategorySettings.ShowAll, sorting)
	{
	}

	public PropertyEditingListControl(PropertyCategorySettings catSet, PropertySorting sorting)
		: this(PropertyGridMode.DisableDragDropColumnHeaders, catSet, sorting)
	{
	}

	public PropertyEditingListControl(PropertyGridMode gridMode, PropertyCategorySettings catSet, PropertySorting sorting)
	{
		InitializeComponent();
		base.ShowCommandText = false;
		m_gridControl = new GridControl(gridMode, catSet);
		base.ChildControls.Add(m_gridControl);
		m_gridControl.BorderStyle = BorderStyle.None;
		m_gridControl.Margin = new Padding(0, 0, 0, 0);
		m_gridControl.Dock = DockStyle.Fill;
		m_gridControl.GridView.AutoScaleColumns = true;
		m_gridControl.PropertySorting = sorting;
		RegisterGridSelectionNotification();
	}

	public PropertyEditingListControl(IEnumerable<CommandInfo> commands, ICommandClient commandClient)
		: base(commands, commandClient)
	{
	}

	public override void Bind(ICommandContext context)
	{
		IPropertyEditingListContext propertyEditingListContext = context as IPropertyEditingListContext;
		BugSubmitter.Assert(context == null || propertyEditingListContext != null, "Bad abstraction at PropertyEditingListControl.Bind has become a problem. @summary Bad abstraction is biting you in the ass. @assign bwhitman");
		if (m_observableContext != null)
		{
			m_observableContext.ItemInserted -= ObservableContext_ItemInserted;
			m_observableContext.ItemRemoved -= ObservableContext_ItemRemoved;
			m_observableContext.Reloaded -= ObservableContext_Reloaded;
		}
		if (m_validationContext != null)
		{
			m_validationContext.Beginning -= ValidationContext_Beginning;
			m_validationContext.Ended -= ValidationContext_Ended;
			m_validationContext.Cancelled -= ValidationContext_Cancelled;
		}
		UnregisterSelectionNotification();
		m_propEditContext = propertyEditingListContext;
		m_observableContext = m_propEditContext.As<IObservableContext>();
		m_selectionContext = m_propEditContext.As<ISelectionContext>();
		m_validationContext = m_propEditContext.As<IValidationContext>();
		RegisterSelectionNotification();
		if (m_observableContext != null)
		{
			m_observableContext.ItemInserted += ObservableContext_ItemInserted;
			m_observableContext.ItemRemoved += ObservableContext_ItemRemoved;
			m_observableContext.Reloaded += ObservableContext_Reloaded;
		}
		if (m_validationContext != null)
		{
			m_validationContext.Beginning += ValidationContext_Beginning;
			m_validationContext.Ended += ValidationContext_Ended;
			m_validationContext.Cancelled += ValidationContext_Cancelled;
		}
		m_gridControl.Bind(null);
		m_gridControl.Bind(m_propEditContext);
		m_gridControl.GridView.ApplyColumnBestFitAllColumns();
		if (m_propEditContext != null)
		{
			m_gridControl.GridView.SortByProperty(m_propEditContext.DefaultSortPropertyName, m_propEditContext.DefaultListSortDirection);
		}
		base.Bind(context);
	}

	private void ObservableContext_Reloaded(object sender, EventArgs e)
	{
		DoReload();
	}

	private void ObservableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		DoUpdate();
	}

	private void ObservableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		DoUpdate();
	}

	private void ValidationContext_Cancelled(object sender, EventArgs e)
	{
		EndUpdate();
	}

	private void ValidationContext_Ended(object sender, EventArgs e)
	{
		EndUpdate();
	}

	private void ValidationContext_Beginning(object sender, EventArgs e)
	{
		BeginUpdate();
	}

	private void BeginUpdate()
	{
		m_updateDepth++;
	}

	private void DoUpdate()
	{
		if (m_updateDepth > 0)
		{
			if (m_updateStatus == UpdateStatus.NoUpdateNeeded)
			{
				m_updateStatus = UpdateStatus.UpdateNeeded;
			}
		}
		else
		{
			RebindControl();
			m_updateStatus = UpdateStatus.NoUpdateNeeded;
		}
	}

	private void DoReload()
	{
		if (m_updateDepth > 0)
		{
			if (m_updateStatus == UpdateStatus.NoUpdateNeeded)
			{
				m_updateStatus = UpdateStatus.ReloadNeeded;
			}
		}
		else
		{
			RebindControl();
			m_updateStatus = UpdateStatus.NoUpdateNeeded;
		}
	}

	private void EndUpdate()
	{
		m_updateDepth--;
		if (m_updateDepth < 0)
		{
			m_updateDepth = 0;
			m_updateStatus = UpdateStatus.ReloadNeeded;
			Outputs.WriteLine(OutputMessageType.Error, OutputMessageVerbosity.ExtremelyVerbose, "Ended update while not updating in {0}!", GetType().Name);
		}
		if (m_updateDepth == 0)
		{
			switch (m_updateStatus)
			{
			case UpdateStatus.UpdateNeeded:
				DoUpdate();
				break;
			case UpdateStatus.ReloadNeeded:
				DoReload();
				break;
			}
			m_updateStatus = UpdateStatus.NoUpdateNeeded;
		}
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
		int numSelectedObjects = selectedObjs.Count();
		foreach (int selectedObj in selectedObjs)
		{
			if (selectedObj < numSelectedObjects)
			{
				yield return allObjs.ElementAt(selectedObj);
			}
		}
	}

	private void SelectionContext_SelectionChanging(object sender, EventArgs e)
	{
		UnregisterGridSelectionNotification();
		m_gridControl.GridView.ClearSelection();
		RegisterGridSelectionNotification();
	}

	private void SelectionContext_SelectionChanged(object sender, EventArgs e)
	{
		IEnumerable<object> source = m_selectionContext.Selection.Where((object whrObj) => whrObj != null && m_gridControl.GridView.SelectedObjects.Contains(whrObj));
		if (source.Any())
		{
			IEnumerable<int> enumerable = source.Select((object selObj) => m_gridControl.GridView.SelectedObjects.IndexOf(selObj));
			if (enumerable.Any())
			{
				UnregisterGridSelectionNotification();
				m_gridControl.GridView.SetSelection(enumerable);
				RegisterGridSelectionNotification();
			}
		}
	}

	private void GridView_SelectedRowsChanged(object sender, EventArgs e)
	{
		if (m_propEditContext == null || m_selectionContext == null)
		{
			return;
		}
		UnregisterSelectionNotification();
		if (m_gridControl.GridView.SelectedCount > 0)
		{
			ICollection<object> collection = new List<object>();
			foreach (int selectedIndex in m_gridControl.GridView.SelectedIndices)
			{
				if (selectedIndex < m_gridControl.GridView.SelectedObjects.Length)
				{
					collection.Add(m_gridControl.GridView.SelectedObjects[selectedIndex]);
				}
			}
			m_selectionContext.SetRange(collection);
		}
		else
		{
			m_selectionContext.Clear();
		}
		RegisterSelectionNotification();
	}

	private void UnregisterGridSelectionNotification()
	{
		m_gridControl.GridView.SelectedRowsChanged -= GridView_SelectedRowsChanged;
	}

	private void RegisterGridSelectionNotification()
	{
		UnregisterGridSelectionNotification();
		m_gridControl.GridView.SelectedRowsChanged += GridView_SelectedRowsChanged;
	}

	private void UnregisterSelectionNotification()
	{
		if (m_selectionContext != null)
		{
			m_selectionContext.SelectionChanging -= SelectionContext_SelectionChanging;
			m_selectionContext.SelectionChanged -= SelectionContext_SelectionChanged;
		}
	}

	private void RegisterSelectionNotification()
	{
		if (m_selectionContext != null)
		{
			UnregisterSelectionNotification();
			m_selectionContext.SelectionChanging += SelectionContext_SelectionChanging;
			m_selectionContext.SelectionChanged += SelectionContext_SelectionChanged;
		}
	}

	private void RebindControl()
	{
		IEnumerable<int> selectedIndices = m_gridControl.GridView.SelectedIndices;
		IEnumerable<object> second = ConvertToObjects(selectedIndices, m_gridControl.GridView.SelectedObjects);
		UnregisterSelectionNotification();
		UnregisterGridSelectionNotification();
		m_selectionContext = m_propEditContext.As<ISelectionContext>();
		m_gridControl.Bind(null);
		m_gridControl.Bind(m_propEditContext);
		m_gridControl.GridView.ApplyColumnBestFitAllColumns();
		m_gridControl.GridView.SortByProperty(m_propEditContext.DefaultSortPropertyName, m_propEditContext.DefaultListSortDirection);
		IEnumerable<object> selectedObjs = m_propEditContext.Items.Intersect(second);
		IEnumerable<int> selection = ConvertToIndices(selectedObjs, m_gridControl.GridView.EditingContext.Items);
		m_gridControl.GridView.SetSelection(selection);
		RegisterGridSelectionNotification();
		RegisterSelectionNotification();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.SystemColors.Control;
		base.Margin = new System.Windows.Forms.Padding(0);
		base.Name = "PropertyEditingListControl";
		base.Size = new System.Drawing.Size(461, 410);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
