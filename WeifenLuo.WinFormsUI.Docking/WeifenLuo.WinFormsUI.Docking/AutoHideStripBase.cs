using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace WeifenLuo.WinFormsUI.Docking;

public abstract class AutoHideStripBase : Control
{
	protected class Tab : IDisposable
	{
		private IDockContent m_content;

		public IDockContent Content => m_content;

		protected internal Tab(IDockContent content)
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

		public DockPanel DockPanel => DockPane.DockPanel;

		public int Count => DockPane.DisplayingContents.Count;

		public Tab this[int index]
		{
			get
			{
				IDockContent dockContent = DockPane.DisplayingContents[index];
				if (dockContent == null)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				if (dockContent.DockHandler.AutoHideTab == null)
				{
					dockContent.DockHandler.AutoHideTab = DockPanel.AutoHideStripControl.CreateTab(dockContent);
				}
				return dockContent.DockHandler.AutoHideTab as Tab;
			}
		}

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
			return IndexOf(tab.Content);
		}

		public int IndexOf(IDockContent content)
		{
			return DockPane.DisplayingContents.IndexOf(content);
		}
	}

	protected class Pane : IDisposable
	{
		private DockPane m_dockPane;

		public DockPane DockPane => m_dockPane;

		public TabCollection AutoHideTabs
		{
			get
			{
				if (DockPane.AutoHideTabs == null)
				{
					DockPane.AutoHideTabs = new TabCollection(DockPane);
				}
				return DockPane.AutoHideTabs as TabCollection;
			}
		}

		protected internal Pane(DockPane dockPane)
		{
			m_dockPane = dockPane;
		}

		~Pane()
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

	protected sealed class PaneCollection : IEnumerable<Pane>, IEnumerable
	{
		private class AutoHideState
		{
			public DockState m_dockState;

			public bool m_selected;

			public DockState DockState => m_dockState;

			public bool Selected
			{
				get
				{
					return m_selected;
				}
				set
				{
					m_selected = value;
				}
			}

			public AutoHideState(DockState dockState)
			{
				m_dockState = dockState;
			}
		}

		private class AutoHideStateCollection
		{
			private AutoHideState[] m_states;

			public AutoHideState this[DockState dockState]
			{
				get
				{
					for (int i = 0; i < m_states.Length; i++)
					{
						if (m_states[i].DockState == dockState)
						{
							return m_states[i];
						}
					}
					throw new ArgumentOutOfRangeException("dockState");
				}
			}

			public AutoHideStateCollection()
			{
				m_states = new AutoHideState[4]
				{
					new AutoHideState(DockState.DockTopAutoHide),
					new AutoHideState(DockState.DockBottomAutoHide),
					new AutoHideState(DockState.DockLeftAutoHide),
					new AutoHideState(DockState.DockRightAutoHide)
				};
			}

			public bool ContainsPane(DockPane pane)
			{
				if (pane.IsHidden)
				{
					return false;
				}
				for (int i = 0; i < m_states.Length; i++)
				{
					if (m_states[i].DockState == pane.DockState && m_states[i].Selected)
					{
						return true;
					}
				}
				return false;
			}
		}

		private DockPanel m_dockPanel;

		private AutoHideStateCollection m_states;

		public DockPanel DockPanel => m_dockPanel;

		private AutoHideStateCollection States => m_states;

		public int Count
		{
			get
			{
				int num = 0;
				foreach (DockPane pane in DockPanel.Panes)
				{
					if (States.ContainsPane(pane))
					{
						num++;
					}
				}
				return num;
			}
		}

		public Pane this[int index]
		{
			get
			{
				int num = 0;
				foreach (DockPane pane in DockPanel.Panes)
				{
					if (!States.ContainsPane(pane))
					{
						continue;
					}
					if (num == index)
					{
						if (pane.AutoHidePane == null)
						{
							pane.AutoHidePane = DockPanel.AutoHideStripControl.CreatePane(pane);
						}
						return pane.AutoHidePane as Pane;
					}
					num++;
				}
				throw new ArgumentOutOfRangeException("index");
			}
		}

		internal PaneCollection(DockPanel panel, DockState dockState)
		{
			m_dockPanel = panel;
			m_states = new AutoHideStateCollection();
			States[DockState.DockTopAutoHide].Selected = dockState == DockState.DockTopAutoHide;
			States[DockState.DockBottomAutoHide].Selected = dockState == DockState.DockBottomAutoHide;
			States[DockState.DockLeftAutoHide].Selected = dockState == DockState.DockLeftAutoHide;
			States[DockState.DockRightAutoHide].Selected = dockState == DockState.DockRightAutoHide;
		}

		public bool Contains(Pane pane)
		{
			return IndexOf(pane) != -1;
		}

		public int IndexOf(Pane pane)
		{
			if (pane == null)
			{
				return -1;
			}
			int num = 0;
			foreach (DockPane pane2 in DockPanel.Panes)
			{
				if (States.ContainsPane(pane.DockPane))
				{
					if (pane == pane2.AutoHidePane)
					{
						return num;
					}
					num++;
				}
			}
			return -1;
		}

		IEnumerator<Pane> IEnumerable<Pane>.GetEnumerator()
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
	}

	public class AutoHideStripsAccessibleObject : ControlAccessibleObject
	{
		private AutoHideStripBase _strip;

		public override AccessibleRole Role => AccessibleRole.Window;

		public AutoHideStripsAccessibleObject(AutoHideStripBase strip)
			: base(strip)
		{
			_strip = strip;
		}

		public override int GetChildCount()
		{
			return 4;
		}

		public override AccessibleObject GetChild(int index)
		{
			return index switch
			{
				0 => new AutoHideStripAccessibleObject(_strip, DockState.DockTopAutoHide, this), 
				1 => new AutoHideStripAccessibleObject(_strip, DockState.DockBottomAutoHide, this), 
				2 => new AutoHideStripAccessibleObject(_strip, DockState.DockLeftAutoHide, this), 
				_ => new AutoHideStripAccessibleObject(_strip, DockState.DockRightAutoHide, this), 
			};
		}

		public override AccessibleObject HitTest(int x, int y)
		{
			Dictionary<DockState, Rectangle> obj = new Dictionary<DockState, Rectangle>
			{
				{
					DockState.DockTopAutoHide,
					_strip.GetTabStripRectangle(DockState.DockTopAutoHide)
				},
				{
					DockState.DockBottomAutoHide,
					_strip.GetTabStripRectangle(DockState.DockBottomAutoHide)
				},
				{
					DockState.DockLeftAutoHide,
					_strip.GetTabStripRectangle(DockState.DockLeftAutoHide)
				},
				{
					DockState.DockRightAutoHide,
					_strip.GetTabStripRectangle(DockState.DockRightAutoHide)
				}
			};
			Point pt = _strip.PointToClient(new Point(x, y));
			foreach (KeyValuePair<DockState, Rectangle> item in obj)
			{
				if (item.Value.Contains(pt))
				{
					return new AutoHideStripAccessibleObject(_strip, item.Key, this);
				}
			}
			return null;
		}
	}

	public class AutoHideStripAccessibleObject : AccessibleObject
	{
		private AutoHideStripBase _strip;

		private DockState _state;

		private AccessibleObject _parent;

		public override AccessibleObject Parent => _parent;

		public override AccessibleRole Role => AccessibleRole.PageTabList;

		public override Rectangle Bounds => ToScreen(_strip.GetTabStripRectangle(_state), _strip);

		public AutoHideStripAccessibleObject(AutoHideStripBase strip, DockState state, AccessibleObject parent)
		{
			_strip = strip;
			_state = state;
			_parent = parent;
		}

		public override int GetChildCount()
		{
			int num = 0;
			foreach (Pane item in (IEnumerable<Pane>)_strip.GetPanes(_state))
			{
				num += item.AutoHideTabs.Count;
			}
			return num;
		}

		public override AccessibleObject GetChild(int index)
		{
			List<Tab> list = new List<Tab>();
			foreach (Pane item in (IEnumerable<Pane>)_strip.GetPanes(_state))
			{
				list.AddRange(item.AutoHideTabs);
			}
			return new AutoHideStripTabAccessibleObject(_strip, list[index], this);
		}
	}

	protected class AutoHideStripTabAccessibleObject : AccessibleObject
	{
		private AutoHideStripBase _strip;

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

		internal AutoHideStripTabAccessibleObject(AutoHideStripBase strip, Tab tab, AccessibleObject parent)
		{
			_strip = strip;
			_tab = tab;
			_parent = parent;
		}
	}

	private GraphicsPath m_displayingArea;

	protected DockPanel DockPanel { get; private set; }

	protected PaneCollection PanesTop { get; private set; }

	protected PaneCollection PanesBottom { get; private set; }

	protected PaneCollection PanesLeft { get; private set; }

	protected PaneCollection PanesRight { get; private set; }

	protected Rectangle RectangleTopLeft
	{
		get
		{
			int num = MeasureHeight();
			int dockPadding = DockPanel.Theme.Measures.DockPadding;
			int num2 = ((PanesLeft.Count > 0) ? num : dockPadding);
			int num3 = ((PanesTop.Count > 0) ? num : dockPadding);
			return new Rectangle(0, 0, num2, num3);
		}
	}

	protected Rectangle RectangleTopRight
	{
		get
		{
			int num = MeasureHeight();
			int dockPadding = DockPanel.Theme.Measures.DockPadding;
			int num2 = ((PanesRight.Count > 0) ? num : dockPadding);
			int num3 = ((PanesTop.Count > 0) ? num : dockPadding);
			return new Rectangle(base.Width - num2, 0, num2, num3);
		}
	}

	protected Rectangle RectangleBottomLeft
	{
		get
		{
			int num = MeasureHeight();
			int dockPadding = DockPanel.Theme.Measures.DockPadding;
			int num2 = ((PanesLeft.Count > 0) ? num : dockPadding);
			int num3 = ((PanesBottom.Count > 0) ? num : dockPadding);
			return new Rectangle(0, base.Height - num3, num2, num3);
		}
	}

	protected Rectangle RectangleBottomRight
	{
		get
		{
			int num = MeasureHeight();
			int dockPadding = DockPanel.Theme.Measures.DockPadding;
			int num2 = ((PanesRight.Count > 0) ? num : dockPadding);
			int num3 = ((PanesBottom.Count > 0) ? num : dockPadding);
			return new Rectangle(base.Width - num2, base.Height - num3, num2, num3);
		}
	}

	private GraphicsPath DisplayingArea
	{
		get
		{
			if (m_displayingArea == null)
			{
				m_displayingArea = new GraphicsPath();
			}
			return m_displayingArea;
		}
	}

	protected AutoHideStripBase(DockPanel panel)
	{
		DockPanel = panel;
		PanesTop = new PaneCollection(panel, DockState.DockTopAutoHide);
		PanesBottom = new PaneCollection(panel, DockState.DockBottomAutoHide);
		PanesLeft = new PaneCollection(panel, DockState.DockLeftAutoHide);
		PanesRight = new PaneCollection(panel, DockState.DockRightAutoHide);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.Selectable, value: false);
	}

	protected PaneCollection GetPanes(DockState dockState)
	{
		return dockState switch
		{
			DockState.DockTopAutoHide => PanesTop, 
			DockState.DockBottomAutoHide => PanesBottom, 
			DockState.DockLeftAutoHide => PanesLeft, 
			DockState.DockRightAutoHide => PanesRight, 
			_ => throw new ArgumentOutOfRangeException("dockState"), 
		};
	}

	internal int GetNumberOfPanes(DockState dockState)
	{
		return GetPanes(dockState).Count;
	}

	protected internal Rectangle GetTabStripRectangle(DockState dockState)
	{
		return dockState switch
		{
			DockState.DockTopAutoHide => new Rectangle(RectangleTopLeft.Width, 0, base.Width - RectangleTopLeft.Width - RectangleTopRight.Width, RectangleTopLeft.Height), 
			DockState.DockBottomAutoHide => new Rectangle(RectangleBottomLeft.Width, base.Height - RectangleBottomLeft.Height, base.Width - RectangleBottomLeft.Width - RectangleBottomRight.Width, RectangleBottomLeft.Height), 
			DockState.DockLeftAutoHide => new Rectangle(0, RectangleTopLeft.Height, RectangleTopLeft.Width, base.Height - RectangleTopLeft.Height - RectangleBottomLeft.Height), 
			DockState.DockRightAutoHide => new Rectangle(base.Width - RectangleTopRight.Width, RectangleTopRight.Height, RectangleTopRight.Width, base.Height - RectangleTopRight.Height - RectangleBottomRight.Height), 
			_ => Rectangle.Empty, 
		};
	}

	private void SetRegion()
	{
		DisplayingArea.Reset();
		DisplayingArea.AddRectangle(RectangleTopLeft);
		DisplayingArea.AddRectangle(RectangleTopRight);
		DisplayingArea.AddRectangle(RectangleBottomLeft);
		DisplayingArea.AddRectangle(RectangleBottomRight);
		DisplayingArea.AddRectangle(GetTabStripRectangle(DockState.DockTopAutoHide));
		DisplayingArea.AddRectangle(GetTabStripRectangle(DockState.DockBottomAutoHide));
		DisplayingArea.AddRectangle(GetTabStripRectangle(DockState.DockLeftAutoHide));
		DisplayingArea.AddRectangle(GetTabStripRectangle(DockState.DockRightAutoHide));
		base.Region = new Region(DisplayingArea);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button == MouseButtons.Left)
		{
			IDockContent dockContent = HitTest();
			if (dockContent != null)
			{
				SetActiveAutoHideContent(dockContent);
				dockContent.DockHandler.Activate();
			}
		}
	}

	protected override void OnMouseHover(EventArgs e)
	{
		base.OnMouseHover(e);
		if (DockPanel.ShowAutoHideContentOnHover)
		{
			IDockContent activeAutoHideContent = HitTest();
			SetActiveAutoHideContent(activeAutoHideContent);
			ResetMouseEventArgs();
		}
	}

	private void SetActiveAutoHideContent(IDockContent content)
	{
		if (content != null)
		{
			if (DockPanel.ActiveAutoHideContent != content)
			{
				DockPanel.ActiveAutoHideContent = content;
			}
			else if (!DockPanel.ShowAutoHideContentOnHover)
			{
				DockPanel.ActiveAutoHideContent = null;
			}
		}
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		RefreshChanges();
		base.OnLayout(levent);
	}

	internal void RefreshChanges()
	{
		if (!base.IsDisposed)
		{
			SetRegion();
			OnRefreshChanges();
		}
	}

	protected virtual void OnRefreshChanges()
	{
	}

	protected internal abstract int MeasureHeight();

	private IDockContent HitTest()
	{
		Point point = PointToClient(Control.MousePosition);
		return HitTest(point);
	}

	protected virtual Tab CreateTab(IDockContent content)
	{
		return new Tab(content);
	}

	protected virtual Pane CreatePane(DockPane dockPane)
	{
		return new Pane(dockPane);
	}

	protected abstract IDockContent HitTest(Point point);

	protected override AccessibleObject CreateAccessibilityInstance()
	{
		return new AutoHideStripsAccessibleObject(this);
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
}
