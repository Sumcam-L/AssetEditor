using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public abstract class DockPaneStripBase : Control
{
	protected internal class Tab : IDisposable
	{
		private IDockContent m_content;

		public IDockContent Content => m_content;

		public Form ContentForm => m_content as Form;

		public Tab(IDockContent content)
		{
			m_content = content;
		}

		~Tab()
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
		}
	}

	protected sealed class TabCollection : IEnumerable<Tab>, IEnumerable
	{
		private DockPane m_dockPane;

		public DockPane DockPane => m_dockPane;

		public int Count => DockPane.DisplayingContents.Count;

		public Tab this[int index] => (DockPane.DisplayingContents[index] ?? throw new ArgumentOutOfRangeException("index")).DockHandler.GetTab(DockPane.TabStripControl);

		IEnumerator<Tab> IEnumerable<Tab>.GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return this[i];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
			{
				yield return this[i];
			}
		}

		internal TabCollection(DockPane pane)
		{
			m_dockPane = pane;
		}

		public bool Contains(Tab tab)
		{
			return IndexOf(tab) != -1;
		}

		public bool Contains(IDockContent content)
		{
			return IndexOf(content) != -1;
		}

		public int IndexOf(Tab tab)
		{
			if (tab == null)
			{
				return -1;
			}
			return DockPane.DisplayingContents.IndexOf(tab.Content);
		}

		public int IndexOf(IDockContent content)
		{
			return DockPane.DisplayingContents.IndexOf(content);
		}
	}

	public class DockPaneStripAccessibleObject : ControlAccessibleObject
	{
		private DockPaneStripBase _strip;

		public override AccessibleRole Role => AccessibleRole.PageTabList;

		public DockPaneStripAccessibleObject(DockPaneStripBase strip)
			: base(strip)
		{
			_strip = strip;
		}

		public override int GetChildCount()
		{
			return _strip.Tabs.Count;
		}

		public override AccessibleObject GetChild(int index)
		{
			return new DockPaneStripTabAccessibleObject(_strip, _strip.Tabs[index], this);
		}

		public override AccessibleObject HitTest(int x, int y)
		{
			Point pt = new Point(x, y);
			foreach (Tab item in (IEnumerable<Tab>)_strip.Tabs)
			{
				if (ToScreen(_strip.GetTabBounds(item), _strip).Contains(pt))
				{
					return new DockPaneStripTabAccessibleObject(_strip, item, this);
				}
			}
			return null;
		}
	}

	protected class DockPaneStripTabAccessibleObject : AccessibleObject
	{
		private DockPaneStripBase _strip;

		private Tab _tab;

		private AccessibleObject _parent;

		public override AccessibleObject Parent => _parent;

		public override AccessibleRole Role => AccessibleRole.PageTab;

		public override Rectangle Bounds => ToScreen(_strip.GetTabBounds(_tab), _strip);

		public override string Name
		{
			get
			{
				return _tab.Content.DockHandler.TabText;
			}
			set
			{
			}
		}

		internal DockPaneStripTabAccessibleObject(DockPaneStripBase strip, Tab tab, AccessibleObject parent)
		{
			_strip = strip;
			_tab = tab;
			_parent = parent;
		}
	}

	private DockPane m_dockPane;

	private TabCollection m_tabs;

	private Rectangle _dragBox = Rectangle.Empty;

	protected DockPane DockPane => m_dockPane;

	protected DockPane.AppearanceStyle Appearance => DockPane.Appearance;

	protected TabCollection Tabs => m_tabs ?? (m_tabs = new TabCollection(DockPane));

	protected bool HasTabPageContextMenu => DockPane.HasTabPageContextMenu;

	protected DockPaneStripBase(DockPane pane)
	{
		m_dockPane = pane;
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.Selectable, value: false);
		AllowDrop = true;
	}

	internal void RefreshChanges()
	{
		if (!base.IsDisposed)
		{
			OnRefreshChanges();
		}
	}

	protected virtual void OnRefreshChanges()
	{
	}

	protected internal abstract int MeasureHeight();

	protected internal abstract void EnsureTabVisible(IDockContent content);

	protected int HitTest()
	{
		return HitTest(PointToClient(Control.MousePosition));
	}

	protected internal abstract int HitTest(Point point);

	protected virtual bool MouseDownActivateTest(MouseEventArgs e)
	{
		return true;
	}

	public abstract GraphicsPath GetOutline(int index);

	protected internal virtual Tab CreateTab(IDockContent content)
	{
		return new Tab(content);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		int num = HitTest();
		if (num != -1)
		{
			if (e.Button == MouseButtons.Middle)
			{
				TryCloseTab(num);
			}
			else
			{
				IDockContent content = Tabs[num].Content;
				if (DockPane.ActiveContent != content && MouseDownActivateTest(e))
				{
					DockPane.ActiveContent = content;
				}
			}
		}
		if (e.Button == MouseButtons.Left)
		{
			Size dragSize = SystemInformation.DragSize;
			_dragBox = new Rectangle(new Point(e.X - dragSize.Width / 2, e.Y - dragSize.Height / 2), dragSize);
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (e.Button == MouseButtons.Left && !_dragBox.Contains(e.Location) && DockPane.ActiveContent != null && DockPane.DockPanel.AllowEndUserDocking && DockPane.AllowDockDragAndDrop && DockPane.ActiveContent.DockHandler.AllowEndUserDocking)
		{
			DockPane.DockPanel.BeginDrag(DockPane.ActiveContent.DockHandler);
		}
	}

	protected void ShowTabPageContextMenu(Point position)
	{
		DockPane.ShowTabPageContextMenu(this, position);
	}

	protected bool TryCloseTab(int index)
	{
		if (index >= 0 || index < Tabs.Count)
		{
			IDockContent content = Tabs[index].Content;
			DockPane.CloseContent(content);
			return true;
		}
		return false;
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (e.Button == MouseButtons.Right)
		{
			ShowTabPageContextMenu(new Point(e.X, e.Y));
		}
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 515)
		{
			base.WndProc(ref m);
			int num = HitTest();
			if (DockPane.DockPanel.AllowEndUserDocking && DockPane.DockPanel.AllowDoubleClickFloating && num != -1)
			{
				IDockContent content = Tabs[num].Content;
				if (content.DockHandler.CheckDockState(!content.DockHandler.IsFloat) != DockState.Unknown)
				{
					content.DockHandler.IsFloat = !content.DockHandler.IsFloat;
				}
			}
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	protected override void OnDragOver(DragEventArgs drgevent)
	{
		base.OnDragOver(drgevent);
		int num = HitTest();
		if (num != -1)
		{
			IDockContent content = Tabs[num].Content;
			if (DockPane.ActiveContent != content)
			{
				DockPane.ActiveContent = content;
			}
		}
	}

	protected void ContentClosed()
	{
		if (m_tabs.Count == 0)
		{
			DockPane.ClearLastActiveContent();
		}
	}

	protected abstract Rectangle GetTabBounds(Tab tab);

	internal static Rectangle ToScreen(Rectangle rectangle, Control parent)
	{
		if (parent == null)
		{
			return rectangle;
		}
		return new Rectangle(parent.PointToScreen(new Point(rectangle.Left, rectangle.Top)), new Size(rectangle.Width, rectangle.Height));
	}

	protected override AccessibleObject CreateAccessibilityInstance()
	{
		return new DockPaneStripAccessibleObject(this);
	}
}
