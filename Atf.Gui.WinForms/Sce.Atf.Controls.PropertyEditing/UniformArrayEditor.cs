using System;
using System.Collections;
using System.Windows.Forms;

namespace Sce.Atf.Controls.PropertyEditing;

public class UniformArrayEditor<T> : NumericEditor
{
	protected class UniformArrayTextBox : NumericTextBox, ICacheablePropertyControl
	{
		private readonly PropertyEditorControlContext m_context;

		public virtual bool Cacheable => true;

		public UniformArrayTextBox(PropertyEditorControlContext context)
			: base(typeof(T))
		{
			m_context = context;
			base.BorderStyle = BorderStyle.None;
			RefreshValue();
		}

		protected override void OnValueEdited(EventArgs e)
		{
			object value = SingleToArray(base.Value);
			m_context.SetValue(value);
			base.OnValueEdited(e);
		}

		public override void Refresh()
		{
			RefreshValue();
			base.Refresh();
		}

		private void RefreshValue()
		{
			object value = m_context.GetValue();
			if (value == null)
			{
				base.Enabled = false;
				return;
			}
			base.Value = ArrayToSingle(value);
			base.Enabled = !m_context.IsReadOnly;
		}

		private object SingleToArray(object single)
		{
			int count = ((IList)m_context.GetValue()).Count;
			T val = (T)single;
			T[] array = new T[count];
			for (int i = 0; i < count; i++)
			{
				array[i] = val;
			}
			return array;
		}

		private object ArrayToSingle(object array)
		{
			return ((T[])array)[0];
		}
	}

	public UniformArrayEditor()
		: base(typeof(T))
	{
	}

	public override Control GetEditingControl(PropertyEditorControlContext context)
	{
		UniformArrayTextBox uniformArrayTextBox = new UniformArrayTextBox(context);
		uniformArrayTextBox.ScaleFactor = base.ScaleFactor;
		return uniformArrayTextBox;
	}
}
