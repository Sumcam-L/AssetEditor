using System;
using System.Collections.Generic;
using System.Drawing;
using Sce.Atf.Direct2D;

namespace Sce.Atf.Controls.Adaptable;

public class D2dDiagramTheme : D2dResource
{
	private int m_rowSpacing;

	private int m_pinOffset;

	private int m_pinSize = 8;

	private int m_pinMargin = 2;

	private readonly Dictionary<object, D2dBrush> m_brushes = new Dictionary<object, D2dBrush>();

	private readonly Dictionary<object, D2dBrush> m_titleBrushes = new Dictionary<object, D2dBrush>();

	private readonly Dictionary<object, D2dBitmap> m_bitmaps = new Dictionary<object, D2dBitmap>();

	private D2dTextFormat m_d2dTextFormat;

	private D2dBrush m_fillBrush;

	private D2dBrush m_fillTitleBrush;

	private D2dBrush m_textBrush;

	private D2dBrush m_highlightBrush;

	private D2dBrush m_textHighlightBrush;

	private D2dBrush m_lastHighlightBrush;

	private D2dBrush m_ghostBrush;

	private D2dBrush m_hiddenBrush;

	private D2dBrush m_templatedInstance;

	private D2dBrush m_copyInstance;

	private D2dBrush m_hotBrush;

	private D2dBrush m_dropTargetBrush;

	private D2dBrush m_dragSourceBrush;

	private D2dBrush m_errorBrush;

	private D2dBrush m_infoBrush;

	private D2dBrush m_outlineBrush;

	private D2dBrush m_hoverBorderBrush;

	private D2dLinearGradientBrush m_fillLinearGradientBrush;

	private int m_pickTolerance = 3;

	public virtual int RowSpacing => m_rowSpacing;

	public virtual int PinOffset => m_pinOffset;

	public int PinSize => m_pinSize;

	public int PinMargin => m_pinMargin;

	public float StrokeWidth { get; set; }

	public D2dTextFormat TextFormat
	{
		get
		{
			return m_d2dTextFormat;
		}
		set
		{
			SetDisposableField(value, ref m_d2dTextFormat);
		}
	}

	public D2dBrush OutlineBrush
	{
		get
		{
			return m_outlineBrush;
		}
		set
		{
			SetDisposableField(value, ref m_outlineBrush);
		}
	}

	public D2dLinearGradientBrush FillGradientBrush
	{
		get
		{
			return m_fillLinearGradientBrush;
		}
		set
		{
			SetDisposableField(value, ref m_fillLinearGradientBrush);
		}
	}

	public D2dBrush FillBrush
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

	public D2dBrush FillTitleBrush
	{
		get
		{
			return m_fillTitleBrush;
		}
		set
		{
			SetDisposableField(value, ref m_fillTitleBrush);
		}
	}

	public D2dBrush TextBrush
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

	public D2dBrush HighlightBrush
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

	public D2dBrush LastHighlightBrush
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

	public D2dBrush TextHighlightBrush
	{
		get
		{
			return m_textHighlightBrush;
		}
		set
		{
			SetDisposableField(value, ref m_textHighlightBrush);
		}
	}

	public D2dBrush GhostBrush
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

	public D2dBrush HiddenBrush
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

	public D2dBrush TemplatedInstance
	{
		get
		{
			return m_templatedInstance;
		}
		set
		{
			SetDisposableField(value, ref m_templatedInstance);
		}
	}

	public D2dBrush CopyInstance
	{
		get
		{
			return m_copyInstance;
		}
		set
		{
			SetDisposableField(value, ref m_copyInstance);
		}
	}

	public D2dBrush HotBrush
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

	public D2dBrush DragSourceBrush
	{
		get
		{
			return m_dragSourceBrush;
		}
		set
		{
			SetDisposableField(value, ref m_dragSourceBrush);
		}
	}

	public D2dBrush DropTargetBrush
	{
		get
		{
			return m_dropTargetBrush;
		}
		set
		{
			SetDisposableField(value, ref m_dropTargetBrush);
		}
	}

	public D2dBrush ErrorBrush
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

	public D2dBrush InfoBrush
	{
		get
		{
			return m_infoBrush;
		}
		set
		{
			SetDisposableField(value, ref m_infoBrush);
		}
	}

	public D2dBrush HoverBorderBrush
	{
		get
		{
			return m_hoverBorderBrush;
		}
		set
		{
			SetDisposableField(value, ref m_hoverBorderBrush);
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

	public event EventHandler Redraw;

	public D2dDiagramTheme()
		: this("Microsoft Sans Serif", 10f)
	{
	}

	public D2dDiagramTheme(string fontFamilyName, float fontSize)
	{
		m_d2dTextFormat = D2dFactory.CreateTextFormat(fontFamilyName, fontSize);
		m_fillBrush = D2dFactory.CreateSolidBrush(SystemColors.Window);
		m_fillTitleBrush = D2dFactory.CreateSolidBrush(Color.YellowGreen);
		m_textBrush = D2dFactory.CreateSolidBrush(SystemColors.WindowText);
		m_outlineBrush = D2dFactory.CreateSolidBrush(SystemColors.ControlDark);
		m_highlightBrush = D2dFactory.CreateSolidBrush(SystemColors.Highlight);
		m_lastHighlightBrush = D2dFactory.CreateSolidBrush(SystemColors.Highlight);
		m_textHighlightBrush = D2dFactory.CreateSolidBrush(SystemColors.Highlight);
		m_hotBrush = D2dFactory.CreateSolidBrush(SystemColors.HotTrack);
		m_dragSourceBrush = D2dFactory.CreateSolidBrush(Color.SlateBlue);
		m_dropTargetBrush = D2dFactory.CreateSolidBrush(Color.Chartreuse);
		m_ghostBrush = D2dFactory.CreateSolidBrush(Color.White);
		m_hiddenBrush = D2dFactory.CreateSolidBrush(Color.LightGray);
		m_templatedInstance = D2dFactory.CreateSolidBrush(Color.Yellow);
		m_copyInstance = D2dFactory.CreateSolidBrush(Color.Green);
		m_infoBrush = D2dFactory.CreateSolidBrush(SystemColors.Info);
		m_hoverBorderBrush = D2dFactory.CreateSolidBrush(SystemColors.ControlDarkDark);
		int num = (int)TextFormat.FontHeight;
		m_rowSpacing = num + PinMargin;
		m_pinOffset = (num - m_pinSize) / 2;
		D2dGradientStop[] gradientStops = new D2dGradientStop[2]
		{
			new D2dGradientStop(Color.White, 0f),
			new D2dGradientStop(Color.LightSteelBlue, 1f)
		};
		m_fillLinearGradientBrush = D2dFactory.CreateLinearGradientBrush(gradientStops);
		D2dGradientStop[] gradientStops2 = new D2dGradientStop[2]
		{
			new D2dGradientStop(Color.White, 0f),
			new D2dGradientStop(Color.MediumVioletRed, 1f)
		};
		m_errorBrush = D2dFactory.CreateLinearGradientBrush(gradientStops2);
		StrokeWidth = 2f;
	}

	public void RegisterCustomBrush(object key, D2dBrush brush)
	{
		m_brushes.TryGetValue(key, out var value);
		if (brush != value)
		{
			value?.Dispose();
			m_brushes[key] = brush;
			OnRedraw();
		}
	}

	public D2dBrush GetCustomBrush(object key)
	{
		m_brushes.TryGetValue(key, out var value);
		return value;
	}

	public void RegisterBitmap(object key, Image image)
	{
		D2dBitmap value = D2dFactory.CreateBitmap(image);
		m_bitmaps[key] = value;
	}

	public D2dBitmap GetBitmap(object key)
	{
		if (m_bitmaps.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	public D2dBrush GetFillTitleBrush(object key)
	{
		m_titleBrushes.TryGetValue(key, out var value);
		return value ?? m_fillTitleBrush;
	}

	public void RegisterFillTitleBrush(object key, D2dBrush brush)
	{
		m_titleBrushes.TryGetValue(key, out var value);
		if (brush != value)
		{
			value?.Dispose();
			m_titleBrushes[key] = brush;
			OnRedraw();
		}
	}

	public D2dBrush GetCustomOrDefaultBrush(object key)
	{
		m_brushes.TryGetValue(key, out var value);
		if (value == null)
		{
			return m_fillLinearGradientBrush;
		}
		return value;
	}

	protected virtual void OnRedraw()
	{
		this.Redraw.Raise(this, EventArgs.Empty);
	}

	protected override void Dispose(bool disposing)
	{
		if (base.IsDisposed)
		{
			return;
		}
		if (disposing)
		{
			m_fillBrush.Dispose();
			m_fillTitleBrush.Dispose();
			m_textBrush.Dispose();
			m_d2dTextFormat.Dispose();
			m_highlightBrush.Dispose();
			m_lastHighlightBrush.Dispose();
			m_ghostBrush.Dispose();
			m_hiddenBrush.Dispose();
			m_templatedInstance.Dispose();
			m_copyInstance.Dispose();
			m_hotBrush.Dispose();
			m_errorBrush.Dispose();
			m_highlightBrush.Dispose();
			foreach (D2dBrush value in m_brushes.Values)
			{
				value.Dispose();
			}
			m_brushes.Clear();
			foreach (D2dBrush value2 in m_titleBrushes.Values)
			{
				value2.Dispose();
			}
			m_titleBrushes.Clear();
			foreach (D2dBitmap value3 in m_bitmaps.Values)
			{
				value3.Dispose();
			}
			m_bitmaps.Clear();
		}
		base.Dispose(disposing);
	}

	public D2dBrush GetOutLineBrush(DiagramDrawingStyle style)
	{
		return style switch
		{
			DiagramDrawingStyle.Normal => m_outlineBrush, 
			DiagramDrawingStyle.Selected => m_highlightBrush, 
			DiagramDrawingStyle.LastSelected => m_lastHighlightBrush, 
			DiagramDrawingStyle.Hot => m_hotBrush, 
			DiagramDrawingStyle.DragSource => m_dragSourceBrush, 
			DiagramDrawingStyle.DropTarget => m_dropTargetBrush, 
			DiagramDrawingStyle.Ghosted => m_ghostBrush, 
			DiagramDrawingStyle.Hidden => m_hiddenBrush, 
			DiagramDrawingStyle.TemplatedInstance => m_templatedInstance, 
			DiagramDrawingStyle.CopyInstance => m_copyInstance, 
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
}
