using System;
using System.Drawing;
using System.IO;
using Sce.Atf.VectorMath;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.WIC;

namespace Sce.Atf.Direct2D;

public static class D2dFactory
{
	private static bool m_checking;

	private static readonly SharpDX.Direct2D1.Factory s_d2dFactory;

	private static readonly SharpDX.DirectWrite.Factory s_dwFactory;

	private static readonly ImagingFactory s_wicFactory;

	private static D2dHwndGraphics s_gfx;

	private static RenderTargetProperties s_rtprops;

	public static SizeF DesktopDpi => new SizeF(s_d2dFactory.DesktopDpi.Width, s_d2dFactory.DesktopDpi.Height);

	public static uint RenderTargetNumber => (s_gfx != null) ? s_gfx.RenderTargetNumber : 0u;

	internal static SharpDX.DirectWrite.Factory NativeDwFactory => s_dwFactory;

	internal static SharpDX.Direct2D1.Factory NativeFactory => s_d2dFactory;

	internal static ImagingFactory NativeWicFactory => s_wicFactory;

	internal static RenderTargetProperties RenderTargetProperties => s_rtprops;

	public static D2dHwndGraphics CreateD2dHwndGraphics(IntPtr hwnd)
	{
		CheckForRecreateTarget();
		HwndRenderTargetProperties hwndProperties = new HwndRenderTargetProperties
		{
			Hwnd = hwnd,
			PixelSize = new Size2(16, 16),
			PresentOptions = PresentOptions.Immediately
		};
		WindowRenderTarget windowRenderTarget = null;
		if (s_rtprops.DpiX == 0f)
		{
			s_rtprops.DpiX = 96f;
			s_rtprops.DpiY = 96f;
			s_rtprops.PixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
			s_rtprops.Usage = RenderTargetUsage.GdiCompatible;
			s_rtprops.MinLevel = FeatureLevel.Level_DEFAULT;
			try
			{
				s_rtprops.Type = RenderTargetType.Hardware;
				windowRenderTarget = new WindowRenderTarget(s_d2dFactory, s_rtprops, hwndProperties);
			}
			catch
			{
				s_rtprops.Type = RenderTargetType.Software;
				windowRenderTarget = new WindowRenderTarget(s_d2dFactory, s_rtprops, hwndProperties);
			}
		}
		else
		{
			windowRenderTarget = new WindowRenderTarget(s_d2dFactory, s_rtprops, hwndProperties);
		}
		return new D2dHwndGraphics(windowRenderTarget);
	}

	public static void EnableResourceSharing(IntPtr hwnd)
	{
		if (s_gfx == null)
		{
			s_gfx = CreateD2dHwndGraphics(hwnd);
		}
	}

	public static D2dWicGraphics CreateWicGraphics(int width, int height)
	{
		SharpDX.WIC.Bitmap wicBitmap = new SharpDX.WIC.Bitmap(s_wicFactory, width, height, SharpDX.WIC.PixelFormat.Format32bppPBGRA, BitmapCreateCacheOption.CacheOnLoad);
		RenderTargetProperties renderTargetProperties = new RenderTargetProperties
		{
			Type = RenderTargetType.Default,
			DpiX = 96f,
			DpiY = 96f,
			PixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.Unknown, AlphaMode.Unknown),
			Usage = RenderTargetUsage.None,
			MinLevel = FeatureLevel.Level_DEFAULT
		};
		WicRenderTarget renderTarget = new WicRenderTarget(s_d2dFactory, wicBitmap, renderTargetProperties);
		return new D2dWicGraphics(renderTarget, wicBitmap);
	}

	public static D2dStrokeStyle CreateD2dStrokeStyle(D2dStrokeStyleProperties props)
	{
		return CreateD2dStrokeStyle(props, new float[0]);
	}

	public static D2dStrokeStyle CreateD2dStrokeStyle(D2dStrokeStyleProperties props, float[] dashes)
	{
		StrokeStyle strokeStyle = new StrokeStyle(properties: new StrokeStyleProperties
		{
			DashCap = (CapStyle)props.DashCap,
			DashOffset = props.DashOffset,
			DashStyle = (DashStyle)props.DashStyle,
			EndCap = (CapStyle)props.EndCap,
			LineJoin = (LineJoin)props.LineJoin,
			MiterLimit = props.MiterLimit,
			StartCap = (CapStyle)props.StartCap
		}, factory: s_d2dFactory, dashes: dashes);
		return new D2dStrokeStyle(strokeStyle);
	}

	public static D2dTextFormat CreateTextFormat(string fontFamilyName, float fontSize)
	{
		TextFormat textFormat = new TextFormat(s_dwFactory, fontFamilyName, fontSize);
		return new D2dTextFormat(textFormat);
	}

	public static D2dTextFormat CreateTextFormat(string fontFamilyName, D2dFontWeight fontWeight, D2dFontStyle fontStyle, float fontSize)
	{
		TextFormat textFormat = new TextFormat(s_dwFactory, fontFamilyName, null, (FontWeight)fontWeight, (SharpDX.DirectWrite.FontStyle)fontStyle, FontStretch.Normal, fontSize, "");
		return new D2dTextFormat(textFormat);
	}

	public static D2dTextFormat CreateTextFormat(string fontFamilyName, D2dFontWeight fontWeight, D2dFontStyle fontStyle, D2dFontStretch fontStretch, float fontSize, string localeName)
	{
		TextFormat textFormat = new TextFormat(s_dwFactory, fontFamilyName, null, (FontWeight)fontWeight, (SharpDX.DirectWrite.FontStyle)fontStyle, FontStretch.Normal, fontSize, localeName);
		return new D2dTextFormat(textFormat);
	}

	public static D2dTextFormat CreateTextFormat(System.Drawing.Font font)
	{
		float fontSize = font.SizeInPoints * 96f / 72f;
		FontWeight fontWeight = FontWeight.Normal;
		if ((font.Style & System.Drawing.FontStyle.Bold) == System.Drawing.FontStyle.Bold)
		{
			fontWeight = FontWeight.Bold;
		}
		SharpDX.DirectWrite.FontStyle fontStyle = SharpDX.DirectWrite.FontStyle.Normal;
		if ((font.Style & System.Drawing.FontStyle.Italic) == System.Drawing.FontStyle.Italic)
		{
			fontStyle = SharpDX.DirectWrite.FontStyle.Italic;
		}
		TextFormat textFormat = new TextFormat(s_dwFactory, font.FontFamily.Name, fontWeight, fontStyle, fontSize);
		return new D2dTextFormat(textFormat);
	}

	public static D2dTextLayout CreateTextLayout(string text, D2dTextFormat textFormat)
	{
		return CreateTextLayout(text, textFormat, 2048f, 2048f);
	}

	public static D2dTextLayout CreateTextLayout(string text, D2dTextFormat textFormat, float layoutWidth, float layoutHeight)
	{
		TextLayout textLayout = new TextLayout(s_dwFactory, text, textFormat.NativeTextFormat, layoutWidth, layoutHeight);
		return new D2dTextLayout(text, textLayout);
	}

	public static D2dTextLayout CreateTextLayout(string text, D2dTextFormat textFormat, Matrix3x2F transform)
	{
		return CreateTextLayout(text, textFormat, 2048f, 2048f, transform);
	}

	public static D2dTextLayout CreateTextLayout(string text, D2dTextFormat textFormat, float layoutWidth, float layoutHeight, Matrix3x2F transform)
	{
		Matrix3x2 value = new Matrix3x2
		{
			M11 = transform.M11,
			M12 = transform.M12,
			M21 = transform.M21,
			M22 = transform.M22,
			M31 = transform.DX,
			M32 = transform.DY
		};
		TextLayout textLayout = new TextLayout(s_dwFactory, text, textFormat.NativeTextFormat, layoutWidth, layoutHeight, 1f, value, useGdiNatural: true);
		return new D2dTextLayout(text, textLayout);
	}

	public static SizeF MeasureText(string text, D2dTextFormat textFormat)
	{
		if (string.IsNullOrEmpty(text))
		{
			return SizeF.Empty;
		}
		return s_gfx.MeasureText(text, textFormat);
	}

	public static SizeF MeasureText(string text, D2dTextFormat textFormat, SizeF maxSize)
	{
		if (string.IsNullOrEmpty(text))
		{
			return SizeF.Empty;
		}
		return s_gfx.MeasureText(text, textFormat, maxSize);
	}

	public static D2dSolidColorBrush CreateSolidBrush(System.Drawing.Color color)
	{
		return s_gfx.CreateSolidBrush(color);
	}

	public static D2dLinearGradientBrush CreateLinearGradientBrush(params D2dGradientStop[] gradientStops)
	{
		return s_gfx.CreateLinearGradientBrush(gradientStops);
	}

	public static D2dLinearGradientBrush CreateLinearGradientBrush(PointF pt1, PointF pt2, D2dGradientStop[] gradientStops, D2dExtendMode extendMode, D2dGamma gamma)
	{
		return s_gfx.CreateLinearGradientBrush(pt1, pt2, gradientStops, extendMode, gamma);
	}

	public static D2dRadialGradientBrush CreateRadialGradientBrush(params D2dGradientStop[] gradientStops)
	{
		return s_gfx.CreateRadialGradientBrush(new PointF(0f, 0f), new PointF(0f, 0f), 1f, 1f, gradientStops);
	}

	public static D2dRadialGradientBrush CreateRadialGradientBrush(PointF center, PointF gradientOriginOffset, float radiusX, float radiusY, params D2dGradientStop[] gradientStops)
	{
		return s_gfx.CreateRadialGradientBrush(center, gradientOriginOffset, radiusX, radiusY, gradientStops);
	}

	public static D2dBitmapBrush CreateBitmapBrush(D2dBitmap bitmap)
	{
		return s_gfx.CreateBitmapBrush(bitmap);
	}

	public static D2dBitmap CreateBitmap(Type type, string resource)
	{
		return s_gfx.CreateBitmap(type, resource);
	}

	public static D2dBitmap CreateBitmap(Stream stream)
	{
		return s_gfx.CreateBitmap(stream);
	}

	public static D2dBitmap CreateBitmap(string filename)
	{
		return s_gfx.CreateBitmap(filename);
	}

	public static D2dBitmap CreateBitmap(Image img)
	{
		return s_gfx.CreateBitmap(img);
	}

	public static D2dBitmap CreateBitmap(System.Drawing.Bitmap bmp)
	{
		return s_gfx.CreateBitmap(bmp);
	}

	public static D2dBitmap CreateBitmap(int width, int height)
	{
		return s_gfx.CreateBitmap(width, height);
	}

	public static float FontSizeToPixel(float point)
	{
		float num = point / 72f;
		return num * s_d2dFactory.DesktopDpi.Width;
	}

	public static void ReloadSystemMetrics()
	{
		s_d2dFactory.ReloadSystemMetrics();
	}

	internal static void CheckForRecreateTarget()
	{
		if (m_checking)
		{
			return;
		}
		try
		{
			m_checking = true;
			if (s_gfx != null)
			{
				s_gfx.BeginDraw();
				s_gfx.EndDraw();
			}
		}
		finally
		{
			m_checking = false;
		}
	}

	static D2dFactory()
	{
		s_rtprops = default(RenderTargetProperties);
		if (Environment.OSVersion.Version.Major < 6)
		{
			throw new Exception("Direct2D requires Windows Vista or newer");
		}
		s_d2dFactory = new SharpDX.Direct2D1.Factory();
		s_dwFactory = new SharpDX.DirectWrite.Factory();
		s_wicFactory = new ImagingFactory();
	}
}
