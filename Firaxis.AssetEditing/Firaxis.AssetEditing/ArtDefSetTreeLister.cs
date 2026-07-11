using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(ArtDefSetTreeLister))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ArtDefSetTreeLister : IDisposable
{
	private ISelectionContext m_artDefContext;

	private IIndexSelectionContext m_currentEditingContext;

	private Control m_editControl;

	private Sce.Atf.Controls.PropertyEditing.PropertyGrid m_propertyControl;

	private FilteredTreeControlEditor m_treeControlEditor;

	private readonly IDictionary<string, IATFEditor> m_customEditors = new Dictionary<string, IATFEditor>();

	private readonly IPropertyEditingContext m_selectionPropEditContext;

	public ArtDefSetControl MainControl { get; internal set; }

	public ArtDefSetTreeLister(ICommandService commandService, IDocument document, ArtDefCommands artDefCommands, IFileDialogService fileDialog, ICivTechService civTechSvc)
	{
		m_treeControlEditor = new FilteredTreeControlEditor(commandService, artDefCommands);
		m_treeControlEditor.TreeControlAdapter.AutoExpand = false;
		m_treeControlEditor.TreeControl.NavigationKeyBehavior = TreeControl.KeyboardShortcuts.WindowsExplorer;
		m_propertyControl = new Sce.Atf.Controls.PropertyEditing.PropertyGrid();
		m_propertyControl.PropertySorting = PropertySorting.Categorized;
		MainControl = new ArtDefSetControl(fileDialog, civTechSvc);
		MainControl.SuspendLayout();
		MainControl.PanelTreeArea.Controls.Add(m_treeControlEditor.Control);
		m_treeControlEditor.Control.Dock = DockStyle.Fill;
		m_treeControlEditor.TreeControl.ShowRoot = false;
		m_treeControlEditor.TreeControl.AllowDrop = true;
		m_treeControlEditor.TreeControl.SelectionMode = SelectionMode.MultiExtended;
		m_treeControlEditor.TreeControl.NodeSelectionFiltered += TreeControl_NodeSelectionFiltered;
		MainControl.ResumeLayout();
		MainControl.VisibleChanged += MainControl_VisibleChanged;
		SetEditorControl(m_propertyControl);
		ArtDefContext artDefContext = document.As<ArtDefContext>();
		m_selectionPropEditContext = new MultiSelectPropertyEditingContext(artDefContext, artDefContext.Selection);
		m_propertyControl.Bind(m_selectionPropEditContext);
		artDefContext.SelectionChanged += ArtDefContext_SelectionChanged;
		m_artDefContext = artDefContext;
		m_treeControlEditor.TreeView = new FilteredTreeView(document.As<ArtDefSetTreeView>(), m_treeControlEditor.DefaultFilter);
	}

	private void TreeControl_NodeSelectionFiltered(object sender, TreeControl.NodeEventArgs e)
	{
		TreeControl treeControl = sender as TreeControl;
		TreeControl.Node node = e.Node;
		if (treeControl == null || treeControl.FirstSelectedNode == null)
		{
			return;
		}
		IEnumerable<TreeControl.Node> selectedNodes = treeControl.SelectedNodes;
		if (selectedNodes == null || selectedNodes.Count() != 0)
		{
			Path<object> path = MakePath(node);
			Path<object> path2 = MakePath(treeControl.SelectedNodes.Last());
			if (path.Count != path2.Count)
			{
				e.Node.AllowSelect = false;
			}
			else if (treeControl.SelectedNodes.Last().Parent != node.Parent)
			{
				e.Node.AllowSelect = false;
			}
			else
			{
				e.Node.AllowSelect = true;
			}
		}
	}

	private Path<object> MakePath(TreeControl.Node node)
	{
		List<object> list = new List<object>();
		while (node != null)
		{
			list.Add(node.Tag);
			node = node.Parent;
		}
		list.Reverse();
		return new AdaptablePath<object>(list);
	}

	private void MainControl_VisibleChanged(object sender, EventArgs e)
	{
		if (MainControl.Visible)
		{
			SkinService.ApplyActiveSkin(MainControl);
		}
	}

	public void Dispose()
	{
		Dispose(bDisposing: true);
		GC.SuppressFinalize(this);
	}

	public IEnumerable<object> GetCurrentSubselection()
	{
		if (m_currentEditingContext != null)
		{
			return m_currentEditingContext.SelectedObjects;
		}
		return Enumerable.Empty<object>();
	}

	protected virtual void Dispose(bool bDisposing)
	{
		if (!bDisposing)
		{
			return;
		}
		if (m_artDefContext != null)
		{
			m_artDefContext.SelectionChanged -= ArtDefContext_SelectionChanged;
			m_artDefContext = null;
		}
		m_editControl = null;
		m_treeControlEditor = null;
		if (m_propertyControl != null)
		{
			m_propertyControl.Bind(null);
			m_propertyControl.Dispose();
			m_propertyControl = null;
		}
		foreach (IATFEditor value in m_customEditors.Values)
		{
			value.Bind(null);
			value.Dispose();
		}
		m_customEditors.Clear();
		if (MainControl != null)
		{
			MainControl.Dispose();
			MainControl = null;
		}
	}

	private void ArtDefContext_SelectionChanged(object sender, EventArgs e)
	{
		ArtDefContext artDefContext = sender as ArtDefContext;
		DomNode selectedDomNode = GetSelectedDomNode(artDefContext);
		m_currentEditingContext = null;
		if (selectedDomNode != null)
		{
			BindEditor(selectedDomNode);
			return;
		}
		m_propertyControl.Bind(null);
		SetEditorControl(m_propertyControl);
	}

	private void BindEditor(DomNode node)
	{
		IATFEditorTarget iATFEditorTarget = node.As<IATFEditorTarget>();
		if (SupportsCustomEditing(iATFEditorTarget))
		{
			IATFEditor customEditor = GetCustomEditor(iATFEditorTarget.CustomEditor);
			IATFEditingContext iATFEditingContext = iATFEditorTarget.As<IATFEditingContext>();
			m_currentEditingContext = iATFEditingContext as IIndexSelectionContext;
			SetEditorControl(customEditor as Control);
			customEditor?.Bind(iATFEditingContext);
			return;
		}
		ArtDefContext artDefContext = node.GetRoot().As<ArtDefContext>();
		if (artDefContext != null)
		{
			if (artDefContext.Selection.Count > 1)
			{
				IEnumerable<object> selection = from wc in artDefContext.Selection.Select(delegate(object sc)
					{
						AdaptablePath<object> adaptablePath = sc.As<AdaptablePath<object>>();
						return (!(adaptablePath != null)) ? null : adaptablePath.Last.As<IPropertyEditingContext>();
					})
					where wc != null
					select wc;
				m_propertyControl.Bind(new ArtDefElementMultiSelectPropertyEditingContext(artDefContext, selection));
			}
			else if (artDefContext.Selection.Count == 1)
			{
				m_propertyControl.Bind(node.As<IPropertyEditingContext>());
			}
			else
			{
				BugSubmitter.SilentReport(string.Format("Current Selection: {0}\nLineage: {1} @summary Found objects in TreeView selection that are not ", string.Join(", ", node.GetRoot().As<ArtDefContext>().Selection.Select((object n) => n.GetType())), string.Join(", ", node.Lineage.Select((DomNode n) => n.Type))) + "AdaptablePath<object>. This may not be intended. @assign matthew.kelley");
				m_propertyControl.Bind(null);
			}
		}
		else
		{
			m_propertyControl.Bind(null);
		}
		SetEditorControl(m_propertyControl);
	}

	private IATFEditor GetCustomEditor(string customEditorName)
	{
		IATFEditor value = null;
		if (!m_customEditors.TryGetValue(customEditorName, out value))
		{
			value = EditorCatalog.CreateEditor(customEditorName);
			m_customEditors[customEditorName] = value;
		}
		return value;
	}

	private DomNode GetSelectedDomNode(ArtDefContext artDefContext)
	{
		AdaptablePath<object> adaptablePath = artDefContext.LastSelected as AdaptablePath<object>;
		if (adaptablePath != null)
		{
			return adaptablePath.Last.As<DomNode>();
		}
		return artDefContext.LastSelected as DomNode;
	}

	private void SetEditorControl(Control ctl)
	{
		if (m_editControl != ctl)
		{
			MainControl.SuspendLayout();
			if (m_editControl != null)
			{
				MainControl.PropertyArea.Controls.Remove(m_editControl);
			}
			m_editControl = ctl;
			if (m_editControl != null)
			{
				MainControl.PropertyArea.Controls.Add(m_editControl);
				m_editControl.Dock = DockStyle.Fill;
			}
			MainControl.ResumeLayout();
		}
	}

	private bool SupportsCustomEditing(IATFEditorTarget editorTarget)
	{
		if (editorTarget != null && editorTarget.UseCustomEditor)
		{
			return EditorCatalog.IsEditorSupported(editorTarget.CustomEditor);
		}
		return false;
	}
}
