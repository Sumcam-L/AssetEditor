using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using System.Xml;
using Sce.Atf.Adaptation;
using Sce.Atf.Collections;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class PropertyGrid : Control, IDisposable
{
	public static readonly DependencyProperty InstancesProperty;

	public static readonly DependencyProperty CustomPropertyDescriptorsProperty;

	public static readonly DependencyProperty EditorTemplateSelectorProperty;

	public static readonly DependencyProperty PropertiesProperty;

	public static readonly DependencyProperty HeaderPropertyProperty;

	public static readonly DependencyProperty TransactionContextProperty;

	public static readonly DependencyProperty SelectedPropertyProperty;

	public static readonly DependencyProperty SortingProperty;

	public static readonly DependencyProperty GroupingProperty;

	public static readonly DependencyProperty ToolBarStyleProperty;

	public static readonly DependencyProperty PropertyDetailsVisibilityProperty;

	public static readonly DependencyProperty LabelWidthProperty;

	public static readonly DependencyProperty ListBoxItemsPanelProperty;

	public static readonly DependencyProperty ListBoxItemContainerStyleProperty;

	public static readonly DependencyProperty PropertyFactoryProperty;

	private bool m_disposed;

	private ChangeListener m_listener;

	private readonly DispatcherTimer m_bindingUpdateTimer;

	private readonly Dictionary<string, bool> m_categoryExpanded = new Dictionary<string, bool>();

	public IEnumerable Instances
	{
		get
		{
			return (IEnumerable)GetValue(InstancesProperty);
		}
		set
		{
			SetValue(InstancesProperty, value);
		}
	}

	public IEnumerable<PropertyDescriptor> CustomPropertyDescriptors
	{
		get
		{
			return (IEnumerable<PropertyDescriptor>)GetValue(CustomPropertyDescriptorsProperty);
		}
		set
		{
			SetValue(CustomPropertyDescriptorsProperty, value);
		}
	}

	public DataTemplateSelector EditorTemplateSelector
	{
		get
		{
			return (DataTemplateSelector)GetValue(EditorTemplateSelectorProperty);
		}
		set
		{
			SetValue(EditorTemplateSelectorProperty, value);
		}
	}

	public IEnumerable<PropertyNode> Properties
	{
		get
		{
			return (IEnumerable<PropertyNode>)GetValue(PropertiesProperty);
		}
		set
		{
			SetValue(PropertiesProperty, value);
		}
	}

	public PropertyNode HeaderProperty
	{
		get
		{
			return (PropertyNode)GetValue(HeaderPropertyProperty);
		}
		set
		{
			SetValue(HeaderPropertyProperty, value);
		}
	}

	public object TransactionContext
	{
		get
		{
			return GetValue(TransactionContextProperty);
		}
		set
		{
			SetValue(TransactionContextProperty, value);
		}
	}

	public PropertyNode SelectedProperty
	{
		get
		{
			return (PropertyNode)GetValue(SelectedPropertyProperty);
		}
		set
		{
			SetValue(SelectedPropertyProperty, value);
		}
	}

	public IComparer Sorting
	{
		get
		{
			return (IComparer)GetValue(SortingProperty);
		}
		set
		{
			SetValue(SortingProperty, value);
		}
	}

	public GroupDescription Grouping
	{
		get
		{
			return (GroupDescription)GetValue(GroupingProperty);
		}
		set
		{
			SetValue(GroupingProperty, value);
		}
	}

	public Style ToolBarStyle
	{
		get
		{
			return (Style)GetValue(ToolBarStyleProperty);
		}
		set
		{
			SetValue(ToolBarStyleProperty, value);
		}
	}

	public Visibility PropertyDetailsVisibility
	{
		get
		{
			return (Visibility)GetValue(PropertyDetailsVisibilityProperty);
		}
		set
		{
			SetValue(PropertyDetailsVisibilityProperty, value);
		}
	}

	public double LabelWidth
	{
		get
		{
			return (double)GetValue(LabelWidthProperty);
		}
		set
		{
			SetValue(LabelWidthProperty, value);
		}
	}

	public ItemsPanelTemplate ListBoxItemsPanel
	{
		get
		{
			return (ItemsPanelTemplate)GetValue(ListBoxItemsPanelProperty);
		}
		set
		{
			SetValue(ListBoxItemsPanelProperty, value);
		}
	}

	public Style ListBoxItemContainerStyle
	{
		get
		{
			return (Style)GetValue(ListBoxItemContainerStyleProperty);
		}
		set
		{
			SetValue(ListBoxItemContainerStyleProperty, value);
		}
	}

	public IPropertyFactory PropertyFactory
	{
		get
		{
			return (IPropertyFactory)GetValue(PropertyFactoryProperty);
		}
		set
		{
			SetValue(PropertyFactoryProperty, value);
		}
	}

	public static ComponentResourceKey PropertyNameTemplateKey => new ComponentResourceKey(typeof(PropertyGrid), "PropertyNameTemplate");

	public static ComponentResourceKey ReadOnlyStyleKey => new ComponentResourceKey(typeof(PropertyGrid), "ReadOnlyStyle");

	public static ComponentResourceKey ReadOnlyTemplateKey => new ComponentResourceKey(typeof(PropertyGrid), "ReadOnlyTemplate");

	public static ComponentResourceKey DefaultTextEditorStyleKey => new ComponentResourceKey(typeof(PropertyGrid), "DefaultTextEditorStyle");

	public static ComponentResourceKey DefaultTextEditorTemplateKey => new ComponentResourceKey(typeof(PropertyGrid), "DefaultTextEditorTemplate");

	public static ComponentResourceKey ComboEditorStyleKey => new ComponentResourceKey(typeof(PropertyGrid), "ComboEditorStyle");

	public static ComponentResourceKey ComboEditorTemplateKey => new ComponentResourceKey(typeof(PropertyGrid), "ComboEditorTemplate");

	public static ComponentResourceKey StandardValuesEditorTemplateKey => new ComponentResourceKey(typeof(PropertyGrid), "StandardValuesEditorTemplate");

	public static ComponentResourceKey BoolEditorStyleKey => new ComponentResourceKey(typeof(PropertyGrid), "BoolEditorStyle");

	public static ComponentResourceKey BoolEditorTemplateKey => new ComponentResourceKey(typeof(PropertyGrid), "BoolEditorTemplate");

	public string Settings
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", Encoding.UTF8.WebName, "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("PropertyView");
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
			XmlNodeList xmlNodeList = documentElement.SelectNodes("Category");
			if (xmlNodeList != null)
			{
				foreach (XmlElement item in xmlNodeList)
				{
					string attribute = item.GetAttribute("Name");
					string attribute2 = item.GetAttribute("Expanded");
					if (attribute2 != null && bool.TryParse(attribute2, out var result))
					{
						m_categoryExpanded[attribute] = result;
					}
				}
			}
			ReadSettings(documentElement);
		}
	}

	public ObservableCollection<ValueEditor> Editors { get; set; }

	public event EventHandler<PropertyErrorEventArgs> PropertyError;

	public event EventHandler<PropertyEditedEventArgs> PropertyEdited;

	private static void InstancesPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		if (s is PropertyGrid propertyGrid)
		{
			propertyGrid.GetBindingExpression(CustomPropertyDescriptorsProperty)?.UpdateTarget();
			propertyGrid.InstancesOrPropertiesChanged(e.OldValue as IEnumerable);
		}
	}

	private static void CustomPropertyDescriptorsPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		if (s is PropertyGrid propertyGrid)
		{
			BindingExpression bindingExpression = propertyGrid.GetBindingExpression(InstancesProperty);
			bindingExpression.UpdateTarget();
			propertyGrid.InstancesOrPropertiesChanged(e.OldValue as IEnumerable);
		}
	}

	private static void PropertiesPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		if (s is PropertyGrid propertyGrid)
		{
			propertyGrid.SelectedProperty = null;
			propertyGrid.SetGrouping();
			propertyGrid.SetSorting();
		}
	}

	private static void TransactionContextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
	}

	private static void SortingPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		if (s is PropertyGrid propertyGrid)
		{
			propertyGrid.SetSorting();
		}
	}

	private void SetSorting()
	{
		if (CollectionViewSource.GetDefaultView(Properties) is ListCollectionView listCollectionView)
		{
			listCollectionView.CustomSort = Sorting;
		}
	}

	private static void GroupingPropertyChanged(DependencyObject s, DependencyPropertyChangedEventArgs e)
	{
		if (s is PropertyGrid propertyGrid)
		{
			propertyGrid.SetGrouping();
		}
	}

	private void SetGrouping()
	{
		if (Properties != null)
		{
			ICollectionView defaultView = CollectionViewSource.GetDefaultView(Properties);
			defaultView.GroupDescriptions.Clear();
			if (Grouping != null)
			{
				defaultView.GroupDescriptions.Add(Grouping);
			}
		}
	}

	protected virtual void OnPropertyError(PropertyErrorEventArgs e)
	{
		this.PropertyError.Raise(this, e);
	}

	protected virtual void OnPropertyEdited(PropertyEditedEventArgs e)
	{
		this.PropertyEdited.Raise(this, e);
	}

	protected virtual void ReadSettings(XmlElement root)
	{
	}

	protected virtual void WriteSettings(XmlElement root)
	{
	}

	static PropertyGrid()
	{
		InstancesProperty = DependencyProperty.Register("Instances", typeof(IEnumerable), typeof(PropertyGrid), new FrameworkPropertyMetadata(InstancesPropertyChanged));
		CustomPropertyDescriptorsProperty = DependencyProperty.Register("CustomPropertyDescriptors", typeof(IEnumerable<PropertyDescriptor>), typeof(PropertyGrid), new FrameworkPropertyMetadata(CustomPropertyDescriptorsPropertyChanged));
		EditorTemplateSelectorProperty = DependencyProperty.Register("EditorTemplateSelector", typeof(DataTemplateSelector), typeof(PropertyGrid), new UIPropertyMetadata(null));
		PropertiesProperty = DependencyProperty.Register("Properties", typeof(IEnumerable<PropertyNode>), typeof(PropertyGrid), new FrameworkPropertyMetadata(PropertiesPropertyChanged));
		HeaderPropertyProperty = DependencyProperty.Register("HeaderProperty", typeof(PropertyNode), typeof(PropertyGrid), new UIPropertyMetadata(null));
		TransactionContextProperty = DependencyProperty.Register("TransactionContext", typeof(object), typeof(PropertyGrid), new PropertyMetadata(null, TransactionContextPropertyChanged));
		SelectedPropertyProperty = DependencyProperty.Register("SelectedProperty", typeof(PropertyNode), typeof(PropertyGrid), new UIPropertyMetadata(null));
		SortingProperty = DependencyProperty.Register("Sorting", typeof(IComparer), typeof(PropertyGrid), new FrameworkPropertyMetadata(SortingPropertyChanged));
		GroupingProperty = DependencyProperty.Register("Grouping", typeof(GroupDescription), typeof(PropertyGrid), new FrameworkPropertyMetadata(GroupingPropertyChanged));
		ToolBarStyleProperty = DependencyProperty.Register("ToolBarStyle", typeof(Style), typeof(PropertyGrid));
		PropertyDetailsVisibilityProperty = DependencyProperty.Register("PropertyDetailsVisibility", typeof(Visibility), typeof(PropertyGrid), new UIPropertyMetadata(Visibility.Visible));
		LabelWidthProperty = DependencyProperty.Register("LabelWidth", typeof(double), typeof(PropertyGrid), new UIPropertyMetadata(100.0));
		ListBoxItemsPanelProperty = DependencyProperty.Register("ListBoxItemsPanel", typeof(ItemsPanelTemplate), typeof(PropertyGrid));
		ListBoxItemContainerStyleProperty = DependencyProperty.Register("ListBoxItemContainerStyle", typeof(Style), typeof(PropertyGrid));
		PropertyFactoryProperty = DependencyProperty.Register("PropertyFactory", typeof(IPropertyFactory), typeof(PropertyGrid), new UIPropertyMetadata(null));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid), new FrameworkPropertyMetadata(typeof(PropertyGrid)));
	}

	public PropertyGrid()
	{
		Editors = new ObservableCollection<ValueEditor>();
		EditorTemplateSelector = new EditorTemplateSelector(Editors);
		m_bindingUpdateTimer = new DispatcherTimer();
		m_bindingUpdateTimer.Tick += BindingUpdateTimer_Tick;
		m_bindingUpdateTimer.Interval = TimeSpan.FromMilliseconds(50.0);
		Grouping = DefaultPropertyGrouping.ByCategory;
		base.Loaded += PropertyGrid_Loaded;
		base.Unloaded += PropertyGrid_Unloaded;
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		DependencyObject templateChild = GetTemplateChild("PART_Selector");
		if (templateChild is TreeView treeView)
		{
			treeView.SelectedItemChanged += view_SelectedItemChanged;
		}
		else if (templateChild is Selector selector)
		{
			selector.SelectionChanged += selector_SelectionChanged;
		}
	}

	public void Refresh()
	{
		if (Properties == null)
		{
			return;
		}
		foreach (PropertyNode property in Properties)
		{
			property.Refresh();
		}
		if (HeaderProperty != null)
		{
			HeaderProperty.Refresh();
		}
	}

	private void DestroyPropertyNode(PropertyNode node)
	{
		node.ValueSet -= node_ValueSet;
		node.ValueError -= node_ValueError;
		node.Dispose();
	}

	protected virtual void RebuildPropertyNodesImpl()
	{
		DestroyPropertyNodes();
		object[] array = ((Instances == null) ? EmptyArray<object>.Instance : Instances.Cast<object>().ToArray());
		IEnumerable<PropertyDescriptor> enumerable = null;
		if (CustomPropertyDescriptors != null)
		{
			enumerable = CustomPropertyDescriptors;
		}
		else if (Instances != null)
		{
			enumerable = PropertyUtils.GetSharedPropertiesOriginal(array);
		}
		PropertyNode propertyNode = null;
		ObservableCollection<PropertyNode> observableCollection = new ObservableCollection<PropertyNode>();
		if (enumerable != null)
		{
			ITransactionContext transactionContext = TransactionContext.As<ITransactionContext>();
			if (transactionContext == null)
			{
				transactionContext = base.DataContext.As<ITransactionContext>();
			}
			foreach (PropertyDescriptor item in enumerable)
			{
				if (item.IsBrowsable)
				{
					PropertyNode propertyNode2;
					if (PropertyFactory != null)
					{
						propertyNode2 = PropertyFactory.CreateProperty(array, item, isEnumerable: true, transactionContext);
					}
					else
					{
						propertyNode2 = new PropertyNode();
						propertyNode2.Initialize(array, item, isEnumerable: true);
					}
					propertyNode2.ValueSet += node_ValueSet;
					propertyNode2.ValueError += node_ValueError;
					if (propertyNode2.Category != null && m_categoryExpanded.TryGetValue(propertyNode2.Category, out var value))
					{
						propertyNode2.IsExpanded = value;
					}
					if (propertyNode == null && item.Attributes[typeof(HeaderPropertyAttribute)] != null)
					{
						propertyNode = propertyNode2;
					}
					else
					{
						observableCollection.Add(propertyNode2);
					}
				}
			}
		}
		m_listener = ChangeListener.Create(observableCollection, "IsExpanded");
		m_listener.PropertyChanged += ChildExpandedPropertyChanged;
		Properties = observableCollection;
		HeaderProperty = propertyNode;
	}

	protected virtual void DestroyPropertyNodes()
	{
		if (m_listener != null)
		{
			m_listener.PropertyChanged -= ChildExpandedPropertyChanged;
			m_listener.Dispose();
			m_listener = null;
		}
		if (Properties != null)
		{
			foreach (PropertyNode property in Properties)
			{
				DestroyPropertyNode(property);
			}
			Properties = null;
		}
		if (HeaderProperty != null)
		{
			DestroyPropertyNode(HeaderProperty);
			HeaderProperty = null;
		}
	}

	private void RebuildPropertyNodes()
	{
		if (base.IsLoaded)
		{
			m_bindingUpdateTimer.Stop();
			m_bindingUpdateTimer.Start();
		}
	}

	private void ChildExpandedPropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (sender is PropertyNode propertyNode && !string.IsNullOrEmpty(propertyNode.Category))
		{
			m_categoryExpanded[propertyNode.Category] = propertyNode.IsExpanded;
		}
	}

	private void BindingUpdateTimer_Tick(object sender, EventArgs e)
	{
		m_bindingUpdateTimer.Stop();
		RebuildPropertyNodesImpl();
	}

	private void PropertyGrid_Loaded(object sender, RoutedEventArgs e)
	{
		RebuildPropertyNodes();
	}

	private void PropertyGrid_Unloaded(object sender, RoutedEventArgs e)
	{
		DestroyPropertyNodes();
		SelectedProperty = null;
	}

	private void node_ValueSet(object sender, EventArgs e)
	{
		PropertyNode propertyNode = sender as PropertyNode;
		OnPropertyEdited(new PropertyEditedEventArgs(propertyNode.Instance, propertyNode.Descriptor, propertyNode.OldValue, propertyNode.Value));
	}

	private void node_ValueError(object sender, EventArgs e)
	{
		PropertyNode propertyNode = sender as PropertyNode;
		OnPropertyError(new PropertyErrorEventArgs(propertyNode.Instance, propertyNode.Descriptor, propertyNode.PropertyValueError));
	}

	private void selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		PropertyNode propertyNode = ((Selector)sender).SelectedItem as PropertyNode;
		if (propertyNode != SelectedProperty)
		{
			SelectedProperty = propertyNode;
		}
	}

	private void view_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
	{
		PropertyNode propertyNode = e.NewValue as PropertyNode;
		if (propertyNode != SelectedProperty)
		{
			SelectedProperty = propertyNode;
		}
	}

	private void InstancesOrPropertiesChanged(IEnumerable oldInstances)
	{
		if (oldInstances is INotifyCollectionChanged notifyCollectionChanged)
		{
			notifyCollectionChanged.CollectionChanged -= Instances_CollectionChanged;
		}
		if (Instances is INotifyCollectionChanged notifyCollectionChanged2)
		{
			notifyCollectionChanged2.CollectionChanged += Instances_CollectionChanged;
		}
		RebuildPropertyNodes();
	}

	private void Instances_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		RebuildPropertyNodes();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposed)
		{
			return;
		}
		if (disposing)
		{
			IEnumerable<PropertyNode> properties = Properties;
			if (properties != null)
			{
				foreach (PropertyNode item in properties)
				{
					item.UnBind();
					item.ValueSet -= node_ValueSet;
					item.ValueError -= node_ValueError;
				}
			}
		}
		m_disposed = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
