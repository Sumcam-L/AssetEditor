using System;
using System.Linq;
using Firaxis.AssetEditing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetPreviewing;

public class SplineSetWidgetAdapter : BehaviorComponentAdapterBase
{
	private ISelectionContext SplineSetSelection { get; set; }

	private IPreviewerWidgetService PreviewerWidgetService { get; set; }

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		SplineSetSelection = base.DomNode.As<ISelectionContext>();
		if (SplineSetSelection != null)
		{
			SplineSetSelection.SelectionChanging += SplineSetSelection_SelectionChanging;
			SplineSetSelection.SelectionChanged += SplineSetSelection_SelectionChanged;
		}
	}

	private void SplineSetSelection_SelectionChanging(object sender, EventArgs e)
	{
		if (FiraxisATFRegistry.PreviewerWidgetService?.ActiveWidgetEditor is SplineWidgetEditor splineWidgetEditor)
		{
			splineWidgetEditor.Spline = null;
		}
	}

	private void SplineSetSelection_SelectionChanged(object sender, EventArgs e)
	{
		SplineAdapter splineAdapter = SplineSetSelection?.Selection.FirstOrDefault().As<SplineAdapter>();
		if (splineAdapter != null)
		{
			IPreviewerWidgetService previewerWidgetService = FiraxisATFRegistry.PreviewerWidgetService;
			previewerWidgetService.SetActiveWidgetEditor(SplineWidgetEditor.WidgetName);
			SplineWidgetEditor splineWidgetEditor = previewerWidgetService?.ActiveWidgetEditor as SplineWidgetEditor;
			BugSubmitter.SilentAssert(splineWidgetEditor != null, "SplineSetWidgetAdapter failed to make the SplineWidgetEditor active. @assign bwhitman");
			splineWidgetEditor.Spline = splineAdapter;
		}
	}
}
