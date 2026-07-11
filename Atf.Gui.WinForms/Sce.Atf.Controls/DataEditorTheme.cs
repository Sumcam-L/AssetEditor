using System;
using System.Drawing;
using System.Windows.Forms;

namespace Sce.Atf.Controls;

public class DataEditorTheme : IDisposable
{
	private Font m_font;

	private Brush m_textBrush;

	private Brush m_readonlyBrush;

	private Brush m_fillBrush;

	private SolidBrush m_solidBrush;

	private Pen m_sliderTrackPen = new Pen(Color.Silver);

	private int m__defaultSliderWidth;

	private Padding m_padding;

	private bool m_disposed;

	public Font Font
	{
		get
		{
			return m_font;
		}
		set
		{
			SetDisposableField(value, ref m_font);
		}
	}

	public Brush TextBrush
	{
		get
		{
			return m_textBrush;
		}
		set
		{
			SetDisposableField(value, ref m_textBrush);
		}
	}

	public Brush ReadonlyBrush
	{
		get
		{
			return m_readonlyBrush;
		}
		set
		{
			SetDisposableField(value, ref m_readonlyBrush);
		}
	}

	public Brush FillBrush
	{
		get
		{
			return m_fillBrush;
		}
		set
		{
			SetDisposableField(value, ref m_fillBrush);
		}
	}

	public SolidBrush SolidBrush
	{
		get
		{
			return m_solidBrush;
		}
		set
		{
			SetDisposableField(value, ref m_solidBrush);
		}
	}

	public Pen SliderTrackPen
	{
		get
		{
			return m_sliderTrackPen;
		}
		set
		{
			SetDisposableField(value, ref m_sliderTrackPen);
		}
	}

	public int DefaultSliderWidth
	{
		get
		{
			return m__defaultSliderWidth;
		}
		set
		{
			m__defaultSliderWidth = value;
		}
	}

	public Padding Padding
	{
		get
		{
			return m_padding;
		}
		set
		{
			m_padding = value;
		}
	}

	public DataEditorTheme(Font font)
	{
		m_font = font;
		m_textBrush = new SolidBrush(SystemColors.WindowText);
		m_readonlyBrush = new SolidBrush(SystemColors.GrayText);
		m_sliderTrackPen = new Pen(Color.Silver);
		m_padding = new Padding(8);
		m__defaultSliderWidth = 100;
		m_fillBrush = new SolidBrush(SystemColors.Window);
		m_solidBrush = new SolidBrush(Color.MediumBlue);
	}

	private void SetDisposableField<T>(T value, ref T field) where T : class, IDisposable
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (field != value)
		{
			field.Dispose();
			field = value;
		}
	}

	~DataEditorTheme()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!m_disposed)
		{
			m_textBrush.Dispose();
			m_readonlyBrush.Dispose();
			m_sliderTrackPen.Dispose();
			m_fillBrush.Dispose();
			m_solidBrush.Dispose();
			m_disposed = true;
		}
	}
}
