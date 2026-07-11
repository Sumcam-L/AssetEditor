using System;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class AssetPreviewerViewModel : Notifier, IDisposable
{
	public IAssetPreviewer AssetPreviewer { get; private set; }

	public ICivTechService CivTechService { get; private set; }

	public event EventHandler<SelectedLightRigChangedEventArgs> SelectedLightRigChanged;

	public event EventHandler<SelectedPreviewModuleChangedEventArgs> SelectedPreviewModuleChanged;

	public event EventHandler<SelectedAssetChangedEventArgs> SelectedAssetChanged;

	public event SourcedLogEventHandler SourcedLogEvent;

	public AssetPreviewerViewModel(ICivTechService civTechSvc, IXLPRegistry xlpRegistry)
	{
		CivTechService = civTechSvc;
		try
		{
			AssetPreviewer = CivTechService.CivTechContext.CreateInstance<IAssetPreviewer>();
			AssetPreviewer.Logger += HandleLogEvent;
			AssetPreviewer.Startup(CivTechService, xlpRegistry);
			Console.WriteLine("Asset Previewer ToString: " + AssetPreviewer.ToString());
		}
		catch (Exception ex)
		{
			AssetPreviewer = null;
			throw ex;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (AssetPreviewer != null)
		{
			AssetPreviewer.Logger -= HandleLogEvent;
			AssetPreviewer.Dispose();
			AssetPreviewer = null;
		}
	}

	private LogEventType ConvertEventType(LogLevel logLevel)
	{
		return logLevel switch
		{
			LogLevel.Error => LogEventType.Error, 
			LogLevel.Warning => LogEventType.Warning, 
			_ => LogEventType.Info, 
		};
	}

	protected virtual void HandleLogEvent(string context, LogLevel logLevel, string text)
	{
		OnSourcedLogEvent(ConvertEventType(logLevel), context, text);
	}

	protected virtual void OnSelectedAssetChanged(SelectedAssetChangedEventArgs e)
	{
		this.SelectedAssetChanged?.Invoke(this, e);
	}

	protected virtual void OnSelectedLightRigChanged(SelectedLightRigChangedEventArgs e)
	{
		this.SelectedLightRigChanged?.Invoke(this, e);
	}

	protected virtual void OnSelectedPreviewModuleChanged(SelectedPreviewModuleChangedEventArgs e)
	{
		this.SelectedPreviewModuleChanged?.Invoke(this, e);
	}

	private void OnSourcedLogEvent(LogEventType evtType, string source, string text)
	{
		this.SourcedLogEvent?.Invoke(evtType, source, text);
	}
}
