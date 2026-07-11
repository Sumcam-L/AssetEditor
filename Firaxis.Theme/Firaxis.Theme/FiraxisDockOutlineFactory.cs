using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.Theme;

internal class FiraxisDockOutlineFactory : DockPanelExtender.IDockOutlineFactory
{
	private class FiraxisLightDockOutline : DockOutlineBase
	{
		private DragForm m_dragForm;

		private DragForm DragForm => m_dragForm;

		public FiraxisLightDockOutline()
		{
			m_dragForm = new DragForm();
			SetDragForm(Rectangle.Empty);
			DragForm.BackColor = ColorTranslator.FromHtml("#FFC2C2C2");
			DragForm.BackgroundColor = ColorTranslator.FromHtml("#FF5BADFF");
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
			if (DragForm.Region != null)
			{
				DragForm.Region.Dispose();
			}
			DragForm.Region = region;
		}
	}

	public DockOutlineBase CreateDockOutline()
	{
		return new FiraxisLightDockOutline();
	}
}
