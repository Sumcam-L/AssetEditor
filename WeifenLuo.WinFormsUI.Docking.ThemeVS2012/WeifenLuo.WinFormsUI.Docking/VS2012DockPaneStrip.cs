using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.ThemeVS2012;

namespace WeifenLuo.WinFormsUI.Docking;

internal class VS2012DockPaneStrip : DockPaneStripBase
{
	private class TabVS2012 : Tab
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

		public TabVS2012(IDockContent content)
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

	private const int _ToolWindowStripGapTop = 0;

	private const int _ToolWindowStripGapBottom = 0;

	private const int _ToolWindowStripGapLeft = 0;

	private const int _ToolWindowStripGapRight = 0;

	private const int _ToolWindowImageHeight = 16;

	private const int _ToolWindowImageWidth = 0;

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

	private const int _DocumentTabGapTop = 0;

	private const int _DocumentTabGapLeft = 0;

	private const int _DocumentTabGapRight = 0;

	private const int _DocumentIconGapBottom = 2;

	private const int _DocumentIconGapLeft = 8;

	private const int _DocumentIconGapRight = 0;

	private const int _DocumentIconHeight = 16;

	private const int _DocumentIconWidth = 16;

	private const int _DocumentTextGapRight = 6;

	private ContextMenuStrip m_selectMenu;

	private InertButton m_buttonOverflow;

	private InertButton m_buttonWindowList;

	private IContainer m_components;

	private ToolTip m_toolTip;

	private Font m_font;

	private Font m_boldFont;

	private int m_startDisplayingTab;

	private int m_endDisplayingTab;

	private int m_firstDisplayingTab;

	private bool m_documentTabsOverflow;

	private static string m_toolTipSelect;

	private Rectangle _activeClose;

	private int _selectMenuMargin = 5;

	private bool m_suspendDrag;

	private const int TAB_CLOSE_BUTTON_WIDTH = 30;

	private bool m_isMouseDown;

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
			int num5 = num + DocumentTabGapLeft;
			num3 -= DocumentTabGapLeft + DocumentTabGapRight + DocumentButtonGapRight + ButtonOverflow.Width + ButtonWindowList.Width + 2 * DocumentButtonGapBetween;
			return new Rectangle(num5, num2, num3, num4);
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

	private static GraphicsPath GraphicsPath => VS2012AutoHideStrip.GraphicsPath;

	private IContainer Components => m_components;

	public Font TextFont => base.DockPane.DockPanel.Theme.Skin.DockPaneStripSkin.TextFont;

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

	private static int ToolWindowImageWidth => 0;

	private static int ToolWindowImageGapTop => 3;

	private static int ToolWindowImageGapBottom => 1;

	private static int ToolWindowImageGapLeft => 2;

	private static int ToolWindowImageGapRight => 0;

	private static int ToolWindowTextGapRight => 3;

	private static int ToolWindowTabSeperatorGapTop => 3;

	private static int ToolWindowTabSeperatorGapBottom => 3;

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

	private static int DocumentTabGapTop => 0;

	private static int DocumentTabGapLeft => 0;

	private static int DocumentTabGapRight => 0;

	private static int DocumentIconGapBottom => 2;

	private static int DocumentIconGapLeft => 8;

	private static int DocumentIconGapRight => 0;

	private static int DocumentIconWidth => 16;

	private static int DocumentIconHeight => 16;

	private static int DocumentTextGapRight => 6;

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
		return new TabVS2012(content);
	}

	public VS2012DockPaneStrip(DockPane pane)
		: base(pane)
	{
		SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, value: true);
		SuspendLayout();
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
			if (m_boldFont != null)
			{
				m_boldFont.Dispose();
				m_boldFont = null;
			}
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
		return Math.Max(TextFont.Height + DocumentTabGapTop + ((PatchController.EnableHighDpi == true) ? DocumentIconGapBottom : 0), ButtonOverflow.Height + DocumentButtonGapTop + DocumentButtonGapBottom) + DocumentStripGapBottom + DocumentStripGapTop;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		CalculateTabs();
		if (base.Appearance == DockPane.AppearanceStyle.Document && base.DockPane.ActiveContent != null && EnsureDocumentTabVisible(base.DockPane.ActiveContent, repaint: false))
		{
			CalculateTabs();
		}
		DrawTabStrip(e.Graphics);
	}

	protected override void OnRefreshChanges()
	{
		SetInertButtons();
		Invalidate();
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
		foreach (TabVS2012 item in (IEnumerable<Tab>)base.Tabs)
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
			foreach (TabVS2012 item2 in (IEnumerable<Tab>)base.Tabs)
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
			foreach (TabVS2012 item3 in (IEnumerable<Tab>)base.Tabs)
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
		foreach (TabVS2012 item4 in (IEnumerable<Tab>)base.Tabs)
		{
			item4.TabX = num6;
			num6 += item4.TabWidth;
		}
	}

	private bool CalculateDocumentTab(Rectangle rectTabStrip, ref int x, int index)
	{
		bool result = false;
		TabVS2012 tabVS = base.Tabs[index] as TabVS2012;
		tabVS.MaxWidth = GetMaxTabWidth(index);
		int num = Math.Min(tabVS.MaxWidth, DocumentTabMaxWidth);
		if (x + num < rectTabStrip.Right || index == StartDisplayingTab)
		{
			tabVS.TabX = x;
			tabVS.TabWidth = num;
			EndDisplayingTab = index;
		}
		else
		{
			tabVS.TabX = 0;
			tabVS.TabWidth = 0;
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
			(base.Tabs[m_startDisplayingTab] as TabVS2012).MaxWidth = GetMaxTabWidth(m_startDisplayingTab);
			for (int num3 = StartDisplayingTab; num3 >= 0; num3--)
			{
				CalculateDocumentTab(tabsRectangle, ref num2, num3);
			}
			FirstDisplayingTab = EndDisplayingTab;
			num2 = num;
			for (int i = EndDisplayingTab; i < base.Tabs.Count; i++)
			{
				flag = CalculateDocumentTab(tabsRectangle, ref num2, i);
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
				flag = CalculateDocumentTab(tabsRectangle, ref num, j);
			}
			for (int k = 0; k < StartDisplayingTab; k++)
			{
				flag = CalculateDocumentTab(tabsRectangle, ref num, k);
			}
			FirstDisplayingTab = StartDisplayingTab;
		}
		if (!flag)
		{
			m_startDisplayingTab = 0;
			FirstDisplayingTab = 0;
			num = tabsRectangle.X;
			foreach (TabVS2012 item in (IEnumerable<Tab>)base.Tabs)
			{
				item.TabX = num;
				num += item.TabWidth;
			}
		}
		DocumentTabsOverflow = flag;
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
		if ((base.Tabs[num] as TabVS2012).TabWidth != 0)
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
		Size size = TextRenderer.MeasureText(base.Tabs[index].Content.DockHandler.TabText, TextFont);
		return ToolWindowImageWidth + size.Width + ToolWindowImageGapLeft + ToolWindowImageGapRight + ToolWindowTextGapRight;
	}

	private int GetMaxTabWidth_Document(int index)
	{
		IDockContent content = base.Tabs[index].Content;
		int num = GetTabRectangle_Document(index).Height;
		Size size = TextRenderer.MeasureText(content.DockHandler.TabText, BoldFont, new Size(DocumentTabMaxWidth, num), DocumentTextFormat);
		int num2 = ((!base.DockPane.DockPanel.ShowDocumentIcon) ? (size.Width + DocumentIconGapLeft + DocumentTextGapRight) : (size.Width + DocumentIconWidth + DocumentIconGapLeft + DocumentIconGapRight + DocumentTextGapRight));
		return num2 + 30;
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
		TabVS2012 tabVS = null;
		g.SetClip(DrawHelper.RtlTransform(this, tabsRectangle));
		for (int i = 0; i < count; i++)
		{
			empty = GetTabRectangle(i);
			if (base.Tabs[i].Content == base.DockPane.ActiveContent)
			{
				tabVS = base.Tabs[i] as TabVS2012;
			}
			else if (empty.IntersectsWith(tabsRectangle))
			{
				DrawTab(g, base.Tabs[i] as TabVS2012, empty, last: false);
			}
		}
		g.SetClip(clip);
		if (base.DockPane.DockPanel.DocumentTabStripLocation != DocumentTabStripLocation.Bottom)
		{
			Color color = ((tabVS == null || !base.DockPane.IsActiveDocumentPane) ? base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Background : base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedActive.Background);
			g.DrawLine(base.DockPane.DockPanel.Theme.PaintingService.GetPen(color, 4), clip.Left, clip.Bottom, clip.Right, clip.Bottom);
		}
		g.SetClip(DrawHelper.RtlTransform(this, tabsRectangle));
		if (tabVS != null)
		{
			empty = GetTabRectangle(base.Tabs.IndexOf(tabVS));
			if (empty.IntersectsWith(tabsRectangle))
			{
				empty.Intersect(tabsRectangle);
				DrawTab(g, tabVS, empty, last: false);
			}
		}
	}

	private void DrawTabStrip_ToolWindow(Graphics g)
	{
		for (int i = 0; i < base.Tabs.Count; i++)
		{
			DrawTab(g, base.Tabs[i] as TabVS2012, GetTabRectangle(i), i == base.Tabs.Count - 1);
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
		TabVS2012 tabVS = (TabVS2012)base.Tabs[index];
		return new Rectangle(tabVS.TabX, tabStripRectangle.Y, tabVS.TabWidth, tabStripRectangle.Height);
	}

	private Rectangle GetTabRectangle_Document(int index)
	{
		Rectangle tabStripRectangle = TabStripRectangle;
		TabVS2012 tabVS = (TabVS2012)base.Tabs[index];
		Rectangle result = new Rectangle
		{
			X = tabVS.TabX,
			Width = tabVS.TabWidth,
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

	private void DrawTab(Graphics g, TabVS2012 tab, Rectangle rect, bool last)
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

	private void DrawTab_ToolWindow(Graphics g, TabVS2012 tab, Rectangle rect, bool last)
	{
		rect.Y++;
		Rectangle rectangle = new Rectangle(rect.X + ToolWindowImageGapLeft, rect.Y - 1 + rect.Height - ToolWindowImageGapBottom - ToolWindowImageHeight, ToolWindowImageWidth, ToolWindowImageHeight);
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

	private void DrawTab_Document(Graphics g, TabVS2012 tab, Rectangle rect)
	{
		if (tab.TabWidth == 0)
		{
			return;
		}
		Rectangle closeButtonRect = GetCloseButtonRect(rect);
		Rectangle rectangle = new Rectangle(rect.X + DocumentIconGapLeft, rect.Y + rect.Height - DocumentIconGapBottom - DocumentIconHeight, DocumentIconWidth, DocumentIconHeight);
		Rectangle rectangle2 = ((PatchController.EnableHighDpi == true) ? new Rectangle(rect.X + DocumentIconGapLeft, rect.Y + rect.Height - DocumentIconGapBottom - TextFont.Height, DocumentIconWidth, TextFont.Height) : rectangle);
		if (base.DockPane.DockPanel.ShowDocumentIcon)
		{
			rectangle2.X += rectangle.Width + DocumentIconGapRight;
			rectangle2.Y = rect.Y;
			rectangle2.Width = rect.Width - rectangle.Width - DocumentIconGapLeft - DocumentIconGapRight - DocumentTextGapRight - closeButtonRect.Width;
			rectangle2.Height = rect.Height;
		}
		else
		{
			rectangle2.Width = rect.Width - DocumentIconGapLeft - DocumentTextGapRight - closeButtonRect.Width;
		}
		Rectangle rectangle3 = DrawHelper.RtlTransform(this, rect);
		Rectangle rectangle4 = DrawHelper.RtlTransform(this, rect);
		rectangle4.Width += DocumentIconGapLeft;
		rectangle4.X -= DocumentIconGapLeft;
		rectangle2 = DrawHelper.RtlTransform(this, rectangle2);
		rectangle = DrawHelper.RtlTransform(this, rectangle);
		Color background = base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedActive.Background;
		Color background2 = base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Background;
		Color background3 = base.DockPane.DockPanel.Theme.ColorPalette.MainWindowActive.Background;
		Color background4 = base.DockPane.DockPanel.Theme.ColorPalette.TabUnselectedHovered.Background;
		Color color = base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedActive.Text;
		Color color2 = base.DockPane.DockPanel.Theme.ColorPalette.TabSelectedInactive.Text;
		Color color3 = base.DockPane.DockPanel.Theme.ColorPalette.TabUnselected.Text;
		Color color4 = base.DockPane.DockPanel.Theme.ColorPalette.TabUnselectedHovered.Text;
		Image image = null;
		IImageService imageService = base.DockPane.DockPanel.Theme.ImageService;
		Color color5;
		Color foreColor;
		if (base.DockPane.ActiveContent == tab.Content)
		{
			if (base.DockPane.IsActiveDocumentPane)
			{
				color5 = background;
				foreColor = color;
				image = (IsMouseDown ? imageService.TabPressActive_Close : ((closeButtonRect == ActiveClose) ? imageService.TabHoverActive_Close : imageService.TabActive_Close));
			}
			else
			{
				color5 = background2;
				foreColor = color2;
				image = (IsMouseDown ? imageService.TabPressLostFocus_Close : ((closeButtonRect == ActiveClose) ? imageService.TabHoverLostFocus_Close : imageService.TabLostFocus_Close));
			}
		}
		else if (tab.Content == base.DockPane.MouseOverTab)
		{
			color5 = background4;
			foreColor = color4;
			image = (IsMouseDown ? imageService.TabPressInactive_Close : ((closeButtonRect == ActiveClose) ? imageService.TabHoverInactive_Close : imageService.TabInactive_Close));
		}
		else
		{
			color5 = background3;
			foreColor = color3;
		}
		g.FillRectangle(base.DockPane.DockPanel.Theme.PaintingService.GetBrush(color5), rect);
		TextRenderer.DrawText(g, tab.Content.DockHandler.TabText, TextFont, rectangle2, foreColor, DocumentTextFormat);
		if (image != null)
		{
			g.DrawImage(image, closeButtonRect);
		}
		if (rectangle3.Contains(rectangle) && base.DockPane.DockPanel.ShowDocumentIcon)
		{
			g.DrawIcon(tab.Content.DockHandler.Icon, rectangle);
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
			TabVS2012 tabVS = base.Tabs[num] as TabVS2012;
			if (base.Appearance == DockPane.AppearanceStyle.ToolWindow || base.Appearance == DockPane.AppearanceStyle.Document)
			{
				flag = SetMouseOverTab((tabVS.Content == base.DockPane.ActiveContent) ? null : tabVS.Content);
			}
			if (!string.IsNullOrEmpty(tabVS.Content.DockHandler.ToolTipText))
			{
				text = tabVS.Content.DockHandler.ToolTipText;
			}
			else if (tabVS.MaxWidth > tabVS.TabWidth)
			{
				text = tabVS.Content.DockHandler.TabText;
			}
			Point location = PointToClient(Control.MousePosition);
			Rectangle tabRectangle = GetTabRectangle(num);
			Rectangle closeButtonRect = GetCloseButtonRect(tabRectangle);
			Rectangle rect = new Rectangle(location, new Size(1, 1));
			flag2 = SetActiveClose(closeButtonRect.IntersectsWith(rect) ? closeButtonRect : Rectangle.Empty);
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

	private void WindowList_Click(object sender, EventArgs e)
	{
		SelectMenu.Items.Clear();
		foreach (TabVS2012 item in (IEnumerable<Tab>)base.Tabs)
		{
			IDockContent content = item.Content;
			ToolStripItem toolStripItem = SelectMenu.Items.Add(content.DockHandler.TabText, content.DockHandler.Icon.ToBitmap());
			toolStripItem.Tag = item.Content;
			toolStripItem.Click += ContextMenuItem_Click;
		}
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
		if (base.Appearance == DockPane.AppearanceStyle.ToolWindow)
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
			if (GetTabOutline(item, rtlTransform: true, toScreen: false).IsVisible(point))
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
		RectangleF bounds = GetTabOutline(tab, rtlTransform: true, toScreen: false).GetBounds();
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
		bool num = SetMouseOverTab(null);
		bool flag = SetActiveClose(Rectangle.Empty);
		if (num || flag)
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
