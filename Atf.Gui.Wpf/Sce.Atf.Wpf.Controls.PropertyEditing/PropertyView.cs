using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public abstract class PropertyView : UserControl, IPropertyEditingControlOwner
{
	public class Property
	{
		public PropertyDescriptor Descriptor;

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
					return Category == null || (Category.Expanded && (Category.Parent == null || Category.Parent.Expanded));
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

		public Property[] Properties;

		public Category Parent;

		private PropertyView m_owner;

		public bool Expanded
		{
			get
			{
				return m_owner.m_categoryExpanded[Name];
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

		internal static bool IsSubCategoryName(string fullName, out string parentName, out string childName)
		{
			parentName = null;
			childName = null;
			int num = fullName.IndexOf('\\');
			if (num < 0)
			{
				return false;
			}
			parentName = fullName.Substring(0, num);
			childName = fullName.Substring(num + 1);
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

	public static bool EnableSubCategories = true;

	private string m_filterPattern;

	protected Property m_selectedProperty;

	private IPropertyEditingContext m_editingContext;

	private object[] m_selectedObjects = EmptyArray<object>.Instance;

	private PropertyDescriptor[] m_propertyDescriptors = EmptyArray<PropertyDescriptor>.Instance;

	private Category[] m_categories = EmptyArray<Category>.Instance;

	private readonly List<Property> m_activeProperties = new List<Property>();

	private readonly Dictionary<PropertyDescriptor, Property> m_cacheableProperties = new Dictionary<PropertyDescriptor, Property>();

	private PropertySorting m_propertySorting = PropertySorting.ByCategory;

	private readonly Dictionary<string, bool> m_categoryExpanded = new Dictionary<string, bool>();

	private readonly HashSet<PropertyDescriptor> m_processedDescriptors = new HashSet<PropertyDescriptor>();

	private List<string> m_customSortOrder;

	protected static Size SystemDragSize = new Size(SystemParameters.MinimumHorizontalDragDistance, SystemParameters.MinimumVerticalDragDistance);

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
				IObservableContext observableContext = m_editingContext.As<IObservableContext>();
				if (observableContext != null)
				{
					observableContext.ItemInserted -= observableContext_ItemInserted;
					observableContext.ItemRemoved -= observableContext_ItemRemoved;
					observableContext.ItemChanged -= observableContext_ItemChanged;
					observableContext.Reloaded -= observableContext_Reloaded;
				}
				ISelectionContext selectionContext = m_editingContext.As<ISubSelectionContext>()?.SubSelectionContext;
				if (selectionContext != null)
				{
					selectionContext.SelectionChanged -= subSelectionContext_SelectionChanged;
				}
			}
			OnEditingContextChanging();
			m_editingContext = value;
			m_selectedObjects = EmptyArray<object>.Instance;
			m_propertyDescriptors = EmptyArray<PropertyDescriptor>.Instance;
			if (m_editingContext != null)
			{
				m_selectedObjects = m_editingContext.Items.ToArray();
				m_propertyDescriptors = m_editingContext.PropertyDescriptors.ToArray();
				IObservableContext observableContext2 = m_editingContext.As<IObservableContext>();
				if (observableContext2 != null)
				{
					observableContext2.ItemInserted += observableContext_ItemInserted;
					observableContext2.ItemRemoved += observableContext_ItemRemoved;
					observableContext2.ItemChanged += observableContext_ItemChanged;
					observableContext2.Reloaded += observableContext_Reloaded;
				}
				ISelectionContext selectionContext2 = m_editingContext.As<ISubSelectionContext>()?.SubSelectionContext;
				if (selectionContext2 != null)
				{
					selectionContext2.SelectionChanged += subSelectionContext_SelectionChanged;
				}
			}
			UpdateEditingContext();
			OnEditingContextChanged();
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

	public PropertyDescriptor SelectedPropertyDescriptor => (m_selectedProperty != null) ? m_selectedProperty.Descriptor : null;

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
			InvalidateVisual();
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

	public event EventHandler SelectedPropertyChanged;

	public PropertyView()
	{
		base.AllowDrop = true;
		FilterPattern = "";
	}

	protected virtual void observableContext_Reloaded(object sender, EventArgs e)
	{
		IPropertyEditingContext editingContext = m_editingContext;
		if (editingContext == null)
		{
			return;
		}
		if (!m_selectedObjects.SequenceEqual(m_editingContext.Items) || !m_propertyDescriptors.SequenceEqual(m_editingContext.PropertyDescriptors))
		{
			EditingContext = null;
			EditingContext = editingContext;
		}
		else
		{
			InvalidateVisual();
			base.Dispatcher.InvokeIfRequired(delegate
			{
				RefreshEditingControls();
			});
		}
		OnSubSelectionChanged();
	}

	private void subSelectionContext_SelectionChanged(object sender, EventArgs e)
	{
		InvalidateVisual();
		RefreshEditingControls();
		OnSubSelectionChanged();
	}

	private void UpdateEditingContext()
	{
		using (base.Dispatcher.DisableProcessing())
		{
			base.Dispatcher.InvokeIfRequired(delegate
			{
				SelectedProperty = null;
				ClearCurrentProperties();
				if (base.Visibility == Visibility.Visible && m_editingContext != null)
				{
					BuildProperties();
				}
				UpdatePropertySorting();
				RefreshEditingControls();
				InvalidateVisual();
			});
		}
	}

	private void observableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
	{
		UpdateEditingContext();
	}

	private void observableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
	{
		UpdateEditingContext();
	}

	private void OnSubSelectionChanged()
	{
		ISelectionContext selectionContext = m_editingContext.As<ISubSelectionContext>()?.SubSelectionContext;
		if (selectionContext == null || selectionContext.SelectionCount <= 0)
		{
			return;
		}
		object[] array = selectionContext.Selection.ToArray();
		PropertyDescriptor b = array[0].As<PropertyDescriptor>();
		foreach (Property activeProperty in m_activeProperties)
		{
			if (PropertyUtils.PropertyDescriptorsEqual(activeProperty.Descriptor, b))
			{
				SelectedProperty = activeProperty;
			}
		}
	}

	private void observableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		InvalidateVisual();
		RefreshEditingControls();
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
			InvalidateVisual();
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
		InvalidateVisual();
	}

	public void RefreshProperties()
	{
		InvalidateVisual();
	}

	protected virtual void RefreshEditingControls()
	{
		foreach (Property property in Properties)
		{
			property.Control?.InvalidateVisual();
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
			InvalidateVisual();
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
		foreach (string propertyName in propertyNames)
		{
			Property property = null;
			foreach (Property item2 in list)
			{
				if (item2.Descriptor.Name.Equals(propertyName))
				{
					property = item2;
					AddProperty(item2);
					break;
				}
			}
			if (property != null)
			{
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
		InvalidateVisual();
	}

	private void BuildProperties()
	{
		List<PropertyDescriptor> list = new List<PropertyDescriptor>(m_editingContext.PropertyDescriptors);
		m_activeProperties.Clear();
		m_processedDescriptors.Clear();
		int index = 0;
		for (int i = 0; i < list.Count; i++)
		{
			PropertyDescriptor propertyDescriptor = list[i];
			if (propertyDescriptor.IsBrowsable && (FilterPattern.Length == 0 || propertyDescriptor.Name.ToLower().Contains(FilterPattern.ToLower())))
			{
				Property property = BuildProperty(propertyDescriptor, index++);
				bool reflected = propertyDescriptor.GetType().Name == "ReflectPropertyDescriptor";
				AddChildProperty(property, reflected, ref index);
			}
		}
		foreach (KeyValuePair<PropertyDescriptor, Property> cacheableProperty in m_cacheableProperties)
		{
			Control control = cacheableProperty.Value.Control;
			if (control != null && !m_activeProperties.Contains(cacheableProperty.Value))
			{
				control.Visibility = Visibility.Hidden;
			}
		}
	}

	private Property BuildProperty(PropertyDescriptor descriptor, int index)
	{
		ITransactionContext transactionContext = m_editingContext.As<ITransactionContext>();
		if (m_cacheableProperties.TryGetValue(descriptor, out var value))
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
		if (control != null)
		{
			control.Visibility = Visibility.Hidden;
			if (customizeAttribute != null)
			{
				control.Width = customizeAttribute.ColumnWidth;
			}
		}
		m_activeProperties.Add(value);
		m_processedDescriptors.Add(descriptor);
		return value;
	}

	private void AddChildProperty(Property property, bool reflected, ref int index)
	{
		List<PropertyDescriptor> list = new List<PropertyDescriptor>();
		foreach (PropertyDescriptor childProperty in property.Descriptor.GetChildProperties())
		{
			if ((!reflected || !m_processedDescriptors.Contains(childProperty)) && (!(childProperty.GetType().Name == "ReflectPropertyDescriptor") || reflected))
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

	private void ClearCurrentProperties()
	{
		foreach (Property activeProperty in m_activeProperties)
		{
			activeProperty.Context.TransactionContext = null;
			activeProperty.Context.ClearCachedSelection();
			Control control = activeProperty.Control;
			if (control != null && !activeProperty.Cacheable)
			{
				RemoveVisualChild(control);
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
		Property lowestCommonAncestor = GetLowestCommonAncestor(a, b);
		List<Property> list = new List<Property>();
		Property property = a;
		do
		{
			list.Add(property);
			if (property == lowestCommonAncestor)
			{
				break;
			}
			property = property.Parent;
		}
		while (property != null);
		List<Property> list2 = new List<Property>();
		property = b;
		do
		{
			list2.Add(property);
			if (property == lowestCommonAncestor)
			{
				break;
			}
			property = property.Parent;
		}
		while (property != null);
		if (lowestCommonAncestor != null && list.Count == 1 && list2.Count > 1)
		{
			return -1;
		}
		if (lowestCommonAncestor != null && list2.Count == 1 && list.Count > 1)
		{
			return 1;
		}
		int num = Math.Min(list.Count, list2.Count);
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			num2 = string.Compare(list[list.Count - i - 1].Descriptor.Name, list2[list2.Count - i - 1].Descriptor.Name);
			if (num2 != 0)
			{
				break;
			}
		}
		return num2;
	}

	private void BuildCategories()
	{
		List<string> list = new List<string>();
		List<List<Property>> list2 = new List<List<Property>>();
		foreach (Property activeProperty in m_activeProperties)
		{
			string categoryName = PropertyUtils.GetCategoryName(activeProperty.Descriptor);
			if (categoryName == null)
			{
				continue;
			}
			List<Property> list3 = null;
			for (int i = 0; i < list.Count; i++)
			{
				if (categoryName.Equals(list[i]))
				{
					list3 = list2[i];
					break;
				}
			}
			if (list3 == null)
			{
				list.Add(categoryName);
				list3 = new List<Property>();
				list2.Add(list3);
			}
			list3.Add(activeProperty);
		}
		m_categories = new Category[list.Count];
		for (int j = 0; j < list.Count; j++)
		{
			m_categories[j] = BuildCategory(list2[j], list[j]);
		}
		if ((PropertySorting & PropertySorting.CategoryAlphabetical) != PropertySorting.None)
		{
			Array.Sort(m_categories, (Category x, Category y) => x.Name.CompareTo(y.Name));
		}
		if (EnableSubCategories)
		{
			List<Category> list4 = new List<Category>(m_categories.Length);
			Category[] categories = m_categories;
			foreach (Category category in categories)
			{
				if (Category.IsSubCategoryName(category.Name, out var parentName, out var childName))
				{
					Category category2 = null;
					int num2 = list4.Count;
					while (--num2 >= 0)
					{
						if (list4[num2].Name.Equals(parentName))
						{
							category2 = list4[num2];
							break;
						}
					}
					if (category2 == null)
					{
						category2 = BuildCategory(new List<Property>(), parentName);
						list4.Add(category2);
					}
					category.Name = childName;
					category.Parent = category2;
				}
				list4.Add(category);
			}
			m_categories = list4.ToArray();
		}
		Category[] categories2 = m_categories;
		foreach (Category category3 in categories2)
		{
			if (!m_categoryExpanded.ContainsKey(category3.Name))
			{
				m_categoryExpanded.Add(category3.Name, value: true);
			}
		}
	}

	private Category BuildCategory(List<Property> properties, string currentName)
	{
		Category category = new Category(this);
		category.Name = currentName;
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
}
