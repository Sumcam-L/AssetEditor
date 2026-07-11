using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Sce.Atf.Controls.PropertyEditing;

public abstract class PropertyView : Control, IPropertyEditingControlOwner
{
	private enum UpdateStatus
	{
		NoUpdateNeeded,
		UpdateNeeded,
		RebuildNeeded,
		ReloadNeeded
	}

	public class Property
	{
		public System.ComponentModel.PropertyDescriptor Descriptor;

		public Category Category;

		public PropertyEditorControlContext Context;

		public int DescriptorIndex;

		public bool FirstInCategory;

		public bool DisableSort;

		public bool DisableDragging;

		public bool DisableResize;

		public bool DisableEditing;

		public bool HideDisplayName;

		public int DefaultWidth;

		public int HorizontalEditorOffset = -1;

		public bool NameHasWholeRow;

		public PropertyDescriptorCollection ChildProperties;

		public Property Parent;

		public bool ChildrenExpanded;

		private Control m_control;

		private bool m_cacheable;

		private readonly PropertyView m_owner;

		public Control Control
		{
			get
			{
				return m_control;
			}
			set
			{
				if (m_cacheable)
				{
					m_owner.m_cacheableProperties.Remove(Descriptor);
				}
				m_control = value;
				m_cacheable = (m_control as ICacheablePropertyControl)?.Cacheable ?? false;
				if (m_cacheable)
				{
					m_owner.m_cacheableProperties.Remove(Descriptor);
					m_owner.m_cacheableProperties.Add(Descriptor, this);
				}
			}
		}

		public bool Cacheable => m_cacheable;

		public bool Visible
		{
			get
			{
				Property property = this;
				while (property.Parent != null)
				{
					if (!property.Parent.ChildrenExpanded)
					{
						return false;
					}
					property = property.Parent;
				}
				if (property.Parent == null)
				{
					if (Category == null)
					{
						return true;
					}
					return Category.Expanded;
				}
				return property.ChildrenExpanded;
			}
		}

		public IEnumerable<Property> Lineage
		{
			get
			{
				for (Property property = this; property != null; property = property.Parent)
				{
					yield return property;
				}
			}
		}

		internal Property(PropertyView owner)
		{
			m_owner = owner;
		}
	}

	public class Category
	{
		public string Name;

		public string DisplayName;

		public Property[] Properties;

		public Category Parent;

		private PropertyView m_owner;

		private static readonly char[] SubCategorySeparator = new char[1] { '\\' };

		public bool Expanded
		{
			get
			{
				if (!m_owner.m_categoryExpanded[Name])
				{
					return false;
				}
				if (Parent == null)
				{
					return true;
				}
				return Parent.Expanded;
			}
			set
			{
				m_owner.m_categoryExpanded[Name] = value;
			}
		}

		public bool Visible => Parent == null || Parent.Expanded;

		public Category(PropertyView owner)
		{
			m_owner = owner;
		}

		internal static bool IsSubCategoryName(string fullName, out string parentName, out string[] childName)
		{
			parentName = null;
			childName = null;
			int num = fullName.IndexOf('\\');
			if (num < 0)
			{
				return false;
			}
			parentName = fullName.Substring(0, num);
			childName = fullName.Substring(num + 1).Split(SubCategorySeparator, StringSplitOptions.RemoveEmptyEntries);
			return true;
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class CustomizeAttribute : Sce.Atf.Controls.PropertyEditing.CustomizeAttribute
	{
		public CustomizeAttribute(string propertyName, int columnWidth = 0, bool disableSort = false, bool disableDragging = false, bool disableResize = false, bool disableEditing = false, bool hideDisplayName = false, int horizontalEditorOffset = -1, bool nameHasWholeRow = false)
			: base(propertyName, columnWidth, disableSort, disableDragging, disableResize, disableEditing, hideDisplayName, horizontalEditorOffset, nameHasWholeRow)
		{
		}
	}

	public static bool EnableSubCategories;

	protected Font BoldFont;

	protected int RowHeight;

	private UpdateStatus m_updateStatus = UpdateStatus.NoUpdateNeeded;

	private int m_updateDepth = 0;

	private string m_filterPattern;

	protected Property m_selectedProperty;

	private FilteringContext m_editingContext;

	private object[] m_selectedObjects = EmptyArray<object>.Instance;

	private System.ComponentModel.PropertyDescriptor[] m_propertyDescriptors = EmptyArray<System.ComponentModel.PropertyDescriptor>.Instance;

	protected readonly Queue<Control> m_reusableControls = new Queue<Control>();

	private Category[] m_categories = EmptyArray<Category>.Instance;

	private readonly List<Property> m_activeProperties = new List<Property>();

	private readonly Dictionary<System.ComponentModel.PropertyDescriptor, Property> m_cacheableProperties = new Dictionary<System.ComponentModel.PropertyDescriptor, Property>();

	private PropertySorting m_propertySorting = PropertySorting.ByCategory;

	private readonly Dictionary<string, bool> m_categoryExpanded = new Dictionary<string, bool>();

	private readonly HashSet<System.ComponentModel.PropertyDescriptor> m_processedDescriptors = new HashSet<System.ComponentModel.PropertyDescriptor>();

	private List<string> m_customSortOrder;

	protected static StringFormat LeftStringFormat;

	protected static StringFormat RightStringFormat;

	protected static Size SystemDragSize;

	protected const int ExpanderSize = 8;

	public object[] SelectedObjects => m_selectedObjects;

	public object LastSelectedObject => (m_selectedObjects.Length != 0) ? m_selectedObjects[m_selectedObjects.Length - 1] : null;

	public IPropertyEditingContext EditingContext
	{
		get
		{
			return m_editingContext;
		}
		set
		{
			if (value == m_editingContext)
			{
				return;
			}
			if (m_editingContext != null)
			{
				m_editingContext.FilterChanged -= FilteringContext_FilterChanged;
				IObservableContext observableContext = m_editingContext.As<IObservableContext>();
				if (observableContext != null)
				{
					observableContext.ItemInserted -= ObservableContext_OnItemInserted;
					observableContext.ItemRemoved -= ObservableContext_OnItemRemoved;
					observableContext.ItemChanged -= ObservableContext_OnItemChanged;
					observableContext.Reloaded -= ObservableContext_OnReloaded;
				}
				IValidationContext validationContext = m_editingContext.As<IValidationContext>();
				if (validationContext != null)
				{
					validationContext.Beginning -= ValidationContext_Beginning;
					validationContext.Ended -= ValidationContext_Ended;
					validationContext.Cancelled -= ValidationContext_Cancelled;
				}
				ISelectionContext selectionContext = m_editingContext.As<ISubSelectionContext>()?.SubSelectionContext;
				if (selectionContext != null)
				{
					selectionContext.SelectionChanged -= subSelectionContext_SelectionChanged;
				}
			}
			System.ComponentModel.PropertyDescriptor[] oldPropertyDescriptors = m_propertyDescriptors;
			OnEditingContextChanging();
			if (value == null || value.Is<FilteringContext>())
			{
				m_editingContext = value.As<FilteringContext>();
			}
			else
			{
				m_editingContext = new FilteringContext(value);
			}
			m_selectedObjects = EmptyArray<object>.Instance;
			m_propertyDescriptors = EmptyArray<System.ComponentModel.PropertyDescriptor>.Instance;
			if (m_editingContext != null)
			{
				m_editingContext.FilterChanged += FilteringContext_FilterChanged;
				RefreshSelectedObjects();
				m_propertyDescriptors = m_editingContext.PropertyDescriptors.ToArray();
				IValidationContext validationContext2 = m_editingContext.As<IValidationContext>();
				if (validationContext2 != null)
				{
					validationContext2.Beginning += ValidationContext_Beginning;
					validationContext2.Ended += ValidationContext_Ended;
					validationContext2.Cancelled += ValidationContext_Cancelled;
				}
				IObservableContext observableContext2 = m_editingContext.As<IObservableContext>();
				if (observableContext2 != null)
				{
					observableContext2.ItemInserted += ObservableContext_OnItemInserted;
					observableContext2.ItemRemoved += ObservableContext_OnItemRemoved;
					observableContext2.ItemChanged += ObservableContext_OnItemChanged;
					observableContext2.Reloaded += ObservableContext_OnReloaded;
				}
				ISelectionContext selectionContext2 = m_editingContext.As<ISubSelectionContext>()?.SubSelectionContext;
				if (selectionContext2 != null)
				{
					selectionContext2.SelectionChanged += subSelectionContext_SelectionChanged;
				}
			}
			if (HasSamePropertyDescriptors(oldPropertyDescriptors, m_propertyDescriptors))
			{
				SuspendLayout();
				RefreshEditingControls();
				ResumeLayout(performLayout: true);
				Invalidate();
				this.EditingContextUpdated.Raise(this, EventArgs.Empty);
			}
			else
			{
				var sw = Stopwatch.StartNew();
				UpdateEditingContext();
				sw.Stop();
				PaintTimingLog.Write("PropertyView.BuildProperties: {0}ms ({1} props)", sw.ElapsedMilliseconds, m_propertyDescriptors?.Length ?? 0);
			}
			{
				var sw = Stopwatch.StartNew();
				OnEditingContextChanged();
				sw.Stop();
				if (sw.ElapsedMilliseconds > 5)
					PaintTimingLog.Write("OnEditingContextChanged: {0}ms", sw.ElapsedMilliseconds);
			}
			this.EditingContextChanged.Raise(this, EventArgs.Empty);
		}
	}

	public IContextRegistry ContextRegistry { get; set; }

	public string FilterPattern
	{
		get
		{
			return m_filterPattern;
		}
		set
		{
			m_filterPattern = value;
			UpdateEditingContext();
		}
	}

	public bool CanResetCurrent => PropertyUtils.CanResetProperty(SelectedObjects, SelectedPropertyDescriptor);

	public System.ComponentModel.PropertyDescriptor SelectedPropertyDescriptor => (m_selectedProperty != null) ? m_selectedProperty.Descriptor : null;

	protected Property SelectedProperty
	{
		get
		{
			return m_selectedProperty;
		}
		set
		{
			if (m_selectedProperty != value)
			{
				m_selectedProperty = value;
				OnSelectedPropertyChanged(EventArgs.Empty);
			}
		}
	}

	public string Settings
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("PropertyView");
			xmlElement.SetAttribute("PropertySorting", m_propertySorting.ToString());
			xmlDocument.AppendChild(xmlElement);
			foreach (KeyValuePair<string, bool> item in m_categoryExpanded)
			{
				XmlElement xmlElement2 = xmlDocument.CreateElement("Category");
				xmlElement.AppendChild(xmlElement2);
				xmlElement2.SetAttribute("Name", item.Key);
				xmlElement2.SetAttribute("Expanded", item.Value.ToString());
			}
			WriteSettings(xmlElement);
			return xmlDocument.InnerXml;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				return;
			}
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(value);
			XmlElement documentElement = xmlDocument.DocumentElement;
			if (documentElement == null || documentElement.Name != "PropertyView")
			{
				throw new Exception("Invalid PropertyView settings");
			}
			string attribute = documentElement.GetAttribute("PropertySorting");
			m_propertySorting = (PropertySorting)Enum.Parse(typeof(PropertySorting), attribute);
			XmlNodeList xmlNodeList = documentElement.SelectNodes("Category");
			foreach (XmlElement item in xmlNodeList)
			{
				string attribute2 = item.GetAttribute("Name");
				attribute = item.GetAttribute("Width");
				if (bool.TryParse(attribute, out var result))
				{
					m_categoryExpanded[attribute2] = result;
				}
			}
			ReadSettings(documentElement);
			Invalidate();
		}
	}

	public PropertySorting PropertySorting
	{
		get
		{
			return m_propertySorting;
		}
		set
		{
			if (m_propertySorting != value)
			{
				m_propertySorting = value;
				UpdatePropertySorting();
				OnPropertySortingChanged();
			}
		}
	}

	public IEnumerable<Property> Properties
	{
		get
		{
			if ((m_propertySorting & PropertySorting.Categorized) != PropertySorting.None)
			{
				if (m_categories == null)
				{
					yield break;
				}
				Category[] categories = m_categories;
				foreach (Category category in categories)
				{
					Property[] properties = category.Properties;
					for (int j = 0; j < properties.Length; j++)
					{
						yield return properties[j];
					}
				}
				yield break;
			}
			foreach (Property activeProperty in m_activeProperties)
			{
				yield return activeProperty;
			}
		}
	}

	protected IEnumerable<object> Items
	{
		get
		{
			if ((m_propertySorting & PropertySorting.Categorized) != PropertySorting.None)
			{
				if (m_categories == null)
				{
					yield break;
				}
				Category[] categories = m_categories;
				foreach (Category category in categories)
				{
					yield return category;
					Property[] properties = category.Properties;
					for (int j = 0; j < properties.Length; j++)
					{
						yield return properties[j];
					}
				}
				yield break;
			}
			foreach (Property activeProperty in m_activeProperties)
			{
				yield return activeProperty;
			}
		}
	}

	protected IEnumerable<object> VisibleItems
	{
		get
		{
			foreach (object obj in Items)
			{
				if (obj is Property p)
				{
					if (p.Visible)
					{
						yield return obj;
					}
					continue;
				}
				Category c = (Category)obj;
				if (c.Visible)
				{
					yield return obj;
				}
			}
		}
	}

	public CustomizeAttribute[] CustomizeAttributes { get; set; }

	public event EventHandler EditingContextChanged;

	public event EventHandler EditingContextUpdated;

	public event EventHandler SelectedPropertyChanged;

	public PropertyView()
	{
		UpdateFonts();
		UpdateRowHeight();
		base.DoubleBuffered = true;
		base.AllowDrop = true;
		FilterPattern = "";
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (BoldFont != null)
			{
				BoldFont.Dispose();
			}
			while (m_reusableControls.Count > 0)
			{
				m_reusableControls.Dequeue().Dispose();
			}
			EditingContext = null;
		}
		base.Dispose(disposing);
	}

	public override void Refresh()
	{
		RefreshEditingControls();
		base.Refresh();
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		UpdateEditingContext();
		SkinService.ApplyActiveSkin(this);
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		UpdateFonts();
		UpdateRowHeight();
		PerformLayout();
		Invalidate();
	}

	private void UpdateFonts()
	{
		if (BoldFont != null)
		{
			BoldFont.Dispose();
		}
		if (base.Font.FontFamily.IsStyleAvailable(FontStyle.Bold))
		{
			BoldFont = new Font(base.Font, FontStyle.Bold);
		}
		else
		{
			BoldFont = new Font(base.Font, base.Font.Style);
		}
	}

	protected override void OnMarginChanged(EventArgs e)
	{
		UpdateRowHeight();
		PerformLayout();
		base.OnMarginChanged(e);
	}

	private void UpdateRowHeight()
	{
		RowHeight = base.FontHeight + base.Margin.Top;
	}

	protected void SetFont(Control control, System.ComponentModel.PropertyDescriptor descriptor)
	{
	}

	protected void FilteringContext_FilterChanged(object sender, EventArgs e)
	{
		RefreshSelectedObjects();
		Invalidate();
		RefreshEditingControls();
	}

	private void RefreshSelectedObjects()
	{
		if (m_editingContext != null)
		{
			m_selectedObjects = m_editingContext.Items.ToArray();
		}
		else
		{
			m_selectedObjects = EmptyArray<object>.Instance;
		}
	}

	private void subSelectionContext_SelectionChanged(object sender, EventArgs e)
	{
		Invalidate();
		RefreshEditingControls();
		OnSubSelectionChanged();
	}

	private void UpdateEditingContext()
	{
		this.CheckForIllegalCrossThreadCall();
		SuspendLayout();
		SelectedProperty = null;
		ClearCurrentProperties();
		if (base.Visible && m_editingContext != null)
		{
			int needed = m_propertyDescriptors.Length - m_reusableControls.Count;
			for (int i = 0; i < needed; i++)
			{
				m_reusableControls.Enqueue(new PropertyEditingControl());
			}
			BuildProperties();
		}
		UpdatePropertySorting();
		ResumeLayout(performLayout: true);
		RefreshEditingControls();
		Invalidate();
		this.EditingContextUpdated.Raise(this, EventArgs.Empty);
	}

	private static bool HasSamePropertyDescriptors(System.ComponentModel.PropertyDescriptor[] oldDescriptors, System.ComponentModel.PropertyDescriptor[] newDescriptors)
	{
		if (oldDescriptors == null || newDescriptors == null)
		{
			return false;
		}
		if (oldDescriptors.Length != newDescriptors.Length)
		{
			return false;
		}
		for (int i = 0; i < oldDescriptors.Length; i++)
		{
			if (oldDescriptors[i].Name != newDescriptors[i].Name || oldDescriptors[i].ComponentType != newDescriptors[i].ComponentType)
			{
				return false;
			}
		}
		return true;
	}

	private void BeginUpdate()
	{
		m_updateDepth++;
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
			case UpdateStatus.RebuildNeeded:
				DoRebuild();
				break;
			case UpdateStatus.ReloadNeeded:
				DoReload();
				break;
			}
			m_updateStatus = UpdateStatus.NoUpdateNeeded;
		}
	}

	private void ValidationContext_Beginning(object sender, EventArgs e)
	{
		BeginUpdate();
	}

	private void ValidationContext_Cancelled(object sender, EventArgs e)
	{
		EndUpdate();
	}

	private void ValidationContext_Ended(object sender, EventArgs e)
	{
		EndUpdate();
	}

	private void DoUpdate()
	{
		if (m_updateDepth > 0)
		{
			if (m_updateStatus < UpdateStatus.UpdateNeeded)
			{
				m_updateStatus = UpdateStatus.UpdateNeeded;
			}
		}
		else
		{
			Invalidate();
			RefreshEditingControls();
		}
	}

	private void DoRebuild()
	{
		if (m_updateDepth > 0)
		{
			if (m_updateStatus < UpdateStatus.RebuildNeeded)
			{
				m_updateStatus = UpdateStatus.RebuildNeeded;
			}
		}
		else
		{
			RefreshSelectedObjects();
			UpdateEditingContext();
		}
	}

	private void DoReload()
	{
		if (m_updateDepth > 0)
		{
			if (m_updateStatus < UpdateStatus.ReloadNeeded)
			{
				m_updateStatus = UpdateStatus.ReloadNeeded;
			}
			return;
		}
		IPropertyEditingContext editingContext = m_editingContext;
		if (editingContext == null)
		{
			return;
		}
		bool itemsChanged = !m_selectedObjects.SequenceEqual(m_editingContext.Items);
		bool descriptorsChanged = !m_propertyDescriptors.SequenceEqual(m_editingContext.PropertyDescriptors);
		if (itemsChanged || descriptorsChanged)
		{
			EditingContext = null;
			EditingContext = editingContext;
		}
		else
		{
			Invalidate();
			this.InvokeIfRequired(delegate
			{
				RefreshEditingControls();
			});
		}
		OnSubSelectionChanged();
	}

	protected virtual void ObservableContext_OnItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		DoUpdate();
	}

	protected virtual void ObservableContext_OnItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		DoRebuild();
	}

	protected virtual void ObservableContext_OnItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		DoRebuild();
	}

	protected virtual void ObservableContext_OnReloaded(object sender, EventArgs e)
	{
		DoReload();
	}

	private void OnSubSelectionChanged()
	{
		ISelectionContext selectionContext = m_editingContext.As<ISubSelectionContext>()?.SubSelectionContext;
		if (selectionContext == null || selectionContext.SelectionCount <= 0)
		{
			return;
		}
		object[] array = selectionContext.Selection.ToArray();
		System.ComponentModel.PropertyDescriptor b = array[0].As<System.ComponentModel.PropertyDescriptor>();
		foreach (Property activeProperty in m_activeProperties)
		{
			if (PropertyUtils.PropertyDescriptorsEqual(activeProperty.Descriptor, b))
			{
				SelectedProperty = activeProperty;
			}
		}
	}

	protected virtual void OnEditingContextChanging()
	{
	}

	protected virtual void OnEditingContextChanged()
	{
	}

	protected virtual void OnCreateProperties()
	{
	}

	public void ResetCurrent()
	{
		if (CanResetCurrent)
		{
			PropertyUtils.ResetProperty(SelectedObjects, SelectedPropertyDescriptor);
			Invalidate();
		}
	}

	public void ResetAll()
	{
		if (m_editingContext == null)
		{
			return;
		}
		foreach (Property property in Properties)
		{
			if (PropertyUtils.CanResetProperty(SelectedObjects, property.Descriptor))
			{
				PropertyUtils.ResetProperty(SelectedObjects, property.Descriptor);
			}
		}
		if (!EditingContext.Is<IObservableContext>())
		{
			RefreshEditingControls();
		}
		Invalidate();
	}

	public void RefreshProperties()
	{
		Refresh();
	}

	protected virtual void RefreshEditingControls()
	{
		foreach (Property property in Properties)
		{
			Control control = property.Control;
			if (control != null && control.Visible)
			{
				SetFont(control, property.Descriptor);
				control.Refresh();
			}
		}
	}

	protected virtual void OnSelectedPropertyChanged(EventArgs e)
	{
		this.SelectedPropertyChanged?.Invoke(this, e);
	}

	public void ClearSelectedProperty()
	{
		if (SelectedProperty != null)
		{
			SelectedProperty = null;
			Refresh();
		}
	}

	public virtual void SetCustomPropertySortOrder(List<string> customSortOrder)
	{
		PropertySorting = PropertySorting.Custom;
		m_customSortOrder = customSortOrder;
		UpdateEditingContext();
	}

	protected void SortPropertiesFromPropertyNamesList(List<string> propertyNames)
	{
		List<Property> list = new List<Property>();
		foreach (Property activeProperty in m_activeProperties)
		{
			list.Add(activeProperty);
		}
		foreach (Property item in list)
		{
			RemoveProperty(item);
		}
		Dictionary<string, Property> lookup = new Dictionary<string, Property>(list.Count);
		foreach (Property p in list)
		{
			lookup[p.Descriptor.Name] = p;
		}
		foreach (string propertyName in propertyNames)
		{
			if (lookup.TryGetValue(propertyName, out var property))
			{
				AddProperty(property);
				list.Remove(property);
			}
		}
		foreach (Property item3 in list)
		{
			AddProperty(item3);
		}
	}

	protected virtual void ReadSettings(XmlElement root)
	{
	}

	protected virtual void WriteSettings(XmlElement root)
	{
	}

	protected virtual void OnPropertySortingChanged()
	{
		PerformLayout();
	}

	private void BuildProperties()
	{
		List<System.ComponentModel.PropertyDescriptor> list = new List<System.ComponentModel.PropertyDescriptor>(m_editingContext.PropertyDescriptors);
		m_activeProperties.Clear();
		m_processedDescriptors.Clear();
		int index = 0;
		string filterLower = m_filterPattern.Length > 0 ? m_filterPattern.ToLower() : null;
		for (int i = 0; i < list.Count; i++)
		{
			System.ComponentModel.PropertyDescriptor propertyDescriptor = list[i];
			if (!propertyDescriptor.IsBrowsable)
			{
				continue;
			}
			object[] selectedObjects = SelectedObjects;
			object lastSelectedObject = LastSelectedObject;
			ITransactionContext transactionContext = m_editingContext.As<ITransactionContext>();
			PropertyEditorControlContext propertyEditorControlContext = new PropertyEditorControlContext(this, propertyDescriptor, transactionContext, ContextRegistry);
			string propertyText = propertyEditorControlContext.GetPropertyText();
			Property property = BuildProperty(propertyDescriptor, index++);
			IgnoreChildrenAttribute ignoreChildrenAttribute = propertyDescriptor.Attributes.OfType<IgnoreChildrenAttribute>().FirstOrDefault();
			if (ignoreChildrenAttribute != null && ignoreChildrenAttribute.IgnoreChildren)
			{
				continue;
			}
			bool reflected = propertyDescriptor.GetType().Name == "ReflectPropertyDescriptor";
			AddChildProperty(property, reflected, ref index);
			if (property.Control != null && property.Control is EmbeddedCollectionEditor.CollectionControl)
			{
				property.Control.Refresh();
			}
			if (filterLower != null && !propertyDescriptor.Name.ToLower().Contains(filterLower) && !propertyText.ToLower().Contains(filterLower))
			{
				if (property.Control != null)
				{
					EmbeddedCollectionEditor.CollectionControl collectionControl2 = property.Control as EmbeddedCollectionEditor.CollectionControl;
					if (collectionControl2 == null)
					{
						m_activeProperties.Remove(property);
					}
				}
				else
				{
					m_activeProperties.Remove(property);
				}
			}
		}
		foreach (KeyValuePair<System.ComponentModel.PropertyDescriptor, Property> cacheableProperty in m_cacheableProperties)
		{
			Control control = cacheableProperty.Value.Control;
			if (control != null && !m_activeProperties.Contains(cacheableProperty.Value))
			{
				control.Visible = false;
			}
		}
	}

	private Property BuildProperty(System.ComponentModel.PropertyDescriptor descriptor, int index)
	{
		ITransactionContext transactionContext = m_editingContext.As<ITransactionContext>();
		INonCacheableDescriptor nonCacheableDescriptor = descriptor as INonCacheableDescriptor;
		if (nonCacheableDescriptor == null && m_cacheableProperties.TryGetValue(descriptor, out var value))
		{
			descriptor = value.Descriptor;
		}
		else
		{
			value = new Property(this);
		}
		value.Descriptor = descriptor;
		value.DescriptorIndex = index;
		if (value.Context != null)
		{
			value.Context.TransactionContext = transactionContext;
		}
		else
		{
			value.Context = new PropertyEditorControlContext(this, descriptor, transactionContext, ContextRegistry);
		}
		CustomizeAttribute customizeAttribute = null;
		CustomizeAttribute[] array = CustomizeAttributes;
		if (array == null)
		{
			array = (CustomizeAttribute[])descriptor.ComponentType.GetCustomAttributes(typeof(CustomizeAttribute), inherit: true);
		}
		if (array != null)
		{
			CustomizeAttribute[] array2 = array;
			foreach (CustomizeAttribute customizeAttribute2 in array2)
			{
				if (customizeAttribute2 != null && customizeAttribute2.PropertyName.Equals(descriptor.Name))
				{
					customizeAttribute = customizeAttribute2;
					break;
				}
			}
		}
		if (customizeAttribute != null)
		{
			value.DisableSort = customizeAttribute.DisableSort;
			value.DisableDragging = customizeAttribute.DisableDragging;
			value.DisableResize = customizeAttribute.DisableResize;
			value.DisableEditing = customizeAttribute.DisableEditing;
			value.DefaultWidth = customizeAttribute.ColumnWidth;
			value.HideDisplayName = customizeAttribute.HideDisplayName;
			value.HorizontalEditorOffset = customizeAttribute.HorizontalEditorOffset;
			value.NameHasWholeRow = customizeAttribute.NameHasWholeRow;
		}
		Control control = value.Control;
		if (control == null)
		{
			control = GetEditingControl(value);
			if (control != null)
			{
				control.Height = (int)Math.Round(GdiUtil.DpiFactor * (float)control.Height);
				IntPtr handle = control.Handle;
				value.Control = control;
				base.Controls.Add(control);
			}
		}
		if (control != null)
		{
			control.Visible = false;
			SetFont(control, descriptor);
			if (customizeAttribute != null)
			{
				control.Width = customizeAttribute.ColumnWidth;
			}
		}
		m_activeProperties.Add(value);
		m_processedDescriptors.Add(descriptor);
		return value;
	}

#if DEBUG
	private static int s_rebuildCount;
#endif

	private void AddChildProperty(Property property, bool reflected, ref int index)
	{
		List<System.ComponentModel.PropertyDescriptor> list = new List<System.ComponentModel.PropertyDescriptor>();
		foreach (System.ComponentModel.PropertyDescriptor childProperty in property.Descriptor.GetChildProperties())
		{
			if ((!reflected || !m_processedDescriptors.Contains(childProperty)) && (!(childProperty.GetType().Name == "ReflectPropertyDescriptor") || reflected) && childProperty.IsBrowsable)
			{
				Property property2 = BuildProperty(childProperty, index++);
				list.Add(childProperty);
				property2.Parent = property;
				AddChildProperty(property2, reflected, ref index);
			}
		}
		if (list.Count > 0)
		{
			property.ChildProperties = new PropertyDescriptorCollection(list.ToArray());
		}
	}

	protected virtual Control GetEditingControl(Property property)
	{
		Control result = null;
		if (property.Descriptor.GetEditor(typeof(IPropertyEditor)) is IPropertyEditor propertyEditor)
		{
			result = propertyEditor.GetEditingControl(property.Context);
		}
		return result;
	}

	private void ClearCurrentProperties()
	{
		foreach (Property activeProperty in m_activeProperties)
		{
			activeProperty.Context.TransactionContext = null;
			activeProperty.Context.ClearCachedSelection();
			Control control = activeProperty.Control;
			if (control != null)
			{
				if (control is PropertyEditingControl)
				{
					base.Controls.Remove(control);
					control.Visible = false;
					m_reusableControls.Enqueue(control);
				}
				else if (!activeProperty.Cacheable || activeProperty.Descriptor is INonCacheableDescriptor)
				{
					base.Controls.Remove(control);
					control.Font = null;
					control.Dispose();
				}
				else
				{
					control.Hide();
				}
			}
		}
		m_activeProperties.Clear();
		m_processedDescriptors.Clear();
		m_categories = null;
	}

	private void UpdatePropertySorting()
	{
		if ((PropertySorting & PropertySorting.Custom) == PropertySorting.Custom)
		{
			if (m_customSortOrder != null)
			{
				SortPropertiesFromPropertyNamesList(m_customSortOrder);
			}
		}
		else if ((PropertySorting & PropertySorting.Alphabetical) == 0)
		{
			m_activeProperties.Sort((Property x, Property y) => x.DescriptorIndex.CompareTo(y.DescriptorIndex));
		}
		else
		{
			m_activeProperties.Sort(MultiLevelSort);
		}
		foreach (Property activeProperty in m_activeProperties)
		{
			activeProperty.Category = null;
			activeProperty.FirstInCategory = false;
		}
		if ((m_propertySorting & PropertySorting.Categorized) != PropertySorting.None)
		{
			BuildCategories();
		}
	}

	private Property GetLowestCommonAncestor(Property node1, Property node2)
	{
		if (node1 == null)
		{
			return node2;
		}
		if (node2 == null)
		{
			return node1;
		}
		HashSet<Property> hashSet = new HashSet<Property>(node1.Lineage);
		foreach (Property item in node2.Lineage)
		{
			if (hashSet.Contains(item))
			{
				return item;
			}
		}
		return null;
	}

	private int MultiLevelSort(Property a, Property b)
	{
		if (a.Parent == null && b.Parent == null)
		{
			return string.Compare(a.Descriptor.Name, b.Descriptor.Name);
		}
		int depthA = GetDepth(a);
		int depthB = GetDepth(b);
		Property nodeA = a;
		Property nodeB = b;
		while (depthA > depthB)
		{
			nodeA = nodeA.Parent;
			depthA--;
		}
		while (depthB > depthA)
		{
			nodeB = nodeB.Parent;
			depthB--;
		}
		while (nodeA != nodeB && nodeA != null && nodeB != null)
		{
			nodeA = nodeA.Parent;
			nodeB = nodeB.Parent;
		}
		Property lowestCommonAncestor = nodeA;
		int depthFromA = 0;
		Property temp = a;
		while (temp != lowestCommonAncestor && temp != null)
		{
			depthFromA++;
			temp = temp.Parent;
		}
		int depthFromB = 0;
		temp = b;
		while (temp != lowestCommonAncestor && temp != null)
		{
			depthFromB++;
			temp = temp.Parent;
		}
		if (lowestCommonAncestor != null && depthFromA == 0 && depthFromB > 0)
		{
			return -1;
		}
		if (lowestCommonAncestor != null && depthFromB == 0 && depthFromA > 0)
		{
			return 1;
		}
		int num = Math.Min(depthFromA, depthFromB);
		Property walkA = a;
		Property walkB = b;
		for (int i = 0; i < num; i++)
		{
			walkA = walkA.Parent;
			walkB = walkB.Parent;
		}
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 = string.Compare(walkA.Descriptor.Name, walkB.Descriptor.Name);
			if (num2 != 0)
			{
				break;
			}
			walkA = walkA.Parent;
			walkB = walkB.Parent;
		}
		return num2;
	}

	private static int GetDepth(Property node)
	{
		int depth = 0;
		while (node.Parent != null)
		{
			depth++;
			node = node.Parent;
		}
		return depth;
	}

	private void BuildCategories()
	{
		Dictionary<string, List<Property>> categoryMap = new Dictionary<string, List<Property>>();
		List<string> categoryOrder = new List<string>();
		foreach (Property activeProperty in m_activeProperties)
		{
			string categoryName = PropertyUtils.GetCategoryName(activeProperty.Descriptor, LastSelectedObject);
			if (categoryName == null)
			{
				continue;
			}
			if (!categoryMap.TryGetValue(categoryName, out var list3))
			{
				categoryOrder.Add(categoryName);
				list3 = new List<Property>();
				categoryMap[categoryName] = list3;
			}
			list3.Add(activeProperty);
		}
		m_categories = new Category[categoryOrder.Count];
		for (int j = 0; j < categoryOrder.Count; j++)
		{
			m_categories[j] = BuildCategory(categoryMap[categoryOrder[j]], categoryOrder[j]);
		}
		if ((PropertySorting & PropertySorting.CategoryAlphabetical) != PropertySorting.None)
		{
			Array.Sort(m_categories, (Category x, Category y) => x.Name.CompareTo(y.Name));
		}
		if (EnableSubCategories)
		{
			m_categories = UnrollCategories(m_categories);
		}
		Category[] categories = m_categories;
		foreach (Category category in categories)
		{
			if (!m_categoryExpanded.ContainsKey(category.Name))
			{
				m_categoryExpanded.Add(category.Name, value: true);
			}
		}
	}

	private void BuildCategoryTree(IDictionary<string, Category> knownCats, Category cat, IList<Category> cats)
	{
		string[] array = cat.Name.Split(new char[1] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length != 0)
		{
			Category category = null;
			StringBuilder stringBuilder = new StringBuilder();
			string empty = string.Empty;
			for (int i = 0; i < array.Length - 1; i++)
			{
				empty = array[i];
				stringBuilder.Append(empty);
				category = BuildCategoryTreeNode(category, empty, stringBuilder.ToString(), cats, knownCats);
				stringBuilder.Append('\\');
			}
			empty = array[array.Length - 1];
			stringBuilder.Append(empty);
			BuildCategoryTreeNode(category, empty, stringBuilder.ToString(), cats, knownCats);
		}
	}

	private Category BuildCategoryTreeNode(Category parent, string catName, string catPath, IList<Category> allCats, IDictionary<string, Category> knownLeafCats)
	{
		Category category = allCats.FirstOrDefault((Category ac) => ac.Name.Equals(catPath));
		if (category == null)
		{
			category = new Category(this);
			category.Name = catPath;
			category.DisplayName = catName;
			category.Parent = parent;
			Category value = null;
			if (knownLeafCats.TryGetValue(catPath.ToString(), out value))
			{
				category.Properties = value.Properties;
			}
			else
			{
				category.Properties = Enumerable.Empty<Property>().ToArray();
			}
			Property[] properties = category.Properties;
			foreach (Property property in properties)
			{
				property.Category = category;
			}
			if (category.Properties.Length != 0)
			{
				category.Properties[0].FirstInCategory = true;
			}
			allCats.Add(category);
		}
		return category;
	}

	private Category[] UnrollCategories(Category[] sortedCats)
	{
		IDictionary<string, Category> dictionary = new Dictionary<string, Category>();
		List<Category> list = new List<Category>();
		foreach (Category category in sortedCats)
		{
			dictionary[category.Name] = category;
		}
		foreach (Category cat in sortedCats)
		{
			BuildCategoryTree(dictionary, cat, list);
		}
		return list.ToArray();
	}

	private Category BuildCategory(List<Property> properties, string currentName)
	{
		Category category = new Category(this);
		category.Name = currentName;
		if (currentName.Contains('\\'))
		{
			string[] array = currentName.Split(new char[1] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
			category.DisplayName = array[array.Length - 1];
		}
		else
		{
			category.DisplayName = currentName;
		}
		category.Properties = properties.ToArray();
		foreach (Property property in properties)
		{
			property.Category = category;
		}
		if (properties.Count > 0)
		{
			properties[0].FirstInCategory = true;
		}
		return category;
	}

	protected Property GetPreviousProperty(Property property)
	{
		Property result = null;
		foreach (Property property2 in Properties)
		{
			if (property2.Visible)
			{
				if (property2 == property)
				{
					return result;
				}
				result = property2;
			}
		}
		return null;
	}

	protected Property GetNextProperty(Property property)
	{
		Property property2 = null;
		foreach (Property property3 in Properties)
		{
			if (property3.Visible)
			{
				if (property2 != null)
				{
					return property3;
				}
				if (property3 == property)
				{
					property2 = property3;
				}
			}
		}
		return null;
	}

	protected Property GetPreviousEditableProperty(Property property)
	{
		Property previousProperty = GetPreviousProperty(property);
		while (previousProperty != null && previousProperty.DisableEditing)
		{
			previousProperty = GetPreviousProperty(previousProperty);
		}
		return previousProperty;
	}

	protected Property GetNextEditableProperty(Property property)
	{
		Property nextProperty = GetNextProperty(property);
		while (nextProperty != null && nextProperty.DisableEditing)
		{
			nextProperty = GetNextProperty(nextProperty);
		}
		return nextProperty;
	}

	protected Property GetFirstProperty()
	{
		foreach (Property property in Properties)
		{
			if (!property.Visible)
			{
				continue;
			}
			return property;
		}
		return null;
	}

	protected Property GetLastProperty()
	{
		foreach (Property item in Properties.Reverse())
		{
			if (!item.Visible)
			{
				continue;
			}
			return item;
		}
		return null;
	}

	protected void AddProperty(Property p)
	{
		m_activeProperties.Add(p);
	}

	protected bool RemoveProperty(Property p)
	{
		return m_activeProperties.Remove(p);
	}

	protected void InsertProperty(int index, Property p)
	{
		m_activeProperties.Insert(index, p);
	}

	static PropertyView()
	{
		EnableSubCategories = true;
		SystemDragSize = SystemInformation.DragSize;
		LeftStringFormat = new StringFormat();
		LeftStringFormat.Alignment = StringAlignment.Near;
		LeftStringFormat.Trimming = StringTrimming.EllipsisCharacter;
		LeftStringFormat.FormatFlags = StringFormatFlags.NoWrap;
		RightStringFormat = new StringFormat();
		RightStringFormat.Alignment = StringAlignment.Far;
		RightStringFormat.Trimming = StringTrimming.EllipsisCharacter;
		RightStringFormat.FormatFlags = StringFormatFlags.NoWrap;
	}
}
