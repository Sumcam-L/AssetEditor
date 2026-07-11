using System;
using System.ComponentModel.Composition;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IPreviewerEntityLoadingService))]
[Export(typeof(PreviewerEntityLoadingService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PreviewerEntityLoadingService : IPreviewerEntityLoadingService, ISequencedProjectChangeWatcher, IDisposable
{
	private ICivTechService CivTechService { get; set; }

	public IInstanceSet InstanceSet { get; private set; }

	[ImportingConstructor]
	public PreviewerEntityLoadingService(ICivTechService civTechSvc)
	{
		CivTechService = civTechSvc;
		InitializeInstanceSet();
	}

	public void Dispose()
	{
		ShutdownInstanceSet();
	}

	public void StartProjectChange(Action<string> statusMessagePrinter)
	{
		ShutdownInstanceSet();
	}

	public void FinishProjectChange(Action<string> statusMessagePrinter)
	{
		InitializeInstanceSet();
	}

	public IInstanceEntity LoadEntity(string name, InstanceType type)
	{
		return InstanceSet.LoadEntityByName(name, type);
	}

	public void UnloadEntity(IInstanceEntity entity)
	{
		InstanceSet.Remove(entity);
	}

	private void InitializeInstanceSet()
	{
		InstanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { CivTechService.GetActivePantryPaths() });
	}

	private void ShutdownInstanceSet()
	{
		InstanceSet.Clear();
		InstanceSet = null;
	}
}
