using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Sce.Atf.Wpf.Docking;

public class SidePopup : Selector, IDockLayout, IXmlSerializable
{
	public static DependencyProperty ThumbBrushProperty;

	public static DependencyProperty TabsPlacementProperty;

	public static DependencyProperty OrientationProperty;

	private List<DockContent> m_children;

	private SideBarButton m_lastItemOver;

	private System.Timers.Timer m_timer;

	private Grid PART_Grid;

	private ResizablePopup PART_Popup;

	private DateTime m_timerTime;

	public Brush ThumbBrush
	{
		get
		{
			return (Brush)GetValue(ThumbBrushProperty);
		}
		set
		{
			SetValue(ThumbBrushProperty, value);
		}
	}

	public Dock TabsPlacement
	{
		get
		{
			return (Dock)GetValue(TabsPlacementProperty);
		}
		set
		{
			SetValue(TabsPlacementProperty, value);
		}
	}

	public Orientation Orientation
	{
		get
		{
			return (Orientation)GetValue(OrientationProperty);
		}
		private set
		{
			SetValue(OrientationProperty, value);
		}
	}

	public DockPanel Root { get; set; }

	public static void TabsPlacementPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
	{
		SidePopup sidePopup = (SidePopup)o;
		switch ((Dock)args.NewValue)
		{
		case System.Windows.Controls.Dock.Top:
		case System.Windows.Controls.Dock.Bottom:
			sidePopup.Orientation = Orientation.Horizontal;
			break;
		case System.Windows.Controls.Dock.Left:
		case System.Windows.Controls.Dock.Right:
			sidePopup.Orientation = Orientation.Vertical;
			break;
		}
		sidePopup.UpdatePopupProperties();
	}

	static SidePopup()
	{
		ThumbBrushProperty = DependencyProperty.Register("ThumbBrush", typeof(Brush), typeof(SidePopup));
		TabsPlacementProperty = DependencyProperty.Register("TabsPlacement", typeof(Dock), typeof(SidePopup), new PropertyMetadata(TabsPlacementPropertyChanged));
		OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SidePopup));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SidePopup), new FrameworkPropertyMetadata(typeof(SidePopup)));
	}

	public SidePopup()
	{
		m_children = new List<DockContent>();
		Orientation = Orientation.Vertical;
		TabsPlacement = System.Windows.Controls.Dock.Left;
		m_timer = new System.Timers.Timer();
		m_timer.AutoReset = false;
		m_timer.Interval = 50.0;
		m_timer.Elapsed += Timer_Elapsed;
		Application.Current.MainWindow.Deactivated += MainWindow_Deactivated;
	}

	private void Window_Closing(object sender, EventArgs e)
	{
		IDockContent content = (IDockContent)PART_Popup.Tag;
		ClosePopup();
		m_lastItemOver = null;
		Undock(content);
	}

	private void MainWindow_Deactivated(object sender, EventArgs e)
	{
		m_timer.Stop();
		if (PART_Popup != null)
		{
			PART_Popup.IsOpen = false;
		}
	}

	private void Timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		if (base.Dispatcher.Thread != Thread.CurrentThread)
		{
			base.Dispatcher.BeginInvoke(new EventHandler<ElapsedEventArgs>(Timer_Elapsed), sender, e);
			return;
		}
		try
		{
			Point point = ((m_lastItemOver != null) ? Win32Calls.GetPosition(m_lastItemOver) : new Point(-1.0, -1.0));
			if (m_lastItemOver != null && new Rect(0.0, 0.0, m_lastItemOver.ActualWidth, m_lastItemOver.ActualHeight).Contains(point))
			{
				if ((DateTime.Now - m_timerTime).TotalMilliseconds > 500.0 || Mouse.LeftButton == MouseButtonState.Pressed)
				{
					ShowPopup();
					m_lastItemOver.IsChecked = true;
					m_lastItemOver = null;
					m_timerTime = DateTime.Now;
				}
				m_timer.Start();
				return;
			}
			Application current = Application.Current;
			if (current == null)
			{
				return;
			}
			Window mainWindow = current.MainWindow;
			if (mainWindow == null || !mainWindow.IsActive || !PART_Popup.IsOpen || PART_Popup.Resizing)
			{
				return;
			}
			Win32Calls.Win32Point pt = default(Win32Calls.Win32Point);
			Win32Calls.GetCursorPos(ref pt);
			Point point2 = new Point(pt.X, pt.Y);
			Point point3 = PointToScreen(new Point(0.0, 0.0));
			Matrix transformToDevice = PresentationSource.FromVisual(Window.GetWindow(this)).CompositionTarget.TransformToDevice;
			if (transformToDevice != Matrix.Identity)
			{
				transformToDevice.Invert();
				point2 = transformToDevice.Transform(point2);
				point3 = transformToDevice.Transform(point3);
			}
			switch (TabsPlacement)
			{
			case System.Windows.Controls.Dock.Top:
				point3.Y += base.ActualHeight;
				break;
			case System.Windows.Controls.Dock.Bottom:
				point3.Y -= PART_Popup.Height;
				break;
			case System.Windows.Controls.Dock.Left:
				point3.X += base.ActualWidth;
				break;
			case System.Windows.Controls.Dock.Right:
				point3.X -= PART_Popup.Width;
				break;
			}
			bool flag = new Rect(point3.X, point3.Y, PART_Popup.Width, PART_Popup.Height).Contains(point2);
			Point position = Win32Calls.GetPosition(this);
			bool flag2 = new Rect(0.0, 0.0, base.ActualWidth, base.ActualHeight).Contains(position);
			if (!flag && !flag2)
			{
				if ((DateTime.Now - m_timerTime).TotalMilliseconds > 1000.0)
				{
					ClosePopup();
				}
				else
				{
					m_timer.Start();
				}
			}
			else
			{
				m_timerTime = DateTime.Now;
				m_timer.Start();
			}
		}
		catch (InvalidOperationException)
		{
		}
	}

	private void ShowPopup()
	{
		DockContent dockContent = (DockContent)m_lastItemOver.Tag;
		ShowPopup(dockContent);
	}

	internal void ShowPopup(DockContent dockContent)
	{
		if (PART_Popup.IsOpen)
		{
			if (((DockedWindow)PART_Popup.Content).HasChild(dockContent))
			{
				return;
			}
			ClosePopup();
		}
		ContentSettings settings = dockContent.Settings;
		switch (Orientation)
		{
		case Orientation.Horizontal:
			PART_Popup.MaxWidth = base.RenderSize.Width;
			PART_Popup.MaxHeight = 0.75 * Math.Min(Root.RenderSize.Height, SystemParameters.PrimaryScreenHeight);
			PART_Popup.Width = base.RenderSize.Width;
			PART_Popup.Height = Math.Min(settings.Size.Height + 25.0, PART_Popup.MaxHeight);
			break;
		case Orientation.Vertical:
			PART_Popup.MaxWidth = 0.75 * Math.Min(Root.RenderSize.Width, SystemParameters.PrimaryScreenWidth);
			PART_Popup.MaxHeight = base.RenderSize.Height;
			PART_Popup.Width = Math.Min(settings.Size.Width + 7.0, PART_Popup.MaxWidth);
			PART_Popup.Height = base.RenderSize.Height;
			break;
		}
		DockedWindow dockedWindow = new DockedWindow(Root, dockContent);
		dockedWindow.Closing += Window_Closing;
		dockedWindow.Focused = true;
		dockedWindow.IsCollapsed = true;
		PART_Popup.Content = dockedWindow;
		PART_Popup.Tag = dockContent;
		BooleanAnimationUsingKeyFrames booleanAnimationUsingKeyFrames = new BooleanAnimationUsingKeyFrames();
		booleanAnimationUsingKeyFrames.Duration = TimeSpan.FromSeconds(0.1);
		booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame(value: false, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.0))));
		booleanAnimationUsingKeyFrames.KeyFrames.Add(new DiscreteBooleanKeyFrame(value: true, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.1))));
		PART_Popup.IsOpen = true;
		Root.Focus(dockContent);
		m_timer.Start();
	}

	private void ClosePopup()
	{
		PART_Popup.IsOpen = false;
		if (PART_Popup.Content is DockedWindow dockedWindow)
		{
			DockContent dockContent = (DockContent)PART_Popup.Tag;
			dockedWindow.Closing -= Window_Closing;
			dockedWindow.Close();
			dockContent.IsVisible = true;
			PART_Popup.Tag = null;
			PART_Popup.Content = null;
		}
		foreach (SideBarButton item in (IEnumerable)base.Items)
		{
			item.IsChecked = false;
		}
	}

	private void TabItem_MouseEnter(object sender, MouseEventArgs e)
	{
		m_lastItemOver = (SideBarButton)sender;
		m_timerTime = DateTime.Now;
		m_timer.Start();
	}

	private void TabItem_DragEnter(object sender, DragEventArgs e)
	{
		e.Effects = DragDropEffects.Scroll & e.AllowedEffects;
		e.Handled = true;
		m_lastItemOver = (SideBarButton)sender;
		m_timerTime = DateTime.Now;
		m_timer.Start();
	}

	private void TabItem_Click(object sender, RoutedEventArgs e)
	{
		m_timerTime = DateTime.Now;
		Timer_Elapsed(m_timer, null);
	}

	private void AddContent(DockContent content)
	{
		m_children.Add(content);
		content.Settings.DockState = DockState.Collapsed;
		FrameworkElement content2 = CreateHeader(content);
		SideBarButton sideBarButton = new SideBarButton(TabsPlacement)
		{
			Content = content2,
			Tag = content
		};
		sideBarButton.MouseEnter += TabItem_MouseEnter;
		sideBarButton.AllowDrop = true;
		sideBarButton.DragEnter += TabItem_DragEnter;
		sideBarButton.Click += TabItem_Click;
		base.Items.Add(sideBarButton);
		content.IsVisible = true;
		content.IsFocused = false;
		content.IsFocusedChanged += DockContent_IsFocusedChanged;
	}

	private void DockContent_IsFocusedChanged(object sender, BooleanArgs e)
	{
		if (e.Value)
		{
			DockContent dockContent = (DockContent)sender;
			if (m_children.Contains(dockContent))
			{
				ShowPopup(dockContent);
			}
		}
		else
		{
			ClosePopup();
		}
	}

	private FrameworkElement CreateHeader(IDockContent content)
	{
		StackPanel stackPanel = new StackPanel
		{
			Orientation = Orientation.Horizontal
		};
		if (content.Icon is ImageSource && (Root.IconVisibility & IconVisibility.SideBar) == IconVisibility.SideBar)
		{
			Image element = new Image
			{
				Source = (ImageSource)content.Icon,
				Width = Root.HeaderIconSize.Width,
				Height = Root.HeaderIconSize.Height,
				IsHitTestVisible = false
			};
			stackPanel.Children.Add(element);
		}
		TextBlock textBlock = new TextBlock
		{
			Text = content.Header,
			IsHitTestVisible = false,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center
		};
		textBlock.RenderTransformOrigin = new Point(0.5, 0.5);
		stackPanel.Children.Add(textBlock);
		stackPanel.LayoutTransform = new RotateTransform((Orientation != Orientation.Horizontal) ? 90 : 0);
		return stackPanel;
	}

	public override void OnApplyTemplate()
	{
		PART_Grid = (Grid)base.Template.FindName("PART_Grid", this);
		PART_Popup = (ResizablePopup)base.Template.FindName("PART_Popup", this);
		UpdatePopupProperties();
	}

	private void UpdatePopupProperties()
	{
		if (PART_Popup != null)
		{
			PART_Popup.DockSide = TabsPlacement;
			switch (TabsPlacement)
			{
			case System.Windows.Controls.Dock.Top:
				PART_Popup.Placement = PlacementMode.Bottom;
				break;
			case System.Windows.Controls.Dock.Bottom:
				PART_Popup.Placement = PlacementMode.Top;
				break;
			case System.Windows.Controls.Dock.Left:
				PART_Popup.Placement = PlacementMode.Right;
				break;
			case System.Windows.Controls.Dock.Right:
				PART_Popup.Placement = PlacementMode.Left;
				break;
			}
		}
	}

	public Rect RectForContent(IDockContent content)
	{
		UpdateLayout();
		Rect result = default(Rect);
		foreach (SideBarButton item in (IEnumerable)base.Items)
		{
			if (item.Tag == content)
			{
				result = new Rect(item.PointToScreen(new Point(0.0, 0.0)), item.RenderSize);
				return result;
			}
		}
		return result;
	}

	DockContent IDockLayout.HitTest(Point position)
	{
		return null;
	}

	public bool HasChild(IDockContent content)
	{
		return m_children.Any((DockContent x) => x == content);
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
				AddContent(current);
				enumerator = tabLayout.Children.GetEnumerator();
			}
		}
		else
		{
			AddContent((DockContent)newContent);
		}
	}

	public void Undock(IDockContent content)
	{
		ClosePopup();
		if (!(content is DockContent dockContent))
		{
			return;
		}
		dockContent.IsVisible = false;
		dockContent.IsFocusedChanged -= DockContent_IsFocusedChanged;
		m_children.Remove(dockContent);
		foreach (SideBarButton item in (IEnumerable)base.Items)
		{
			if (item.Tag == dockContent)
			{
				if (m_lastItemOver == item)
				{
					m_timer.Stop();
					m_lastItemOver = null;
				}
				base.Items.Remove(item);
				break;
			}
		}
	}

	void IDockLayout.Undock(IDockLayout child)
	{
		throw new NotImplementedException();
	}

	void IDockLayout.Replace(IDockLayout oldLayout, IDockLayout newLayout)
	{
		throw new NotImplementedException();
	}

	public void Close()
	{
		m_children.Clear();
		base.Items.Clear();
	}

	IDockLayout IDockLayout.FindParentLayout(IDockContent content)
	{
		return (content is DockContent && m_children.Contains((DockContent)content)) ? this : null;
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
		if (reader.ReadToDescendant("Content"))
		{
			do
			{
				string attribute = reader.GetAttribute("UCID");
				DockContent content = Root.GetContent(attribute);
				if (content != null)
				{
					AddContent(content);
				}
			}
			while (reader.ReadToNextSibling("Content"));
		}
		reader.Read();
	}

	public void WriteXml(XmlWriter writer)
	{
		writer.WriteStartElement(GetType().Name);
		writer.WriteAttributeString("Side", TabsPlacement.ToString());
		foreach (DockContent child in m_children)
		{
			writer.WriteStartElement("Content");
			writer.WriteAttributeString("UCID", ((IDockContent)child).UID);
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
	}
}
