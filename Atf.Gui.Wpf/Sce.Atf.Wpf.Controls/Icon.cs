using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Sce.Atf.Wpf.Controls;

public class Icon : System.Windows.Controls.Image
{
	public static readonly DependencyProperty RedChromaProperty;

	public static readonly DependencyProperty BlueChromaProperty;

	public static readonly DependencyProperty GreenChromaProperty;

	public static readonly DependencyProperty SourceBrushProperty;

	public static readonly DependencyProperty SourceKeyProperty;

	public static readonly DependencyProperty UseShadowProperty;

	public static readonly DependencyProperty DeselectedDrawingBrushProperty;

	public static readonly DependencyProperty DeselectedImageProperty;

	public static readonly DependencyProperty SelectedDrawingBrushProperty;

	public static readonly DependencyProperty SelectedImageProperty;

	public static readonly DependencyProperty ShowSelectedIconOnMouseOverProperty;

	public static readonly DependencyProperty OverlayImageSourceProperty;

	public static readonly DependencyProperty OverlayRectProperty;

	public static readonly DependencyProperty ShowOverlayProperty;

	public SolidColorBrush RedChroma
	{
		get
		{
			return (SolidColorBrush)GetValue(RedChromaProperty);
		}
		set
		{
			SetValue(RedChromaProperty, value);
		}
	}

	public SolidColorBrush BlueChroma
	{
		get
		{
			return (SolidColorBrush)GetValue(BlueChromaProperty);
		}
		set
		{
			SetValue(BlueChromaProperty, value);
		}
	}

	public SolidColorBrush GreenChroma
	{
		get
		{
			return (SolidColorBrush)GetValue(GreenChromaProperty);
		}
		set
		{
			SetValue(GreenChromaProperty, value);
		}
	}

	public DrawingBrush SourceBrush
	{
		get
		{
			return (DrawingBrush)GetValue(SourceBrushProperty);
		}
		set
		{
			SetValue(SourceBrushProperty, value);
		}
	}

	public ImageSource OverlayImageSource
	{
		get
		{
			return (ImageSource)GetValue(OverlayImageSourceProperty);
		}
		set
		{
			SetValue(OverlayImageSourceProperty, value);
		}
	}

	public Rect OverlayRect
	{
		get
		{
			return (Rect)GetValue(OverlayRectProperty);
		}
		set
		{
			SetValue(OverlayRectProperty, value);
		}
	}

	public bool ShowOverlay
	{
		get
		{
			return (bool)GetValue(ShowOverlayProperty);
		}
		set
		{
			SetValue(ShowOverlayProperty, value);
		}
	}

	private ImageSource RenderSource
	{
		get
		{
			if (base.Source == null)
			{
				return null;
			}
			if (RedChroma == null && GreenChroma == null && BlueChroma == null)
			{
				return base.Source;
			}
			return ColorSwapper.SwapColors(base.Source, ConvertColor);
		}
	}

	private DrawingBrush RenderSourceBrush
	{
		get
		{
			if (SourceBrush == null)
			{
				return null;
			}
			if (RedChroma == null && GreenChroma == null && BlueChroma == null)
			{
				return SourceBrush;
			}
			return (DrawingBrush)ColorSwapper.SwapColors(SourceBrush, ConvertColor);
		}
	}

	public static SolidColorBrush GetRedChroma(DependencyObject obj)
	{
		return (SolidColorBrush)obj.GetValue(RedChromaProperty);
	}

	public static void SetRedChroma(DependencyObject obj, SolidColorBrush value)
	{
		obj.SetValue(RedChromaProperty, value);
	}

	public static SolidColorBrush GetBlueChroma(DependencyObject obj)
	{
		return (SolidColorBrush)obj.GetValue(BlueChromaProperty);
	}

	public static void SetBlueChroma(DependencyObject obj, SolidColorBrush value)
	{
		obj.SetValue(BlueChromaProperty, value);
	}

	public static void SetGreenChroma(DependencyObject obj, SolidColorBrush value)
	{
		obj.SetValue(GreenChromaProperty, value);
	}

	public static SolidColorBrush GetGreenChroma(DependencyObject obj)
	{
		return (SolidColorBrush)obj.GetValue(GreenChromaProperty);
	}

	public static void SetSourceKey(UIElement element, object value)
	{
		element.SetValue(SourceKeyProperty, value);
	}

	public static object GetSourceKey(UIElement element)
	{
		return element.GetValue(SourceKeyProperty);
	}

	public static void SetUseShadow(UIElement element, bool value)
	{
		element.SetValue(UseShadowProperty, value);
	}

	public static bool GetUseShadow(UIElement element)
	{
		return (bool)element.GetValue(UseShadowProperty);
	}

	public static void SetDeselectedDrawingBrush(DependencyObject obj, DrawingBrush value)
	{
		obj.SetValue(DeselectedDrawingBrushProperty, value);
	}

	public static DrawingBrush GetDeselectedDrawingBrush(DependencyObject obj)
	{
		return (DrawingBrush)obj.GetValue(DeselectedDrawingBrushProperty);
	}

	public static void SetSelectedImage(DependencyObject obj, ImageSource value)
	{
		obj.SetValue(SelectedImageProperty, value);
	}

	public static ImageSource GetSelectedImage(DependencyObject obj)
	{
		return (ImageSource)obj.GetValue(SelectedImageProperty);
	}

	public static void SetSelectedDrawingBrush(DependencyObject obj, DrawingBrush value)
	{
		obj.SetValue(SelectedDrawingBrushProperty, value);
	}

	public static DrawingBrush GetSelectedDrawingBrush(DependencyObject obj)
	{
		return (DrawingBrush)obj.GetValue(SelectedDrawingBrushProperty);
	}

	public static void SetDeselectedImage(DependencyObject obj, ImageSource value)
	{
		obj.SetValue(DeselectedImageProperty, value);
	}

	public static ImageSource GetDeselectedImage(DependencyObject obj)
	{
		return (ImageSource)obj.GetValue(DeselectedImageProperty);
	}

	public static void SetShowSelectedIconOnMouseOver(DependencyObject obj, bool value)
	{
		obj.SetValue(ShowSelectedIconOnMouseOverProperty, value);
	}

	public static bool GetShowSelectedIconOnMouseOver(DependencyObject obj)
	{
		return (bool)obj.GetValue(ShowSelectedIconOnMouseOverProperty);
	}

	public static System.Windows.Point GetPixelSnappingOffset(Visual visual)
	{
		PresentationSource presentationSource = PresentationSource.FromVisual(visual);
		if (presentationSource != null)
		{
			return GetPixelSnappingOffset(visual, presentationSource.RootVisual);
		}
		return default(System.Windows.Point);
	}

	protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
	{
		if (base.Source == null)
		{
			return finalSize;
		}
		return base.ArrangeOverride(finalSize);
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		BitmapSource bitmapSource = base.Source as BitmapSource;
		Rect rectangle = new Rect(0.0, 0.0, base.RenderSize.Width, base.RenderSize.Height);
		if (SourceBrush == null || (bitmapSource != null && IsClose(bitmapSource.Width, base.RenderSize.Width) && IsClose(bitmapSource.Height, base.RenderSize.Height)))
		{
			ImageSource renderSource = RenderSource;
			if (renderSource != null)
			{
				drawingContext.DrawImage(renderSource, rectangle);
			}
		}
		else
		{
			if (GetUseShadow(this))
			{
				Rect rectangle2 = new Rect(1.5, 1.5, base.RenderSize.Width, base.RenderSize.Height);
				System.Windows.Media.Brush brush = ColorSwapper.SwapColors(SourceBrush, GetShadowColor);
				drawingContext.DrawRectangle(brush, null, rectangle2);
			}
			drawingContext.DrawRectangle(RenderSourceBrush, null, rectangle);
		}
		if (ShowOverlay)
		{
			ImageSource overlayImageSource = OverlayImageSource;
			if (overlayImageSource != null)
			{
				drawingContext.DrawImage(overlayImageSource, OverlayRect);
			}
		}
	}

	static Icon()
	{
		RedChromaProperty = DependencyProperty.RegisterAttached("RedChroma", typeof(SolidColorBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		BlueChromaProperty = DependencyProperty.RegisterAttached("BlueChroma", typeof(SolidColorBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		GreenChromaProperty = DependencyProperty.RegisterAttached("GreenChroma", typeof(SolidColorBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		SourceBrushProperty = DependencyProperty.Register("SourceBrush", typeof(DrawingBrush), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		SourceKeyProperty = DependencyProperty.RegisterAttached("SourceKey", typeof(object), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, SourceKeyPropertyChanged));
		UseShadowProperty = DependencyProperty.RegisterAttached("UseShadow", typeof(bool), typeof(Icon), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
		DeselectedDrawingBrushProperty = DependencyProperty.RegisterAttached("DeselectedDrawingBrush", typeof(DrawingBrush), typeof(Icon), new PropertyMetadata(null));
		DeselectedImageProperty = DependencyProperty.RegisterAttached("DeselectedImage", typeof(ImageSource), typeof(Icon), new PropertyMetadata(null));
		SelectedDrawingBrushProperty = DependencyProperty.RegisterAttached("SelectedDrawingBrush", typeof(DrawingBrush), typeof(Icon), new PropertyMetadata(null));
		SelectedImageProperty = DependencyProperty.RegisterAttached("SelectedImage", typeof(ImageSource), typeof(Icon), new PropertyMetadata(null));
		ShowSelectedIconOnMouseOverProperty = DependencyProperty.RegisterAttached("ShowSelectedIconOnMouseOver", typeof(bool), typeof(Icon), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));
		OverlayImageSourceProperty = DependencyProperty.Register("OverlayImageSource", typeof(ImageSource), typeof(Icon), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
		OverlayRectProperty = DependencyProperty.Register("OverlayRect", typeof(Rect), typeof(Icon), new FrameworkPropertyMetadata(new Rect(0.0, 0.0, 0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender));
		ShowOverlayProperty = DependencyProperty.Register("ShowOverlay", typeof(bool), typeof(Icon), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));
		System.Windows.Controls.Image.StretchProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(Stretch.None));
	}

	private static void SourceKeyPropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
	{
		if (!(o is Icon icon))
		{
			return;
		}
		icon.Source = null;
		icon.SourceBrush = null;
		if (e.NewValue == null)
		{
			return;
		}
		object obj = icon.TryFindResource(e.NewValue);
		if (obj == null)
		{
			System.Drawing.Image image = Sce.Atf.ResourceUtil.GetImage(e.NewValue.ToString());
			if (image != null)
			{
				obj = ResourceUtil.ConvertWinFormsImage(image);
			}
		}
		if (obj is DrawingBrush sourceBrush)
		{
			icon.SourceBrush = sourceBrush;
			icon.Stretch = Stretch.None;
		}
		else if (obj is ImageSource source)
		{
			icon.Source = source;
			icon.Stretch = Stretch.Uniform;
		}
	}

	private System.Windows.Media.Color GetShadowColor(System.Windows.Media.Color c)
	{
		return System.Windows.Media.Color.FromRgb(22, 22, 22);
	}

	private static System.Windows.Point GetPixelSnappingOffset(Visual visual, Visual rootVisual)
	{
		System.Windows.Point point = default(System.Windows.Point);
		if (rootVisual != null && visual.TransformToAncestor(rootVisual) is Transform { Value: { HasInverse: not false } })
		{
			return visual.PointFromScreen(visual.PointToScreen(point));
		}
		return point;
	}

	private System.Windows.Media.Color ConvertColor(System.Windows.Media.Color color)
	{
		if (color.R != color.G || color.R != color.B)
		{
			if (color.G == color.B && RedChroma != null)
			{
				return ScaleColor(RedChroma.Color, color.R, color.G, color.A);
			}
			if (color.R == color.B && GreenChroma != null)
			{
				return ScaleColor(GreenChroma.Color, color.G, color.R, color.A);
			}
			if (color.R == color.G && BlueChroma != null)
			{
				return ScaleColor(BlueChroma.Color, color.B, color.R, color.A);
			}
		}
		return color;
	}

	private static bool IsClose(double num1, double num2)
	{
		return num1 > num2 * 0.9;
	}

	private System.Windows.Media.Color ScaleColor(System.Windows.Media.Color color, byte primary, byte white, byte alpha)
	{
		return System.Windows.Media.Color.FromArgb((byte)(alpha * color.A / 255), (byte)((double)(int)color.R / 255.0 * (double)(primary - white) + (double)(int)white), (byte)((double)(int)color.G / 255.0 * (double)(primary - white) + (double)(int)white), (byte)((double)(int)color.B / 255.0 * (double)(primary - white) + (double)(int)white));
	}
}
