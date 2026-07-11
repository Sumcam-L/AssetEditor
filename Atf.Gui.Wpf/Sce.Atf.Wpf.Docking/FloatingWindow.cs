using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Sce.Atf.Wpf.Docking;

public class FloatingWindow : Window, IDockLayout, IDockable, IXmlSerializable
{
	public static DependencyProperty DockedContentProperty;

	private const int WM_SYSCOMMAND = 274;

	private const int SC_MOVE = 61456;

	private Point m_lastMousePos;

	private List<IDockable> m_dockOver;

	private bool m_validPosition;

	private bool m_dragging;

	private DockIcon m_dockTabIcon;

	private FrameworkElement m_dockPreviewShape;

	private DockIconsLayer m_dockIconsLayer;

	public TabLayout DockedContent
	{
		get
		{
			return (TabLayout)GetValue(DockedContentProperty);
		}
		set
		{
			SetValue(DockedContentProperty, value);
		}
	}

	internal DockIconsLayer DockIconsLayer
	{
		get
		{
			if (m_dockIconsLayer == null)
			{
				m_dockIconsLayer = new DockIconsLayer(this);
				m_dockIconsLayer.Owner = Window.GetWindow(this);
				m_dockIconsLayer.Closing += DockIconsLayer_Closing;
				m_dockIconsLayer.Show();
			}
			return m_dockIconsLayer;
		}
	}

	public DockPanel Root { get; private set; }

	public DockTo? DockPreview
	{
		get
		{
			DockTo? result = null;
			if (m_dockTabIcon.Highlight)
			{
				result = DockTo.Center;
			}
			return result;
		}
	}

	private FloatingWindow(DockPanel dockPanel)
	{
		Root = dockPanel;
		base.ShowInTaskbar = false;
		base.WindowStyle = WindowStyle.ToolWindow;
		base.WindowStartupLocation = WindowStartupLocation.Manual;
		m_dockOver = new List<IDockable>();
		base.Loaded += FloatingWindow_Loaded;
		base.Closing += FloatingWindow_Closing;
		base.MouseMove += FloatingWindow_MouseMove;
		base.MouseUp += FloatingWindow_MouseUp;
		base.MouseLeave += FloatingWindow_MouseLeave;
		base.Activated += FloatingWindow_Activated;
		base.LocationChanged += FloatingWindow_LocationChanged;
		base.SizeChanged += FloatingWindow_SizeChanged;
		Window mainWindow = Application.Current.MainWindow;
		if (mainWindow != null && mainWindow.InputBindings != null)
		{
			base.InputBindings.AddRange(mainWindow.InputBindings);
		}
	}

	static FloatingWindow()
	{
		DockedContentProperty = DependencyProperty.Register("DockedContent", typeof(TabLayout), typeof(FloatingWindow));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FloatingWindow), new FrameworkPropertyMetadata(typeof(FloatingWindow)));
	}

	public FloatingWindow(DockPanel root, IDockContent content, Point origin, Size size)
		: this(root)
	{
		base.Left = origin.X;
		base.Top = origin.Y;
		base.Width = size.Width;
		base.Height = size.Height;
		if (content is TabLayout)
		{
			DockedContent = (TabLayout)content;
		}
		else
		{
			DockedContent = new TabLayout(Root);
			DockedContent.Dock(null, content, DockTo.Center);
		}
		foreach (DockContent child in DockedContent.Children)
		{
			child.Settings.DockState = DockState.Floating;
		}
		base.Content = DockedContent;
		Binding binding = new Binding("Header")
		{
			Source = DockedContent
		};
		SetBinding(Window.TitleProperty, binding);
	}

	public FloatingWindow(DockPanel root, XmlReader reader)
		: this(root)
	{
		ReadXml(reader);
		base.Content = DockedContent;
		base.Title = DockedContent.Header;
		Binding binding = new Binding("Header")
		{
			Source = DockedContent
		};
		SetBinding(Window.TitleProperty, binding);
	}

	public IDockContent GetActiveContent()
	{
		return (DockedContent != null) ? DockedContent.GetActiveContent() : null;
	}

	private void DockIconsLayer_Closing(object sender, CancelEventArgs e)
	{
		m_dockIconsLayer = null;
	}

	private void FloatingWindow_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (DockedContent == null)
		{
			return;
		}
		foreach (DockContent child in DockedContent.Children)
		{
			child.Settings.Size = new Size(base.Width, base.Height);
		}
	}

	private void FloatingWindow_LocationChanged(object sender, EventArgs e)
	{
		if (DockedContent == null)
		{
			return;
		}
		foreach (DockContent child in DockedContent.Children)
		{
			child.Settings.Location = new Point(base.Left, base.Top);
		}
	}

	private void FloatingWindow_MouseLeave(object sender, MouseEventArgs e)
	{
		if (base.IsMouseCaptured)
		{
			ReleaseMouseCapture();
		}
	}

	private void FloatingWindow_Activated(object sender, EventArgs e)
	{
		if (DockedContent != null && DockedContent.SelectedItem != null)
		{
			Root.Focus((IDockContent)DockedContent.SelectedItem);
		}
	}

	private void FloatingWindow_Closing(object sender, CancelEventArgs e)
	{
		if (DockedContent != null)
		{
			DockedContent.OnClose(this, new ContentClosedEventArgs(ContentToClose.All));
			DockedContent.Close();
			base.Content = null;
			DockedContent = null;
		}
	}

	private void FloatingWindow_Loaded(object sender, RoutedEventArgs e)
	{
		HwndSource hwndSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
		hwndSource.AddHook(WndProc);
	}

	private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		if (msg == 274)
		{
			int num = (int)wParam;
			if ((num & 0xFFF0) == 61456)
			{
				handled = true;
				base.Dispatcher.BeginInvoke(new EventHandler(MouseTitleDown), this, EventArgs.Empty);
			}
		}
		return IntPtr.Zero;
	}

	private void MouseTitleDown(object o, EventArgs args)
	{
		m_lastMousePos = Mouse.GetPosition(this);
		m_validPosition = false;
		m_dragging = false;
		CaptureMouse();
	}

	private void FloatingWindow_MouseMove(object sender, MouseEventArgs e)
	{
		Point position = e.GetPosition(this);
		if (!m_validPosition && position != new Point(0.0, 0.0))
		{
			m_lastMousePos = e.GetPosition(base.Owner);
			m_validPosition = true;
		}
		if (!base.IsMouseCaptured || !m_validPosition)
		{
			return;
		}
		base.Topmost = true;
		position = e.GetPosition(base.Owner);
		if (!m_dragging && (m_lastMousePos - position).Length > 2.0)
		{
			m_dragging = true;
		}
		if (!m_dragging)
		{
			return;
		}
		if (base.WindowState == WindowState.Maximized)
		{
			base.WindowState = WindowState.Normal;
			base.Left = 0.0;
			base.Top = 0.0;
		}
		else
		{
			base.Left = base.Left + position.X - m_lastMousePos.X;
			base.Top = base.Top + position.Y - m_lastMousePos.Y;
		}
		m_lastMousePos = position;
		List<IDockable> list = Root.FindElementsAt(e);
		DockDragDropEventArgs e2 = new DockDragDropEventArgs(DockedContent, e);
		foreach (IDockable item in m_dockOver)
		{
			if (!list.Contains(item))
			{
				item.DockDragLeave(this, e2);
			}
		}
		foreach (IDockable item2 in list)
		{
			if (!m_dockOver.Contains(item2))
			{
				item2.DockDragEnter(this, e2);
			}
		}
		foreach (IDockable item3 in list)
		{
			item3.DockDragOver(this, e2);
		}
		m_dockOver = list;
	}

	private void FloatingWindow_MouseUp(object sender, MouseButtonEventArgs e)
	{
		if (!base.IsMouseCaptured)
		{
			return;
		}
		ReleaseMouseCapture();
		base.Topmost = false;
		m_validPosition = false;
		m_dragging = false;
		bool flag = false;
		DockDragDropEventArgs e2 = new DockDragDropEventArgs(DockedContent, e);
		foreach (IDockable item in m_dockOver)
		{
			item.DockDragLeave(this, e2);
			if (!flag && item.DockPreview.HasValue)
			{
				base.Content = null;
				DockedContent = null;
				flag = true;
				item.DockDrop(this, e2);
			}
		}
		m_dockOver = new List<IDockable>();
		if (flag)
		{
			base.Owner.Focus();
			base.Owner.Activate();
			Close();
		}
		else
		{
			Focus();
		}
	}

	public DockContent HitTest(Point position)
	{
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
		if (content is DockContent)
		{
			DockedContent.RemoveItem((DockContent)content);
			if (DockedContent.Children.Count == 0)
			{
				Close();
			}
		}
	}

	public void Undock(IDockLayout child)
	{
		throw new NotImplementedException();
	}

	public void Replace(IDockLayout oldLayout, IDockLayout newLayout)
	{
		throw new NotImplementedException();
	}

	public void Drag(IDockContent content)
	{
		Root.Drag(this, content);
	}

	IDockLayout IDockLayout.FindParentLayout(IDockContent content)
	{
		return ((IDockLayout)DockedContent).FindParentLayout(content);
	}

	public XmlSchema GetSchema()
	{
		return null;
	}

	public void ReadXml(XmlReader reader)
	{
		if (!reader.ReadToFollowing(GetType().Name))
		{
			return;
		}
		reader.ReadStartElement(GetType().Name);
		if (reader.LocalName == typeof(TabLayout).Name || reader.LocalName == "MultiContent")
		{
			DockedContent = new TabLayout(Root, reader.ReadSubtree());
			if (DockedContent.Children.Count > 0)
			{
				ContentSettings settings = DockedContent.Children[0].Settings;
				base.Left = settings.Location.X;
				base.Top = settings.Location.Y;
				base.Width = settings.Size.Width;
				base.Height = settings.Size.Height;
				base.Width = Math.Max(base.Width, SystemParameters.MinimumWindowWidth);
				base.Height = Math.Max(base.Height, SystemParameters.MinimumWindowHeight);
				base.Left = Math.Max(SystemParameters.VirtualScreenLeft, Math.Min(base.Left, SystemParameters.VirtualScreenWidth + SystemParameters.VirtualScreenLeft - base.Width));
				base.Top = Math.Max(SystemParameters.VirtualScreenTop, Math.Min(base.Top, SystemParameters.VirtualScreenHeight + SystemParameters.VirtualScreenTop - base.Height));
				reader.ReadEndElement();
			}
		}
		reader.ReadEndElement();
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteStartElement(GetType().Name);
		DockedContent.WriteXml(writer);
		writer.WriteEndElement();
	}

	public void DockDragEnter(object sender, DockDragDropEventArgs e)
	{
		Point point = new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0);
		ResourceDictionary resourceDictionary = new ResourceDictionary();
		resourceDictionary.Source = new Uri("/Atf.Gui.Wpf;component/Resources/DockIcons.xaml", UriKind.Relative);
		if (m_dockTabIcon == null)
		{
			m_dockTabIcon = new DockIcon((Style)resourceDictionary["DockTabIcon"], Root.DockIconSize);
		}
		m_dockTabIcon.Offset = new Point(point.X - Root.DockIconSize.Width / 2.0, point.Y - Root.DockIconSize.Height / 2.0);
		m_dockTabIcon.Highlight = false;
		DockIconsLayer.AddChild(m_dockTabIcon);
	}

	public void DockDragOver(object sender, DockDragDropEventArgs e)
	{
		Point position = e.MouseEventArgs.GetPosition(this);
		bool flag = m_dockTabIcon.HitTest(position);
		if (flag && !m_dockTabIcon.Highlight)
		{
			m_dockTabIcon.Highlight = flag;
			DockIconsLayer.RemoveChild(m_dockPreviewShape);
			m_dockPreviewShape = null;
			Window window = Window.GetWindow(this);
			Rectangle rectangle = new Rectangle();
			rectangle.Fill = Brushes.RoyalBlue;
			rectangle.Opacity = 0.3;
			m_dockPreviewShape = rectangle;
			double num = 2.0;
			Point point = PointFromScreen(PointToScreen(new Point(num, num)));
			Canvas canvas = new Canvas();
			canvas.SnapsToDevicePixels = true;
			canvas.Width = DockedContent.ActualWidth;
			canvas.Height = DockedContent.ActualHeight;
			Canvas.SetLeft(canvas, point.X);
			Canvas.SetTop(canvas, point.Y);
			m_dockPreviewShape.Width = DockedContent.ActualWidth - num * 2.0;
			m_dockPreviewShape.Height = DockedContent.ActualHeight - 20.0 - num * 2.0;
			Canvas.SetLeft(m_dockPreviewShape, 0.0);
			Canvas.SetTop(m_dockPreviewShape, 0.0);
			canvas.Children.Add(m_dockPreviewShape);
			rectangle = new Rectangle();
			rectangle.Fill = Brushes.RoyalBlue;
			rectangle.Opacity = 0.3;
			rectangle.Width = Math.Min(DockedContent.ActualWidth / 4.0, 50.0);
			rectangle.Height = 20.0;
			Canvas.SetLeft(rectangle, 0.0);
			Canvas.SetTop(rectangle, DockedContent.ActualHeight - 20.0 - num * 2.0);
			canvas.Children.Add(rectangle);
			m_dockPreviewShape = canvas;
			DockIconsLayer.InsertChild(0, m_dockPreviewShape);
		}
		else if (!flag)
		{
			if (m_dockPreviewShape != null)
			{
				DockIconsLayer.RemoveChild(m_dockPreviewShape);
				m_dockPreviewShape = null;
			}
			m_dockTabIcon.Highlight = false;
		}
	}

	public void DockDragLeave(object sender, DockDragDropEventArgs e)
	{
		DockIconsLayer.RemoveChild(m_dockTabIcon);
		if (m_dockPreviewShape != null)
		{
			DockIconsLayer.RemoveChild(m_dockPreviewShape);
			m_dockPreviewShape = null;
		}
		DockIconsLayer.CloseIfEmpty();
	}

	public void DockDrop(object sender, DockDragDropEventArgs e)
	{
		if (e.Content is TabLayout)
		{
			foreach (DockContent child in ((TabLayout)e.Content).Children)
			{
				child.Settings.DockState = DockState.Floating;
			}
		}
		else
		{
			((DockContent)e.Content).Settings.DockState = DockState.Floating;
		}
		Dock(null, e.Content, DockTo.Center);
	}

	void IDockLayout.Close()
	{
		Close();
	}
}
