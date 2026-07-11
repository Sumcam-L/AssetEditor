using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Firaxis.CivTech;
using Sce.Atf.Applications;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisDockPaneStrip : DockPaneStripBase
{
	private const int WmPaint = 0x000F;
	private const int WmEraseBackground = 0x0014;

	private IDockContent m_tracedActiveContent;
	private IDockContent m_tracedPreviousContent;

	private class TabFiraxis : Tab
	{
		private int m_tabX;

		private int m_tabWidth;

		private int m_maxWidth;

		private bool m_flag;

		public int TabX
		{
			get
			{
				return m_tabX;
			}
			set
			{
				m_tabX = value;
			}
		}

		public int TabWidth
		{
			get
			{
				return m_tabWidth;
			}
			set
			{
				m_tabWidth = value;
			}
		}

		public int MaxWidth
		{
			get
			{
				return m_maxWidth;
			}
			set
			{
				m_maxWidth = value;
			}
		}

		protected internal bool Flag
		{
			get
			{
				return m_flag;
			}
			set
			{
				m_flag = value;
			}
		}

		public TabFiraxis(IDockContent content)
			: base(content)
		{
		}
	}

	private sealed class InertButton : InertButtonBase
	{
		private Bitmap _hovered;

		private Bitmap _normal;

		private Bitmap _pressed;

		public override Bitmap Image => _normal;

		public override Bitmap HoverImage => _hovered;

		public override Bitmap PressImage => _pressed;

		public InertButton(Bitmap hovered, Bitmap normal, Bitmap pressed)
		{
			_hovered = hovered;
			_normal = normal;
			_pressed = pressed;
		}
	}

	private class DocumentTabSelectInfo
	{
		public IDockContent Content;

		public string Filename;

		public string FullPath;

		public string Project;

		public Image TypeImage;
	}

	private const int _ToolWindowStripGapTop = 0;

	private const int _ToolWindowStripGapBottom = 0;

	private const int _ToolWindowStripGapLeft = 0;

	private const int _ToolWindowStripGapRight = 0;

	private const int _ToolWindowImageHeight = 16;

	private const int _ToolWindowImageWidth = 16;

	private const int _ToolWindowImageGapTop = 3;

	private const int _ToolWindowImageGapBottom = 1;

	private const int _ToolWindowImageGapLeft = 2;

	private const int _ToolWindowImageGapRight = 0;

	private const int _ToolWindowTextGapRight = 3;

	private const int _ToolWindowTabSeperatorGapTop = 3;

	private const int _ToolWindowTabSeperatorGapBottom = 3;

	private const int _DocumentStripGapTop = 0;

	private const int _DocumentStripGapBottom = 1;

	private const int _DocumentTabMaxWidth = 200;

	private const int _DocumentButtonGapTop = 3;

	private const int _DocumentButtonGapBottom = 3;

	private const int _DocumentButtonGapBetween = 0;

	private const int _DocumentButtonGapRight = 3;

	private const int _DocumentTabGapTop = 3;

	private const int _DocumentTabGapLeft = 3;

	private const int _DocumentTabGapRight = 3;

	private const int _DocumentIconGapBottom = 2;

	private const int _DocumentIconGapLeft = 8;

	private const int _DocumentIconGapRight = 0;

	private const int _DocumentIconHeight = 16;

	private const int _DocumentIconWidth = 16;

	private const int _DocumentIconHeightLarge = 32;

	private const int _DocumentIconWidthLarge = 32;

	private const int _DocumentTextGapRight = 6;

	private ContextMenuStrip m_selectMenu;

	private InertButton m_buttonOverflow;

	private InertButton m_buttonWindowList;

	private IContainer m_components;

	private ToolTip m_toolTip;

	private Font m_font;

	private Font m_boldFont;

	private Font m_italicFont;

	private int m_startDisplayingTab = 0;

	private int m_endDisplayingTab = 0;

	private int m_firstDisplayingTab = 0;

	private bool m_documentTabsOverflow = false;

	private static string m_toolTipSelect;

	private Rectangle _activeClose;

	private int _selectMenuMargin = 5;

	private bool m_suspendDrag = false;

	private bool m_alwaysShowDocOverflow;

	private const int TAB_CLOSE_BUTTON_WIDTH = 30;

	private bool m_isMouseDown = false;

	private Regex externalProjectParser = new Regex("(.*)\\(.+\\)", RegexOptions.Compiled);

	private Regex readonlyParser = new Regex("(.*)\\(Read Only\\)", RegexOptions.Compiled);

	public bool AlwaysShowDocOverflow
	{
		get
		{
			return m_alwaysShowDocOverflow;
		}
		set
		{
			m_alwaysShowDocOverflow = value;
		}
	}

	private Rectangle TabStripRectangle
	{
		get
		{
			if (base.Appearance == DockPane.AppearanceStyle.Document)
			{
				return TabStripRectangle_Document;
			}
			return TabStripRectangle_ToolWindow;
		}
	}

	private Rectangle TabStripRectangle_ToolWindow
	{
		get
		{
			Rectangle clientRectangle = base.ClientRectangle;
			return new Rectangle(clientRectangle.X, clientRectangle.Top + ToolWindowStripGapTop, clientRectangle.Width, clientRectangle.Height - ToolWindowStripGapTop - ToolWindowStripGapBottom);
		}
	}

	private Rectangle TabStripRectangle_Document
	{
		get
		{
			Rectangle clientRectangle = base.ClientRectangle;
			return new Rectangle(clientRectangle.X, clientRectangle.Top + DocumentStripGapTop, clientRectangle.Width, clientRectangle.Height + DocumentStripGapTop - DocumentStripGapBottom);
		}
	}

	private Rectangle TabsRectangle
	{
		get
		{
			if (base.Appearance == DockPane.AppearanceStyle.ToolWindow)
			{
				return TabStripRectangle;
			}
			Rectangle tabStripRectangle = TabStripRectangle;
			int num = tabStripRectangle.X;
			int num2 = tabStripRectangle.Y;
			int num3 = tabStripRectangle.Width;
			int num4 = tabStripRectangle.Height;
			num += DocumentTabGapLeft;
			num3 -= DocumentTabGapLeft + DocumentTabGapRight + DocumentButtonGapRight + ButtonOverflow.Width + ButtonWindowList.Width + 2 * DocumentButtonGapBetween;
			return new Rectangle(num, num2, num3, num4);
		}
	}

	private ContextMenuStrip SelectMenu => m_selectMenu;

	public int SelectMenuMargin
	{
		get
		{
			return _selectMenuMargin;
		}
		set
		{
			_selectMenuMargin = value;
		}
	}

	private InertButton ButtonOverflow
	{
		get
		{
			if (m_buttonOverflow == null)
			{
				m_buttonOverflow = new InertButton(base.DockPane.DockPanel.Theme.ImageService.DockPaneHover_OptionOverflow, base.DockPane.DockPanel.Theme.ImageService.DockPane_OptionOverflow, base.DockPane.DockPanel.Theme.ImageService.DockPanePress_OptionOverflow);
				m_buttonOverflow.Click += WindowList_Click;
				base.Controls.Add(m_buttonOverflow);
			}
			return m_buttonOverflow;
		}
	}

	private InertButton ButtonWindowList
	{
		get
		{
			if (m_buttonWindowList == null)
			{
				m_buttonWindowList = new InertButton(base.DockPane.DockPanel.Theme.ImageService.DockPaneHover_List, base.DockPane.DockPanel.Theme.ImageService.DockPane_List, base.DockPane.DockPanel.Theme.ImageService.DockPanePress_List);
				m_buttonWindowList.Click += WindowList_Click;
				base.Controls.Add(m_buttonWindowList);
			}
			return m_buttonWindowList;
		}
	}

	private static GraphicsPath GraphicsPath => FiraxisAutoHideStrip.GraphicsPath;

	private IContainer Components => m_components;

	public Font TextFont => base.DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.TextFont;

	private Font ItalicFont
	{
		get
		{
			if (base.IsDisposed)
			{
				return null;
			}
			if (m_italicFont == null)
			{
				m_font = TextFont;
				m_italicFont = new Font(TextFont, FontStyle.Italic);
			}
			else if (m_font != TextFont)
			{
				m_italicFont.Dispose();
				m_font = TextFont;
				m_italicFont = new Font(TextFont, FontStyle.Italic);
			}
			return m_italicFont;
		}
	}

	private Font BoldFont
	{
		get
		{
			if (base.IsDisposed)
			{
				return null;
			}
			if (m_boldFont == null)
			{
				m_font = TextFont;
				m_boldFont = new Font(TextFont, FontStyle.Bold);
			}
			else if (m_font != TextFont)
			{
				m_boldFont.Dispose();
				m_font = TextFont;
				m_boldFont = new Font(TextFont, FontStyle.Bold);
			}
			return m_boldFont;
		}
	}

	private int StartDisplayingTab
	{
		get
		{
			return m_startDisplayingTab;
		}
		set
		{
			m_startDisplayingTab = value;
			Invalidate();
		}
	}

	private int EndDisplayingTab
	{
		get
		{
			return m_endDisplayingTab;
		}
		set
		{
			m_endDisplayingTab = value;
		}
	}

	private int FirstDisplayingTab
	{
		get
		{
			return m_firstDisplayingTab;
		}
		set
		{
			m_firstDisplayingTab = value;
		}
	}

	private bool DocumentTabsOverflow
	{
		set
		{
			if (m_documentTabsOverflow != value)
			{
				m_documentTabsOverflow = value;
				SetInertButtons();
			}
		}
	}

	private static int ToolWindowStripGapTop => 0;

	private static int ToolWindowStripGapBottom => 0;

	private static int ToolWindowStripGapLeft => 0;

	private static int ToolWindowStripGapRight => 0;

	private static int ToolWindowImageHeight => 16;

	private static int ToolWindowImageWidth => 16;

	private static int ToolWindowImageGapTop => 3;

	private static int ToolWindowImageGapBottom => 1;

	private static int ToolWindowImageGapLeft => 2;

	private static int ToolWindowImageGapRight => 0;

	private static int ToolWindowTextGapRight => 3;

	private static int ToolWindowTabSeperatorGapTop => 3;

	private static int ToolWindowTabSeperatorGapBottom => 3;

	private static Color DirtyDocumentTabOverlayColor => Color.FromArgb(96, Color.Red);

	private static string ToolTipSelect
	{
		get
		{
			if (m_toolTipSelect == null)
			{
				m_toolTipSelect = Strings.DockPaneStrip_ToolTipWindowList;
			}
			return m_toolTipSelect;
		}
	}

	private TextFormatFlags ToolWindowTextFormat
	{
		get
		{
			TextFormatFlags textFormatFlags = TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
			if (RightToLeft == RightToLeft.Yes)
			{
				return textFormatFlags | TextFormatFlags.RightToLeft | TextFormatFlags.Right;
			}
			return textFormatFlags;
		}
	}

	private static int DocumentStripGapTop => 0;

	private static int DocumentStripGapBottom => 1;

	private TextFormatFlags DocumentTextFormat
	{
		get
		{
			TextFormatFlags textFormatFlags = TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
			if (RightToLeft == RightToLeft.Yes)
			{
				return textFormatFlags | TextFormatFlags.RightToLeft;
			}
			return textFormatFlags;
		}
	}

	private static int DocumentTabMaxWidth => 200;

	private static int DocumentButtonGapTop => 3;

	private static int DocumentButtonGapBottom => 3;

	private static int DocumentButtonGapBetween => 0;

	private static int DocumentButtonGapRight => 3;

	private static int DocumentTabGapTop => 3;

	private static int DocumentTabGapLeft => 3;

	private static int DocumentTabGapRight => 3;

	private static int DocumentIconGapBottom => 2;

	private static int DocumentIconGapLeft => 8;

	private static int DocumentIconGapRight => 0;

	private static int DocumentIconWidth => 16;

	private static int DocumentIconHeight => 16;

	private static int DocumentTextGapRight => 6;

	private static int DocumentIconHeightLarge => 32;

	private static int DocumentIconWidthLarge => 32;

	private int CurrentDocumentIconWidth => base.DockPane.DockPanel.LargeDocumentIcon ? DocumentIconWidthLarge : DocumentIconWidth;

	private int CurrentDocumentIconHeight => base.DockPane.DockPanel.LargeDocumentIcon ? DocumentIconHeightLarge : DocumentIconHeight;

	protected bool IsMouseDown
	{
		get
		{
			return m_isMouseDown;
		}
		private set
		{
			if (m_isMouseDown != value)
			{
				m_isMouseDown = value;
				Invalidate();
			}
		}
	}

	private Rectangle ActiveClose => _activeClose;

	protected override Tab CreateTab(IDockContent content)
	{
		return new TabFiraxis(content);
	}

	public FiraxisDockPaneStrip(DockPane pane)
		: base(pane)
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		SuspendLayout();
		base.Height = CurrentDocumentIconHeight + DocumentIconGapBottom;
		m_components = new Container();
		m_toolTip = new ToolTip(Components);
		m_selectMenu = new ContextMenuStrip(Components);
		pane.DockPanel.Theme.ApplyTo(m_selectMenu);
		ResumeLayout();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Components.Dispose();
			m_boldFont?.Dispose();
			m_boldFont = null;
			m_italicFont?.Dispose();
			m_italicFont = null;
		}
		base.Dispose(disposing);
	}

	protected override int MeasureHeight()
	{
		if (base.Appearance == DockPane.AppearanceStyle.ToolWindow)
		{
			return MeasureHeight_ToolWindow();
		}
		return MeasureHeight_Document();
	}

	private int MeasureHeight_ToolWindow()
	{
		if (base.DockPane.IsAutoHide || base.Tabs.Count <= 1)
		{
			return 0;
		}
		return Math.Max(TextFont.Height + ((PatchController.EnableHighDpi == true) ? DocumentIconGapBottom : 0), ToolWindowImageHeight + ToolWindowImageGapTop + ToolWindowImageGapBottom) + ToolWindowStripGapTop + ToolWindowStripGapBottom;
	}

	private int MeasureHeight_Document()
	{
		return Math.Max(TextFont.Height + DocumentTabGapTop + ((PatchController.EnableHighDpi == true) ? DocumentIconGapBottom : 0), Math.Max(ButtonOverflow.Height, CurrentDocumentIconHeight) + DocumentButtonGapTop + DocumentButtonGapBottom) + DocumentStripGapBottom + DocumentStripGapTop;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		TraceActiveContentChange();
		TraceTabStrip("paint-begin", e.ClipRectangle);
		try
		{
			base.OnPaint(e);
			CalculateTabs();
			if (base.Appearance == DockPane.AppearanceStyle.Document && base.DockPane.ActiveContent != null && EnsureDocumentTabVisible(base.DockPane.ActiveContent, repaint: false))
			{
				CalculateTabs();
			}
			DrawTabStrip(e.Graphics);
		}
		finally
		{
			TraceTabStrip("paint-end", e.ClipRectangle);
		}
	}

	protected override void OnRefreshChanges()
	{
		IDockContent previousContent = m_tracedActiveContent;
		bool activeContentChanged = TraceActiveContentChange();
		TraceTabStrip("refresh", null);
		SetInertButtons();
		base.Height = MeasureHeight();
		if (activeContentChanged && base.Appearance == DockPane.AppearanceStyle.Document)
			RepaintChangedDocumentTabs(previousContent, base.DockPane.ActiveContent);
		Invalidate();
	}

	protected override void WndProc(ref Message m)
	{
		bool tracePaint = m.Msg == WmPaint || m.Msg == WmEraseBackground;
		if (tracePaint)
			TraceTabStrip("native-before", null, m.Msg);
		try
		{
			base.WndProc(ref m);
		}
		finally
		{
			if (tracePaint)
				TraceTabStrip("native-after", null, m.Msg);
		}
	}

	private bool TraceActiveContentChange()
	{
		IDockContent current = base.DockPane.ActiveContent;
		if (ReferenceEquals(m_tracedActiveContent, current))
			return false;

		m_tracedPreviousContent = m_tracedActiveContent;
		m_tracedActiveContent = current;
		if (DocumentSwitchTrace.IsActive)
			TraceTabStrip("active-change", null);
		return true;
	}

	private void RepaintChangedDocumentTabs(IDockContent previous, IDockContent current)
	{
		CalculateTabs();
		Rectangle repaintRectangle = GetTraceRectangle(previous);
		repaintRectangle = Rectangle.Union(repaintRectangle, GetTraceRectangle(current));
		repaintRectangle.Intersect(TabsRectangle);
		if (repaintRectangle.IsEmpty)
			return;

		Invalidate(DrawHelper.RtlTransform(this, repaintRectangle));
		Update();
	}

	private void TraceTabStrip(string phase, Rectangle? clip, int message = 0)
	{
		if (!DocumentSwitchTrace.IsActive)
			return;

		DocumentSwitchTrace.TraceTabStrip(phase, string.Format(
			"message={0}, clip={1}, bounds={2}, old={3}, new={4}, oldRect={5}, newRect={6}",
			message == WmPaint ? "WM_PAINT" : message == WmEraseBackground ? "WM_ERASEBKGND" : "none",
			clip.HasValue ? clip.Value.ToString() : "none", ClientRectangle,
			GetTraceIdentity(m_tracedPreviousContent), GetTraceIdentity(m_tracedActiveContent),
			GetTraceRectangle(m_tracedPreviousContent), GetTraceRectangle(m_tracedActiveContent)));
	}

	private Rectangle GetTraceRectangle(IDockContent content)
	{
		if (content == null)
			return Rectangle.Empty;
		for (int i = 0; i < base.Tabs.Count; i++)
		{
			if (ReferenceEquals(base.Tabs[i].Content, content))
				return GetTabRectangle(i);
		}
		return Rectangle.Empty;
	}

	private static string GetTraceIdentity(IDockContent content)
	{
		if (content == null)
			return "null";
		try
		{
			return string.Format("{0}:{1}", content.DockHandler.Form.Name, content.DockHandler.TabText);
		}
		catch
		{
			return "unavailable";
		}
	}

	public override GraphicsPath GetOutline(int index)
	{
		if (base.Appearance == DockPane.AppearanceStyle.Document)
		{
			return GetOutline_Document(index);
		}
		return GetOutline_ToolWindow(index);
	}

	private GraphicsPath GetOutline_Document(int index)
	{
		Rectangle tabRectangle = GetTabRectangle(index);
		tabRectangle.X -= tabRectangle.Height / 2;
		tabRectangle.Intersect(TabsRectangle);
		tabRectangle = RectangleToScreen(DrawHelper.RtlTransform(this, tabRectangle));
		Rectangle rectangle = base.DockPane.RectangleToScreen(base.DockPane.ClientRectangle);
		GraphicsPath graphicsPath = new GraphicsPath();
		GraphicsPath tabOutline_Document = GetTabOutline_Document(base.Tabs[index], rtlTransform: true, toScreen: true, full: true);
		graphicsPath.AddPath(tabOutline_Document, connect: true);
		if (base.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
		{
			graphicsPath.AddLine(tabRectangle.Right, tabRectangle.Top, rectangle.Right, tabRectangle.Top);
			graphicsPath.AddLine(rectangle.Right, tabRectangle.Top, rectangle.Right, rectangle.Top);
			graphicsPath.AddLine(rectangle.Right, rectangle.Top, rectangle.Left, rectangle.Top);
			graphicsPath.AddLine(rectangle.Left, rectangle.Top, rectangle.Left, tabRectangle.Top);
			graphicsPath.AddLine(rectangle.Left, tabRectangle.Top, tabRectangle.Right, tabRectangle.Top);
		}
		else
		{
			graphicsPath.AddLine(tabRectangle.Right, tabRectangle.Bottom, rectangle.Right, tabRectangle.Bottom);
			graphicsPath.AddLine(rectangle.Right, tabRectangle.Bottom, rectangle.Right, rectangle.Bottom);
			graphicsPath.AddLine(rectangle.Right, rectangle.Bottom, rectangle.Left, rectangle.Bottom);
			graphicsPath.AddLine(rectangle.Left, rectangle.Bottom, rectangle.Left, tabRectangle.Bottom);
			graphicsPath.AddLine(rectangle.Left, tabRectangle.Bottom, tabRectangle.Right, tabRectangle.Bottom);
		}
		return graphicsPath;
	}

	private GraphicsPath GetOutline_ToolWindow(int index)
	{
		Rectangle tabRectangle = GetTabRectangle(index);
		tabRectangle.Intersect(TabsRectangle);
		tabRectangle = RectangleToScreen(DrawHelper.RtlTransform(this, tabRectangle));
		Rectangle rectangle = base.DockPane.RectangleToScreen(base.DockPane.ClientRectangle);
		GraphicsPath graphicsPath = new GraphicsPath();
		GraphicsPath tabOutline = GetTabOutline(base.Tabs[index], rtlTransform: true, toScreen: true);
		graphicsPath.AddPath(tabOutline, connect: true);
		graphicsPath.AddLine(tabRectangle.Left, tabRectangle.Top, rectangle.Left, tabRectangle.Top);
		graphicsPath.AddLine(rectangle.Left, tabRectangle.Top, rectangle.Left, rectangle.Top);
		graphicsPath.AddLine(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Top);
		graphicsPath.AddLine(rectangle.Right, rectangle.Top, rectangle.Right, tabRectangle.Top);
		graphicsPath.AddLine(rectangle.Right, tabRectangle.Top, tabRectangle.Right, tabRectangle.Top);
		return graphicsPath;
	}

	private void CalculateTabs()
	{
		if (base.Appearance == DockPane.AppearanceStyle.ToolWindow)
		{
			CalculateTabs_ToolWindow();
		}
		else
		{
			CalculateTabs_Document();
		}
	}

	private void CalculateTabs_ToolWindow()
	{
		if (base.Tabs.Count <= 1 || base.DockPane.IsAutoHide)
		{
			return;
		}
		Rectangle tabStripRectangle = TabStripRectangle;
		int count = base.Tabs.Count;
		foreach (TabFiraxis item in (IEnumerable<Tab>)base.Tabs)
		{
			item.MaxWidth = GetMaxTabWidth(base.Tabs.IndexOf(item));
			item.Flag = false;
		}
		bool flag = true;
		int num = tabStripRectangle.Width - ToolWindowStripGapLeft - ToolWindowStripGapRight;
		int num2 = 0;
		int num3 = num / count;
		int num4 = count;
		flag = true;
		while (flag && num4 > 0)
		{
			flag = false;
			foreach (TabFiraxis item2 in (IEnumerable<Tab>)base.Tabs)
			{
				if (!item2.Flag && item2.MaxWidth <= num3)
				{
					item2.Flag = true;
					item2.TabWidth = item2.MaxWidth;
					num2 += item2.TabWidth;
					flag = true;
					num4--;
				}
			}
			if (num4 != 0)
			{
				num3 = (num - num2) / num4;
			}
		}
		if (num4 > 0)
		{
			int num5 = num - num2 - num3 * num4;
			foreach (TabFiraxis item3 in (IEnumerable<Tab>)base.Tabs)
			{
				if (!item3.Flag)
				{
					item3.Flag = true;
					if (num5 > 0)
					{
						item3.TabWidth = num3 + 1;
						num5--;
					}
					else
					{
						item3.TabWidth = num3;
					}
				}
			}
		}
		int num6 = tabStripRectangle.X + ToolWindowStripGapLeft;
		foreach (TabFiraxis item4 in (IEnumerable<Tab>)base.Tabs)
		{
			item4.TabX = num6;
			num6 += item4.TabWidth;
		}
	}

	private bool CalculateDocumentTab(Rectangle rectTabStrip, ref int x, int index)
	{
		bool result = false;
		TabFiraxis tabFiraxis = base.Tabs[index] as TabFiraxis;
		tabFiraxis.MaxWidth = GetMaxTabWidth(index);
		int num = Math.Min(tabFiraxis.MaxWidth, DocumentTabMaxWidth);
		if (x + num < rectTabStrip.Right || index == StartDisplayingTab)
		{
			tabFiraxis.TabX = x;
			tabFiraxis.TabWidth = num;
			EndDisplayingTab = index;
		}
		else
		{
			tabFiraxis.TabX = 0;
			tabFiraxis.TabWidth = 0;
			result = true;
		}
		x += num;
		return result;
	}

	private void CalculateTabs_Document()
	{
		if (m_startDisplayingTab >= base.Tabs.Count)
		{
			m_startDisplayingTab = 0;
		}
		Rectangle tabsRectangle = TabsRectangle;
		int num = tabsRectangle.X;
		bool flag = false;
		if (m_startDisplayingTab > 0)
		{
			int num2 = num;
			TabFiraxis tabFiraxis = base.Tabs[m_startDisplayingTab] as TabFiraxis;
			tabFiraxis.MaxWidth = GetMaxTabWidth(m_startDisplayingTab);
			for (int num3 = StartDisplayingTab; num3 >= 0; num3--)
			{
				CalculateDocumentTab(tabsRectangle, ref num2, num3);
			}
			FirstDisplayingTab = EndDisplayingTab;
			num2 = num;
			for (int i = EndDisplayingTab; i < base.Tabs.Count; i++)
			{
				flag |= CalculateDocumentTab(tabsRectangle, ref num2, i);
			}
			if (FirstDisplayingTab != 0)
			{
				flag = true;
			}
		}
		else
		{
			for (int j = StartDisplayingTab; j < base.Tabs.Count; j++)
			{
				flag |= CalculateDocumentTab(tabsRectangle, ref num, j);
			}
			for (int k = 0; k < StartDisplayingTab; k++)
			{
				flag |= CalculateDocumentTab(tabsRectangle, ref num, k);
			}
			FirstDisplayingTab = StartDisplayingTab;
		}
		if (!flag)
		{
			m_startDisplayingTab = 0;
			FirstDisplayingTab = 0;
			num = tabsRectangle.X;
			foreach (TabFiraxis item in (IEnumerable<Tab>)base.Tabs)
			{
				item.TabX = num;
				num += item.TabWidth;
			}
		}
		DocumentTabsOverflow = flag || AlwaysShowDocOverflow;
	}

	protected override void EnsureTabVisible(IDockContent content)
	{
		if (base.Appearance == DockPane.AppearanceStyle.Document && base.Tabs.Contains(content))
		{
			CalculateTabs();
			EnsureDocumentTabVisible(content, repaint: true);
		}
	}

	private bool EnsureDocumentTabVisible(IDockContent content, bool repaint)
	{
		int num = base.Tabs.IndexOf(content);
		if (num == -1)
		{
			return false;
		}
		TabFiraxis tabFiraxis = base.Tabs[num] as TabFiraxis;
		if (tabFiraxis.TabWidth != 0)
		{
			return false;
		}
		StartDisplayingTab = num;
		if (repaint)
		{
			Invalidate();
		}
		return true;
	}

	private int GetMaxTabWidth(int index)
	{
		if (base.Appearance == DockPane.AppearanceStyle.ToolWindow)
		{
			return GetMaxTabWidth_ToolWindow(index);
		}
		return GetMaxTabWidth_Document(index);
	}

	private int GetMaxTabWidth_ToolWindow(int index)
	{
		IDockContent content = base.Tabs[index].Content;
		Size size = TextRenderer.MeasureText(content.DockHandler.TabText, TextFont);
		return ToolWindowImageWidth + size.Width + ToolWindowImageGapLeft + ToolWindowImageGapRight + ToolWindowTextGapRight;
	}

	private int GetMaxTabWidth_Document(int index)
	{
		IDockContent content = base.Tabs[index].Content;
		int num = GetTabRectangle_Document(index).Height;
		Size size = (content.DockHandler.ShowTabText ? TextRenderer.MeasureText(content.DockHandler.TabText, BoldFont, new Size(DocumentTabMaxWidth, num), DocumentTextFormat) : default(Size));
		int num2 = ((!base.DockPane.DockPanel.ShowDocumentIcon) ? (size.Width + DocumentIconGapLeft + DocumentTextGapRight) : (size.Width + CurrentDocumentIconWidth + DocumentIconGapLeft + DocumentIconGapRight + DocumentTextGapRight));
		if (content.DockHandler.CloseButtonVisible)
		{
			num2 += 30;
		}
		return num2;
	}

	private void DrawTabStrip(Graphics g)
	{
		Rectangle tabStripRectangle = TabStripRectangle;
		g.FillRectangle(base.DockPane.DockPanel.Theme.PaintingService.GetBrush(base.DockPane.DockPanel.Theme.ColorPalette.MainWindowActive.Background), tabStripRectangle);
		if (base.Appearance == DockPane.AppearanceStyle.Document)
		{
			DrawTabStrip_Document(g);
		}
		else
		{
			DrawTabStrip_ToolWindow(g);
		}
	}

	private void DrawTabStrip_Document(Graphics g)
	{
		int count = base.Tabs.Count;
		if (count == 0)
		{
			return;
		}
		Rectangle clip = new Rectangle(TabStripRectangle.Location, TabStripRectangle.Size);
		clip.Height++;
		Rectangle tabsRectangle = TabsRectangle;
		Rectangle empty = Rectangle.Empty;
		TabFiraxis tabFiraxis = null;
		g.SetClip(DrawHelper.RtlTransform(this, tabsRectangle));
		for (int i = 0; i < count; i++)
		{
			empty = GetTabRectangle(i);
			if (base.Tabs[i].Content == base.DockPane.ActiveContent)
			{
				tabFiraxis = base.Tabs[i] as TabFiraxis;
			}
			else if (empty.IntersectsWith(tabsRectangle))
			{
				DrawTab(g, base.Tabs[i] as TabFiraxis, empty, last: false);
			}
		}
		g.SetClip(clip);
		if (base.DockPane.DockPanel.DocumentTabStripLocation != DocumentTabStripLocation.Bottom)
		{
			Color color = ((tabFiraxis == null || !base.DockPane.IsActiveDocumentPane) ? base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Background : base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedActive.Background);
			g.DrawLine(base.DockPane.DockPanel.Theme.PaintingService.GetPen(color, 4), clip.Left, clip.Bottom, clip.Right, clip.Bottom);
		}
		g.SetClip(DrawHelper.RtlTransform(this, tabsRectangle));
		if (tabFiraxis != null)
		{
			empty = GetTabRectangle(base.Tabs.IndexOf(tabFiraxis));
			if (empty.IntersectsWith(tabsRectangle))
			{
				empty.Intersect(tabsRectangle);
				DrawTab(g, tabFiraxis, empty, last: false);
			}
		}
	}

	private void DrawTabStrip_ToolWindow(Graphics g)
	{
		for (int i = 0; i < base.Tabs.Count; i++)
		{
			DrawTab(g, base.Tabs[i] as TabFiraxis, GetTabRectangle(i), i == base.Tabs.Count - 1);
		}
	}

	private Rectangle GetTabRectangle(int index)
	{
		if (base.Appearance == DockPane.AppearanceStyle.ToolWindow)
		{
			return GetTabRectangle_ToolWindow(index);
		}
		return GetTabRectangle_Document(index);
	}

	private Rectangle GetTabRectangle_ToolWindow(int index)
	{
		Rectangle tabStripRectangle = TabStripRectangle;
		TabFiraxis tabFiraxis = (TabFiraxis)base.Tabs[index];
		return new Rectangle(tabFiraxis.TabX, tabStripRectangle.Y, tabFiraxis.TabWidth, tabStripRectangle.Height);
	}

	private Rectangle GetTabRectangle_Document(int index)
	{
		Rectangle tabStripRectangle = TabStripRectangle;
		TabFiraxis tabFiraxis = (TabFiraxis)base.Tabs[index];
		Rectangle result = new Rectangle
		{
			X = tabFiraxis.TabX,
			Width = tabFiraxis.TabWidth,
			Height = tabStripRectangle.Height - DocumentTabGapTop
		};
		if (base.DockPane.DockPanel.DocumentTabStripLocation == DocumentTabStripLocation.Bottom)
		{
			result.Y = tabStripRectangle.Y + DocumentStripGapBottom;
		}
		else
		{
			result.Y = tabStripRectangle.Y + DocumentTabGapTop;
		}
		return result;
	}

	private void DrawTab(Graphics g, TabFiraxis tab, Rectangle rect, bool last)
	{
		if (base.Appearance == DockPane.AppearanceStyle.ToolWindow)
		{
			DrawTab_ToolWindow(g, tab, rect, last);
		}
		else
		{
			DrawTab_Document(g, tab, rect);
		}
	}

	private GraphicsPath GetTabOutline(Tab tab, bool rtlTransform, bool toScreen)
	{
		if (base.Appearance == DockPane.AppearanceStyle.ToolWindow)
		{
			return GetTabOutline_ToolWindow(tab, rtlTransform, toScreen);
		}
		return GetTabOutline_Document(tab, rtlTransform, toScreen, full: false);
	}

	private GraphicsPath GetTabOutline_ToolWindow(Tab tab, bool rtlTransform, bool toScreen)
	{
		Rectangle rectangle = GetTabRectangle(base.Tabs.IndexOf(tab));
		if (rtlTransform)
		{
			rectangle = DrawHelper.RtlTransform(this, rectangle);
		}
		if (toScreen)
		{
			rectangle = RectangleToScreen(rectangle);
		}
		DrawHelper.GetRoundedCornerTab(GraphicsPath, rectangle, upCorner: false);
		return GraphicsPath;
	}

	private GraphicsPath GetTabOutline_Document(Tab tab, bool rtlTransform, bool toScreen, bool full)
	{
		GraphicsPath.Reset();
		Rectangle rectangle = GetTabRectangle(base.Tabs.IndexOf(tab));
		rectangle.Intersect(TabsRectangle);
		rectangle.Width--;
		if (rtlTransform)
		{
			rectangle = DrawHelper.RtlTransform(this, rectangle);
		}
		if (toScreen)
		{
			rectangle = RectangleToScreen(rectangle);
		}
		GraphicsPath.AddRectangle(rectangle);
		return GraphicsPath;
	}

	private void DrawTab_ToolWindow(Graphics g, TabFiraxis tab, Rectangle rect, bool last)
	{
		rect.Y++;
		Rectangle rectangle = new Rectangle(rect.X + ToolWindowImageGapLeft, rect.Y - 1 + rect.Height - ToolWindowImageGapBottom - ToolWindowImageHeight, ToolWindowImageWidth, ToolWindowImageHeight);
		if (!tab.ContentForm.ShowIcon)
		{
			rectangle.Width = 0;
			rectangle.Height = 0;
		}
		Rectangle rectangle2 = ((PatchController.EnableHighDpi == true) ? new Rectangle(rect.X + ToolWindowImageGapLeft, rect.Y - 1 + rect.Height - ToolWindowImageGapBottom - TextFont.Height, ToolWindowImageWidth, TextFont.Height) : rectangle);
		rectangle2.X += rectangle.Width + ToolWindowImageGapRight;
		rectangle2.Width = rect.Width - rectangle.Width - ToolWindowImageGapLeft - ToolWindowImageGapRight - ToolWindowTextGapRight;
		Rectangle rectangle3 = DrawHelper.RtlTransform(this, rect);
		rectangle2 = DrawHelper.RtlTransform(this, rectangle2);
		rectangle = DrawHelper.RtlTransform(this, rectangle);
		Color toolWindowSeparator = base.DockPane.DockPanel.Theme.ColorPalette.ToolWindowSeparator;
		if (base.DockPane.ActiveContent == tab.Content)
		{
			Color foreColor;
			Color background;
			if (base.DockPane.IsActiveDocumentPane)
			{
				foreColor = base.DockPane.DockPanel.Theme.ColorPalette.ToolWindowTabSelectedActive.Text;
				background = base.DockPane.DockPanel.Theme.ColorPalette.ToolWindowTabSelectedActive.Background;
			}
			else
			{
				foreColor = base.DockPane.DockPanel.Theme.ColorPalette.ToolWindowTabSelectedInactive.Text;
				background = base.DockPane.DockPanel.Theme.ColorPalette.ToolWindowTabSelectedInactive.Background;
			}
			g.FillRectangle(base.DockPane.DockPanel.Theme.PaintingService.GetBrush(background), rect);
			TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectangle2, foreColor, ToolWindowTextFormat);
			g.DrawLine(base.DockPane.DockPanel.Theme.PaintingService.GetPen(toolWindowSeparator), rect.X + rect.Width - 1, rect.Y, rect.X + rect.Width - 1, rect.Height);
		}
		else
		{
			Color foreColor2;
			Color background2;
			if (tab.Content == base.DockPane.MouseOverTab)
			{
				foreColor2 = base.DockPane.DockPanel.Theme.ColorPalette.ToolWindowTabUnselectedHovered.Text;
				background2 = base.DockPane.DockPanel.Theme.ColorPalette.ToolWindowTabUnselectedHovered.Background;
			}
			else
			{
				foreColor2 = base.DockPane.DockPanel.Theme.ColorPalette.ToolWindowTabUnselected.Text;
				background2 = base.DockPane.DockPanel.Theme.ColorPalette.MainWindowActive.Background;
			}
			g.FillRectangle(base.DockPane.DockPanel.Theme.PaintingService.GetBrush(background2), rect);
			TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectangle2, foreColor2, ToolWindowTextFormat);
			if (!last)
			{
				g.DrawLine(base.DockPane.DockPanel.Theme.PaintingService.GetPen(toolWindowSeparator), rect.X + rect.Width - 1, rect.Y, rect.X + rect.Width - 1, rect.Height);
			}
		}
		if (rectangle3.Contains(rectangle))
		{
			g.DrawIcon(tab.Content.DockHandler.Icon, rectangle);
		}
	}

	private void DrawTab_Document(Graphics g, TabFiraxis tab, Rectangle rect)
	{
		if (tab.TabWidth == 0)
		{
			return;
		}
		Rectangle rectangle = GetCloseButtonRect(rect);
		if (!tab.Content.DockHandler.CloseButtonVisible)
		{
			rectangle = Rectangle.Empty;
		}
		Rectangle rectangle2 = new Rectangle(rect.X + DocumentIconGapLeft, rect.Y + rect.Height - DocumentIconGapBottom - CurrentDocumentIconHeight, CurrentDocumentIconWidth, CurrentDocumentIconHeight);
		Rectangle rectangle3 = rectangle2;
		if (PatchController.EnableHighDpi.HasValue && PatchController.EnableHighDpi.Value)
		{
			rectangle3 = new Rectangle(rect.X + DocumentIconGapLeft, rect.Y + rect.Height - DocumentIconGapBottom - TextFont.Height, CurrentDocumentIconWidth, TextFont.Height);
		}
		if (tab.Content.DockHandler.ShowTabText)
		{
			if (base.DockPane.DockPanel.ShowDocumentIcon)
			{
				rectangle3.X += rectangle2.Width + DocumentIconGapRight;
				rectangle3.Y = rect.Y;
				rectangle3.Width = rect.Width - rectangle2.Width - DocumentIconGapLeft - DocumentIconGapRight - DocumentTextGapRight - rectangle.Width;
			}
			else
			{
				rectangle3.Width = rect.Width - DocumentIconGapLeft - DocumentIconGapRight - DocumentTextGapRight - rectangle.Width;
			}
		}
		Rectangle rectangle4 = DrawHelper.RtlTransform(this, rect);
		Rectangle rectangle5 = DrawHelper.RtlTransform(this, rect);
		rectangle5.Width += DocumentIconGapLeft;
		rectangle5.X -= DocumentIconGapLeft;
		rectangle3 = DrawHelper.RtlTransform(this, rectangle3);
		rectangle2 = DrawHelper.RtlTransform(this, rectangle2);
		Color background = base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedActive.Background;
		Color background2 = base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Background;
		Color background3 = base.DockPane.DockPanel.Theme.ColorPalette.MainWindowActive.Background;
		Color background4 = base.DockPane.DockPanel.Theme.ColorPalette.TabUnselectedHovered.Background;
		Color color = base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedActive.Text;
		Color color2 = base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Text;
		Color color3 = base.DockPane.DockPanel.Theme.ColorPalette.TabUnselected.Text;
		Color color4 = base.DockPane.DockPanel.Theme.ColorPalette.TabUnselectedHovered.Text;
		Color color5;
		Color foreColor;
		if (base.DockPane.ActiveContent == tab.Content)
		{
			if (base.DockPane.IsActiveDocumentPane)
			{
				color5 = background;
				foreColor = color;
			}
			else
			{
				color5 = background2;
				foreColor = color2;
			}
		}
		else if (tab.Content == base.DockPane.MouseOverTab)
		{
			color5 = background4;
			foreColor = color4;
		}
		else
		{
			color5 = background3;
			foreColor = color3;
		}
		g.FillRectangle(base.DockPane.DockPanel.Theme.PaintingService.GetBrush(color5), rect);
		if (tab.Content.DockHandler.IsReadOnlyDocument())
		{
			Color foreColor2 = ((base.DockPane.ActiveContent == tab.Content) ? background3 : background);
			Color backColor = ((base.DockPane.ActiveContent == tab.Content) ? background : background3);
			using Brush brush = new HatchBrush(HatchStyle.BackwardDiagonal, foreColor2, backColor);
			g.FillRectangle(brush, rect);
		}
		Image image = null;
		IImageService imageService = base.DockPane.DockPanel.Theme.ImageService;
		image = (IsMouseDown ? imageService.TabPressInactive_Close : ((rectangle == ActiveClose) ? imageService.TabHoverInactive_Close : imageService.TabInactive_Close));
		if (image != null)
		{
			g.DrawImage(image, rectangle);
		}
		if (base.DockPane.DockPanel.ShowDocumentIcon)
		{
			g.DrawIcon(tab.Content.DockHandler.Icon, rectangle2);
		}
		if (tab.Content.DockHandler.IsDirtyDocument())
		{
			g.FillRectangle(base.DockPane.DockPanel.Theme.PaintingService.GetBrush(DirtyDocumentTabOverlayColor), rect);
		}
		if (tab.Content.DockHandler.ShowTabText)
		{
			TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectangle3, foreColor, DocumentTextFormat);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (IsMouseDown)
		{
			IsMouseDown = false;
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		m_suspendDrag = ActiveCloseHitTest(e.Location);
		if (!IsMouseDown)
		{
			IsMouseDown = true;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (!m_suspendDrag)
		{
			base.OnMouseMove(e);
		}
		int num = HitTest(PointToClient(Control.MousePosition));
		string text = string.Empty;
		bool flag = false;
		bool flag2 = false;
		if (num != -1)
		{
			TabFiraxis tabFiraxis = base.Tabs[num] as TabFiraxis;
			if (base.Appearance == DockPane.AppearanceStyle.ToolWindow || base.Appearance == DockPane.AppearanceStyle.Document)
			{
				flag = SetMouseOverTab((tabFiraxis.Content == base.DockPane.ActiveContent) ? null : tabFiraxis.Content);
			}
			if (!string.IsNullOrEmpty(tabFiraxis.Content.DockHandler.ToolTipText))
			{
				text = tabFiraxis.Content.DockHandler.ToolTipText;
			}
			else if (tabFiraxis.MaxWidth > tabFiraxis.TabWidth)
			{
				text = tabFiraxis.Content.DockHandler.TabText;
			}
			if (tabFiraxis.Content.DockHandler.CloseButtonVisible)
			{
				Point location = PointToClient(Control.MousePosition);
				Rectangle tabRectangle = GetTabRectangle(num);
				Rectangle closeButtonRect = GetCloseButtonRect(tabRectangle);
				Rectangle rect = new Rectangle(location, new Size(1, 1));
				flag2 = SetActiveClose(closeButtonRect.IntersectsWith(rect) ? closeButtonRect : Rectangle.Empty);
			}
		}
		else
		{
			flag = SetMouseOverTab(null);
			flag2 = SetActiveClose(Rectangle.Empty);
		}
		if (flag || flag2)
		{
			Invalidate();
		}
		if (m_toolTip.GetToolTip(this) != text)
		{
			m_toolTip.Active = false;
			m_toolTip.SetToolTip(this, text);
			m_toolTip.Active = true;
		}
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		if (e.Button == MouseButtons.Left && base.Appearance == DockPane.AppearanceStyle.Document)
		{
			int num = HitTest();
			if (num > -1)
			{
				TabCloseButtonHit(num);
			}
		}
	}

	private void TabCloseButtonHit(int index)
	{
		Point ptMouse = PointToClient(Control.MousePosition);
		if (GetTabBounds(base.Tabs[index]).Contains(ActiveClose) && ActiveCloseHitTest(ptMouse))
		{
			TryCloseTab(index);
		}
	}

	private Rectangle GetCloseButtonRect(Rectangle rectTab)
	{
		if (base.Appearance != DockPane.AppearanceStyle.Document)
		{
			return Rectangle.Empty;
		}
		int num = ((PatchController.EnableHighDpi == true) ? (rectTab.Height - 6) : 15);
		return new Rectangle(rectTab.X + rectTab.Width - num - 3 - 1, rectTab.Y + 3, num, num);
	}

	private bool AreFilePathDocuments(TabCollection tabs)
	{
		return tabs.All(delegate(Tab tab)
		{
			string text = StripProjectFromFileName(tab.Content.DockHandler.TabText);
			string toolTipText = tab.Content.DockHandler.ToolTipText;
			return !(text == toolTipText) && Path.GetFileName(toolTipText) == text;
		});
	}

	private string StripProjectFromFileName(string text)
	{
		Match match = externalProjectParser.Match(text);
		if (match.Groups.Count != 2)
		{
			match = readonlyParser.Match(text);
			if (match.Groups.Count != 2)
			{
				return text;
			}
		}
		return match.Groups[1].Value;
	}

	private void WindowList_Click(object sender, EventArgs e)
	{
		SelectMenu.Items.Clear();
		if (!AreFilePathDocuments(base.Tabs))
		{
			foreach (TabFiraxis item in (IEnumerable<Tab>)base.Tabs)
			{
				IDockContent content = item.Content;
				ToolStripItem toolStripItem = SelectMenu.Items.Add(content.DockHandler.TabText, content.DockHandler.Icon.ToBitmap());
				toolStripItem.Tag = item.Content;
				toolStripItem.Click += ContextMenuItem_Click;
			}
		}
		else
		{
			IOrderedEnumerable<DocumentTabSelectInfo> orderedEnumerable = from t in base.Tabs
				select new DocumentTabSelectInfo
				{
					Content = t.Content,
					Filename = StripProjectFromFileName(t.Content.DockHandler.TabText),
					FullPath = t.Content.DockHandler.ToolTipText,
					Project = CivTechRegistry.CivTechService.GetProjectName(new Uri(t.Content.DockHandler.ToolTipText)),
					TypeImage = t.Content.DockHandler.Icon.ToBitmap()
				} into o
				orderby o.Project
				select o;
			ISet<string> set = new HashSet<string>();
			foreach (DocumentTabSelectInfo item2 in orderedEnumerable)
			{
				if (set.Add(item2.Project))
				{
					if (set.Count > 1)
					{
						SelectMenu.Items.Add(new ToolStripSeparator());
					}
					string project = item2.Project;
					ToolStripLabel toolStripLabel = new ToolStripLabel(project);
					toolStripLabel.Font = ItalicFont;
					SelectMenu.Items.Add(toolStripLabel);
					SelectMenu.Items.Add(new ToolStripSeparator());
				}
				ToolStripItem toolStripItem2 = SelectMenu.Items.Add(item2.Filename, item2.TypeImage);
				toolStripItem2.ToolTipText = item2.FullPath;
				toolStripItem2.Tag = item2.Content;
				toolStripItem2.Click += ContextMenuItem_Click;
			}
		}
		base.DockPane.DockPanel.Theme.ApplyTo(SelectMenu);
		Rectangle workingArea = Screen.GetWorkingArea(ButtonWindowList.PointToScreen(new Point(ButtonWindowList.Width / 2, ButtonWindowList.Height / 2)));
		Rectangle rectangle = new Rectangle(ButtonWindowList.PointToScreen(new Point(0, ButtonWindowList.Location.Y + ButtonWindowList.Height)), SelectMenu.Size);
		Rectangle rect = new Rectangle(rectangle.X - SelectMenuMargin, rectangle.Y - SelectMenuMargin, rectangle.Width + SelectMenuMargin, rectangle.Height + SelectMenuMargin);
		if (workingArea.Contains(rect))
		{
			SelectMenu.Show(rectangle.Location);
			return;
		}
		Point location = rectangle.Location;
		location.X = DrawHelper.Balance(SelectMenu.Width, SelectMenuMargin, location.X, workingArea.Left, workingArea.Right);
		location.Y = DrawHelper.Balance(SelectMenu.Size.Height, SelectMenuMargin, location.Y, workingArea.Top, workingArea.Bottom);
		Point point = ButtonWindowList.PointToScreen(new Point(0, ButtonWindowList.Height));
		if (location.Y < point.Y)
		{
			location.Y = point.Y - ButtonWindowList.Height;
			SelectMenu.Show(location, ToolStripDropDownDirection.AboveRight);
		}
		else
		{
			SelectMenu.Show(location);
		}
	}

	private void ContextMenuItem_Click(object sender, EventArgs e)
	{
		if (sender is ToolStripMenuItem toolStripMenuItem)
		{
			IDockContent activeContent = (IDockContent)toolStripMenuItem.Tag;
			base.DockPane.ActiveContent = activeContent;
		}
	}

	private void SetInertButtons()
	{
		if (base.Appearance == DockPane.AppearanceStyle.ToolWindow || (base.Tabs.Count < 2 && !AlwaysShowDocOverflow))
		{
			if (m_buttonOverflow != null)
			{
				m_buttonOverflow.Left = -m_buttonOverflow.Width;
			}
			if (m_buttonWindowList != null)
			{
				m_buttonWindowList.Left = -m_buttonWindowList.Width;
			}
		}
		else
		{
			ButtonOverflow.Visible = m_documentTabsOverflow;
			ButtonOverflow.RefreshChanges();
			ButtonWindowList.Visible = !m_documentTabsOverflow;
			ButtonWindowList.RefreshChanges();
		}
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		if (base.Appearance == DockPane.AppearanceStyle.Document)
		{
			LayoutButtons();
			OnRefreshChanges();
		}
		base.OnLayout(levent);
	}

	private void LayoutButtons()
	{
		Rectangle tabStripRectangle = TabStripRectangle;
		int num = ButtonOverflow.Image.Width;
		int num2 = ButtonOverflow.Image.Height;
		int num3 = tabStripRectangle.Height - DocumentButtonGapTop - DocumentButtonGapBottom;
		if (num2 < num3)
		{
			num = num * num3 / num2;
			num2 = num3;
		}
		Size size = new Size(num, num2);
		int num4 = tabStripRectangle.X + tabStripRectangle.Width - DocumentTabGapLeft - DocumentButtonGapRight - num;
		int num5 = tabStripRectangle.Y + DocumentButtonGapTop;
		Point location = new Point(num4, num5);
		ButtonOverflow.Bounds = DrawHelper.RtlTransform(this, new Rectangle(location, size));
		ButtonWindowList.Bounds = DrawHelper.RtlTransform(this, new Rectangle(location, size));
	}

	private void Close_Click(object sender, EventArgs e)
	{
		base.DockPane.CloseActiveContent();
		if (PatchController.EnableMemoryLeakFix == true)
		{
			ContentClosed();
		}
	}

	protected override int HitTest(Point point)
	{
		if (!TabsRectangle.Contains(point))
		{
			return -1;
		}
		foreach (Tab item in (IEnumerable<Tab>)base.Tabs)
		{
			GraphicsPath tabOutline = GetTabOutline(item, rtlTransform: true, toScreen: false);
			if (tabOutline.IsVisible(point))
			{
				return base.Tabs.IndexOf(item);
			}
		}
		return -1;
	}

	protected override bool MouseDownActivateTest(MouseEventArgs e)
	{
		bool flag = base.MouseDownActivateTest(e);
		if (flag && e.Button == MouseButtons.Left && base.Appearance == DockPane.AppearanceStyle.Document)
		{
			flag = !ActiveCloseHitTest(e.Location);
		}
		return flag;
	}

	private bool ActiveCloseHitTest(Point ptMouse)
	{
		bool result = false;
		if (!ActiveClose.IsEmpty)
		{
			Rectangle rect = new Rectangle(ptMouse, new Size(1, 1));
			result = ActiveClose.IntersectsWith(rect);
		}
		return result;
	}

	protected override Rectangle GetTabBounds(Tab tab)
	{
		GraphicsPath tabOutline = GetTabOutline(tab, rtlTransform: true, toScreen: false);
		RectangleF bounds = tabOutline.GetBounds();
		return new Rectangle((int)bounds.Left, (int)bounds.Top, (int)bounds.Width, (int)bounds.Height);
	}

	private bool SetActiveClose(Rectangle rectangle)
	{
		if (_activeClose == rectangle)
		{
			return false;
		}
		_activeClose = rectangle;
		return true;
	}

	private bool SetMouseOverTab(IDockContent content)
	{
		if (base.DockPane.MouseOverTab == content)
		{
			return false;
		}
		base.DockPane.MouseOverTab = content;
		return true;
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		bool flag = SetMouseOverTab(null);
		bool flag2 = SetActiveClose(Rectangle.Empty);
		if (flag || flag2)
		{
			Invalidate();
		}
		base.OnMouseLeave(e);
	}

	protected override void OnRightToLeftChanged(EventArgs e)
	{
		base.OnRightToLeftChanged(e);
		PerformLayout();
	}
}
