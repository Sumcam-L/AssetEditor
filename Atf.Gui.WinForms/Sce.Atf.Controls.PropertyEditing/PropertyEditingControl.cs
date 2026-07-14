using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.VisualStyles;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class PropertyEditingControl : Control, IWindowsFormsEditorService, ITypeDescriptorContext, IServiceProvider, ICacheablePropertyControl, IFormsOwner
{
	public enum CustomStringFormatFlags
	{
		None,
		TrimLeftWithEllipses
	}

	public class ExtendedStringFormat
	{
		public StringFormat Format;

		public CustomStringFormatFlags CustomFlags;
	}

	private class EditButton : Button
	{
		private bool m_modal;

		private bool m_pushed;

		public Color DisabledTextColor { get; set; } = SystemColors.GrayText;

		public bool Modal
		{
			get
			{
				return m_modal;
			}
			set
			{
				m_modal = value;
				Invalidate();
			}
		}

		public EditButton()
		{
			SetStyle(ControlStyles.Selectable, value: true);
			base.TabStop = false;
			base.IsDefault = false;
		}

		protected override void OnMouseDown(MouseEventArgs arg)
		{
			base.OnMouseDown(arg);
			if (arg.Button == MouseButtons.Left)
			{
				m_pushed = true;
				Invalidate();
			}
		}

		protected override void OnMouseUp(MouseEventArgs arg)
		{
			base.OnMouseUp(arg);
			if (arg.Button == MouseButtons.Left)
			{
				m_pushed = false;
				Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;
			Rectangle clientRectangle = base.ClientRectangle;
			if (m_modal)
			{
				base.OnPaint(e);
				int num = clientRectangle.X + clientRectangle.Width / 2 - 5;
				int num2 = clientRectangle.Bottom - 5;
				using Brush brush = new SolidBrush(base.Enabled ? ForeColor : DisabledTextColor);
				graphics.FillRectangle(brush, num, num2, 2, 2);
				graphics.FillRectangle(brush, num + 4, num2, 2, 2);
				graphics.FillRectangle(brush, num + 8, num2, 2, 2);
				return;
			}
			if (Application.RenderWithVisualStyles)
			{
				ComboBoxRenderer.DrawDropDownButton(graphics, base.ClientRectangle, (!base.Enabled) ? ComboBoxState.Disabled : ((!m_pushed) ? ComboBoxState.Normal : ComboBoxState.Pressed));
			}
			else
			{
				ControlPaint.DrawButton(graphics, base.ClientRectangle, (!base.Enabled) ? ButtonState.Inactive : (m_pushed ? ButtonState.Pushed : ButtonState.Normal));
			}
		}
	}

	private class DropDownForm : Form
	{
		private Control m_control;

		private readonly PropertyEditingControl m_parent;

		public DropDownForm(PropertyEditingControl parent)
		{
			m_parent = parent;
			base.StartPosition = FormStartPosition.Manual;
			base.ShowInTaskbar = false;
			base.ControlBox = false;
			base.MinimizeBox = false;
			base.MaximizeBox = false;
			base.FormBorderStyle = FormBorderStyle.None;
			base.Visible = false;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				m_parent.CloseDropDown();
			}
			base.OnMouseDown(e);
		}

		protected override void OnClosed(EventArgs e)
		{
			if (base.Visible)
			{
				m_parent.CloseDropDown();
			}
			base.OnClosed(e);
		}

		protected override void OnDeactivate(EventArgs e)
		{
            if (base.Visible)
            {
                m_parent.CloseDropDown();
            }
            base.OnDeactivate(e);
		}

		public void SetControl(Control control)
		{
			if (m_control != null)
			{
				base.Controls.Remove(m_control);
				m_control.Resize -= control_Resize;
				m_control = null;
			}
			if (control != null)
			{
				control.Width = Math.Max(m_parent.Width, control.Width);
				base.Size = control.Size;
				m_control = control;
				base.Controls.Add(m_control);
				m_control.Location = new Point(0, 0);
				m_control.Visible = true;
				UpdateBoundsFromControl();
				m_control.Size = base.Size;
				m_control.Resize += control_Resize;
			}
			base.Enabled = m_control != null;
		}

		private void UpdateBoundsFromControl()
		{
			Rectangle bounds = base.Bounds;
			bounds.Width = Math.Max(m_parent.Width, m_control.Width);
			Rectangle workingArea = Screen.FromControl(this).WorkingArea;
			bounds.Intersect(workingArea);
			base.Bounds = bounds;
		}

		private void control_Resize(object sender, EventArgs e)
		{
			UpdateBoundsFromControl();
		}
	}

	private class TextBox : System.Windows.Forms.TextBox
	{
		public bool AutoCompleteEnabled => base.AutoCompleteMode == AutoCompleteMode.SuggestAppend && base.AutoCompleteCustomSource.Count > 0;

		public TextBox()
		{
			AutoSize = false;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			base.Height = base.FontHeight;
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Up || keyData == Keys.Down)
			{
				return false;
			}
			if (keyData == Keys.Left && base.SelectionStart == 0 && SelectionLength == 0)
			{
				return false;
			}
			if (keyData == Keys.Right && base.SelectionStart == Text.Length)
			{
				return false;
			}
			if (keyData == Keys.Left && SelectionLength > 0)
			{
				SelectionLength = 0;
			}
			return base.IsInputKey(keyData);
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (AutoCompleteEnabled && char.IsLetterOrDigit(e.KeyChar))
			{
				int length = ((base.SelectionStart < base.Text.Length) ? base.SelectionStart : base.Text.Length);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append(base.Text.Substring(0, length));
				stringBuilder.Append(e.KeyChar);
				string text = stringBuilder.ToString();
				if (!ValidateInput(text))
				{
					e.Handled = true;
				}
			}
			base.OnKeyPress(e);
		}

		private bool ValidateInput(string text)
		{
			foreach (string item in base.AutoCompleteCustomSource)
			{
				if (string.Compare(text, 0, item, 0, text.Length, ignoreCase: true, CultureInfo.CurrentCulture) == 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	private const int PaintRectWidth = 19;

	private const int PaintRectHeight = 15;

	private const int PaintRectTextOffset = 23;

	private const int TrimmingEllipsesWidth = 16;

	private PropertyEditorControlContext m_context;

	private PropertyDescriptor m_descriptor;

	private string m_initialText = string.Empty;

	private bool m_settingValue;

	private bool m_isEditing;

	private bool m_absorbEditButtonClick;

	private readonly EditButton m_editButton;

	private readonly TextBox m_textBox;

	private readonly DropDownForm m_dropDownForm;

	private bool m_closingDropDown;

	private static ExtendedStringFormat s_textFormat;

	private static bool s_drawingEditableValue;

	public Color ReadOnlyTextColor { get; set; } = SystemColors.GrayText;

	public static Color PaintValueBorderColor { get; set; }

	public Size EditButtonSize
	{
		get
		{
			return m_editButton.Size;
		}
		set
		{
			m_editButton.Size = value;
			m_editButton.Left = base.Right - value.Width;
		}
	}

	public IEnumerable<Form> Forms
	{
		get
		{
			yield return m_dropDownForm;
		}
	}

	public virtual bool Cacheable => true;

	public static StringFormat TextFormat
	{
		get
		{
			return s_textFormat.Format;
		}
		set
		{
			s_textFormat.Format = value;
		}
	}

	public static ExtendedStringFormat ExtendedTextFormat
	{
		get
		{
			return s_textFormat;
		}
		set
		{
			s_textFormat = value;
		}
	}

	public static bool DrawingEditableValue => s_drawingEditableValue;

	public object Instance => m_context.LastSelectedObject;

	public PropertyDescriptor PropertyDescriptor => m_descriptor;

	public override bool AllowDrop
	{
		set
		{
			base.AllowDrop = value;
			m_textBox.AllowDrop = value;
		}
	}

	public event EventHandler<PropertyEditedEventArgs> PropertyEdited;

	public PropertyEditingControl()
	{
		m_editButton = new EditButton();
		m_textBox = new TextBox();
		IntPtr handle = m_editButton.Handle;
		handle = m_textBox.Handle;
		SuspendLayout();
		m_editButton.Left = base.Right - 18;
		m_editButton.Size = new Size(18, 18);
		m_editButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
		m_editButton.Visible = false;
		m_editButton.Click += editButton_Click;
		m_editButton.MouseDown += editButton_MouseDown;
		m_textBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
		m_textBox.BorderStyle = BorderStyle.None;
		m_textBox.LostFocus += textBox_LostFocus;
		m_textBox.DragOver += textBox_DragOver;
		m_textBox.DragDrop += textBox_DragDrop;
		m_textBox.MouseHover += textBox_MouseHover;
		m_textBox.MouseLeave += textBox_MouseLeave;
		m_textBox.Visible = false;
		base.Controls.Add(m_editButton);
		base.Controls.Add(m_textBox);
		ResumeLayout();
		m_textBox.SizeChanged += delegate
		{
			base.Height = m_textBox.Height + 1;
		};
		m_dropDownForm = new DropDownForm(this);
	}

	public void Bind(PropertyEditorControlContext context)
	{
		if (m_textBox.Focused)
		{
			Flush();
		}
		m_context = context;
		m_descriptor = m_context.Descriptor;
		if (!(base.Visible = m_context != null))
		{
			return;
		}
		SetTextBoxFromProperty();
		bool visible = false;
		if (!m_context.IsReadOnly)
		{
			UITypeEditor uITypeEditor = WinFormsPropertyUtils.GetUITypeEditor(m_descriptor, this);
			if (uITypeEditor != null)
			{
				visible = true;
				m_editButton.Modal = uITypeEditor.GetEditStyle(this) == UITypeEditorEditStyle.Modal;
			}
		}
		m_editButton.Visible = visible;
		if (!m_context.IsReadOnly && m_descriptor.Converter != null)
		{
			TypeDescriptorContext context2 = new TypeDescriptorContext(m_context.LastSelectedObject, m_descriptor, null);
			if (m_descriptor.Converter.GetStandardValuesExclusive(context2))
			{
				m_textBox.AutoCompleteMode = AutoCompleteMode.None;
				m_textBox.AutoCompleteCustomSource.Clear();
				AutoCompleteStringCollection autoCompleteStringCollection = new AutoCompleteStringCollection();
				ICollection standardValues = m_descriptor.Converter.GetStandardValues(context2);
				foreach (object item in standardValues)
				{
					autoCompleteStringCollection.Add(item.ToString());
				}
				m_textBox.AutoCompleteCustomSource = autoCompleteStringCollection;
				m_textBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
				m_textBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
			}
			else
			{
				m_textBox.AutoCompleteMode = AutoCompleteMode.None;
				m_textBox.AutoCompleteSource = AutoCompleteSource.None;
				m_textBox.AutoCompleteCustomSource.Clear();
			}
		}
		else
		{
			m_textBox.AutoCompleteMode = AutoCompleteMode.None;
			m_textBox.AutoCompleteSource = AutoCompleteSource.None;
			m_textBox.AutoCompleteCustomSource.Clear();
		}
		PerformLayout();
		Invalidate();
	}

	public void CancelEdit()
	{
		SetTextBoxFromProperty();
		DisableTextBox();
	}

	public void Flush()
	{
		SetPropertyFromTextBox();
	}

	public static void DrawProperty(PropertyDescriptor descriptor, ITypeDescriptorContext context, Rectangle bounds, Font font, Brush brush, Graphics g)
	{
		object instance = context.Instance;
		if (instance == null)
		{
			return;
		}
		UITypeEditor uITypeEditor = WinFormsPropertyUtils.GetUITypeEditor(descriptor, context);
		if (uITypeEditor != null && uITypeEditor.GetPaintValueSupported(context))
		{
			object value = descriptor.GetValue(instance);
			Rectangle rectangle = new Rectangle(bounds.Left + 1, bounds.Top + 1, 19, 15);
			uITypeEditor.PaintValue(new PaintValueEventArgs(context, value, g, rectangle));
			bounds.X += 23;
			bounds.Width -= 23;
			using Pen pen = new Pen(PaintValueBorderColor);
			g.DrawRectangle(pen, rectangle);
		}
		string s = PropertyUtils.GetPropertyText(instance, descriptor);
		bounds.Height = font.Height;
		if (s_textFormat.CustomFlags == CustomStringFormatFlags.TrimLeftWithEllipses)
		{
			s = TrimStringLeftWithEllipses(s, bounds, font, g);
		}
		g.DrawString(s, font, brush, bounds, s_textFormat.Format);
	}

	private static string TrimStringLeftWithEllipses(string text, Rectangle bounds, Font font, Graphics graphics)
	{
		if (text.Length == 0)
		{
			return text;
		}
		if (bounds.Width == 0)
		{
			return "";
		}
		SizeF sizeF = graphics.MeasureString(text, font);
		if ((int)sizeF.Width <= bounds.Width)
		{
			return text;
		}
		int num = bounds.Width - 16;
		if (num <= 0)
		{
			return "";
		}
		int num2 = 0;
		do
		{
			float num3 = (float)num / sizeF.Width;
			int num4 = (int)((float)text.Length * num3);
			if (num4 == text.Length)
			{
				num4--;
			}
			text = text.Remove(0, text.Length - num4);
			sizeF = graphics.MeasureString(text, font);
		}
		while (sizeF.Width > (float)num && ++num2 < 5 && text.Length > 0);
		return "... " + text;
	}

	protected virtual void OnPropertyEdited(PropertyEditedEventArgs e)
	{
		if (this.PropertyEdited != null)
		{
			this.PropertyEdited(this, e);
		}
	}

	public new object GetService(Type serviceType)
	{
		if (typeof(IWindowsFormsEditorService).IsAssignableFrom(serviceType) || typeof(ITypeDescriptorContext).IsAssignableFrom(serviceType))
		{
			return this;
		}
		return base.GetService(serviceType);
	}

	public void DropDownControl(Control control)
	{
		m_dropDownForm.SetControl(control);
		SkinService.ApplyActiveSkin(control);
		Point p = new Point(base.Right, base.Bottom);
		p = base.Parent.PointToScreen(p);
		Rectangle bounds = new Rectangle(p.X - control.Width, p.Y, control.Width, control.Height);
		Rectangle workingArea = Screen.GetWorkingArea(base.Parent);
		bounds.Intersect(workingArea);
		m_dropDownForm.Bounds = bounds;
		m_dropDownForm.Visible = true;
		control.Focus();
		while (m_dropDownForm.Visible)
		{
			Application.DoEvents();
			MsgWaitForMultipleObjects(0, 0, bWaitAll: true, 250, 255);
		}
	}

	public void CloseDropDown()
	{
		if (m_closingDropDown)
		{
			return;
		}
		EnableTextBox();
		try
		{
			m_closingDropDown = true;
			if (m_dropDownForm.Visible)
			{
				m_dropDownForm.SetControl(null);
				m_dropDownForm.Visible = false;
			}
		}
		finally
		{
			m_closingDropDown = false;
		}
	}

	public DialogResult ShowDialog(Form dialog)
	{
		dialog.ShowDialog(this);
		return dialog.DialogResult;
	}

	[DllImport("user32.dll")]
	private static extern int MsgWaitForMultipleObjects(int nCount, int pHandles, bool bWaitAll, int dwMilliseconds, int dwWakeMask);

	protected void HideForm()
	{
		CloseDropDown();
	}

	public void OnComponentChanged()
	{
	}

	public bool OnComponentChanging()
	{
		return true;
	}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if ((keyData == (Keys.Down | Keys.Alt) || keyData == (Keys.Down | Keys.Control)) && m_editButton.Visible)
		{
			OpenDropDownEditor();
		}
		return base.ProcessCmdKey(ref msg, keyData);
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (keyData == Keys.Return)
		{
			base.ProcessDialogKey(keyData);
			Flush();
			DisableTextBox();
			base.Visible = false;
			return true;
		}
		if (keyData == Keys.Escape)
		{
			base.ProcessDialogKey(keyData);
			CancelEdit();
			return true;
		}
		if (keyData == Keys.Tab || keyData == (Keys.Tab | Keys.Shift))
		{
			Flush();
		}
		return base.ProcessDialogKey(keyData);
	}

	protected override void OnLayout(LayoutEventArgs levent)
	{
		if (m_context != null)
		{
			int num = 1;
			int num2 = base.Width;
			UITypeEditor uITypeEditor = WinFormsPropertyUtils.GetUITypeEditor(m_descriptor, this);
			if (uITypeEditor != null)
			{
				if (uITypeEditor.GetPaintValueSupported(this))
				{
					num += 23;
				}
				m_textBox.Left = num;
				m_textBox.Width = num2 - num - m_editButton.Width;
			}
			else
			{
				m_textBox.Left = num;
				m_textBox.Width = num2 - num;
			}
		}
		base.OnLayout(levent);
	}

	protected override void OnBackColorChanged(EventArgs e)
	{
		m_textBox.BackColor = BackColor;
		base.OnBackColorChanged(e);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		base.OnPaint(e);
		if (m_context == null)
		{
			return;
		}
		Rectangle clientRectangle = base.ClientRectangle;
		clientRectangle.Width -= m_editButton.Width;
		try
		{
			s_drawingEditableValue = true;
			using Brush brush = new SolidBrush(m_descriptor.IsReadOnly ? ReadOnlyTextColor : ForeColor);
			DrawProperty(m_descriptor, this, clientRectangle, Font, brush, e.Graphics);
		}
		finally
		{
			s_drawingEditableValue = false;
		}
	}

	protected void EnableTextBox()
	{
		m_textBox.Show();
		SetTextBoxFromProperty();
		m_textBox.SelectAll();
		m_textBox.Focus();
	}

	protected void DisableTextBox()
	{
		m_textBox.Hide();
	}

	protected override void OnMouseClick(MouseEventArgs e)
	{
		EnableTextBox();
		base.OnMouseClick(e);
	}

	protected override void OnGotFocus(EventArgs e)
	{
		EnableTextBox();
		base.OnGotFocus(e);
	}

	public override void Refresh()
	{
		if (m_textBox.Visible)
		{
			SetTextBoxFromProperty();
		}
		base.Refresh();
	}

	private void editButton_MouseDown(object sender, MouseEventArgs e)
	{
		m_absorbEditButtonClick = m_isEditing;
	}

	private void editButton_Click(object sender, EventArgs e)
	{
		if (m_absorbEditButtonClick)
		{
			m_absorbEditButtonClick = false;
			m_dropDownForm.Visible = false;
		}
		else
		{
			OpenDropDownEditor();
		}
	}

	private void textBox_LostFocus(object sender, EventArgs e)
	{
		SetPropertyFromTextBox();
		if (!m_isEditing)
		{
			DisableTextBox();
		}
	}

	private void textBox_DragOver(object sender, DragEventArgs e)
	{
		OnDragOver(e);
	}

	private void textBox_DragDrop(object sender, DragEventArgs e)
	{
		OnDragDrop(e);
	}

	private void textBox_MouseHover(object sender, EventArgs e)
	{
		OnMouseHover(e);
	}

	private void textBox_MouseLeave(object sender, EventArgs e)
	{
		OnMouseLeave(e);
	}

	private void OpenDropDownEditor()
	{
		try
		{
			m_isEditing = true;
			PropertyEditorControlContext context = m_context;
			object value;
			object obj;
			try
			{
				context.CacheSelection();
				value = m_context.GetValue();
				UITypeEditor uITypeEditor = WinFormsPropertyUtils.GetUITypeEditor(m_descriptor, this);
				obj = uITypeEditor.EditValue(this, this, value);
				if (value != obj)
				{
					context.SetValue(obj);
				}
			}
			finally
			{
				context.ClearCachedSelection();
			}
			NotifyPropertyEdit(value, obj);
			if (context == m_context)
			{
				SetTextBoxFromProperty();
				EnableTextBox();
			}
			Invalidate();
		}
		finally
		{
			m_isEditing = false;
		}
	}

	private void SetPropertyFromTextBox()
	{
		if (m_settingValue)
		{
			return;
		}
		try
		{
			m_settingValue = true;
			string text = m_textBox.Text;
			if (m_textBox.AutoCompleteEnabled)
			{
				bool flag = false;
				foreach (string item in m_textBox.AutoCompleteCustomSource)
				{
					if (string.Compare(item, text, StringComparison.CurrentCultureIgnoreCase) == 0)
					{
						text = item;
						flag = true;
						break;
					}
				}
				if (!flag && m_context != null)
				{
					text = m_initialText;
				}
			}
			bool flag2 = text != m_initialText;
			if (m_context != null && flag2)
			{
				object value = m_context.GetValue();
				if (TryConvertString(text, out var value2))
				{
					m_context.SetValue(value2);
					NotifyPropertyEdit(value, value2);
					m_initialText = text;
				}
			}
		}
		finally
		{
			m_settingValue = false;
		}
	}

	private bool TryConvertString(string newText, out object value)
	{
		bool result = false;
		value = newText;
		try
		{
			TypeConverter converter = m_descriptor.Converter;
			if (converter != null && value != null && converter.CanConvertFrom(value.GetType()))
			{
				value = converter.ConvertFrom(this, CultureInfo.CurrentCulture, value);
			}
			result = true;
		}
		catch (Exception ex)
		{
			CancelEdit();
			MessageBox.Show(ex.Message, "Error".Localize());
		}
		return result;
	}

	private void SetTextBoxFromProperty()
	{
		if (m_context != null && m_context.LastSelectedObject != null)
		{
			string text = (m_initialText = PropertyUtils.GetPropertyText(m_context.LastSelectedObject, m_descriptor));
			m_textBox.Text = text;
			m_textBox.Font = Font;
			m_textBox.ReadOnly = m_descriptor.IsReadOnly;
			m_textBox.ForeColor = (m_descriptor.IsReadOnly ? ReadOnlyTextColor : ForeColor);
		}
	}

	private void NotifyPropertyEdit(object oldValue, object newValue)
	{
		OnPropertyEdited(new PropertyEditedEventArgs(m_context.LastSelectedObject, m_descriptor, oldValue, newValue));
	}

	static PropertyEditingControl()
	{
		PaintValueBorderColor = SystemColors.ControlDark;
		s_textFormat = new ExtendedStringFormat();
		s_textFormat.Format = new StringFormat();
		s_textFormat.Format.Alignment = StringAlignment.Near;
		s_textFormat.Format.Trimming = StringTrimming.EllipsisPath;
		s_textFormat.Format.FormatFlags = StringFormatFlags.NoWrap;
		s_textFormat.CustomFlags = CustomStringFormatFlags.None;
	}
}
