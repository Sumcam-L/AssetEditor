using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.PropertyEditing;

public class NestedCollectionEditor : UITypeEditor, IPropertyEditor
{
	public delegate void CollectionChangedEventHandler(object sender, object instance, object value);

	public delegate object CreateCollectionObject(object parentProperty);

	public class EditingEventArgs : EventArgs
	{
		public readonly object Value;

		public Func<object, ItemInfo, bool> GetItemInfo;

		public Func<Path<object>, IEnumerable<Pair<Type, CreateCollectionObject>>> GetCollectionItemCreators;

		public EditingEventArgs(object value)
		{
			Value = value;
		}
	}

	private ISelectionContext m_selectionContext;

	public event CollectionChangedEventHandler CollectionChanged;

	public event EventHandler<EditingEventArgs> EditorOpening;

	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		m_selectionContext = context.TransactionContext.As<ISelectionContext>();
		return null;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		if (context != null && context.Instance != null && provider != null)
		{
			IWindowsFormsEditorService windowsFormsEditorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
			if (windowsFormsEditorService != null)
			{
				EditingEventArgs e = new EditingEventArgs(value);
				OnEditorOpening(e);
				NestedCollectionEditorForm dialog = CreateForm(context, m_selectionContext, value, e.GetCollectionItemCreators, e.GetItemInfo);
				context.OnComponentChanging();
				if (windowsFormsEditorService.ShowDialog(dialog) == DialogResult.OK)
				{
					OnCollectionChanged(context.Instance, value);
					context.OnComponentChanged();
				}
			}
		}
		return value;
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		if (context != null && context.Instance != null)
		{
			return UITypeEditorEditStyle.Modal;
		}
		return base.GetEditStyle(context);
	}

	protected virtual void OnEditorOpening(EditingEventArgs e)
	{
		this.EditorOpening.Raise(this, e);
	}

	protected virtual void OnCollectionChanged(object instance, object value)
	{
		if (this.CollectionChanged != null)
		{
			this.CollectionChanged(this, instance, value);
		}
	}

	protected virtual NestedCollectionEditorForm CreateForm(ITypeDescriptorContext context, ISelectionContext selectionContext, object value, Func<Path<object>, IEnumerable<Pair<Type, CreateCollectionObject>>> getCollectionItemCreators, Func<object, ItemInfo, bool> getItemInfo)
	{
		return new NestedCollectionEditorForm(context, selectionContext, value, getCollectionItemCreators, getItemInfo);
	}
}
