using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Sce.Atf.Wpf.Docking;

public class DockedWindow : ContentControl, IDockLayout, IDockable, IXmlSerializable
{
	public static DependencyProperty DockedContentProperty;

	public static DependencyProperty FocusedProperty;

	public static DependencyProperty HeaderProperty;

	public static DependencyProperty ShowIconProperty;

	private DockIcon m_dockLeftIcon;

	private DockIcon m_dockRightIcon;

	private DockIcon m_dockTopIcon;

	private DockIcon m_dockBottomIcon;

	private DockIcon m_dockTabIcon;

	private FrameworkElement m_dockPreviewShape;

	private DockTo? m_dockPreview;

	private Point m_mouseClickPosition;

	private bool m_mouseClickInside;

	private Button PART_CloseButton;

	private ToggleButton PART_CollapseButton;

	private bool m_isCollapsed;

	public bool ShowIcon
	{
		get
		{
			return (bool)GetValue(ShowIconProperty);
		}
		set
		{
			SetValue(ShowIconProperty, value);
		}
	}

	public TabLayout DockedContent
	{
		get
		{
			return (TabLayout)GetValue(DockedContentProperty);
		}
		private set
		{
			SetValue(DockedContentProperty, value);
		}
	}

	public bool IsCollapsed
	{
		get
		{
			return PART_CollapseButton.IsChecked.HasValue && PART_CollapseButton.IsChecked.Value;
		}
		set
		{
			m_isCollapsed = value;
			if (PART_CollapseButton != null)
			{
				PART_CollapseButton.IsChecked = value;
			}
		}
	}

	public bool Focused
	{
		get
		{
			return (bool)GetValue(FocusedProperty);
		}
		set
		{
			SetValue(FocusedProperty, value);
		}
	}

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

	public DockPanel Root { get; private set; }

	public DockTo? DockPreview => m_dockPreview;

	public event EventHandler Closing;

	static DockedWindow()
	{
		DockedContentProperty = DependencyProperty.Register("DockedContent", typeof(TabLayout), typeof(DockedWindow), new UIPropertyMetadata(DockedContentPropertyChanged));
		FocusedProperty = DependencyProperty.Register("Focused", typeof(bool), typeof(DockedWindow), new FrameworkPropertyMetadata(FocusedPropertyChanged));
		HeaderProperty = DependencyProperty.Register("Header", typeof(string), typeof(DockedWindow));
		ShowIconProperty = DependencyProperty.Register("ShowIcon", typeof(bool), typeof(DockedWindow));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DockedWindow), new FrameworkPropertyMetadata(typeof(DockedWindow)));
	}

	public DockedWindow(DockPanel dockPanel, IDockContent content)
	{
		Root = dockPanel;
		DockedContent = content as TabLayout;
		if (DockedContent == null)
		{
			DockedContent = new TabLayout(Root);
			DockedContent.Dock(null, content, DockTo.Center);
		}
		base.Content = DockedContent;
		ShowIcon = DockedContent.Icon != null && (Root.IconVisibility & IconVisibility.Header) == IconVisibility.Header;
		DockedContent.Closing += Content_Closing;
		base.PreviewMouseDown += DockedWindow_PreviewMouseDown;
		Focused = ((IDockContent)DockedContent).IsFocused;
	}

	public DockedWindow(DockPanel dockPanel, XmlReader reader)
	{
		Root = dockPanel;
		ReadXml(reader);
		ShowIcon = DockedContent.Icon != null && (Root.IconVisibility & IconVisibility.Header) == IconVisibility.Header;
		DockedContent.Closing += Content_Closing;
		base.PreviewMouseDown += DockedWindow_PreviewMouseDown;
		Focused = ((IDockContent)DockedContent).IsFocused;
	}

	private static void FocusedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TabLayout dockedContent = ((DockedWindow)d).DockedContent;
		if (dockedContent != null)
		{
			if ((bool)e.NewValue)
			{
				dockedContent.Activate();
			}
			else
			{
				dockedContent.Deactivate();
			}
		}
	}

	private void DockedWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		Root.Focus((IDockContent)DockedContent.SelectedItem);
	}

	private void Content_Closing(object sender, ContentClosedEventArgs args)
	{
		if (base.Parent is IDockLayout)
		{
			((IDockLayout)base.Parent).Undock((IDockContent)DockedContent);
		}
		Close();
	}

	public IDockContent GetActiveContent()
	{
		return (DockedContent != null) ? DockedContent.GetActiveContent() : null;
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		FrameworkElement frameworkElement = (FrameworkElement)base.Template.FindName("PART_TitleBar", this);
		frameworkElement.MouseDown += TitleBarMouseDown;
		frameworkElement.MouseMove += TitleBarMouseMove;
		PART_CloseButton = (Button)base.Template.FindName("PART_CloseButton", this);
		PART_CloseButton.Click += CloseButton_Click;
		PART_CollapseButton = (ToggleButton)base.Template.FindName("PART_CollapseButton", this);
		PART_CollapseButton.Click += CollapseButton_Click;
		PART_CollapseButton.IsChecked = m_isCollapsed;
	}

	private void CollapseButton_Click(object sender, RoutedEventArgs e)
	{
		if (m_isCollapsed)
		{
			Root.UnCollapse(DockedContent);
		}
		else
		{
			Root.Collapse(DockedContent);
		}
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		if (this.Closing != null)
		{
			this.Closing(this, EventArgs.Empty);
		}
		if (DockedContent != null)
		{
			DockedContent.OnClose(this, new ContentClosedEventArgs(ContentToClose.Current));
		}
	}

	private void TitleBarMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed && !IsCollapsed)
		{
			m_mouseClickInside = true;
			m_mouseClickPosition = e.GetPosition(this);
			e.Handled = true;
		}
	}

	private void TitleBarMouseMove(object sender, MouseEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed && m_mouseClickInside && !IsCollapsed)
		{
			Point position = e.GetPosition(this);
			Vector vector = m_mouseClickPosition - position;
			if (Math.Abs(vector.X) > SystemParameters.MinimumHorizontalDragDistance || Math.Abs(vector.Y) > SystemParameters.MinimumVerticalDragDistance)
			{
				m_mouseClickInside = false;
				e.Handled = true;
				Root.Drag(this, DockedContent);
			}
		}
		else
		{
			m_mouseClickInside = false;
		}
	}

	public static void DockedContentPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
	{
		DockedWindow dockedWindow = null;
		if (sender is DockedWindow dockedWindow2)
		{
			if (args.OldValue is IDockContent dockContent)
			{
				dockContent.IsFocusedChanged -= dockedWindow2.DockContent_IsFocusChanged;
			}
			if (args.NewValue is IDockContent dockContent2)
			{
				dockContent2.IsFocusedChanged += dockedWindow2.DockContent_IsFocusChanged;
			}
		}
	}

	private void DockContent_IsFocusChanged(object sender, BooleanArgs e)
	{
		Focused = ((IDockContent)DockedContent).IsFocused;
	}

	public DockContent HitTest(Point position)
	{
		if (new Rect(0.0, 20.0, base.ActualWidth, base.ActualHeight).Contains(PointFromScreen(position)))
		{
			return DockedContent.Children[0];
		}
		return null;
	}

	public bool HasChild(IDockContent content)
	{
		if (DockedContent == content || DockedContent.Children.Any((DockContent x) => x == content))
		{
			return true;
		}
		return false;
	}

	public bool HasDescendant(IDockContent content)
	{
		return HasChild(content);
	}

	public void Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo)
	{
		DockedContent.Dock(nextTo as DockContent, newContent, dockTo);
	}

	public void Undock(IDockContent content)
	{
		if (DockedContent == content)
		{
			Undock((IDockLayout)content);
		}
		else if (DockedContent != null)
		{
			DockedContent.Undock(content);
		}
	}

	public void Undock(IDockLayout child)
	{
		if (DockedContent == child)
		{
			DockedContent = null;
			base.Content = null;
			if (base.Parent is IDockLayout)
			{
				((IDockLayout)base.Parent).Undock(this);
			}
		}
	}

	public void Replace(IDockLayout oldLayout, IDockLayout newLayout)
	{
		throw new NotImplementedException();
	}

	public void Close()
	{
		Root.Focus(null);
		if (DockedContent != null)
		{
			DockedContent.Close();
			base.Content = null;
			DockedContent = null;
		}
	}

	IDockLayout IDockLayout.FindParentLayout(IDockContent content)
	{
		return ((IDockLayout)DockedContent).FindParentLayout(content);
	}

	public void DockDragEnter(object sender, DockDragDropEventArgs e)
	{
		Point point = new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0);
		int num = (int)Root.DockIconSize.Width / 4;
		ResourceDictionary resourceDictionary = new ResourceDictionary();
		resourceDictionary.Source = new Uri("/Atf.Gui.Wpf;component/Resources/DockIcons.xaml", UriKind.Relative);
		if (m_dockLeftIcon == null)
		{
			m_dockLeftIcon = new DockIcon((Style)resourceDictionary["DockLeftIcon"], Root.DockIconSize);
			m_dockRightIcon = new DockIcon((Style)resourceDictionary["DockRightIcon"], Root.DockIconSize);
			m_dockTopIcon = new DockIcon((Style)resourceDictionary["DockTopIcon"], Root.DockIconSize);
			m_dockBottomIcon = new DockIcon((Style)resourceDictionary["DockBottomIcon"], Root.DockIconSize);
			m_dockTabIcon = new DockIcon((Style)resourceDictionary["DockTabIcon"], Root.DockIconSize);
		}
		Window window = Window.GetWindow(this);
		Point point2 = Root.PointFromScreen(PointToScreen(new Point(0.0, 0.0)));
		m_dockLeftIcon.Offset = new Point(point2.X + point.X - Root.DockIconSize.Width / 2.0 - Root.DockIconSize.Width - (double)num, point2.Y + point.Y - Root.DockIconSize.Height / 2.0);
		m_dockRightIcon.Offset = new Point(point2.X + point.X + Root.DockIconSize.Width / 2.0 + (double)num, point2.Y + point.Y - Root.DockIconSize.Height / 2.0);
		m_dockTopIcon.Offset = new Point(point2.X + point.X - Root.DockIconSize.Width / 2.0, point2.Y + point.Y - Root.DockIconSize.Height / 2.0 - Root.DockIconSize.Height - (double)num);
		m_dockBottomIcon.Offset = new Point(point2.X + point.X - Root.DockIconSize.Width / 2.0, point2.Y + point.Y + Root.DockIconSize.Height / 2.0 + (double)num);
		m_dockTabIcon.Offset = new Point(point2.X + point.X - Root.DockIconSize.Width / 2.0, point2.Y + point.Y - Root.DockIconSize.Height / 2.0);
		DockIconsLayer dockIconsLayer = Root.DockIconsLayer;
		dockIconsLayer.AddChild(m_dockLeftIcon);
		dockIconsLayer.AddChild(m_dockRightIcon);
		dockIconsLayer.AddChild(m_dockTopIcon);
		dockIconsLayer.AddChild(m_dockBottomIcon);
		dockIconsLayer.AddChild(m_dockTabIcon);
	}

	public void DockDragOver(object sender, DockDragDropEventArgs e)
	{
		DockTo? dockPreview = m_dockPreview;
		m_dockPreview = null;
		Point position = e.MouseEventArgs.GetPosition(Root);
		m_dockLeftIcon.Highlight = m_dockLeftIcon.HitTest(position);
		m_dockRightIcon.Highlight = m_dockRightIcon.HitTest(position);
		m_dockTopIcon.Highlight = m_dockTopIcon.HitTest(position);
		m_dockBottomIcon.Highlight = m_dockBottomIcon.HitTest(position);
		m_dockTabIcon.Highlight = m_dockTabIcon.HitTest(position);
		m_dockPreview = (m_dockLeftIcon.Highlight ? new DockTo?(DockTo.Left) : m_dockPreview);
		m_dockPreview = (m_dockRightIcon.Highlight ? new DockTo?(DockTo.Right) : m_dockPreview);
		m_dockPreview = (m_dockTopIcon.Highlight ? new DockTo?(DockTo.Top) : m_dockPreview);
		m_dockPreview = (m_dockBottomIcon.Highlight ? new DockTo?(DockTo.Bottom) : m_dockPreview);
		m_dockPreview = (m_dockTabIcon.Highlight ? new DockTo?(DockTo.Center) : m_dockPreview);
		if (m_dockPreview.HasValue)
		{
			if (dockPreview != m_dockPreview)
			{
				Root.DockIconsLayer.RemoveChild(m_dockPreviewShape);
				m_dockPreviewShape = null;
				Window window = Window.GetWindow(this);
				Rectangle rectangle = new Rectangle();
				rectangle.Fill = Brushes.RoyalBlue;
				rectangle.Opacity = 0.3;
				FrameworkElement frameworkElement = rectangle;
				double num = 2.0;
				Point point = Root.PointFromScreen(PointToScreen(new Point(num, num)));
				ContentSettings contentSettings = ((e.Content is TabLayout) ? ((TabLayout)e.Content).Children[0].Settings : ((DockContent)e.Content).Settings);
				double num2 = Math.Max(Math.Min(contentSettings.Size.Width, base.ActualWidth / 2.0), base.ActualWidth / 5.0);
				double num3 = Math.Max(Math.Min(contentSettings.Size.Height, base.ActualHeight / 2.0), base.ActualHeight / 5.0);
				double num4 = num2 / base.ActualWidth;
				double num5 = num3 / base.ActualHeight;
				switch (m_dockPreview)
				{
				case DockTo.Left:
					frameworkElement.Width = base.ActualWidth * num4 - num * 2.0;
					frameworkElement.Height = base.ActualHeight - num * 2.0;
					Canvas.SetLeft(frameworkElement, point.X);
					Canvas.SetTop(frameworkElement, point.Y);
					break;
				case DockTo.Right:
					frameworkElement.Width = base.ActualWidth * num4 - num * 2.0;
					frameworkElement.Height = base.ActualHeight - num * 2.0;
					Canvas.SetLeft(frameworkElement, point.X + base.ActualWidth * (1.0 - num4));
					Canvas.SetTop(frameworkElement, point.Y);
					break;
				case DockTo.Top:
					frameworkElement.Width = base.ActualWidth - num * 2.0;
					frameworkElement.Height = base.ActualHeight * num5 - num * 2.0;
					Canvas.SetLeft(frameworkElement, point.X);
					Canvas.SetTop(frameworkElement, point.Y);
					break;
				case DockTo.Bottom:
					frameworkElement.Width = base.ActualWidth - num * 2.0;
					frameworkElement.Height = base.ActualHeight * num5 - num * 2.0;
					Canvas.SetLeft(frameworkElement, point.X);
					Canvas.SetTop(frameworkElement, point.Y + base.ActualHeight * (1.0 - num5));
					break;
				case DockTo.Center:
				{
					Canvas canvas = new Canvas();
					canvas.SnapsToDevicePixels = true;
					canvas.Width = base.ActualWidth;
					canvas.Height = base.ActualHeight;
					Canvas.SetLeft(canvas, point.X);
					Canvas.SetTop(canvas, point.Y);
					frameworkElement.Width = base.ActualWidth - num * 2.0;
					frameworkElement.Height = base.ActualHeight - 20.0 - num * 2.0;
					Canvas.SetLeft(frameworkElement, 0.0);
					Canvas.SetTop(frameworkElement, 0.0);
					canvas.Children.Add(frameworkElement);
					rectangle = new Rectangle();
					rectangle.Fill = Brushes.RoyalBlue;
					rectangle.Opacity = 0.3;
					rectangle.Width = Math.Min(base.ActualWidth / 4.0, 50.0);
					rectangle.Height = 20.0;
					Canvas.SetLeft(rectangle, 0.0);
					Canvas.SetTop(rectangle, base.ActualHeight - 20.0 - num * 2.0);
					canvas.Children.Add(rectangle);
					frameworkElement = canvas;
					break;
				}
				}
				Root.DockIconsLayer.InsertChild(0, frameworkElement);
				m_dockPreviewShape = frameworkElement;
			}
		}
		else if (m_dockPreviewShape != null)
		{
			Root.DockIconsLayer.RemoveChild(m_dockPreviewShape);
			m_dockPreviewShape = null;
		}
	}

	public void DockDragLeave(object sender, DockDragDropEventArgs e)
	{
		DockIconsLayer dockIconsLayer = Root.DockIconsLayer;
		dockIconsLayer.RemoveChild(m_dockLeftIcon);
		dockIconsLayer.RemoveChild(m_dockRightIcon);
		dockIconsLayer.RemoveChild(m_dockTopIcon);
		dockIconsLayer.RemoveChild(m_dockBottomIcon);
		dockIconsLayer.RemoveChild(m_dockTabIcon);
		if (m_dockPreviewShape != null)
		{
			Root.DockIconsLayer.RemoveChild(m_dockPreviewShape);
			m_dockPreviewShape = null;
		}
		dockIconsLayer.CloseIfEmpty();
	}

	public void DockDrop(object sender, DockDragDropEventArgs e)
	{
		DockPanel root = Root;
		DockTo value = m_dockPreview.Value;
		if (e.Content is TabLayout)
		{
			foreach (DockContent child in ((TabLayout)e.Content).Children)
			{
				ContentSettings settings = child.Settings;
				settings.DockState = DockState.Docked;
			}
		}
		else
		{
			ContentSettings settings2 = ((DockContent)e.Content).Settings;
			settings2.DockState = DockState.Docked;
		}
		switch (value)
		{
		case DockTo.Left:
		case DockTo.Right:
		case DockTo.Top:
		case DockTo.Bottom:
			((IDockLayout)base.Parent).Dock(DockedContent, e.Content, value);
			break;
		case DockTo.Center:
			Dock(null, e.Content, value);
			break;
		}
		root.CheckConsistency();
	}

	public XmlSchema GetSchema()
	{
		throw new NotImplementedException();
	}

	public void ReadXml(XmlReader reader)
	{
		if (reader.ReadToFollowing(GetType().Name))
		{
			reader.ReadStartElement();
			if (reader.LocalName == typeof(TabLayout).Name || reader.LocalName == "MultiContent")
			{
				DockedContent = new TabLayout(Root, reader.ReadSubtree());
				base.Content = DockedContent;
				reader.ReadEndElement();
			}
			reader.ReadEndElement();
		}
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteStartElement(GetType().Name);
		DockedContent.WriteXml(writer);
		writer.WriteEndElement();
	}
}
