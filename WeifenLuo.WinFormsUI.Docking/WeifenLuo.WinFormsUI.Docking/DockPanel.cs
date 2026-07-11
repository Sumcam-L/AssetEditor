using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using WeifenLuo.WinFormsUI.Docking.Win32;

namespace WeifenLuo.WinFormsUI.Docking;

[LocalizedDescription("DockPanel_Description")]
[Designer("System.Windows.Forms.Design.ControlDesigner, System.Design")]
[ToolboxBitmap(typeof(resfinder), "WeifenLuo.WinFormsUI.Docking.DockPanel.bmp")]
[DefaultProperty("DocumentStyle")]
[DefaultEvent("ActiveContentChanged")]
public class DockPanel : Panel
{
	private sealed class SplitterDragHandler : DragHandler
	{
		private class SplitterOutline
		{
			private DragForm m_dragForm;

			private DragForm DragForm => m_dragForm;

			public SplitterOutline()
			{
				m_dragForm = new DragForm();
				SetDragForm(Rectangle.Empty);
				DragForm.BackColor = Color.Black;
				DragForm.Opacity = 0.7;
				DragForm.Show(bActivate: false);
			}

			public void Show(Rectangle rect)
			{
				SetDragForm(rect);
			}

			public void Close()
			{
				DragForm.Bounds = Rectangle.Empty;
				DragForm.Close();
			}

			private void SetDragForm(Rectangle rect)
			{
				DragForm.Bounds = rect;
				if (rect == Rectangle.Empty)
				{
					DragForm.Region = new Region(Rectangle.Empty);
				}
				else if (DragForm.Region != null)
				{
					DragForm.Region = null;
				}
			}
		}

		private SplitterOutline m_outline;

		private Rectangle m_rectSplitter;

		public new ISplitterDragSource DragSource
		{
			get
			{
				return base.DragSource as ISplitterDragSource;
			}
			private set
			{
				base.DragSource = value;
			}
		}

		private SplitterOutline Outline
		{
			get
			{
				return m_outline;
			}
			set
			{
				m_outline = value;
			}
		}

		private Rectangle RectSplitter
		{
			get
			{
				return m_rectSplitter;
			}
			set
			{
				m_rectSplitter = value;
			}
		}

		public SplitterDragHandler(DockPanel dockPanel)
			: base(dockPanel)
		{
		}

		public void BeginDrag(ISplitterDragSource dragSource, Rectangle rectSplitter)
		{
			DragSource = dragSource;
			RectSplitter = rectSplitter;
			if (!BeginDrag())
			{
				DragSource = null;
				return;
			}
			Outline = new SplitterOutline();
			Outline.Show(rectSplitter);
			DragSource.BeginDrag(rectSplitter);
		}

		protected override void OnDragging()
		{
			Outline.Show(GetSplitterOutlineBounds(Control.MousePosition));
		}

		protected override void OnEndDrag(bool abort)
		{
			base.DockPanel.SuspendLayout(allWindows: true);
			Outline.Close();
			if (!abort)
			{
				DragSource.MoveSplitter(GetMovingOffset(Control.MousePosition));
			}
			DragSource.EndDrag();
			base.DockPanel.ResumeLayout(performLayout: true, allWindows: true);
		}

		private int GetMovingOffset(Point ptMouse)
		{
			Rectangle splitterOutlineBounds = GetSplitterOutlineBounds(ptMouse);
			if (DragSource.IsVertical)
			{
				return splitterOutlineBounds.X - RectSplitter.X;
			}
			return splitterOutlineBounds.Y - RectSplitter.Y;
		}

		private Rectangle GetSplitterOutlineBounds(Point ptMouse)
		{
			Rectangle dragLimitBounds = DragSource.DragLimitBounds;
			Rectangle rectSplitter = RectSplitter;
			if (dragLimitBounds.Width <= 0 || dragLimitBounds.Height <= 0)
			{
				return rectSplitter;
			}
			if (DragSource.IsVertical)
			{
				rectSplitter.X += ptMouse.X - base.StartMousePosition.X;
				rectSplitter.Height = dragLimitBounds.Height;
			}
			else
			{
				rectSplitter.Y += ptMouse.Y - base.StartMousePosition.Y;
				rectSplitter.Width = dragLimitBounds.Width;
			}
			if (rectSplitter.Left < dragLimitBounds.Left)
			{
				rectSplitter.X = dragLimitBounds.X;
			}
			if (rectSplitter.Top < dragLimitBounds.Top)
			{
				rectSplitter.Y = dragLimitBounds.Y;
			}
			if (rectSplitter.Right > dragLimitBounds.Right)
			{
				rectSplitter.X -= rectSplitter.Right - dragLimitBounds.Right;
			}
			if (rectSplitter.Bottom > dragLimitBounds.Bottom)
			{
				rectSplitter.Y -= rectSplitter.Bottom - dragLimitBounds.Bottom;
			}
			return rectSplitter;
		}
	}

	public abstract class DragHandlerBase : NativeWindow, IMessageFilter
	{
		private Point m_startMousePosition = Point.Empty;

		protected abstract Control DragControl { get; }

		protected Point StartMousePosition
		{
			get
			{
				return m_startMousePosition;
			}
			private set
			{
				m_startMousePosition = value;
			}
		}

		protected bool BeginDrag()
		{
			if (DragControl == null)
			{
				return false;
			}
			StartMousePosition = Control.MousePosition;
			if (!Win32Helper.IsRunningOnMono && !NativeMethods.DragDetect(DragControl.Handle, StartMousePosition))
			{
				return false;
			}
			DragControl.FindForm().Capture = true;
			AssignHandle(DragControl.FindForm().Handle);
			if (PatchController.EnableActiveXFix == false)
			{
				Application.AddMessageFilter(this);
			}
			return true;
		}

		protected abstract void OnDragging();

		protected abstract void OnEndDrag(bool abort);

		private void EndDrag(bool abort)
		{
			ReleaseHandle();
			if (PatchController.EnableActiveXFix == false)
			{
				Application.RemoveMessageFilter(this);
			}
			DragControl.FindForm().Capture = false;
			OnEndDrag(abort);
		}

		bool IMessageFilter.PreFilterMessage(ref Message m)
		{
			if (PatchController.EnableActiveXFix == false)
			{
				if (m.Msg == 512)
				{
					OnDragging();
				}
				else if (m.Msg == 514)
				{
					EndDrag(abort: false);
				}
				else if (m.Msg == 533)
				{
					EndDrag(!Win32Helper.IsRunningOnMono);
				}
				else if (m.Msg == 256 && (int)m.WParam == 27)
				{
					EndDrag(abort: true);
				}
			}
			return OnPreFilterMessage(ref m);
		}

		protected virtual bool OnPreFilterMessage(ref Message m)
		{
			if (PatchController.EnableActiveXFix == true)
			{
				if (m.Msg == 512)
				{
					OnDragging();
				}
				else if (m.Msg == 514)
				{
					EndDrag(abort: false);
				}
				else if (m.Msg == 533)
				{
					EndDrag(!Win32Helper.IsRunningOnMono);
				}
				else if (m.Msg == 256 && (int)m.WParam == 27)
				{
					EndDrag(abort: true);
				}
			}
			return false;
		}

		protected sealed override void WndProc(ref Message m)
		{
			if (PatchController.EnableActiveXFix == true)
			{
				OnPreFilterMessage(ref m);
			}
			if (m.Msg == 31 || m.Msg == 533)
			{
				EndDrag(abort: true);
			}
			base.WndProc(ref m);
		}
	}

	public abstract class DragHandler : DragHandlerBase
	{
		private DockPanel m_dockPanel;

		private IDragSource m_dragSource;

		public DockPanel DockPanel => m_dockPanel;

		protected IDragSource DragSource
		{
			get
			{
				return m_dragSource;
			}
			set
			{
				m_dragSource = value;
			}
		}

		protected sealed override Control DragControl
		{
			get
			{
				if (DragSource != null)
				{
					return DragSource.DragControl;
				}
				return null;
			}
		}

		protected DragHandler(DockPanel dockPanel)
		{
			m_dockPanel = dockPanel;
		}

		protected sealed override bool OnPreFilterMessage(ref Message m)
		{
			if ((m.Msg == 256 || m.Msg == 257) && ((int)m.WParam == 17 || (int)m.WParam == 16))
			{
				OnDragging();
			}
			return base.OnPreFilterMessage(ref m);
		}
	}

	private class MdiClientController : NativeWindow, IComponent, IDisposable
	{
		private bool m_autoScroll = true;

		private BorderStyle m_borderStyle = BorderStyle.Fixed3D;

		private MdiClient m_mdiClient;

		private Form m_parentForm;

		private ISite m_site;

		public bool AutoScroll
		{
			get
			{
				return m_autoScroll;
			}
			set
			{
				m_autoScroll = value;
				if (MdiClient != null)
				{
					UpdateStyles();
				}
			}
		}

		public BorderStyle BorderStyle
		{
			set
			{
				if (!Enum.IsDefined(typeof(BorderStyle), value))
				{
					throw new InvalidEnumArgumentException();
				}
				m_borderStyle = value;
				if (MdiClient == null || (Site != null && Site.DesignMode))
				{
					return;
				}
				if (!Win32Helper.IsRunningOnMono)
				{
					int num = NativeMethods.GetWindowLong(MdiClient.Handle, -16);
					int num2 = NativeMethods.GetWindowLong(MdiClient.Handle, -20);
					switch (m_borderStyle)
					{
					case BorderStyle.Fixed3D:
						num2 |= 0x200;
						num &= -8388609;
						break;
					case BorderStyle.FixedSingle:
						num2 &= -513;
						num |= 0x800000;
						break;
					case BorderStyle.None:
						num &= -8388609;
						num2 &= -513;
						break;
					}
					NativeMethods.SetWindowLong(MdiClient.Handle, -16, num);
					NativeMethods.SetWindowLong(MdiClient.Handle, -20, num2);
				}
				UpdateStyles();
			}
		}

		public MdiClient MdiClient => m_mdiClient;

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public Form ParentForm
		{
			get
			{
				return m_parentForm;
			}
			set
			{
				if (m_parentForm != null)
				{
					m_parentForm.HandleCreated -= ParentFormHandleCreated;
					m_parentForm.MdiChildActivate -= ParentFormMdiChildActivate;
				}
				m_parentForm = value;
				if (m_parentForm != null)
				{
					if (m_parentForm.IsHandleCreated)
					{
						InitializeMdiClient();
						RefreshProperties();
					}
					else
					{
						m_parentForm.HandleCreated += ParentFormHandleCreated;
					}
					m_parentForm.MdiChildActivate += ParentFormMdiChildActivate;
				}
			}
		}

		public ISite Site
		{
			get
			{
				return m_site;
			}
			set
			{
				m_site = value;
				if (m_site != null && value.GetService(typeof(IDesignerHost)) is IDesignerHost { RootComponent: Form rootComponent })
				{
					ParentForm = rootComponent;
				}
			}
		}

		public event EventHandler Disposed;

		public event EventHandler HandleAssigned;

		public event EventHandler MdiChildActivate;

		public event LayoutEventHandler Layout;

		public event PaintEventHandler Paint;

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Site != null && Site.Container != null)
				{
					Site.Container.Remove(this);
				}
				if (this.Disposed != null)
				{
					this.Disposed(this, EventArgs.Empty);
				}
			}
		}

		public void RenewMdiClient()
		{
			InitializeMdiClient();
			RefreshProperties();
		}

		protected virtual void OnHandleAssigned(EventArgs e)
		{
			if (this.HandleAssigned != null)
			{
				this.HandleAssigned(this, e);
			}
		}

		protected virtual void OnMdiChildActivate(EventArgs e)
		{
			if (this.MdiChildActivate != null)
			{
				this.MdiChildActivate(this, e);
			}
		}

		protected virtual void OnLayout(LayoutEventArgs e)
		{
			if (this.Layout != null)
			{
				this.Layout(this, e);
			}
		}

		protected virtual void OnPaint(PaintEventArgs e)
		{
			if (this.Paint != null)
			{
				this.Paint(this, e);
			}
		}

		protected override void WndProc(ref Message m)
		{
			int msg = m.Msg;
			if (msg == 131 && !AutoScroll && !Win32Helper.IsRunningOnMono)
			{
				NativeMethods.ShowScrollBar(m.HWnd, 3, 0);
			}
			base.WndProc(ref m);
		}

		private void ParentFormHandleCreated(object sender, EventArgs e)
		{
			m_parentForm.HandleCreated -= ParentFormHandleCreated;
			InitializeMdiClient();
			RefreshProperties();
		}

		private void ParentFormMdiChildActivate(object sender, EventArgs e)
		{
			OnMdiChildActivate(e);
		}

		private void MdiClientLayout(object sender, LayoutEventArgs e)
		{
			OnLayout(e);
		}

		private void MdiClientHandleDestroyed(object sender, EventArgs e)
		{
			if (m_mdiClient != null)
			{
				m_mdiClient.HandleDestroyed -= MdiClientHandleDestroyed;
				m_mdiClient = null;
			}
			ReleaseHandle();
		}

		private void InitializeMdiClient()
		{
			if (MdiClient != null)
			{
				MdiClient.HandleDestroyed -= MdiClientHandleDestroyed;
				MdiClient.Layout -= MdiClientLayout;
			}
			if (ParentForm == null)
			{
				return;
			}
			foreach (Control control in ParentForm.Controls)
			{
				m_mdiClient = control as MdiClient;
				if (m_mdiClient != null)
				{
					ReleaseHandle();
					AssignHandle(MdiClient.Handle);
					OnHandleAssigned(EventArgs.Empty);
					MdiClient.HandleDestroyed += MdiClientHandleDestroyed;
					MdiClient.Layout += MdiClientLayout;
					break;
				}
			}
		}

		private void RefreshProperties()
		{
			BorderStyle = m_borderStyle;
			AutoScroll = m_autoScroll;
		}

		private void UpdateStyles()
		{
			if (!Win32Helper.IsRunningOnMono)
			{
				NativeMethods.SetWindowPos(MdiClient.Handle, IntPtr.Zero, 0, 0, 0, 0, FlagsSetWindowPos.SWP_NOSIZE | FlagsSetWindowPos.SWP_NOMOVE | FlagsSetWindowPos.SWP_NOZORDER | FlagsSetWindowPos.SWP_NOACTIVATE | FlagsSetWindowPos.SWP_FRAMECHANGED | FlagsSetWindowPos.SWP_NOOWNERZORDER);
			}
		}
	}

	internal class DefaultAutoHideWindowControl : AutoHideWindowControl
	{
		public DefaultAutoHideWindowControl(DockPanel dockPanel)
			: base(dockPanel)
		{
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.DockPadding.All = 0;
			if (base.DockState == DockState.DockLeftAutoHide)
			{
				base.DockPadding.Right = 2;
				base.m_splitter.Dock = DockStyle.Right;
			}
			else if (base.DockState == DockState.DockRightAutoHide)
			{
				base.DockPadding.Left = 2;
				base.m_splitter.Dock = DockStyle.Left;
			}
			else if (base.DockState == DockState.DockTopAutoHide)
			{
				base.DockPadding.Bottom = 2;
				base.m_splitter.Dock = DockStyle.Bottom;
			}
			else if (base.DockState == DockState.DockBottomAutoHide)
			{
				base.DockPadding.Top = 2;
				base.m_splitter.Dock = DockStyle.Top;
			}
			Rectangle displayingRectangle = DisplayingRectangle;
			Rectangle bounds = new Rectangle(-displayingRectangle.Width, displayingRectangle.Y, displayingRectangle.Width, displayingRectangle.Height);
			foreach (Control control in base.Controls)
			{
				if (control is DockPane dockPane)
				{
					if (dockPane == base.ActivePane)
					{
						dockPane.Bounds = displayingRectangle;
					}
					else
					{
						dockPane.Bounds = bounds;
					}
				}
			}
			base.OnLayout(levent);
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			var sw = System.Diagnostics.Stopwatch.StartNew();
			Graphics graphics = e.Graphics;
			if (base.DockState == DockState.DockBottomAutoHide)
			{
				graphics.DrawLine(SystemPens.ControlLightLight, 0, 1, base.ClientRectangle.Right, 1);
			}
			else if (base.DockState == DockState.DockRightAutoHide)
			{
				graphics.DrawLine(SystemPens.ControlLightLight, 1, 0, 1, base.ClientRectangle.Bottom);
			}
			else if (base.DockState == DockState.DockTopAutoHide)
			{
				graphics.DrawLine(SystemPens.ControlDark, 0, base.ClientRectangle.Height - 2, base.ClientRectangle.Right, base.ClientRectangle.Height - 2);
				graphics.DrawLine(SystemPens.ControlDarkDark, 0, base.ClientRectangle.Height - 1, base.ClientRectangle.Right, base.ClientRectangle.Height - 1);
			}
			else if (base.DockState == DockState.DockLeftAutoHide)
			{
				graphics.DrawLine(SystemPens.ControlDark, base.ClientRectangle.Width - 2, 0, base.ClientRectangle.Width - 2, base.ClientRectangle.Bottom);
				graphics.DrawLine(SystemPens.ControlDarkDark, base.ClientRectangle.Width - 1, 0, base.ClientRectangle.Width - 1, base.ClientRectangle.Bottom);
			}
			base.OnPaint(e);
			sw.Stop();
			if (sw.ElapsedMilliseconds > 1)
				System.Diagnostics.Trace.WriteLine($"[Paint] DefaultAutoHideWindowControl.OnPaint: {sw.ElapsedMilliseconds}ms");
		}
	}

	public class AutoHideWindowControl : Panel, ISplitterHost, ISplitterDragSource, IDragSource
	{
		protected class SplitterControl : SplitterBase
		{
			private AutoHideWindowControl m_autoHideWindow;

			private AutoHideWindowControl AutoHideWindow => m_autoHideWindow;

			protected override int SplitterSize => AutoHideWindow.DockPanel.Theme.Measures.AutoHideSplitterSize;

			public SplitterControl(AutoHideWindowControl autoHideWindow)
			{
				m_autoHideWindow = autoHideWindow;
			}

			protected override void StartDrag()
			{
				AutoHideWindow.DockPanel.BeginDrag(AutoHideWindow, AutoHideWindow.RectangleToScreen(base.Bounds));
			}
		}

		private const int ANIMATE_TIME = 100;

		private Timer m_timerMouseTrack;

		private DockPanel m_dockPanel;

		private DockPane m_activePane;

		private static readonly object AutoHideActiveContentChangedEvent = new object();

		private IDockContent m_activeContent;

		private bool m_flagAnimate = true;

		private bool m_flagDragging;

		protected SplitterBase m_splitter { get; private set; }

		public bool IsDockWindow => false;

		public DockPanel DockPanel => m_dockPanel;

		public DockPane ActivePane => m_activePane;

		public IDockContent ActiveContent
		{
			get
			{
				return m_activeContent;
			}
			set
			{
				if (value == m_activeContent)
				{
					return;
				}
				if (value != null && (!DockHelper.IsDockStateAutoHide(value.DockHandler.DockState) || value.DockHandler.DockPanel != DockPanel))
				{
					throw new InvalidOperationException(Strings.DockPanel_ActiveAutoHideContent_InvalidValue);
				}
				DockPanel.SuspendLayout();
				if (m_activeContent != null)
				{
					if (m_activeContent.DockHandler.Form.ContainsFocus && !Win32Helper.IsRunningOnMono)
					{
						DockPanel.ContentFocusManager.GiveUpFocus(m_activeContent);
					}
					AnimateWindow(show: false);
				}
				m_activeContent = value;
				SetActivePane();
				if (ActivePane != null)
				{
					ActivePane.ActiveContent = m_activeContent;
				}
				if (m_activeContent != null)
				{
					AnimateWindow(show: true);
				}
				DockPanel.ResumeLayout();
				DockPanel.RefreshAutoHideStrip();
				SetTimerMouseTrack();
				OnActiveContentChanged(EventArgs.Empty);
			}
		}

		public DockState DockState
		{
			get
			{
				if (ActiveContent != null)
				{
					return ActiveContent.DockHandler.DockState;
				}
				return DockState.Unknown;
			}
		}

		private bool FlagAnimate
		{
			get
			{
				return m_flagAnimate;
			}
			set
			{
				m_flagAnimate = value;
			}
		}

		internal bool FlagDragging
		{
			get
			{
				return m_flagDragging;
			}
			set
			{
				if (m_flagDragging != value)
				{
					m_flagDragging = value;
					SetTimerMouseTrack();
				}
			}
		}

		protected virtual Rectangle DisplayingRectangle
		{
			get
			{
				Rectangle clientRectangle = base.ClientRectangle;
				if (DockState == DockState.DockBottomAutoHide)
				{
					clientRectangle.Y += 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
					clientRectangle.Height -= 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
				}
				else if (DockState == DockState.DockRightAutoHide)
				{
					clientRectangle.X += 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
					clientRectangle.Width -= 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
				}
				else if (DockState == DockState.DockTopAutoHide)
				{
					clientRectangle.Height -= 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
				}
				else if (DockState == DockState.DockLeftAutoHide)
				{
					clientRectangle.Width -= 2 + DockPanel.Theme.Measures.AutoHideSplitterSize;
				}
				return clientRectangle;
			}
		}

		bool ISplitterDragSource.IsVertical
		{
			get
			{
				if (DockState != DockState.DockLeftAutoHide)
				{
					return DockState == DockState.DockRightAutoHide;
				}
				return true;
			}
		}

		Rectangle ISplitterDragSource.DragLimitBounds
		{
			get
			{
				Rectangle dockArea = DockPanel.DockArea;
				if (((ISplitterDragSource)this).IsVertical)
				{
					dockArea.X += 24;
					dockArea.Width -= 48;
				}
				else
				{
					dockArea.Y += 24;
					dockArea.Height -= 48;
				}
				return DockPanel.RectangleToScreen(dockArea);
			}
		}

		Control IDragSource.DragControl => this;

		public event EventHandler ActiveContentChanged
		{
			add
			{
				base.Events.AddHandler(AutoHideActiveContentChangedEvent, value);
			}
			remove
			{
				base.Events.RemoveHandler(AutoHideActiveContentChangedEvent, value);
			}
		}

		public AutoHideWindowControl(DockPanel dockPanel)
		{
			m_dockPanel = dockPanel;
			m_timerMouseTrack = new Timer();
			m_timerMouseTrack.Tick += TimerMouseTrack_Tick;
			base.Visible = false;
			m_splitter = DockPanel.Theme.Extender.WindowSplitterControlFactory.CreateSplitterControl(this);
			base.Controls.Add(m_splitter);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				m_timerMouseTrack.Dispose();
			}
			base.Dispose(disposing);
		}

		private void SetActivePane()
		{
			DockPane dockPane = ((ActiveContent == null) ? null : ActiveContent.DockHandler.Pane);
			if (dockPane != m_activePane)
			{
				m_activePane = dockPane;
			}
		}

		protected virtual void OnActiveContentChanged(EventArgs e)
		{
		((EventHandler)base.Events[AutoHideActiveContentChangedEvent])?.Invoke(this, e);
		}

		private void AnimateWindow(bool show)
		{
			if (!FlagAnimate && base.Visible != show)
			{
				base.Visible = show;
				return;
			}
			base.Parent.SuspendLayout();
			Rectangle rectangle = GetRectangle(!show);
			Rectangle rectangle2 = GetRectangle(show);
			int num2;
			int num3;
			int num4;
			int num = (num2 = (num3 = (num4 = 0)));
			if (DockState == DockState.DockTopAutoHide)
			{
				num4 = (show ? 1 : (-1));
			}
			else if (DockState == DockState.DockLeftAutoHide)
			{
				num3 = (show ? 1 : (-1));
			}
			else if (DockState == DockState.DockRightAutoHide)
			{
				num = ((!show) ? 1 : (-1));
				num3 = (show ? 1 : (-1));
			}
			else if (DockState == DockState.DockBottomAutoHide)
			{
				num2 = ((!show) ? 1 : (-1));
				num4 = (show ? 1 : (-1));
			}
			if (show)
			{
				base.Bounds = DockPanel.GetAutoHideWindowBounds(new Rectangle(-rectangle2.Width, -rectangle2.Height, rectangle2.Width, rectangle2.Height));
				if (!base.Visible)
				{
					base.Visible = true;
				}
				PerformLayout();
			}
			SuspendLayout();
			LayoutAnimateWindow(rectangle);
			if (!base.Visible)
			{
				base.Visible = true;
			}
			int num5 = 1;
			int num6 = ((rectangle.Width != rectangle2.Width) ? Math.Abs(rectangle.Width - rectangle2.Width) : Math.Abs(rectangle.Height - rectangle2.Height));
			DateTime now = DateTime.Now;
			while (rectangle != rectangle2)
			{
				DateTime now2 = DateTime.Now;
				rectangle.X += num * num5;
				rectangle.Y += num2 * num5;
				rectangle.Width += num3 * num5;
				rectangle.Height += num4 * num5;
				if (Math.Sign(rectangle2.X - rectangle.X) != Math.Sign(num))
				{
					rectangle.X = rectangle2.X;
				}
				if (Math.Sign(rectangle2.Y - rectangle.Y) != Math.Sign(num2))
				{
					rectangle.Y = rectangle2.Y;
				}
				if (Math.Sign(rectangle2.Width - rectangle.Width) != Math.Sign(num3))
				{
					rectangle.Width = rectangle2.Width;
				}
				if (Math.Sign(rectangle2.Height - rectangle.Height) != Math.Sign(num4))
				{
					rectangle.Height = rectangle2.Height;
				}
				LayoutAnimateWindow(rectangle);
				if (base.Parent != null)
				{
					base.Parent.Update();
				}
				num6 -= num5;
				do
				{
					TimeSpan timeSpan = new TimeSpan(0, 0, 0, 0, 100);
					TimeSpan timeSpan2 = DateTime.Now - now2;
					TimeSpan timeSpan3 = DateTime.Now - now;
					if ((int)(timeSpan - timeSpan3).TotalMilliseconds <= 0)
					{
						num5 = num6;
						break;
					}
					num5 = num6 * (int)timeSpan2.TotalMilliseconds / (int)(timeSpan - timeSpan3).TotalMilliseconds;
				}
				while (num5 < 1);
			}
			ResumeLayout();
			base.Parent.ResumeLayout();
		}

		private void LayoutAnimateWindow(Rectangle rect)
		{
			base.Bounds = DockPanel.GetAutoHideWindowBounds(rect);
			Rectangle clientRectangle = base.ClientRectangle;
			if (DockState == DockState.DockLeftAutoHide)
			{
				ActivePane.Location = new Point(clientRectangle.Right - 2 - DockPanel.Theme.Measures.AutoHideSplitterSize - ActivePane.Width, ActivePane.Location.Y);
			}
			else if (DockState == DockState.DockTopAutoHide)
			{
				ActivePane.Location = new Point(ActivePane.Location.X, clientRectangle.Bottom - 2 - DockPanel.Theme.Measures.AutoHideSplitterSize - ActivePane.Height);
			}
		}

		private Rectangle GetRectangle(bool show)
		{
			if (DockState == DockState.Unknown)
			{
				return Rectangle.Empty;
			}
			Rectangle autoHideWindowRectangle = DockPanel.AutoHideWindowRectangle;
			if (show)
			{
				return autoHideWindowRectangle;
			}
			if (DockState == DockState.DockLeftAutoHide)
			{
				autoHideWindowRectangle.Width = 0;
			}
			else if (DockState == DockState.DockRightAutoHide)
			{
				autoHideWindowRectangle.X += autoHideWindowRectangle.Width;
				autoHideWindowRectangle.Width = 0;
			}
			else if (DockState == DockState.DockTopAutoHide)
			{
				autoHideWindowRectangle.Height = 0;
			}
			else
			{
				autoHideWindowRectangle.Y += autoHideWindowRectangle.Height;
				autoHideWindowRectangle.Height = 0;
			}
			return autoHideWindowRectangle;
		}

		private void SetTimerMouseTrack()
		{
			if (ActivePane == null || ActivePane.IsActivated || FlagDragging)
			{
				m_timerMouseTrack.Enabled = false;
				return;
			}
			int num = SystemInformation.MouseHoverTime;
			if (num <= 0)
			{
				num = 400;
			}
			m_timerMouseTrack.Interval = 2 * num;
			m_timerMouseTrack.Enabled = true;
		}

		public void RefreshActiveContent()
		{
			if (ActiveContent != null && !DockHelper.IsDockStateAutoHide(ActiveContent.DockHandler.DockState))
			{
				FlagAnimate = false;
				ActiveContent = null;
				FlagAnimate = true;
			}
		}

		public void RefreshActivePane()
		{
			SetTimerMouseTrack();
		}

		private void TimerMouseTrack_Tick(object sender, EventArgs e)
		{
			if (base.IsDisposed)
			{
				return;
			}
			if (ActivePane == null || ActivePane.IsActivated)
			{
				m_timerMouseTrack.Enabled = false;
				return;
			}
			DockPane activePane = ActivePane;
			Point pt = PointToClient(Control.MousePosition);
			Point pt2 = DockPanel.PointToClient(Control.MousePosition);
			Rectangle tabStripRectangle = DockPanel.GetTabStripRectangle(activePane.DockState);
			if (!base.ClientRectangle.Contains(pt) && !tabStripRectangle.Contains(pt2))
			{
				ActiveContent = null;
				m_timerMouseTrack.Enabled = false;
			}
		}

		void ISplitterDragSource.BeginDrag(Rectangle rectSplitter)
		{
			FlagDragging = true;
		}

		void ISplitterDragSource.EndDrag()
		{
			FlagDragging = false;
		}

		void ISplitterDragSource.MoveSplitter(int offset)
		{
			Rectangle dockArea = DockPanel.DockArea;
			IDockContent activeContent = ActiveContent;
			if (DockState == DockState.DockLeftAutoHide && dockArea.Width > 0)
			{
				if (activeContent.DockHandler.AutoHidePortion < 1.0)
				{
					activeContent.DockHandler.AutoHidePortion += (double)offset / (double)dockArea.Width;
				}
				else
				{
					activeContent.DockHandler.AutoHidePortion = base.Width + offset;
				}
			}
			else if (DockState == DockState.DockRightAutoHide && dockArea.Width > 0)
			{
				if (activeContent.DockHandler.AutoHidePortion < 1.0)
				{
					activeContent.DockHandler.AutoHidePortion -= (double)offset / (double)dockArea.Width;
				}
				else
				{
					activeContent.DockHandler.AutoHidePortion = base.Width - offset;
				}
			}
			else if (DockState == DockState.DockBottomAutoHide && dockArea.Height > 0)
			{
				if (activeContent.DockHandler.AutoHidePortion < 1.0)
				{
					activeContent.DockHandler.AutoHidePortion -= (double)offset / (double)dockArea.Height;
				}
				else
				{
					activeContent.DockHandler.AutoHidePortion = base.Height - offset;
				}
			}
			else if (DockState == DockState.DockTopAutoHide && dockArea.Height > 0)
			{
				if (activeContent.DockHandler.AutoHidePortion < 1.0)
				{
					activeContent.DockHandler.AutoHidePortion += (double)offset / (double)dockArea.Height;
				}
				else
				{
					activeContent.DockHandler.AutoHidePortion = base.Height + offset;
				}
			}
		}
	}

	public interface IPaneIndicator : IHitTest
	{
		Point Location { get; set; }

		bool Visible { get; set; }

		int Left { get; }

		int Top { get; }

		int Right { get; }

		int Bottom { get; }

		Rectangle ClientRectangle { get; }

		int Width { get; }

		int Height { get; }

		GraphicsPath DisplayingGraphicsPath { get; }
	}

	public struct HotSpotIndex
	{
		private int m_x;

		private int m_y;

		private DockStyle m_dockStyle;

		public int X => m_x;

		public int Y => m_y;

		public DockStyle DockStyle => m_dockStyle;

		public HotSpotIndex(int x, int y, DockStyle dockStyle)
		{
			m_x = x;
			m_y = y;
			m_dockStyle = dockStyle;
		}
	}

	internal class DefaultPaneIndicator : PictureBox, IPaneIndicator, IHitTest
	{
		private static Bitmap _bitmapPaneDiamond = Resources.DockIndicator_PaneDiamond;

		private static Bitmap _bitmapPaneDiamondLeft = Resources.DockIndicator_PaneDiamond_Left;

		private static Bitmap _bitmapPaneDiamondRight = Resources.DockIndicator_PaneDiamond_Right;

		private static Bitmap _bitmapPaneDiamondTop = Resources.DockIndicator_PaneDiamond_Top;

		private static Bitmap _bitmapPaneDiamondBottom = Resources.DockIndicator_PaneDiamond_Bottom;

		private static Bitmap _bitmapPaneDiamondFill = Resources.DockIndicator_PaneDiamond_Fill;

		private static Bitmap _bitmapPaneDiamondHotSpot = Resources.DockIndicator_PaneDiamond_HotSpot;

		private static Bitmap _bitmapPaneDiamondHotSpotIndex = Resources.DockIndicator_PaneDiamond_HotSpotIndex;

		private static HotSpotIndex[] _hotSpots = new HotSpotIndex[5]
		{
			new HotSpotIndex(1, 0, DockStyle.Top),
			new HotSpotIndex(0, 1, DockStyle.Left),
			new HotSpotIndex(1, 1, DockStyle.Fill),
			new HotSpotIndex(2, 1, DockStyle.Right),
			new HotSpotIndex(1, 2, DockStyle.Bottom)
		};

		private GraphicsPath _displayingGraphicsPath = DrawHelper.CalculateGraphicsPathFromBitmap(_bitmapPaneDiamond);

		private DockStyle m_status;

		public GraphicsPath DisplayingGraphicsPath => _displayingGraphicsPath;

		public DockStyle Status
		{
			get
			{
				return m_status;
			}
			set
			{
				m_status = value;
				if (m_status == DockStyle.None)
				{
					base.Image = _bitmapPaneDiamond;
				}
				else if (m_status == DockStyle.Left)
				{
					base.Image = _bitmapPaneDiamondLeft;
				}
				else if (m_status == DockStyle.Right)
				{
					base.Image = _bitmapPaneDiamondRight;
				}
				else if (m_status == DockStyle.Top)
				{
					base.Image = _bitmapPaneDiamondTop;
				}
				else if (m_status == DockStyle.Bottom)
				{
					base.Image = _bitmapPaneDiamondBottom;
				}
				else if (m_status == DockStyle.Fill)
				{
					base.Image = _bitmapPaneDiamondFill;
				}
			}
		}

		public DefaultPaneIndicator()
		{
			base.SizeMode = PictureBoxSizeMode.AutoSize;
			base.Image = _bitmapPaneDiamond;
			base.Region = new Region(DisplayingGraphicsPath);
		}

		public DockStyle HitTest(Point pt)
		{
			if (!base.Visible)
			{
				return DockStyle.None;
			}
			pt = PointToClient(pt);
			if (!base.ClientRectangle.Contains(pt))
			{
				return DockStyle.None;
			}
			for (int i = _hotSpots.GetLowerBound(0); i <= _hotSpots.GetUpperBound(0); i++)
			{
				if (_bitmapPaneDiamondHotSpot.GetPixel(pt.X, pt.Y) == _bitmapPaneDiamondHotSpotIndex.GetPixel(_hotSpots[i].X, _hotSpots[i].Y))
				{
					return _hotSpots[i].DockStyle;
				}
			}
			return DockStyle.None;
		}
	}

	public interface IHitTest
	{
		DockStyle Status { get; set; }

		DockStyle HitTest(Point pt);
	}

	public interface IPanelIndicator : IHitTest
	{
		Point Location { get; set; }

		bool Visible { get; set; }

		Rectangle Bounds { get; }

		int Width { get; }

		int Height { get; }
	}

	internal class DefaultPanelIndicator : PictureBox, IPanelIndicator, IHitTest
	{
		private static Image _imagePanelLeft = Resources.DockIndicator_PanelLeft;

		private static Image _imagePanelRight = Resources.DockIndicator_PanelRight;

		private static Image _imagePanelTop = Resources.DockIndicator_PanelTop;

		private static Image _imagePanelBottom = Resources.DockIndicator_PanelBottom;

		private static Image _imagePanelFill = Resources.DockIndicator_PanelFill;

		private static Image _imagePanelLeftActive = Resources.DockIndicator_PanelLeft_Active;

		private static Image _imagePanelRightActive = Resources.DockIndicator_PanelRight_Active;

		private static Image _imagePanelTopActive = Resources.DockIndicator_PanelTop_Active;

		private static Image _imagePanelBottomActive = Resources.DockIndicator_PanelBottom_Active;

		private static Image _imagePanelFillActive = Resources.DockIndicator_PanelFill_Active;

		private DockStyle m_dockStyle;

		private DockStyle m_status;

		private bool m_isActivated;

		private DockStyle DockStyle => m_dockStyle;

		public DockStyle Status
		{
			get
			{
				return m_status;
			}
			set
			{
				if (value != DockStyle && value != DockStyle.None)
				{
					throw new InvalidEnumArgumentException();
				}
				if (m_status != value)
				{
					m_status = value;
					IsActivated = m_status != DockStyle.None;
				}
			}
		}

		private Image ImageInactive
		{
			get
			{
				if (DockStyle == DockStyle.Left)
				{
					return _imagePanelLeft;
				}
				if (DockStyle == DockStyle.Right)
				{
					return _imagePanelRight;
				}
				if (DockStyle == DockStyle.Top)
				{
					return _imagePanelTop;
				}
				if (DockStyle == DockStyle.Bottom)
				{
					return _imagePanelBottom;
				}
				if (DockStyle == DockStyle.Fill)
				{
					return _imagePanelFill;
				}
				return null;
			}
		}

		private Image ImageActive
		{
			get
			{
				if (DockStyle == DockStyle.Left)
				{
					return _imagePanelLeftActive;
				}
				if (DockStyle == DockStyle.Right)
				{
					return _imagePanelRightActive;
				}
				if (DockStyle == DockStyle.Top)
				{
					return _imagePanelTopActive;
				}
				if (DockStyle == DockStyle.Bottom)
				{
					return _imagePanelBottomActive;
				}
				if (DockStyle == DockStyle.Fill)
				{
					return _imagePanelFillActive;
				}
				return null;
			}
		}

		private bool IsActivated
		{
			get
			{
				return m_isActivated;
			}
			set
			{
				m_isActivated = value;
				base.Image = (IsActivated ? ImageActive : ImageInactive);
			}
		}

		public DefaultPanelIndicator(DockStyle dockStyle)
		{
			m_dockStyle = dockStyle;
			base.SizeMode = PictureBoxSizeMode.AutoSize;
			base.Image = ImageInactive;
		}

		public DockStyle HitTest(Point pt)
		{
			if (!base.Visible || !base.ClientRectangle.Contains(PointToClient(pt)))
			{
				return DockStyle.None;
			}
			return DockStyle;
		}
	}

	internal class DefaultDockOutline : DockOutlineBase
	{
		private DragForm m_dragForm;

		private DragForm DragForm => m_dragForm;

		public DefaultDockOutline()
		{
			m_dragForm = new DragForm();
			SetDragForm(Rectangle.Empty);
			DragForm.BackColor = SystemColors.ActiveCaption;
			DragForm.Opacity = 0.5;
			DragForm.Show(bActivate: false);
		}

		protected override void OnShow()
		{
			CalculateRegion();
		}

		protected override void OnClose()
		{
			DragForm.Close();
		}

		private void CalculateRegion()
		{
			if (!base.SameAsOldValue)
			{
				if (!base.FloatWindowBounds.IsEmpty)
				{
					SetOutline(base.FloatWindowBounds);
				}
				else if (base.DockTo is DockPanel)
				{
					SetOutline(base.DockTo as DockPanel, base.Dock, base.ContentIndex != 0);
				}
				else if (base.DockTo is DockPane)
				{
					SetOutline(base.DockTo as DockPane, base.Dock, base.ContentIndex);
				}
				else
				{
					SetOutline();
				}
			}
		}

		private void SetOutline()
		{
			SetDragForm(Rectangle.Empty);
		}

		private void SetOutline(Rectangle floatWindowBounds)
		{
			SetDragForm(floatWindowBounds);
		}

		private void SetOutline(DockPanel dockPanel, DockStyle dock, bool fullPanelEdge)
		{
			Rectangle dragForm = (fullPanelEdge ? dockPanel.DockArea : dockPanel.DocumentWindowBounds);
			dragForm.Location = dockPanel.PointToScreen(dragForm.Location);
			switch (dock)
			{
			case DockStyle.Top:
			{
				int dockWindowSize4 = dockPanel.GetDockWindowSize(DockState.DockTop);
				dragForm = new Rectangle(dragForm.X, dragForm.Y, dragForm.Width, dockWindowSize4);
				break;
			}
			case DockStyle.Bottom:
			{
				int dockWindowSize3 = dockPanel.GetDockWindowSize(DockState.DockBottom);
				dragForm = new Rectangle(dragForm.X, dragForm.Bottom - dockWindowSize3, dragForm.Width, dockWindowSize3);
				break;
			}
			case DockStyle.Left:
			{
				int dockWindowSize2 = dockPanel.GetDockWindowSize(DockState.DockLeft);
				dragForm = new Rectangle(dragForm.X, dragForm.Y, dockWindowSize2, dragForm.Height);
				break;
			}
			case DockStyle.Right:
			{
				int dockWindowSize = dockPanel.GetDockWindowSize(DockState.DockRight);
				dragForm = new Rectangle(dragForm.Right - dockWindowSize, dragForm.Y, dockWindowSize, dragForm.Height);
				break;
			}
			case DockStyle.Fill:
				dragForm = dockPanel.DocumentWindowBounds;
				dragForm.Location = dockPanel.PointToScreen(dragForm.Location);
				break;
			}
			SetDragForm(dragForm);
		}

		private void SetOutline(DockPane pane, DockStyle dock, int contentIndex)
		{
			if (dock != DockStyle.Fill)
			{
				Rectangle displayingRectangle = pane.DisplayingRectangle;
				if (dock == DockStyle.Right)
				{
					displayingRectangle.X += displayingRectangle.Width / 2;
				}
				if (dock == DockStyle.Bottom)
				{
					displayingRectangle.Y += displayingRectangle.Height / 2;
				}
				if (dock == DockStyle.Left || dock == DockStyle.Right)
				{
					displayingRectangle.Width -= displayingRectangle.Width / 2;
				}
				if (dock == DockStyle.Top || dock == DockStyle.Bottom)
				{
					displayingRectangle.Height -= displayingRectangle.Height / 2;
				}
				displayingRectangle.Location = pane.PointToScreen(displayingRectangle.Location);
				SetDragForm(displayingRectangle);
				return;
			}
			if (contentIndex == -1)
			{
				Rectangle displayingRectangle2 = pane.DisplayingRectangle;
				displayingRectangle2.Location = pane.PointToScreen(displayingRectangle2.Location);
				SetDragForm(displayingRectangle2);
				return;
			}
			using GraphicsPath graphicsPath = pane.TabStripControl.GetOutline(contentIndex);
			RectangleF bounds = graphicsPath.GetBounds();
			Rectangle rect = new Rectangle((int)bounds.X, (int)bounds.Y, (int)bounds.Width, (int)bounds.Height);
			using (Matrix matrix = new Matrix(rect, new Point[3]
			{
				new Point(0, 0),
				new Point(rect.Width, 0),
				new Point(0, rect.Height)
			}))
			{
				graphicsPath.Transform(matrix);
			}
			Region region = new Region(graphicsPath);
			SetDragForm(rect, region);
		}

		private void SetDragForm(Rectangle rect)
		{
			DragForm.Bounds = rect;
			if (rect == Rectangle.Empty)
			{
				if (DragForm.Region != null)
				{
					DragForm.Region.Dispose();
				}
				DragForm.Region = new Region(Rectangle.Empty);
			}
			else if (DragForm.Region != null)
			{
				DragForm.Region.Dispose();
				DragForm.Region = null;
			}
		}

		private void SetDragForm(Rectangle rect, Region region)
		{
			DragForm.Bounds = rect;
			DragForm.Region = region;
		}
	}

	public sealed class DockDragHandler : DragHandler
	{
		public class DockIndicator : DragForm
		{
			private int _PanelIndicatorMargin = 10;

			private DockDragHandler m_dragHandler;

			private IPaneIndicator m_paneDiamond;

			private IPanelIndicator m_panelLeft;

			private IPanelIndicator m_panelRight;

			private IPanelIndicator m_panelTop;

			private IPanelIndicator m_panelBottom;

			private IPanelIndicator m_panelFill;

			private bool m_fullPanelEdge;

			private DockPane m_dockPane;

			private IHitTest m_hitTest;

			private IPaneIndicator PaneDiamond
			{
				get
				{
					if (m_paneDiamond == null)
					{
						m_paneDiamond = m_dragHandler.DockPanel.Theme.Extender.PaneIndicatorFactory.CreatePaneIndicator(m_dragHandler.DockPanel.Theme);
					}
					return m_paneDiamond;
				}
			}

			private IPanelIndicator PanelLeft
			{
				get
				{
					if (m_panelLeft == null)
					{
						m_panelLeft = m_dragHandler.DockPanel.Theme.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Left, m_dragHandler.DockPanel.Theme);
					}
					return m_panelLeft;
				}
			}

			private IPanelIndicator PanelRight
			{
				get
				{
					if (m_panelRight == null)
					{
						m_panelRight = m_dragHandler.DockPanel.Theme.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Right, m_dragHandler.DockPanel.Theme);
					}
					return m_panelRight;
				}
			}

			private IPanelIndicator PanelTop
			{
				get
				{
					if (m_panelTop == null)
					{
						m_panelTop = m_dragHandler.DockPanel.Theme.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Top, m_dragHandler.DockPanel.Theme);
					}
					return m_panelTop;
				}
			}

			private IPanelIndicator PanelBottom
			{
				get
				{
					if (m_panelBottom == null)
					{
						m_panelBottom = m_dragHandler.DockPanel.Theme.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Bottom, m_dragHandler.DockPanel.Theme);
					}
					return m_panelBottom;
				}
			}

			private IPanelIndicator PanelFill
			{
				get
				{
					if (m_panelFill == null)
					{
						m_panelFill = m_dragHandler.DockPanel.Theme.Extender.PanelIndicatorFactory.CreatePanelIndicator(DockStyle.Fill, m_dragHandler.DockPanel.Theme);
					}
					return m_panelFill;
				}
			}

			public bool FullPanelEdge
			{
				get
				{
					return m_fullPanelEdge;
				}
				set
				{
					if (m_fullPanelEdge != value)
					{
						m_fullPanelEdge = value;
						RefreshChanges();
					}
				}
			}

			public DockDragHandler DragHandler => m_dragHandler;

			public DockPanel DockPanel => DragHandler.DockPanel;

			public DockPane DockPane
			{
				get
				{
					return m_dockPane;
				}
				internal set
				{
					if (m_dockPane != value)
					{
						DockPane displayingPane = DisplayingPane;
						m_dockPane = value;
						if (displayingPane != DisplayingPane)
						{
							RefreshChanges();
						}
					}
				}
			}

			private IHitTest HitTestResult
			{
				get
				{
					return m_hitTest;
				}
				set
				{
					if (m_hitTest != value)
					{
						if (m_hitTest != null)
						{
							m_hitTest.Status = DockStyle.None;
						}
						m_hitTest = value;
					}
				}
			}

			private DockPane DisplayingPane
			{
				get
				{
					if (!ShouldPaneDiamondVisible())
					{
						return null;
					}
					return DockPane;
				}
			}

			public DockIndicator(DockDragHandler dragHandler)
			{
				m_dragHandler = dragHandler;
				base.Controls.AddRange(new Control[6]
				{
					(Control)PaneDiamond,
					(Control)PanelLeft,
					(Control)PanelRight,
					(Control)PanelTop,
					(Control)PanelBottom,
					(Control)PanelFill
				});
				base.Region = new Region(Rectangle.Empty);
			}

			private void RefreshChanges()
			{
				Region region = new Region(Rectangle.Empty);
				Rectangle r = (FullPanelEdge ? DockPanel.DockArea : DockPanel.DocumentWindowBounds);
				r = RectangleToClient(DockPanel.RectangleToScreen(r));
				if (ShouldPanelIndicatorVisible(DockState.DockLeft))
				{
					PanelLeft.Location = new Point(r.X + _PanelIndicatorMargin, r.Y + (r.Height - PanelRight.Height) / 2);
					PanelLeft.Visible = true;
					region.Union(PanelLeft.Bounds);
				}
				else
				{
					PanelLeft.Visible = false;
				}
				if (ShouldPanelIndicatorVisible(DockState.DockRight))
				{
					PanelRight.Location = new Point(r.X + r.Width - PanelRight.Width - _PanelIndicatorMargin, r.Y + (r.Height - PanelRight.Height) / 2);
					PanelRight.Visible = true;
					region.Union(PanelRight.Bounds);
				}
				else
				{
					PanelRight.Visible = false;
				}
				if (ShouldPanelIndicatorVisible(DockState.DockTop))
				{
					PanelTop.Location = new Point(r.X + (r.Width - PanelTop.Width) / 2, r.Y + _PanelIndicatorMargin);
					PanelTop.Visible = true;
					region.Union(PanelTop.Bounds);
				}
				else
				{
					PanelTop.Visible = false;
				}
				if (ShouldPanelIndicatorVisible(DockState.DockBottom))
				{
					PanelBottom.Location = new Point(r.X + (r.Width - PanelBottom.Width) / 2, r.Y + r.Height - PanelBottom.Height - _PanelIndicatorMargin);
					PanelBottom.Visible = true;
					region.Union(PanelBottom.Bounds);
				}
				else
				{
					PanelBottom.Visible = false;
				}
				if (ShouldPanelIndicatorVisible(DockState.Document))
				{
					Rectangle rectangle = RectangleToClient(DockPanel.RectangleToScreen(DockPanel.DocumentWindowBounds));
					PanelFill.Location = new Point(rectangle.X + (rectangle.Width - PanelFill.Width) / 2, rectangle.Y + (rectangle.Height - PanelFill.Height) / 2);
					PanelFill.Visible = true;
					region.Union(PanelFill.Bounds);
				}
				else
				{
					PanelFill.Visible = false;
				}
				if (ShouldPaneDiamondVisible())
				{
					Rectangle rectangle2 = RectangleToClient(DockPane.RectangleToScreen(DockPane.ClientRectangle));
					PaneDiamond.Location = new Point(rectangle2.Left + (rectangle2.Width - PaneDiamond.Width) / 2, rectangle2.Top + (rectangle2.Height - PaneDiamond.Height) / 2);
					PaneDiamond.Visible = true;
					using GraphicsPath graphicsPath = PaneDiamond.DisplayingGraphicsPath.Clone() as GraphicsPath;
					Point[] plgpts = new Point[3]
					{
						new Point(PaneDiamond.Left, PaneDiamond.Top),
						new Point(PaneDiamond.Right, PaneDiamond.Top),
						new Point(PaneDiamond.Left, PaneDiamond.Bottom)
					};
					using (Matrix matrix = new Matrix(PaneDiamond.ClientRectangle, plgpts))
					{
						graphicsPath.Transform(matrix);
					}
					region.Union(graphicsPath);
				}
				else
				{
					PaneDiamond.Visible = false;
				}
				base.Region = region;
			}

			private bool ShouldPanelIndicatorVisible(DockState dockState)
			{
				if (!base.Visible)
				{
					return false;
				}
				if (DockPanel.DockWindows[dockState].Visible)
				{
					return false;
				}
				return DragHandler.DragSource.IsDockStateValid(dockState);
			}

			private bool ShouldPaneDiamondVisible()
			{
				if (DockPane == null)
				{
					return false;
				}
				if (!DockPanel.AllowEndUserNestedDocking)
				{
					return false;
				}
				return DragHandler.DragSource.CanDockTo(DockPane);
			}

			public override void Show(bool bActivate)
			{
				base.Show(bActivate);
				base.Bounds = SystemInformation.VirtualScreen;
				RefreshChanges();
			}

			public void TestDrop()
			{
				Point mousePosition = Control.MousePosition;
				DockPane = DockHelper.PaneAtPoint(mousePosition, DockPanel);
				if (TestDrop(PanelLeft, mousePosition) != DockStyle.None)
				{
					HitTestResult = PanelLeft;
				}
				else if (TestDrop(PanelRight, mousePosition) != DockStyle.None)
				{
					HitTestResult = PanelRight;
				}
				else if (TestDrop(PanelTop, mousePosition) != DockStyle.None)
				{
					HitTestResult = PanelTop;
				}
				else if (TestDrop(PanelBottom, mousePosition) != DockStyle.None)
				{
					HitTestResult = PanelBottom;
				}
				else if (TestDrop(PanelFill, mousePosition) != DockStyle.None)
				{
					HitTestResult = PanelFill;
				}
				else if (TestDrop(PaneDiamond, mousePosition) != DockStyle.None)
				{
					HitTestResult = PaneDiamond;
				}
				else
				{
					HitTestResult = null;
				}
				if (HitTestResult != null)
				{
					if (HitTestResult is IPaneIndicator)
					{
						DragHandler.Outline.Show(DockPane, HitTestResult.Status);
					}
					else
					{
						DragHandler.Outline.Show(DockPanel, HitTestResult.Status, FullPanelEdge);
					}
				}
			}

			private static DockStyle TestDrop(IHitTest hitTest, Point pt)
			{
				return hitTest.Status = hitTest.HitTest(pt);
			}
		}

		private DockOutlineBase m_outline;

		private DockIndicator m_indicator;

		private Rectangle m_floatOutlineBounds;

		public new IDockDragSource DragSource
		{
			get
			{
				return base.DragSource as IDockDragSource;
			}
			set
			{
				base.DragSource = value;
			}
		}

		public DockOutlineBase Outline
		{
			get
			{
				return m_outline;
			}
			private set
			{
				m_outline = value;
			}
		}

		private DockIndicator Indicator
		{
			get
			{
				return m_indicator;
			}
			set
			{
				m_indicator = value;
			}
		}

		private Rectangle FloatOutlineBounds
		{
			get
			{
				return m_floatOutlineBounds;
			}
			set
			{
				m_floatOutlineBounds = value;
			}
		}

		public DockDragHandler(DockPanel panel)
			: base(panel)
		{
		}

		public void BeginDrag(IDockDragSource dragSource)
		{
			DragSource = dragSource;
			if (!BeginDrag())
			{
				DragSource = null;
				return;
			}
			Outline = base.DockPanel.Theme.Extender.DockOutlineFactory.CreateDockOutline();
			Indicator = base.DockPanel.Theme.Extender.DockIndicatorFactory.CreateDockIndicator(this);
			Indicator.Show(bActivate: false);
			FloatOutlineBounds = DragSource.BeginDrag(base.StartMousePosition);
		}

		protected override void OnDragging()
		{
			TestDrop();
		}

		protected override void OnEndDrag(bool abort)
		{
			base.DockPanel.SuspendLayout(allWindows: true);
			Outline.Close();
			Indicator.Close();
			EndDrag(abort);
			base.DockPanel.PerformMdiClientLayout();
			base.DockPanel.ResumeLayout(performLayout: true, allWindows: true);
			DragSource.EndDrag();
			DragSource = null;
			base.DockPanel.OnDocumentDragged();
		}

		private void TestDrop()
		{
			Outline.FlagTestDrop = false;
			Indicator.FullPanelEdge = (Control.ModifierKeys & Keys.Shift) != 0;
			if ((Control.ModifierKeys & Keys.Control) == 0)
			{
				Indicator.TestDrop();
				if (!Outline.FlagTestDrop)
				{
					DockPane dockPane = DockHelper.PaneAtPoint(Control.MousePosition, base.DockPanel);
					if (dockPane != null && DragSource.IsDockStateValid(dockPane.DockState))
					{
						dockPane.TestDrop(DragSource, Outline);
					}
				}
				if (!Outline.FlagTestDrop && DragSource.IsDockStateValid(DockState.Float))
				{
					DockHelper.FloatWindowAtPoint(Control.MousePosition, base.DockPanel)?.TestDrop(DragSource, Outline);
				}
			}
			else
			{
				Indicator.DockPane = DockHelper.PaneAtPoint(Control.MousePosition, base.DockPanel);
			}
			if (!Outline.FlagTestDrop && DragSource.IsDockStateValid(DockState.Float))
			{
				Rectangle floatOutlineBounds = FloatOutlineBounds;
				floatOutlineBounds.Offset(Control.MousePosition.X - base.StartMousePosition.X, Control.MousePosition.Y - base.StartMousePosition.Y);
				Outline.Show(floatOutlineBounds);
			}
			if (!Outline.FlagTestDrop)
			{
				Cursor.Current = Cursors.No;
				Outline.Show();
			}
			else
			{
				Cursor.Current = DragControl.Cursor;
			}
		}

		private void EndDrag(bool abort)
		{
			if (!abort)
			{
				if (!Outline.FloatWindowBounds.IsEmpty)
				{
					DragSource.FloatAt(Outline.FloatWindowBounds);
				}
				else if (Outline.DockTo is DockPane)
				{
					DockPane pane = Outline.DockTo as DockPane;
					DragSource.DockTo(pane, Outline.Dock, Outline.ContentIndex);
				}
				else if (Outline.DockTo is DockPanel)
				{
					DockPanel dockPanel = Outline.DockTo as DockPanel;
					dockPanel.UpdateDockWindowZOrder(Outline.Dock, Outline.FlagFullEdge);
					DragSource.DockTo(dockPanel, Outline.Dock);
				}
			}
		}
	}

	private static class Persistor
	{
		private class DummyContent : DockContent
		{
		}

		private struct DockPanelStruct
		{
			private double m_dockLeftPortion;

			private double m_dockRightPortion;

			private double m_dockTopPortion;

			private double m_dockBottomPortion;

			private int m_indexActiveDocumentPane;

			private int m_indexActivePane;

			public double DockLeftPortion
			{
				get
				{
					return m_dockLeftPortion;
				}
				set
				{
					m_dockLeftPortion = value;
				}
			}

			public double DockRightPortion
			{
				get
				{
					return m_dockRightPortion;
				}
				set
				{
					m_dockRightPortion = value;
				}
			}

			public double DockTopPortion
			{
				get
				{
					return m_dockTopPortion;
				}
				set
				{
					m_dockTopPortion = value;
				}
			}

			public double DockBottomPortion
			{
				get
				{
					return m_dockBottomPortion;
				}
				set
				{
					m_dockBottomPortion = value;
				}
			}

			public int IndexActiveDocumentPane
			{
				get
				{
					return m_indexActiveDocumentPane;
				}
				set
				{
					m_indexActiveDocumentPane = value;
				}
			}

			public int IndexActivePane
			{
				get
				{
					return m_indexActivePane;
				}
				set
				{
					m_indexActivePane = value;
				}
			}
		}

		private struct ContentStruct
		{
			private string m_persistString;

			private double m_autoHidePortion;

			private bool m_isHidden;

			private bool m_isFloat;

			public string PersistString
			{
				get
				{
					return m_persistString;
				}
				set
				{
					m_persistString = value;
				}
			}

			public double AutoHidePortion
			{
				get
				{
					return m_autoHidePortion;
				}
				set
				{
					m_autoHidePortion = value;
				}
			}

			public bool IsHidden
			{
				get
				{
					return m_isHidden;
				}
				set
				{
					m_isHidden = value;
				}
			}

			public bool IsFloat
			{
				get
				{
					return m_isFloat;
				}
				set
				{
					m_isFloat = value;
				}
			}
		}

		private struct PaneStruct
		{
			private DockState m_dockState;

			private int m_indexActiveContent;

			private int[] m_indexContents;

			private int m_zOrderIndex;

			public DockState DockState
			{
				get
				{
					return m_dockState;
				}
				set
				{
					m_dockState = value;
				}
			}

			public int IndexActiveContent
			{
				get
				{
					return m_indexActiveContent;
				}
				set
				{
					m_indexActiveContent = value;
				}
			}

			public int[] IndexContents
			{
				get
				{
					return m_indexContents;
				}
				set
				{
					m_indexContents = value;
				}
			}

			public int ZOrderIndex
			{
				get
				{
					return m_zOrderIndex;
				}
				set
				{
					m_zOrderIndex = value;
				}
			}
		}

		private struct NestedPane
		{
			private int m_indexPane;

			private int m_indexPrevPane;

			private DockAlignment m_alignment;

			private double m_proportion;

			public int IndexPane
			{
				get
				{
					return m_indexPane;
				}
				set
				{
					m_indexPane = value;
				}
			}

			public int IndexPrevPane
			{
				get
				{
					return m_indexPrevPane;
				}
				set
				{
					m_indexPrevPane = value;
				}
			}

			public DockAlignment Alignment
			{
				get
				{
					return m_alignment;
				}
				set
				{
					m_alignment = value;
				}
			}

			public double Proportion
			{
				get
				{
					return m_proportion;
				}
				set
				{
					m_proportion = value;
				}
			}
		}

		private struct DockWindowStruct
		{
			private DockState m_dockState;

			private int m_zOrderIndex;

			private NestedPane[] m_nestedPanes;

			public DockState DockState
			{
				get
				{
					return m_dockState;
				}
				set
				{
					m_dockState = value;
				}
			}

			public int ZOrderIndex
			{
				get
				{
					return m_zOrderIndex;
				}
				set
				{
					m_zOrderIndex = value;
				}
			}

			public NestedPane[] NestedPanes
			{
				get
				{
					return m_nestedPanes;
				}
				set
				{
					m_nestedPanes = value;
				}
			}
		}

		private struct FloatWindowStruct
		{
			private Rectangle m_bounds;

			private int m_zOrderIndex;

			private NestedPane[] m_nestedPanes;

			public Rectangle Bounds
			{
				get
				{
					return m_bounds;
				}
				set
				{
					m_bounds = value;
				}
			}

			public int ZOrderIndex
			{
				get
				{
					return m_zOrderIndex;
				}
				set
				{
					m_zOrderIndex = value;
				}
			}

			public NestedPane[] NestedPanes
			{
				get
				{
					return m_nestedPanes;
				}
				set
				{
					m_nestedPanes = value;
				}
			}
		}

		private const string ConfigFileVersion = "1.0";

		private static string[] CompatibleConfigFileVersions = new string[0];

		public static void SaveAsXml(DockPanel dockPanel, string fileName)
		{
			SaveAsXml(dockPanel, fileName, Encoding.Unicode);
		}

		public static void SaveAsXml(DockPanel dockPanel, string fileName, Encoding encoding)
		{
			using FileStream fileStream = new FileStream(fileName, FileMode.Create);
			try
			{
				SaveAsXml(dockPanel, fileStream, encoding);
			}
			finally
			{
				fileStream.Close();
			}
		}

		public static void SaveAsXml(DockPanel dockPanel, Stream stream, Encoding encoding)
		{
			SaveAsXml(dockPanel, stream, encoding, upstream: false);
		}

		public static void SaveAsXml(DockPanel dockPanel, Stream stream, Encoding encoding, bool upstream)
		{
			XmlWriter xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings
			{
				Encoding = encoding,
				Indent = true
			});
			if (!upstream)
			{
				xmlWriter.WriteStartDocument();
			}
			xmlWriter.WriteComment(Strings.DockPanel_Persistor_XmlFileComment1);
			xmlWriter.WriteComment(Strings.DockPanel_Persistor_XmlFileComment2);
			xmlWriter.WriteStartElement("DockPanel");
			xmlWriter.WriteAttributeString("FormatVersion", "1.0");
			xmlWriter.WriteAttributeString("DockLeftPortion", dockPanel.DockLeftPortion.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("DockRightPortion", dockPanel.DockRightPortion.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("DockTopPortion", dockPanel.DockTopPortion.ToString(CultureInfo.InvariantCulture));
			xmlWriter.WriteAttributeString("DockBottomPortion", dockPanel.DockBottomPortion.ToString(CultureInfo.InvariantCulture));
			if (!Win32Helper.IsRunningOnMono)
			{
				xmlWriter.WriteAttributeString("ActiveDocumentPane", dockPanel.Panes.IndexOf(dockPanel.ActiveDocumentPane).ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteAttributeString("ActivePane", dockPanel.Panes.IndexOf(dockPanel.ActivePane).ToString(CultureInfo.InvariantCulture));
			}
			xmlWriter.WriteStartElement("Contents");
			xmlWriter.WriteAttributeString("Count", dockPanel.Contents.Count.ToString(CultureInfo.InvariantCulture));
			foreach (IDockContent content in dockPanel.Contents)
			{
				xmlWriter.WriteStartElement("Content");
				xmlWriter.WriteAttributeString("ID", dockPanel.Contents.IndexOf(content).ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteAttributeString("PersistString", content.DockHandler.PersistString);
				xmlWriter.WriteAttributeString("AutoHidePortion", content.DockHandler.AutoHidePortion.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteAttributeString("IsHidden", content.DockHandler.IsHidden.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteAttributeString("IsFloat", content.DockHandler.IsFloat.ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("Panes");
			xmlWriter.WriteAttributeString("Count", dockPanel.Panes.Count.ToString(CultureInfo.InvariantCulture));
			foreach (DockPane pane in dockPanel.Panes)
			{
				xmlWriter.WriteStartElement("Pane");
				xmlWriter.WriteAttributeString("ID", dockPanel.Panes.IndexOf(pane).ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteAttributeString("DockState", pane.DockState.ToString());
				xmlWriter.WriteAttributeString("ActiveContent", dockPanel.Contents.IndexOf(pane.ActiveContent).ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteStartElement("Contents");
				xmlWriter.WriteAttributeString("Count", pane.Contents.Count.ToString(CultureInfo.InvariantCulture));
				foreach (IDockContent content2 in pane.Contents)
				{
					xmlWriter.WriteStartElement("Content");
					xmlWriter.WriteAttributeString("ID", pane.Contents.IndexOf(content2).ToString(CultureInfo.InvariantCulture));
					xmlWriter.WriteAttributeString("RefID", dockPanel.Contents.IndexOf(content2).ToString(CultureInfo.InvariantCulture));
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteStartElement("DockWindows");
			int num = 0;
			foreach (DockWindow dockWindow in dockPanel.DockWindows)
			{
				xmlWriter.WriteStartElement("DockWindow");
				xmlWriter.WriteAttributeString("ID", num.ToString(CultureInfo.InvariantCulture));
				num++;
				xmlWriter.WriteAttributeString("DockState", dockWindow.DockState.ToString());
				xmlWriter.WriteAttributeString("ZOrderIndex", dockPanel.Controls.IndexOf(dockWindow).ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteStartElement("NestedPanes");
				xmlWriter.WriteAttributeString("Count", dockWindow.NestedPanes.Count.ToString(CultureInfo.InvariantCulture));
				foreach (DockPane nestedPane in dockWindow.NestedPanes)
				{
					xmlWriter.WriteStartElement("Pane");
					xmlWriter.WriteAttributeString("ID", dockWindow.NestedPanes.IndexOf(nestedPane).ToString(CultureInfo.InvariantCulture));
					xmlWriter.WriteAttributeString("RefID", dockPanel.Panes.IndexOf(nestedPane).ToString(CultureInfo.InvariantCulture));
					NestedDockingStatus nestedDockingStatus = nestedPane.NestedDockingStatus;
					xmlWriter.WriteAttributeString("PrevPane", dockPanel.Panes.IndexOf(nestedDockingStatus.PreviousPane).ToString(CultureInfo.InvariantCulture));
					xmlWriter.WriteAttributeString("Alignment", nestedDockingStatus.Alignment.ToString());
					xmlWriter.WriteAttributeString("Proportion", nestedDockingStatus.Proportion.ToString(CultureInfo.InvariantCulture));
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			RectangleConverter rectangleConverter = new RectangleConverter();
			xmlWriter.WriteStartElement("FloatWindows");
			xmlWriter.WriteAttributeString("Count", dockPanel.FloatWindows.Count.ToString(CultureInfo.InvariantCulture));
			foreach (FloatWindow floatWindow in dockPanel.FloatWindows)
			{
				xmlWriter.WriteStartElement("FloatWindow");
				xmlWriter.WriteAttributeString("ID", dockPanel.FloatWindows.IndexOf(floatWindow).ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteAttributeString("Bounds", rectangleConverter.ConvertToInvariantString(floatWindow.Bounds));
				xmlWriter.WriteAttributeString("ZOrderIndex", floatWindow.DockPanel.FloatWindows.IndexOf(floatWindow).ToString(CultureInfo.InvariantCulture));
				xmlWriter.WriteStartElement("NestedPanes");
				xmlWriter.WriteAttributeString("Count", floatWindow.NestedPanes.Count.ToString(CultureInfo.InvariantCulture));
				foreach (DockPane nestedPane2 in floatWindow.NestedPanes)
				{
					xmlWriter.WriteStartElement("Pane");
					xmlWriter.WriteAttributeString("ID", floatWindow.NestedPanes.IndexOf(nestedPane2).ToString(CultureInfo.InvariantCulture));
					xmlWriter.WriteAttributeString("RefID", dockPanel.Panes.IndexOf(nestedPane2).ToString(CultureInfo.InvariantCulture));
					NestedDockingStatus nestedDockingStatus2 = nestedPane2.NestedDockingStatus;
					xmlWriter.WriteAttributeString("PrevPane", dockPanel.Panes.IndexOf(nestedDockingStatus2.PreviousPane).ToString(CultureInfo.InvariantCulture));
					xmlWriter.WriteAttributeString("Alignment", nestedDockingStatus2.Alignment.ToString());
					xmlWriter.WriteAttributeString("Proportion", nestedDockingStatus2.Proportion.ToString(CultureInfo.InvariantCulture));
					xmlWriter.WriteEndElement();
				}
				xmlWriter.WriteEndElement();
				xmlWriter.WriteEndElement();
			}
			xmlWriter.WriteEndElement();
			xmlWriter.WriteEndElement();
			if (!upstream)
			{
				xmlWriter.WriteEndDocument();
				xmlWriter.Close();
			}
			else
			{
				xmlWriter.Flush();
			}
		}

		public static void LoadFromXml(DockPanel dockPanel, string fileName, DeserializeDockContent deserializeContent)
		{
			using FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			try
			{
				LoadFromXml(dockPanel, fileStream, deserializeContent, closeStream: true);
			}
			finally
			{
				fileStream.Close();
			}
		}

		private static ContentStruct[] LoadContents(XmlTextReader xmlIn)
		{
			int num = Convert.ToInt32(xmlIn.GetAttribute("Count"), CultureInfo.InvariantCulture);
			ContentStruct[] array = new ContentStruct[num];
			MoveToNextElement(xmlIn);
			for (int i = 0; i < num; i++)
			{
				int num2 = Convert.ToInt32(xmlIn.GetAttribute("ID"), CultureInfo.InvariantCulture);
				if (xmlIn.Name != "Content" || num2 != i)
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				array[i].PersistString = xmlIn.GetAttribute("PersistString");
				array[i].AutoHidePortion = Convert.ToDouble(xmlIn.GetAttribute("AutoHidePortion"), CultureInfo.InvariantCulture);
				array[i].IsHidden = Convert.ToBoolean(xmlIn.GetAttribute("IsHidden"), CultureInfo.InvariantCulture);
				array[i].IsFloat = Convert.ToBoolean(xmlIn.GetAttribute("IsFloat"), CultureInfo.InvariantCulture);
				MoveToNextElement(xmlIn);
			}
			return array;
		}

		private static PaneStruct[] LoadPanes(XmlTextReader xmlIn)
		{
			EnumConverter enumConverter = new EnumConverter(typeof(DockState));
			int num = Convert.ToInt32(xmlIn.GetAttribute("Count"), CultureInfo.InvariantCulture);
			PaneStruct[] array = new PaneStruct[num];
			MoveToNextElement(xmlIn);
			for (int i = 0; i < num; i++)
			{
				int num2 = Convert.ToInt32(xmlIn.GetAttribute("ID"), CultureInfo.InvariantCulture);
				if (xmlIn.Name != "Pane" || num2 != i)
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				array[i].DockState = (DockState)enumConverter.ConvertFrom(xmlIn.GetAttribute("DockState"));
				array[i].IndexActiveContent = Convert.ToInt32(xmlIn.GetAttribute("ActiveContent"), CultureInfo.InvariantCulture);
				array[i].ZOrderIndex = -1;
				MoveToNextElement(xmlIn);
				if (xmlIn.Name != "Contents")
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				int num3 = Convert.ToInt32(xmlIn.GetAttribute("Count"), CultureInfo.InvariantCulture);
				array[i].IndexContents = new int[num3];
				MoveToNextElement(xmlIn);
				for (int j = 0; j < num3; j++)
				{
					int num4 = Convert.ToInt32(xmlIn.GetAttribute("ID"), CultureInfo.InvariantCulture);
					if (xmlIn.Name != "Content" || num4 != j)
					{
						throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
					}
					array[i].IndexContents[j] = Convert.ToInt32(xmlIn.GetAttribute("RefID"), CultureInfo.InvariantCulture);
					MoveToNextElement(xmlIn);
				}
			}
			return array;
		}

		private static DockWindowStruct[] LoadDockWindows(XmlTextReader xmlIn, DockPanel dockPanel)
		{
			EnumConverter enumConverter = new EnumConverter(typeof(DockState));
			EnumConverter enumConverter2 = new EnumConverter(typeof(DockAlignment));
			int count = dockPanel.DockWindows.Count;
			DockWindowStruct[] array = new DockWindowStruct[count];
			MoveToNextElement(xmlIn);
			for (int i = 0; i < count; i++)
			{
				int num = Convert.ToInt32(xmlIn.GetAttribute("ID"), CultureInfo.InvariantCulture);
				if (xmlIn.Name != "DockWindow" || num != i)
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				array[i].DockState = (DockState)enumConverter.ConvertFrom(xmlIn.GetAttribute("DockState"));
				array[i].ZOrderIndex = Convert.ToInt32(xmlIn.GetAttribute("ZOrderIndex"), CultureInfo.InvariantCulture);
				MoveToNextElement(xmlIn);
				if (xmlIn.Name != "DockList" && xmlIn.Name != "NestedPanes")
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				int num2 = Convert.ToInt32(xmlIn.GetAttribute("Count"), CultureInfo.InvariantCulture);
				array[i].NestedPanes = new NestedPane[num2];
				MoveToNextElement(xmlIn);
				for (int j = 0; j < num2; j++)
				{
					int num3 = Convert.ToInt32(xmlIn.GetAttribute("ID"), CultureInfo.InvariantCulture);
					if (xmlIn.Name != "Pane" || num3 != j)
					{
						throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
					}
					array[i].NestedPanes[j].IndexPane = Convert.ToInt32(xmlIn.GetAttribute("RefID"), CultureInfo.InvariantCulture);
					array[i].NestedPanes[j].IndexPrevPane = Convert.ToInt32(xmlIn.GetAttribute("PrevPane"), CultureInfo.InvariantCulture);
					array[i].NestedPanes[j].Alignment = (DockAlignment)enumConverter2.ConvertFrom(xmlIn.GetAttribute("Alignment"));
					array[i].NestedPanes[j].Proportion = Convert.ToDouble(xmlIn.GetAttribute("Proportion"), CultureInfo.InvariantCulture);
					MoveToNextElement(xmlIn);
				}
			}
			return array;
		}

		private static FloatWindowStruct[] LoadFloatWindows(XmlTextReader xmlIn)
		{
			EnumConverter enumConverter = new EnumConverter(typeof(DockAlignment));
			RectangleConverter rectangleConverter = new RectangleConverter();
			int num = Convert.ToInt32(xmlIn.GetAttribute("Count"), CultureInfo.InvariantCulture);
			FloatWindowStruct[] array = new FloatWindowStruct[num];
			MoveToNextElement(xmlIn);
			for (int i = 0; i < num; i++)
			{
				int num2 = Convert.ToInt32(xmlIn.GetAttribute("ID"), CultureInfo.InvariantCulture);
				if (xmlIn.Name != "FloatWindow" || num2 != i)
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				array[i].Bounds = (Rectangle)rectangleConverter.ConvertFromInvariantString(xmlIn.GetAttribute("Bounds"));
				array[i].ZOrderIndex = Convert.ToInt32(xmlIn.GetAttribute("ZOrderIndex"), CultureInfo.InvariantCulture);
				MoveToNextElement(xmlIn);
				if (xmlIn.Name != "DockList" && xmlIn.Name != "NestedPanes")
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				int num3 = Convert.ToInt32(xmlIn.GetAttribute("Count"), CultureInfo.InvariantCulture);
				array[i].NestedPanes = new NestedPane[num3];
				MoveToNextElement(xmlIn);
				for (int j = 0; j < num3; j++)
				{
					int num4 = Convert.ToInt32(xmlIn.GetAttribute("ID"), CultureInfo.InvariantCulture);
					if (xmlIn.Name != "Pane" || num4 != j)
					{
						throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
					}
					array[i].NestedPanes[j].IndexPane = Convert.ToInt32(xmlIn.GetAttribute("RefID"), CultureInfo.InvariantCulture);
					array[i].NestedPanes[j].IndexPrevPane = Convert.ToInt32(xmlIn.GetAttribute("PrevPane"), CultureInfo.InvariantCulture);
					array[i].NestedPanes[j].Alignment = (DockAlignment)enumConverter.ConvertFrom(xmlIn.GetAttribute("Alignment"));
					array[i].NestedPanes[j].Proportion = Convert.ToDouble(xmlIn.GetAttribute("Proportion"), CultureInfo.InvariantCulture);
					MoveToNextElement(xmlIn);
				}
			}
			return array;
		}

		public static void LoadFromXml(DockPanel dockPanel, Stream stream, DeserializeDockContent deserializeContent, bool closeStream)
		{
			if (dockPanel.Contents.Count != 0)
			{
				throw new InvalidOperationException(Strings.DockPanel_LoadFromXml_AlreadyInitialized);
			}
			DockPanelStruct dockPanelStruct;
			ContentStruct[] array;
			PaneStruct[] array2;
			DockWindowStruct[] array3;
			FloatWindowStruct[] array4;
			using (XmlTextReader xmlTextReader = new XmlTextReader(stream)
			{
				WhitespaceHandling = WhitespaceHandling.None
			})
			{
				xmlTextReader.MoveToContent();
				while (!xmlTextReader.Name.Equals("DockPanel"))
				{
					if (!MoveToNextElement(xmlTextReader))
					{
						throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
					}
				}
				if (!IsFormatVersionValid(xmlTextReader.GetAttribute("FormatVersion")))
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidFormatVersion);
				}
				dockPanelStruct = new DockPanelStruct
				{
					DockLeftPortion = Convert.ToDouble(xmlTextReader.GetAttribute("DockLeftPortion"), CultureInfo.InvariantCulture),
					DockRightPortion = Convert.ToDouble(xmlTextReader.GetAttribute("DockRightPortion"), CultureInfo.InvariantCulture),
					DockTopPortion = Convert.ToDouble(xmlTextReader.GetAttribute("DockTopPortion"), CultureInfo.InvariantCulture),
					DockBottomPortion = Convert.ToDouble(xmlTextReader.GetAttribute("DockBottomPortion"), CultureInfo.InvariantCulture),
					IndexActiveDocumentPane = Convert.ToInt32(xmlTextReader.GetAttribute("ActiveDocumentPane"), CultureInfo.InvariantCulture),
					IndexActivePane = Convert.ToInt32(xmlTextReader.GetAttribute("ActivePane"), CultureInfo.InvariantCulture)
				};
				MoveToNextElement(xmlTextReader);
				if (xmlTextReader.Name != "Contents")
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				array = LoadContents(xmlTextReader);
				if (xmlTextReader.Name != "Panes")
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				array2 = LoadPanes(xmlTextReader);
				if (xmlTextReader.Name != "DockWindows")
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				array3 = LoadDockWindows(xmlTextReader, dockPanel);
				if (xmlTextReader.Name != "FloatWindows")
				{
					throw new ArgumentException(Strings.DockPanel_LoadFromXml_InvalidXmlFormat);
				}
				array4 = LoadFloatWindows(xmlTextReader);
				if (closeStream)
				{
					xmlTextReader.Close();
				}
			}
			dockPanel.SuspendLayout(allWindows: true);
			dockPanel.DockLeftPortion = dockPanelStruct.DockLeftPortion;
			dockPanel.DockRightPortion = dockPanelStruct.DockRightPortion;
			dockPanel.DockTopPortion = dockPanelStruct.DockTopPortion;
			dockPanel.DockBottomPortion = dockPanelStruct.DockBottomPortion;
			int num = int.MaxValue;
			for (int i = 0; i < array3.Length; i++)
			{
				int num2 = -1;
				int num3 = -1;
				for (int j = 0; j < array3.Length; j++)
				{
					if (array3[j].ZOrderIndex > num2 && array3[j].ZOrderIndex < num)
					{
						num2 = array3[j].ZOrderIndex;
						num3 = j;
					}
				}
				dockPanel.DockWindows[array3[num3].DockState].BringToFront();
				num = num2;
			}
			for (int k = 0; k < array.Length; k++)
			{
				IDockContent dockContent = deserializeContent(array[k].PersistString);
				if (dockContent == null)
				{
					dockContent = new DummyContent();
				}
				dockContent.DockHandler.DockPanel = dockPanel;
				dockContent.DockHandler.AutoHidePortion = array[k].AutoHidePortion;
				dockContent.DockHandler.IsHidden = true;
				dockContent.DockHandler.IsFloat = array[k].IsFloat;
			}
			for (int l = 0; l < array2.Length; l++)
			{
				DockPane dockPane = null;
				for (int m = 0; m < array2[l].IndexContents.Length; m++)
				{
					IDockContent dockContent2 = dockPanel.Contents[array2[l].IndexContents[m]];
					if (m == 0)
					{
						dockPane = dockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(dockContent2, array2[l].DockState, show: false);
					}
					else if (array2[l].DockState == DockState.Float)
					{
						dockContent2.DockHandler.FloatPane = dockPane;
					}
					else
					{
						dockContent2.DockHandler.PanelPane = dockPane;
					}
				}
			}
			for (int n = 0; n < array3.Length; n++)
			{
				for (int num4 = 0; num4 < array3[n].NestedPanes.Length; num4++)
				{
					DockWindow dockWindow = dockPanel.DockWindows[array3[n].DockState];
					int indexPane = array3[n].NestedPanes[num4].IndexPane;
					DockPane dockPane2 = dockPanel.Panes[indexPane];
					int indexPrevPane = array3[n].NestedPanes[num4].IndexPrevPane;
					DockPane previousPane = ((indexPrevPane == -1) ? dockWindow.NestedPanes.GetDefaultPreviousPane(dockPane2) : dockPanel.Panes[indexPrevPane]);
					DockAlignment alignment = array3[n].NestedPanes[num4].Alignment;
					double proportion = array3[n].NestedPanes[num4].Proportion;
					dockPane2.DockTo(dockWindow, previousPane, alignment, proportion);
					if (array2[indexPane].DockState == dockWindow.DockState)
					{
						array2[indexPane].ZOrderIndex = array3[n].ZOrderIndex;
					}
				}
			}
			for (int num5 = 0; num5 < array4.Length; num5++)
			{
				FloatWindow floatWindow = null;
				for (int num6 = 0; num6 < array4[num5].NestedPanes.Length; num6++)
				{
					int indexPane2 = array4[num5].NestedPanes[num6].IndexPane;
					DockPane dockPane3 = dockPanel.Panes[indexPane2];
					if (num6 == 0)
					{
						floatWindow = dockPanel.Theme.Extender.FloatWindowFactory.CreateFloatWindow(dockPanel, dockPane3, array4[num5].Bounds);
					}
					else
					{
						int indexPrevPane2 = array4[num5].NestedPanes[num6].IndexPrevPane;
						DockPane previousPane2 = ((indexPrevPane2 == -1) ? null : dockPanel.Panes[indexPrevPane2]);
						DockAlignment alignment2 = array4[num5].NestedPanes[num6].Alignment;
						double proportion2 = array4[num5].NestedPanes[num6].Proportion;
						dockPane3.DockTo(floatWindow, previousPane2, alignment2, proportion2);
					}
					if (array2[indexPane2].DockState == floatWindow.DockState)
					{
						array2[indexPane2].ZOrderIndex = array4[num5].ZOrderIndex;
					}
				}
			}
			int[] array5 = null;
			if (array.Length != 0)
			{
				array5 = new int[array.Length];
				for (int num7 = 0; num7 < array.Length; num7++)
				{
					array5[num7] = num7;
				}
				_ = array.Length;
				for (int num8 = 0; num8 < array.Length - 1; num8++)
				{
					for (int num9 = num8 + 1; num9 < array.Length; num9++)
					{
						DockPane pane = dockPanel.Contents[array5[num8]].DockHandler.Pane;
						int num10 = ((pane != null) ? array2[dockPanel.Panes.IndexOf(pane)].ZOrderIndex : 0);
						DockPane pane2 = dockPanel.Contents[array5[num9]].DockHandler.Pane;
						int num11 = ((pane2 != null) ? array2[dockPanel.Panes.IndexOf(pane2)].ZOrderIndex : 0);
						if (num10 > num11)
						{
							int num12 = array5[num8];
							array5[num8] = array5[num9];
							array5[num9] = num12;
						}
					}
				}
			}
			for (int num13 = 0; num13 < array.Length; num13++)
			{
				IDockContent dockContent3 = dockPanel.Contents[array5[num13]];
				if (dockContent3.DockHandler.Pane != null && dockContent3.DockHandler.Pane.DockState != DockState.Document)
				{
					dockContent3.DockHandler.SuspendAutoHidePortionUpdates = true;
					dockContent3.DockHandler.IsHidden = array[array5[num13]].IsHidden;
					dockContent3.DockHandler.SuspendAutoHidePortionUpdates = false;
				}
			}
			for (int num14 = 0; num14 < array.Length; num14++)
			{
				IDockContent dockContent4 = dockPanel.Contents[array5[num14]];
				if (dockContent4.DockHandler.Pane != null && dockContent4.DockHandler.Pane.DockState == DockState.Document)
				{
					dockContent4.DockHandler.SuspendAutoHidePortionUpdates = true;
					dockContent4.DockHandler.IsHidden = array[array5[num14]].IsHidden;
					dockContent4.DockHandler.SuspendAutoHidePortionUpdates = false;
				}
			}
			for (int num15 = 0; num15 < array2.Length; num15++)
			{
				dockPanel.Panes[num15].ActiveContent = ((array2[num15].IndexActiveContent == -1) ? null : dockPanel.Contents[array2[num15].IndexActiveContent]);
			}
			if (dockPanelStruct.IndexActiveDocumentPane >= 0 && dockPanel.Panes.Count > dockPanelStruct.IndexActiveDocumentPane)
			{
				dockPanel.Panes[dockPanelStruct.IndexActiveDocumentPane].Activate();
			}
			if (dockPanelStruct.IndexActivePane >= 0 && dockPanel.Panes.Count > dockPanelStruct.IndexActivePane)
			{
				dockPanel.Panes[dockPanelStruct.IndexActivePane].Activate();
			}
			for (int num16 = dockPanel.Contents.Count - 1; num16 >= 0; num16--)
			{
				if (dockPanel.Contents[num16] is DummyContent)
				{
					dockPanel.Contents[num16].DockHandler.Form.Close();
				}
			}
			dockPanel.ResumeLayout(performLayout: true, allWindows: true);
		}

		private static bool MoveToNextElement(XmlTextReader xmlIn)
		{
			if (!xmlIn.Read())
			{
				return false;
			}
			while (xmlIn.NodeType == XmlNodeType.EndElement)
			{
				if (!xmlIn.Read())
				{
					return false;
				}
			}
			return true;
		}

		private static bool IsFormatVersionValid(string formatVersion)
		{
			if (formatVersion == "1.0")
			{
				return true;
			}
			string[] compatibleConfigFileVersions = CompatibleConfigFileVersions;
			for (int i = 0; i < compatibleConfigFileVersions.Length; i++)
			{
				if (compatibleConfigFileVersions[i] == formatVersion)
				{
					return true;
				}
			}
			return false;
		}
	}

	private interface IFocusManager
	{
		bool IsFocusTrackingSuspended { get; }

		IDockContent ActiveContent { get; }

		DockPane ActivePane { get; }

		IDockContent ActiveDocument { get; }

		DockPane ActiveDocumentPane { get; }

		void SuspendFocusTracking();

		void ResumeFocusTracking();
	}

	private class FocusManagerImpl : Component, IContentFocusManager, IFocusManager
	{
		private class HookEventArgs : EventArgs
		{
			public int HookCode;

			public IntPtr wParam;

			public IntPtr lParam;
		}

		private class LocalWindowsHook : IDisposable
		{
			public delegate void HookEventHandler(object sender, HookEventArgs e);

			private IntPtr m_hHook = IntPtr.Zero;

			private NativeMethods.HookProc m_filterFunc;

			private HookType m_hookType;

			public event HookEventHandler HookInvoked;

			protected void OnHookInvoked(HookEventArgs e)
			{
				if (this.HookInvoked != null)
				{
					this.HookInvoked(this, e);
				}
			}

			public LocalWindowsHook(HookType hook)
			{
				m_hookType = hook;
				m_filterFunc = CoreHookProc;
			}

			public IntPtr CoreHookProc(int code, IntPtr wParam, IntPtr lParam)
			{
				if (code < 0)
				{
					return NativeMethods.CallNextHookEx(m_hHook, code, wParam, lParam);
				}
				HookEventArgs e = new HookEventArgs();
				e.HookCode = code;
				e.wParam = wParam;
				e.lParam = lParam;
				OnHookInvoked(e);
				return NativeMethods.CallNextHookEx(m_hHook, code, wParam, lParam);
			}

			public void Install()
			{
				if (m_hHook != IntPtr.Zero)
				{
					Uninstall();
				}
				int currentThreadId = NativeMethods.GetCurrentThreadId();
				m_hHook = NativeMethods.SetWindowsHookEx(m_hookType, m_filterFunc, IntPtr.Zero, currentThreadId);
			}

			public void Uninstall()
			{
				if (m_hHook != IntPtr.Zero)
				{
					NativeMethods.UnhookWindowsHookEx(m_hHook);
					m_hHook = IntPtr.Zero;
				}
			}

			~LocalWindowsHook()
			{
				Dispose(disposing: false);
			}

			public void Dispose()
			{
				Dispose(disposing: true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				Uninstall();
			}
		}

		[ThreadStatic]
		private static LocalWindowsHook sm_localWindowsHook;

		[ThreadStatic]
		private static int _referenceCount;

		private readonly LocalWindowsHook.HookEventHandler m_hookEventHandler;

		private DockPanel m_dockPanel;

		private bool m_disposed;

		private IDockContent m_contentActivating;

		private List<IDockContent> m_listContent = new List<IDockContent>();

		private IDockContent m_lastActiveContent;

		private uint m_countSuspendFocusTracking;

		private bool m_inRefreshActiveWindow;

		private DockPane m_activePane;

		private IDockContent m_activeContent;

		private DockPane m_activeDocumentPane;

		private IDockContent m_activeDocument;

		public DockPanel DockPanel => m_dockPanel;

		private IDockContent ContentActivating
		{
			get
			{
				return m_contentActivating;
			}
			set
			{
				m_contentActivating = value;
			}
		}

		private List<IDockContent> ListContent => m_listContent;

		private IDockContent LastActiveContent
		{
			get
			{
				return m_lastActiveContent;
			}
			set
			{
				m_lastActiveContent = value;
			}
		}

		public bool IsFocusTrackingSuspended => m_countSuspendFocusTracking != 0;

		private bool InRefreshActiveWindow => m_inRefreshActiveWindow;

		public DockPane ActivePane => m_activePane;

		public IDockContent ActiveContent => m_activeContent;

		public DockPane ActiveDocumentPane => m_activeDocumentPane;

		public IDockContent ActiveDocument => m_activeDocument;

		public FocusManagerImpl(DockPanel dockPanel)
		{
			m_dockPanel = dockPanel;
			if (!Win32Helper.IsRunningOnMono)
			{
				m_hookEventHandler = HookEventHandler;
				if (sm_localWindowsHook == null)
				{
					sm_localWindowsHook = new LocalWindowsHook(HookType.WH_CALLWNDPROCRET);
					sm_localWindowsHook.Install();
				}
				sm_localWindowsHook.HookInvoked += m_hookEventHandler;
				_referenceCount++;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!m_disposed && disposing)
			{
				if (!Win32Helper.IsRunningOnMono)
				{
					sm_localWindowsHook.HookInvoked -= m_hookEventHandler;
				}
				_referenceCount--;
				if (_referenceCount == 0 && sm_localWindowsHook != null)
				{
					sm_localWindowsHook.Dispose();
					sm_localWindowsHook = null;
				}
				m_disposed = true;
			}
			base.Dispose(disposing);
		}

		public void Activate(IDockContent content)
		{
			if (IsFocusTrackingSuspended)
			{
				ContentActivating = content;
			}
			else
			{
				if (content == null)
				{
					return;
				}
				DockContentHandler dockHandler = content.DockHandler;
				if (!dockHandler.Form.IsDisposed)
				{
					if (ContentContains(content, dockHandler.ActiveWindowHandle) && !Win32Helper.IsRunningOnMono)
					{
						NativeMethods.SetFocus(dockHandler.ActiveWindowHandle);
					}
					if (!dockHandler.Form.ContainsFocus && !dockHandler.Form.SelectNextControl(dockHandler.Form.ActiveControl, forward: true, tabStopOnly: true, nested: true, wrap: true) && !Win32Helper.IsRunningOnMono)
					{
						NativeMethods.SetFocus(dockHandler.Form.Handle);
					}
				}
			}
		}

		public void AddToList(IDockContent content)
		{
			if (!ListContent.Contains(content) && !IsInActiveList(content))
			{
				ListContent.Add(content);
			}
		}

		public void RemoveFromList(IDockContent content)
		{
			if (IsInActiveList(content))
			{
				RemoveFromActiveList(content);
			}
			if (ListContent.Contains(content))
			{
				ListContent.Remove(content);
			}
		}

		private bool IsInActiveList(IDockContent content)
		{
			if (content.DockHandler.NextActive == null)
			{
				return LastActiveContent == content;
			}
			return true;
		}

		private void AddLastToActiveList(IDockContent content)
		{
			IDockContent lastActiveContent = LastActiveContent;
			if (lastActiveContent != content)
			{
				DockContentHandler dockHandler = content.DockHandler;
				if (IsInActiveList(content))
				{
					RemoveFromActiveList(content);
				}
				dockHandler.PreviousActive = lastActiveContent;
				dockHandler.NextActive = null;
				LastActiveContent = content;
				if (lastActiveContent != null)
				{
					lastActiveContent.DockHandler.NextActive = LastActiveContent;
				}
			}
		}

		private void RemoveFromActiveList(IDockContent content)
		{
			if (LastActiveContent == content)
			{
				LastActiveContent = content.DockHandler.PreviousActive;
			}
			IDockContent previousActive = content.DockHandler.PreviousActive;
			IDockContent nextActive = content.DockHandler.NextActive;
			if (previousActive != null)
			{
				previousActive.DockHandler.NextActive = nextActive;
			}
			if (nextActive != null)
			{
				nextActive.DockHandler.PreviousActive = previousActive;
			}
			content.DockHandler.PreviousActive = null;
			content.DockHandler.NextActive = null;
		}

		public void GiveUpFocus(IDockContent content)
		{
			DockContentHandler dockHandler = content.DockHandler;
			if (!dockHandler.Form.ContainsFocus)
			{
				return;
			}
			if (IsFocusTrackingSuspended)
			{
				DockPanel.DummyControl.Focus();
			}
			if (LastActiveContent == content)
			{
				IDockContent previousActive = dockHandler.PreviousActive;
				if (previousActive != null)
				{
					Activate(previousActive);
				}
				else if (ListContent.Count > 0)
				{
					Activate(ListContent[ListContent.Count - 1]);
				}
			}
			else if (LastActiveContent != null)
			{
				Activate(LastActiveContent);
			}
			else if (ListContent.Count > 0)
			{
				Activate(ListContent[ListContent.Count - 1]);
			}
		}

		private static bool ContentContains(IDockContent content, IntPtr hWnd)
		{
			for (Control control = Control.FromChildHandle(hWnd); control != null; control = control.Parent)
			{
				if (control == content.DockHandler.Form)
				{
					return true;
				}
			}
			return false;
		}

		public void SuspendFocusTracking()
		{
			if (!m_disposed && m_countSuspendFocusTracking++ == 0 && !Win32Helper.IsRunningOnMono)
			{
				sm_localWindowsHook.HookInvoked -= m_hookEventHandler;
			}
		}

		public void ResumeFocusTracking()
		{
			if (!m_disposed && m_countSuspendFocusTracking != 0 && --m_countSuspendFocusTracking == 0)
			{
				if (ContentActivating != null)
				{
					Activate(ContentActivating);
					ContentActivating = null;
				}
				if (!Win32Helper.IsRunningOnMono)
				{
					sm_localWindowsHook.HookInvoked += m_hookEventHandler;
				}
				if (!InRefreshActiveWindow)
				{
					RefreshActiveWindow();
				}
			}
		}

		private void HookEventHandler(object sender, HookEventArgs e)
		{
			switch ((Msgs)Marshal.ReadInt32(e.lParam, IntPtr.Size * 3))
			{
			case Msgs.WM_KILLFOCUS:
			{
				IntPtr hWnd = Marshal.ReadIntPtr(e.lParam, IntPtr.Size * 2);
				if (GetPaneFromHandle(hWnd) == null)
				{
					RefreshActiveWindow();
				}
				break;
			}
			case Msgs.WM_SETFOCUS:
			case Msgs.WM_MDIACTIVATE:
				RefreshActiveWindow();
				break;
			}
		}

		private DockPane GetPaneFromHandle(IntPtr hWnd)
		{
			Control control = Control.FromChildHandle(hWnd);
			IDockContent dockContent = null;
			DockPane dockPane = null;
			while (control != null)
			{
				dockContent = control as IDockContent;
				if (dockContent != null)
				{
					dockContent.DockHandler.ActiveWindowHandle = hWnd;
				}
				if (dockContent != null && dockContent.DockHandler.DockPanel == DockPanel)
				{
					return dockContent.DockHandler.Pane;
				}
				dockPane = control as DockPane;
				if (dockPane != null && dockPane.DockPanel == DockPanel)
				{
					break;
				}
				control = control.Parent;
			}
			return dockPane;
		}

		private void RefreshActiveWindow()
		{
			SuspendFocusTracking();
			m_inRefreshActiveWindow = true;
			DockPane activePane = ActivePane;
			IDockContent activeContent = ActiveContent;
			IDockContent activeDocument = ActiveDocument;
			SetActivePane();
			SetActiveContent();
			SetActiveDocumentPane();
			SetActiveDocument();
			DockPanel.AutoHideWindow.RefreshActivePane();
			ResumeFocusTracking();
			m_inRefreshActiveWindow = false;
			if (activeContent != ActiveContent)
			{
				DockPanel.OnActiveContentChanged(EventArgs.Empty);
			}
			if (activeDocument != ActiveDocument)
			{
				DockPanel.OnActiveDocumentChanged(EventArgs.Empty);
			}
			if (activePane != ActivePane)
			{
				DockPanel.OnActivePaneChanged(EventArgs.Empty);
			}
		}

		private void SetActivePane()
		{
			DockPane dockPane = (Win32Helper.IsRunningOnMono ? null : GetPaneFromHandle(NativeMethods.GetFocus()));
			if (m_activePane != dockPane)
			{
				if (m_activePane != null)
				{
					m_activePane.SetIsActivated(value: false);
				}
				m_activePane = dockPane;
				if (m_activePane != null)
				{
					m_activePane.SetIsActivated(value: true);
				}
			}
		}

		internal void SetActiveContent()
		{
			IDockContent dockContent = ((ActivePane == null) ? null : ActivePane.ActiveContent);
			if (m_activeContent == dockContent)
			{
				return;
			}
			if (m_activeContent != null)
			{
				m_activeContent.DockHandler.IsActivated = false;
			}
			m_activeContent = dockContent;
			if (m_activeContent != null)
			{
				m_activeContent.DockHandler.IsActivated = true;
				if (!DockHelper.IsDockStateAutoHide(m_activeContent.DockHandler.DockState))
				{
					AddLastToActiveList(m_activeContent);
				}
			}
		}

		private void SetActiveDocumentPane()
		{
			DockPane dockPane = null;
			if (ActivePane != null && ActivePane.DockState == DockState.Document)
			{
				dockPane = ActivePane;
			}
			if (dockPane == null && DockPanel.DockWindows != null)
			{
				dockPane = ((ActiveDocumentPane == null) ? DockPanel.DockWindows[DockState.Document].DefaultPane : ((ActiveDocumentPane.DockPanel == DockPanel && ActiveDocumentPane.DockState == DockState.Document) ? ActiveDocumentPane : DockPanel.DockWindows[DockState.Document].DefaultPane));
			}
			if (m_activeDocumentPane != dockPane)
			{
				if (m_activeDocumentPane != null)
				{
					m_activeDocumentPane.SetIsActiveDocumentPane(value: false);
				}
				m_activeDocumentPane = dockPane;
				if (m_activeDocumentPane != null)
				{
					m_activeDocumentPane.SetIsActiveDocumentPane(value: true);
				}
			}
		}

		private void SetActiveDocument()
		{
			IDockContent dockContent = ((ActiveDocumentPane == null) ? null : ActiveDocumentPane.ActiveContent);
			if (m_activeDocument != dockContent)
			{
				m_activeDocument = dockContent;
			}
		}
	}

	private SplitterDragHandler m_splitterDragHandler;

	private MdiClientController m_mdiClientController;

	private DockDragHandler m_dockDragHandler;

	private readonly FocusManagerImpl m_focusManager;

	private readonly DockPaneCollection m_panes;

	private readonly FloatWindowCollection m_floatWindows;

	private AutoHideWindowControl m_autoHideWindow;

	private DockWindowCollection m_dockWindows;

	private readonly DockContent m_dummyContent;

	private readonly Control m_dummyControl;

	private Color m_BackColor;

	private AutoHideStripBase m_autoHideStripControl;

	private bool m_disposed;

	private bool m_allowEndUserDocking = !Win32Helper.IsRunningOnMono;

	private bool m_allowEndUserNestedDocking = !Win32Helper.IsRunningOnMono;

	private DockContentCollection m_contents = new DockContentCollection();

	private bool m_rightToLeftLayout;

	private bool m_showDocumentIcon;

	private bool m_largeDocumentIcon;

	private double m_dockBottomPortion = 0.25;

	private double m_dockLeftPortion = 0.25;

	private double m_dockRightPortion = 0.25;

	private double m_dockTopPortion = 0.25;

	private DocumentStyle m_documentStyle;

	private PaintEventHandler m_dummyControlPaintEventHandler;

	private Rectangle[] m_clipRects;

	private static readonly object ActiveAutoHideContentChangedEvent;

	private static readonly object ContentAddedEvent;

	private static readonly object ContentRemovedEvent;

	private static readonly object ActiveDocumentChangedEvent;

	private static readonly object ActiveContentChangedEvent;

	private static readonly object DocumentDraggedEvent;

	private static readonly object ActivePaneChangedEvent;

	private ThemeBase m_dockPanelTheme = new VS2005Theme();

	private bool MdiClientExists => GetMdiClientController().MdiClient != null;

	private AutoHideWindowControl AutoHideWindow => m_autoHideWindow;

	internal Control AutoHideControl => m_autoHideWindow;

	internal Rectangle AutoHideWindowRectangle
	{
		get
		{
			DockState dockState = AutoHideWindow.DockState;
			Rectangle dockArea = DockArea;
			if (ActiveAutoHideContent == null)
			{
				return Rectangle.Empty;
			}
			if (base.Parent == null)
			{
				return Rectangle.Empty;
			}
			Rectangle empty = Rectangle.Empty;
			double num = ActiveAutoHideContent.DockHandler.AutoHidePortion;
			switch (dockState)
			{
			case DockState.DockLeftAutoHide:
				if (num < 1.0)
				{
					num = (double)dockArea.Width * num;
				}
				if (num > (double)(dockArea.Width - 24))
				{
					num = dockArea.Width - 24;
				}
				empty.X = dockArea.X - Theme.Measures.DockPadding;
				empty.Y = dockArea.Y;
				empty.Width = (int)num;
				empty.Height = dockArea.Height;
				break;
			case DockState.DockRightAutoHide:
				if (num < 1.0)
				{
					num = (double)dockArea.Width * num;
				}
				if (num > (double)(dockArea.Width - 24))
				{
					num = dockArea.Width - 24;
				}
				empty.X = dockArea.X + dockArea.Width - (int)num + Theme.Measures.DockPadding;
				empty.Y = dockArea.Y;
				empty.Width = (int)num;
				empty.Height = dockArea.Height;
				break;
			case DockState.DockTopAutoHide:
				if (num < 1.0)
				{
					num = (double)dockArea.Height * num;
				}
				if (num > (double)(dockArea.Height - 24))
				{
					num = dockArea.Height - 24;
				}
				empty.X = dockArea.X;
				empty.Y = dockArea.Y - Theme.Measures.DockPadding;
				empty.Width = dockArea.Width;
				empty.Height = (int)num;
				break;
			case DockState.DockBottomAutoHide:
				if (num < 1.0)
				{
					num = (double)dockArea.Height * num;
				}
				if (num > (double)(dockArea.Height - 24))
				{
					num = dockArea.Height - 24;
				}
				empty.X = dockArea.X;
				empty.Y = dockArea.Y + dockArea.Height - (int)num + Theme.Measures.DockPadding;
				empty.Width = dockArea.Width;
				empty.Height = (int)num;
				break;
			}
			return empty;
		}
	}

	[Description("Determines the color with which the client rectangle will be drawn.\r\nIf this property is used instead of the BackColor it will not have any influence on the borders to the surrounding controls (DockPane).\r\nThe BackColor property changes the borders of surrounding controls (DockPane).\r\nAlternatively both properties may be used (BackColor to draw and define the color of the borders and DockBackColor to define the color of the client rectangle).\r\nFor Backgroundimages: Set your prefered Image, then set the DockBackColor and the BackColor to the same Color (Control).")]
	public Color DockBackColor
	{
		get
		{
			if (m_BackColor.IsEmpty)
			{
				return base.BackColor;
			}
			return m_BackColor;
		}
		set
		{
			if (m_BackColor != value)
			{
				m_BackColor = value;
				Refresh();
			}
		}
	}

	internal AutoHideStripBase AutoHideStripControl
	{
		get
		{
			if (m_autoHideStripControl == null)
			{
				m_autoHideStripControl = Theme.Extender.AutoHideStripFactory.CreateAutoHideStrip(this);
				base.Controls.Add(m_autoHideStripControl);
			}
			return m_autoHideStripControl;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IDockContent ActiveAutoHideContent
	{
		get
		{
			return AutoHideWindow.ActiveContent;
		}
		set
		{
			AutoHideWindow.ActiveContent = value;
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_AllowEndUserDocking_Description")]
	[DefaultValue(true)]
	public bool AllowEndUserDocking
	{
		get
		{
			if (Win32Helper.IsRunningOnMono && m_allowEndUserDocking)
			{
				m_allowEndUserDocking = false;
			}
			return m_allowEndUserDocking;
		}
		set
		{
			if (Win32Helper.IsRunningOnMono && value)
			{
				throw new InvalidOperationException("AllowEndUserDocking can only be false if running on Mono");
			}
			m_allowEndUserDocking = value;
		}
	}

	[DefaultValue(false)]
	public bool AllowDoubleClickFloating { get; set; }

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_AllowEndUserNestedDocking_Description")]
	[DefaultValue(true)]
	public bool AllowEndUserNestedDocking
	{
		get
		{
			if (Win32Helper.IsRunningOnMono && m_allowEndUserDocking)
			{
				m_allowEndUserDocking = false;
			}
			return m_allowEndUserNestedDocking;
		}
		set
		{
			if (Win32Helper.IsRunningOnMono && value)
			{
				throw new InvalidOperationException("AllowEndUserNestedDocking can only be false if running on Mono");
			}
			m_allowEndUserNestedDocking = value;
		}
	}

	[Browsable(false)]
	public DockContentCollection Contents => m_contents;

	internal DockContent DummyContent => m_dummyContent;

	[DefaultValue(false)]
	[LocalizedCategory("Appearance")]
	[LocalizedDescription("DockPanel_RightToLeftLayout_Description")]
	public bool RightToLeftLayout
	{
		get
		{
			return m_rightToLeftLayout;
		}
		set
		{
			if (m_rightToLeftLayout == value)
			{
				return;
			}
			m_rightToLeftLayout = value;
			foreach (FloatWindow floatWindow in FloatWindows)
			{
				floatWindow.RightToLeftLayout = value;
			}
		}
	}

	[DefaultValue(false)]
	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_ShowDocumentIcon_Description")]
	public bool ShowDocumentIcon
	{
		get
		{
			return m_showDocumentIcon;
		}
		set
		{
			if (m_showDocumentIcon != value)
			{
				m_showDocumentIcon = value;
				Refresh();
			}
		}
	}

	[DefaultValue(false)]
	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_LargeDocumentIcon_Description")]
	public bool LargeDocumentIcon
	{
		get
		{
			return m_largeDocumentIcon;
		}
		set
		{
			if (m_largeDocumentIcon != value)
			{
				m_largeDocumentIcon = value;
				Refresh();
			}
		}
	}

	[DefaultValue(DocumentTabStripLocation.Top)]
	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_DocumentTabStripLocation")]
	public DocumentTabStripLocation DocumentTabStripLocation { get; set; }

	[Browsable(false)]
	[Obsolete("Use Theme.Extender instead.")]
	public DockPanelExtender Extender => null;

	[Browsable(false)]
	[Obsolete("Use Theme.Extender instead.")]
	public DockPanelExtender.IDockPaneFactory DockPaneFactory => null;

	[Browsable(false)]
	[Obsolete("Use Theme.Extender instead.")]
	public DockPanelExtender.IFloatWindowFactory FloatWindowFactory => null;

	[Browsable(false)]
	[Obsolete("Use Theme.Extender instead.")]
	public DockPanelExtender.IDockWindowFactory DockWindowFactory => null;

	[Browsable(false)]
	public DockPaneCollection Panes => m_panes;

	public Rectangle DockArea => new Rectangle(base.DockPadding.Left, base.DockPadding.Top, base.ClientRectangle.Width - base.DockPadding.Left - base.DockPadding.Right, base.ClientRectangle.Height - base.DockPadding.Top - base.DockPadding.Bottom);

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_DockBottomPortion_Description")]
	[DefaultValue(0.25)]
	public double DockBottomPortion
	{
		get
		{
			return m_dockBottomPortion;
		}
		set
		{
			if (value <= 0.0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (!(Math.Abs(value - m_dockBottomPortion) < double.Epsilon))
			{
				m_dockBottomPortion = value;
				if (m_dockBottomPortion < 1.0 && m_dockTopPortion < 1.0 && m_dockTopPortion + m_dockBottomPortion > 1.0)
				{
					m_dockTopPortion = 1.0 - m_dockBottomPortion;
				}
				PerformLayout();
			}
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_DockLeftPortion_Description")]
	[DefaultValue(0.25)]
	public double DockLeftPortion
	{
		get
		{
			return m_dockLeftPortion;
		}
		set
		{
			if (value <= 0.0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (!(Math.Abs(value - m_dockLeftPortion) < double.Epsilon))
			{
				m_dockLeftPortion = value;
				if (m_dockLeftPortion < 1.0 && m_dockRightPortion < 1.0 && m_dockLeftPortion + m_dockRightPortion > 1.0)
				{
					m_dockRightPortion = 1.0 - m_dockLeftPortion;
				}
				PerformLayout();
			}
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_DockRightPortion_Description")]
	[DefaultValue(0.25)]
	public double DockRightPortion
	{
		get
		{
			return m_dockRightPortion;
		}
		set
		{
			if (value <= 0.0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (!(Math.Abs(value - m_dockRightPortion) < double.Epsilon))
			{
				m_dockRightPortion = value;
				if (m_dockLeftPortion < 1.0 && m_dockRightPortion < 1.0 && m_dockLeftPortion + m_dockRightPortion > 1.0)
				{
					m_dockLeftPortion = 1.0 - m_dockRightPortion;
				}
				PerformLayout();
			}
		}
	}

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_DockTopPortion_Description")]
	[DefaultValue(0.25)]
	public double DockTopPortion
	{
		get
		{
			return m_dockTopPortion;
		}
		set
		{
			if (value <= 0.0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			if (!(Math.Abs(value - m_dockTopPortion) < double.Epsilon))
			{
				m_dockTopPortion = value;
				if (m_dockTopPortion < 1.0 && m_dockBottomPortion < 1.0 && m_dockTopPortion + m_dockBottomPortion > 1.0)
				{
					m_dockBottomPortion = 1.0 - m_dockTopPortion;
				}
				PerformLayout();
			}
		}
	}

	[Browsable(false)]
	public DockWindowCollection DockWindows => m_dockWindows;

	[Browsable(false)]
	public int DocumentsCount
	{
		get
		{
			int num = 0;
			foreach (IDockContent document in Documents)
			{
				_ = document;
				num++;
			}
			return num;
		}
	}

	[Browsable(false)]
	public IEnumerable<IDockContent> Documents
	{
		get
		{
			foreach (IDockContent content in Contents)
			{
				if (content.DockHandler.DockState == DockState.Document)
				{
					yield return content;
				}
			}
		}
	}

	private Control DummyControl => m_dummyControl;

	[Browsable(false)]
	public FloatWindowCollection FloatWindows => m_floatWindows;

	[Category("Layout")]
	[LocalizedDescription("DockPanel_DefaultFloatWindowSize_Description")]
	public Size DefaultFloatWindowSize { get; set; } = new Size(300, 300);

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_DocumentStyle_Description")]
	[DefaultValue(DocumentStyle.DockingMdi)]
	public DocumentStyle DocumentStyle
	{
		get
		{
			return m_documentStyle;
		}
		set
		{
			if (value == m_documentStyle)
			{
				return;
			}
			if (!Enum.IsDefined(typeof(DocumentStyle), value))
			{
				throw new InvalidEnumArgumentException();
			}
			if (value == DocumentStyle.SystemMdi && DockWindows[DockState.Document].VisibleNestedPanes.Count > 0)
			{
				throw new InvalidEnumArgumentException();
			}
			m_documentStyle = value;
			SuspendLayout(allWindows: true);
			SetAutoHideWindowParent();
			SetMdiClient();
			InvalidateWindowRegion();
			foreach (IDockContent content in Contents)
			{
				if (content.DockHandler.DockState == DockState.Document)
				{
					content.DockHandler.SetPaneAndVisible(content.DockHandler.Pane);
				}
			}
			PerformMdiClientLayout();
			ResumeLayout(performLayout: true, allWindows: true);
		}
	}

	[LocalizedCategory("Category_Performance")]
	[LocalizedDescription("DockPanel_SupportDeeplyNestedContent_Description")]
	[DefaultValue(false)]
	public bool SupportDeeplyNestedContent { get; set; }

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_ShowAutoHideContentOnHover_Description")]
	[DefaultValue(true)]
	public bool ShowAutoHideContentOnHover { get; set; }

	internal Form ParentForm
	{
		get
		{
			if (!IsParentFormValid())
			{
				throw new InvalidOperationException(Strings.DockPanel_ParentForm_Invalid);
			}
			return GetMdiClientController().ParentForm;
		}
	}

	private Rectangle SystemMdiClientBounds
	{
		get
		{
			if (!IsParentFormValid() || !base.Visible)
			{
				return Rectangle.Empty;
			}
			return ParentForm.RectangleToClient(RectangleToScreen(DocumentWindowBounds));
		}
	}

	public Rectangle DocumentWindowBounds
	{
		get
		{
			Rectangle displayRectangle = DisplayRectangle;
			if (DockWindows[DockState.DockLeft].Visible)
			{
				displayRectangle.X += DockWindows[DockState.DockLeft].Width;
				displayRectangle.Width -= DockWindows[DockState.DockLeft].Width;
			}
			if (DockWindows[DockState.DockRight].Visible)
			{
				displayRectangle.Width -= DockWindows[DockState.DockRight].Width;
			}
			if (DockWindows[DockState.DockTop].Visible)
			{
				displayRectangle.Y += DockWindows[DockState.DockTop].Height;
				displayRectangle.Height -= DockWindows[DockState.DockTop].Height;
			}
			if (DockWindows[DockState.DockBottom].Visible)
			{
				displayRectangle.Height -= DockWindows[DockState.DockBottom].Height;
			}
			return displayRectangle;
		}
	}

	private IFocusManager FocusManager => m_focusManager;

	internal IContentFocusManager ContentFocusManager => m_focusManager;

	[Browsable(false)]
	public IDockContent ActiveContent => FocusManager.ActiveContent;

	[Browsable(false)]
	public DockPane ActivePane => FocusManager.ActivePane;

	[Browsable(false)]
	public IDockContent ActiveDocument => FocusManager.ActiveDocument;

	[Browsable(false)]
	public DockPane ActiveDocumentPane => FocusManager.ActiveDocumentPane;

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_DockPanelSkin")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	[Obsolete("Use Theme.Skin instead.")]
	public DockPanelSkin Skin => null;

	[LocalizedCategory("Category_Docking")]
	[LocalizedDescription("DockPanel_DockPanelTheme")]
	public ThemeBase Theme
	{
		get
		{
			return m_dockPanelTheme;
		}
		set
		{
			if (value != null && !(m_dockPanelTheme.GetType() == value.GetType()))
			{
				m_dockPanelTheme?.CleanUp(this);
				m_dockPanelTheme = value;
				m_dockPanelTheme.ApplyTo(this);
				m_dockPanelTheme.PostApply(this);
			}
		}
	}

	[LocalizedCategory("Category_DockingNotification")]
	[LocalizedDescription("DockPanel_ActiveAutoHideContentChanged_Description")]
	public event EventHandler ActiveAutoHideContentChanged
	{
		add
		{
			base.Events.AddHandler(ActiveAutoHideContentChangedEvent, value);
		}
		remove
		{
			base.Events.RemoveHandler(ActiveAutoHideContentChangedEvent, value);
		}
	}

	[LocalizedCategory("Category_DockingNotification")]
	[LocalizedDescription("DockPanel_ContentAdded_Description")]
	public event EventHandler<DockContentEventArgs> ContentAdded
	{
		add
		{
			base.Events.AddHandler(ContentAddedEvent, value);
		}
		remove
		{
			base.Events.RemoveHandler(ContentAddedEvent, value);
		}
	}

	[LocalizedCategory("Category_DockingNotification")]
	[LocalizedDescription("DockPanel_ContentRemoved_Description")]
	public event EventHandler<DockContentEventArgs> ContentRemoved
	{
		add
		{
			base.Events.AddHandler(ContentRemovedEvent, value);
		}
		remove
		{
			base.Events.RemoveHandler(ContentRemovedEvent, value);
		}
	}

	[LocalizedCategory("Category_PropertyChanged")]
	[LocalizedDescription("DockPanel_ActiveDocumentChanged_Description")]
	public event EventHandler ActiveDocumentChanged
	{
		add
		{
			base.Events.AddHandler(ActiveDocumentChangedEvent, value);
		}
		remove
		{
			base.Events.RemoveHandler(ActiveDocumentChangedEvent, value);
		}
	}

	[LocalizedCategory("Category_PropertyChanged")]
	[LocalizedDescription("DockPanel_ActiveContentChanged_Description")]
	public event EventHandler ActiveContentChanged
	{
		add
		{
			base.Events.AddHandler(ActiveContentChangedEvent, value);
		}
		remove
		{
			base.Events.RemoveHandler(ActiveContentChangedEvent, value);
		}
	}

	[LocalizedCategory("Category_PropertyChanged")]
	[LocalizedDescription("DockPanel_ActiveContentChanged_Description")]
	public event EventHandler DocumentDragged
	{
		add
		{
			base.Events.AddHandler(DocumentDraggedEvent, value);
		}
		remove
		{
			base.Events.RemoveHandler(DocumentDraggedEvent, value);
		}
	}

	[LocalizedCategory("Category_PropertyChanged")]
	[LocalizedDescription("DockPanel_ActivePaneChanged_Description")]
	public event EventHandler ActivePaneChanged
	{
		add
		{
			base.Events.AddHandler(ActivePaneChangedEvent, value);
		}
		remove
		{
			base.Events.RemoveHandler(ActivePaneChangedEvent, value);
		}
	}

	private SplitterDragHandler GetSplitterDragHandler()
	{
		if (m_splitterDragHandler == null)
		{
			m_splitterDragHandler = new SplitterDragHandler(this);
		}
		return m_splitterDragHandler;
	}

	public void BeginDrag(ISplitterDragSource dragSource, Rectangle rectSplitter)
	{
		GetSplitterDragHandler().BeginDrag(dragSource, rectSplitter);
	}

	private MdiClientController GetMdiClientController()
	{
		if (m_mdiClientController == null)
		{
			m_mdiClientController = new MdiClientController();
			m_mdiClientController.HandleAssigned += MdiClientHandleAssigned;
			m_mdiClientController.MdiChildActivate += ParentFormMdiChildActivate;
			m_mdiClientController.Layout += MdiClient_Layout;
		}
		return m_mdiClientController;
	}

	private void ParentFormMdiChildActivate(object sender, EventArgs e)
	{
		if (GetMdiClientController().ParentForm != null && GetMdiClientController().ParentForm.ActiveMdiChild is IDockContent dockContent && dockContent.DockHandler.DockPanel == this && dockContent.DockHandler.Pane != null)
		{
			dockContent.DockHandler.Pane.ActiveContent = dockContent;
		}
	}

	private void SetMdiClientBounds(Rectangle bounds)
	{
		GetMdiClientController().MdiClient.Bounds = bounds;
	}

	private void SuspendMdiClientLayout()
	{
		if (GetMdiClientController().MdiClient != null)
		{
			GetMdiClientController().MdiClient.SuspendLayout();
		}
	}

	private void ResumeMdiClientLayout(bool perform)
	{
		if (GetMdiClientController().MdiClient != null)
		{
			GetMdiClientController().MdiClient.ResumeLayout(perform);
		}
	}

	private void PerformMdiClientLayout()
	{
		if (GetMdiClientController().MdiClient != null)
		{
			GetMdiClientController().MdiClient.PerformLayout();
		}
	}

	private void SetMdiClient()
	{
		MdiClientController mdiClientController = GetMdiClientController();
		if (DocumentStyle == DocumentStyle.DockingMdi)
		{
			mdiClientController.AutoScroll = false;
			mdiClientController.BorderStyle = BorderStyle.None;
			if (MdiClientExists)
			{
				mdiClientController.MdiClient.Dock = DockStyle.Fill;
			}
		}
		else if (DocumentStyle == DocumentStyle.DockingSdi || DocumentStyle == DocumentStyle.DockingWindow)
		{
			mdiClientController.AutoScroll = true;
			mdiClientController.BorderStyle = BorderStyle.Fixed3D;
			if (MdiClientExists)
			{
				mdiClientController.MdiClient.Dock = DockStyle.Fill;
			}
		}
		else if (DocumentStyle == DocumentStyle.SystemMdi)
		{
			mdiClientController.AutoScroll = true;
			mdiClientController.BorderStyle = BorderStyle.Fixed3D;
			if (mdiClientController.MdiClient != null)
			{
				mdiClientController.MdiClient.Dock = DockStyle.None;
				mdiClientController.MdiClient.Bounds = SystemMdiClientBounds;
			}
		}
	}

	internal Rectangle RectangleToMdiClient(Rectangle rect)
	{
		if (MdiClientExists)
		{
			return GetMdiClientController().MdiClient.RectangleToClient(rect);
		}
		return Rectangle.Empty;
	}

	internal void RefreshActiveAutoHideContent()
	{
		AutoHideWindow.RefreshActiveContent();
	}

	internal Rectangle GetAutoHideWindowBounds(Rectangle rectAutoHideWindow)
	{
		if (DocumentStyle == DocumentStyle.SystemMdi || DocumentStyle == DocumentStyle.DockingMdi)
		{
			if (base.Parent != null)
			{
				return base.Parent.RectangleToClient(RectangleToScreen(rectAutoHideWindow));
			}
			return Rectangle.Empty;
		}
		return rectAutoHideWindow;
	}

	internal void RefreshAutoHideStrip()
	{
		AutoHideStripControl.RefreshChanges();
	}

	private DockDragHandler GetDockDragHandler()
	{
		if (m_dockDragHandler == null)
		{
			m_dockDragHandler = new DockDragHandler(this);
		}
		return m_dockDragHandler;
	}

	internal void BeginDrag(IDockDragSource dragSource)
	{
		GetDockDragHandler().BeginDrag(dragSource);
	}

	public DockPanel()
	{
		ShowAutoHideContentOnHover = true;
		m_focusManager = new FocusManagerImpl(this);
		m_panes = new DockPaneCollection();
		m_floatWindows = new FloatWindowCollection();
		SuspendLayout();
		m_dummyControl = new DummyControl();
		m_dummyControl.Bounds = new Rectangle(0, 0, 1, 1);
		base.Controls.Add(m_dummyControl);
		Theme.ApplyTo(this);
		m_autoHideWindow = Theme.Extender.AutoHideWindowFactory.CreateAutoHideWindow(this);
		m_autoHideWindow.Visible = false;
		m_autoHideWindow.ActiveContentChanged += m_autoHideWindow_ActiveContentChanged;
		SetAutoHideWindowParent();
		LoadDockWindows();
		m_dummyContent = new DockContent();
		ResumeLayout();
	}

	internal void ResetDummy()
	{
		DummyControl.ResetBackColor();
	}

	internal void SetDummy()
	{
		DummyControl.BackColor = DockBackColor;
	}

	private bool ShouldSerializeDockBackColor()
	{
		return !m_BackColor.IsEmpty;
	}

	internal void ResetAutoHideStripControl()
	{
		if (m_autoHideStripControl != null)
		{
			m_autoHideStripControl.Dispose();
		}
		m_autoHideStripControl = null;
	}

	private void MdiClientHandleAssigned(object sender, EventArgs e)
	{
		SetMdiClient();
		PerformLayout();
	}

	private void MdiClient_Layout(object sender, LayoutEventArgs e)
	{
		if (DocumentStyle != DocumentStyle.DockingMdi)
		{
			return;
		}
		foreach (DockPane pane in Panes)
		{
			if (pane.DockState == DockState.Document)
			{
				pane.SetContentBounds();
			}
		}
		InvalidateWindowRegion();
	}

	protected override void Dispose(bool disposing)
	{
		if (!m_disposed && disposing)
		{
			m_focusManager.Dispose();
			if (m_mdiClientController != null)
			{
				m_mdiClientController.HandleAssigned -= MdiClientHandleAssigned;
				m_mdiClientController.MdiChildActivate -= ParentFormMdiChildActivate;
				m_mdiClientController.Layout -= MdiClient_Layout;
				m_mdiClientController.Dispose();
			}
			FloatWindows.Dispose();
			Panes.Dispose();
			DummyContent.Dispose();
			m_disposed = true;
		}
		base.Dispose(disposing);
	}

	protected override void OnRightToLeftChanged(EventArgs e)
	{
		base.OnRightToLeftChanged(e);
		foreach (FloatWindow floatWindow in FloatWindows)
		{
			floatWindow.RightToLeft = RightToLeft;
		}
	}

	public void UpdateDockWindowZOrder(DockStyle dockStyle, bool fullPanelEdge)
	{
		switch (dockStyle)
		{
		case DockStyle.Left:
			if (fullPanelEdge)
			{
				DockWindows[DockState.DockLeft].SendToBack();
			}
			else
			{
				DockWindows[DockState.DockLeft].BringToFront();
			}
			break;
		case DockStyle.Right:
			if (fullPanelEdge)
			{
				DockWindows[DockState.DockRight].SendToBack();
			}
			else
			{
				DockWindows[DockState.DockRight].BringToFront();
			}
			break;
		case DockStyle.Top:
			if (fullPanelEdge)
			{
				DockWindows[DockState.DockTop].SendToBack();
			}
			else
			{
				DockWindows[DockState.DockTop].BringToFront();
			}
			break;
		case DockStyle.Bottom:
			if (fullPanelEdge)
			{
				DockWindows[DockState.DockBottom].SendToBack();
			}
			else
			{
				DockWindows[DockState.DockBottom].BringToFront();
			}
			break;
		}
	}

	public IDockContent[] DocumentsToArray()
	{
		IDockContent[] array = new IDockContent[DocumentsCount];
		int num = 0;
		foreach (IDockContent document in Documents)
		{
			array[num] = document;
			num++;
		}
		return array;
	}

	private bool ShouldSerializeDefaultFloatWindowSize()
	{
		return DefaultFloatWindowSize != new Size(300, 300);
	}

	private void ResetDefaultFloatWindowSize()
	{
		DefaultFloatWindowSize = new Size(300, 300);
	}

	public int GetDockWindowSize(DockState dockState)
	{
		switch (dockState)
		{
		case DockState.DockLeft:
		case DockState.DockRight:
		{
			int num5 = base.ClientRectangle.Width - base.DockPadding.Left - base.DockPadding.Right;
			int num6 = ((m_dockLeftPortion >= 1.0) ? ((int)m_dockLeftPortion) : ((int)((double)num5 * m_dockLeftPortion)));
			int num7 = ((m_dockRightPortion >= 1.0) ? ((int)m_dockRightPortion) : ((int)((double)num5 * m_dockRightPortion)));
			if (num6 < 24)
			{
				num6 = 24;
			}
			if (num7 < 24)
			{
				num7 = 24;
			}
			if (num6 + num7 > num5 - 24)
			{
				int num8 = num6 + num7 - (num5 - 24);
				num6 -= num8 / 2;
				num7 -= num8 / 2;
			}
			if (dockState != DockState.DockLeft)
			{
				return num7;
			}
			return num6;
		}
		case DockState.DockTop:
		case DockState.DockBottom:
		{
			int num = base.ClientRectangle.Height - base.DockPadding.Top - base.DockPadding.Bottom;
			int num2 = ((m_dockTopPortion >= 1.0) ? ((int)m_dockTopPortion) : ((int)((double)num * m_dockTopPortion)));
			int num3 = ((m_dockBottomPortion >= 1.0) ? ((int)m_dockBottomPortion) : ((int)((double)num * m_dockBottomPortion)));
			if (num2 < 24)
			{
				num2 = 24;
			}
			if (num3 < 24)
			{
				num3 = 24;
			}
			if (num2 + num3 > num - 24)
			{
				int num4 = num2 + num3 - (num - 24);
				num2 -= num4 / 2;
				num3 -= num4 / 2;
			}
			if (dockState != DockState.DockTop)
			{
				return num3;
			}
			return num2;
		}
		default:
			return 0;
		}
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		SuspendLayout(allWindows: true);
		AutoHideStripControl.Bounds = base.ClientRectangle;
		CalculateDockPadding();
		DockWindows[DockState.DockLeft].Width = GetDockWindowSize(DockState.DockLeft);
		DockWindows[DockState.DockRight].Width = GetDockWindowSize(DockState.DockRight);
		DockWindows[DockState.DockTop].Height = GetDockWindowSize(DockState.DockTop);
		DockWindows[DockState.DockBottom].Height = GetDockWindowSize(DockState.DockBottom);
		AutoHideWindow.Bounds = GetAutoHideWindowBounds(AutoHideWindowRectangle);
		DockWindow dockWindow = DockWindows[DockState.Document];
		if (dockWindow.Parent == AutoHideWindow.Parent)
		{
			AutoHideWindow.Parent.Controls.SetChildIndex(AutoHideWindow, 0);
			dockWindow.Parent.Controls.SetChildIndex(dockWindow, 1);
		}
		else
		{
			dockWindow.BringToFront();
			AutoHideWindow.BringToFront();
		}
		base.OnLayout(levent);
		if (DocumentStyle == DocumentStyle.SystemMdi && MdiClientExists)
		{
			SetMdiClientBounds(SystemMdiClientBounds);
			InvalidateWindowRegion();
		}
		else if (DocumentStyle == DocumentStyle.DockingMdi)
		{
			InvalidateWindowRegion();
		}
		ResumeLayout(performLayout: true, allWindows: true);
	}

	internal Rectangle GetTabStripRectangle(DockState dockState)
	{
		return AutoHideStripControl.GetTabStripRectangle(dockState);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		var sw = System.Diagnostics.Stopwatch.StartNew();
		base.OnPaint(e);
		if (DockBackColor.ToArgb() != BackColor.ToArgb())
		{
			Graphics graphics = e.Graphics;
			SolidBrush brush = new SolidBrush(DockBackColor);
			graphics.FillRectangle(brush, base.ClientRectangle);
		}
		sw.Stop();
		if (sw.ElapsedMilliseconds > 1)
			System.Diagnostics.Trace.WriteLine($"[Paint] DockPanel.OnPaint: {sw.ElapsedMilliseconds}ms");
	}

	internal void AddContent(IDockContent content)
	{
		if (content == null)
		{
			throw new ArgumentNullException();
		}
		if (!Contents.Contains(content))
		{
			Contents.Add(content);
			OnContentAdded(new DockContentEventArgs(content));
		}
	}

	internal void AddPane(DockPane pane)
	{
		if (!Panes.Contains(pane))
		{
			Panes.Add(pane);
		}
	}

	internal void AddFloatWindow(FloatWindow floatWindow)
	{
		if (!FloatWindows.Contains(floatWindow))
		{
			FloatWindows.Add(floatWindow);
		}
	}

	private void CalculateDockPadding()
	{
		base.DockPadding.All = Theme.Measures.DockPadding;
		int num = AutoHideStripControl.MeasureHeight();
		if (AutoHideStripControl.GetNumberOfPanes(DockState.DockLeftAutoHide) > 0)
		{
			base.DockPadding.Left = num;
		}
		if (AutoHideStripControl.GetNumberOfPanes(DockState.DockRightAutoHide) > 0)
		{
			base.DockPadding.Right = num;
		}
		if (AutoHideStripControl.GetNumberOfPanes(DockState.DockTopAutoHide) > 0)
		{
			base.DockPadding.Top = num;
		}
		if (AutoHideStripControl.GetNumberOfPanes(DockState.DockBottomAutoHide) > 0)
		{
			base.DockPadding.Bottom = num;
		}
	}

	internal void RemoveContent(IDockContent content)
	{
		if (content == null)
		{
			throw new ArgumentNullException();
		}
		if (Contents.Contains(content))
		{
			Contents.Remove(content);
			OnContentRemoved(new DockContentEventArgs(content));
		}
	}

	internal void RemovePane(DockPane pane)
	{
		if (Panes.Contains(pane))
		{
			Panes.Remove(pane);
		}
	}

	internal void RemoveFloatWindow(FloatWindow floatWindow)
	{
		if (FloatWindows.Contains(floatWindow))
		{
			FloatWindows.Remove(floatWindow);
			if (FloatWindows.Count == 0 && ParentForm != null)
			{
				ParentForm.Focus();
			}
		}
	}

	public void SetPaneIndex(DockPane pane, int index)
	{
		int num = Panes.IndexOf(pane);
		if (num == -1)
		{
			throw new ArgumentException(Strings.DockPanel_SetPaneIndex_InvalidPane);
		}
		if ((index < 0 || index > Panes.Count - 1) && index != -1)
		{
			throw new ArgumentOutOfRangeException(Strings.DockPanel_SetPaneIndex_InvalidIndex);
		}
		if (num != index && (num != Panes.Count - 1 || index != -1))
		{
			Panes.Remove(pane);
			if (index == -1)
			{
				Panes.Add(pane);
			}
			else if (num < index)
			{
				Panes.AddAt(pane, index - 1);
			}
			else
			{
				Panes.AddAt(pane, index);
			}
		}
	}

	public void SuspendLayout(bool allWindows)
	{
		FocusManager.SuspendFocusTracking();
		SuspendLayout();
		if (allWindows)
		{
			SuspendMdiClientLayout();
		}
	}

	public void ResumeLayout(bool performLayout, bool allWindows)
	{
		FocusManager.ResumeFocusTracking();
		ResumeLayout(performLayout);
		if (allWindows)
		{
			ResumeMdiClientLayout(performLayout);
		}
	}

	private bool IsParentFormValid()
	{
		if (DocumentStyle == DocumentStyle.DockingSdi || DocumentStyle == DocumentStyle.DockingWindow)
		{
			return true;
		}
		if (!MdiClientExists)
		{
			GetMdiClientController().RenewMdiClient();
		}
		return MdiClientExists;
	}

	protected override void OnParentChanged(EventArgs e)
	{
		SetAutoHideWindowParent();
		GetMdiClientController().ParentForm = base.Parent as Form;
		base.OnParentChanged(e);
	}

	private void SetAutoHideWindowParent()
	{
		Control control = ((DocumentStyle != DocumentStyle.DockingMdi && DocumentStyle != DocumentStyle.SystemMdi) ? this : base.Parent);
		if (AutoHideWindow.Parent != control)
		{
			AutoHideWindow.Parent = control;
			AutoHideWindow.BringToFront();
		}
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (base.Visible)
		{
			SetMdiClient();
		}
	}

	private void InvalidateWindowRegion()
	{
		if (!base.DesignMode)
		{
			if (m_dummyControlPaintEventHandler == null)
			{
				m_dummyControlPaintEventHandler = DummyControl_Paint;
			}
			DummyControl.Paint += m_dummyControlPaintEventHandler;
			DummyControl.Invalidate();
		}
	}

	private void DummyControl_Paint(object sender, PaintEventArgs e)
	{
		DummyControl.Paint -= m_dummyControlPaintEventHandler;
		UpdateWindowRegion();
	}

	private void UpdateWindowRegion()
	{
		if (DocumentStyle == DocumentStyle.DockingMdi)
		{
			UpdateWindowRegion_ClipContent();
		}
		else if (DocumentStyle == DocumentStyle.DockingSdi || DocumentStyle == DocumentStyle.DockingWindow)
		{
			UpdateWindowRegion_FullDocumentArea();
		}
		else if (DocumentStyle == DocumentStyle.SystemMdi)
		{
			UpdateWindowRegion_EmptyDocumentArea();
		}
	}

	private void UpdateWindowRegion_FullDocumentArea()
	{
		SetRegion(null);
	}

	private void UpdateWindowRegion_EmptyDocumentArea()
	{
		Rectangle documentWindowBounds = DocumentWindowBounds;
		SetRegion(new Rectangle[1] { documentWindowBounds });
	}

	private void UpdateWindowRegion_ClipContent()
	{
		int num = 0;
		foreach (DockPane pane in Panes)
		{
			if (pane.Visible && pane.DockState == DockState.Document)
			{
				num++;
			}
		}
		if (num == 0)
		{
			SetRegion(null);
			return;
		}
		Rectangle[] array = new Rectangle[num];
		int num2 = 0;
		foreach (DockPane pane2 in Panes)
		{
			if (pane2.Visible && pane2.DockState == DockState.Document)
			{
				array[num2] = RectangleToClient(pane2.RectangleToScreen(pane2.ContentRectangle));
				num2++;
			}
		}
		SetRegion(array);
	}

	private void SetRegion(Rectangle[] clipRects)
	{
		if (!IsClipRectsChanged(clipRects))
		{
			return;
		}
		m_clipRects = clipRects;
		if (m_clipRects == null || m_clipRects.GetLength(0) == 0)
		{
			base.Region = null;
			return;
		}
		Region region = new Region(new Rectangle(0, 0, base.Width, base.Height));
		Rectangle[] clipRects2 = m_clipRects;
		foreach (Rectangle rect in clipRects2)
		{
			region.Exclude(rect);
		}
		if (base.Region != null)
		{
			base.Region.Dispose();
		}
		base.Region = region;
	}

	private bool IsClipRectsChanged(Rectangle[] clipRects)
	{
		if (clipRects == null && m_clipRects == null)
		{
			return false;
		}
		if (clipRects == null != (m_clipRects == null))
		{
			return true;
		}
		Rectangle[] array = clipRects;
		foreach (Rectangle rectangle in array)
		{
			bool flag = false;
			Rectangle[] clipRects2 = m_clipRects;
			foreach (Rectangle rectangle2 in clipRects2)
			{
				if (rectangle == rectangle2)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return true;
			}
		}
		array = m_clipRects;
		foreach (Rectangle rectangle3 in array)
		{
			bool flag2 = false;
			Rectangle[] clipRects2 = clipRects;
			for (int j = 0; j < clipRects2.Length; j++)
			{
				if (clipRects2[j] == rectangle3)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				return true;
			}
		}
		return false;
	}

	protected virtual void OnActiveAutoHideContentChanged(EventArgs e)
	{
		((EventHandler)base.Events[ActiveAutoHideContentChangedEvent])?.Invoke(this, e);
	}

	private void m_autoHideWindow_ActiveContentChanged(object sender, EventArgs e)
	{
		OnActiveAutoHideContentChanged(e);
	}

	protected virtual void OnContentAdded(DockContentEventArgs e)
	{
		((EventHandler<DockContentEventArgs>)base.Events[ContentAddedEvent])?.Invoke(this, e);
	}

	protected virtual void OnContentRemoved(DockContentEventArgs e)
	{
		((EventHandler<DockContentEventArgs>)base.Events[ContentRemovedEvent])?.Invoke(this, e);
	}

	internal void ResetDockWindows()
	{
		if (m_autoHideWindow == null)
		{
			return;
		}
		DockWindowCollection dockWindows = m_dockWindows;
		LoadDockWindows();
		foreach (DockWindow item in dockWindows)
		{
			base.Controls.Remove(item);
			item.Dispose();
		}
	}

	internal void LoadDockWindows()
	{
		m_dockWindows = new DockWindowCollection(this);
		foreach (DockWindow dockWindow in DockWindows)
		{
			base.Controls.Add(dockWindow);
		}
	}

	public void ResetAutoHideStripWindow()
	{
		AutoHideWindowControl autoHideWindow = m_autoHideWindow;
		m_autoHideWindow = Theme.Extender.AutoHideWindowFactory.CreateAutoHideWindow(this);
		m_autoHideWindow.Visible = false;
		SetAutoHideWindowParent();
		autoHideWindow.Visible = false;
		autoHideWindow.Parent = null;
		autoHideWindow.Dispose();
	}

	public void SaveAsXml(string fileName)
	{
		Persistor.SaveAsXml(this, fileName);
	}

	public void SaveAsXml(string fileName, Encoding encoding)
	{
		Persistor.SaveAsXml(this, fileName, encoding);
	}

	public void SaveAsXml(Stream stream, Encoding encoding)
	{
		Persistor.SaveAsXml(this, stream, encoding);
	}

	public void SaveAsXml(Stream stream, Encoding encoding, bool upstream)
	{
		Persistor.SaveAsXml(this, stream, encoding, upstream);
	}

	public void LoadFromXml(string fileName, DeserializeDockContent deserializeContent)
	{
		Persistor.LoadFromXml(this, fileName, deserializeContent);
	}

	public void LoadFromXml(Stream stream, DeserializeDockContent deserializeContent)
	{
		Persistor.LoadFromXml(this, stream, deserializeContent, closeStream: true);
	}

	public void LoadFromXml(Stream stream, DeserializeDockContent deserializeContent, bool closeStream)
	{
		Persistor.LoadFromXml(this, stream, deserializeContent, closeStream);
	}

	internal void SaveFocus()
	{
		DummyControl.Focus();
	}

	protected virtual void OnActiveDocumentChanged(EventArgs e)
	{
		((EventHandler)base.Events[ActiveDocumentChangedEvent])?.Invoke(this, e);
	}

	protected void OnActiveContentChanged(EventArgs e)
	{
		((EventHandler)base.Events[ActiveContentChangedEvent])?.Invoke(this, e);
	}

	internal void OnDocumentDragged()
	{
		((EventHandler)base.Events[DocumentDraggedEvent])?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnActivePaneChanged(EventArgs e)
	{
		((EventHandler)base.Events[ActivePaneChangedEvent])?.Invoke(this, e);
	}

	static DockPanel()
	{
		ActiveAutoHideContentChangedEvent = new object();
		ContentAddedEvent = new object();
		ContentRemovedEvent = new object();
		ActiveDocumentChangedEvent = new object();
		ActiveContentChangedEvent = new object();
		DocumentDraggedEvent = new object();
		ActivePaneChangedEvent = new object();
	}
}
