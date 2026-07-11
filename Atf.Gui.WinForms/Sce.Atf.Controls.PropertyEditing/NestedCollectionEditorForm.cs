using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class NestedCollectionEditorForm : Form
{
	private class TreeView : ITreeView, IItemView, ISelectionContext
	{
		private readonly object mRoot;

		private Func<object, ItemInfo, bool> m_getItemInfo;

		private readonly Dictionary<object, ICollection> m_itemCollection = new Dictionary<object, ICollection>();

		private readonly Selection<object> m_selection;

		public Func<object, ItemInfo, bool> GetItemInfo
		{
			get
			{
				return m_getItemInfo;
			}
			set
			{
				m_getItemInfo = value;
			}
		}

		public object Root => mRoot;

		public IEnumerable<object> Selection
		{
			get
			{
				return m_selection;
			}
			set
			{
				m_selection.SetRange(value);
			}
		}

		public object LastSelected => m_selection.LastSelected;

		public int SelectionCount => m_selection.Count;

		public event EventHandler SelectionChanging;

		public event EventHandler SelectionChanged;

		public TreeView(ITypeDescriptorContext context, object value)
		{
			mRoot = value;
			m_selection = new Selection<object>();
			m_selection.Changed += selection_Changed;
			if (this.SelectionChanging != null)
			{
			}
		}

		public IEnumerable<object> GetChildren(object parent)
		{
			if (!(parent is ICollection collection))
			{
				yield break;
			}
			foreach (object subItem in collection)
			{
				if (!m_itemCollection.ContainsKey(subItem))
				{
					m_itemCollection.Add(subItem, collection);
				}
				yield return subItem;
			}
		}

		public void GetInfo(object item, ItemInfo info)
		{
			if (m_getItemInfo == null || !m_getItemInfo(item, info))
			{
				info.AllowLabelEdit = false;
				info.Label = item.ToString();
			}
		}

		public IEnumerable<T> GetSelection<T>() where T : class
		{
			return m_selection.AsIEnumerable<T>();
		}

		public T GetLastSelected<T>() where T : class
		{
			return m_selection.GetLastSelected<T>();
		}

		public bool SelectionContains(object item)
		{
			return m_selection.Contains(item);
		}

		internal ICollection ItemCollection(object item)
		{
			m_itemCollection.TryGetValue(item, out var value);
			return value;
		}

		internal void RemoveItemCollection(object item)
		{
			if (m_itemCollection.ContainsKey(item))
			{
				m_itemCollection.Remove(item);
			}
		}

		private void selection_Changed(object sender, EventArgs e)
		{
			this.SelectionChanged.Raise(this, EventArgs.Empty);
		}
	}

	private PropertyGrid m_propertyGrid;

	private TreeControl m_treeControl;

	private TreeView m_collectionTreeView;

	private TreeControlAdapter m_treeControlAdapter;

	private ITypeDescriptorContext m_context;

	private Func<Path<object>, IEnumerable<Pair<Type, NestedCollectionEditor.CreateCollectionObject>>> m_getCollectionItemCreators;

	private readonly List<Pair<Type, NestedCollectionEditor.CreateCollectionObject>> m_availaibleTypeCreators = new List<Pair<Type, NestedCollectionEditor.CreateCollectionObject>>();

	private object m_rootValue;

	private readonly SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();

	private IContainer components = null;

	private SplitContainer splitContainer1;

	private Label label1;

	private Button addButton;

	private ComboBox comboBox1;

	private Button downButton;

	private Button upButton;

	private Button deleteButton;

	public Func<Path<object>, IEnumerable<Pair<Type, NestedCollectionEditor.CreateCollectionObject>>> GetCollectionItemCreators
	{
		get
		{
			return m_getCollectionItemCreators;
		}
		set
		{
			m_getCollectionItemCreators = value;
		}
	}

	public NestedCollectionEditorForm(ITypeDescriptorContext context, ISelectionContext selectionContext, object value, Func<Path<object>, IEnumerable<Pair<Type, NestedCollectionEditor.CreateCollectionObject>>> getCollectionItemCreators, Func<object, ItemInfo, bool> getItemInfo)
	{
		InitializeComponent();
		addButton.Click += addButton_Click;
		deleteButton.Click += deleteButton_Click;
		upButton.Click += upButton_Click;
		downButton.Click += downButton_Click;
		base.FormClosed += nestedCollectionEditorForm_FormClosed;
		GetCollectionItemCreators = getCollectionItemCreators;
		Bind(context, selectionContext, value, getItemInfo);
	}

	private void Bind(ITypeDescriptorContext context, ISelectionContext selectionContext, object value, Func<object, ItemInfo, bool> getItemInfo)
	{
		splitContainer1.SuspendLayout();
		m_treeControl = new TreeControl(TreeControl.Style.SimpleTree);
		m_treeControl.Dock = DockStyle.Fill;
		m_treeControl.SelectionMode = SelectionMode.One;
		m_treeControl.ImageList = ResourceUtil.GetImageList16();
		m_treeControl.Location = new Point(comboBox1.Location.X, comboBox1.Location.Y + 2 * base.FontHeight);
		m_treeControl.Width = upButton.Location.X - m_treeControl.Location.X - base.Margin.Right - base.Margin.Left;
		m_treeControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
		m_treeControl.Height = splitContainer1.Panel1.Height - m_treeControl.Location.Y - (base.Margin.Bottom + base.Margin.Top);
		splitContainer1.Panel1.Controls.Add(m_treeControl);
		m_propertyGrid = new PropertyGrid();
		m_defaultContext.SelectionContext = selectionContext;
		m_propertyGrid.Bind(m_defaultContext);
		m_propertyGrid.Dock = DockStyle.Fill;
		splitContainer1.Panel2.Controls.Add(m_propertyGrid);
		splitContainer1.ResumeLayout();
		m_rootValue = value;
		m_collectionTreeView = new TreeView(context, value);
		m_collectionTreeView.GetItemInfo = getItemInfo;
		m_treeControlAdapter = new TreeControlAdapter(m_treeControl);
		m_collectionTreeView.SelectionChanged += collectionTreeView_SelectionChanged;
		m_treeControl.ShowRoot = false;
		m_treeControlAdapter.TreeView = m_collectionTreeView;
		m_context = context;
		UpdateAvailaibleTypes(new Path<object>(context.Instance));
	}

	private void collectionTreeView_SelectionChanged(object sender, EventArgs e)
	{
		Path<object> path = m_collectionTreeView.LastSelected as Path<object>;
		if (path == null)
		{
			m_propertyGrid.Bind(null);
		}
		else
		{
			m_propertyGrid.Bind(path.Last);
		}
		UpdateAvailaibleTypes(path);
	}

	private void addButton_Click(object sender, EventArgs e)
	{
		foreach (Pair<Type, NestedCollectionEditor.CreateCollectionObject> availaibleTypeCreator in m_availaibleTypeCreators)
		{
			if (!(availaibleTypeCreator.First.Name == comboBox1.Text))
			{
				continue;
			}
			Path<object> path = m_collectionTreeView.LastSelected as Path<object>;
			object instance = null;
			if (availaibleTypeCreator.Second == null)
			{
				instance = Activator.CreateInstance(availaibleTypeCreator.First, nonPublic: true);
			}
			else
			{
				instance = availaibleTypeCreator.Second(m_rootValue);
			}
			if (path == null || path.Count == 1)
			{
				IList listToInsert = m_rootValue as IList;
				if (listToInsert != null)
				{
					ITransactionContext context = m_defaultContext.As<ITransactionContext>();
					context.DoTransaction(delegate
					{
						listToInsert.Add(instance);
					}, "Added Collection Item".Localize());
					m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
				}
			}
			else if (m_collectionTreeView.ItemCollection(path.Last) is IList list)
			{
				int num = list.IndexOf(path.Last);
				list.Insert(num + 1, instance);
				m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
			}
			using (IEnumerator<Path<object>> enumerator2 = m_treeControlAdapter.GetPaths(instance).GetEnumerator())
			{
				if (enumerator2.MoveNext())
				{
					Path<object> current2 = enumerator2.Current;
					m_collectionTreeView.Selection = new object[1] { current2 };
				}
			}
			RefreshOwnerForm();
			m_treeControl.Focus();
			break;
		}
	}

	private void deleteButton_Click(object sender, EventArgs e)
	{
		Path<object> lastPath = m_collectionTreeView.LastSelected as Path<object>;
		if (!(lastPath != null))
		{
			return;
		}
		IList listToRemove = m_collectionTreeView.ItemCollection(lastPath.Last) as IList;
		if (listToRemove != null)
		{
			int num = listToRemove.IndexOf(lastPath.Last);
			int num2 = ((num + 1 < listToRemove.Count) ? (num + 1) : (num - 1));
			object obj = ((num2 >= 0) ? listToRemove[num2] : null);
			ITransactionContext context = m_defaultContext.As<ITransactionContext>();
			context.DoTransaction(delegate
			{
				listToRemove.Remove(lastPath.Last);
			}, "Removed Collection Item".Localize());
			m_collectionTreeView.RemoveItemCollection(lastPath.Last);
			if (obj != null)
			{
				using IEnumerator<Path<object>> enumerator = m_treeControlAdapter.GetPaths(obj).GetEnumerator();
				if (enumerator.MoveNext())
				{
					Path<object> current = enumerator.Current;
					m_collectionTreeView.Selection = new object[1] { current };
				}
			}
			else
			{
				m_collectionTreeView.Selection = EmptyEnumerable<object>.Instance;
				UpdateAvailaibleTypes(new Path<object>(m_context.Instance));
			}
			m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
		}
		RefreshOwnerForm();
		m_treeControl.Focus();
	}

	private void downButton_Click(object sender, EventArgs e)
	{
		Path<object> lastPath = m_collectionTreeView.LastSelected as Path<object>;
		if (lastPath != null && lastPath.Count > 1)
		{
			IList listToInsert = m_collectionTreeView.ItemCollection(lastPath.Last) as IList;
			if (listToInsert != null)
			{
				int index = listToInsert.IndexOf(lastPath.Last);
				if (index >= 0 && index < listToInsert.Count - 1)
				{
					ITransactionContext context = m_defaultContext.As<ITransactionContext>();
					context.DoTransaction(delegate
					{
						listToInsert.RemoveAt(index);
						listToInsert.Insert(index + 1, lastPath.Last);
					}, "Moved Collection Item Down".Localize());
					m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
				}
			}
		}
		RefreshOwnerForm();
		m_treeControl.Focus();
	}

	private void upButton_Click(object sender, EventArgs e)
	{
		Path<object> lastPath = m_collectionTreeView.LastSelected as Path<object>;
		if (lastPath != null && lastPath.Count > 1)
		{
			IList listToInsert = m_collectionTreeView.ItemCollection(lastPath.Last) as IList;
			if (listToInsert != null)
			{
				int index = listToInsert.IndexOf(lastPath.Last);
				if (index > 0)
				{
					ITransactionContext context = m_defaultContext.As<ITransactionContext>();
					context.DoTransaction(delegate
					{
						listToInsert.RemoveAt(index);
						listToInsert.Insert(index - 1, lastPath.Last);
					}, "Moved Collection Item Up".Localize());
					m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
				}
			}
		}
		RefreshOwnerForm();
		m_treeControl.Focus();
	}

	private void nestedCollectionEditorForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		RefreshOwnerForm();
	}

	private void RefreshOwnerForm()
	{
		if (base.Owner is NestedCollectionEditorForm)
		{
			((NestedCollectionEditorForm)base.Owner).ForceRefresh();
		}
	}

	private void ForceRefresh()
	{
		m_treeControlAdapter.Refresh(m_collectionTreeView.Root);
		if (base.Owner is NestedCollectionEditorForm)
		{
			((NestedCollectionEditorForm)base.Owner).ForceRefresh();
		}
	}

	private void UpdateAvailaibleTypes(Path<object> objectPath)
	{
		comboBox1.Items.Clear();
		m_availaibleTypeCreators.Clear();
		if (GetCollectionItemCreators != null)
		{
			foreach (Pair<Type, NestedCollectionEditor.CreateCollectionObject> item in GetCollectionItemCreators(objectPath))
			{
				comboBox1.Items.Add(item.First.Name);
				m_availaibleTypeCreators.Add(new Pair<Type, NestedCollectionEditor.CreateCollectionObject>(item.First, item.Second));
			}
		}
		if (!comboBox1.Items.Contains(comboBox1.Text))
		{
			if (comboBox1.Items.Count > 0)
			{
				comboBox1.Text = comboBox1.Items[0].ToString();
			}
			else
			{
				comboBox1.Text = string.Empty;
			}
		}
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Sce.Atf.Controls.PropertyEditing.NestedCollectionEditorForm));
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.deleteButton = new System.Windows.Forms.Button();
		this.downButton = new System.Windows.Forms.Button();
		this.upButton = new System.Windows.Forms.Button();
		this.addButton = new System.Windows.Forms.Button();
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		base.SuspendLayout();
		resources.ApplyResources(this.splitContainer1, "splitContainer1");
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.deleteButton);
		this.splitContainer1.Panel1.Controls.Add(this.downButton);
		this.splitContainer1.Panel1.Controls.Add(this.upButton);
		this.splitContainer1.Panel1.Controls.Add(this.addButton);
		this.splitContainer1.Panel1.Controls.Add(this.comboBox1);
		this.splitContainer1.Panel1.Controls.Add(this.label1);
		resources.ApplyResources(this.deleteButton, "deleteButton");
		this.deleteButton.Name = "deleteButton";
		this.deleteButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.downButton, "downButton");
		this.downButton.Name = "downButton";
		this.downButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.upButton, "upButton");
		this.upButton.Name = "upButton";
		this.upButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.addButton, "addButton");
		this.addButton.Name = "addButton";
		this.addButton.UseVisualStyleBackColor = true;
		resources.ApplyResources(this.comboBox1, "comboBox1");
		this.comboBox1.FormattingEnabled = true;
		this.comboBox1.Name = "comboBox1";
		resources.ApplyResources(this.label1, "label1");
		this.label1.Name = "label1";
		resources.ApplyResources(this, "$this");
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.splitContainer1);
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "NestedCollectionEditorForm";
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}
