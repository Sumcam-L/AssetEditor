using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Firaxis.AssetBrowser.Commands;
using Firaxis.AssetBrowser.Views;
using Firaxis.AssetEditing;
using Firaxis.AssetPreviewing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.Properties;
using Firaxis.MVVMBase.Helpers;
using Firaxis.Threading;
using Firaxis.Utility;
using Firaxis.VersionControl;
using Microsoft.Win32;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Controls;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;
using Sce.Atf.WinForms.Applications;

namespace AssetEditor;

internal static class Program
{
	private const int GWL_EXSTYLE = -20;

	private const int WS_EX_TOOLWINDOW = 0x00000080;

	private const int WS_EX_APPWINDOW = 0x00040000;

	private const uint SWP_NOSIZE = 0x0001;

	private const uint SWP_NOMOVE = 0x0002;

	private const uint SWP_NOZORDER = 0x0004;

	private const uint SWP_NOACTIVATE = 0x0010;

	private const uint SWP_FRAMECHANGED = 0x0020;

	[ComImport]
	[Guid("56FDF344-FD6D-11D0-958A-006097C9A090")]
	[ClassInterface(ClassInterfaceType.None)]
	private class TaskbarList
	{
	}

	[ComImport]
	[Guid("56FDF342-FD6D-11D0-958A-006097C9A090")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface ITaskbarList
	{
		void HrInit();
		void AddTab(IntPtr hwnd);
		void DeleteTab(IntPtr hwnd);
		void ActivateTab(IntPtr hwnd);
		void SetActiveAlt(IntPtr hwnd);
	}

	private static string kOfflineEnvironment = "--offline=";

	private static string kStaticDependencies = "--static-deps=";

	private static string kNoAssetPreviewer = "--no-asset-previewer";

	[DllImport("user32.dll")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

	private static void RefreshMainWindowTaskbarRegistration(Form form)
	{
		if (form == null || form.IsDisposed || form.Disposing)
		{
			return;
		}
		IntPtr handle = form.Handle;
		int exStyle = GetWindowLong(handle, GWL_EXSTYLE);
		int taskbarStyle = (exStyle | WS_EX_APPWINDOW) & ~WS_EX_TOOLWINDOW;
		if (taskbarStyle != exStyle)
		{
			SetWindowLong(handle, GWL_EXSTYLE, taskbarStyle);
			SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
		}
		object taskbarList = null;
		try
		{
			taskbarList = new TaskbarList();
			ITaskbarList taskbar = (ITaskbarList)taskbarList;
			taskbar.HrInit();
			taskbar.AddTab(handle);
		}
		catch (COMException)
		{
			// Shell integration must not prevent the editor from starting.
		}
		finally
		{
			if (taskbarList != null && Marshal.IsComObject(taskbarList))
			{
				Marshal.FinalReleaseComObject(taskbarList);
			}
		}
	}

	private static bool PauseForDebugAttach()
	{
		RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Firaxis\\Tools\\AssetCloudCiv6", writable: false);
		if (registryKey != null && (int)registryKey.GetValue("DebugCloud", 0) != 0)
		{
			return true;
		}
		return false;
	}

	private static bool HasCommandLineFlag(string[] args, string flag)
	{
		return args.Contains(flag, StringComparer.CurrentCultureIgnoreCase);
	}

	private static bool HasCommandLineOption(string[] args, string flag)
	{
		return args.Any((string arg) => arg.StartsWith(flag, StringComparison.CurrentCultureIgnoreCase));
	}

	private static string GetCommandLineOption(string[] args, string flag)
	{
		return args.FirstOrDefault((string arg) => arg.StartsWith(flag, StringComparison.CurrentCultureIgnoreCase)).Substring(flag.Length).Trim('"');
	}

	private static string DetermineBaseWorkspaceRoot(string pantryPath)
	{
		string text = DetermineWorkspaceRootImpl("/Civ6", pantryPath);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return Path.GetDirectoryName(pantryPath);
	}

	private static string DetermineWorkspaceRootImpl(string markerPath, string pantryPath)
	{
		string pathRoot = Path.GetPathRoot(pantryPath);
		string directoryName = Path.GetDirectoryName(pantryPath);
		while (PathCompareHelper.StartsWith(directoryName, pathRoot, bIgnoreCase: true) && !PathCompareHelper.EndsWith(directoryName, markerPath, bIgnoreCase: true))
		{
			directoryName = Path.GetDirectoryName(directoryName);
		}
		if (PathCompareHelper.EndsWith(directoryName, markerPath, bIgnoreCase: true))
		{
			return Path.GetDirectoryName(directoryName);
		}
		return string.Empty;
	}

	private static void RegisterEnvironmentParts(string[] args, List<Type> parts)
	{
		if (!Firaxis.CivTech.Properties.Resources.ModTools)
		{
			parts.AddRange(GetEnvironmentTypes());
			if (HasCommandLineOption(args, kStaticDependencies))
			{
				parts.Add(typeof(StaticDependencyRegistryService));
			}
			else
			{
				parts.Add(typeof(WorkspaceDependencyRegistryService));
			}
			if (HasCommandLineOption(args, kOfflineEnvironment))
			{
				parts.Add(typeof(LocalVersionControlSelectionService));
				parts.Add(typeof(LocalProjectSelectionService));
				parts.Add(typeof(LocalProjectConfigService));
			}
			else
			{
				parts.Add(typeof(AssetCloudSettingService));
				parts.Add(typeof(ProjectConfigService));
				parts.Add(typeof(CommonConfigsRootProvider));
				parts.Add(typeof(VersionControlSelectionService));
				parts.Add(typeof(ProjectSelectionService));
			}
		}
		else
		{
			parts.AddRange(new Type[9]
			{
				typeof(TunerCommands),
				typeof(TunerService),
				typeof(CookableRegistry),
				typeof(HotloadTargetsDockWindow),
				typeof(LocalCookService),
				typeof(CookCommands),
				typeof(WorkspaceWatcherService),
				typeof(WorkspaceChangeMediator),
				typeof(TemporaryArtProjectService)
			});
			parts.AddRange(new Type[4]
			{
				typeof(ModWorkspaceDependencyRegistryService),
				typeof(GameArtSpecificationEditor),
				typeof(GameArtSpecificationSchemaLoader),
				typeof(ModVersionControlSelectionService)
			});
			parts.Add(typeof(ProjectSelectionCommands));
			parts.Add(typeof(ProjectChangeMediator));
		}
	}

	private static bool ComposeEnvironmentParts(string[] args, CompositionContainer container)
	{
		if (Firaxis.CivTech.Properties.Resources.ModTools)
		{
			if (args.Length < 3 || (args.Length - 1) % 2 != 0)
			{
				System.Windows.Forms.MessageBox.Show("Incorrect number of arguments!\n\nUsage: AssetEditor.exe PathToGameInstall ModName ModPantryPath [DependencyName DependencyPantryPath...DependencyName DependencyPantryPath]");
				System.Windows.Forms.Application.Exit();
				return false;
			}
			string text = args[0];
			if (!Directory.Exists(text))
			{
				System.Windows.Forms.MessageBox.Show("Invalid game path \"" + text + "\".\n\nUsage: AssetEditor.exe PathToGameInstall ModName ModPantryPath [DependencyName DependencyPantryPath...DependencyName DependencyPantryPath]");
				System.Windows.Forms.Application.Exit();
				return false;
			}
			IList<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
			for (int i = 1; i < args.Length - 1; i += 2)
			{
				string text2 = args[i];
				string text3 = args[i + 1];
				if (Directory.Exists(text3))
				{
					list.Add(new KeyValuePair<string, string>(text2, text3));
					continue;
				}
				System.Windows.Forms.MessageBox.Show("Pantry path \"" + text3 + "\" for project \"" + text2 + "\" not found. Project ignored.");
			}
			if (list.Count < 2)
			{
				System.Windows.Forms.MessageBox.Show("Invalid base game assets path \"" + text + "\".\n\nUsage: AssetEditor.exe PathToGameInstall ModName Mod PantryPath [DependencyName DependencyPantryPath...DependencyName DependencyPantryPath]");
				System.Windows.Forms.Application.Exit();
				return false;
			}
			string directoryName = Path.GetDirectoryName(list[0].Value);
			string baseRoot = DetermineBaseWorkspaceRoot(list[1].Value);
			container.ComposeExportedValue((IAssetCloudSettingService)new ModAssetCloudSettingService(text));
			container.ComposeExportedValue((IProjectConfigService)new BasicProjectConfigService(Path.Combine(text, "Civ6.cfg")));
			container.ComposeExportedValue((IModWorkspaceRootProvider)new ModWorkspaceRootProvider(directoryName, baseRoot));
			container.ComposeExportedValue((IProjectSelectionService)new ModProjectSelectionService(text, list));
		}
		else if (HasCommandLineOption(args, kOfflineEnvironment))
		{
			string commandLineOption = GetCommandLineOption(args, kOfflineEnvironment);
			if (string.IsNullOrEmpty(commandLineOption))
			{
				System.Windows.Forms.MessageBox.Show("To few parameters for \"offline\" mode.\n\n Usage: app.exe --offline=\"path to workspace root\"");
				System.Windows.Forms.Application.Exit();
				return false;
			}
			if (!Directory.Exists(commandLineOption))
			{
				System.Windows.Forms.MessageBox.Show("Workspace root \"{0}\" not found.", commandLineOption);
				System.Windows.Forms.Application.Exit();
				return false;
			}
			container.ComposeExportedValue((IAssetCloudSettingService)new AssetCloudSettingService(useLocalToolHost: false, useLocalConfig: false, Path.Combine(commandLineOption, "Civ6", "pantry", "Civ6.cfg")));
			container.ComposeExportedValue((IProjectRootProvider)new LocalWorkspaceRootProvider(commandLineOption));
			container.ComposeExportedValue((ICommonConfigsRootProvider)new LocalCommonConfigsRootProvider(commandLineOption));
		}
		if (HasCommandLineOption(args, kStaticDependencies))
		{
			string commandLineOption2 = GetCommandLineOption(args, kStaticDependencies);
			if (string.IsNullOrEmpty(commandLineOption2))
			{
				System.Windows.Forms.MessageBox.Show("To few parameters for \"static-deps\" mode.\n\n Usage: app.exe --static-deps=\"path to deps.json folder\"");
				System.Windows.Forms.Application.Exit();
				return false;
			}
			if (!Directory.Exists(commandLineOption2))
			{
				System.Windows.Forms.MessageBox.Show("Depedency root \"{0}\" not found.", commandLineOption2);
				System.Windows.Forms.Application.Exit();
				return false;
			}
			container.ComposeExportedValue((IDependencyRootProvider)new LocalDependencyFolderProvider(commandLineOption2));
		}
		return true;
	}

	[STAThread]
	private static void Main(string[] args)
	{
		try
		{
			MainImpl(args);
		}
		catch (System.Exception ex)
		{
			var sb = new System.Text.StringBuilder();
			System.Exception cur = ex;
			while (cur != null)
			{
				sb.AppendLine($"--- Inner --- Type: {cur.GetType().FullName}");
				sb.AppendLine($"Message: {cur.Message}");
				sb.AppendLine($"StackTrace:");
				sb.AppendLine(cur.StackTrace);
				cur = cur.InnerException;
			}
			File.WriteAllText(Path.Combine(Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath) ?? ".", "crash.log"),
				$"Time: {DateTime.Now}\r\n{sb.ToString()}");
			throw;
		}
	}

	private static void MainImpl(string[] args)
	{
		PaintTimingLog.Clear();
		if (PauseForDebugAttach())
		{
			System.Windows.Forms.MessageBox.Show("Attach Debugger Now");
		}
		System.Windows.Forms.Application.EnableVisualStyles();
		System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
		System.Windows.Forms.Application.DoEvents();
		if (System.Windows.Application.Current == null)
		{
			new App();
		}
		Firaxis.MVVMBase.Helpers.ApplicationHelper.ImportResourceDictionary(typeof(AssetBrowserView), "Shared.xaml");
		List<Type> list = new List<Type>();
		list.AddRange(GetCoreTypes());
		list.AddRange(GetToolAppTypes());
		list.AddRange(GetAssetEditorTypes());
		if (!HasCommandLineFlag(args, kNoAssetPreviewer))
		{
			list.AddRange(GetAssetPreviewerTypes());
		}
		list.AddRange(GetDebuggingTypes());
		RegisterEnvironmentParts(args, list);
		list.Add(typeof(SettingsService));
		EmbeddedCollectionEditor.AddImage = ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.AddItemIcon);
		EmbeddedCollectionEditor.RemoveImage = ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.RemoveItemIcon);
		EmbeddedCollectionEditor.UpImage = ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.ArrowUpIcon);
		EmbeddedCollectionEditor.DownImage = ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.ArrowDownIcon);
		using CompositionContainer compositionContainer = new CompositionContainer(new TypeCatalog(list));
		Thread.CurrentThread.CurrentUICulture = CultureInfo.CurrentCulture;
		Localizer.SetStringLocalizer(new EmbeddedResourceStringLocalizer());
		Firaxis.ATF.SplashScreen splash = new Firaxis.ATF.SplashScreen("Asset Editor");
		splash.ShowInTaskbar = true;
		splash.Message = "Constructing components...";
		splash.CaptionImage = ResourceUtil.GetIcon(Resources.AssetEditorIcon);
		splash.ShowOutputWindow();
		splash.Show();
		_ = compositionContainer.GetExport<IMessageBoxService>().Value;
		_ = compositionContainer.GetExport<MessageBoxes>().Value;
		compositionContainer.GetExport<LogOutputWriter>().Value.AddFrameNumber = true;
		if (!ComposeEnvironmentParts(args, compositionContainer))
		{
			return;
		}
		ICrashSubmissionService value = compositionContainer.GetExport<ICrashSubmissionService>().Value;
		_ = compositionContainer.GetExport<CrashHandlerService>().Value;
		DomNodeType.BaseOfAllTypes.AddAdapterCreator(new AdapterCreator<CustomTypeDescriptorNodeAdapter>());
		AssetEditorForm assetEditorForm = new AssetEditorForm(new ToolStripContainer
		{
			Dock = DockStyle.Fill
		})
		{
			Text = "Asset Editor".Localize(),
			Icon = ResourceUtil.GetIcon(Resources.AssetEditorIcon),
			ShowInTaskbar = true
		};
		CompositionBatch batch = new CompositionBatch();
		batch.AddPart(assetEditorForm);
		try
		{
			compositionContainer.Compose(batch);
		}
		catch (ChangeRejectedException ex)
		{
			BugSubmitter.Assert(condition: false, "We get exports for some things very early on, before we compose the batch. this means some imports must support recomposition.\n\n" + ex.Message);
			return;
		}
		catch (CompositionException ex2)
		{
			if (ex2.RootCauses.Count > 0)
			{
				if (ex2.RootCauses[0].InnerException is ProjectConfigException ex3)
				{
					splash?.Close();
					MessageBoxes.Show(ex3.Message + "\n\nApplication will exit.", "Failed to initialize ProjectConfigService", Sce.Atf.Applications.MessageBoxButton.OK, Sce.Atf.Applications.MessageBoxImage.Error);
					return;
				}
				if (ex2.RootCauses[0].InnerException is ResultCodeException ex4)
				{
					splash?.Close();
					MessageBoxes.Show(ex4.Message + "\n\nApplication will exit.", "Failed to initialize VersionControlService", Sce.Atf.Applications.MessageBoxButton.OK, Sce.Atf.Applications.MessageBoxImage.Error);
					return;
				}
			}
			throw ex2;
		}
		_ = compositionContainer.GetExport<ISingleInstanceService>().Value;
		_ = compositionContainer.GetExport<ColumnarOutputService>().Value;
		_ = compositionContainer.GetExport<Outputs>().Value;
		ISplashScreenOutputWriter value2 = compositionContainer.GetExport<ISplashScreenOutputWriter>().Value;
		splash.HookOutputWriter(value2);
		splash.Message = "Initializing asset cloud settings...";
		Lazy<IAssetCloudSettingService> export = compositionContainer.GetExport<IAssetCloudSettingService>();
		_ = export.Value;
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>(new object[1] { export.Value.AssetCloudSettings.ShowAssertions ? AssertionConfiguration.eDialog : AssertionConfiguration.eOff });
		Outputs.WriteLine(OutputMessageType.Info, "Using Firaxis.CivTech.Impl from {0}", civTechContext.CivTechPath);
		Lazy<IProjectSelectionService> export2 = compositionContainer.GetExport<IProjectSelectionService>();
		Lazy<IVersionControlSelectionService> export3 = compositionContainer.GetExport<IVersionControlSelectionService>();
		Lazy<IProjectConfigService> export4 = compositionContainer.GetExport<IProjectConfigService>();
		splash.Message = "Initializing project settings...";
		try
		{
			_ = export3.Value;
			IProjectSelectionService value3 = export2.Value;
			_ = export4.Value;
			string activeProject = value3.ActiveProject;
			if (!Firaxis.CivTech.Properties.Resources.ModTools)
			{
				_ = compositionContainer.GetExport<ProjectChangeMediator>().Value;
				if (!value3.Projects.ContainsProject(activeProject))
				{
					MessageBoxes.Show("Project " + activeProject + " not found in project map. Defaulting to Civ 6 base game.", "Error configuring project", Sce.Atf.Applications.MessageBoxButton.OK, Sce.Atf.Applications.MessageBoxImage.Error);
					value3.ActiveProject = "Civ6";
				}
			}
			else if (!value3.Projects.ContainsProject(activeProject))
			{
				if (!value3.Projects.ProjectInfos.Any())
				{
					splash?.Close();
					MessageBoxes.Show("No asset mod projects, create one in launcher first", "Error configuring project", Sce.Atf.Applications.MessageBoxButton.OK, Sce.Atf.Applications.MessageBoxImage.Error);
					return;
				}
				string name = value3.Projects.ProjectInfos.First().Name;
				MessageBoxes.Show("Project " + activeProject + " not found in project map. Defaulting to " + name + " asset mod.", "Error configuring project", Sce.Atf.Applications.MessageBoxButton.OK, Sce.Atf.Applications.MessageBoxImage.Error);
				value3.ActiveProject = name;
			}
		}
		catch (CompositionException ex5)
		{
			if (ex5.RootCauses.Count > 0)
			{
				ProjectConfigException ex6 = ex5.RootCauses[0].InnerException as ProjectConfigException;
				if (ex6 != null)
				{
					splash?.Close();
					MessageBoxes.Show(ex6.Message + "\n\nApplication will exit.", "Failed to initialize ProjectConfigService", Sce.Atf.Applications.MessageBoxButton.OK, Sce.Atf.Applications.MessageBoxImage.Error);
					return;
				}
				if (ex5.RootCauses[0].InnerException is ResultCodeException)
				{
					splash?.Close();
					MessageBoxes.Show(ex6.Message + "\n\nApplication will exit.", "Failed to initialize VersionControlService", Sce.Atf.Applications.MessageBoxButton.OK, Sce.Atf.Applications.MessageBoxImage.Error);
					return;
				}
			}
			throw ex5;
		}
		Lazy<IProjectMapService> export5 = compositionContainer.GetExport<IProjectMapService>();
		Lazy<IWorkspaceDependencyRegistryService> export6 = compositionContainer.GetExport<IWorkspaceDependencyRegistryService>();
		try
		{
			splash.Message = "Setting up project environment...";
			_ = export5.Value;
			value.EnableBugHelper(export5.Value.PrimaryProject.Paths.GameDirectory);
			splash.Message = "Updating dependency information...";
			_ = export6.Value;
		}
		catch (CompositionException ex7)
		{
			if (ex7.RootCauses.Count > 0)
			{
				if (ex7.RootCauses[0].InnerException is FileNotFoundException ex8)
				{
					MessageBoxes.Show(ex8.Message + "\n\n" + ex8.FileName + "\n\nApplication will exit.", "Failed to initialize CivTechService", Sce.Atf.Applications.MessageBoxButton.OK, Sce.Atf.Applications.MessageBoxImage.Error);
					return;
				}
				if (ex7.RootCauses[0].InnerException is ResultCodeException ex9)
				{
					MessageBoxes.Show(ex9.Message + "\n\nApplication will exit.", "Failed to initialize VersionControlService", Sce.Atf.Applications.MessageBoxButton.OK, Sce.Atf.Applications.MessageBoxImage.Error);
					return;
				}
				throw ex7.RootCauses[0].InnerException;
			}
			throw ex7;
		}
		splash.Message = "Initializing all components...";
		ICivTechService value4 = compositionContainer.GetExport<ICivTechService>().Value;
		string toolHostDllPath = value4.ToolHostLoader.ToolHostDllPath;
		if (!File.Exists(toolHostDllPath))
		{
			if (export.Value.AssetCloudSettings.UseLocalToolHost)
			{
				System.Windows.Forms.MessageBox.Show("Could not find toolhost DLL at path \"" + toolHostDllPath + "\". AssetEditor will now close. You may need to rebuild the tool host DLL in the game solution.", "Fatal error starting tool", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else
			{
				System.Windows.Forms.MessageBox.Show("Could not find toolhost DLL at path \"" + toolHostDllPath + "\". AssetEditor will now close. You may need to reinstall the tools to replace the missing release DLL.", "Fatal error starting tool", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			return;
		}
		_ = compositionContainer.GetExport<AutoDocumentService>().Value;
		compositionContainer.InitializeAll();
		splash.Message = "Initialization complete";
		civTechContext.CivTechLogger.EngineLog += delegate(LogEventType evtType, string source, string text)
		{
			CategorizedOutputs.WriteLine(source, CategorizedOutputs.ConvertEventType(evtType), text);
		};
		if (File.Exists(value4.PrimaryProject.ActiveConfigPath))
		{
			value.AddAttachment(new Uri(value4.PrimaryProject.ActiveConfigPath));
		}
		Lazy<ISettingsPathsProvider> export7 = compositionContainer.GetExport<ISettingsPathsProvider>();
		if (export7 != null && export7.Value != null && !string.IsNullOrWhiteSpace(export7.Value.SettingsPath))
		{
			value.AddAttachment(new Uri(export7.Value.SettingsPath));
		}
		if (export.Value.AssetCloudSettings.EnablePreviewerTracing && File.Exists(export.Value.AssetCloudSettings.PreviewerTraceLocation + "\\Previewer.trace"))
		{
			value.AddAttachment(new Uri(export.Value.AssetCloudSettings.PreviewerTraceLocation + "\\Previewer.trace"));
		}
		assetEditorForm.Shown += delegate
		{
			if (assetEditorForm.WindowState == FormWindowState.Minimized)
			{
				assetEditorForm.WindowState = FormWindowState.Normal;
			}
			splash.Close();
			assetEditorForm.BeginInvoke((Action)delegate
			{
				if (assetEditorForm.IsDisposed || assetEditorForm.Disposing)
				{
					return;
				}
				RefreshMainWindowTaskbarRegistration(assetEditorForm);
			});
			StringBuilder stringBuilder = new StringBuilder();
			ReaderWriterStatistics.DumpStatistics(stringBuilder);
			Outputs.Write(OutputMessageType.Info, OutputMessageVerbosity.ExtremelyVerbose, stringBuilder.ToString());
		};
		System.Windows.Forms.Application.Run(assetEditorForm);
	}

	public static IEnumerable<Type> GetCoreTypes()
	{
		return new List<Type>
		{
			typeof(LogOutputWriter),
			typeof(DebugOutputWriter),
			typeof(OutputWriterCategorizedProxy),
			typeof(BugSubmitter),
			typeof(CrashHandlerService),
			typeof(NativeCrashHandlerService),
			typeof(CrashSubmissionService),
			typeof(CrashSubmissionAttachmentService),
			typeof(ToolHostUsage),
			typeof(ToolHostLoaderService),
			typeof(MessageBoxes),
			typeof(MessageBoxService),
			typeof(Outputs),
			typeof(CategorizedOutputs)
		};
	}

	public static IEnumerable<Type> GetToolAppTypes()
	{
		return new List<Type>
		{
			typeof(ProjectMapService),
			typeof(CivTechService),
			typeof(ControlHostService),
			typeof(ContextRegistry),
			typeof(StandardFileExitCommand),
			typeof(DefaultTabCommands),
			typeof(VersionService),
			typeof(TabbedControlSelector),
			typeof(SourceControlNotifier),
			typeof(DocumentRegistry),
			typeof(AutoDocumentService),
			typeof(RecentDocumentCommands),
			typeof(WindowLayoutService),
			typeof(WindowLayoutServiceCommands),
			typeof(DialogHostService),
			typeof(SplashScreenService),
			typeof(StandardEditCommands),
			typeof(StandardEditHistoryCommands),
			typeof(HistoryLister),
			typeof(SplashScreenOutputWriter),
			typeof(ColumnarOutputService),
			typeof(SkinService),
			typeof(SkinServiceCommands),
			typeof(SkinSchemaLoader),
			typeof(SkinServiceEditor),
			typeof(ThemeService),
			typeof(ThemeServiceCommands),
			typeof(WpfSkinService),
			typeof(PythonService),
			typeof(ScriptConsole),
			typeof(AtfScriptVariables),
			typeof(ScriptCommands),
			typeof(DocumentReloadService),
			typeof(SingleInstanceService),
			typeof(AudioLoader)
		};
	}

	public static IEnumerable<Type> GetAssetPreviewerTypes()
	{
		return new List<Type>
		{
			typeof(AnimationRecorderService),
			typeof(PreviewerCaptureService),
			typeof(AnimationRecorderDockWindow),
			typeof(AnimationKnobService2),
			typeof(PreviewerDockWindow2),
			typeof(PreviewerKnobService),
			typeof(PreviewerDocumentService),
			typeof(TimelinePlaybackService),
			typeof(PreviewerWidgetService),
			typeof(PreviewerCacheDockWindow),
			typeof(PreviewerCacheService),
			typeof(PreviewSetCommands),
			typeof(PreviewerEntityLoadingService),
			typeof(EntitySchemaLoaderPreviewer)
		};
	}

	public static IEnumerable<Type> GetAssetEditorTypes()
	{
		return new List<Type>
		{
			typeof(AssetEditorCommandService),
			typeof(DocumentRegistryMediator),
			typeof(ProjectMainWindowTitleService),
			typeof(PropertyEditingCommands),
			typeof(FileDialogService),
			typeof(FileWatcherService),
			typeof(FileWatchDockWindow),
			typeof(FiraxisATFRegistry),
			typeof(DependencyInfoDockWindow),
			typeof(BatchEntitySourceControlService),
			typeof(BaseSchemaLoader),
			typeof(BaseSchemaLoaderGUI),
			typeof(FieldSchemaLoader),
			typeof(FieldSchemaLoaderGUI),
			typeof(EntityFilteringService),
			typeof(AssetBrowserFileCommands),
			typeof(AssetBrowserFileService),
			typeof(ImportServiceCommands),
			typeof(EntityDocumentImportWatcherService),
			typeof(FireFXScriptCommands),
			typeof(FireFXLibraryEditor),
			typeof(FireFXLibrarySchemaLoader),
			typeof(ArtDefCommands),
			typeof(AssetCommands),
			typeof(ArtDefSchemaLoader),
			typeof(ArtDefEditor),
			typeof(ArtDefRegistry),
			typeof(XLPSchemaLoader),
			typeof(XLPSchemaLoaderGUI),
			typeof(XLPEditor),
			typeof(XLPRegistry),
			typeof(EntityCacheService),
			typeof(EntityQueryService),
			typeof(FireFXService),
			typeof(TimelineDockWindow),
			typeof(TimelineTrackCommands),
			typeof(EntitySchemaInitializer),
			typeof(EntitySchemaExtension),
			typeof(EntitySchemaLoader),
			typeof(EntitySchemaLoaderGUI),
			typeof(EntitySchemaAdapterCreator),
			typeof(EntityEditorControlService),
			typeof(Firaxis.AssetEditing.AssetEditor),
			typeof(FireFXEditor),
			typeof(MaterialEditor),
			typeof(TextureEntityEditor),
			typeof(AnimationEditor),
			typeof(GeometryEditor),
			typeof(AnalyticLightEditor),
			typeof(EnvironmentLightEditor),
			typeof(ParticleEffectEditor),
			typeof(LightRigEditor),
			typeof(BehaviorEditor),
			typeof(ShadowDocumentClient),
			typeof(FiraxisATFScriptVariables),
			typeof(FileServiceNameEditor),
			typeof(AutomaticXLPEntryService),
			typeof(AssetEditorConfigurer),
			typeof(AssetEditorHelpAboutCommand),
			typeof(AssetBrowserDockWindow),
			typeof(Firaxis.AssetEditing.ReimportSelectedCommand),
			typeof(ReimportSelectedWithClassDefaultsCommand),
			typeof(OpenInFolderCommand),
			typeof(RenderEntityFromSourceFileCommand)
		};
	}

	private static IEnumerable<Type> GetEnvironmentTypes()
	{
		return new List<Type>
		{
			typeof(ProjectChangeMediatorDialog),
			typeof(ProjectSelectionCommands),
			typeof(TemporaryArtProjectService),
			typeof(WorkspaceWatcherService),
			typeof(WorkspaceChangeMediator),
			typeof(DanglingAssetReportService),
			typeof(DanglingAssetReportCommands),
			typeof(TunerCommands),
			typeof(TunerService),
			typeof(CookableRegistry),
			typeof(HotloadTargetsDockWindow),
			typeof(LocalCookService),
			typeof(CookCommands),
			typeof(VersionControlDocuments)
		};
	}

	private static IEnumerable<Type> GetDebuggingTypes()
	{
		return new List<Type>
		{
			typeof(DomExplorer),
			typeof(PerformanceMonitor)
		};
	}
}
