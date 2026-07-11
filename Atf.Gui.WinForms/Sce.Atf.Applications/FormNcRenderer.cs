using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

[SuppressUnmanagedCodeSecurity]
[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
public class FormNcRenderer : NativeWindow
{
	public class SkinInfo
	{
		public Color ActiveBorderColor { get; set; }

		public Color InactiveBorderColor { get; set; }

		public Color TitleBarBackColor { get; set; }

		public Color TitleBarForeColor { get; set; }

		public Color ActiveTitleBarForeColor { get; set; }

		public Color CaptionButtonHoverColor { get; set; }

		public SkinInfo()
		{
			ActiveBorderColor = Color.FromArgb(0, 121, 203);
			InactiveBorderColor = Color.FromArgb(70, 70, 75);
			TitleBarBackColor = Color.FromArgb(45, 45, 48);
			TitleBarForeColor = Color.FromArgb(160, 160, 160);
			ActiveTitleBarForeColor = Color.FromArgb(250, 250, 250);
			CaptionButtonHoverColor = Color.FromArgb(70, 70, 75);
		}
	}

	private class CustomCaptionButton
	{
		private static Pen s_genPen = new Pen(Color.White);

		private static SolidBrush s_genBrush = new SolidBrush(Color.White);

		public readonly int Id;

		public Rectangle Bound;

		public bool Pressed;

		public bool MouseIn;

		public CustomCaptionButton(int id)
		{
			Id = id;
		}

		public void PerformAction(Form form)
		{
			if (Id == 20)
			{
				form.Close();
			}
			else if (Id == 9)
			{
				if (form.WindowState == FormWindowState.Normal)
				{
					form.WindowState = FormWindowState.Maximized;
				}
				else
				{
					form.WindowState = FormWindowState.Normal;
				}
			}
			else if (Id == 8)
			{
				form.WindowState = FormWindowState.Minimized;
			}
		}

		public void Draw(Graphics g, SkinInfo skin, Form form, bool active)
		{
			if (MouseIn)
			{
				s_genBrush.Color = skin.CaptionButtonHoverColor;
				g.FillRectangle(s_genBrush, Bound);
			}
			else
			{
				s_genBrush.Color = skin.TitleBarBackColor;
			}
			s_genPen.Color = (active ? skin.ActiveTitleBarForeColor : skin.TitleBarForeColor);
			int num = (int)((float)Bound.Height * 0.3f);
			Rectangle bound = Bound;
			bound.Inflate(-num, -num);
			if (Id == 20)
			{
				SmoothingMode smoothingMode = g.SmoothingMode;
				g.SmoothingMode = SmoothingMode.AntiAlias;
				s_genPen.Width = 2f;
				g.DrawLine(s_genPen, bound.X, bound.Y, bound.Right, bound.Bottom);
				g.DrawLine(s_genPen, bound.Right, bound.Y, bound.X, bound.Bottom);
				g.SmoothingMode = smoothingMode;
			}
			else if (Id == 9)
			{
				s_genPen.Width = 1f;
				if (form.WindowState == FormWindowState.Normal)
				{
					g.DrawRectangle(s_genPen, bound);
					g.DrawLine(s_genPen, bound.X, bound.Y + 1, bound.Right, bound.Y + 1);
					return;
				}
				Rectangle rect = bound;
				rect.X += num / 2;
				rect.Y -= num / 2;
				g.DrawRectangle(s_genPen, rect);
				g.DrawLine(s_genPen, rect.X, rect.Y + 1, rect.Right, rect.Y + 1);
				rect.X -= num / 2;
				rect.Y += num / 2;
				g.FillRectangle(s_genBrush, rect);
				g.DrawRectangle(s_genPen, rect);
				g.DrawLine(s_genPen, rect.X, rect.Y + 1, rect.Right, rect.Y + 1);
			}
			else if (Id == 8)
			{
				s_genPen.Width = 2f;
				g.DrawLine(s_genPen, bound.X, bound.Bottom, bound.Right, bound.Bottom);
			}
		}
	}

	private static class HitTest
	{
		public const int HTBORDER = 18;

		public const int HTBOTTOM = 15;

		public const int HTBOTTOMLEFT = 16;

		public const int HTBOTTOMRIGHT = 17;

		public const int HTCAPTION = 2;

		public const int HTCLIENT = 1;

		public const int HTCLOSE = 20;

		public const int HTERROR = -2;

		public const int HTGROWBOX = 4;

		public const int HTSIZE = 4;

		public const int HTHELP = 21;

		public const int HTHSCROLL = 6;

		public const int HTLEFT = 10;

		public const int HTMENU = 5;

		public const int HTMAXBUTTON = 9;

		public const int HTZOOM = 9;

		public const int HTMINBUTTON = 8;

		public const int HTREDUCE = 8;

		public const int HTNOWHERE = 0;

		public const int HTRIGHT = 11;

		public const int HTSYSMENU = 3;

		public const int HTTOP = 12;

		public const int HTTOPLEFT = 13;

		public const int HTTOPRIGHT = 14;

		public const int HTTRANSPARENT = -1;

		public const int HTVSCROLL = 7;
	}

	private static class WM_ACTIVATEState
	{
		public const int WA_INACTIVE = 0;

		public const int WA_ACTIVE = 1;

		public const int WA_CLICKACTIVE = 2;
	}

	private static class WinMessages
	{
		public const uint WM_SETICON = 128u;

		public const uint WM_SETTEXT = 12u;

		public const uint WM_GETTEXT = 13u;

		public const uint WM_STYLECHANGED = 125u;

		public const uint WM_ACTIVATE = 6u;

		public const uint WM_ACTIVATEAPP = 28u;

		public const uint WM_EXITSIZEMOVE = 562u;

		public const uint WM_SIZE = 5u;

		public const uint WM_SIZING = 532u;

		public const uint WM_GETMINMAXINFO = 36u;

		public const uint WM_WINDOWPOSCHANGING = 70u;

		public const uint WM_WINDOWPOSCHANGED = 71u;

		public const uint WM_NCHITTEST = 132u;

		public const uint WM_NCPAINT = 133u;

		public const uint WM_NCLBUTTONDOWN = 161u;

		public const uint WM_NCMOUSEMOVE = 160u;

		public const uint WM_NCMOUSELEAVE = 674u;

		public const uint WM_NCLBUTTONUP = 162u;

		public const uint WM_NCCALCSIZE = 131u;

		public const uint WM_NCACTIVATE = 134u;

		public const uint WM_MOUSEHOVER = 673u;

		public const uint WM_SHOWWINDOW = 24u;

		public const uint WM_NCUAHDRAWCAPTION = 174u;

		public const uint WM_NCUAHDRAWFRAME = 175u;

		public const uint WM_SYSCOMMAND = 274u;

		public const uint WM_SETCURSOR = 32u;

		public const uint WM_ERASEBKGND = 20u;
	}

	public static class SYSCOMMANDWPARAM
	{
		public const uint SC_MAXIMIZE = 61488u;

		public const uint SC_MINIMIZE = 61472u;

		public const uint SC_RESTORE = 61728u;

		public const uint SC_MOUSEMENU = 61584u;

		public const uint SC_CLOSE = 61536u;
	}

	[Flags]
	public enum DCXFlags : uint
	{
		DCX_CACHE = 2u,
		DCX_CLIPCHILDREN = 8u,
		DCX_CLIPSIBLINGS = 0x10u,
		DCX_EXCLUDERGN = 0x40u,
		DCX_EXCLUDEUPDATE = 0x100u,
		DCX_INTERSECTRGN = 0x80u,
		DCX_INTERSECTUPDATE = 0x200u,
		DCX_LOCKWINDOWUPDATE = 0x400u,
		DCX_NORECOMPUTE = 0x100000u,
		DCX_NORESETATTRS = 4u,
		DCX_PARENTCLIP = 0x20u,
		DCX_VALIDATE = 0x200000u,
		DCX_WINDOW = 1u,
		DCX_UNDOCUMENTED = 0x10000u
	}

	[Flags]
	public enum SWPFlags
	{
		SWP_NOSIZE = 1,
		SWP_NOMOVE = 2,
		SWP_NOZORDER = 4,
		SWP_NOREDRAW = 8,
		SWP_NOACTIVATE = 0x10,
		SWP_FRAMECHANGED = 0x20
	}

	public enum GetWindowLongIndex
	{
		GWL_STYLE = -16,
		GWL_EXSTYLE = -20
	}

	public enum WindowStyles : ulong
	{
		WS_CAPTION = 12582912uL,
		WS_BORDER = 8388608uL,
		WS_MAXIMIZEBOX = 65536uL,
		WS_MINIMIZEBOX = 131072uL,
		WS_SYSMENU = 524288uL,
		WS_POPUP = 2147483648uL
	}

	public struct POINT
	{
		public int X;

		public int Y;
	}

	public struct RECT
	{
		public int Left;

		public int Top;

		public int Right;

		public int Bottom;

		public int Width => Right - Left;

		public int Height => Bottom - Top;
	}

	public struct MINMAXINFO
	{
		public POINT ptReserved;

		public POINT ptMaxSize;

		public POINT ptMaxPosition;

		public POINT ptMinTrackSize;

		public POINT ptMaxTrackSize;
	}

	public struct NCCALCSIZE_PARAMS
	{
		public RECT rect0;

		public RECT rect1;

		public RECT rect2;

		public WINDOWPOS IntPtr;
	}

	public struct WINDOWPOS
	{
		public IntPtr hwnd;

		public IntPtr hwndInsertAfter;

		public int x;

		public int y;

		public int cx;

		public int cy;

		public uint flags;
	}

	private enum WINDOWPOSFlags : uint
	{
		SWP_DRAWFRAME = 32u,
		SWP_FRAMECHANGED = 32u,
		SWP_HIDEWINDOW = 128u,
		SWP_NOACTIVATE = 16u,
		SWP_NOCOPYBITS = 256u,
		SWP_NOMOVE = 2u,
		SWP_NOOWNERZORDER = 512u,
		SWP_NOREDRAW = 8u,
		SWP_NOREPOSITION = 512u,
		SWP_NOSENDCHANGING = 1024u,
		SWP_NOSIZE = 1u,
		SWP_NOZORDER = 4u,
		SWP_SHOWWINDOW = 64u
	}

	private bool m_active;

	private bool m_paintOnResize;

	private bool m_inSizeMove;

	private bool m_disabled;

	private SkinInfo m_skin = new SkinInfo();

	private static Pen s_genPen = new Pen(Color.White);

	private static SolidBrush s_genBrush = new SolidBrush(Color.White);

	private static BufferedGraphicsContext s_context;

	private static StringFormat s_captionFormat;

	private bool m_showIcon;

	private Rectangle m_iconRect;

	private Rectangle m_winRect;

	private Rectangle m_winClientRect;

	private int m_borderSize;

	private int m_titleSize;

	private int m_titleAndBorderSize;

	private Form m_form;

	private List<CustomCaptionButton> m_captionButtons = new List<CustomCaptionButton>();

	public SkinInfo Skin
	{
		get
		{
			return m_skin;
		}
		set
		{
			if (value != null)
			{
				m_skin = value;
				PaintTitleBar(m_active);
			}
		}
	}

	public bool CustomPaintDisabled
	{
		get
		{
			return m_disabled || !m_form.IsHandleCreated || (m_form != null && m_form.FormBorderStyle == FormBorderStyle.None);
		}
		set
		{
			m_disabled = m_form == null || m_form.Parent != null || value;
			if (m_form != null && !m_form.IsDisposed && m_form.IsHandleCreated)
			{
				if (m_disabled)
				{
					SetWindowTheme(m_form.Handle, "Explorer", null);
					return;
				}
				m_active = Form.ActiveForm == m_form;
				SetWindowTheme(m_form.Handle, "", "");
				CreateCaptionButtons();
			}
		}
	}

	public FormNcRenderer(Form form)
	{
		if (form == null)
		{
			throw new ArgumentNullException();
		}
		m_form = form;
		if (s_captionFormat == null)
		{
			s_captionFormat = new StringFormat
			{
				Alignment = StringAlignment.Near,
				LineAlignment = StringAlignment.Center,
				FormatFlags = StringFormatFlags.NoWrap,
				Trimming = StringTrimming.EllipsisCharacter
			};
		}
		if (s_context == null)
		{
			s_context = new BufferedGraphicsContext();
		}
		if (form.Handle != IntPtr.Zero)
		{
			HanldeCreated();
		}
		m_form.HandleCreated += delegate
		{
			HanldeCreated();
		};
		m_form.HandleDestroyed += delegate
		{
			ReleaseHandle();
		};
		m_disabled = form.Parent != null;
		m_form.ParentChanged += Form_ParentChanged;
		Application.ApplicationExit += delegate
		{
			if (s_context != null)
			{
				s_context.Dispose();
				s_context = null;
			}
		};
	}

	private void Form_ParentChanged(object sender, EventArgs e)
	{
		CustomPaintDisabled = m_form == null || !m_form.IsHandleCreated || m_form.Parent != null;
	}

	protected override void WndProc(ref Message m)
	{
		if (m_form == null || !m_form.IsHandleCreated || m_form.IsDisposed)
		{
			return;
		}
		if (CustomPaintDisabled)
		{
			base.WndProc(ref m);
			return;
		}
		bool flag = false;
		switch ((uint)m.Msg)
		{
		case 133u:
			if (ResizeTrace.IsResizing)
				ResizeTrace.Log("FormNcRenderer: WM_NCPAINT" + (m_inSizeMove ? " (inSizeMove, skip)" : ""));
			if (!m_inSizeMove)
			{
				flag = PaintTitleBar(m_active);
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				m.Result = IntPtr.Zero;
			}
			break;
		case 28u:
			m_active = m.WParam != IntPtr.Zero;
			flag = PaintTitleBar(m_active);
			if (flag)
			{
				m.Result = IntPtr.Zero;
			}
			break;
		case 6u:
		{
			int num2 = LowWord(m.WParam.ToInt32());
			m_active = num2 != 0;
			if (PaintTitleBar(m_active))
			{
				m.Result = IntPtr.Zero;
			}
			flag = false;
			break;
		}
		case 134u:
			if (m.WParam != IntPtr.Zero)
			{
				m.Result = IntPtr.Zero;
			}
			else
			{
				m.Result = (IntPtr)1;
			}
			flag = true;
			PaintTitleBar(m.WParam != IntPtr.Zero);
			break;
		case 174u:
		case 175u:
			flag = true;
			break;
		case 5u:
			if (m_paintOnResize)
			{
				m_paintOnResize = false;
				PaintTitleBar(m_active);
			}
			break;
		case 274u:
		{
			uint num = (uint)((int)m.WParam & 0xFFF0);
			m_paintOnResize = (num & 0xF120) != 0 || (num & 0xF030) != 0;
			break;
		}
		case 561u:
			ResizeTrace.Log("FormNcRenderer: WM_ENTERSIZEMOVE");
			m_inSizeMove = true;
			break;
		case 562u:
			ResizeTrace.Log("FormNcRenderer: WM_EXITSIZEMOVE");
			m_inSizeMove = false;
			PaintTitleBar(m_active);
			break;
		case 125u:
			CreateCaptionButtons();
			m.Result = IntPtr.Zero;
			flag = true;
			break;
		case 128u:
			CreateCaptionButtons();
			break;
		case 12u:
			base.WndProc(ref m);
			PaintTitleBar(m_active);
			flag = true;
			break;
		case 132u:
			flag = OnHitTest(ref m);
			break;
		case 674u:
			SetHoverState(null);
			break;
		case 161u:
			flag = OnNcLButtonDown(ref m);
			break;
		case 162u:
			flag = OnNcLButtonUp(ref m);
			break;
		case 160u:
			flag = OnNcMouseMove(ref m);
			break;
		}
		if (!flag)
		{
			base.WndProc(ref m);
		}
	}

	private bool OnNcLButtonDown(ref Message msg)
	{
		bool result = false;
		foreach (CustomCaptionButton captionButton in m_captionButtons)
		{
			captionButton.MouseIn = false;
			captionButton.Pressed = false;
		}
		CustomCaptionButton customCaptionButton = FindButtonById(msg.WParam.ToInt32());
		if (customCaptionButton != null)
		{
			customCaptionButton.MouseIn = true;
			customCaptionButton.Pressed = true;
			result = true;
			msg.Result = IntPtr.Zero;
		}
		uint num = (uint)(int)msg.WParam;
		if (num == 3)
		{
			SendMessage(m_form.Handle, 274u, (IntPtr)61584L, msg.LParam);
			msg.Result = IntPtr.Zero;
			result = true;
		}
		PaintTitleBar(m_active);
		return result;
	}

	private bool OnNcMouseMove(ref Message msg)
	{
		if (Control.MouseButtons != MouseButtons.Left)
		{
			CustomCaptionButton hoverState = FindButtonById((int)msg.WParam);
			SetHoverState(hoverState);
		}
		return false;
	}

	private bool OnNcLButtonUp(ref Message msg)
	{
		CustomCaptionButton pressedButton = GetPressedButton();
		bool result = false;
		foreach (CustomCaptionButton captionButton in m_captionButtons)
		{
			captionButton.MouseIn = false;
			captionButton.Pressed = false;
		}
		CustomCaptionButton customCaptionButton = FindButtonById(msg.WParam.ToInt32());
		if (customCaptionButton != null && customCaptionButton == pressedButton)
		{
			result = true;
			msg.Result = (IntPtr)0;
			PaintTitleBar(m_active);
			m_paintOnResize = true;
			customCaptionButton.PerformAction(m_form);
		}
		PaintTitleBar(m_active);
		return result;
	}

	private CustomCaptionButton GetPressedButton()
	{
		foreach (CustomCaptionButton captionButton in m_captionButtons)
		{
			if (captionButton.Pressed)
			{
				return captionButton;
			}
		}
		return null;
	}

	private CustomCaptionButton FindButtonById(int id)
	{
		foreach (CustomCaptionButton captionButton in m_captionButtons)
		{
			if (captionButton.Id == id)
			{
				return captionButton;
			}
		}
		return null;
	}

	private void SetHoverState(CustomCaptionButton btn)
	{
		bool flag = false;
		foreach (CustomCaptionButton captionButton in m_captionButtons)
		{
			if (captionButton == btn)
			{
				flag |= !btn.MouseIn;
				btn.MouseIn = true;
			}
			else
			{
				flag |= captionButton.MouseIn;
				captionButton.MouseIn = false;
			}
		}
		if (flag)
		{
			PaintTitleBar(m_active);
		}
	}

	private bool OnHitTest(ref Message msg)
	{
		Point scrPt = new Point(msg.LParam.ToInt32());
		Point pt = PointToWindow(scrPt);
		if (m_winClientRect.Contains(pt))
		{
			msg.Result = (IntPtr)1;
			return true;
		}
		Rectangle winRect = m_winRect;
		winRect.Inflate(-m_borderSize, -m_borderSize);
		if (!winRect.Contains(pt))
		{
			return false;
		}
		Rectangle winRect2 = m_winRect;
		winRect2.Height = m_titleAndBorderSize;
		if (winRect2.Contains(pt))
		{
			foreach (CustomCaptionButton captionButton in m_captionButtons)
			{
				if (captionButton.Bound.Contains(pt))
				{
					msg.Result = (IntPtr)captionButton.Id;
					return true;
				}
			}
			if (m_showIcon && m_iconRect.Contains(pt))
			{
				msg.Result = (IntPtr)3;
				return true;
			}
			msg.Result = (IntPtr)2;
			return true;
		}
		return false;
	}

	private void CreateCaptionButtons()
	{
		m_captionButtons.Clear();
		m_showIcon = false;
		m_iconRect = Rectangle.Empty;
		FormBorderStyle formBorderStyle = m_form.FormBorderStyle;
		if (formBorderStyle == FormBorderStyle.None || !m_form.ControlBox)
		{
			return;
		}
		m_showIcon = m_form.ControlBox && m_form.ShowIcon && formBorderStyle != FormBorderStyle.FixedToolWindow && formBorderStyle != FormBorderStyle.SizableToolWindow && formBorderStyle != FormBorderStyle.FixedDialog;
		CustomCaptionButton item = new CustomCaptionButton(20);
		m_captionButtons.Add(item);
		if (m_form.FormBorderStyle != FormBorderStyle.FixedToolWindow && m_form.FormBorderStyle != FormBorderStyle.SizableToolWindow)
		{
			if (m_form.MaximizeBox)
			{
				m_captionButtons.Add(new CustomCaptionButton(9));
			}
			if (m_form.MinimizeBox)
			{
				m_captionButtons.Add(new CustomCaptionButton(8));
			}
		}
		UpdateBounds();
		UpdateCaptionButtons();
	}

	private Size GetCaptionButtonSize()
	{
		FormBorderStyle formBorderStyle = m_form.FormBorderStyle;
		if (formBorderStyle == FormBorderStyle.FixedToolWindow || formBorderStyle == FormBorderStyle.SizableToolWindow)
		{
			return SystemInformation.ToolWindowCaptionButtonSize;
		}
		return SystemInformation.CaptionButtonSize;
	}

	private void UpdateCaptionButtons()
	{
		int height = GetCaptionButtonSize().Height;
		int num = Math.Min(4, m_titleAndBorderSize - (height + 1));
		int num2 = m_titleAndBorderSize - 2;
		int y = 2;
		int x = 2;
		if (m_form.WindowState == FormWindowState.Maximized)
		{
			num = m_borderSize;
			y = num;
			x = num;
			num2 -= num;
		}
		Rectangle bound = new Rectangle(m_winRect.Width - (height + 3), num, height, height);
		foreach (CustomCaptionButton captionButton in m_captionButtons)
		{
			captionButton.Bound = bound;
			bound.X -= height + 1;
		}
		if (m_showIcon)
		{
			m_iconRect = new Rectangle(x, y, num2, num2);
		}
	}

	private Point PointToWindow(Point scrPt)
	{
		return new Point(scrPt.X - m_form.Location.X, scrPt.Y - m_form.Location.Y);
	}

	private void HanldeCreated()
	{
		if (m_form.Parent == null)
		{
			SetWindowTheme(m_form.Handle, "", "");
			CreateCaptionButtons();
		}
		else
		{
			m_disabled = true;
		}
		AssignHandle(m_form.Handle);
	}

	private bool PaintTitleBar(bool active)
	{
		if (CustomPaintDisabled || m_form == null || !m_form.Visible)
		{
			return false;
		}
		UpdateBounds();
		UpdateCaptionButtons();
		IntPtr dCEx = GetDCEx(m_form.Handle, IntPtr.Zero, 19u);
		if (dCEx == IntPtr.Zero)
		{
			return false;
		}
		ExcludeClipRect(dCEx, m_winClientRect.X, m_winClientRect.Y, m_winClientRect.Right, m_winClientRect.Bottom);
		Size maximumBuffer = s_context.MaximumBuffer;
		if (m_winRect.Width > maximumBuffer.Width || m_winRect.Height > maximumBuffer.Height)
		{
			s_context.MaximumBuffer = m_winRect.Size;
		}
		BufferedGraphics bufferedGraphics = s_context.Allocate(dCEx, m_winRect);
		bufferedGraphics.Graphics.SetClip(m_winClientRect, CombineMode.Exclude);
		PaintTitleBar(bufferedGraphics.Graphics, active);
		bufferedGraphics.Render();
		bufferedGraphics.Dispose();
		ReleaseDC(m_form.Handle, dCEx);
		return true;
	}

	private void PaintTitleBar(Graphics g, bool active)
	{
		g.Clear(m_skin.TitleBarBackColor);
		s_genPen.Color = (active ? m_skin.ActiveBorderColor : m_skin.InactiveBorderColor);
		Rectangle winRect = m_winRect;
		winRect.X = 1;
		winRect.Y = 1;
		winRect.Width -= 2;
		winRect.Height -= 2;
		s_genPen.Width = 2f;
		g.DrawRectangle(s_genPen, winRect);
		foreach (CustomCaptionButton captionButton in m_captionButtons)
		{
			captionButton.Draw(g, m_skin, m_form, active);
		}
		if (m_showIcon)
		{
			g.DrawIcon(m_form.Icon, m_iconRect);
		}
		string text = m_form.Text;
		if (!string.IsNullOrWhiteSpace(text))
		{
			int num = ((m_iconRect.Width > 0) ? (-(m_iconRect.Width + 1)) : (-6));
			num = ((m_captionButtons.Count <= 0) ? (num + m_winRect.Width) : (num + m_captionButtons[m_captionButtons.Count - 1].Bound.X));
			int x = ((m_iconRect.Right > 0) ? m_iconRect.Right : 6);
			int y = 0;
			int height = m_titleAndBorderSize;
			if (m_form.WindowState == FormWindowState.Maximized)
			{
				y = m_borderSize;
				height = m_titleSize;
			}
			s_genBrush.Color = (active ? m_skin.ActiveTitleBarForeColor : m_skin.TitleBarForeColor);
			g.DrawString(layoutRectangle: new Rectangle(x, y, num, height), s: text, font: SystemFonts.CaptionFont, brush: s_genBrush, format: s_captionFormat);
		}
	}

	private void UpdateBounds()
	{
		RECT lpRect = default(RECT);
		GetWindowRect(m_form.Handle, ref lpRect);
		int width = lpRect.Width;
		int height = lpRect.Height;
		m_winRect = new Rectangle(0, 0, width, height);
		RECT lpRect2 = default(RECT);
		GetClientRect(m_form.Handle, ref lpRect2);
		int width2 = lpRect2.Width;
		int height2 = lpRect2.Height;
		m_borderSize = (width - width2) / 2;
		m_titleSize = height - height2 - 2 * m_borderSize;
		m_titleAndBorderSize = m_borderSize + m_titleSize;
		m_winClientRect = new Rectangle(m_borderSize, m_titleAndBorderSize, width2, height2);
	}

	public int HiWord(int val)
	{
		return (val >> 16) & 0xFFFF;
	}

	public int LowWord(int val)
	{
		return val & 0xFFFF;
	}

	private int Get_X_LPARAMint(IntPtr lparam)
	{
		return (short)(lparam.ToInt32() & 0xFFFF);
	}

	private int Get_Y_LPARAM(IntPtr lparam)
	{
		return (short)((lparam.ToInt32() & 0xFFFF0000u) >> 16);
	}

	[DllImport("uxtheme.dll")]
	public static extern int SetWindowTheme(IntPtr hwnd, string pszSubAppName, string pszSubIdList);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll")]
	public static extern void DisableProcessWindowsGhosting();

	public static IntPtr GetWindowLong(IntPtr hWnd, int nIndex)
	{
		if (IntPtr.Size == 4)
		{
			return GetWindowLong32(hWnd, nIndex);
		}
		return GetWindowLongPtr64(hWnd, nIndex);
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLong")]
	private static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtr")]
	private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll", SetLastError = true)]
	public static extern int SetWindowLong(IntPtr hWnd, int Offset, int newLong);

	[DllImport("user32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);

	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);

	[DllImport("user32.dll")]
	public static extern IntPtr GetDCEx(IntPtr hwnd, IntPtr hrgnclip, uint fdwOptions);

	[DllImport("user32.dll")]
	public static extern int ReleaseDC(IntPtr hwnd, IntPtr hDC);

	[DllImport("User32.dll")]
	public static extern IntPtr GetWindowDC(IntPtr hWnd);

	[DllImport("User32.dll")]
	public static extern IntPtr GetActiveWindow();

	[DllImport("gdi32.dll")]
	public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

	[DllImport("gdi32.dll")]
	public static extern int SelectClipRgn(IntPtr hdc, IntPtr hrgn);

	[DllImport("gdi32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool DeleteObject(IntPtr hObject);

	[DllImport("gdi32.dll")]
	private static extern int ExcludeClipRect(IntPtr hdc, int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

	private void DisplayWINDOWPOSFlags(uint flags)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Winpos flags:");
		foreach (uint value in Enum.GetValues(typeof(WINDOWPOSFlags)))
		{
			if ((flags & value) != 0)
			{
				WINDOWPOSFlags wINDOWPOSFlags = (WINDOWPOSFlags)value;
				stringBuilder.AppendLine(wINDOWPOSFlags.ToString());
			}
		}
		Console.WriteLine(stringBuilder);
	}
}
