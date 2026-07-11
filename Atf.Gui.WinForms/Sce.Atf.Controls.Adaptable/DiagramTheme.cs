using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Sce.Atf.Controls.Adaptable;

[Export(typeof(DiagramTheme))]
public class DiagramTheme : IDisposable
{
	private const int DefaultPenWidth = 3;

	private bool m_disposed;

	private Font m_font;

	private int m_pickTolerance = 3;

	private Pen m_outlinePen;

	private Brush m_fillBrush;

	private Brush m_textBrush;

	private Pen m_highlightPen;

	private Brush m_highlightBrush;

	private Pen m_lastHighlightPen;

	private Brush m_lastHighlightBrush;

	private Pen m_ghostPen;

	private Brush m_ghostBrush;

	private Pen m_hiddenPen;

	private Brush m_hiddenBrush;

	private Pen m_hotPen;

	private Brush m_hotBrush;

	private Pen m_errorPen;

	private Brush m_errorBrush;

	private readonly Dictionary<object, Pen> m_pens = new Dictionary<object, Pen>();

	private readonly Dictionary<object, Brush> m_brushes = new Dictionary<object, Brush>();

	private readonly StringFormat m_leftFormat = new StringFormat();

	private readonly StringFormat m_rightFormat = new StringFormat();

	private readonly StringFormat m_centerFormat = new StringFormat();

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

	public int PickTolerance
	{
		get
		{
			return m_pickTolerance;
		}
		set
		{
			m_pickTolerance = value;
		}
	}

	public Pen OutlinePen
	{
		get
		{
			return m_outlinePen;
		}
		set
		{
			SetDisposableField(value, ref m_outlinePen);
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

	public Pen HighlightPen
	{
		get
		{
			return m_highlightPen;
		}
		set
		{
			SetDisposableField(value, ref m_highlightPen);
		}
	}

	public Brush HighlightBrush
	{
		get
		{
			return m_highlightBrush;
		}
		set
		{
			SetDisposableField(value, ref m_highlightBrush);
		}
	}

	public Pen LastHighlightPen
	{
		get
		{
			return m_lastHighlightPen;
		}
		set
		{
			SetDisposableField(value, ref m_lastHighlightPen);
		}
	}

	public Brush LastHighlightBrush
	{
		get
		{
			return m_lastHighlightBrush;
		}
		set
		{
			SetDisposableField(value, ref m_lastHighlightBrush);
		}
	}

	public Pen GhostPen
	{
		get
		{
			return m_ghostPen;
		}
		set
		{
			SetDisposableField(value, ref m_ghostPen);
		}
	}

	public Brush GhostBrush
	{
		get
		{
			return m_ghostBrush;
		}
		set
		{
			SetDisposableField(value, ref m_ghostBrush);
		}
	}

	public Pen HiddenPen
	{
		get
		{
			return m_hiddenPen;
		}
		set
		{
			SetDisposableField(value, ref m_hiddenPen);
		}
	}

	public Brush HiddenBrush
	{
		get
		{
			return m_hiddenBrush;
		}
		set
		{
			SetDisposableField(value, ref m_hiddenBrush);
		}
	}

	public Pen HotPen
	{
		get
		{
			return m_hotPen;
		}
		set
		{
			SetDisposableField(value, ref m_hotPen);
		}
	}

	public Brush HotBrush
	{
		get
		{
			return m_hotBrush;
		}
		set
		{
			SetDisposableField(value, ref m_hotBrush);
		}
	}

	public Pen ErrorPen
	{
		get
		{
			return m_errorPen;
		}
		set
		{
			SetDisposableField(value, ref m_errorPen);
		}
	}

	public Brush ErrorBrush
	{
		get
		{
			return m_errorBrush;
		}
		set
		{
			SetDisposableField(value, ref m_errorBrush);
		}
	}

	public StringFormat LeftStringFormat => m_leftFormat;

	public StringFormat RightStringFormat => m_rightFormat;

	public StringFormat CenterStringFormat => m_centerFormat;

	public event EventHandler Redraw;

	public DiagramTheme()
		: this(new Font("Microsoft Sans Serif", 8f))
	{
	}

	public DiagramTheme(Font font)
	{
		m_font = font;
		m_fillBrush = new SolidBrush(SystemColors.Window);
		m_textBrush = new SolidBrush(SystemColors.WindowText);
		m_outlinePen = new Pen(SystemColors.ControlDark, 1f);
		m_highlightPen = new Pen(SystemColors.Highlight, 3f);
		m_highlightPen.DashStyle = DashStyle.Dot;
		m_highlightBrush = new HatchBrush(HatchStyle.ForwardDiagonal, SystemColors.Highlight);
		m_lastHighlightPen = new Pen(SystemColors.Highlight, 3f);
		m_lastHighlightBrush = new SolidBrush(SystemColors.Highlight);
		m_hotPen = new Pen(SystemColors.HotTrack, 3f);
		m_hotPen.Alignment = PenAlignment.Inset;
		m_hotBrush = new SolidBrush(SystemColors.HotTrack);
		m_ghostPen = new Pen(Color.Silver, 1f);
		m_ghostBrush = new SolidBrush(Color.White);
		m_hiddenPen = new Pen(Color.LightGray, 1f);
		m_hiddenBrush = new SolidBrush(Color.LightGray);
		m_errorPen = new Pen(Color.Tomato, 1f);
		m_errorBrush = new SolidBrush(Color.Tomato);
		m_leftFormat.Alignment = StringAlignment.Near;
		m_rightFormat.Alignment = StringAlignment.Far;
		m_centerFormat.Alignment = StringAlignment.Center;
		m_centerFormat.LineAlignment = StringAlignment.Center;
	}

	protected virtual void OnRedraw()
	{
		this.Redraw.Raise(this, EventArgs.Empty);
	}

	~DiagramTheme()
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
		if (m_disposed)
		{
			return;
		}
		m_outlinePen.Dispose();
		m_fillBrush.Dispose();
		m_textBrush.Dispose();
		m_highlightPen.Dispose();
		m_highlightBrush.Dispose();
		m_lastHighlightPen.Dispose();
		m_lastHighlightBrush.Dispose();
		m_ghostPen.Dispose();
		m_ghostBrush.Dispose();
		m_hiddenPen.Dispose();
		m_hiddenBrush.Dispose();
		m_hotPen.Dispose();
		m_hotBrush.Dispose();
		m_errorPen.Dispose();
		m_errorBrush.Dispose();
		foreach (Pen value in m_pens.Values)
		{
			value.Dispose();
		}
		foreach (Brush value2 in m_brushes.Values)
		{
			value2.Dispose();
		}
		m_disposed = true;
	}

	public Pen GetPen(DiagramDrawingStyle style)
	{
		return style switch
		{
			DiagramDrawingStyle.Normal => m_outlinePen, 
			DiagramDrawingStyle.Selected => m_highlightPen, 
			DiagramDrawingStyle.LastSelected => m_lastHighlightPen, 
			DiagramDrawingStyle.Hot => m_hotPen, 
			DiagramDrawingStyle.Ghosted => m_ghostPen, 
			DiagramDrawingStyle.Hidden => m_hiddenPen, 
			_ => m_errorPen, 
		};
	}

	public Brush GetBrush(DiagramDrawingStyle style)
	{
		return style switch
		{
			DiagramDrawingStyle.Normal => m_fillBrush, 
			DiagramDrawingStyle.Selected => m_highlightBrush, 
			DiagramDrawingStyle.LastSelected => m_lastHighlightBrush, 
			DiagramDrawingStyle.Hot => m_hotBrush, 
			DiagramDrawingStyle.Ghosted => m_ghostBrush, 
			DiagramDrawingStyle.Hidden => m_hiddenBrush, 
			_ => m_errorBrush, 
		};
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
			OnRedraw();
		}
	}

	public void RegisterCustomPen(object key, Pen pen)
	{
		m_pens.TryGetValue(key, out var value);
		if (pen != value)
		{
			value?.Dispose();
			m_pens[key] = pen;
			OnRedraw();
		}
	}

	public Pen GetCustomPen(object key)
	{
		m_pens.TryGetValue(key, out var value);
		return value;
	}

	public void RegisterCustomBrush(object key, Brush brush)
	{
		m_brushes.TryGetValue(key, out var value);
		if (brush != value)
		{
			value?.Dispose();
			m_brushes[key] = brush;
			OnRedraw();
		}
	}

	public Brush GetCustomBrush(object key)
	{
		m_brushes.TryGetValue(key, out var value);
		return value;
	}
}
