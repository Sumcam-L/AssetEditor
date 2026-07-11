using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Sce.Atf.Applications.Controls;

namespace Sce.Atf.Applications;

[Export(typeof(PerformanceMonitor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PerformanceMonitor : IControlHostClient, IInitializable
{
	public class TargetChangeEventArgs : CancelEventArgs
	{
		public IPerformanceTarget Target;

		public string Name;

		public TargetChangeEventArgs(IPerformanceTarget target, string name)
		{
			Target = target;
			Name = name;
		}
	}

	public delegate void TargetChangeEventHandler(object source, TargetChangeEventArgs e);

	private readonly PerformanceMonitorControl m_perfControl = new PerformanceMonitorControl();

	private readonly ControlInfo m_controlInfo;

	private readonly IControlRegistry m_controlRegistry;

	private readonly IControlHostService m_controlHostService;

	protected PerformanceMonitorControl PerformanceMonitorControl => m_perfControl;

	protected ControlInfo ControlInfo => m_controlInfo;

	public event TargetChangeEventHandler TargetChanging;

	[ImportingConstructor]
	public PerformanceMonitor(IControlRegistry controlRegistry, IControlHostService controlHostService)
	{
		m_controlRegistry = controlRegistry;
		m_controlHostService = controlHostService;
		m_controlInfo = new ControlInfo("Performance Monitor".Localize(), "Displays performance data on the currently active Control".Localize(), StandardControlGroup.Floating);
		m_controlInfo.ControlVisibility = ControlInitialVisibility.AlwaysHidden;
		m_controlInfo.MenuText = "Tool Debug\\Performance Monitor";
		m_controlInfo.MenuGroupOverride = StandardCommandGroup.UILayout;
	}

	public PerformanceMonitor()
	{
	}

	void IInitializable.Initialize()
	{
		m_controlHostService.RegisterControl(m_perfControl, m_controlInfo, this);
		m_controlRegistry.ActiveControlChanged += ActiveControlChanged;
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

	public void SetTarget(object target, string name)
	{
		IPerformanceTarget performanceTarget = target as IPerformanceTarget;
		if (performanceTarget == null && target is Control control)
		{
			performanceTarget = new PerformanceMonitorControl.ControlAdapter(control);
		}
		TargetChangeEventArgs e = new TargetChangeEventArgs(performanceTarget, name);
		OnTargetChanging(e);
		if (!e.Cancel)
		{
			m_perfControl.Bind(e.Target, e.Name);
		}
	}

	protected virtual void OnTargetChanging(TargetChangeEventArgs e)
	{
		if (this.TargetChanging == null)
		{
			return;
		}
		Delegate[] invocationList = this.TargetChanging.GetInvocationList();
		foreach (Delegate obj in invocationList)
		{
			obj.DynamicInvoke(this, e);
			if (e.Cancel)
			{
				break;
			}
		}
	}

	private void ActiveControlChanged(object sender, EventArgs e)
	{
		ControlInfo activeControl = m_controlRegistry.ActiveControl;
		if (activeControl != null && activeControl.Control != m_perfControl)
		{
			SetTarget(activeControl.Control, activeControl.Name);
		}
	}
}
