using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Sce.Atf.Wpf.Docking;

public class TabLayout : TabControl, IDockContent, INotifyPropertyChanged, IDockLayout, IXmlSerializable
{
	private DateTime m_timerTime;

	private System.Timers.Timer m_timer;

	private TabItem m_lastItemOver;

	private static readonly string s_headerPropertyName;

	private static readonly string s_iconPropertyName;

	private static readonly string s_isFocusedPropertyName;

	public static DependencyProperty HeaderProperty;

	public static DependencyProperty IconProperty;

	public static DependencyProperty ItemsCountProperty;

	public static DependencyProperty ItemStyleProperty;

	private TabItem m_movingTabItem;

	private Point m_dragStartPosition;

	public static DependencyProperty ChildrenProperty;

	private bool m_isFocused;

	public int ItemsCount
	{
		get
		{
			return (int)GetValue(ItemsCountProperty);
		}
		set
		{
			SetValue(ItemsCountProperty, value);
		}
	}

	public Style ItemStyle
	{
		get
		{
			return (Style)GetValue(ItemStyleProperty);
		}
		set
		{
			SetValue(ItemStyleProperty, value);
		}
	}

	public ObservableCollection<DockContent> Children
	{
		get
		{
			return (ObservableCollection<DockContent>)GetValue(ChildrenProperty);
		}
		set
		{
			SetValue(ChildrenProperty, value);
		}
	}

	public DockPanel Root { get; private set; }

	public string Header
	{
		get
		{
			return (string)GetValue(HeaderProperty);
		}
		set
		{
			SetValue(HeaderProperty, value);
		}
	}

	public object Icon
	{
		get
		{
			return GetValue(IconProperty);
		}
		set
		{
			SetValue(IconProperty, value);
		}
	}

	public string UID => string.Empty;

	public object Content => this;

	bool IDockContent.IsVisible => true;

	bool IDockContent.IsFocused => m_isFocused;

	public event ContentClosedEvent Closing;

	private event EventHandler<BooleanArgs> m_isVisibleChanged;

	event EventHandler<BooleanArgs> IDockContent.IsVisibleChanged
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			m_isVisibleChanged += value;
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			m_isVisibleChanged -= value;
		}
	}

	private event EventHandler<BooleanArgs> m_isFocusedChanged;

	event EventHandler<BooleanArgs> IDockContent.IsFocusedChanged
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			m_isFocusedChanged += value;
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			m_isFocusedChanged -= value;
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	static TabLayout()
	{
		s_headerPropertyName = TypeUtil.GetProperty((DockContent x) => x.Header).Name;
		s_iconPropertyName = TypeUtil.GetProperty((DockContent x) => x.Icon).Name;
		s_isFocusedPropertyName = TypeUtil.GetProperty((DockContent x) => x.IsFocused).Name;
		HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(TabLayout));
		IconProperty = DependencyProperty.Register("Icon", typeof(object), typeof(TabLayout));
		ItemsCountProperty = DependencyProperty.Register("ItemsCount", typeof(int), typeof(TabLayout));
		ItemStyleProperty = DependencyProperty.Register("ItemStyle", typeof(Style), typeof(TabLayout));
		ChildrenProperty = DependencyProperty.Register("Children", typeof(ObservableCollection<DockContent>), typeof(TabLayout));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(TabLayout), new FrameworkPropertyMetadata(typeof(TabLayout)));
	}

	public TabLayout(DockPanel dockPanel)
	{
		Root = dockPanel;
		Header = string.Empty;
		Children = new ObservableCollection<DockContent>();
		ItemsCount = 0;
		base.SizeChanged += TabLayout_SizeChanged;
		base.MouseMove += TabControl_MouseMove;
		base.MouseUp += TabControl_MouseUp;
		base.MouseLeave += TabControl_MouseLeave;
		base.ItemsSource = Children;
		m_timer = new System.Timers.Timer();
		m_timer.AutoReset = false;
		m_timer.Interval = 500.0;
		m_timer.Elapsed += Timer_Elapsed;
	}

	private void Timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		if (base.Dispatcher.Thread != Thread.CurrentThread)
		{
			base.Dispatcher.BeginInvoke(new EventHandler<ElapsedEventArgs>(Timer_Elapsed), sender, e);
		}
		else if (m_lastItemOver != null)
		{
			Point position = Win32Calls.GetPosition(m_lastItemOver);
			if (new Rect(0.0, 0.0, m_lastItemOver.ActualWidth, m_lastItemOver.ActualHeight).Contains(position))
			{
				base.SelectedItem = m_lastItemOver.Content;
			}
		}
	}

	public TabLayout(DockPanel dockPanel, XmlReader reader)
		: this(dockPanel)
	{
		ReadXml(reader);
	}

	protected override void OnSelectionChanged(SelectionChangedEventArgs e)
	{
		base.OnSelectionChanged(e);
		foreach (DockContent child in Children)
		{
			if (child != base.SelectedItem)
			{
				if (child.IsFocused)
				{
					child.IsFocused = false;
				}
			}
			else if (!child.IsFocused)
			{
				child.IsFocused = true;
			}
		}
		UpdateIconAndHeader();
	}

	protected override DependencyObject GetContainerForItemOverride()
	{
		TabItem tabItem = new TabItem();
		if (ItemStyle != null)
		{
			tabItem.Style = ItemStyle;
		}
		tabItem.PreviewMouseDown += TabControl_PreviewMouseDown;
		tabItem.AllowDrop = true;
		tabItem.DragEnter += TabItem_DragEnter;
		return tabItem;
	}

	private void TabItem_DragEnter(object sender, DragEventArgs e)
	{
		e.Effects = DragDropEffects.Scroll & e.AllowedEffects;
		e.Handled = true;
		m_lastItemOver = (TabItem)sender;
		m_timerTime = DateTime.Now;
		m_timer.Start();
	}

	private void TabLayout_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		foreach (DockContent child in Children)
		{
			if (child.Settings.DockState == DockState.Docked || child.Settings.DockState == DockState.Collapsed)
			{
				Point point = Root.PointFromScreen(PointToScreen(new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0)));
				child.Settings.Location = new Point(point.X / Root.ActualWidth, point.Y / Root.ActualHeight);
				child.Settings.Size = new Size(base.ActualWidth, base.ActualHeight);
			}
		}
	}

	private void AddOneItem(DockContent toItem, DockContent content)
	{
		if (toItem != null && !Children.Contains(toItem))
		{
			throw new ArgumentOutOfRangeException();
		}
		int index = ((toItem != null) ? Children.IndexOf(toItem) : Children.Count);
		content.PropertyChanged += Content_PropertyChanged;
		Children.Insert(index, content);
		ItemsCount = Children.Count;
		base.SelectedIndex = base.Items.Count - 1;
		content.IsVisible = true;
		UpdateIconAndHeader();
	}

	private void Content_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == s_headerPropertyName || e.PropertyName == s_iconPropertyName)
		{
			UpdateIconAndHeader();
		}
		if (!(e.PropertyName == s_isFocusedPropertyName))
		{
			return;
		}
		DockContent dockContent = null;
		foreach (DockContent child in Children)
		{
			if (child.IsFocused)
			{
				dockContent = child;
				break;
			}
		}
		bool flag = dockContent != null;
		if (dockContent != null)
		{
			base.SelectedItem = dockContent;
		}
		if (m_isFocused != flag)
		{
			m_isFocused = flag;
			if (this.m_isFocusedChanged != null)
			{
				this.m_isFocusedChanged(this, new BooleanArgs(m_isFocused));
			}
		}
	}

	public void RemoveItem(DockContent item)
	{
		item.PropertyChanged -= Content_PropertyChanged;
		Children.Remove(item);
		ItemsCount = Children.Count;
		item.IsVisible = false;
		item.IsFocused = false;
		UpdateIconAndHeader();
	}

	private void UpdateIconAndHeader()
	{
		DockContent activeContent = GetActiveContent();
		if (activeContent != null)
		{
			Header = activeContent.Header;
			Icon = activeContent.Icon;
		}
	}

	private void TabControl_PreviewMouseDown(object sender, MouseEventArgs e)
	{
		if (ItemsCount > 1 && e.Source is TabItem movingTabItem && e.LeftButton == MouseButtonState.Pressed)
		{
			m_movingTabItem = movingTabItem;
			m_dragStartPosition = e.GetPosition(this);
		}
	}

	private void TabControl_MouseMove(object sender, MouseEventArgs e)
	{
		if (m_movingTabItem != null && e.LeftButton == MouseButtonState.Pressed)
		{
			Point position = e.GetPosition(this);
			Vector vector = m_dragStartPosition - position;
			if (Math.Abs(vector.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(vector.Y) > SystemParameters.MinimumVerticalDragDistance)
			{
				TabItem movingTabItem = m_movingTabItem;
				m_movingTabItem = null;
				Root.Drag(this, (IDockContent)movingTabItem.Content);
			}
		}
	}

	private void TabControl_MouseUp(object sender, MouseButtonEventArgs e)
	{
		m_movingTabItem = null;
	}

	private void TabControl_MouseLeave(object sender, MouseEventArgs e)
	{
		if (m_movingTabItem != null && e.LeftButton == MouseButtonState.Pressed)
		{
			TabItem movingTabItem = m_movingTabItem;
			m_movingTabItem = null;
			Root.Drag(this, (IDockContent)movingTabItem.Content);
		}
	}

	private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (base.SelectedIndex < 0)
		{
			base.SelectedIndex = 0;
		}
		UpdateIconAndHeader();
	}

	public void Activate()
	{
		if (base.SelectedItem is FrameworkElement frameworkElement)
		{
			frameworkElement.Focus();
		}
	}

	public void Deactivate()
	{
	}

	public DockContent GetActiveContent()
	{
		return (base.SelectedItem != null) ? ((DockContent)base.SelectedItem) : null;
	}

	public void OnClose(object sender, ContentClosedEventArgs args)
	{
		if (args.ContentToClose == ContentToClose.Current && Children.Count > 1)
		{
			DockContent dockContent = (DockContent)base.SelectedItem;
			RemoveItem(dockContent);
			dockContent.OnClose(dockContent, new ContentClosedEventArgs(ContentToClose.All));
			return;
		}
		while (Children.Count > 0)
		{
			DockContent dockContent2 = Children[0];
			RemoveItem(dockContent2);
			dockContent2.OnClose(dockContent2, new ContentClosedEventArgs(ContentToClose.All));
		}
		if (Children.Count == 0 && this.Closing != null)
		{
			this.Closing(this, new ContentClosedEventArgs(ContentToClose.Current));
		}
	}

	public XmlSchema GetSchema()
	{
		throw new NotImplementedException();
	}

	public void ReadXml(XmlReader reader)
	{
		reader.Read();
		if (!(reader.LocalName == GetType().Name) && !(reader.LocalName == "MultiContent"))
		{
			return;
		}
		string attribute = reader.GetAttribute("SelectedUID");
		DockContent dockContent = null;
		reader.ReadStartElement();
		if (reader.LocalName == "Content")
		{
			do
			{
				string attribute2 = reader.GetAttribute("UCID");
				DockContent content = Root.GetContent(attribute2);
				if (content != null)
				{
					AddOneItem(null, content);
					if (attribute == attribute2)
					{
						dockContent = content;
					}
				}
			}
			while (reader.ReadToNextSibling("Content"));
			if (dockContent != null)
			{
				base.SelectedItem = dockContent;
			}
		}
		reader.ReadEndElement();
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteStartElement(GetType().Name);
		writer.WriteAttributeString("SelectedUID", ((DockContent)base.SelectedItem).UID);
		foreach (DockContent child in Children)
		{
			writer.WriteStartElement("Content");
			writer.WriteAttributeString("UCID", child.UID);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}

	public DockContent HitTest(Point position)
	{
		Rect rect = new Rect(0.0, 0.0, base.ActualWidth, base.ActualHeight);
		Point point = PointFromScreen(position);
		if (rect.Contains(point))
		{
			return (DockContent)base.SelectedItem;
		}
		return null;
	}

	public bool HasChild(IDockContent content)
	{
		return Children.Any((DockContent x) => x == content);
	}

	public bool HasDescendant(IDockContent content)
	{
		return HasChild(content);
	}

	public void Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo)
	{
		if (newContent is TabLayout tabLayout)
		{
			IEnumerator<DockContent> enumerator = tabLayout.Children.GetEnumerator();
			while (enumerator.MoveNext())
			{
				DockContent current = enumerator.Current;
				tabLayout.RemoveItem(current);
				AddOneItem((DockContent)nextTo, current);
				enumerator = tabLayout.Children.GetEnumerator();
			}
		}
		else
		{
			AddOneItem((DockContent)nextTo, newContent as DockContent);
		}
		Focus();
		UpdateLayout();
	}

	public void Undock(IDockContent content)
	{
		foreach (DockContent child in Children)
		{
			if (content == child)
			{
				RemoveItem(content as DockContent);
				if (base.Parent is IDockLayout && Children.Count == 0)
				{
					((IDockLayout)base.Parent).Undock((IDockLayout)this);
				}
				break;
			}
		}
	}

	public void Undock(IDockLayout child)
	{
		throw new NotImplementedException();
	}

	public void Replace(IDockLayout oldLayout, IDockLayout newLayout)
	{
	}

	public void Close()
	{
		while (Children.Count > 0)
		{
			RemoveItem(Children[0]);
		}
		ItemsCount = 0;
	}

	IDockLayout IDockLayout.FindParentLayout(IDockContent content)
	{
		return (content is DockContent && Children.Contains((DockContent)content)) ? this : null;
	}
}
