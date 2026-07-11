using System.Drawing;
using System.Windows.Forms;
using Firaxis.AssetEditing.Entities.Asset;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class AnimationBindingEditor : IPropertyEditor
{
	private readonly IAssetBrowserDialogService m_assetBrowser;

	private readonly ICivTechService m_civtechService;

	private ITimelinePlaybackService m_timelinePlaybackService;

	private IAnimationKnobService m_animationKnobService;

	private AnimationBindingControl m_objectControl;

	public AnimationBindingEditor(ICivTechService civtechService, IAssetBrowserDialogService assetBrowserSvc, ITimelinePlaybackService timelinePlaybackSvc, IAnimationKnobService animationKnobSvc)
	{
		m_civtechService = civtechService;
		m_assetBrowser = assetBrowserSvc;
		m_timelinePlaybackService = timelinePlaybackSvc;
		m_animationKnobService = animationKnobSvc;
	}

	public Control GetEditingControl(PropertyEditorControlContext context)
	{
		m_objectControl = new AnimationBindingControl(context, m_civtechService, m_assetBrowser, m_timelinePlaybackService, m_animationKnobService);
		SkinService.ApplyActiveSkin(m_objectControl);
		return m_objectControl;
	}

	public SizeF GetDesiredSize(Graphics g, Font f)
	{
		return new SizeF(0f, 0f);
	}
}
