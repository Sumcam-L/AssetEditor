using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class LongEnumEditor : IPropertyEditor, IAnnotatedParams
{
	private class EnumEditorControl : ComboBox, ICacheablePropertyControl
	{
		private bool m_refreshing;

		private PropertyEditorControlContext m_context;

		private string[] m_names;

		private string[] m_displayNames;

		private Image[] m_images;

		bool ICacheablePropertyControl.Cacheable => false;

		public EnumEditorControl(PropertyEditorControlContext context, string[] names, string[] displayNames, Image[] images)
		{
			m_images = images;
			m_context = context;
			m_displayNames = displayNames;
			m_names = names;
			if (m_images != null && m_images.Length != 0)
			{
				base.DrawMode = DrawMode.OwnerDrawFixed;
			}
			base.IntegralHeight = false;
			string[] items = displayNames ?? names;
			BeginUpdate();
			base.Items.AddRange(items);
			EndUpdate();
			base.FlatStyle = FlatStyle.Standard;
			base.MaxDropDownItems = 6;
			base.DrawMode = DrawMode.OwnerDrawFixed;
			base.AutoCompleteMode = AutoCompleteMode.Suggest;
			base.AutoCompleteSource = AutoCompleteSource.ListItems;
			base.SelectedIndexChanged += ComboboxSelectedIndexChanged;
			SetDropDownWidth();
			base.FontChanged += delegate
			{
				SetDropDownWidth();
			};
			RefreshValue();
		}

		protected override void OnDrawItem(DrawItemEventArgs e)
		{
			if (e.Index == -1)
			{
				return;
			}
			e.DrawBackground();
			Rectangle rect = Rectangle.Empty;
			if (m_images != null && e.Index < m_images.Length)
			{
				Image image = m_images[e.Index];
				rect = e.Bounds;
				rect.Width = e.Bounds.Height;
				if (image != null)
				{
					int num = (rect.Width = Math.Min(e.Bounds.Height, image.Height));
					rect.Height = num;
					e.Graphics.DrawImage(image, rect);
				}
			}
			using SolidBrush brush = new SolidBrush(ForeColor);
			Rectangle bounds = e.Bounds;
			bounds.Width = e.Bounds.Width - rect.Width;
			bounds.X = e.Bounds.X + rect.Width;
			e.Graphics.DrawString(base.Items[e.Index].ToString(), Font, brush, bounds);
		}

		protected override bool IsInputKey(Keys keyData)
		{
			if (keyData == Keys.Up || keyData == Keys.Down)
			{
				return false;
			}
			return base.IsInputKey(keyData);
		}

		public override void Refresh()
		{
			RefreshValue();
			base.Refresh();
		}

		protected override void OnLostFocus(EventArgs e)
		{
			object value = m_context.GetValue();
			string text = null;
			TypeConverter converter = m_context.Descriptor.Converter;
			text = ((converter == null || !converter.CanConvertTo(typeof(string))) ? IdToDisplayName((string)value) : IdToDisplayName((string)converter.ConvertTo(value, typeof(string))));
			if (text != Text)
			{
				SetProperty();
			}
			base.OnLostFocus(e);
		}

		private void ComboboxSelectedIndexChanged(object sender, EventArgs e)
		{
			if (!m_refreshing)
			{
				SetProperty();
			}
		}

		private void SetProperty()
		{
			if (!m_refreshing)
			{
				string value = DisplayNameToId(Text);
				m_context.SetValue(value);
			}
		}

		private void RefreshValue()
		{
			if (m_refreshing)
			{
				return;
			}
			try
			{
				m_refreshing = true;
				object value = m_context.GetValue();
				if (value == null)
				{
					base.Enabled = false;
					return;
				}
				string text = null;
				TypeConverter converter = m_context.Descriptor.Converter;
				text = ((converter == null || !converter.CanConvertTo(typeof(string))) ? IdToDisplayName((string)value) : IdToDisplayName((string)converter.ConvertTo(value, typeof(string))));
				Text = text;
				base.Enabled = !m_context.IsReadOnly;
			}
			finally
			{
				m_refreshing = false;
			}
		}

		private string IdToDisplayName(string id)
		{
			string result = id;
			if (m_displayNames != null)
			{
				for (int i = 0; i < m_names.Length; i++)
				{
					if (id == m_names[i])
					{
						result = m_displayNames[i];
						break;
					}
				}
			}
			return result;
		}

		private string DisplayNameToId(string displayName)
		{
			string result = displayName;
			if (m_displayNames != null)
			{
				for (int i = 0; i < m_displayNames.Length; i++)
				{
					if (displayName == m_displayNames[i])
					{
						result = m_names[i];
					}
				}
			}
			return result;
		}

		private void SetDropDownWidth()
		{
			int num = 0;
			if (m_images != null && m_images.Length != 0)
			{
				Image[] images = m_images;
				foreach (Image image in images)
				{
					if (image != null && image.Width > num)
					{
						num = image.Width;
					}
				}
			}
			string[] array = m_displayNames ?? m_names;
			int num2 = 0;
			string[] array2 = array;
			foreach (string text in array2)
			{
				Size size = TextRenderer.MeasureText(text, Font);
				if (size.Width > num2)
				{
					num2 = size.Width;
				}
			}
			base.DropDownWidth = num + num2;
		}
	}

	private int m_maxDropDownItems = 6;

	private string[] m_names;

	private string[] m_displayNames;

	private Image[] m_images;

	public bool TextEditEnabled { get; set; }

	public int MaxDropDownItems
	{
		get
		{
			return m_maxDropDownItems;
		}
		set
		{
			m_maxDropDownItems = value;
			if (m_maxDropDownItems < 1)
			{
				m_maxDropDownItems = 1;
			}
		}
	}

	public LongEnumEditor()
	{
	}

	public LongEnumEditor(Type enumType, Image[] images = null)
	{
		if (!enumType.IsEnum)
		{
			throw new ArgumentException("enumType must Enum");
		}
		m_names = Enum.GetNames(enumType);
		m_images = images;
	}

	public void DefineEnum(string[] names, Image[] images = null)
	{
		if (names == null || names.Length == 0)
		{
			throw new ArgumentException();
		}
		EnumUtil.ParseEnumDefinitions(names, out m_names, out m_displayNames, out var _);
		if (m_names.SequenceEqual(m_displayNames))
		{
			m_displayNames = null;
		}
		m_images = images;
	}

	public virtual Control GetEditingControl(PropertyEditorControlContext context)
	{
		EnumEditorControl enumEditorControl = new EnumEditorControl(context, m_names, m_displayNames, m_images);
		enumEditorControl.DropDownStyle = (((context == null || context.Descriptor.Converter == null) && TextEditEnabled) ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList);
		enumEditorControl.MaxDropDownItems = MaxDropDownItems;
		SkinService.ApplyActiveSkin(enumEditorControl);
		return enumEditorControl;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}

	void IAnnotatedParams.Initialize(string[] parameters)
	{
		DefineEnum(parameters);
	}
}
