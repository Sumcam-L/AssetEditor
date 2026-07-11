using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Controls.CurveEditing;

[Export(typeof(CurveEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class CurveEditor : IInitializable, IControlHostClient
{
	private readonly IControlHostService m_controlHostService;

	private readonly IContextRegistry m_contextRegistry;

	[Import(AllowDefault = true)]
	private ISettingsService m_settingsService;

	private ISelectionContext m_selectionContext;

	private IObservableContext m_observableContext;

	private IValidationContext m_validationContext;

	private bool m_curveItemChanged;

	private readonly CurveEditingControl m_curveEditorControl;

	private readonly ControlInfo m_controlInfo;

	private readonly Dictionary<ICurve, Color> m_originalCurveColors = new Dictionary<ICurve, Color>();

	public CurveEditingControl Control => m_curveEditorControl;

	public bool MultiSelectionOverlay { get; set; }

	[ImportingConstructor]
	public CurveEditor(ICommandService commandService, IControlHostService controlHostService, IContextRegistry contextRegistry)
	{
		m_controlHostService = controlHostService;
		m_contextRegistry = contextRegistry;
		m_curveEditorControl = new CurveEditingControl();
		m_controlInfo = new ControlInfo("Curve Editor".Localize(), "Edits selected object curves".Localize(), StandardControlGroup.Bottom);
	}

	public CurveEditor(ICommandService commandService, IControlHostService controlHostService, IContextRegistry contextRegistry, CurveEditingControl curveEditingControl, ControlInfo controlInfo)
	{
		m_controlHostService = controlHostService;
		m_contextRegistry = contextRegistry;
		m_curveEditorControl = curveEditingControl;
		m_controlInfo = controlInfo;
	}

	void IInitializable.Initialize()
	{
		m_contextRegistry.ActiveContextChanged += contextRegistry_ActiveContextChanged;
		m_controlHostService.RegisterControl(m_curveEditorControl, m_controlInfo, this);
		if (m_settingsService != null)
		{
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(m_curveEditorControl, () => m_curveEditorControl.InputMode, "Input mode".Localize(), null, null));
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(m_curveEditorControl, () => m_curveEditorControl.LockOrigin, "Lock origin".Localize("This is the name of a command. Lock is a verb. Origin is like the origin of a graph."), null, null));
			m_settingsService.RegisterSettings(this, new BoundPropertyDescriptor(m_curveEditorControl, () => m_curveEditorControl.FlipY, "Flip Y-axis".Localize("same as 'flip vertical axis'"), null, null));
		}
	}

	void IControlHostClient.Activate(Control control)
	{
	}

	void IControlHostClient.Deactivate(Control control)
	{
	}

	bool IControlHostClient.Close(Control control)
	{
		return true;
	}

	private void contextRegistry_ActiveContextChanged(object sender, EventArgs e)
	{
		m_curveEditorControl.Context = m_contextRegistry.ActiveContext;
		if (m_selectionContext != null)
		{
			m_selectionContext.SelectionChanged -= selectionContext_SelectionChanged;
		}
		m_selectionContext = m_contextRegistry.GetActiveContext<ISelectionContext>();
		if (m_selectionContext != null)
		{
			m_selectionContext.SelectionChanged += selectionContext_SelectionChanged;
		}
		if (m_validationContext != null)
		{
			m_validationContext.Ended -= RefreshCurveControl;
			m_validationContext.Cancelled -= RefreshCurveControl;
		}
		m_validationContext = m_contextRegistry.GetActiveContext<IValidationContext>();
		if (m_validationContext != null)
		{
			m_validationContext.Ended += RefreshCurveControl;
			m_validationContext.Cancelled += RefreshCurveControl;
		}
		if (m_observableContext != null)
		{
			m_observableContext.ItemChanged -= ObservableContextItemChanged;
		}
		m_observableContext = m_contextRegistry.GetActiveContext<IObservableContext>();
		if (m_observableContext != null)
		{
			m_observableContext.ItemChanged += ObservableContextItemChanged;
		}
	}

	private void RefreshCurveControl(object sender, EventArgs e)
	{
		if (m_curveItemChanged)
		{
			m_curveEditorControl.Refresh();
		}
		m_curveItemChanged = false;
	}

	private void selectionContext_SelectionChanged(object sender, EventArgs e)
	{
		IList<ICurve> list2;
		if (MultiSelectionOverlay)
		{
			foreach (KeyValuePair<ICurve, Color> originalCurveColor in m_originalCurveColors)
			{
				originalCurveColor.Key.CurveColor = originalCurveColor.Value;
			}
			m_originalCurveColors.Clear();
			List<ICurve> list = new List<ICurve>();
			foreach (object item in m_selectionContext.Selection)
			{
				list.AddRange(GetCurves(item));
			}
			if (m_selectionContext.SelectionCount > 1)
			{
				for (int i = 0; i < list.Count; i++)
				{
					ICurve curve = list[i];
					float h = (float)i * 360f / (float)list.Count;
					m_originalCurveColors[curve] = curve.CurveColor;
					curve.CurveColor = ColorUtil.FromAhsb(255, h, 1f, 0.5f);
				}
			}
			list2 = list;
		}
		else
		{
			list2 = GetCurves(m_selectionContext.LastSelected);
		}
		foreach (ICurve item2 in list2)
		{
			CurveUtils.ComputeTangent(item2);
		}
		m_curveEditorControl.Curves = new ReadOnlyCollection<ICurve>(list2);
	}

	private static IList<ICurve> GetCurves(object selectedObject)
	{
		Path<object> path = selectedObject as Path<object>;
		object reference = ((path != null) ? path.Last : selectedObject);
		ICurveSet curveSet = reference.As<ICurveSet>();
		if (curveSet != null && curveSet.Curves != null)
		{
			return curveSet.Curves;
		}
		ICurve curve = reference.As<ICurve>();
		if (curve != null)
		{
			return new List<ICurve> { curve };
		}
		return new List<ICurve>();
	}

	private void ObservableContextItemChanged(object sender, ItemChangedEventArgs<object> e)
	{
		m_curveItemChanged = e.Item.Is<ICurve>() || e.Item.Is<IControlPoint>();
	}
}
