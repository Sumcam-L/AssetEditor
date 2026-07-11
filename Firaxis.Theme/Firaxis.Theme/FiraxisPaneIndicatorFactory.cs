using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisPaneIndicatorFactory : DockPanelExtender.IPaneIndicatorFactory
{
	private class FiraxisPaneIndicator : PictureBox, DockPanel.IPaneIndicator, DockPanel.IHitTest
	{
		private Bitmap _bitmapPaneDiamond;

		private Bitmap _bitmapPaneDiamondLeft;

		private Bitmap _bitmapPaneDiamondRight;

		private Bitmap _bitmapPaneDiamondTop;

		private Bitmap _bitmapPaneDiamondBottom;

		private Bitmap _bitmapPaneDiamondFill;

		private Bitmap _bitmapPaneDiamondHotSpot;

		private Bitmap _bitmapPaneDiamondHotSpotIndex;

		private static DockPanel.HotSpotIndex[] _hotSpots = new DockPanel.HotSpotIndex[5]
		{
			new DockPanel.HotSpotIndex(1, 0, DockStyle.Top),
			new DockPanel.HotSpotIndex(0, 1, DockStyle.Left),
			new DockPanel.HotSpotIndex(1, 1, DockStyle.Fill),
			new DockPanel.HotSpotIndex(2, 1, DockStyle.Right),
			new DockPanel.HotSpotIndex(1, 2, DockStyle.Bottom)
		};

		private GraphicsPath _displayingGraphicsPath;

		private DockStyle m_status = DockStyle.None;

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

		public FiraxisPaneIndicator(ThemeBase theme)
		{
			_bitmapPaneDiamond = theme.ImageService.Dockindicator_PaneDiamond;
			_bitmapPaneDiamondLeft = theme.ImageService.Dockindicator_PaneDiamond_Fill;
			_bitmapPaneDiamondRight = theme.ImageService.Dockindicator_PaneDiamond_Fill;
			_bitmapPaneDiamondTop = theme.ImageService.Dockindicator_PaneDiamond_Fill;
			_bitmapPaneDiamondBottom = theme.ImageService.Dockindicator_PaneDiamond_Fill;
			_bitmapPaneDiamondFill = theme.ImageService.Dockindicator_PaneDiamond_Fill;
			_bitmapPaneDiamondHotSpot = theme.ImageService.Dockindicator_PaneDiamond_Hotspot;
			_bitmapPaneDiamondHotSpotIndex = theme.ImageService.DockIndicator_PaneDiamond_HotspotIndex;
			_displayingGraphicsPath = DrawHelper.CalculateGraphicsPathFromBitmap(_bitmapPaneDiamond);
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

	public DockPanel.IPaneIndicator CreatePaneIndicator(ThemeBase theme)
	{
		return new FiraxisPaneIndicator(theme);
	}
}
