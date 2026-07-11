using System.Drawing;
using SharpDX;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

public class D2dBitmapBrush : D2dBrush
{
	private D2dBitmap m_bitmap;

	private D2dExtendMode m_extendModeX;

	private D2dExtendMode m_extendModeY;

	private D2dBitmapInterpolationMode m_interpolationMode;

	private PointF m_location;

	public D2dBitmap Bitmap
	{
		get
		{
			return m_bitmap;
		}
		set
		{
			m_bitmap = value;
			BitmapBrush bitmapBrush = (BitmapBrush)base.NativeBrush;
			bitmapBrush.Bitmap = m_bitmap.NativeBitmap;
		}
	}

	public D2dExtendMode ExtendModeX
	{
		get
		{
			return m_extendModeX;
		}
		set
		{
			m_extendModeX = value;
			BitmapBrush bitmapBrush = (BitmapBrush)base.NativeBrush;
			bitmapBrush.ExtendModeX = (ExtendMode)value;
		}
	}

	public D2dExtendMode ExtendModeY
	{
		get
		{
			return m_extendModeY;
		}
		set
		{
			m_extendModeY = value;
			BitmapBrush bitmapBrush = (BitmapBrush)base.NativeBrush;
			bitmapBrush.ExtendModeY = (ExtendMode)value;
		}
	}

	public D2dBitmapInterpolationMode InterpolationMode
	{
		get
		{
			return m_interpolationMode;
		}
		set
		{
			m_interpolationMode = value;
			BitmapBrush bitmapBrush = (BitmapBrush)base.NativeBrush;
			bitmapBrush.InterpolationMode = (BitmapInterpolationMode)value;
		}
	}

	public PointF Location
	{
		get
		{
			return m_location;
		}
		set
		{
			m_location = value;
			BitmapBrush bitmapBrush = (BitmapBrush)base.NativeBrush;
			Matrix3x2 identity = Matrix3x2.Identity;
			identity.M31 = value.X;
			identity.M32 = value.Y;
			bitmapBrush.Transform = identity;
		}
	}

	internal D2dBitmapBrush(D2dGraphics owner, D2dBitmap bitmap)
		: base(owner)
	{
		m_bitmap = bitmap;
		m_extendModeX = D2dExtendMode.Clamp;
		m_extendModeY = D2dExtendMode.Clamp;
		m_location = new PointF(1f, 1f);
		m_interpolationMode = D2dBitmapInterpolationMode.Linear;
		Create();
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed)
		{
			base.Dispose(disposing);
		}
	}

	internal override void Create()
	{
		if (base.NativeBrush != null)
		{
			base.NativeBrush.Dispose();
		}
		m_bitmap.Create();
		base.NativeBrush = new BitmapBrush(bitmapBrushProperties: new BitmapBrushProperties
		{
			InterpolationMode = BitmapInterpolationMode.Linear
		}, renderTarget: base.Owner.D2dRenderTarget, bitmap: m_bitmap.NativeBitmap);
		ExtendModeX = m_extendModeX;
		ExtendModeY = m_extendModeY;
		InterpolationMode = D2dBitmapInterpolationMode.Linear;
		Location = m_location;
		Bitmap = m_bitmap;
	}
}
