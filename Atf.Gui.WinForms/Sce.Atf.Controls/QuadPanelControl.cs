using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;

namespace Sce.Atf.Controls;

public class QuadPanelControl : Control
{
	private readonly object m_context;

	private Control m_topLeft;

	private Control m_topRight;

	private Control m_bottomLeft;

	private Control m_bottomRight;

	private Control m_activeControl;

	private float m_splitX = 0.5f;

	private float m_splitY = 0.5f;

	private bool m_draggingX;

	private bool m_draggingY;

	private int m_splitterThickness = 8;

	private int m_tolerance = 2;

	private bool m_enableX = true;

	private bool m_enableY = true;

	private static readonly Cursor s_xyCursor = ResourceUtil.GetCursor(Resources.FourWayCursor);

	private static readonly Cursor s_xCursor = ResourceUtil.GetCursor(Resources.VerticalSizeCursor);

	private static readonly Cursor s_yCursor = ResourceUtil.GetCursor(Resources.HorizSizeCursor);

	private const string SettingsElementName = "QuadPanelControlSettings";

	private const string SettingsSplitXAttributeName = "SplitX";

	private const string SettingsSplitYAttributeName = "SplitY";

	private const string SettingsEnableXAttributeName = "EnableX";

	private const string SettingsEnableYAttributeName = "EnableY";

	public object Context => m_context;

	public Control TopLeft
	{
		get
		{
			return m_topLeft;
		}
		set
		{
			if (m_topLeft != null)
			{
				base.Controls.Remove(m_topLeft);
				m_topLeft.GotFocus -= ControlGotFocus;
			}
			m_topLeft = value;
			if (m_topLeft != null)
			{
				m_topLeft.Name = "TopLeft";
				base.Controls.Add(m_topLeft);
				m_topLeft.GotFocus += ControlGotFocus;
				SizeTopLeft();
			}
		}
	}

	public Control TopRight
	{
		get
		{
			return m_topRight;
		}
		set
		{
			if (m_topRight != null)
			{
				base.Controls.Remove(m_topRight);
				m_topRight.GotFocus -= ControlGotFocus;
			}
			m_topRight = value;
			if (m_topRight != null)
			{
				m_topRight.Name = "TopRight";
				base.Controls.Add(m_topRight);
				m_topRight.GotFocus += ControlGotFocus;
				SizeTopRight();
			}
		}
	}

	public Control BottomLeft
	{
		get
		{
			return m_bottomLeft;
		}
		set
		{
			if (m_bottomLeft != null)
			{
				base.Controls.Remove(m_bottomLeft);
				m_bottomLeft.GotFocus -= ControlGotFocus;
			}
			m_bottomLeft = value;
			if (m_bottomLeft != null)
			{
				m_bottomLeft.Name = "BottomLeft";
				base.Controls.Add(m_bottomLeft);
				m_bottomLeft.GotFocus += ControlGotFocus;
				SizeBottomLeft();
			}
		}
	}

	public Control BottomRight
	{
		get
		{
			return m_bottomRight;
		}
		set
		{
			if (m_bottomRight != null)
			{
				base.Controls.Remove(m_bottomRight);
				m_bottomRight.GotFocus -= ControlGotFocus;
			}
			m_bottomRight = value;
			if (m_bottomRight != null)
			{
				m_bottomRight.Name = "BottomRight";
				base.Controls.Add(m_bottomRight);
				m_bottomRight.GotFocus += ControlGotFocus;
				SizeBottomRight();
			}
		}
	}

	public Control ActiveControl
	{
		get
		{
			return (m_activeControl == null) ? m_topLeft : m_activeControl;
		}
		set
		{
			if (value != m_topLeft && value != m_topRight && value != m_bottomLeft && value != m_bottomRight)
			{
				throw new InvalidOperationException("Invalid control");
			}
			m_activeControl = value;
			Invalidate();
		}
	}

	public float SplitX
	{
		get
		{
			return m_splitX;
		}
		set
		{
			if (m_splitX != value)
			{
				SetSplitX(value);
				SizeAll();
				Refresh();
			}
		}
	}

	public float SplitY
	{
		get
		{
			return m_splitY;
		}
		set
		{
			if (m_splitY != value)
			{
				SetSplitY(value);
				SizeAll();
				Refresh();
			}
		}
	}

	public bool EnableX
	{
		get
		{
			return m_enableX;
		}
		set
		{
			m_enableX = value;
		}
	}

	public bool EnableY
	{
		get
		{
			return m_enableY;
		}
		set
		{
			m_enableY = value;
		}
	}

	public int SplitterThickness
	{
		get
		{
			return m_splitterThickness;
		}
		set
		{
			value = Math.Min(Math.Max(0, value), 20);
			if (m_splitterThickness != value)
			{
				m_splitterThickness = value;
				Refresh();
			}
		}
	}

	public int Tolerance
	{
		get
		{
			return m_tolerance;
		}
		set
		{
			m_tolerance = value;
		}
	}

	public string PersistedSettings
	{
		get
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.AppendChild(xmlDocument.CreateXmlDeclaration("1.0", "utf-8", "yes"));
			XmlElement xmlElement = xmlDocument.CreateElement("QuadPanelControlSettings");
			xmlDocument.AppendChild(xmlElement);
			try
			{
				xmlElement.SetAttribute("SplitX", SplitX.ToString());
				xmlElement.SetAttribute("SplitY", SplitY.ToString());
				xmlElement.SetAttribute("EnableX", EnableX.ToString());
				xmlElement.SetAttribute("EnableY", EnableY.ToString());
				if (xmlDocument.DocumentElement == null)
				{
					xmlDocument.RemoveAll();
				}
			}
			catch (Exception ex)
			{
				Outputs.WriteLine(OutputMessageType.Error, "{0}: Exception saving quad panel persisted settings: {1}", this, ex.Message);
				xmlDocument.RemoveAll();
			}
			return xmlDocument.InnerXml.Trim();
		}
		set
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.LoadXml(value);
				if (xmlDocument.DocumentElement != null)
				{
					XmlElement documentElement = xmlDocument.DocumentElement;
					SplitX = float.Parse(documentElement.GetAttribute("SplitX"));
					SplitY = float.Parse(documentElement.GetAttribute("SplitY"));
					EnableX = bool.Parse(documentElement.GetAttribute("EnableX"));
					EnableY = bool.Parse(documentElement.GetAttribute("EnableY"));
				}
			}
			catch (Exception ex)
			{
				Outputs.WriteLine(OutputMessageType.Error, "{0}: Exception loading quad panel persisted settings: {1}", this, ex.Message);
			}
		}
	}

	private bool MultiPanelMode => m_enableX || m_enableY || m_splitX < 1f || m_splitY < 1f;

	public event EventHandler<EventArgs> SplitterDragged;

	public QuadPanelControl()
	{
	}

	public QuadPanelControl(object context)
	{
		m_context = context;
	}

	protected virtual void OnSplitterDragged(EventArgs e)
	{
		this.SplitterDragged?.Invoke(this, e);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		SizeAll();
		Refresh();
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (IsOverSplitX(e.X))
		{
			m_draggingY = true;
			DrawXSplitter();
		}
		if (IsOverSplitY(e.Y))
		{
			m_draggingX = true;
			DrawYSplitter();
		}
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			if (m_draggingY)
			{
				DrawXSplitter();
				SetSplitX((float)e.X / (float)base.Width);
				DrawXSplitter();
			}
			if (m_draggingX)
			{
				DrawYSplitter();
				SetSplitY((float)e.Y / (float)base.Height);
				DrawYSplitter();
			}
		}
		bool flag = IsOverSplitX(e.X);
		bool flag2 = IsOverSplitY(e.Y);
		if (flag && flag2)
		{
			Cursor = s_xyCursor;
		}
		else if (flag)
		{
			Cursor = s_xCursor;
		}
		else if (flag2)
		{
			Cursor = s_yCursor;
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (m_draggingX || m_draggingY)
		{
			m_draggingY = (m_draggingX = false);
			OnSplitterDragged(EventArgs.Empty);
			SizeAll();
			Refresh();
		}
		base.OnMouseUp(e);
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		Cursor = Cursors.Arrow;
		base.OnMouseLeave(e);
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		SizeAll();
		Refresh();
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		int splitX = GetSplitX();
		int splitY = GetSplitY();
		if (m_enableX)
		{
			ControlPaint.DrawBorder3D(e.Graphics, splitX - m_splitterThickness / 2, -1, m_splitterThickness, base.Height + 2);
		}
		if (m_enableY)
		{
			ControlPaint.DrawBorder3D(e.Graphics, -1, splitY - m_splitterThickness / 2, base.Width + 2, m_splitterThickness);
		}
		if (MultiPanelMode)
		{
			ControlPaint.DrawBorder3D(e.Graphics, splitX - m_splitterThickness / 2, splitY - m_splitterThickness / 2, m_splitterThickness, m_splitterThickness, Border3DStyle.Flat, Border3DSide.Middle);
			Control activeControl = ActiveControl;
			if (activeControl != null)
			{
				Rectangle bounds = activeControl.Bounds;
				bounds.Inflate(2, 2);
				ControlPaint.DrawBorder(e.Graphics, bounds, Color.Blue, 2, ButtonBorderStyle.Solid, Color.Blue, 2, ButtonBorderStyle.Solid, Color.Blue, 2, ButtonBorderStyle.Solid, Color.Blue, 2, ButtonBorderStyle.Solid);
			}
		}
	}

	private void ControlGotFocus(object sender, EventArgs e)
	{
		m_activeControl = sender as Control;
		Refresh();
	}

	private void SizeAll()
	{
		SizeTopLeft();
		SizeTopRight();
		SizeBottomLeft();
		SizeBottomRight();
	}

	private void SizeTopLeft()
	{
		if (m_topLeft != null)
		{
			int num = (int)(m_splitX * (float)base.Width) - m_splitterThickness / 2;
			int num2 = (int)(m_splitY * (float)base.Height) - m_splitterThickness / 2;
			m_topLeft.Bounds = new Rectangle(1, 1, num - 2, num2 - 2);
		}
	}

	private void SizeTopRight()
	{
		if (m_topRight != null)
		{
			int num = (int)(m_splitX * (float)base.Width) - m_splitterThickness / 2 + m_splitterThickness;
			int num2 = (int)(m_splitY * (float)base.Height) - m_splitterThickness / 2;
			m_topRight.Bounds = new Rectangle(num, 1, base.Width - num - 1, num2 - 2);
		}
	}

	private void SizeBottomLeft()
	{
		if (m_bottomLeft != null)
		{
			int num = (int)(m_splitX * (float)base.Width) - m_splitterThickness / 2;
			int num2 = (int)(m_splitY * (float)base.Height) - m_splitterThickness / 2 + m_splitterThickness;
			m_bottomLeft.Bounds = new Rectangle(1, num2, num - 2, base.Height - num2 - 1);
		}
	}

	private void SizeBottomRight()
	{
		if (m_bottomRight != null)
		{
			int num = (int)(m_splitX * (float)base.Width) - m_splitterThickness / 2 + m_splitterThickness;
			int num2 = (int)(m_splitY * (float)base.Height) - m_splitterThickness / 2 + m_splitterThickness;
			m_bottomRight.Bounds = new Rectangle(num, num2, base.Width - num - 1, base.Height - num2 - 1);
		}
	}

	private void DrawXSplitter()
	{
		int splitX = GetSplitX();
		Point start = PointToScreen(new Point(splitX, 0));
		Point end = PointToScreen(new Point(splitX, base.Height));
		ControlPaint.DrawReversibleLine(start, end, BackColor);
	}

	private void DrawYSplitter()
	{
		int splitY = GetSplitY();
		Point start = PointToScreen(new Point(0, splitY));
		Point end = PointToScreen(new Point(base.Width, splitY));
		ControlPaint.DrawReversibleLine(start, end, BackColor);
	}

	private bool IsOverSplitX(int x)
	{
		if (!m_enableX)
		{
			return false;
		}
		return Math.Abs(GetSplitX() - x) < m_splitterThickness / 2 + m_tolerance;
	}

	private bool IsOverSplitY(int y)
	{
		if (!m_enableY)
		{
			return false;
		}
		return Math.Abs(GetSplitY() - y) < m_splitterThickness / 2 + m_tolerance;
	}

	private int GetSplitX()
	{
		return (int)(m_splitX * (float)base.Width);
	}

	private int GetSplitY()
	{
		return (int)(m_splitY * (float)base.Height);
	}

	private void SetSplitX(float x)
	{
		m_splitX = Math.Min(Math.Max(0f, x), 1f);
	}

	private void SetSplitY(float y)
	{
		m_splitY = Math.Min(Math.Max(0f, y), 1f);
	}
}
