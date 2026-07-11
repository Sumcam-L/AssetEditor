using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable;

public class LabelEditAdapter : ControlAdapter, ILabelEditAdapter, IDisposable
{
	private class TextBox : System.Windows.Forms.TextBox
	{
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
			case Keys.A | Keys.Control:
				SelectAll();
				return true;
			case Keys.Z | Keys.Control:
				Undo();
				return true;
			default:
				return ProcessCmdKey(ref msg, keyData);
			}
		}
	}

	private ITransformAdapter m_transformAdapter;

	private ISelectionAdapter m_selectionAdapter;

	private INamingContext m_namingContext;

	private readonly Timer m_labelEditTimer;

	private DiagramHitRecord m_itemHitRecord;

	private DiagramLabel m_hitLabel;

	private object m_item;

	private DiagramLabel m_label;

	private Rectangle m_labelBounds;

	private readonly TextBox m_textBox;

	public LabelEditAdapter()
	{
		m_labelEditTimer = new Timer();
		m_labelEditTimer.Tick += editLabelTimer_Tick;
		m_textBox = new TextBox();
		m_textBox.Visible = false;
		m_textBox.BorderStyle = BorderStyle.None;
		m_textBox.TextChanged += textBox_TextChanged;
		m_textBox.LostFocus += textBox_LostFocus;
		m_textBox.PreviewKeyDown += textBox_PreviewKeyDown;
	}

	public virtual void Dispose()
	{
		m_labelEditTimer.Dispose();
	}

	public void BeginEdit(INamingContext namingContext, object item, DiagramLabel label)
	{
		m_namingContext = namingContext;
		m_item = item;
		m_label = label;
		m_labelBounds = label.Bounds;
		float num = 1f;
		if (m_transformAdapter != null)
		{
			Matrix transform = m_transformAdapter.Transform;
			m_labelBounds = GdiUtil.Transform(transform, m_labelBounds);
			num *= transform.Elements[3];
		}
		m_textBox.Text = m_namingContext.GetName(m_item);
		Font font = base.AdaptedControl.Font;
		Font font2 = m_textBox.Font;
		m_textBox.Font = new Font(font.FontFamily, (int)(font.SizeInPoints * num));
		font2?.Dispose();
		TextFormatFlags format = m_label.Format;
		m_textBox.Multiline = (format & TextFormatFlags.SingleLine) == 0;
		HorizontalAlignment textAlign = HorizontalAlignment.Left;
		if ((format & TextFormatFlags.Right) != TextFormatFlags.Default)
		{
			textAlign = HorizontalAlignment.Right;
		}
		else if ((format & TextFormatFlags.HorizontalCenter) != TextFormatFlags.Default)
		{
			textAlign = HorizontalAlignment.Center;
		}
		m_textBox.TextAlign = textAlign;
		SizeTextBox();
		m_textBox.Visible = true;
		m_textBox.Focus();
		m_textBox.SelectAll();
	}

	public void EndEdit()
	{
		m_labelEditTimer.Enabled = false;
		if (m_namingContext == null || m_item == null || !m_textBox.Visible)
		{
			return;
		}
		string text = m_textBox.Text;
		if (text != m_namingContext.GetName(m_item))
		{
			ITransactionContext transactionContext = base.AdaptedControl.ContextAs<ITransactionContext>();
			if (transactionContext == null || !transactionContext.InTransaction)
			{
				transactionContext.DoTransaction(delegate
				{
					m_namingContext.SetName(m_item, text);
				}, "Edit Label".Localize());
			}
		}
		m_textBox.Visible = false;
		m_namingContext = null;
		m_item = null;
		base.AdaptedControl.Invalidate();
	}

	protected override void Bind(AdaptableControl control)
	{
		m_transformAdapter = control.As<ITransformAdapter>();
		m_selectionAdapter = control.As<ISelectionAdapter>();
		if (m_selectionAdapter != null)
		{
			m_selectionAdapter.SelectedItemHit += selectionAdapter_SelectedItemHit;
		}
		control.ContextChanged += control_ContextChanged;
		control.Invalidated += control_Invalidated;
		control.KeyDown += control_KeyDown;
		control.Controls.Add(m_textBox);
	}

	protected override void Unbind(AdaptableControl control)
	{
		control.ContextChanged -= control_ContextChanged;
		control.Invalidated -= control_Invalidated;
		control.KeyDown -= control_KeyDown;
		control.Controls.Remove(m_textBox);
	}

	private void selectionAdapter_SelectedItemHit(object sender, DiagramHitEventArgs e)
	{
		m_itemHitRecord = e.HitRecord;
		DiagramLabel diagramLabel = e.HitRecord.Part.As<DiagramLabel>();
		if (diagramLabel != null)
		{
			INamingContext namingContext = base.AdaptedControl.ContextAs<INamingContext>();
			if (namingContext != null && namingContext.CanSetName(e.HitRecord.Item))
			{
				m_hitLabel = diagramLabel;
				m_labelEditTimer.Interval = SystemInformation.DoubleClickTime;
				m_labelEditTimer.Enabled = true;
			}
		}
	}

	private void editLabelTimer_Tick(object sender, EventArgs e)
	{
		m_labelEditTimer.Enabled = false;
		PrepareForEdit();
	}

	private void PrepareForEdit()
	{
		if (!base.AdaptedControl.Capture)
		{
			INamingContext namingContext = base.AdaptedControl.ContextAs<INamingContext>();
			BeginEdit(namingContext, m_itemHitRecord.Item, m_hitLabel);
		}
		m_itemHitRecord = null;
		m_hitLabel = null;
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		EndEdit();
	}

	private void control_Invalidated(object sender, InvalidateEventArgs e)
	{
		EndEdit();
	}

	private void control_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyData == Keys.F2 && m_itemHitRecord != null && m_itemHitRecord.DefaultPart.As<DiagramLabel>() != null)
		{
			m_hitLabel = m_itemHitRecord.DefaultPart.As<DiagramLabel>();
			PrepareForEdit();
		}
	}

	private void textBox_TextChanged(object sender, EventArgs e)
	{
		SizeTextBox();
	}

	private void textBox_LostFocus(object sender, EventArgs e)
	{
		EndEdit();
	}

	private void textBox_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
	{
		if (e.KeyData == Keys.Escape)
		{
			if (m_namingContext != null && m_item != null)
			{
				m_textBox.Text = m_namingContext.GetName(m_item);
				EndEdit();
				base.AdaptedControl.Invalidate();
			}
		}
		else if (e.KeyData == Keys.Return)
		{
			EndEdit();
		}
	}

	private void SizeTextBox()
	{
		SizeF sizeF = TextRenderer.MeasureText(m_textBox.Text, m_textBox.Font, m_textBox.ClientRectangle.Size, m_label.Format);
		m_textBox.Size = new Size(Math.Max((int)sizeF.Width, m_labelBounds.Width), Math.Max((int)sizeF.Height, m_labelBounds.Height));
		Size size = m_textBox.Size;
		int y = m_labelBounds.Y + m_labelBounds.Height / 2 - size.Height / 2;
		int x = m_labelBounds.X;
		if (m_textBox.TextAlign == HorizontalAlignment.Right)
		{
			x = m_labelBounds.Right - size.Width;
		}
		else if (m_textBox.TextAlign == HorizontalAlignment.Center)
		{
			x = m_labelBounds.X + m_labelBounds.Width / 2 - size.Width / 2;
		}
		m_textBox.Location = new Point(x, y);
		int selectionStart = m_textBox.SelectionStart;
		m_textBox.SelectionStart = 0;
		m_textBox.SelectionStart = selectionStart;
	}
}
