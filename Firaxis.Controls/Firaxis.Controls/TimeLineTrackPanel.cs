using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.Collections;
using Firaxis.Controls.Scrollables;
using Firaxis.Utility;

namespace Firaxis.Controls;

public class TimeLineTrackPanel : UserControl
{
	private enum DragMode
	{
		Idle,
		Pan,
		Shuttle,
		Key
	}

	public delegate void ContextMenuKeyHandler(object sender, ScrollableTree.TreeNode node, EventArgs e);

	private DragMode drag;

	private Point drag_pt;

	private bool drag_moved;

	private ScrollableItemPaintEventArgs paint_args;

	private TimeLineHitArgs timeline_args;

	private ScrollableTree.TreeNode drag_node;

	private IAsyncResult m_inFlightInvoke = null;

	private IContainer components = null;

	public Color BackColor0 { get; set; }

	public Color BackColor1 { get; set; }

	public bool LockKeys { get; set; }

	[Browsable(false)]
	public ListEvent<IKey> SelectedKeys { get; private set; }

	private TimeLineControl TimeLineControl { get; set; }

	private TimeRulerControl Ruler => TimeLineControl.TimeRuler;

	private ScrollableTree Tree => TimeLineControl.ScrollableTree;

	public event EventHandler MovingKeyBegin;

	public event EventHandler MovingKey;

	public event EventHandler MovingKeyEnd;

	public event EventHandler ModifiedAction;

	public event ContextMenuKeyHandler ContextMenuKey;

	public event EventHandler<ScrollableItemPaintEventArgs> PostPaintTracks;

	public TimeLineTrackPanel(TimeLineControl timeLineControl, Panel panel)
	{
		TimeLineControl = timeLineControl;
		TimeLineControl.TimeRuler.ScaleChanged += TimeRuler_ScaleChanged;
		InitializeComponent();
		SelectedKeys = new ListEvent<IKey>();
		paint_args = new ScrollableItemPaintEventArgs();
		timeline_args = new TimeLineHitArgs(timeLineControl, Rectangle.Empty);
		drag = DragMode.Idle;
		drag_moved = false;
		Dock = DockStyle.Fill;
		BackColor = panel.BackColor;
		BackColor0 = Color.FromArgb(170, 170, 170);
		BackColor1 = Color.FromArgb(181, 181, 181);
		Ruler.CurrentTimeChanged += RefreshDisplay;
		Ruler.OriginChanged += RefreshDisplay;
		Ruler.RangeChanged += RefreshDisplay;
		Tree.DisplayListChanged += RefreshDisplay;
		Tree.VerticalScroll.ValueChanged += RefreshDisplay;
		base.MouseWheel += TimeLineTrackPanel_MouseWheel;
	}

	private void TimeLineTrackPanel_MouseWheel(object sender, MouseEventArgs e)
	{
		if (KeyHelper.CtrlPressed)
		{
			int num = (TimeLineControl.TrackBar.Maximum - TimeLineControl.TrackBar.Minimum) / 20;
			TimeLineControl.SetMajorScale(Ruler.MajorScale + (float)((e.Delta < 0) ? num : (-num)));
		}
		else
		{
			Tree.PerformVScroll((e.Delta < 0) ? ScrollEventType.SmallIncrement : ScrollEventType.SmallDecrement, 0);
		}
	}

	private void TimeRuler_ScaleChanged(object sender, EventArgs e)
	{
		Invalidate();
		Update();
	}

	private void ThrottledRefresh()
	{
		if (m_inFlightInvoke != null && !m_inFlightInvoke.IsCompleted)
		{
			return;
		}
		if (base.IsHandleCreated && base.InvokeRequired)
		{
			if (m_inFlightInvoke != null)
			{
				EndInvoke(m_inFlightInvoke);
			}
			m_inFlightInvoke = BeginInvoke((Action)delegate
			{
				Invalidate();
				Update();
			});
		}
		else
		{
			Invalidate();
			Update();
		}
	}

	private void RefreshDisplay(object sender, EventArgs e)
	{
		ThrottledRefresh();
	}

	private void TimeLineTrackPanel_Paint(object sender, PaintEventArgs e)
	{
		PaintTracks(e.Graphics);
	}

	protected virtual void PaintTracks(Graphics graphics)
	{
		int num = Ruler.TimeToX(0f);
		graphics.DrawLine(Pens.Gray, num, 0, num, base.ClientSize.Height);
		DrawTracks(graphics);
		DrawMajorTickLines(graphics);
		OnPostPaintTracks(graphics);
		DrawTrackRange(graphics);
		DrawShuttle(graphics);
	}

	private void DrawTracks(Graphics graphics)
	{
		int value = Tree.VerticalScroll.Value;
		int num = value + base.ClientSize.Height;
		Rectangle rectangle = new Rectangle(0, 0, base.ClientSize.Width, 0);
		using SolidBrush solidBrush = new SolidBrush(BackColor0);
		using SolidBrush solidBrush2 = new SolidBrush(BackColor1);
		Brush[] array = new Brush[2] { solidBrush, solidBrush2 };
		int num2 = 0;
		int num3 = Ruler.TimeToX(0f);
		foreach (ScrollableTree.DisplayTreeNode displayNode in Tree.DisplayNodes)
		{
			if (displayNode.Origin + displayNode.Node.Item.ItemHeight >= value || displayNode.Origin < num)
			{
				rectangle.Y = displayNode.Origin - value;
				rectangle.Height = displayNode.Node.Item.ItemHeight;
				graphics.FillRectangle(array[num2 % 2], rectangle);
				graphics.DrawLine(Pens.Gray, num3, rectangle.Y, num3, rectangle.Bottom);
				PaintTrack(graphics, displayNode, rectangle);
			}
			num2++;
		}
	}

	private void PaintTrack(Graphics g, ScrollableTree.DisplayTreeNode node, Rectangle r)
	{
		if (node.Node.Item is IScrollableItemTrack scrollableItemTrack)
		{
			ScrollableItemState scrollableItemState = (Tree.SelectedNodes.Contains(node.Node) ? ScrollableItemState.Selected : ScrollableItemState.Normal);
			paint_args.Graphics = g;
			paint_args.Bounds = r;
			paint_args.State = scrollableItemState;
			paint_args.Level = node.Level;
			paint_args.Style = ScrollableItemStyle.Normal;
			paint_args.Interacting = drag == DragMode.Key && drag_moved;
			scrollableItemTrack.PaintTrack(TimeLineControl, paint_args);
		}
		else
		{
			g.DrawLine(Pens.LightGray, r.X, r.Bottom, r.Right, r.Bottom);
		}
	}

	private void DrawMajorTickLines(Graphics graphics)
	{
		using Pen pen = new Pen(Color.FromArgb(64, Color.Gray));
		float num = Ruler.Origin + Ruler.TimeSpan;
		for (float num2 = (float)Math.Truncate(Ruler.Origin); num2 < num; num2 += 1f)
		{
			int num3 = Ruler.TimeToX(num2);
			graphics.DrawLine(pen, num3, 0, num3, base.ClientSize.Height);
		}
	}

	private void OnPostPaintTracks(Graphics graphics)
	{
		EventHandler<ScrollableItemPaintEventArgs> eventHandler = this.PostPaintTracks;
		if (eventHandler != null)
		{
			paint_args.Graphics = graphics;
			paint_args.Bounds = base.ClientRectangle;
			paint_args.State = ScrollableItemState.Normal;
			paint_args.Level = 0;
			paint_args.Style = ScrollableItemStyle.Normal;
			paint_args.Interacting = drag == DragMode.Key && drag_moved;
			eventHandler(this, paint_args);
		}
	}

	private void DrawTrackRange(Graphics graphics)
	{
		if (Ruler.TrackRangeVisible)
		{
			using (Pen pen = new Pen(Ruler.RangeColor))
			{
				int num = Ruler.TimeToX(Ruler.RangeStart);
				graphics.DrawLine(pen, num, 0, num, base.ClientSize.Height);
				num = Ruler.TimeToX(Ruler.RangeStart + Ruler.RangeDuration);
				graphics.DrawLine(pen, num, 0, num, base.ClientSize.Height);
			}
		}
	}

	private void DrawShuttle(Graphics graphics)
	{
		if (Ruler.ShuttleVisible)
		{
			int num = Ruler.TimeToX(Ruler.CurrentTime);
			using Pen pen = new Pen(Ruler.ShuttleColor);
			graphics.DrawLine(pen, num, 0, num, base.ClientSize.Height);
		}
	}

	private void TimeLineTrackPanel_MouseDown(object sender, MouseEventArgs e)
	{
		bool ctrlPressed = KeyHelper.CtrlPressed;
		if (e.Button == MouseButtons.Middle)
		{
			drag = DragMode.Pan;
			drag_pt.X = e.X;
			base.Capture = true;
			Cursor = CustomCursors.Get(CustomCursor.HandDrag);
		}
		else if (e.Button == MouseButtons.Left)
		{
			UpdateSelection(ctrlPressed, clearOnReselect: false, cycleCoincident: true, e.X, e.Y);
			if (TimeLineControl.AllowEdit && !LockKeys)
			{
				drag = DragMode.Key;
				drag_moved = false;
				drag_pt.X = e.X;
				drag_pt.Y = e.Y;
			}
			Invalidate();
		}
		else if (e.Button == MouseButtons.Right)
		{
			UpdateSelection(ctrlPressed, clearOnReselect: false, cycleCoincident: false, e.X, e.Y);
			Invalidate();
			this.ContextMenuKey?.Invoke(this, drag_node, EventArgs.Empty);
		}
	}

	private void UpdateSelection(bool ctrl, bool clearOnReselect, bool cycleCoincident, int x, int y)
	{
		IEnumerable<IKey> enumerable = FindKeys(x, y, ref drag_node);
		bool flag = Tree.SelectedNodes.Contains(drag_node);
		bool flag2 = enumerable.Any();
		bool flag3 = SelectedKeys.Any();
		if (flag)
		{
			if (!flag2)
			{
				if (flag3)
				{
					SelectedKeys.Clear();
				}
			}
			else if (enumerable.Count() > 1)
			{
				if (ctrl)
				{
					SelectedKeys.AddRange(enumerable.Except(SelectedKeys));
				}
				else if (SelectedKeys.Count() != 1 || !enumerable.Contains(SelectedKeys[0]))
				{
					if (clearOnReselect || enumerable.Intersect(SelectedKeys).Count() != enumerable.Count())
					{
						SelectedKeys.Clear();
						SelectedKeys.Add(enumerable.Last());
					}
				}
				else
				{
					if (!cycleCoincident)
					{
						return;
					}
					IKey other = SelectedKeys[0];
					IKey item = enumerable.Last();
					foreach (IKey item2 in enumerable)
					{
						if (item2.Equals(other))
						{
							break;
						}
						item = item2;
					}
					SelectedKeys.Clear();
					SelectedKeys.Add(item);
				}
			}
			else
			{
				if (SelectedKeys.Any() && !ctrl && (clearOnReselect || enumerable.Intersect(SelectedKeys).Count() != enumerable.Count()))
				{
					SelectedKeys.Clear();
				}
				enumerable = enumerable.Except(SelectedKeys);
				SelectedKeys.AddRange(enumerable);
			}
			return;
		}
		Tree.BeginUpdate();
		Tree.SuspendDrawing();
		Tree.SelectedNodes.SuspendNotify();
		Tree.SelectedNodes.Clear();
		Tree.SelectedNodes.ResumeNotify();
		if (drag_node != null)
		{
			Tree.SelectedNodes.Add(drag_node);
		}
		Tree.ResumeDrawing();
		Tree.EndUpdate();
		SelectedKeys.Clear();
		if (flag2)
		{
			if (enumerable.Count() > 1 && !ctrl)
			{
				SelectedKeys.Add(enumerable.Last());
			}
			else
			{
				SelectedKeys.AddRange(enumerable);
			}
		}
	}

	private IEnumerable<IKey> FindKeys(int x, int y, ref ScrollableTree.TreeNode drag_node)
	{
		ScrollableTree.TreeNode treeNode = Tree.FindNode(0, y);
		if (treeNode != null)
		{
			drag_node = treeNode;
			if (treeNode.Item is IScrollableItemKeys)
			{
				timeline_args.Bounds = Tree.GetBounds(treeNode);
				return ((IScrollableItemKeys)treeNode.Item).FindKeys(x, y, timeline_args);
			}
			return Enumerable.Empty<IKey>();
		}
		drag_node = null;
		return Enumerable.Empty<IKey>();
	}

	private void TimeLineTrackPanel_MouseMove(object sender, MouseEventArgs e)
	{
		switch (drag)
		{
		case DragMode.Pan:
			Ruler.Origin = Math.Max(0f, Ruler.Origin - (float)(e.X - drag_pt.X) / Ruler.MajorScale);
			drag_pt.X = e.X;
			break;
		case DragMode.Idle:
			Cursor = (FindKeys(e.X, e.Y, ref drag_node).Any() ? Cursors.Hand : Cursors.Default);
			break;
		case DragMode.Key:
		{
			if (SelectedKeys.Count <= 0 || (drag_pt.X == e.X && drag_pt.Y == e.Y) || e.Button != MouseButtons.Left)
			{
				break;
			}
			if (!drag_moved)
			{
				if (TimeLineControl.AllowEdit && !LockKeys)
				{
					this.MovingKeyBegin?.Invoke(this, EventArgs.Empty);
					base.Capture = true;
				}
				this.ModifiedAction?.Invoke(this, EventArgs.Empty);
				this.MovingKey?.Invoke(this, EventArgs.Empty);
				drag_moved = true;
			}
			IScrollableItemKeys scrollableItemKeys = (IScrollableItemKeys)drag_node.Item;
			int delta = e.X - drag_pt.X;
			foreach (IKey selectedKey in SelectedKeys)
			{
				scrollableItemKeys.MoveKey(TimeLineControl, selectedKey, delta);
			}
			drag_pt.X = e.X;
			drag_pt.Y = e.Y;
			Invalidate();
			Update();
			break;
		}
		case DragMode.Shuttle:
			break;
		}
	}

	private void TimeLineTrackPanel_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Middle && drag == DragMode.Pan)
		{
			drag = DragMode.Idle;
			base.Capture = false;
			Cursor = Cursors.Default;
		}
		else if (e.Button == MouseButtons.Left && drag == DragMode.Key)
		{
			if (drag_moved)
			{
				this.MovingKeyEnd?.Invoke(this, EventArgs.Empty);
			}
			else
			{
				UpdateSelection(KeyHelper.CtrlPressed, clearOnReselect: true, cycleCoincident: false, e.X, e.Y);
			}
			drag_moved = false;
			drag = DragMode.Idle;
			base.Capture = false;
			Cursor = Cursors.Default;
			Invalidate();
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.DarkGray;
		this.DoubleBuffered = true;
		base.Name = "TimeLineTrackPanel";
		base.Size = new System.Drawing.Size(351, 179);
		base.Paint += new System.Windows.Forms.PaintEventHandler(TimeLineTrackPanel_Paint);
		base.MouseMove += new System.Windows.Forms.MouseEventHandler(TimeLineTrackPanel_MouseMove);
		base.MouseDown += new System.Windows.Forms.MouseEventHandler(TimeLineTrackPanel_MouseDown);
		base.MouseUp += new System.Windows.Forms.MouseEventHandler(TimeLineTrackPanel_MouseUp);
		base.ResumeLayout(false);
	}
}
