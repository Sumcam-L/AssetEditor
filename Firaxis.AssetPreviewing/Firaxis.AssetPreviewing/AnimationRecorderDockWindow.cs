using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.ATF;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IInitializable))]
[Export(typeof(AnimationRecorderDockWindow))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AnimationRecorderDockWindow : IInitializable, IControlHostClient, IDisposable
{
	private readonly IControlHostService m_controlHostService;

	private readonly AnimationRecorderControl m_recorderControl;

	[ImportingConstructor]
	public AnimationRecorderDockWindow(IAnimationRecorderService animationRecorderService, IControlHostService controlHostService)
	{
		m_controlHostService = controlHostService;
		m_recorderControl = new AnimationRecorderControl(animationRecorderService);
	}

	public void Activate(Control control)
	{
	}

	public bool Close(Control control)
	{
		return true;
	}

	public void Deactivate(Control control)
	{
	}

	public void Dispose()
	{
		m_recorderControl.Dispose();
	}

	public void Initialize()
	{
		m_controlHostService.RegisterControl(m_recorderControl, "Animation Recorder", "Enables the recording of animations from the asset currently being previewed.", StandardControlGroup.Bottom, this);
		m_controlHostService.Show(m_recorderControl);
	}
}
