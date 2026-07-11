using System;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class BoolEditor : IPropertyEditor
{
	private class BoolControl : Control, ICacheablePropertyControl
	{
		private readonly PropertyEditorControlContext m_context;

		private readonly CheckBox m_checkBox;

		private bool m_refreshing;

		private const int m_topAndLeftMargin = 2;

		public bool Cacheable => true;

		public bool DisableEditing { get; set; }

		public BoolControl(PropertyEditorControlContext context)
		{
			m_context = context;
			m_checkBox = new CheckBox();
			m_checkBox.Size = m_checkBox.PreferredSize;
			m_checkBox.CheckAlign = ContentAlignment.MiddleLeft;
			m_checkBox.CheckedChanged += checkBox_CheckedChanged;
			m_checkBox.Dock = DockStyle.Top;
			base.Controls.Add(m_checkBox);
			base.Height = m_checkBox.Height + 2;
			RefreshValue();
		}

		public override void Refresh()
		{
			RefreshValue();
			base.Refresh();
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			m_checkBox.Location = new Point(2, (base.Height - m_checkBox.Height) / 2 + 1);
			base.OnSizeChanged(e);
		}

		private void checkBox_CheckedChanged(object sender, EventArgs e)
		{
			if (!m_refreshing)
			{
				bool flag = m_checkBox.Checked;
				m_context.SetValue(flag);
			}
		}

		private void RefreshValue()
		{
			try
			{
				m_refreshing = true;
				object value = m_context.GetValue();
				if (value == null)
				{
					m_checkBox.Enabled = false;
					return;
				}
				m_checkBox.Checked = (bool)value;
				m_checkBox.Enabled = !m_context.IsReadOnly && !DisableEditing;
			}
			finally
			{
				m_refreshing = false;
			}
		}
	}

	private BoolControl m_boolControl;

	public bool DisableEditing
	{
		get
		{
			return m_boolControl != null && m_boolControl.DisableEditing;
		}
		set
		{
			if (m_boolControl != null)
			{
				m_boolControl.DisableEditing = value;
			}
		}
	}

	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		m_boolControl = new BoolControl(context);
		SkinService.ApplyActiveSkin(m_boolControl);
		return m_boolControl;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}
}
