using System;
using System.Drawing;

namespace Sce.Atf.Controls.Adaptable.Graphs;

public class CircuitGroupPinInfo
{
	private Color m_color = Color.SandyBrown;

	private bool m_pinned;

	private bool m_visible;

	private bool m_externalConnected;

	public bool Pinned
	{
		get
		{
			return m_pinned;
		}
		set
		{
			if (m_pinned != value)
			{
				m_pinned = value;
				this.Changed.Raise(this, EventArgs.Empty);
			}
		}
	}

	public bool Visible
	{
		get
		{
			return m_visible;
		}
		set
		{
			if (m_visible != value)
			{
				m_visible = value;
				this.Changed.Raise(this, EventArgs.Empty);
			}
		}
	}

	public bool ExternalConnected
	{
		get
		{
			return m_externalConnected;
		}
		set
		{
			if (m_externalConnected != value)
			{
				m_externalConnected = value;
				this.Changed.Raise(this, EventArgs.Empty);
			}
		}
	}

	public Color Color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
		}
	}

	public static int FloatingPinNodeHeight { get; set; }

	public static int FloatingPinNodeMargin { get; set; }

	public static int FloatingPinElementMargin { get; set; }

	public static int FloatingPinBoxHeight { get; set; }

	public static int FloatingPinBoxWidth { get; set; }

	public event EventHandler Changed;

	static CircuitGroupPinInfo()
	{
		FloatingPinNodeHeight = 28;
		FloatingPinNodeMargin = 2;
		FloatingPinElementMargin = 28;
		FloatingPinBoxHeight = 15;
		FloatingPinBoxWidth = 22;
	}
}
