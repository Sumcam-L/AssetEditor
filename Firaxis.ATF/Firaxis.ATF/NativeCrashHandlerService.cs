using System;
using System.ComponentModel.Composition;
using Firaxis.CivTech;
using Firaxis.Utility;

namespace Firaxis.ATF;

[Export(typeof(INativeCrashHandlerService))]
[Export(typeof(NativeCrashHandlerService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class NativeCrashHandlerService : INativeCrashHandlerService, IDisposable
{
	private bool disposedValue;

	private INativeExceptionHandler m_nativeExceptionHandler;

	private IAssetCloudSettingService m_assetCloudSettingService;

	private IToolHostLoaderService m_toolHostLoaderService;

	private ICrashSubmissionService m_crashSubmissionService;

	private ILogFileProvider m_logFileProvider;

	private bool m_automationEnabled;

	public ulong SessionHash
	{
		get
		{
			if (m_nativeExceptionHandler != null)
			{
				return m_nativeExceptionHandler.SessionHash;
			}
			return 0uL;
		}
		set
		{
			if (m_nativeExceptionHandler != null)
			{
				m_nativeExceptionHandler.SessionHash = value;
			}
		}
	}

	[ImportingConstructor]
	public NativeCrashHandlerService(IAssetCloudSettingService acss, ICrashSubmissionService crashSubSvc, ILogFileProvider logFileProvider, IToolHostLoaderService loaderSvc)
	{
		m_assetCloudSettingService = acss;
		m_toolHostLoaderService = loaderSvc;
		m_crashSubmissionService = crashSubSvc;
		m_logFileProvider = logFileProvider;
		SetupAutomationFlag();
		RegisterHandlers();
		Startup();
	}

	private void SetupAutomationFlag()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		foreach (string text in commandLineArgs)
		{
			if (text.StartsWith("-automation", StringComparison.CurrentCultureIgnoreCase) || text.StartsWith("--automation", StringComparison.CurrentCultureIgnoreCase) || text.StartsWith("--teamcity", StringComparison.CurrentCultureIgnoreCase))
			{
				m_automationEnabled = true;
			}
		}
	}

	private AssertionConfiguration GetAssertionConfig()
	{
		if (m_automationEnabled)
		{
			return AssertionConfiguration.eOutput;
		}
		if (m_assetCloudSettingService.AssetCloudSettings.ShowAssertions)
		{
			return AssertionConfiguration.eDialog;
		}
		return AssertionConfiguration.eOff;
	}

	public void EnableCollection(bool bEnabled)
	{
		m_nativeExceptionHandler?.EnableCollection(bEnabled);
	}

	private void RegisterHandlers()
	{
		UnregisterHandlers();
		m_toolHostLoaderService.Loaded += ToolHost_Loaded;
		m_toolHostLoaderService.Unloaded += ToolHost_Unloaded;
	}

	private void UnregisterHandlers()
	{
		m_toolHostLoaderService.Loaded -= ToolHost_Loaded;
		m_toolHostLoaderService.Unloaded -= ToolHost_Unloaded;
	}

	private void ToolHost_Loaded(object sender, ToolHostEventArgs e)
	{
		Startup();
	}

	private void ToolHost_Unloaded(object sender, ToolHostEventArgs e)
	{
		Shutdown();
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				UnregisterHandlers();
				m_nativeExceptionHandler?.Dispose();
				m_nativeExceptionHandler = null;
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	private void Startup()
	{
		if (m_nativeExceptionHandler == null && m_toolHostLoaderService.ToolHostInterface != null && m_toolHostLoaderService.ToolHostInterface.IsLoaded)
		{
			m_nativeExceptionHandler = Context.EnsureCreated<CivTechContext>().CreateInstance<INativeExceptionHandler>(new object[3]
			{
				m_toolHostLoaderService.ToolHostInterface,
				m_logFileProvider.LogFilePath,
				GetAssertionConfig()
			});
			m_crashSubmissionService.SessionHash = m_nativeExceptionHandler.SessionHash;
		}
	}

	private void Shutdown()
	{
		m_nativeExceptionHandler?.Dispose();
		m_nativeExceptionHandler = null;
	}
}
