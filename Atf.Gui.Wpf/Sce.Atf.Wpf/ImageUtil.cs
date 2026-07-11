using System;
using System.Drawing;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf;

public static class ImageUtil
{
	public static BitmapFrame RenderToBitmapFrame(this UIElement visual, double scale)
	{
		return BitmapFrame.Create(visual.RenderToBitmapSource(scale));
	}

	public static BitmapSource RenderToBitmapSource(this UIElement visual, double scale)
	{
		Matrix transformToDevice = PresentationSource.FromVisual(visual).CompositionTarget.TransformToDevice;
		int pixelHeight = (int)(visual.RenderSize.Height * scale);
		int pixelWidth = (int)(visual.RenderSize.Width * scale);
		RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, 96.0, 96.0, PixelFormats.Pbgra32);
		VisualBrush brush = new VisualBrush(visual);
		DrawingVisual drawingVisual = new DrawingVisual();
		using (DrawingContext drawingContext = drawingVisual.RenderOpen())
		{
			drawingContext.PushTransform(new ScaleTransform(scale, scale));
			drawingContext.DrawRectangle(brush, null, new Rect(new System.Windows.Point(0.0, 0.0), new System.Windows.Point(visual.RenderSize.Width, visual.RenderSize.Height)));
		}
		renderTargetBitmap.Render(drawingVisual);
		renderTargetBitmap.Freeze();
		return renderTargetBitmap;
	}

	public static ImageSource CreateFromFile(string path)
	{
		Requires.NotNull(path, "Invalid path");
		BitmapImage bitmapImage = new BitmapImage();
		bitmapImage.BeginInit();
		bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
		try
		{
			bitmapImage.StreamSource = new FileStream(path, FileMode.Open, FileAccess.Read);
			bitmapImage.EndInit();
			bitmapImage.StreamSource.Dispose();
		}
		catch (NotSupportedException)
		{
			return null;
		}
		catch (FileFormatException)
		{
			return null;
		}
		catch (IOException)
		{
			return null;
		}
		catch (SecurityException)
		{
			return null;
		}
		catch (UnauthorizedAccessException)
		{
			return null;
		}
		return bitmapImage;
	}

	public static void RenderToBmp(this UIElement visual, Stream stream, double scale)
	{
		BitmapFrame item = visual.RenderToBitmapFrame(scale);
		BmpBitmapEncoder bmpBitmapEncoder = new BmpBitmapEncoder();
		bmpBitmapEncoder.Frames.Add(item);
		bmpBitmapEncoder.Save(stream);
	}

	public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
	{
		MemoryStream stream = new MemoryStream();
		BitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
		bitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapsource));
		bitmapEncoder.Save(stream);
		return new Bitmap(stream);
	}

	public static BitmapSource BitmapToSource(Bitmap bitmap)
	{
		IntPtr hbitmap = bitmap.GetHbitmap();
		BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();
		BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(hbitmap, IntPtr.Zero, Int32Rect.Empty, sizeOptions);
		bitmapSource.Freeze();
		return bitmapSource;
	}

	public static void RenderToJpeg(this UIElement visual, Stream stream)
	{
		visual.RenderToJpeg(stream, 1.0, 100);
	}

	public static void RenderToJpeg(this UIElement visual, Stream stream, double scale)
	{
		visual.RenderToJpeg(stream, scale, 75);
	}

	public static void RenderToJpeg(this UIElement visual, Stream stream, double scale, int quality)
	{
		BitmapFrame item = visual.RenderToBitmapFrame(scale);
		JpegBitmapEncoder jpegBitmapEncoder = new JpegBitmapEncoder();
		jpegBitmapEncoder.QualityLevel = quality;
		jpegBitmapEncoder.Frames.Add(item);
		jpegBitmapEncoder.Save(stream);
	}

	public static BitmapSource CaptureWindow(Window w)
	{
		return w.RenderToBitmapSource(1.0);
	}

	public static Bitmap Capture(Window w)
	{
		IntPtr handle = new WindowInteropHelper(w).Handle;
		IntPtr dC = User32.GetDC(handle);
		if (dC != IntPtr.Zero)
		{
			User32.RECT rect = default(User32.RECT);
			User32.GetWindowRect(handle, ref rect);
			int width = rect.Right - rect.Left;
			int height = rect.Bottom - rect.Top;
			Bitmap bitmap = new Bitmap(width, height);
			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				User32.PrintWindow(handle, graphics.GetHdc(), 0);
			}
			return bitmap;
		}
		return null;
	}
}
