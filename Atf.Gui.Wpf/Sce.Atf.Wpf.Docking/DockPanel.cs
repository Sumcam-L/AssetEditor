using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;

namespace Sce.Atf.Wpf.Docking;

public class DockPanel : Control, IDockLayout, IDockable
{
	public static ComponentResourceKey GridSplitterStyleKey;

	public static DependencyProperty IconVisibilityProperty;

	public static DependencyProperty HeaderIconSizeProperty;

	public static DependencyProperty RegisteredContentsProperty;

	private DockIconsLayer m_dockIconsLayer;

	private XmlReader m_layoutToApply;

	private bool m_templateApplied;

	private bool m_layoutApplied;

	private DockIcon m_dockLeftIcon;

	private DockIcon m_dockRightIcon;

	private DockIcon m_dockTopIcon;

	private DockIcon m_dockBottomIcon;

	private DockIcon m_dockTabIcon;

	private FrameworkElement m_dockPreviewShape;

	private DockTo? m_dockPreview;

	private DockContent m_lastFocusedContent;

	private Dictionary<string, DockContent> m_registeredContents;

	private List<FloatingWindow> m_windows;

	public ObservableCollection<IDockContent> RegisteredContents
	{
		get
		{
			return (ObservableCollection<IDockContent>)GetValue(RegisteredContentsProperty);
		}
		set
		{
			SetValue(RegisteredContentsProperty, value);
		}
	}

	public Size HeaderIconSize
	{
		get
		{
			return (Size)GetValue(HeaderIconSizeProperty);
		}
		set
		{
			SetValue(HeaderIconSizeProperty, value);
		}
	}

	public IconVisibility IconVisibility
	{
		get
		{
			return (IconVisibility)GetValue(IconVisibilityProperty);
		}
		set
		{
			SetValue(IconVisibilityProperty, value);
		}
	}

	private ContentPresenter PART_MainPanel { get; set; }

	private SidePopup PART_LeftCollapsePanel { get; set; }

	private SidePopup PART_TopCollapsePanel { get; set; }

	private SidePopup PART_RightCollapsePanel { get; set; }

	private SidePopup PART_BottomCollapsePanel { get; set; }

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

	private GridLayout GridLayout
	{
		get
		{
			return PART_MainPanel.Content as GridLayout;
		}
		set
		{
			PART_MainPanel.Content = value;
		}
	}

	internal Size DockIconSize { get; private set; }

	DockPanel IDockLayout.Root => this;

	DockTo? IDockable.DockPreview => m_dockPreview;

	static DockPanel()
	{
		GridSplitterStyleKey = new ComponentResourceKey(typeof(DockPanel), "GridSplitterStyleKey");
		IconVisibilityProperty = DependencyProperty.Register("IconVisibility", typeof(IconVisibility), typeof(DockPanel));
		HeaderIconSizeProperty = DependencyProperty.Register("HeaderIconSize", typeof(Size), typeof(DockPanel));
		RegisteredContentsProperty = DependencyProperty.Register("RegisteredContents", typeof(ObservableCollection<IDockContent>), typeof(DockPanel));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DockPanel), new FrameworkPropertyMetadata(typeof(DockPanel)));
	}

	public DockPanel()
	{
		IconVisibility = IconVisibility.All;
		HeaderIconSize = new Size(16.0, 16.0);
		DockIconSize = new Size(32.0, 32.0);
		base.Focusable = true;
		m_registeredContents = new Dictionary<string, DockContent>();
		RegisteredContents = new ObservableCollection<IDockContent>();
		m_windows = new List<FloatingWindow>();
		base.Loaded += DockPanel_Loaded;
	}

	public DockPanel(double dockIconSize)
	{
		DockIconSize = new Size(dockIconSize, dockIconSize);
	}

	private void DockPanel_Loaded(object sender, RoutedEventArgs e)
	{
		Window window = Window.GetWindow(this);
		if (window != null)
		{
			window.Activated += OwnerWindowActivated;
		}
		if (m_templateApplied && !m_layoutApplied)
		{
			ApplyLayout(m_layoutToApply);
		}
	}

	private void DockIconsLayer_Closing(object sender, CancelEventArgs e)
	{
		m_dockIconsLayer = null;
	}

	public IDockContent GetActiveContent()
	{
		if (!base.IsLoaded)
		{
			return null;
		}
		if (m_lastFocusedContent != null && m_lastFocusedContent.IsFocused)
		{
			return m_lastFocusedContent;
		}
		return null;
	}

	public bool IsContentVisible(IDockContent content)
	{
		if (!base.IsLoaded)
		{
			return false;
		}
		return ((IDockLayout)this).HasDescendant(content);
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		PART_MainPanel = (ContentPresenter)base.Template.FindName("PART_MainPanel", this);
		PART_LeftCollapsePanel = (SidePopup)base.Template.FindName("PART_LeftCollapsePanel", this);
		PART_LeftCollapsePanel.Root = this;
		PART_LeftCollapsePanel.TabsPlacement = Dock.Left;
		PART_TopCollapsePanel = (SidePopup)base.Template.FindName("PART_TopCollapsePanel", this);
		PART_TopCollapsePanel.Root = this;
		PART_TopCollapsePanel.TabsPlacement = Dock.Top;
		PART_RightCollapsePanel = (SidePopup)base.Template.FindName("PART_RightCollapsePanel", this);
		PART_RightCollapsePanel.Root = this;
		PART_RightCollapsePanel.TabsPlacement = Dock.Right;
		PART_BottomCollapsePanel = (SidePopup)base.Template.FindName("PART_BottomCollapsePanel", this);
		PART_BottomCollapsePanel.Root = this;
		PART_BottomCollapsePanel.TabsPlacement = Dock.Bottom;
		m_templateApplied = true;
	}

	internal void CheckConsistency()
	{
		if (GridLayout != null)
		{
			GridLayout.CheckConsistency();
		}
	}

	internal DockContent GetContent(string uniqueName)
	{
		DockContent value = null;
		if (m_registeredContents.TryGetValue(uniqueName, out value))
		{
			return value;
		}
		return null;
	}

	internal void Focus(IDockContent content)
	{
		if (m_lastFocusedContent != content)
		{
			if (m_lastFocusedContent != null)
			{
				m_lastFocusedContent.IsFocused = false;
			}
			if (content != null)
			{
				((DockContent)content).IsFocused = true;
			}
		}
	}

	private void OwnerWindowActivated(object sender, EventArgs e)
	{
	}

	private void ChildWindowClosing(object sender, CancelEventArgs e)
	{
		if (sender is FloatingWindow floatingWindow)
		{
			floatingWindow.Closing -= ChildWindowClosing;
			m_windows.Remove(floatingWindow);
		}
	}

	public IDockContent RegisterContent(object content, string ucid, DockTo dockSide)
	{
		return RegisterContent(content, ucid, dockSide, string.Empty, null);
	}

	public IDockContent RegisterContent(object content, string ucid, DockTo dockSide, string header, ImageSource icon)
	{
		if (m_registeredContents.ContainsKey(ucid))
		{
			throw new ArgumentException("Content with given id already exists, id/name must be unique");
		}
		DockContent dockContent = new DockContent(content, ucid, header, icon)
		{
			Settings = new ContentSettings(dockSide)
		};
		dockContent.IsFocusedChanged += DockContent_IsFocusedChanged;
		m_registeredContents.Add(ucid, dockContent);
		RegisteredContents.Add(dockContent);
		return dockContent;
	}

	private void DockContent_IsFocusedChanged(object sender, BooleanArgs e)
	{
		if (e.Value)
		{
			if (m_lastFocusedContent != sender)
			{
				if (m_lastFocusedContent != null && m_lastFocusedContent.IsFocused)
				{
					m_lastFocusedContent.IsFocused = false;
				}
				m_lastFocusedContent = (DockContent)sender;
			}
		}
		else if (m_lastFocusedContent == sender)
		{
			m_lastFocusedContent = null;
			Focus();
		}
	}

	public void ShowContent(object o)
	{
		foreach (DockContent registeredContent in RegisteredContents)
		{
			if (registeredContent.Content == o)
			{
				ShowContent(registeredContent);
				break;
			}
		}
	}

	public void ShowContent(string ucid)
	{
		if (base.IsLoaded)
		{
			if (!m_registeredContents.ContainsKey(ucid))
			{
				throw new ArgumentOutOfRangeException("Content with given name is not registered!");
			}
			ShowContent(m_registeredContents[ucid]);
		}
	}

	public void ShowContent(IDockContent content)
	{
		if (!base.IsLoaded || !(content is DockContent dockContent))
		{
			return;
		}
		if (!m_registeredContents.ContainsValue(dockContent))
		{
			throw new ArgumentOutOfRangeException("Given content is not registered!");
		}
		if (!((IDockLayout)this).HasDescendant(content))
		{
			ContentSettings settings = dockContent.Settings;
			if (settings.Size == new Size(0.0, 0.0))
			{
				double num = ((settings.DefaultDock == DockTo.Center) ? 0.8 : 0.2);
				settings.Size = new Size(base.ActualWidth * num, base.ActualHeight * num);
			}
			switch (settings.DockState)
			{
			case DockState.Docked:
				if (GridLayout == null)
				{
					((IDockLayout)this).Dock((IDockContent)null, content, DockTo.Center);
				}
				else
				{
					Point point = new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0);
					DockTo defaultDock = settings.DefaultDock;
					switch (defaultDock)
					{
					case DockTo.Left:
						point.X = 1.0;
						break;
					case DockTo.Right:
						point.X = base.ActualWidth - 2.0;
						break;
					case DockTo.Top:
						point.Y = 1.0;
						break;
					case DockTo.Bottom:
						point.Y = base.ActualHeight - 2.0;
						break;
					}
					point = PointToScreen(point);
					DockContent dockContent2 = ((IDockLayout)this).HitTest(point);
					if (dockContent2 != null)
					{
						DockTo defaultDock2 = dockContent2.Settings.DefaultDock;
						if (defaultDock2 == defaultDock)
						{
							((IDockLayout)this).Dock((IDockContent)dockContent2, content, DockTo.Center);
						}
						else if ((defaultDock == DockTo.Center && defaultDock2 == DockTo.Right) || (defaultDock == DockTo.Left && (defaultDock2 == DockTo.Center || defaultDock2 == DockTo.Right)))
						{
							((IDockLayout)this).Dock((IDockContent)dockContent2, content, DockTo.Left);
						}
						else if ((defaultDock == DockTo.Center && defaultDock2 == DockTo.Left) || (defaultDock == DockTo.Right && (defaultDock2 == DockTo.Center || defaultDock2 == DockTo.Left)))
						{
							((IDockLayout)this).Dock((IDockContent)dockContent2, content, DockTo.Right);
						}
						else if ((defaultDock == DockTo.Center && defaultDock2 == DockTo.Bottom) || (defaultDock == DockTo.Top && (defaultDock2 == DockTo.Center || defaultDock2 == DockTo.Bottom)))
						{
							((IDockLayout)this).Dock((IDockContent)dockContent2, content, DockTo.Top);
						}
						else if ((defaultDock == DockTo.Center && defaultDock2 == DockTo.Top) || (defaultDock == DockTo.Bottom && (defaultDock2 == DockTo.Center || defaultDock2 == DockTo.Top)))
						{
							((IDockLayout)this).Dock((IDockContent)dockContent2, content, DockTo.Bottom);
						}
						else
						{
							((IDockLayout)this).Dock((IDockContent)null, content, defaultDock);
						}
					}
					else
					{
						((IDockLayout)this).Dock((IDockContent)null, content, defaultDock);
					}
				}
				UpdateLayout();
				break;
			case DockState.Floating:
			{
				FloatingWindow floatingWindow = new FloatingWindow(this, content, settings.Location, settings.Size);
				floatingWindow.Closing += ChildWindowClosing;
				m_windows.Add(floatingWindow);
				floatingWindow.Owner = Window.GetWindow(this);
				floatingWindow.Show();
				break;
			}
			case DockState.Collapsed:
				Collapse(content);
				break;
			}
		}
		Focus(content);
	}

	public void HideContent(string ucid)
	{
		if (base.IsLoaded)
		{
			if (!m_registeredContents.ContainsKey(ucid))
			{
				throw new ArgumentOutOfRangeException("Specified content name is not registered");
			}
			HideContent(m_registeredContents[ucid]);
		}
	}

	public void HideContent(IDockContent content)
	{
		if (!base.IsLoaded || !(content is DockContent dockContent))
		{
			return;
		}
		if (!m_registeredContents.ContainsValue(dockContent))
		{
			throw new ArgumentOutOfRangeException("Specified content is not registered");
		}
		if (GridLayout != null && GridLayout.HasDescendant(content))
		{
			GridLayout.Undock(content);
		}
		else if (PART_LeftCollapsePanel.HasChild(content))
		{
			PART_LeftCollapsePanel.Undock(content);
		}
		else if (PART_TopCollapsePanel.HasChild(content))
		{
			PART_TopCollapsePanel.Undock(content);
		}
		else if (PART_RightCollapsePanel.HasChild(content))
		{
			PART_RightCollapsePanel.Undock(content);
		}
		else if (PART_BottomCollapsePanel.HasChild(content))
		{
			PART_BottomCollapsePanel.Undock(content);
		}
		else
		{
			foreach (FloatingWindow window in m_windows)
			{
				if (window.HasDescendant(content))
				{
					window.Undock(content);
					break;
				}
			}
		}
		dockContent.IsVisible = ((IDockLayout)this).HasDescendant(content);
	}

	public void UnregisterContent(string ucid)
	{
		if (!m_registeredContents.ContainsKey(ucid))
		{
			throw new ArgumentOutOfRangeException("Specified content name is not registered");
		}
		HideContent(ucid);
		m_registeredContents[ucid].IsFocusedChanged -= DockContent_IsFocusedChanged;
		if (m_lastFocusedContent == m_registeredContents[ucid])
		{
			m_lastFocusedContent = null;
		}
		RegisteredContents.Remove(m_registeredContents[ucid]);
		m_registeredContents.Remove(ucid);
	}

	public void UnregisterContent(IDockContent content)
	{
		if (!(content is DockContent value))
		{
			throw new ArgumentException("Invalid content");
		}
		if (!m_registeredContents.ContainsValue(value))
		{
			throw new ArgumentOutOfRangeException("Specified content is not registered");
		}
		UnregisterContent(content.UID);
	}

	private void CacheLayout(XmlReader reader)
	{
		if (reader == null)
		{
			m_layoutToApply = null;
		}
		else if (reader.ReadToFollowing(GetType().Name, GetType().Namespace))
		{
			string data = reader.ReadOuterXml();
			MemoryStream memoryStream = new MemoryStream();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				Encoding = Encoding.UTF8
			};
			XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings);
			xmlWriter.WriteRaw(data);
			xmlWriter.Close();
			XmlReaderSettings settings2 = new XmlReaderSettings
			{
				IgnoreWhitespace = true
			};
			memoryStream.Seek(0L, SeekOrigin.Begin);
			m_layoutToApply = XmlReader.Create(memoryStream, settings2);
		}
	}

	public void ApplyLayout(XmlReader reader)
	{
		try
		{
			if (reader == null)
			{
				PerformDefaultLayout();
				return;
			}
			if (!m_templateApplied)
			{
				CacheLayout(reader);
				return;
			}
			if (m_layoutApplied)
			{
				((IDockLayout)this).Close();
			}
			if (!reader.ReadToFollowing(GetType().Name, GetType().Namespace))
			{
				return;
			}
			reader.ReadStartElement();
			if (reader.LocalName == "Contents")
			{
				if (reader.ReadToDescendant("Content"))
				{
					do
					{
						string attribute = reader.GetAttribute("UCID");
						DockContent content = GetContent(attribute);
						if (content != null)
						{
							content.Settings.DockState = (DockState)Enum.Parse(typeof(DockState), reader.GetAttribute(typeof(DockState).Name));
							try
							{
								string attribute2 = reader.GetAttribute("Location");
								content.Settings.Location = Point.Parse(attribute2);
							}
							catch (FormatException)
							{
							}
							try
							{
								string attribute3 = reader.GetAttribute("Size");
								content.Settings.Size = Size.Parse(attribute3);
							}
							catch (FormatException)
							{
							}
						}
					}
					while (reader.ReadToNextSibling("Content"));
					reader.ReadEndElement();
				}
				else if (reader.IsEmptyElement)
				{
					reader.Read();
				}
			}
			if (reader.LocalName == typeof(GridLayout).Name)
			{
				GridLayout gridLayout = new GridLayout(this, reader.ReadSubtree());
				GridLayout = ((gridLayout.Layouts.Count > 0) ? gridLayout : null);
				reader.ReadEndElement();
			}
			if (reader.LocalName == typeof(FloatingWindow).Name)
			{
				do
				{
					FloatingWindow floatingWindow = new FloatingWindow(this, reader.ReadSubtree());
					if (floatingWindow.DockedContent.Children.Count > 0)
					{
						floatingWindow.Closing += ChildWindowClosing;
						m_windows.Add(floatingWindow);
						floatingWindow.Owner = Window.GetWindow(this);
						floatingWindow.Show();
						floatingWindow.Left = floatingWindow.Left;
						floatingWindow.Top = floatingWindow.Top;
					}
					else
					{
						floatingWindow.Owner = Window.GetWindow(this);
					}
				}
				while (reader.ReadToNextSibling(typeof(FloatingWindow).Name));
				reader.ReadEndElement();
			}
			if (reader.LocalName == typeof(SidePopup).Name)
			{
				do
				{
					switch ((Dock)Enum.Parse(typeof(Dock), reader.GetAttribute("Side")))
					{
					case Dock.Left:
						PART_LeftCollapsePanel.ReadXml(reader.ReadSubtree());
						break;
					case Dock.Top:
						PART_TopCollapsePanel.ReadXml(reader.ReadSubtree());
						break;
					case Dock.Right:
						PART_RightCollapsePanel.ReadXml(reader.ReadSubtree());
						break;
					case Dock.Bottom:
						PART_BottomCollapsePanel.ReadXml(reader.ReadSubtree());
						break;
					}
				}
				while (reader.ReadToNextSibling(typeof(SidePopup).Name));
				reader.ReadEndElement();
			}
			m_layoutApplied = true;
		}
		catch (Exception ex3)
		{
			((IDockLayout)this).Close();
			PerformDefaultLayout();
			throw ex3;
		}
	}

	private void PerformDefaultLayout()
	{
		if (!base.IsLoaded)
		{
			return;
		}
		foreach (DockContent value in m_registeredContents.Values)
		{
			((IDockLayout)this).Undock((IDockContent)value);
		}
		GridLayout = null;
		foreach (DockContent value2 in m_registeredContents.Values)
		{
			ShowContent(value2);
		}
		m_layoutApplied = true;
	}

	public void SaveLayout(XmlWriter writer)
	{
		if (!base.IsLoaded)
		{
			return;
		}
		Type type = GetType();
		writer.WriteStartElement(type.Name, type.Namespace);
		writer.WriteStartElement("Contents");
		foreach (KeyValuePair<string, DockContent> registeredContent in m_registeredContents)
		{
			writer.WriteStartElement("Content");
			ContentSettings settings = registeredContent.Value.Settings;
			writer.WriteAttributeString("UCID", registeredContent.Key);
			writer.WriteAttributeString(settings.DefaultDock.GetType().Name, settings.DefaultDock.ToString());
			writer.WriteAttributeString(settings.DockState.GetType().Name, settings.DockState.ToString());
			writer.WriteAttributeString("Location", settings.Location.ToString(CultureInfo.InvariantCulture));
			writer.WriteAttributeString("Size", settings.Size.ToString(CultureInfo.InvariantCulture));
			writer.WriteEndElement();
		}
		writer.WriteEndElement();
		if (GridLayout != null)
		{
			GridLayout.WriteXml(writer);
		}
		foreach (FloatingWindow window in m_windows)
		{
			window.WriteXml(writer);
		}
		PART_LeftCollapsePanel.WriteXml(writer);
		PART_TopCollapsePanel.WriteXml(writer);
		PART_RightCollapsePanel.WriteXml(writer);
		PART_BottomCollapsePanel.WriteXml(writer);
		writer.WriteEndElement();
	}

	private IDockLayout GetContentParent(IDockContent content)
	{
		FrameworkElement frameworkElement = content as FrameworkElement;
		FrameworkElement frameworkElement2 = frameworkElement.Parent as FrameworkElement;
		while (frameworkElement2 != null && !(frameworkElement2 is IDockLayout))
		{
			frameworkElement2 = frameworkElement2.Parent as FrameworkElement;
		}
		return frameworkElement2 as IDockLayout;
	}

	internal void Drag(IDockLayout source, IDockContent content)
	{
		FrameworkElement frameworkElement = (FrameworkElement)source;
		ContentSettings contentSettings = ((content is TabLayout) ? ((TabLayout)content).Children[0].Settings : ((DockContent)content).Settings);
		Size size = new Size(frameworkElement.ActualWidth, frameworkElement.ActualHeight);
		Point point = new Point(contentSettings.Size.Width / 2.0, 3.0);
		Point position = Mouse.GetPosition(this);
		position = PointToScreen(position);
		Matrix transformToDevice = PresentationSource.FromVisual(Window.GetWindow(this)).CompositionTarget.TransformToDevice;
		if (transformToDevice != Matrix.Identity)
		{
			transformToDevice.Invert();
			position = transformToDevice.Transform(position);
		}
		point = Mouse.GetPosition(frameworkElement);
		point.Y = ((point.Y > 20.0) ? 10.0 : point.Y);
		position = new Point(position.X - point.X, position.Y - point.Y);
		if (Mouse.LeftButton == MouseButtonState.Pressed)
		{
			source.Undock(content);
			FloatingWindow floatingWindow = new FloatingWindow(this, content, position, size);
			floatingWindow.Closing += ChildWindowClosing;
			m_windows.Add(floatingWindow);
			floatingWindow.Owner = Window.GetWindow(this);
			floatingWindow.Show();
			floatingWindow.DragMove();
		}
	}

	internal void UnCollapse(IDockContent content)
	{
		TabLayout tabLayout = (TabLayout)content;
		while (tabLayout.Children.Count > 0)
		{
			DockContent dockContent = tabLayout.Children[0];
			DockTo dockTo = DockTo.Center;
			if (PART_LeftCollapsePanel.HasChild(dockContent))
			{
				PART_LeftCollapsePanel.Undock(dockContent);
				dockTo = DockTo.Left;
			}
			if (PART_TopCollapsePanel.HasChild(dockContent))
			{
				PART_TopCollapsePanel.Undock(dockContent);
				dockTo = DockTo.Top;
			}
			if (PART_RightCollapsePanel.HasChild(dockContent))
			{
				PART_RightCollapsePanel.Undock(dockContent);
				dockTo = DockTo.Right;
			}
			if (PART_BottomCollapsePanel.HasChild(dockContent))
			{
				PART_BottomCollapsePanel.Undock(dockContent);
				dockTo = DockTo.Bottom;
			}
			dockContent.Settings.DockState = DockState.Docked;
			((IDockLayout)this).Dock((IDockContent)null, (IDockContent)dockContent, dockTo);
		}
	}

	internal void Collapse(IDockContent content)
	{
		Image image = null;
		Rect rect = Rect.Empty;
		FrameworkElement frameworkElement = null;
		if (GridLayout != null && GridLayout.HasDescendant(content))
		{
			FrameworkElement frameworkElement2 = (FrameworkElement)content;
			frameworkElement = (FrameworkElement)frameworkElement2.Parent;
			if (frameworkElement != null)
			{
				rect = new Rect(frameworkElement.PointToScreen(new Point(0.0, 0.0)), frameworkElement.RenderSize);
				VisualBrush brush = new VisualBrush(frameworkElement);
				DrawingVisual drawingVisual = new DrawingVisual();
				DrawingContext drawingContext = drawingVisual.RenderOpen();
				drawingContext.DrawRectangle(brush, null, new Rect(0.0, 0.0, rect.Width, rect.Height));
				drawingContext.Close();
				RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)rect.Width, (int)rect.Height, 96.0, 96.0, PixelFormats.Default);
				renderTargetBitmap.Render(drawingVisual);
				image = new Image();
				image.Source = renderTargetBitmap;
				if (GridLayout != null)
				{
					GridLayout.Undock(content);
				}
				m_lastFocusedContent = null;
			}
		}
		DockContent dockContent = ((content is TabLayout) ? ((TabLayout)content).Children[0] : ((DockContent)content));
		ContentSettings settings = dockContent.Settings;
		Rect to;
		if (settings.Location.X > settings.Location.Y)
		{
			if (1.0 - settings.Location.X > settings.Location.Y)
			{
				PART_TopCollapsePanel.Dock(null, content, DockTo.Top);
				to = PART_TopCollapsePanel.RectForContent(dockContent);
			}
			else
			{
				PART_RightCollapsePanel.Dock(null, content, DockTo.Right);
				to = PART_RightCollapsePanel.RectForContent(dockContent);
			}
		}
		else if (1.0 - settings.Location.X > settings.Location.Y)
		{
			PART_LeftCollapsePanel.Dock(null, content, DockTo.Left);
			to = PART_LeftCollapsePanel.RectForContent(dockContent);
		}
		else
		{
			PART_BottomCollapsePanel.Dock(null, content, DockTo.Bottom);
			to = PART_BottomCollapsePanel.RectForContent(dockContent);
		}
		if (frameworkElement != null)
		{
			AnimateCollapse(image, rect, to);
		}
	}

	private void AnimateCollapse(FrameworkElement element, Rect from, Rect to)
	{
		Duration duration = new Duration(TimeSpan.FromSeconds(0.33));
		DockIconsLayer.ClearChildren();
		element.Opacity = 1.0;
		Point point = PointFromScreen(from.TopLeft);
		Point point2 = PointFromScreen(to.TopLeft);
		Canvas.SetLeft(element, point.X);
		Canvas.SetTop(element, point.Y);
		DockIconsLayer.AddChild(element);
		Storyboard storyboard = new Storyboard();
		storyboard.Children.Add(CreateDoubleAnimation(point.X, point2.X, duration, Canvas.LeftProperty, element));
		storyboard.Children.Add(CreateDoubleAnimation(point.Y, point2.Y, duration, Canvas.TopProperty, element));
		storyboard.Children.Add(CreateDoubleAnimation(from.Width, to.Width, duration, FrameworkElement.WidthProperty, element));
		storyboard.Children.Add(CreateDoubleAnimation(from.Height, to.Height, duration, FrameworkElement.HeightProperty, element));
		storyboard.Completed += CollapseCompleted;
		storyboard.Begin();
	}

	private DoubleAnimation CreateDoubleAnimation(double from, double to, Duration duration, DependencyProperty property, FrameworkElement target)
	{
		DoubleAnimation doubleAnimation = new DoubleAnimation(from, to, duration);
		Storyboard.SetTarget(doubleAnimation, target);
		Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath(property));
		return doubleAnimation;
	}

	private void CollapseCompleted(object sender, EventArgs e)
	{
		DockIconsLayer.ClearChildren();
	}

	internal List<IDockable> FindElementsAt(MouseEventArgs e)
	{
		bool flag = false;
		List<IDockable> list = new List<IDockable>();
		Window window = Window.GetWindow(this);
		foreach (Window ownedWindow in window.OwnedWindows)
		{
			IInputElement inputElement = ownedWindow.InputHitTest(e.GetPosition(ownedWindow));
			if (inputElement == null || ownedWindow is DockIconsLayer || ownedWindow.IsMouseCaptured)
			{
				continue;
			}
			if (ownedWindow is FloatingWindow)
			{
				for (DependencyObject dependencyObject = inputElement as DependencyObject; dependencyObject != null; dependencyObject = VisualTreeHelper.GetParent(dependencyObject))
				{
					if (dependencyObject is IDockable)
					{
						list.Add((IDockable)dependencyObject);
					}
				}
			}
			flag = true;
			break;
		}
		if (!flag)
		{
			IInputElement inputElement2 = window.InputHitTest(e.GetPosition(window));
			if (inputElement2 != null)
			{
				for (DependencyObject dependencyObject2 = inputElement2 as DependencyObject; dependencyObject2 != null; dependencyObject2 = ((!(dependencyObject2 is Visual)) ? LogicalTreeHelper.GetParent(dependencyObject2) : VisualTreeHelper.GetParent(dependencyObject2)))
				{
					if (dependencyObject2 is IDockable)
					{
						list.Add((IDockable)dependencyObject2);
					}
				}
			}
		}
		return list;
	}

	DockContent IDockLayout.HitTest(Point position)
	{
		if (GridLayout != null)
		{
			return GridLayout.HitTest(position);
		}
		return null;
	}

	bool IDockLayout.HasChild(IDockContent content)
	{
		if (!base.IsLoaded)
		{
			return false;
		}
		if (GridLayout != null && GridLayout.HasChild(content))
		{
			return true;
		}
		foreach (FloatingWindow window in m_windows)
		{
			if (window.HasChild(content))
			{
				return true;
			}
		}
		if (PART_LeftCollapsePanel.HasChild(content))
		{
			return true;
		}
		if (PART_TopCollapsePanel.HasChild(content))
		{
			return true;
		}
		if (PART_RightCollapsePanel.HasChild(content))
		{
			return true;
		}
		if (PART_BottomCollapsePanel.HasChild(content))
		{
			return true;
		}
		return false;
	}

	bool IDockLayout.HasDescendant(IDockContent content)
	{
		if (!base.IsLoaded)
		{
			return false;
		}
		if (GridLayout != null && GridLayout.HasDescendant(content))
		{
			return true;
		}
		foreach (FloatingWindow window in m_windows)
		{
			if (window.HasDescendant(content))
			{
				return true;
			}
		}
		if (PART_LeftCollapsePanel.HasDescendant(content))
		{
			return true;
		}
		if (PART_TopCollapsePanel.HasDescendant(content))
		{
			return true;
		}
		if (PART_RightCollapsePanel.HasDescendant(content))
		{
			return true;
		}
		if (PART_BottomCollapsePanel.HasDescendant(content))
		{
			return true;
		}
		return false;
	}

	void IDockLayout.Dock(IDockContent nextTo, IDockContent newContent, DockTo dockTo)
	{
		if (GridLayout == null)
		{
			GridLayout = new GridLayout(this);
			GridLayout.Dock(null, newContent, DockTo.Center);
			return;
		}
		if (nextTo == null)
		{
			if (GridLayout.Children.Count < 2)
			{
				GridLayout.Dock(null, newContent, dockTo);
			}
			else
			{
				if (dockTo == DockTo.Center)
				{
					dockTo = DockTo.Right;
				}
				GridLayout gridLayout = GridLayout;
				GridLayout = null;
				gridLayout = (GridLayout = new GridLayout(this, gridLayout));
				gridLayout.Dock(null, newContent, dockTo);
			}
		}
		else
		{
			GridLayout.Dock(nextTo, newContent, dockTo);
		}
		CheckConsistency();
	}

	void IDockLayout.Undock(IDockContent content)
	{
		if (GridLayout != null && GridLayout.HasDescendant(content))
		{
			GridLayout.Undock(content);
		}
	}

	void IDockLayout.Undock(IDockLayout child)
	{
		if (GridLayout == child)
		{
			GridLayout = null;
		}
	}

	void IDockLayout.Replace(IDockLayout oldLayout, IDockLayout newLayout)
	{
		if (GridLayout == oldLayout)
		{
			GridLayout = (GridLayout)newLayout;
		}
	}

	void IDockLayout.Close()
	{
		if (GridLayout != null)
		{
			GridLayout.Close();
			GridLayout = null;
		}
		while (m_windows.Count > 0)
		{
			m_windows[0].Close();
		}
		m_windows.Clear();
		PART_LeftCollapsePanel.Close();
		PART_TopCollapsePanel.Close();
		PART_RightCollapsePanel.Close();
		PART_BottomCollapsePanel.Close();
	}

	IDockLayout IDockLayout.FindParentLayout(IDockContent content)
	{
		if (base.IsLoaded)
		{
			if (GridLayout != null && GridLayout.HasDescendant(content))
			{
				return ((IDockLayout)GridLayout).FindParentLayout(content);
			}
			if (PART_LeftCollapsePanel.HasDescendant(content))
			{
				return ((IDockLayout)PART_LeftCollapsePanel).FindParentLayout(content);
			}
			if (PART_TopCollapsePanel.HasDescendant(content))
			{
				return ((IDockLayout)PART_TopCollapsePanel).FindParentLayout(content);
			}
			if (PART_RightCollapsePanel.HasDescendant(content))
			{
				return ((IDockLayout)PART_RightCollapsePanel).FindParentLayout(content);
			}
			if (PART_BottomCollapsePanel.HasDescendant(content))
			{
				return ((IDockLayout)PART_BottomCollapsePanel).FindParentLayout(content);
			}
			foreach (FloatingWindow window in m_windows)
			{
				if (window.HasDescendant(content))
				{
					return ((IDockLayout)window).FindParentLayout(content);
				}
			}
		}
		return null;
	}

	void IDockable.DockDragEnter(object sender, DockDragDropEventArgs e)
	{
		Point point = new Point(base.ActualWidth / 2.0, base.ActualHeight / 2.0);
		int num = (int)DockIconSize.Width / 4;
		ResourceDictionary resourceDictionary = new ResourceDictionary();
		resourceDictionary.Source = new Uri("/Atf.Gui.Wpf;component/Resources/DockIcons.xaml", UriKind.Relative);
		if (m_dockLeftIcon == null)
		{
			m_dockLeftIcon = new DockIcon((Style)resourceDictionary["DockLeftIcon"], DockIconSize);
			m_dockRightIcon = new DockIcon((Style)resourceDictionary["DockRightIcon"], DockIconSize);
			m_dockTopIcon = new DockIcon((Style)resourceDictionary["DockTopIcon"], DockIconSize);
			m_dockBottomIcon = new DockIcon((Style)resourceDictionary["DockBottomIcon"], DockIconSize);
			m_dockTabIcon = new DockIcon((Style)resourceDictionary["DockTabIcon"], DockIconSize);
		}
		m_dockLeftIcon.Offset = new Point(num, point.Y - DockIconSize.Height / 2.0);
		m_dockRightIcon.Offset = new Point(base.ActualWidth - DockIconSize.Width - (double)num, point.Y - DockIconSize.Height / 2.0);
		m_dockTopIcon.Offset = new Point(point.X - DockIconSize.Width / 2.0, num);
		m_dockBottomIcon.Offset = new Point(point.X - DockIconSize.Width / 2.0, base.ActualHeight - DockIconSize.Height - (double)num);
		m_dockTabIcon.Offset = new Point(point.X - DockIconSize.Width / 2.0, point.Y - DockIconSize.Height / 2.0);
		if (GridLayout != null)
		{
			DockIconsLayer.AddChild(m_dockLeftIcon);
			DockIconsLayer.AddChild(m_dockRightIcon);
			DockIconsLayer.AddChild(m_dockTopIcon);
			DockIconsLayer.AddChild(m_dockBottomIcon);
		}
		else
		{
			DockIconsLayer.AddChild(m_dockTabIcon);
		}
	}

	void IDockable.DockDragOver(object sender, DockDragDropEventArgs e)
	{
		DockTo? dockPreview = m_dockPreview;
		m_dockPreview = null;
		Point position = e.MouseEventArgs.GetPosition(this);
		if (GridLayout != null)
		{
			m_dockLeftIcon.Highlight = m_dockLeftIcon.HitTest(position);
			m_dockRightIcon.Highlight = m_dockRightIcon.HitTest(position);
			m_dockTopIcon.Highlight = m_dockTopIcon.HitTest(position);
			m_dockBottomIcon.Highlight = m_dockBottomIcon.HitTest(position);
			m_dockPreview = (m_dockLeftIcon.Highlight ? new DockTo?(DockTo.Left) : m_dockPreview);
			m_dockPreview = (m_dockRightIcon.Highlight ? new DockTo?(DockTo.Right) : m_dockPreview);
			m_dockPreview = (m_dockTopIcon.Highlight ? new DockTo?(DockTo.Top) : m_dockPreview);
			m_dockPreview = (m_dockBottomIcon.Highlight ? new DockTo?(DockTo.Bottom) : m_dockPreview);
		}
		else
		{
			m_dockTabIcon.Highlight = m_dockTabIcon.HitTest(position);
			m_dockPreview = (m_dockTabIcon.Highlight ? new DockTo?(DockTo.Center) : m_dockPreview);
		}
		if (m_dockPreview.HasValue)
		{
			if (dockPreview != m_dockPreview)
			{
				DockIconsLayer.RemoveChild(m_dockPreviewShape);
				m_dockPreviewShape = null;
				Window window = Window.GetWindow(this);
				Rectangle rectangle = new Rectangle();
				rectangle.Fill = Brushes.RoyalBlue;
				rectangle.Opacity = 0.3;
				FrameworkElement frameworkElement = rectangle;
				double num = 2.0;
				Point point = new Point(num, num);
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
					frameworkElement.Width = base.ActualWidth - num * 2.0;
					frameworkElement.Height = base.ActualHeight - num * 2.0;
					Canvas.SetLeft(frameworkElement, point.X);
					Canvas.SetTop(frameworkElement, point.Y);
					break;
				}
				DockIconsLayer.InsertChild(0, frameworkElement);
				m_dockPreviewShape = frameworkElement;
			}
		}
		else if (m_dockPreviewShape != null)
		{
			DockIconsLayer.RemoveChild(m_dockPreviewShape);
			m_dockPreviewShape = null;
		}
	}

	void IDockable.DockDragLeave(object sender, DockDragDropEventArgs e)
	{
		DockIconsLayer.RemoveChild(m_dockLeftIcon);
		DockIconsLayer.RemoveChild(m_dockRightIcon);
		DockIconsLayer.RemoveChild(m_dockTopIcon);
		DockIconsLayer.RemoveChild(m_dockBottomIcon);
		DockIconsLayer.RemoveChild(m_dockTabIcon);
		if (m_dockPreviewShape != null)
		{
			DockIconsLayer.RemoveChild(m_dockPreviewShape);
			m_dockPreviewShape = null;
		}
		DockIconsLayer.CloseIfEmpty();
	}

	void IDockable.DockDrop(object sender, DockDragDropEventArgs e)
	{
		if (e.Content is TabLayout)
		{
			foreach (DockContent child in ((TabLayout)e.Content).Children)
			{
				child.Settings.DockState = DockState.Docked;
			}
		}
		else
		{
			ContentSettings contentSettings = ((e.Content is TabLayout) ? ((TabLayout)e.Content).Children[0].Settings : ((DockContent)e.Content).Settings);
			contentSettings.DockState = DockState.Docked;
		}
		((IDockLayout)this).Dock((IDockContent)null, e.Content, m_dockPreview.Value);
	}
}
