using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DatabaseWrapper;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.ContentExporters;
using Firaxis.MVVMBase;

namespace Firaxis.AssetBrowser.ViewModels;

public class ImportedEntityViewModel : InstanceEntityViewModel
{
	private DelegateCommand _reimportCommand;

	private Cursor m_previousCursor;

	public virtual ICommand ReimportCommand
	{
		get
		{
			if (_reimportCommand == null)
			{
				_reimportCommand = new DelegateCommand(Reimport);
			}
			return _reimportCommand;
		}
	}

	public bool IsImporting { get; private set; }

	public ImportedEntityViewModel(ICivTechService civTechSvc, string name, InstanceType type, Func<string, InstanceType, IInstanceEntity> loadFunction)
		: base(civTechSvc, name, type, loadFunction)
	{
	}

	protected virtual void Reimport(object context)
	{
		IsImporting = true;
		BackgroundWorker backgroundWorker = new BackgroundWorker();
		m_previousCursor = Mouse.OverrideCursor;
		Mouse.OverrideCursor = Cursors.Wait;
		backgroundWorker.DoWork += StartReimport;
		backgroundWorker.RunWorkerCompleted += ImportCompleted;
		backgroundWorker.RunWorkerAsync();
	}

	protected virtual void ImportCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		Mouse.OverrideCursor = m_previousCursor;
		ImportOperationResult importOperationResult = e.Result as ImportOperationResult;
		if (!importOperationResult.Result)
		{
			MessageBox.Show((importOperationResult != null) ? importOperationResult.Result.Message : "No export occurred.");
		}
		IsImporting = false;
	}

	protected virtual void StartReimport(object sender, DoWorkEventArgs e)
	{
		e.Result = global::DatabaseWrapper.DatabaseWrapper.ImportEntity(base.CivTechService, base.CivTechService.PrimaryProject.Name, base.Entity as IImportedEntity);
	}
}
