using System;
using System.Drawing;
using System.Drawing.Imaging;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;

namespace Sce.Atf.Direct2D;

public class D2dBitmap : D2dResource
{
	private SharpDX.Direct2D1.Bitmap m_nativeBitmap;

	private System.Drawing.Bitmap m_bitmap;

	private readonly D2dGraphics m_owner;

	private uint m_rtNumber;

	public Size PixelSize => new Size(m_nativeBitmap.PixelSize.Width, m_nativeBitmap.PixelSize.Height);

	public SizeF Size => new SizeF(m_nativeBitmap.Size.Width, m_nativeBitmap.Size.Height);

	public System.Drawing.Bitmap GdiBitmap => m_bitmap;

	internal SharpDX.Direct2D1.Bitmap NativeBitmap => m_nativeBitmap;

	public void Update()
	{
		if (m_bitmap != null)
		{
			BitmapData bitmapData = m_bitmap.LockBits(new System.Drawing.Rectangle(0, 0, m_bitmap.Width, m_bitmap.Height), ImageLockMode.ReadOnly, m_bitmap.PixelFormat);
			m_nativeBitmap.CopyFromMemory(bitmapData.Scan0, bitmapData.Stride);
			m_bitmap.UnlockBits(bitmapData);
		}
	}

	public void CopyFromMemory(byte[] bytes, int stride)
	{
		m_nativeBitmap.CopyFromMemory(bytes, stride);
	}

	internal D2dBitmap(D2dGraphics owner, SharpDX.Direct2D1.Bitmap bmp)
	{
		m_nativeBitmap = bmp;
		m_owner = owner;
		m_bitmap = null;
		m_owner.RecreateResources += RecreateResources;
		m_rtNumber = owner.RenderTargetNumber;
	}

	internal D2dBitmap(D2dGraphics owner, System.Drawing.Bitmap bmp)
	{
		if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppPArgb)
		{
			throw new ArgumentException("pixel format must be GdiPixelFormat.Format32bppPArgb");
		}
		m_owner = owner;
		m_bitmap = bmp;
		Create();
		m_owner.RecreateResources += RecreateResources;
		m_rtNumber = owner.RenderTargetNumber;
	}

	private void RecreateResources(object sender, EventArgs e)
	{
		if (base.IsDisposed)
		{
			m_owner.RecreateResources -= RecreateResources;
		}
		else if (m_rtNumber != m_owner.RenderTargetNumber)
		{
			if (m_bitmap == null)
			{
				Dispose();
				return;
			}
			Create();
			m_rtNumber = m_owner.RenderTargetNumber;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed)
		{
			m_owner.RecreateResources -= RecreateResources;
			if (m_nativeBitmap != null)
			{
				m_nativeBitmap.Dispose();
				m_nativeBitmap = null;
			}
			if (m_bitmap != null)
			{
				m_bitmap.Dispose();
				m_bitmap = null;
			}
			base.Dispose(disposing);
		}
	}

	internal void Create()
	{
		if (m_bitmap != null)
		{
			if (m_nativeBitmap != null)
			{
				m_nativeBitmap.Dispose();
				m_nativeBitmap = null;
			}
			m_nativeBitmap = new SharpDX.Direct2D1.Bitmap(bitmapProperties: new BitmapProperties
			{
				DpiX = m_bitmap.HorizontalResolution,
				DpiY = m_bitmap.VerticalResolution,
				PixelFormat = new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)
			}, renderTarget: m_owner.D2dRenderTarget, size: new Size2(m_bitmap.Width, m_bitmap.Height));
			Update();
		}
	}
}
