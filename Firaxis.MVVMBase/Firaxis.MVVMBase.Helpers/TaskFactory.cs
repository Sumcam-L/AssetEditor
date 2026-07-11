using System.Collections.Generic;
using System.Threading.Tasks;

namespace Firaxis.MVVMBase.Helpers;

public abstract class TaskFactory
{
	protected const int MAX_CONCURRENT = 10;

	protected List<Task> _processing;

	public int MaxConcurrent { get; }

	public abstract int Count { get; }

	protected TaskFactory(int maxConcurrent = 10)
	{
		MaxConcurrent = maxConcurrent;
		_processing = new List<Task>(maxConcurrent);
	}

	private async Task Process()
	{
		await Task.Yield();
		while (Count > 0 || _processing.Count > 0)
		{
			if (_processing.Count >= MaxConcurrent || Count == 0)
			{
				await Task.WhenAll(_processing);
				_processing.Clear();
			}
			if (Count > 0)
			{
				ITaskGenerator taskGenerator = GetNext();
				_processing.Add(taskGenerator.Generate());
			}
		}
	}

	public abstract ITaskGenerator GetNext();

	public async Task Run(ITaskGenerator taskGenerator)
	{
		if (_processing.Count < MaxConcurrent)
		{
			_processing.Add(taskGenerator.Generate());
			return;
		}
		AddInternal(taskGenerator);
		await Process();
	}

	protected abstract void AddInternal(ITaskGenerator taskGenerator);
}
