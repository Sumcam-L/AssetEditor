using System.Collections.Generic;

namespace Firaxis.MVVMBase.Helpers;

public class TaskQueue : TaskFactory
{
	private readonly Queue<ITaskGenerator> _generators;

	public override int Count => _generators.Count;

	public TaskQueue(int maxConcurrent = 10)
		: base(maxConcurrent)
	{
		_generators = new Queue<ITaskGenerator>(maxConcurrent);
	}

	public override ITaskGenerator GetNext()
	{
		return _generators.Dequeue();
	}

	protected override void AddInternal(ITaskGenerator taskGenerator)
	{
		_generators.Enqueue(taskGenerator);
	}
}
