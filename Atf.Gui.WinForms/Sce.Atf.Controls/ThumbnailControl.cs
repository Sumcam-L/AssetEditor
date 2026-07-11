using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class ThumbnailControl : Panel
{
	private class ThumbnailItemList : Collection<ThumbnailControlItem>
	{
		private readonly ThumbnailControl m_control;

		public ThumbnailItemList(ThumbnailControl control)
		{
			m_control = control;
		}

		protected override void InsertItem(int index, ThumbnailControlItem item)
		{
			item.Control = m_control;
			base.InsertItem(index, item);
			m_control.RecalculateClientSize();
		}

		protected override void RemoveItem(int index)
		{
			base[index].Control = null;
			base.RemoveItem(index);
			m_control.RecalculateClientSize();
		}

		protected override void ClearItems()
		{
			using (IEnumerator<ThumbnailControlItem> enumerator = GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ThumbnailControlItem current = enumerator.Current;
					current.Control = null;
				}
			}
			base.ClearItems();
			m_control.RecalculateClientSize();
		}
	}

	private readonly ThumbnailItemList m_items;

	private readonly Selection<ThumbnailControlItem> m_selection = new Selection<ThumbnailControlItem>();

	private Orientation m_orientation = Orientation.Vertical;

	private int m_fontHeight;

	private int m_thumbnailSize = 96;

	private readonly Timer m_hoverTimer;

	private ThumbnailControlItem m_hoverThumbnail;

	private HoverLabel m_hoverLabel;

	private const int ThumbnailMargin = 16;

	private Size m_clientSize;

	private ImageList m_indicatorImages;

	private bool m_multiSelecting;

	private Point m_startPt;

	private Point m_mousePt;

	public Orientation Orientation
	{
		get
		{
			return m_orientation;
		}
		set
		{
			if (m_orientation != value)
			{
				m_orientation = value;
				RecalculateClientSize();
			}
		}
	}

	public int ThumbnailSize
	{
		get
		{
			return m_thumbnailSize;
		}
		set
		{
			if (m_thumbnailSize != value)
			{
				m_thumbnailSize = value;
				RecalculateClientSize();
			}
		}
	}

	public IList<ThumbnailControlItem> Items => m_items;

	public Selection<ThumbnailControlItem> Selection => m_selection;

	public ImageList IndicatorImageList
	{
		get
		{
			return m_indicatorImages;
		}
		set
		{
			m_indicatorImages = value;
			Invalidate();
		}
	}

	public event EventHandler SelectionChanged;

	public ThumbnailControl()
	{
		m_fontHeight = Font.Height;
		m_hoverTimer = new Timer
		{
			Interval = 500
		};
		m_hoverTimer.Tick += HoverTimerTick;
		base.DoubleBuffered = true;
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		m_items = new ThumbnailItemList(this);
	}

	public ThumbnailControl(Orientation orientation, int maxDimension)
		: this()
	{
		m_orientation = orientation;
		m_thumbnailSize = maxDimension;
	}

	public ThumbnailControl(Orientation orientation)
		: this()
	{
		m_orientation = orientation;
	}

	public ThumbnailControlItem PickThumbnail(Point point)
	{
		Point point2 = new Point(16 + base.AutoScrollPosition.X, 16 + base.AutoScrollPosition.Y);
		foreach (ThumbnailControlItem item in m_items)
		{
			if (GetThumbnailBoundaryRect(point2).Contains(point))
			{
				return item;
			}
			point2 = NextThumbnailPosition(point2);
		}
		return null;
	}

	protected override void OnFontChanged(EventArgs e)
	{
		m_fontHeight = Font.Height;
		base.OnFontChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		Point point = new Point(16 + base.AutoScrollPosition.X, 16 + base.AutoScrollPosition.Y);
		using (StringFormat stringFormat = new StringFormat())
		{
			stringFormat.Alignment = StringAlignment.Center;
			HashSet<ThumbnailControlItem> hashSet = new HashSet<ThumbnailControlItem>(m_selection);
			foreach (ThumbnailControlItem item in m_items)
			{
				Image image = item.Image;
				Rectangle thumbnailRect = GetThumbnailRect(point, image);
				e.Graphics.DrawImage(image, thumbnailRect);
				RectangleF layoutRectangle = new RectangleF(point.X, point.Y + m_thumbnailSize + 16, m_thumbnailSize, m_fontHeight);
				e.Graphics.DrawString(item.Name, Font, SystemBrushes.ControlText, layoutRectangle, stringFormat);
				if (m_indicatorImages != null && item.Indicator != null)
				{
					Image image2 = m_indicatorImages.Images[item.Indicator];
					if (image2 != null)
					{
						Rectangle rect = new Rectangle(new Point(point.X - 2, point.Y - 2), m_indicatorImages.ImageSize);
						e.Graphics.DrawImage(image2, rect);
					}
				}
				e.Graphics.DrawRectangle(hashSet.Contains(item) ? Pens.Black : Pens.LightGray, GetThumbnailBoundaryRect(point));
				point = NextThumbnailPosition(point);
			}
		}
		if (m_multiSelecting)
		{
			e.Graphics.DrawRectangle(Pens.DarkGray, GetSelectionRect());
		}
	}

	protected override void OnSizeChanged(EventArgs e)
	{
		RecalculateClientSize();
		base.OnSizeChanged(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && PickThumbnail(e.Location) == null)
		{
			m_multiSelecting = true;
			m_startPt = e.Location;
			m_mousePt = e.Location;
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.None)
		{
			ThumbnailControlItem thumbnailControlItem = PickThumbnail(new Point(e.X, e.Y));
			if (thumbnailControlItem != m_hoverThumbnail)
			{
				m_hoverThumbnail = thumbnailControlItem;
				m_hoverTimer.Start();
			}
		}
		if (m_multiSelecting)
		{
			m_mousePt = e.Location;
			Invalidate();
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			bool flag = (Control.ModifierKeys & Keys.Shift) != 0;
			bool flag2 = (Control.ModifierKeys & Keys.Control) != 0;
			if (!flag && !flag2)
			{
				m_selection.Clear();
			}
			HashSet<ThumbnailControlItem> selected = new HashSet<ThumbnailControlItem>(m_selection);
			if (m_multiSelecting)
			{
				m_multiSelecting = false;
				IEnumerable<ThumbnailControlItem> enumerable = Pick(GetSelectionRect());
				foreach (ThumbnailControlItem item in enumerable)
				{
					SelectItem(item, flag2, selected);
				}
			}
			else
			{
				ThumbnailControlItem thumbnailControlItem = PickThumbnail(e.Location);
				if (thumbnailControlItem != null)
				{
					SelectItem(thumbnailControlItem, flag2, selected);
				}
			}
			OnSelectionChanged(EventArgs.Empty);
			Invalidate();
		}
		base.OnMouseUp(e);
		EndHover();
	}

	private void SelectItem(ThumbnailControlItem item, bool toggle, HashSet<ThumbnailControlItem> selected)
	{
		if (toggle)
		{
			if (selected.Contains(item))
			{
				m_selection.Remove(item);
			}
			else
			{
				m_selection.Add(item);
			}
		}
		else if (!selected.Contains(item))
		{
			m_selection.Add(item);
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		EndHover();
		base.OnMouseLeave(e);
	}

	protected virtual void OnSelectionChanged(EventArgs e)
	{
		if (this.SelectionChanged != null)
		{
			this.SelectionChanged(this, e);
		}
	}

	private void HoverTimerTick(object sender, EventArgs e)
	{
		EndHover();
		if (m_hoverLabel == null && m_hoverThumbnail != null)
		{
			m_hoverLabel = new HoverLabel(m_hoverThumbnail.Description)
			{
				Location = new Point(Control.MousePosition.X - 8, Control.MousePosition.Y + 8)
			};
			m_hoverLabel.ShowWithoutFocus();
		}
	}

	private void EndHover()
	{
		if (m_hoverLabel != null)
		{
			m_hoverLabel.Hide();
			m_hoverLabel.Dispose();
			m_hoverLabel = null;
			m_hoverThumbnail = null;
		}
		m_hoverTimer.Stop();
	}

	private void RecalculateClientSize()
	{
		Size size = new Size(16, 16);
		if (m_items.Count > 0)
		{
			int num;
			int num2;
			if (m_orientation == Orientation.Horizontal)
			{
				num = (base.ClientSize.Height - 16) / (m_thumbnailSize + 32 + m_fontHeight);
				if (num < 1)
				{
					num = 1;
				}
				num2 = m_items.Count / num + 1;
				if (m_items.Count % 2 == 0)
				{
					num2--;
				}
				if (num2 < 1)
				{
					num2 = 1;
				}
			}
			else
			{
				num2 = (base.ClientSize.Width - 16) / (m_thumbnailSize + 16);
				if (num2 < 1)
				{
					num2 = 1;
				}
				num = m_items.Count / num2 + 1;
				if (num2 * num - m_items.Count >= num2)
				{
					num--;
				}
				if (num < 1)
				{
					num = 1;
				}
			}
			size.Width = num2 * (m_thumbnailSize + 16) + 16;
			size.Height = num * (m_thumbnailSize + 32 + m_fontHeight) + 16;
			m_clientSize = size;
		}
		base.AutoScrollMinSize = size;
		Invalidate();
	}

	private Rectangle GetThumbnailRect(Point thumbLoc, Image image)
	{
		Size thumbnailSize = GetThumbnailSize(image);
		int num = Math.Max(0, m_thumbnailSize - thumbnailSize.Width) / 2;
		int num2 = Math.Max(0, m_thumbnailSize - thumbnailSize.Height) / 2;
		thumbLoc.Y += num2;
		thumbLoc.X += num;
		return new Rectangle(thumbLoc, thumbnailSize);
	}

	private Rectangle GetThumbnailBoundaryRect(Point position)
	{
		return new Rectangle(position.X - 4, position.Y - 4, m_thumbnailSize + 8, m_thumbnailSize + 8);
	}

	private Rectangle GetSelectionRect()
	{
		int num = Math.Min(m_startPt.X, m_mousePt.X);
		int num2 = Math.Min(m_startPt.Y, m_mousePt.Y);
		int num3 = Math.Abs(m_mousePt.X - m_startPt.X);
		int num4 = Math.Abs(m_mousePt.Y - m_startPt.Y);
		return new Rectangle(num, num2, num3, num4);
	}

	private IEnumerable<ThumbnailControlItem> Pick(Rectangle rect)
	{
		List<ThumbnailControlItem> list = new List<ThumbnailControlItem>();
		Point point = new Point(16 + base.AutoScrollPosition.X, 16 + base.AutoScrollPosition.Y);
		foreach (ThumbnailControlItem item in m_items)
		{
			Rectangle thumbnailBoundaryRect = GetThumbnailBoundaryRect(point);
			if (rect.IntersectsWith(thumbnailBoundaryRect))
			{
				list.Add(item);
			}
			point = NextThumbnailPosition(point);
		}
		return list;
	}

	private Size GetThumbnailSize(Image image)
	{
		Size size = image.Size;
		int num = Math.Max(size.Width, size.Height);
		if (num > m_thumbnailSize)
		{
			double num2 = (double)m_thumbnailSize / (double)num;
			size.Width = (int)((double)size.Width * num2);
			size.Height = (int)((double)size.Height * num2);
		}
		return size;
	}

	private Point NextThumbnailPosition(Point curPosition)
	{
		Point result = curPosition;
		if (m_orientation == Orientation.Horizontal)
		{
			result.Y += m_thumbnailSize + m_fontHeight + 32;
			if (result.Y >= m_clientSize.Height + base.AutoScrollPosition.Y - 16)
			{
				result.X += 16 + m_thumbnailSize;
				result.Y = 16 + base.AutoScrollPosition.Y;
			}
		}
		else
		{
			result.X += m_thumbnailSize + 16;
			if (result.X >= m_clientSize.Width + base.AutoScrollPosition.X - 16)
			{
				result.X = 16 + base.AutoScrollPosition.X;
				result.Y += m_thumbnailSize + 32 + m_fontHeight;
			}
		}
		return result;
	}
}
