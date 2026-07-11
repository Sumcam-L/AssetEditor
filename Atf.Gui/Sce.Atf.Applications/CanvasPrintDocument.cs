using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;

namespace Sce.Atf.Applications;

public abstract class CanvasPrintDocument : PrintDocument
{
	private RectangleF m_bounds;

	private Rectangle m_marginBounds;

	protected virtual bool AllowNonUniformScale => false;

	protected Rectangle MarginBounds
	{
		get
		{
			return m_marginBounds;
		}
		set
		{
			m_marginBounds = value;
		}
	}

	protected void SetDefaultPrinterSettings()
	{
		m_bounds = GetAllPagesBounds();
		Rectangle bounds = base.DefaultPageSettings.Bounds;
		Margins margins = base.DefaultPageSettings.Margins;
		Rectangle rectangle = new Rectangle(margins.Left, margins.Top, bounds.Width - (margins.Left + margins.Right), bounds.Height - (margins.Top + margins.Bottom));
		int num = (int)Math.Ceiling(m_bounds.Width / (float)rectangle.Width);
		int num2 = (int)Math.Ceiling(m_bounds.Height / (float)rectangle.Height);
		PrinterSettings obj = base.PrinterSettings;
		int minimumPage = (base.PrinterSettings.FromPage = 1);
		obj.MinimumPage = minimumPage;
		PrinterSettings obj2 = base.PrinterSettings;
		minimumPage = (base.PrinterSettings.ToPage = num * num2);
		obj2.MaximumPage = minimumPage;
	}

	protected override void OnBeginPrint(PrintEventArgs e)
	{
		switch (base.PrinterSettings.PrintRange)
		{
		case PrintRange.AllPages:
		case PrintRange.SomePages:
			m_bounds = GetAllPagesBounds();
			break;
		case PrintRange.Selection:
		{
			m_bounds = GetSelectionBounds();
			PrinterSettings obj2 = base.PrinterSettings;
			int fromPage = (base.PrinterSettings.ToPage = 1);
			obj2.FromPage = fromPage;
			break;
		}
		case PrintRange.CurrentPage:
		{
			m_bounds = GetCurrentPageBounds();
			PrinterSettings obj = base.PrinterSettings;
			int fromPage = (base.PrinterSettings.ToPage = 1);
			obj.FromPage = fromPage;
			break;
		}
		}
		base.OnBeginPrint(e);
	}

	protected override void OnPrintPage(PrintPageEventArgs e)
	{
		base.OnPrintPage(e);
		PrinterSettings printerSettings = e.PageSettings.PrinterSettings;
		MarginBounds = e.MarginBounds;
		RectangleF rectangleF = e.MarginBounds;
		RectangleF sourceBounds = m_bounds;
		bool flag = printerSettings.PrintRange == PrintRange.AllPages || printerSettings.PrintRange == PrintRange.SomePages;
		if (flag)
		{
			int num = printerSettings.FromPage - 1;
			int num2 = (int)Math.Ceiling(m_bounds.Width / rectangleF.Width);
			int num3 = 0;
			int num4 = 0;
			if (num2 > 0)
			{
				num3 = num / num2;
				num4 = num % num2;
			}
			sourceBounds = new RectangleF(m_bounds.Left + rectangleF.Width * (float)num4, m_bounds.Top + rectangleF.Height * (float)num3, rectangleF.Width, rectangleF.Height);
		}
		Matrix matrix = new Matrix();
		matrix.Translate(0f - sourceBounds.Left, 0f - sourceBounds.Top);
		if (!flag)
		{
			float num5 = rectangleF.Width / sourceBounds.Width;
			float num6 = rectangleF.Height / sourceBounds.Height;
			if (!AllowNonUniformScale)
			{
				num5 = (num6 = Math.Min(num5, num6));
			}
			matrix.Scale(num5, num6, MatrixOrder.Append);
		}
		matrix.Translate(rectangleF.Left, rectangleF.Top, MatrixOrder.Append);
		Render(sourceBounds, matrix, e.Graphics);
		if (printerSettings.FromPage < printerSettings.ToPage)
		{
			e.HasMorePages = true;
			printerSettings.FromPage++;
		}
	}

	protected abstract RectangleF GetSelectionBounds();

	protected abstract RectangleF GetAllPagesBounds();

	protected abstract RectangleF GetCurrentPageBounds();

	protected abstract void Render(RectangleF sourceBounds, Matrix transform, Graphics g);
}
