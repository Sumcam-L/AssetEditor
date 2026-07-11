using System;
using System.ComponentModel.Composition;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IInitializable))]
[Export(typeof(IControlHostClient))]
[Export(typeof(IPreviewSetService))]
[Export(typeof(IPreviewerKnobService))]
[Export(typeof(PreviewerKnobService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PreviewerKnobService : IInitializable, IPreviewerKnobService, ISequencedProjectChangeWatcher, IControlHostClient, IPreviewSetService, IDisposable
{
	private string ActivePreviewEntityName = string.Empty;

	private string ActivePreviewModule = string.Empty;

	private string ActiveEntityPreviewModule = string.Empty;

	private string ActiveEntityClass = string.Empty;

	private readonly DockHostControl PreviewerKnobsControl;

	private readonly KnobSetEditingControl GlobalKnobsControl;

	private readonly KnobSetEditingControl EntityKnobsControl;

	private readonly ICivTechService CivTechService;

	private readonly IControlHostService ControlHostService;

	private readonly IPreviewerEntityLoadingService PreviewerEntityLoadingService;

	private readonly IThemeService ThemeService;

	private readonly IAssetPreviewer AssetPreviewer;

	private readonly IKnobManager KnobManager;

	private IKnobSet GlobalKnobSet { get; set; }

	private IKnobSet EntityKnobSet { get; set; }

	private string m_appliedGeneralLayout = string.Empty;

	private string m_appliedGlobalLayout = string.Empty;

	private string m_appliedEntityLayout = string.Empty;

	private bool m_hasAppliedGeneralLayout;

	[Import(AllowDefault = true)]
	private ISettingsService SettingsService { get; set; }

	private PreviewerLayout PreviewLayouts { get; set; } = new PreviewerLayout();

	private string PreviewerLayoutData
	{
		get
		{
			return PreviewLayouts.SerializedLayouts;
		}
		set
		{
			PreviewLayouts.SerializedLayouts = value;
		}
	}

	[ImportingConstructor]
	public PreviewerKnobService(ICivTechService civTechSvc, IPreviewerEntityLoadingService loaderSvc, IControlHostService ctlHostSvc, IThemeService themeSvc, IAssetPreviewer previewer, IKnobManager knobMgr)
	{
		CivTechService = civTechSvc;
		PreviewerEntityLoadingService = loaderSvc;
		ControlHostService = ctlHostSvc;
		ThemeService = themeSvc;
		AssetPreviewer = previewer;
		KnobManager = knobMgr;
		PreviewerKnobsControl = new DockHostControl(themeSvc);
		IntPtr handle = PreviewerKnobsControl.Handle;
		PreviewerKnobsControl.Dock = DockStyle.Fill;
		GlobalKnobsControl = new KnobSetEditingControl(themeSvc);
		handle = GlobalKnobsControl.Handle;
		GlobalKnobsControl.Dock = DockStyle.Fill;
		EntityKnobsControl = new KnobSetEditingControl(themeSvc);
		handle = EntityKnobsControl.Handle;
		EntityKnobsControl.Dock = DockStyle.Fill;
		PreviewerKnobsControl.AddDockContext(GlobalKnobsControl, "Global", "Global", "", DockState.Document, canClose: false, show: false);
		PreviewerKnobsControl.AddDockContext(EntityKnobsControl, "Entity", "Entity", "", DockState.Document, canClose: false, show: false);
	}

	public void SetActivePreviewModule(string moduleName)
	{
		if (!(moduleName == ActivePreviewModule))
		{
			SaveGlobalPreviewLayout(ActivePreviewModule);
			if (!string.IsNullOrEmpty(moduleName))
			{
				GlobalKnobSet = KnobManager.GetKnobSet(moduleName);
			}
			else
			{
				GlobalKnobSet = null;
			}
			AddActiveKnobs(GlobalKnobSet, GlobalKnobsControl);
			ActivePreviewModule = moduleName;
			LoadGlobalPreviewLayout(ActivePreviewModule);
		}
	}

	public void ClearIfActive(IPreviewContext previewContext)
	{
		if (previewContext != null && previewContext.EntityAdapter != null)
		{
			string text = $"{previewContext.EntityAdapter.Name}_{previewContext.EntityAdapter.Name}_0";
			if (!(ActivePreviewEntityName != text))
			{
				SetActivePreviewModule(null);
				SetActiveEntity(null);
			}
		}
	}

	public void SetActiveEntity(IPreviewContext previewContext)
	{
		string text = "Entity";
		if (previewContext != null)
		{
			text = previewContext.EntityAdapter.Name;
			string text2 = $"{text}_{text}_0";
			if (EntityKnobSet != null && ActivePreviewEntityName == EntityKnobSet.KnobSetName && ActivePreviewEntityName == text2 && previewContext.PreviewModule == previewContext.PreviewWindow.GetPreviewModule())
			{
				return;
			}
			ActivePreviewEntityName = text2;
			IKnobSet knobSet = KnobManager.GetKnobSet(ActivePreviewEntityName);
			if (knobSet != null)
			{
				EntityKnobSet = new BehaviorKnobSetWrapper(CivTechService.PrimaryProject.Config, PreviewerEntityLoadingService, previewContext, knobSet);
			}
			else
			{
				EntityKnobSet = null;
			}
		}
		else
		{
			EntityKnobSet = null;
		}
		SaveClassPreviewLayout(ActiveEntityPreviewModule, ActiveEntityClass);
		AddActiveKnobs(EntityKnobSet, EntityKnobsControl);
		PreviewerKnobsControl.UpdateDockContextText(EntityKnobsControl, text);
		ActiveEntityPreviewModule = previewContext?.PreviewModule;
		ActiveEntityClass = previewContext?.EntityAdapter.ClassName;
		LoadClassPreviewLayout(ActiveEntityPreviewModule, ActiveEntityClass);
	}

	private void AddActiveKnobs(IKnobSet knobSet, KnobSetEditingControl knobPropControl)
	{
		knobPropControl.ActiveKnobSet = knobSet;
		if (knobSet != null)
		{
			knobPropControl.Invalidate();
		}
	}

	private void CleanupActiveKnobs()
	{
		GlobalKnobsControl.ActiveKnobSet = null;
		EntityKnobsControl.ActiveKnobSet = null;
	}

	public void Dispose()
	{
	}

	public void Initialize()
	{
		ApplyInitialGeneralLayout();
		ControlHostService.RegisterControl(PreviewerKnobsControl, "Asset Previewer Controls", "Asset previewing Controls", StandardControlGroup.Floating, null, this);
		ControlHostService.Show(PreviewerKnobsControl);
		if (SettingsService != null)
		{
			BoundPropertyDescriptor boundPropertyDescriptor = new BoundPropertyDescriptor(this, () => PreviewerLayoutData, "Previewer Layout Data", "Previewer".Localize(), "Layout data per previewer per entity class.".Localize());
			SettingsService.RegisterSettings("PreviewerLayoutData", boundPropertyDescriptor);
		}
	}

	private void ApplyInitialGeneralLayout()
	{
		foreach (string previewModule in PreviewLayouts.ModuleNames)
		{
			string generalLayout = PreviewLayouts[previewModule].GeneralLayout;
			if (string.IsNullOrEmpty(generalLayout))
			{
				continue;
			}
			try
			{
				PaintTimingLog.Write("PreviewerKnobService: apply general layout before host show");
				PreviewerKnobsControl.LayoutState = generalLayout;
				m_appliedGeneralLayout = generalLayout;
				m_hasAppliedGeneralLayout = true;
				return;
			}
			catch (System.Exception exObj)
			{
				BugSubmitter.SilentException(exObj);
			}
		}
		PreviewerKnobsControl.ShowDockContext(GlobalKnobsControl, DockState.Document);
		PreviewerKnobsControl.ShowDockContext(EntityKnobsControl, DockState.Document);
	}

	public void Activate(Control control)
	{
	}

	public void Deactivate(Control control)
	{
	}

	public bool Close(Control control)
	{
		return true;
	}

	private void UpdateSlotKnobs(IKnobSet knobSet, AssetData data)
	{
		knobSet.As<ISlottedKnobSet>()?.ApplySlotData(data);
	}

	public void ApplyPreviewSetData(PreviewSetData data)
	{
		KnobData.UpdateKnobSetData(data.PreviewerKnobSetData, GlobalKnobSet, checkNameConsistency: true);
		foreach (AssetData openedAsset in data.OpenedAssets)
		{
			UpdateSlotKnobs(EntityKnobSet, openedAsset);
		}
	}

	public PreviewSetData GeneratePreviewSetData()
	{
		return new PreviewSetData();
	}

	public void StartProjectChange(Action<string> statusMessagePrinter)
	{
		CleanupActiveKnobs();
	}

	public void FinishProjectChange(Action<string> statusMessagePrinter)
	{
	}

	private void LoadGlobalPreviewLayout(string previewModule)
	{
		if (string.IsNullOrEmpty(previewModule))
		{
			return;
		}
		string generalLayout = PreviewLayouts[previewModule].GeneralLayout;
		string globalLayout = PreviewLayouts[previewModule].GlobalLayout;
		if (!string.IsNullOrEmpty(generalLayout))
		{
			try
			{
				if (!m_hasAppliedGeneralLayout)
				{
					PaintTimingLog.Write("PreviewerKnobService: apply general layout initial");
					PreviewerKnobsControl.LayoutState = generalLayout;
					m_appliedGeneralLayout = generalLayout;
					m_hasAppliedGeneralLayout = true;
				}
				else if (m_appliedGeneralLayout != generalLayout)
				{
					PaintTimingLog.Write("PreviewerKnobService: skip general layout during module switch");
					m_appliedGeneralLayout = generalLayout;
				}
			}
			catch (System.Exception exObj)
			{
				BugSubmitter.SilentException(exObj);
			}
		}
		if (string.IsNullOrEmpty(globalLayout))
		{
			return;
		}
		try
		{
			if (m_appliedGlobalLayout != globalLayout)
			{
				GlobalKnobsControl.LayoutState = globalLayout;
				m_appliedGlobalLayout = globalLayout;
			}
		}
		catch (System.Exception exObj2)
		{
			BugSubmitter.SilentException(exObj2);
		}
	}

	private void LoadClassPreviewLayout(string previewModule, string entityClass)
	{
		if (string.IsNullOrEmpty(previewModule) || string.IsNullOrEmpty(entityClass))
		{
			return;
		}
		string text = PreviewLayouts[previewModule][entityClass];
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		try
		{
			if (m_appliedEntityLayout != text)
			{
				EntityKnobsControl.LayoutState = text;
				m_appliedEntityLayout = text;
			}
		}
		catch (System.Exception exObj)
		{
			BugSubmitter.SilentException(exObj);
		}
	}

	private void SaveGlobalPreviewLayout(string previewModule)
	{
		if (!string.IsNullOrEmpty(previewModule))
		{
			PreviewLayouts[previewModule].GlobalLayout = GlobalKnobsControl.LayoutState;
			PreviewLayouts[previewModule].GeneralLayout = PreviewerKnobsControl.LayoutState;
			m_appliedGlobalLayout = PreviewLayouts[previewModule].GlobalLayout;
			m_appliedGeneralLayout = PreviewLayouts[previewModule].GeneralLayout;
			SettingsService?.SaveSettings();
		}
	}

	private void SaveClassPreviewLayout(string previewModule, string entityClass)
	{
		if (!string.IsNullOrEmpty(previewModule) && !string.IsNullOrEmpty(entityClass))
		{
			PreviewLayouts[previewModule][entityClass] = EntityKnobsControl.LayoutState;
			m_appliedEntityLayout = PreviewLayouts[previewModule][entityClass];
			SettingsService?.SaveSettings();
		}
	}
}
