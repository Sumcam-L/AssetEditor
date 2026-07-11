using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Firaxis.CivTech;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

[Export(typeof(IPartImportsSatisfiedNotification))]
[Export(typeof(CivTechRegistry))]
[Export(typeof(FiraxisATFRegistry))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class FiraxisATFRegistry : CivTechRegistry, IDisposable
{
	private static IAssetPreviewerService s_assetPreviewerService;

	private static IPreviewerCacheService s_previewerCacheService;

	private static IPreviewerWidgetService s_previewerWidgetService;

	private static IDocumentService s_documentService;

	private static ICommandService s_commandService;

	private static IContextRegistry s_contextRegistry;

	private static IEnumerable<IDocumentClient> s_documentClients;

	private static IImportService s_importService;

	private static IDocumentRegistryMediator s_registryMediator;

	private static BatchEntitySourceControlService s_batchEntitySourceControl;

	private static IPreviewerDocumentService s_previewerDocumentService;

	private static IFileDialogService s_fileDialogService;

	private static AssetBrowserFileCommands s_assetBrowserFileCommands;

	private static IAssetBrowserDialogService s_assetBrowserDialogService;

	private static IThemeService s_themeService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IAssetPreviewerService> m_assetPreviewerService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IPreviewerCacheService> m_previewerCacheService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IPreviewerWidgetService> m_previewerWidgetService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<ICommandService> m_commandService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IContextRegistry> m_contextRegistry;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IFileDialogService> m_fileDialogService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IDocumentService> m_documentService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<AssetBrowserFileCommands> m_assetBrowserFileCommands;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IAssetBrowserDialogService> m_assetBrowserDialogService;

	[ImportMany(RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IDocumentClient>[] m_documentClients;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IImportService> m_importService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IDocumentRegistryMediator> m_registryMediator;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<BatchEntitySourceControlService> m_batchEntitySourceControl;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IThemeService> m_themeService;

	[Import(AllowDefault = true, RequiredCreationPolicy = CreationPolicy.Shared)]
	private Lazy<IPreviewerDocumentService> m_previewerDocumentService;

	public static IAssetPreviewerService AssetPreviewerService => s_assetPreviewerService;

	public static IPreviewerCacheService PreviewerCacheService => s_previewerCacheService;

	public static IPreviewerWidgetService PreviewerWidgetService => s_previewerWidgetService;

	public static IDocumentService DocumentService => s_documentService;

	public static ICommandService CommandService => s_commandService;

	public static IContextRegistry ContextRegistry => s_contextRegistry;

	public static IFileDialogService FileDialogService => s_fileDialogService;

	public static AssetBrowserFileCommands AssetBrowserFileCommands => s_assetBrowserFileCommands;

	public static IAssetBrowserDialogService AssetBrowserDialogService => s_assetBrowserDialogService;

	public static IImportService ImportService => s_importService;

	public static IDocumentRegistryMediator RegistryMediator => s_registryMediator;

	public static BatchEntitySourceControlService BatchEntitySourceControl => s_batchEntitySourceControl;

	public static IPreviewerDocumentService PreviewerDocumentService => s_previewerDocumentService;

	public static IEnumerable<IDocumentClient> DocumentClients => s_documentClients;

	public static IThemeService ThemeService => s_themeService;

	static FiraxisATFRegistry()
	{
		s_assetPreviewerService = null;
		s_previewerCacheService = null;
		s_previewerWidgetService = null;
		s_documentService = null;
		s_commandService = null;
		s_contextRegistry = null;
		s_documentClients = null;
		s_importService = null;
		s_registryMediator = null;
		s_batchEntitySourceControl = null;
		s_previewerDocumentService = null;
		s_fileDialogService = null;
		s_assetBrowserFileCommands = null;
		s_assetBrowserDialogService = null;
		s_themeService = null;
	}

	public override void OnImportsSatisfied()
	{
		base.OnImportsSatisfied();
		s_assetPreviewerService = m_assetPreviewerService?.Value;
		s_previewerCacheService = m_previewerCacheService?.Value;
		s_previewerWidgetService = m_previewerWidgetService?.Value;
		s_commandService = m_commandService?.Value;
		s_contextRegistry = m_contextRegistry?.Value;
		s_fileDialogService = m_fileDialogService?.Value;
		s_documentService = m_documentService?.Value;
		s_assetBrowserFileCommands = m_assetBrowserFileCommands?.Value;
		s_assetBrowserDialogService = m_assetBrowserDialogService?.Value;
		s_importService = m_importService?.Value;
		s_registryMediator = m_registryMediator?.Value;
		s_batchEntitySourceControl = m_batchEntitySourceControl?.Value;
		s_documentClients = new List<IDocumentClient>(from se in m_documentClients
			where se.IsValueCreated
			select se.Value);
		s_themeService = m_themeService?.Value;
		s_previewerDocumentService = m_previewerDocumentService?.Value;
	}

	public void Dispose()
	{
		s_assetPreviewerService = null;
		s_previewerCacheService = null;
		s_previewerWidgetService = null;
		s_commandService = null;
		s_contextRegistry = null;
		s_fileDialogService = null;
		s_documentService = null;
		s_assetBrowserFileCommands = null;
		s_assetBrowserDialogService = null;
		s_importService = null;
		s_registryMediator = null;
		s_batchEntitySourceControl = null;
		s_documentClients = null;
		s_themeService = null;
		s_previewerDocumentService = null;
	}
}
