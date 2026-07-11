using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.CivTech.Packages;
using Firaxis.Reflection;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetPreviewing;

[Export(typeof(PreviewerDocumentService))]
[Export(typeof(IPreviewerDocumentService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PreviewerDocumentService : IPreviewerDocumentService, ISequencedProjectChangeWatcher
{
	[Import(AllowDefault = true)]
	private IAnimationRecorderService AnimationRecorderService;

	private IDictionary<IPreviewableDocument, IPreviewWindow> PreviewWindows = new Dictionary<IPreviewableDocument, IPreviewWindow>();

	private List<IPreviewableDocument> ProjectChangeCache = new List<IPreviewableDocument>();

	private IDocumentRegistry DocumentRegistry { get; set; }

	private IAssetPreviewer AssetPreviewer { get; set; }

	private IPreviewerControlHost PreviewerControlHost { get; set; }

	private IPreviewerKnobService KnobService { get; set; }

	private IAnimationKnobService AnimationKnobService { get; set; }

	private IPreviewerWidgetService PreviewerWidgetService { get; set; }

	private IPreviewerEntityLoadingService PreviewerEntityLoadingService { get; set; }

	private IPreviewWindow ActiveWindow { get; set; }

	private ICivTechService CivTechService { get; set; }

	private IPreviewDisplay ActiveDisplay { get; set; }

	private IPreviewWindow KnobChangeWindow { get; set; }

	private IPreviewableDocument KnobChangeDocument { get; set; }

	[ImportingConstructor]
	public PreviewerDocumentService(ICivTechService civTechSvc, IPreviewerEntityLoadingService loaderSvc, IDocumentRegistry docReg, IAssetPreviewer previewer, IPreviewDisplay previewDisplay, IPreviewerKnobService knobSvc, IAnimationKnobService animKnobSvc, IPreviewerControlHost prevCtlHost, IPreviewerWidgetService widgetSvc)
	{
		DocumentRegistry = docReg;
		DocumentRegistry.DocumentAdded += DocumentRegistry_DocumentAdded;
		DocumentRegistry.DocumentRemoved += DocumentRegistry_DocumentRemoved;
		DocumentRegistry.ActiveDocumentChanging += DocumentRegistry_ActiveDocumentChanging;
		DocumentRegistry.ActiveDocumentChanged += DocumentRegistry_ActiveDocumentChanged;
		AssetPreviewer = previewer;
		ActiveDisplay = previewDisplay;
		KnobService = knobSvc;
		PreviewerControlHost = prevCtlHost;
		AnimationKnobService = animKnobSvc;
		PreviewerWidgetService = widgetSvc;
		PreviewerEntityLoadingService = loaderSvc;
		CivTechService = civTechSvc;
	}

	public void ApplyWorkspaceChanges(IEnumerable<Uri> changedFiles)
	{
		foreach (Uri changedFile in changedFiles)
		{
			if (StaticMethods.GetInstanceNameAndType(CivTechService.ProjectMapService, changedFile.LocalPath, out var instanceName, out var type) && !string.IsNullOrEmpty(instanceName) && type != InstanceType.IT_INVALID && type != InstanceType.IT_COUNT)
			{
				AssetPreviewer.ReloadEntity(type, instanceName);
			}
		}
	}

	public IPreviewWindow GetWindowForDocument(IDocument doc)
	{
		return GetPreviewDocumentOrSurrogate(doc)?.PreviewWindow;
	}

	private void DocumentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		IPreviewableDocument previewableDocument = e.Item.As<IPreviewableDocument>();
		if (previewableDocument != null)
		{
			IPreviewWindow previewWindow = (PreviewWindows[previewableDocument] = AssetPreviewer.OpenWindow(PreviewerControlHost.PreviewerControl.Handle, PreviewerEntityLoadingService.InstanceSet));
			IPreviewWindow previewWnd = (previewableDocument.PreviewWindow = previewWindow);
			InitializeDocumentPreviewing(previewableDocument, previewWnd);
		}
	}

	private void DocumentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
	{
		if (UnbindSurrogatePreviewer(e.Item))
		{
			return;
		}
		IPreviewableDocument previewableDocument = e.Item.As<IPreviewableDocument>();
		if (previewableDocument != null)
		{
			if (!PreviewWindows.ContainsKey(previewableDocument))
			{
				BugSubmitter.SilentAssert(condition: false, $"Removing untracked preview window associated with \"{previewableDocument.Uri.LocalPath}\" @summary Removing untracked preview window!");
				return;
			}
			IPreviewWindow previewWindow = PreviewWindows[previewableDocument];
			ShutdownDocumentPreviewing(previewableDocument, previewWindow);
			AssetPreviewer.CloseWindow(previewWindow);
			PreviewWindows.Remove(previewableDocument);
		}
	}

	private bool UnbindSurrogatePreviewer(IDocument doc)
	{
		IPreviewableDocument previewableDocument = FindFirstPreviewableDependent(doc);
		if (previewableDocument != null)
		{
			ActiveDisplay.UnbindWindow();
			ActiveWindow = null;
			return true;
		}
		return false;
	}

	private void PreviewDoc_UriChanged(object sender, UriChangedEventArgs e)
	{
		IPreviewableDocument previewDocumentOrSurrogate = GetPreviewDocumentOrSurrogate(DocumentRegistry.ActiveDocument);
		if (previewDocumentOrSurrogate != null)
		{
			IPreviewWindow previewWindow = PreviewWindows[previewDocumentOrSurrogate];
			if (!previewWindow.IsUnbound())
			{
				ActiveDisplay.UnbindWindow();
				ActiveWindow = null;
			}
			ActivateDocumentPreview(previewDocumentOrSurrogate, previewWindow);
		}
	}

	private void PreviewDoc_PreviewModuleChanged(object sender, EventArgs e)
	{
		if (sender is IPreviewableDocument previewableDocument)
		{
			BugSubmitter.Assert(PreviewWindows.ContainsKey(previewableDocument), "Untracked previewable document");
			IPreviewWindow previewWindow = PreviewWindows[previewableDocument];
			if (!HasValidPreviewModule(previewWindow.GetPreviewModule()) || !(previewWindow.GetPreviewModule() == previewableDocument.PreviewModule))
			{
				ShutdownDocumentPreviewing(previewableDocument, previewWindow);
				previewableDocument.As<EntityPreviewingSequencer>()?.HandlePreviewModuleChanged(previewableDocument);
				KnobChangeWindow = previewWindow;
				KnobChangeDocument = previewableDocument;
				AssetPreviewer.KnobChangesComplete += AssetPreviewer_KnobChangesComplete;
				InitializeDocumentPreviewing(previewableDocument, previewWindow);
			}
		}
	}

	private void AssetPreviewer_KnobChangesComplete(object sender, EventArgs e)
	{
		AssetPreviewer.KnobChangesComplete -= AssetPreviewer_KnobChangesComplete;
		BugSubmitter.Assert(KnobChangeWindow != null, "KnobChangeWindow is not set!");
		BugSubmitter.Assert(KnobChangeDocument != null, "ActiveDocument is not set!");
		PreviewerControlHost.PreviewerControl.BeginInvoke((Action)delegate
		{
			ActivateDocumentPreview(KnobChangeDocument, KnobChangeWindow);
		});
	}

	private void DocumentRegistry_ActiveDocumentChanging(object sender, EventArgs e)
	{
		PaintTimingLog.Write("Previewer: keep active display bound during document change");
		AnimationRecorderService?.SetActiveEntity(InstanceType.IT_INVALID, string.Empty);
	}

	private void DocumentRegistry_ActiveDocumentChanged(object sender, EventArgs e)
	{
		var sw = Stopwatch.StartNew();
		IPreviewableDocument previewDocumentOrSurrogate = GetPreviewDocumentOrSurrogate(DocumentRegistry.ActiveDocument);
		long tGet = sw.ElapsedMilliseconds;
		if (previewDocumentOrSurrogate != null)
		{
			IPreviewWindow wnd = PreviewWindows[previewDocumentOrSurrogate];
			ActivateDocumentPreview(previewDocumentOrSurrogate, wnd);
			long tActivate = sw.ElapsedMilliseconds;
			AnimationRecorderService?.SetActiveEntity(previewDocumentOrSurrogate.EntityAdapter.InstanceType, previewDocumentOrSurrogate.EntityAdapter.Name);
			sw.Stop();
			PaintTimingLog.Write("Previewer: get={0}ms, activate={1}ms, setAnim={2}ms", tGet, tActivate - tGet, sw.ElapsedMilliseconds - tActivate);
		}
		else
		{
			PaintTimingLog.Write("Previewer: unbind active display for null preview begin");
			ActiveDisplay.UnbindWindow();
			ActiveWindow = null;
			PaintTimingLog.Write("Previewer: unbind active display for null preview end");
			sw.Stop();
			PaintTimingLog.Write("Previewer: get={0}ms (null)", tGet);
		}
	}

	private bool InitializeDocumentPreviewing(IPreviewableDocument previewDoc, IPreviewWindow previewWnd)
	{
		previewDoc.PreviewModuleChanged += PreviewDoc_PreviewModuleChanged;
		previewDoc.UriChanged += PreviewDoc_UriChanged;
		if (!HasValidPreviewModule(previewDoc.PreviewModule))
		{
			return false;
		}
		previewWnd.SetPreviewModule(previewDoc.PreviewModule);
		previewWnd.OpenAsset(previewDoc.EntityAdapter.InstanceEntity, 0, delayBind: false);
		IEnumerable<IPreviewerSlotInfo> slotsInfo = previewWnd.GetSlotsInfo();
		if (!slotsInfo.First().XLPClass.Contains("UITexture") && !slotsInfo.Any((IPreviewerSlotInfo previewerSlotInfo) => previewerSlotInfo.XLPClass.Contains("GameLighting") | previewerSlotInfo.XLPClass.Contains("LeaderLighting")))
		{
			Outputs.WriteLine(OutputMessageType.Warning, "No lighting to preview entity with!");
			return false;
		}
		using (new Firaxis.Utility.WaitCursor())
		{
			IAssetInstance assetInstance = previewDoc.EntityAdapter.InstanceEntity.As<IAssetInstance>();
			if (assetInstance != null)
			{
				PreviewerEntityLoadingService.InstanceSet.LoadDependentBehaviors(assetInstance);
			}
			foreach (IPreviewerSlotInfo slotInfo in slotsInfo)
			{
				if (slotInfo.SlotID == 0 || slotInfo.AllowsNull)
				{
					continue;
				}
				IXLPClass iXLPClass = CivTechService.PrimaryProject.Config.XLPClasses.Items.FirstOrDefault((IXLPClass xc) => xc.Name == slotInfo.XLPClass);
				if (iXLPClass != null)
				{
					string displayName = ReflectionHelper.GetDisplayName(iXLPClass.InstanceType);
					IInstanceEntity instanceEntity = PreviewerEntityLoadingService.LoadEntity(slotInfo.DefaultAsset, iXLPClass.InstanceType);
					if (instanceEntity != null)
					{
						Outputs.WriteLine(OutputMessageType.Info, "Loaded slot {0} entity type \"{1}\" named \"{2}\"", slotInfo.SlotID, displayName, slotInfo.DefaultAsset);
						previewWnd.OpenAsset(instanceEntity, slotInfo.SlotID, delayBind: false);
						Outputs.WriteLine(OutputMessageType.Info, "Opened slot {0} entity type \"{1}\" named \"{2}\"", slotInfo.SlotID, displayName, slotInfo.DefaultAsset);
					}
					else
					{
						Outputs.WriteLine(OutputMessageType.Warning, "Failed to loaded slot {0} entity type \"{1}\" named \"{2}\"", slotInfo.SlotID, displayName, slotInfo.DefaultAsset);
					}
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Warning, "Failed to find XLP class \"{0}\" for slot {1} entity named \"{2}\"", slotInfo.XLPClass, slotInfo.SlotID, slotInfo.DefaultAsset);
				}
			}
			previewWnd.BindAssetsToWindow();
		}
		return true;
	}

	private void ShutdownDocumentPreviewing(IPreviewableDocument previewDoc, IPreviewWindow previewWnd)
	{
		previewDoc.PreviewModuleChanged -= PreviewDoc_PreviewModuleChanged;
		previewDoc.UriChanged -= PreviewDoc_UriChanged;
		PreviewerWidgetService.ClearIfActive(previewWnd, previewDoc.As<IEntityDocument>());
		AnimationKnobService.ClearIfActive(previewDoc.EntityAdapter.InstanceEntity);
		KnobService.ClearIfActive(previewDoc);
		if (ActiveWindow == previewWnd)
		{
			ActiveDisplay.UnbindWindow();
			ActiveWindow = null;
		}
	}

	private bool HasValidPreviewModule(string previewModuleName)
	{
		if (string.IsNullOrEmpty(previewModuleName))
		{
			return false;
		}
		if (previewModuleName == "None")
		{
			return false;
		}
		return true;
	}

	private bool CanBePreviewSurrogate(IPreviewableDocument surrogate, IDocument test)
	{
		IWorkspaceDependencyRegistry workspaceDependencyRegistry = CivTechService.GetWorkspaceDependencyRegistry(test.Uri);
		return DependsOnRecursive(workspaceDependencyRegistry, surrogate.Uri, test.Uri);
	}

	private bool DependsOnRecursive(IWorkspaceDependencyRegistry registry, Uri surrogate, Uri test)
	{
		if (registry.DependsOn(surrogate, test))
		{
			return true;
		}
		foreach (Uri dependency in registry.GetDependencies(surrogate))
		{
			if (DependsOnRecursive(registry, dependency, test))
			{
				return true;
			}
		}
		return false;
	}

	private IPreviewableDocument FindFirstPreviewableDependent(IDocument activeDoc)
	{
		if (activeDoc == null)
		{
			return null;
		}
		return DocumentRegistry.Documents.OfType<IPreviewableDocument>().FirstOrDefault((IPreviewableDocument doc) => CanBePreviewSurrogate(doc, activeDoc));
	}

	private IPreviewableDocument GetPreviewDocumentOrSurrogate(IDocument doc)
	{
		return doc.As<IPreviewableDocument>() ?? FindFirstPreviewableDependent(doc);
	}

	private void ActivateDocumentPreview(IPreviewableDocument doc, IPreviewWindow wnd)
	{
		BugSubmitter.Assert(KnobChangeWindow == null || KnobChangeWindow == wnd, "Colliding document preview activations!");
		BugSubmitter.Assert(KnobChangeDocument == null || KnobChangeDocument == doc, "Colliding document preview activations!");
		KnobChangeWindow = null;
		KnobChangeDocument = null;
		if (HasValidPreviewModule(wnd.GetPreviewModule()))
		{
			ActiveWindow = wnd;
			var sw = Stopwatch.StartNew();
			PaintTimingLog.Write("ActivateDocPreview: BindWindow begin");
			ActiveDisplay.BindWindow(wnd);
			long t1 = sw.ElapsedMilliseconds;
			PaintTimingLog.Write("ActivateDocPreview: BindWindow end");
			ActiveDisplay.MakeActiveDisplay();
			long t2 = sw.ElapsedMilliseconds;
			KnobService.SetActivePreviewModule(wnd.GetPreviewModule());
			long t3 = sw.ElapsedMilliseconds;
			KnobService.SetActiveEntity(doc);
			long t4 = sw.ElapsedMilliseconds;
			AnimationKnobService.SetActiveEntity(doc.EntityAdapter.InstanceEntity);
			long t5 = sw.ElapsedMilliseconds;
			PreviewerWidgetService.SetActiveEntity(wnd, DocumentRegistry.ActiveDocument.As<IEntityDocument>());
			sw.Stop();
			PaintTimingLog.Write("ActivateDocPreview: bind={0}ms, makeActive={1}ms, setMod={2}ms, setEnt={3}ms, setAnim={4}ms, setWidget={5}ms",
				t1, t2 - t1, t3 - t2, t4 - t3, t5 - t4, sw.ElapsedMilliseconds - t5);
		}
		else
		{
			KnobService.SetActivePreviewModule(null);
			KnobService.SetActiveEntity(null);
			AnimationKnobService.SetActiveEntity(null);
			PreviewerWidgetService.SetActiveEntity(null, null);
		}
	}

	public void StartProjectChange(Action<string> statusMessagePrinter)
	{
		BugSubmitter.Assert(ProjectChangeCache.Count == 0, "Dirty project change cache");
		statusMessagePrinter?.Invoke("Cleaning up entities being previewed...");
		ProjectChangeCache.AddRange(PreviewWindows.Keys);
		PreviewWindows.ForEach(delegate(KeyValuePair<IPreviewableDocument, IPreviewWindow> dwp)
		{
			ShutdownDocumentPreviewing(dwp.Key, dwp.Value);
			AssetPreviewer.CloseWindow(dwp.Value);
		});
		PreviewWindows.Clear();
	}

	public void FinishProjectChange(Action<string> statusMessagePrinter)
	{
		statusMessagePrinter?.Invoke("Restoring entities being previewed...");
		int count = ProjectChangeCache.Count;
		ProjectChangeCache.ForEach(delegate(IPreviewableDocument doc)
		{
			IPreviewWindow previewWindow = (PreviewWindows[doc] = AssetPreviewer.OpenWindow(PreviewerControlHost.PreviewerControl.Handle, PreviewerEntityLoadingService.InstanceSet));
			IPreviewWindow previewWnd = (doc.PreviewWindow = previewWindow);
			InitializeDocumentPreviewing(doc, previewWnd);
		});
		ProjectChangeCache.Clear();
		BugSubmitter.Assert(PreviewWindows.Count == count, "Lost a preview document during project change!");
	}
}
