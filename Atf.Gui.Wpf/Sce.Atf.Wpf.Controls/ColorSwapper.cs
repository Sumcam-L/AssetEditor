using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf.Controls;

public static class ColorSwapper
{
	public static Brush SwapColors(Brush brush, ColorCallback colorCallback)
	{
		if (colorCallback == null)
		{
			throw new ArgumentNullException("colorCallback");
		}
		Brush brush2 = brush;
		if (brush != null)
		{
			brush2 = brush.Clone();
			SwapColorsWithoutCloning(brush2, colorCallback);
			brush2.Freeze();
		}
		return brush2;
	}

	public static Drawing SwapColors(Drawing drawing, ColorCallback colorCallback)
	{
		if (colorCallback == null)
		{
			throw new ArgumentNullException("colorCallback");
		}
		Drawing drawing2 = drawing;
		if (drawing != null)
		{
			drawing2 = drawing.Clone();
			SwapColorsWithoutCloning(drawing2, colorCallback);
			drawing2.Freeze();
		}
		return drawing2;
	}

	public static ImageSource SwapColors(ImageSource imageSource, ColorCallback colorCallback)
	{
		if (colorCallback == null)
		{
			throw new ArgumentNullException("colorCallback");
		}
		ImageSource result = imageSource;
		if (imageSource == null)
		{
			return result;
		}
		if (imageSource is DrawingImage drawingImage)
		{
			DrawingImage drawingImage2;
			result = (drawingImage2 = drawingImage.Clone());
			SwapColorsWithoutCloning(drawingImage2.Drawing, colorCallback);
			result.Freeze();
			return result;
		}
		if (!(imageSource is BitmapSource bitmapSource))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "UnexpectedImageSourceType", new object[1] { imageSource.GetType().Name }));
		}
		return SwapColors(bitmapSource, colorCallback);
	}

	public static BitmapSource SwapColors(BitmapSource bitmapSource, ColorCallback colorCallback)
	{
		if (colorCallback == null)
		{
			throw new ArgumentNullException("colorCallback");
		}
		BitmapSource bitmapSource2 = bitmapSource;
		if (bitmapSource != null)
		{
			PixelFormat bgra = PixelFormats.Bgra32;
			BitmapPalette bitmapPalette = null;
			double alphaThreshold = 0.0;
			FormatConvertedBitmap formatConvertedBitmap = new FormatConvertedBitmap(bitmapSource, bgra, bitmapPalette, alphaThreshold);
			int pixelWidth = formatConvertedBitmap.PixelWidth;
			int pixelHeight = formatConvertedBitmap.PixelHeight;
			int num = 4 * pixelWidth;
			byte[] array = new byte[num * pixelHeight];
			formatConvertedBitmap.CopyPixels(array, num, 0);
			for (int i = 0; i < array.Length; i += 4)
			{
				Color color = Color.FromArgb(array[i + 3], array[i + 2], array[i + 1], array[i]);
				Color color2 = colorCallback(color);
				if (color2 != color)
				{
					array[i] = color2.B;
					array[i + 1] = color2.G;
					array[i + 2] = color2.R;
					array[i + 3] = color2.A;
				}
			}
			bitmapSource2 = BitmapSource.Create(pixelWidth, pixelHeight, formatConvertedBitmap.DpiX, formatConvertedBitmap.DpiY, bgra, bitmapPalette, array, num);
			bitmapSource2.Freeze();
		}
		return bitmapSource2;
	}

	private static void SwapColorsWithoutCloning(Brush brush, ColorCallback colorCallback)
	{
		if (brush == null)
		{
			return;
		}
		if (brush is SolidColorBrush solidColorBrush)
		{
			solidColorBrush.Color = colorCallback(solidColorBrush.Color);
			return;
		}
		if (!(brush is GradientBrush gradientBrush))
		{
			if (brush is DrawingBrush drawingBrush)
			{
				SwapColorsWithoutCloning(drawingBrush.Drawing, colorCallback);
			}
			else if (brush is ImageBrush imageBrush)
			{
				imageBrush.ImageSource = SwapColorsWithoutCloningIfPossible(imageBrush.ImageSource, colorCallback);
			}
			else if (!(brush is VisualBrush))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "UnexpectedBrushType", new object[1] { brush.GetType().Name }));
			}
			return;
		}
		foreach (GradientStop gradientStop in gradientBrush.GradientStops)
		{
			gradientStop.Color = colorCallback(gradientStop.Color);
		}
	}

	private static void SwapColorsWithoutCloning(Drawing drawing, ColorCallback colorCallback)
	{
		if (drawing == null)
		{
			return;
		}
		if (drawing is DrawingGroup drawingGroup)
		{
			for (int i = 0; i < drawingGroup.Children.Count; i++)
			{
				SwapColorsWithoutCloning(drawingGroup.Children[i], colorCallback);
			}
		}
		else if (drawing is GeometryDrawing geometryDrawing)
		{
			SwapColorsWithoutCloning(geometryDrawing.Brush, colorCallback);
			if (geometryDrawing.Pen != null)
			{
				SwapColorsWithoutCloning(geometryDrawing.Pen.Brush, colorCallback);
			}
		}
		else if (drawing is GlyphRunDrawing glyphRunDrawing)
		{
			SwapColorsWithoutCloning(glyphRunDrawing.ForegroundBrush, colorCallback);
		}
		else if (drawing is ImageDrawing imageDrawing)
		{
			imageDrawing.ImageSource = SwapColorsWithoutCloningIfPossible(imageDrawing.ImageSource, colorCallback);
		}
		else if (!(drawing is VideoDrawing))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "UnexpectedDrawingType", new object[1] { drawing.GetType().Name }));
		}
	}

	private static ImageSource SwapColorsWithoutCloningIfPossible(ImageSource imageSource, ColorCallback colorCallback)
	{
		if (imageSource == null)
		{
			return imageSource;
		}
		if (imageSource is DrawingImage drawingImage)
		{
			SwapColorsWithoutCloning(drawingImage.Drawing, colorCallback);
			return imageSource;
		}
		if (!(imageSource is BitmapSource bitmapSource))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "UnexpectedImageSourceType", new object[1] { imageSource.GetType().Name }));
		}
		return SwapColors(bitmapSource, colorCallback);
	}
}
