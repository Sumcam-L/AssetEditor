using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public partial class PropertyGridView : PropertyView, IComponentConnector
{
	public IPropertyEditingContext Context
	{
		get
		{
			return m_propertyGrid.DataContext.As<IPropertyEditingContext>();
		}
		set
		{
			m_propertyGrid.DataContext = null;
			m_propertyGrid.DataContext = value;
		}
	}

	public PropertyGridView()
	{
		InitializeComponent();
	}

	protected override void observableContext_Reloaded(object sender, EventArgs e)
	{
		base.observableContext_Reloaded(sender, e);
		Context = base.EditingContext;
	}

	private void m_propertyGrid_PropertyEdited(object sender, PropertyEditedEventArgs e)
	{
		IHistoryContext historyContext = base.EditingContext.As<IHistoryContext>();
		if (historyContext != null)
		{
			historyContext.Dirty = true;
		}
	}

}
