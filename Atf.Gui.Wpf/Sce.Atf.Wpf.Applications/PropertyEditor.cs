using System;
using System.ComponentModel.Composition;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Controls.PropertyEditing;

namespace Sce.Atf.Wpf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(IControlHostClient))]
[Export(typeof(PropertyEditor))]
[PartCreationPolicy(CreationPolicy.Any)]
public class PropertyEditor : IInitializable, IControlHostClient
{
	private ControlDef m_controlDef;

	private readonly PropertyGridView m_propertyGridView = new PropertyGridView();

	private readonly SelectionPropertyEditingContext m_defaultContext = new SelectionPropertyEditingContext();

	private static Guid s_propertyGridId = new Guid("047BA5E9-2A48-4B1E-9362-1843965E5476");

	protected IControlHostService ControlHostService { get; private set; }

	protected IContextRegistry ContextRegistry { get; private set; }

	[ImportingConstructor]
	public PropertyEditor(IControlHostService controlHostService, IContextRegistry contextRegistry)
	{
		ControlHostService = controlHostService;
		ContextRegistry = contextRegistry;
	}

	public virtual void Initialize()
	{
		ContextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
		m_controlDef = new ControlDef
		{
			Name = "Property Editor".Localize(),
			Description = "Edits selected object properties".Localize(),
			Group = StandardControlGroup.Right,
			Id = s_propertyGridId.ToString()
		};
		ControlHostService.RegisterControl(m_controlDef, m_propertyGridView, this);
	}

	void IControlHostClient.Activate(object control)
	{
	}

	void IControlHostClient.Deactivate(object control)
	{
	}

	bool IControlHostClient.Close(object control, bool mainWindowClosing)
	{
		return true;
	}

	private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		IPropertyEditingContext context = GetContext();
		m_propertyGridView.EditingContext = context;
	}

	private IPropertyEditingContext GetContext()
	{
		IPropertyEditingContext propertyEditingContext = ContextRegistry.GetMostRecentContext<IPropertyEditingContext>();
		if (propertyEditingContext != null)
		{
			m_defaultContext.SelectionContext = null;
		}
		else
		{
			ISelectionContext mostRecentContext = ContextRegistry.GetMostRecentContext<ISelectionContext>();
			m_defaultContext.SelectionContext = mostRecentContext;
			if (mostRecentContext != null)
			{
				propertyEditingContext = m_defaultContext;
			}
		}
		return propertyEditingContext;
	}
}
