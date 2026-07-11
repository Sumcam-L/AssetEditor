using System;
using SharpDX.Direct2D1;

namespace Sce.Atf.Direct2D;

public abstract class D2dBrush : D2dResource
{
	private readonly D2dGraphics m_owner;

	private uint m_rtNumber;

	public float Opacity
	{
		get
		{
			return NativeBrush.Opacity;
		}
		set
		{
			NativeBrush.Opacity = value;
		}
	}

	public D2dGraphics Owner => m_owner;

	protected internal Brush NativeBrush { get; internal set; }

	protected override void Dispose(bool disposing)
	{
		if (!base.IsDisposed)
		{
			if (disposing)
			{
				m_owner.RecreateResources -= RecreateResources;
				NativeBrush.Dispose();
			}
			base.Dispose(disposing);
		}
	}

	internal virtual void Create()
	{
	}

	internal D2dBrush(D2dGraphics owner)
	{
		m_owner = owner;
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
			Create();
			m_rtNumber = m_owner.RenderTargetNumber;
		}
	}
}
