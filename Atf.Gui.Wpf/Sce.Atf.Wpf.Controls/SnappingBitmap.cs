using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf.Controls;

public class SnappingBitmap : FrameworkElement
{
	public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(BitmapSource), typeof(SnappingBitmap), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnSourceChanged));

	private EventHandler m_sourceDownloaded;

	private EventHandler<ExceptionEventArgs> m_sourceFailed;

	private Point m_pixelOffset;

	public BitmapSource Source
	{
		get
		{
			return (BitmapSource)GetValue(SourceProperty);
		}
		set
		{
			SetValue(SourceProperty, value);
		}
	}

	public event EventHandler<ExceptionEventArgs> BitmapFailed;

	public SnappingBitmap()
	{
		m_sourceDownloaded = OnSourceDownloaded;
		m_sourceFailed = OnSourceFailed;
		base.LayoutUpdated += OnLayoutUpdated;
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		Size result = default(Size);
		BitmapSource source = Source;
		if (source != null)
		{
			PresentationSource presentationSource = PresentationSource.FromVisual(this);
			if (presentationSource != null)
			{
				Matrix transformFromDevice = presentationSource.CompositionTarget.TransformFromDevice;
				Vector vector = new Vector(source.PixelWidth, source.PixelHeight);
				Vector vector2 = transformFromDevice.Transform(vector);
				result = new Size(vector2.X, vector2.Y);
			}
		}
		return result;
	}

	protected override void OnRender(DrawingContext dc)
	{
		BitmapSource source = Source;
		if (source != null)
		{
			m_pixelOffset = GetPixelOffset();
			Size size = new Size(base.DesiredSize.Width - base.Margin.Left - base.Margin.Right, base.DesiredSize.Height - base.Margin.Top - base.Margin.Bottom);
			dc.DrawImage(source, new Rect(m_pixelOffset, size));
		}
	}

	private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		SnappingBitmap snappingBitmap = (SnappingBitmap)d;
		BitmapSource bitmapSource = (BitmapSource)e.OldValue;
		BitmapSource bitmapSource2 = (BitmapSource)e.NewValue;
		if (bitmapSource != null && snappingBitmap.m_sourceDownloaded != null && !bitmapSource.IsFrozen)
		{
			bitmapSource.DownloadCompleted -= snappingBitmap.m_sourceDownloaded;
			bitmapSource.DownloadFailed -= snappingBitmap.m_sourceFailed;
		}
		if (bitmapSource2 != null && !bitmapSource2.IsFrozen)
		{
			bitmapSource2.DownloadCompleted += snappingBitmap.m_sourceDownloaded;
			bitmapSource2.DownloadFailed += snappingBitmap.m_sourceFailed;
		}
	}

	private void OnSourceDownloaded(object sender, EventArgs e)
	{
		InvalidateMeasure();
		InvalidateVisual();
	}

	private void OnSourceFailed(object sender, ExceptionEventArgs e)
	{
		Source = null;
		this.BitmapFailed(this, e);
	}

	private void OnLayoutUpdated(object sender, EventArgs e)
	{
		if (base.ActualHeight != 0.0 && base.ActualWidth != 0.0)
		{
			Point pixelOffset = GetPixelOffset();
			if (!AreClose(pixelOffset, m_pixelOffset))
			{
				InvalidateVisual();
			}
		}
	}

	private Matrix GetVisualTransform(Visual v)
	{
		if (v != null)
		{
			Matrix matrix = Matrix.Identity;
			Transform transform = VisualTreeHelper.GetTransform(v);
			if (transform != null)
			{
				Matrix value = transform.Value;
				matrix = Matrix.Multiply(matrix, value);
			}
			Vector offset = VisualTreeHelper.GetOffset(v);
			matrix.Translate(offset.X, offset.Y);
			return matrix;
		}
		return Matrix.Identity;
	}

	private Point TryApplyVisualTransform(Point point, Visual v, bool inverse, bool throwOnError, out bool success)
	{
		success = true;
		if (v != null)
		{
			Matrix visualTransform = GetVisualTransform(v);
			if (inverse)
			{
				if (!throwOnError && !visualTransform.HasInverse)
				{
					success = false;
					return new Point(0.0, 0.0);
				}
				visualTransform.Invert();
			}
			point = visualTransform.Transform(point);
		}
		return point;
	}

	private Point ApplyVisualTransform(Point point, Visual v, bool inverse)
	{
		bool success = true;
		return TryApplyVisualTransform(point, v, inverse, throwOnError: true, out success);
	}

	private Point GetPixelOffset()
	{
		Point point = default(Point);
		PresentationSource presentationSource = PresentationSource.FromVisual(this);
		if (presentationSource != null)
		{
			Visual rootVisual = presentationSource.RootVisual;
			point = TransformToAncestor(rootVisual).Transform(point);
			point = ApplyVisualTransform(point, rootVisual, inverse: false);
			point = presentationSource.CompositionTarget.TransformToDevice.Transform(point);
			point.X = Math.Round(point.X);
			point.Y = Math.Round(point.Y);
			point = presentationSource.CompositionTarget.TransformFromDevice.Transform(point);
			point = ApplyVisualTransform(point, rootVisual, inverse: true);
			return rootVisual.TransformToDescendant(this).Transform(point);
		}
		return point;
	}

	private bool AreClose(Point point1, Point point2)
	{
		return AreClose(point1.X, point2.X) && AreClose(point1.Y, point2.Y);
	}

	private bool AreClose(double value1, double value2)
	{
		if (value1 == value2)
		{
			return true;
		}
		double num = value1 - value2;
		return num < 1.53E-06 && num > -1.53E-06;
	}
}
