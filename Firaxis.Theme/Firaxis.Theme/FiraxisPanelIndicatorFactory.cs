using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisPanelIndicatorFactory : DockPanelExtender.IPanelIndicatorFactory
{
	private class FiraxisPanelIndicator : PictureBox, DockPanel.IPanelIndicator, DockPanel.IHitTest
	{
		private Image _imagePanelLeft;

		private Image _imagePanelRight;

		private Image _imagePanelTop;

		private Image _imagePanelBottom;

		private Image _imagePanelFill;

		private Image _imagePanelLeftActive;

		private Image _imagePanelRightActive;

		private Image _imagePanelTopActive;

		private Image _imagePanelBottomActive;

		private Image _imagePanelFillActive;

		private DockStyle m_dockStyle;

		private DockStyle m_status;

		private bool m_isActivated = false;

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

		public FiraxisPanelIndicator(DockStyle dockStyle, ThemeBase theme)
		{
			_imagePanelLeft = theme.ImageService.DockIndicator_PanelLeft;
			_imagePanelRight = theme.ImageService.DockIndicator_PanelRight;
			_imagePanelTop = theme.ImageService.DockIndicator_PanelTop;
			_imagePanelBottom = theme.ImageService.DockIndicator_PanelBottom;
			_imagePanelFill = theme.ImageService.DockIndicator_PanelFill;
			_imagePanelLeftActive = theme.ImageService.DockIndicator_PanelLeft;
			_imagePanelRightActive = theme.ImageService.DockIndicator_PanelRight;
			_imagePanelTopActive = theme.ImageService.DockIndicator_PanelTop;
			_imagePanelBottomActive = theme.ImageService.DockIndicator_PanelBottom;
			_imagePanelFillActive = theme.ImageService.DockIndicator_PanelFill;
			m_dockStyle = dockStyle;
			base.SizeMode = PictureBoxSizeMode.AutoSize;
			base.Image = ImageInactive;
		}

		public DockStyle HitTest(Point pt)
		{
			return (base.Visible && base.ClientRectangle.Contains(PointToClient(pt))) ? DockStyle : DockStyle.None;
		}
	}

	public DockPanel.IPanelIndicator CreatePanelIndicator(DockStyle style, ThemeBase theme)
	{
		return new FiraxisPanelIndicator(style, theme);
	}
}
