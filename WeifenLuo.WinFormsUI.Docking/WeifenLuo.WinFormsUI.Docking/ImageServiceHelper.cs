using System.Drawing;
using System.Drawing.Imaging;

namespace WeifenLuo.WinFormsUI.Docking;

public static class ImageServiceHelper
{
	public unsafe static Bitmap GetImage(Bitmap mask, Color glyph, Color background, Color? border = null)
	{
		int width = mask.Width;
		int height = mask.Height;
		Bitmap bitmap = new Bitmap(width, height);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			SolidBrush brush = new SolidBrush(glyph);
			graphics.FillRectangle(brush, 0, 0, width, height);
		}
		Bitmap bitmap2 = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
		Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
		BitmapData bitmapData = mask.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		BitmapData bitmapData2 = bitmap.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		BitmapData bitmapData3 = bitmap2.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
		for (int i = 0; i < bitmap.Height; i++)
		{
			byte* ptr = (byte*)(void*)bitmapData.Scan0 + i * bitmapData.Stride;
			byte* ptr2 = (byte*)(void*)bitmapData2.Scan0 + i * bitmapData2.Stride;
			byte* ptr3 = (byte*)(void*)bitmapData3.Scan0 + i * bitmapData3.Stride;
			for (int j = 0; j < bitmap.Width; j++)
			{
				ptr3[4 * j] = ptr2[4 * j];
				ptr3[4 * j + 1] = ptr2[4 * j + 1];
				ptr3[4 * j + 2] = ptr2[4 * j + 2];
				ptr3[4 * j + 3] = ptr[4 * j];
			}
		}
		mask.UnlockBits(bitmapData);
		bitmap.UnlockBits(bitmapData2);
		bitmap2.UnlockBits(bitmapData3);
		bitmap.Dispose();
		if (!border.HasValue)
		{
			border = background;
		}
		Bitmap bitmap3 = new Bitmap(width, height);
		using (Graphics graphics2 = Graphics.FromImage(bitmap3))
		{
			SolidBrush brush2 = new SolidBrush(background);
			SolidBrush brush3 = new SolidBrush(border.Value);
			graphics2.FillRectangle(brush3, 0, 0, width, height);
			if (background != border.Value)
			{
				graphics2.FillRectangle(brush2, 1, 1, width - 2, height - 2);
			}
			graphics2.DrawImageUnscaled(bitmap2, 0, 0);
		}
		bitmap2.Dispose();
		return bitmap3;
	}

	public static Bitmap GetBackground(Color innerBorder, Color outerBorder, int width, IPaintingService painting)
	{
		Bitmap bitmap = new Bitmap(width, width);
		using Graphics graphics = Graphics.FromImage(bitmap);
		SolidBrush brush = painting.GetBrush(innerBorder);
		SolidBrush brush2 = painting.GetBrush(outerBorder);
		graphics.FillRectangle(brush2, 0, 0, width, width);
		graphics.FillRectangle(brush, 1, 1, width - 2, width - 2);
		return bitmap;
	}

	public static Bitmap GetLayerImage(Color color, int width, IPaintingService painting)
	{
		Bitmap bitmap = new Bitmap(width, width);
		using Graphics graphics = Graphics.FromImage(bitmap);
		SolidBrush brush = painting.GetBrush(color);
		graphics.FillRectangle(brush, 0, 0, width, width);
		return bitmap;
	}

	public static Bitmap GetDockIcon(Bitmap maskArrow, Bitmap layerArrow, Bitmap maskWindow, Bitmap layerWindow, Bitmap maskBack, Color background, IPaintingService painting, Bitmap maskCore = null, Bitmap layerCore = null, Color? separator = null)
	{
		int width = maskBack.Width;
		int height = maskBack.Height;
		new Rectangle(0, 0, width, height);
		Bitmap bitmap = null;
		if (maskArrow != null)
		{
			bitmap = MaskImages(layerArrow, maskArrow);
		}
		Bitmap bitmap2 = MaskImages(layerWindow, maskWindow);
		Bitmap bitmap3 = null;
		if (layerCore != null)
		{
			bitmap3 = MaskImages(layerCore, maskCore);
		}
		Bitmap bitmap4 = new Bitmap(width, height);
		using (Graphics graphics = Graphics.FromImage(bitmap4))
		{
			SolidBrush brush = painting.GetBrush(background);
			graphics.FillRectangle(brush, 0, 0, width, height);
			graphics.DrawImageUnscaled(bitmap2, 0, 0);
			bitmap2.Dispose();
			if (layerCore != null)
			{
				graphics.DrawImageUnscaled(bitmap3, 0, 0);
				bitmap3.Dispose();
			}
			if (separator.HasValue)
			{
				Pen pen = painting.GetPen(separator.Value);
				graphics.DrawRectangle(pen, 0, 0, width - 1, height - 1);
			}
		}
		Bitmap bitmap5 = MaskImages(bitmap4, maskBack);
		bitmap4.Dispose();
		using (Graphics graphics2 = Graphics.FromImage(bitmap5))
		{
			if (bitmap != null)
			{
				graphics2.DrawImageUnscaled(bitmap, 0, 0);
				bitmap.Dispose();
			}
		}
		return bitmap5;
	}

	public unsafe static Bitmap MaskImages(Bitmap input, Bitmap maskArrow)
	{
		int width = input.Width;
		int height = input.Height;
		Rectangle rect = new Rectangle(0, 0, width, height);
		Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
		BitmapData bitmapData = maskArrow.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		BitmapData bitmapData2 = input.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
		BitmapData bitmapData3 = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
		for (int i = 0; i < height; i++)
		{
			byte* ptr = (byte*)(void*)bitmapData.Scan0 + i * bitmapData.Stride;
			byte* ptr2 = (byte*)(void*)bitmapData2.Scan0 + i * bitmapData2.Stride;
			byte* ptr3 = (byte*)(void*)bitmapData3.Scan0 + i * bitmapData3.Stride;
			for (int j = 0; j < width; j++)
			{
				ptr3[4 * j] = ptr2[4 * j];
				ptr3[4 * j + 1] = ptr2[4 * j + 1];
				ptr3[4 * j + 2] = ptr2[4 * j + 2];
				ptr3[4 * j + 3] = ptr[4 * j];
			}
		}
		maskArrow.UnlockBits(bitmapData);
		input.UnlockBits(bitmapData2);
		bitmap.UnlockBits(bitmapData3);
		return bitmap;
	}

	public static Bitmap GetDockImage(Bitmap icon, Bitmap background)
	{
		Bitmap bitmap = new Bitmap(background);
		int num = (background.Width - icon.Width) / 2;
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.DrawImage(icon, num, num);
		return bitmap;
	}

	public static Bitmap CombineFive(Bitmap five, Bitmap bottom, Bitmap center, Bitmap left, Bitmap right, Bitmap top)
	{
		Bitmap bitmap = new Bitmap(five);
		int num = (bitmap.Width - bottom.Width) / 2;
		int num2 = (num - bottom.Width) / 2;
		using Graphics graphics = Graphics.FromImage(bitmap);
		graphics.DrawImageUnscaled(top, num, num2);
		graphics.DrawImageUnscaled(center, num, num);
		graphics.DrawImageUnscaled(bottom, num, 2 * num - num2);
		graphics.DrawImageUnscaled(left, num2, num);
		graphics.DrawImageUnscaled(right, 2 * num - num2, num);
		return bitmap;
	}

	public static Bitmap GetFiveBackground(Bitmap mask, Color innerBorder, Color outerBorder, IPaintingService painting)
	{
		using Bitmap bitmap = GetLayerImage(innerBorder, mask.Width, painting);
		using (Graphics graphics = Graphics.FromImage(bitmap))
		{
			Pen pen = painting.GetPen(outerBorder);
			graphics.DrawLines(pen, new Point[4]
			{
				new Point(36, 25),
				new Point(36, 0),
				new Point(75, 0),
				new Point(75, 25)
			});
			graphics.DrawLines(pen, new Point[4]
			{
				new Point(86, 36),
				new Point(111, 36),
				new Point(111, 75),
				new Point(86, 75)
			});
			graphics.DrawLines(pen, new Point[4]
			{
				new Point(75, 86),
				new Point(75, 111),
				new Point(36, 111),
				new Point(36, 86)
			});
			graphics.DrawLines(pen, new Point[4]
			{
				new Point(25, 75),
				new Point(0, 75),
				new Point(0, 36),
				new Point(25, 36)
			});
			Pen pen2 = painting.GetPen(outerBorder, 2);
			graphics.DrawLine(pen2, new Point(36, 25), new Point(25, 36));
			graphics.DrawLine(pen2, new Point(75, 25), new Point(86, 36));
			graphics.DrawLine(pen2, new Point(86, 75), new Point(75, 86));
			graphics.DrawLine(pen2, new Point(36, 86), new Point(25, 75));
		}
		return MaskImages(bitmap, mask);
	}
}
